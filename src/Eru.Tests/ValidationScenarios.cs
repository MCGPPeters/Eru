using System;
using System.Collections.Generic;
using System.Linq;
using Eru.ExceptionHandling;
using Xunit;

namespace Eru.Tests
{
    public class ValidationScenarios
    {
        public enum Constraint
        {
            AgeMustBePositive,
            NameIsRequired
        }

        public ValidationScenarios()
        {
            Assertions = new[]
            {
                new Assertion<Constraint, Person>(Constraint.AgeMustBePositive, p => p.Age >= 0),
                new Assertion<Constraint, Person>(Constraint.NameIsRequired, p => !string.IsNullOrEmpty(p.Name))
            };
        }

        public Assertion<Constraint, Person>[] Assertions { get; private set; }

        [Fact]
        public void Validating_a_subject_that_is_invalid_returns_the_broken_rules()
        {
            var person = new Person {Age = -1, Name = ""};

            person
                .Assert(Assertions)
                .Match(
                    failure =>
                    {
                        var expectedFailure = new Failure<Constraint>(Assertions.Select(assertion => assertion.Identifier));
                        Assert.True(
                            failure.Equals(expectedFailure));
                        return Unit.Instance;
                    },
                    _ =>
                    {
                        Fail();
                        return Unit.Instance;
                    });
        }

        private static void Fail()
        {
            Assert.False(true);
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class Assertion<TIdentifier, TSubject>
    {
        private readonly Predicate<TSubject> _rule;

        public Assertion(TIdentifier identifier, Predicate<TSubject> rule)
        {
            _rule = rule;
            Identifier = identifier;
        }

        public TIdentifier Identifier { get; set; }

        public bool IsBroken(TSubject subject)
        {
            return !_rule(subject);
        }
    }

    public static class Validation
    {
        private static Either<Failure<TCauseIdentifier>, TRight> Validate<TRight, TCauseIdentifier>(TRight source,
            IEnumerable<Assertion<TCauseIdentifier, TRight>> assertions)
        {
            var failedAssertions = assertions
                .Where(rule => rule.IsBroken(source))
                .Select(rule => rule.Identifier).ToArray();

            if (failedAssertions.Any())
                return
                    new Either<Failure<TCauseIdentifier>, TRight>(
                        new Failure<TCauseIdentifier>(failedAssertions));

            return new Either<Failure<TCauseIdentifier>, TRight>(source);
        }

        public static Either<Failure<TCauseIdentifier>, TRight> Assert<TRight, TCauseIdentifier>(
            this Either<Failure<TCauseIdentifier>, TRight> source,
            IEnumerable<Assertion<TCauseIdentifier, TRight>> rules)
        {
            return source.Bind(right => Validate(right, rules));
        }

        public static Either<Failure<TCauseIdentifier>, TRight> Assert<TRight, TCauseIdentifier>(this TRight source,
            IEnumerable<Assertion<TCauseIdentifier, TRight>> rules)
        {
            return source.AsEither<Failure<TCauseIdentifier>, TRight>().Assert(rules);
        }

        //    this TSource source, ReadOnlyCollection<Tuple<TRuleIdentifier, Predicate<TSource>>> assertions)
        //    <TSource, TCauseIdentifier, TResult, TRuleIdentifier>(


        //public static Either<Failure<TCauseIdentifier>, TResult> When
        //{
        //    return
        //        When<TSource, TCauseIdentifier, TResult, TRuleIdentifier>(
        //            new Either<Failure<TCauseIdentifier>, TSource>(source), assertions);
        //}

        //public static Either<Failure<TCauseIdentifier>, TResult> When
        //    <TSource, TCauseIdentifier, TResult, TRuleIdentifier>(
        //    this Either<Failure<TCauseIdentifier>, TSource> either,
        //    ReadOnlyCollection<Tuple<TRuleIdentifier, Predicate<TSource>>> assertions)
        //{
        //    return
        //        either.Bind(item => WhereSubject<TSource, TCauseIdentifier, TResult, TRuleIdentifier>(item, assertions));
        //}

        //private static Either<Failure<TCauseIdentifier>, TResult> WhereSubject
        //    <TSource, TCauseIdentifier, TResult, TRuleIdentifier>(
        //    TSource source, ReadOnlyCollection<Tuple<TRuleIdentifier, Predicate<TSource>>> assertions)
        //{
        //    try
        //    {
        //        return new Either<Failure<TCauseIdentifier>, TResult>(function(source));
        //    }
        //    catch (Exception ex)
        //    {
        //        var caughtExceptionType = ex.GetType();
        //        var failedRequirement = default(TCauseIdentifier);

        //        //check if the exception was meant to be handled
        //        foreach (var exception in exceptions)
        //        {
        //            var exceptionType = exception.Value.GetType();
        //            if (exceptionType.IsAssignableFrom(caughtExceptionType))
        //                failedRequirement = exception.Key;
        //        }

        //        return
        //            new Either<Failure<TCauseIdentifier>, TResult>(
        //                new FailureBecauseOfException<TCauseIdentifier>(Enumerable.Repeat(failedRequirement, 1), ex));
        //    }
        //}
    }
}