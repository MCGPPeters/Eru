namespace Eru.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Validation
    {
        private static Either<TCauseIdentifier[], TRight> Assert<TRight, TCauseIdentifier>(TRight source,
            params Property<TCauseIdentifier, TRight>[] properties)
        {
            var propertiesThatDoNotHold = properties
                .Where(rule => !rule.Holds(source))
                .Select(rule => rule.Identifier)
                .ToArray();

            return propertiesThatDoNotHold.Any()
                ? Either<TCauseIdentifier[], TRight>.Create(propertiesThatDoNotHold)
                : Either<TCauseIdentifier[], TRight>.Create(source);
        }

        public static Either<TCauseIdentifier[], TRight> Check<TRight, TCauseIdentifier>(
            this Either<TCauseIdentifier[], TRight> source,
            params Property<TCauseIdentifier, TRight>[] properties)
        {
            return source.Bind(right => Assert(right, properties));
        }

        public static Either<TCauseIdentifier[], TRight> Check<TRight, TCauseIdentifier>(this TRight source,
            params Property<TCauseIdentifier, TRight>[] properties)
        {
            return Check(source.Return<TCauseIdentifier[], TRight>(), properties);
        }

        public static Either<string[], TRight> Check<TRight>(this TRight source,
            string cause,
            params Predicate<TRight>[] rules)
        {
            return Check(source.Return<string[], TRight>(),
                rules.Select(predicate => new Property<string, TRight>(cause, predicate)).ToArray());
        }

        public static Either<string[], TRight> Check<TRight>(this TRight source,
            params Predicate<TRight>[] rules)
        {
            return Check(source.Return<string[], TRight>(),
                rules.Select(predicate => new Property<string, TRight>(Guid.NewGuid().ToString(), predicate)).ToArray());
        }

        public static Either<string[], TRight> Check<TRight>(this Either<string[], TRight> source,
            string cause,
            params Predicate<TRight>[] rules)
        {
            return Check(source, rules.Select(predicate => new Property<string, TRight>(cause, predicate)).ToArray());
        }
    }
}