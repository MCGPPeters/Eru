namespace Eru.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Validation
    {
        private static Either<TSuccess, TCauseIdentifier> Assert<TSuccess, TCauseIdentifier>(TSuccess source,
            params Property<TCauseIdentifier, TSuccess>[] properties)
        {
            var propertiesThatDoNotHold = properties
                .Where(rule => !rule.Holds(source))
                .Select(rule => rule.Identifier)
                .ToArray();

            return propertiesThatDoNotHold.Any() 
                ? propertiesThatDoNotHold.Fail<TSuccess, TCauseIdentifier>() 
                : new Either<TSuccess, TCauseIdentifier>.Success(source);
        }

        public static Either<TRight, TCauseIdentifier> Check<TRight, TCauseIdentifier>(
            this Either<TRight, TCauseIdentifier> source,
            params Property<TCauseIdentifier, TRight>[] properties)
        {
            return source.Bind(right => Assert(right, properties));
        }

        public static Either<TRight, TCauseIdentifier> Check<TRight, TCauseIdentifier>(this TRight source,
            params Property<TCauseIdentifier, TRight>[] properties)
        {
            return Check(source.Return<TRight, TCauseIdentifier>(), properties);
        }

        public static Either<TRight, string> Check<TRight>(this TRight source,
            string cause,
            params Predicate<TRight>[] rules)
        {
            return Check(source.Return<TRight, string>(),
                rules.Select(predicate => new Property<string, TRight>(cause, predicate)).ToArray());
        }

        public static Either<TRight, string> Check<TRight>(this TRight source,
            params Predicate<TRight>[] rules)
        {
            return Check(source.Return<TRight, string>(),
                rules.Select(predicate => new Property<string, TRight>(Guid.NewGuid().ToString(), predicate)).ToArray());
        }

        public static Either<TRight, string> Check<TRight>(this Either<TRight, string> source,
            string cause,
            params Predicate<TRight>[] rules)
        {
            return Check(source, rules.Select(predicate => new Property<string, TRight>(cause, predicate)).ToArray());
        }
    }
}