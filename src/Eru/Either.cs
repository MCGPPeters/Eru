namespace Eru
{
    using System;

    public static partial class _
    {

        public static Either<TValue, TAlternative> Return<TValue, TAlternative>(this TValue value) =>
            new Either<TValue, TAlternative>.Success(value);

        public static Either<TValue, TAlternative> AsEither<TValue, TAlternative>(this TValue value) =>
            Return<TValue, TAlternative>(value);

        public static Either<TValue, TAlternative> ReturnAlternative<TValue, TAlternative>(this TAlternative alternative) =>
            new Either<TValue, TAlternative>.Alternative(alternative);

        public static Either<TResult, TAlternative> Bind<TAlternative, TValue, TResult>(this Either<TValue, TAlternative> either,
            Func<TValue, Either<TResult, TAlternative>> function) where TAlternative : IMonoid<TAlternative> =>
                either.Match(success =>
                    function(success), alternatives =>
                        new Either<TResult, TAlternative>.Alternative(alternatives));

        public static Either<TResult, TAlternative> Then<TAlternative, TValue, TResult>(this Either<TValue, TAlternative> either,
            Func<TValue, Either<TResult, TAlternative>> function) where TAlternative : IMonoid<TAlternative> =>
                Bind(either, function);

        public static Either<TResult, TAlternative> SelectMany<TAlternative, TValue, TResult>(this Either<TValue, TAlternative> either,
            Func<TValue, Either<TResult, TAlternative>> function) where TAlternative : IMonoid<TAlternative> =>
                Bind(either, function);

        public static Either<TResult, TAlternative> Map<TAlternative, TValue, TResult>(this Either<TValue, TAlternative> either,
            Func<TValue, TResult> function) where TAlternative : IMonoid<TAlternative> =>
                either.Bind(success =>
                    Return<TResult, TAlternative>(function(success)));

        public static Either<TValue, TResult> MapAlternatives<TAlternative, TValue, TResult>(this Either<TValue, TAlternative> either, Func<TAlternative, TResult> function) where TAlternative : IMonoid<TAlternative> =>
            either.Match(success =>
                Return<TValue, TResult>(success), alternative =>
                    ReturnAlternative<TValue, TResult>(function(alternative)));

        public static Either<TValue, Nothing> Where<TValue, TAlternative>(this Either<TValue, TAlternative> either, Predicate<TValue> predicate) where TAlternative : IMonoid<TAlternative> =>
            either
                .MapAlternatives(msgs => Nothing)
                .Bind(success =>
                    predicate(success)
                        ? Return<TValue, Nothing>(success)
                        : ReturnAlternative<TValue, Nothing>(Nothing));

        public static Either<TValue, Nothing> Filter<TValue, TAlternative>(this Either<TValue, TAlternative> either, Predicate<TValue> predicate) where TAlternative : IMonoid<TAlternative> =>
            Where(either, predicate);

        public static TResult Match<TValue, TAlternative, TResult>(this Either<TValue, TAlternative> either, Func<TValue, TResult> onSuccess,
            Func<TAlternative, TResult> onFailure) where TAlternative : IMonoid<TAlternative>
        {
            switch (either)
            {
                case Either<TValue, TAlternative>.Success s:
                    return onSuccess(s.Value);
                case Either<TValue, TAlternative>.Alternative f:
                    return onFailure(f.Value);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static TResult MatchAlternative<TValue, TAlternative, TResult>(this Either<TValue, TAlternative> either,
            Func<TAlternative, TResult> onAlternative) where TAlternative : IMonoid<TAlternative>
        {
            switch (either)
            {
                case Either<TValue, TAlternative>.Alternative f:
                    return onAlternative(f.Value);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Either<TValue, TAlternative> MergeAlternatives<TValue, TAlternative>(this Either<TValue, TAlternative> either, TAlternative alternative) where TAlternative : IMonoid<TAlternative> =>
                either.Match(success =>
                    Return<TValue, TAlternative>(success), alt =>
                        ReturnAlternative<TValue, TAlternative>(alternative.Concat(alt)));

        public static Either<TResult, TAlternative> Select<TAlternative, TValue, TResult>(this Either<TValue, TAlternative> either, Func<TValue, TResult> function) where TAlternative : IMonoid<TAlternative> =>
            Map(either, function);
    }


    public abstract class Either<TValue, TAlternative>
    {
        public sealed class Success : Either<TValue, TAlternative>
        {
            public Success(TValue value)
            {
                Value = value;
            }

            public TValue Value { get; }
        }

        public sealed class Alternative : Either<TValue, TAlternative>
        {
            public Alternative(TAlternative alternative)
            {
                Value = alternative;
            }

            public TAlternative Value { get; }
        }
    }
}