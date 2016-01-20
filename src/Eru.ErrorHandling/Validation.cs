using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru.ErrorHandling
{
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
            return Assert(source.Return<Failure<TCauseIdentifier>, TRight>(), rules);
        }

        public static Either<Failure<string>, TRight> Assert<TRight>(this TRight source,
           string cause,
            params Predicate<TRight>[] rules)
        {
            return Assert(source.Return<Failure<string>, TRight>(), rules.Select(predicate => new Assertion<string, TRight>(cause, predicate)));
        }
    }
}