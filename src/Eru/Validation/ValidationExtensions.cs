namespace Eru.Validation
{
    using System;
    using System.Linq;
    using static _;

    public static class Validation
    {
        private static Either<TSuccess, TCauseIdentifier> Assert<TSuccess, TCauseIdentifier>(TSuccess source,
            params Property<TCauseIdentifier, TSuccess>[] properties) where TCauseIdentifier : IMonoid<TCauseIdentifier>
        {
            var propertiesThatDoNotHold = properties
                .Where(rule => !rule.Holds(source))
                .Select(rule => rule.Identifier)
                .ToArray();

            return propertiesThatDoNotHold.Any()
                ? propertiesThatDoNotHold.ReturnAlternative<TSuccess, TCauseIdentifier>()
                :
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

        public static Either<TRight, TCause> Check<TRight, TCause>(this TRight source,
            TCause cause,
            params Predicate<TRight>[] rules)
        {
            return Check(source.Return<TRight, TCause>(),
                rules.Select(predicate => new Property<TCause, TRight>(cause, predicate)).ToArray());
        }

        public static Either<TRight, string> Check<TRight>(this TRight source,
            params Predicate<TRight>[] rules)
        {
            return Check(source.Return<TRight, string>(),
                rules.Select(predicate => new Property<string, TRight>(Guid.NewGuid().ToString(), predicate)).ToArray());
        }

        public static Either<TRight, TCause> Check<TRight, TCause>(this Either<TRight, TCause> source,
            TCause cause,
            params Predicate<TRight>[] rules)
        {
            return Check(source, rules.Select(predicate => new Property<TCause, TRight>(cause, predicate)).ToArray());
        }
    }


}