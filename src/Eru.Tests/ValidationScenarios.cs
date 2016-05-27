using System.Linq;
using Eru.ErrorHandling;
using Xunit;

namespace Eru.Tests
{
    public class ValidationScenarios
    {
        public ValidationScenarios()
        {
            Assertions = new[]
            {
                new Assertion<Constraint, Person>(Constraint.AgeMustBePositive, p => p.Age >= 0),
                new Assertion<Constraint, Person>(Constraint.NameIsRequired, p => !string.IsNullOrEmpty(p.Name))
            };
        }

        private Assertion<Constraint, Person>[] Assertions { get; }

        [Fact]
        public void Validating_a_subject_that_is_invalid_returns_the_broken_rules()
        {
            var person = new Person {Age = -1, Name = ""};

            person
                .Assert(Assertions)
                .Match(
                    failure =>
                    {
                        var expectedFailure =
                            new Failure<Constraint>(Assertions.Select(assertion => assertion.Identifier));
                        Assert.True(
                            failure.Equals(expectedFailure));
                    });
        }

        [Fact]
        public void Predicate_based_validation_with_cause_expressed_as_a_string()
        {
            "".Assert("String must have a value",
                x => !string.IsNullOrWhiteSpace(x),
                x => x.Equals("hasValue"))
                .Match(
                    failure =>
                    {
                        var expectedFailure =
                            new Failure<string>("String must have a value");
                        Assert.True(
                            failure.Equals(expectedFailure));
                    });
        }

        [Fact]
        public void Can_chain_predicate_based_validation()
        {
            var person = new Person {Age = -1, Name = ""};

            person
                .Assert("Has a valid age", p => p.Age >= 0)
                .Assert("Has a valid name", p => !string.IsNullOrWhiteSpace(p.Name))
                .Match(
                    failure =>
                    {
                        var expectedFailure =
                            new Failure<string>("Has a valid age");
                        Assert.Equal(expectedFailure, failure);
                    });
        }

        private enum Constraint
        {
            AgeMustBePositive,
            NameIsRequired
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}