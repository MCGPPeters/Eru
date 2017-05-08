using System;
using System.Collections.Generic;

namespace Eru
{
    public static partial class _
    {
        public static Either<TValue, TAlternative> Return<TValue, TAlternative>(this TValue value)
        {
            return new Either<TValue, TAlternative>.Success(value);
        }

        public static Either<TValue, TAlternative> AsEither<TValue, TAlternative>(this TValue value)
        {
            return Return<TValue, TAlternative>(value);
        }

        public static Either<TValue, TAlternative> ReturnAlternative<TValue, TAlternative>(
            this TAlternative alternative)
        {
            return new Either<TValue, TAlternative>.Alternative(alternative);
        }

        public static Either<TResult, TAlternative> Bind<TAlternative, TValue, TResult>(
            this Either<TValue, TAlternative> either,
            Func<TValue, Either<TResult, TAlternative>> function)
        {
            return either.Match(function, alternatives =>
                new Either<TResult, TAlternative>.Alternative(alternatives));
        }

        public static Either<TResult, TAlternative> Then<TAlternative, TValue, TResult>(
            this Either<TValue, TAlternative> either,
            Func<TValue, Either<TResult, TAlternative>> function)
        {
            return Bind(either, function);
        }

        public static Either<TResult, TAlternative> SelectMany<TAlternative, TValue, TResult>(
            this Either<TValue, TAlternative> either,
            Func<TValue, Either<TResult, TAlternative>> function)
        {
            return Bind(either, function);
        }

        public static Either<TResult, TAlternative> Map<TAlternative, TValue, TResult>(
            this Either<TValue, TAlternative> either,
            Func<TValue, TResult> function)
        {
            return either.Bind(success =>
                Return<TResult, TAlternative>(function(success)));
        }

        public static Either<TValue, TAlternativeResult> MapAlternative<TAlternative, TValue, TAlternativeResult>(
            this Either<TValue, TAlternative> either, Func<TAlternative, TAlternativeResult> function)
        {
            return either.Match(Return<TValue, TAlternativeResult>, alternative =>
                ReturnAlternative<TValue, TAlternativeResult>(function(alternative)));
        }

        public static Either<TValue, TAlternative> Where<TValue, TAlternative>(this Either<TValue, TAlternative> either,
            Predicate<TValue> predicate, TAlternative alternative)
        {
            return either
                .Bind(success =>
                    predicate(success)
                        ? Return<TValue, TAlternative>(success)
                        : ReturnAlternative<TValue, TAlternative>(alternative));
        }


        public static Either<TValue, Nothing> Where<TValue>(this TValue value, Predicate<TValue> predicate)
        {
            return Return<TValue, Nothing>(value)
                .Where(predicate, Nothing);
        }

        public static Either<TValue, Nothing> Where<TValue>(this Either<TValue, Nothing> either,
            Predicate<TValue> predicate)
        {
            return either
                .MapAlternative(alternative => Nothing)
                .Bind(success =>
                    predicate(success)
                        ? Return<TValue, Nothing>(success)
                        : ReturnAlternative<TValue, Nothing>(Nothing));
        }


        public static Either<TValue, TNoMatch> Where<TValue, TNoMatch>(this TValue value, Predicate<TValue> predicate,
            TNoMatch alternative)
        {
            return Return<TValue, TNoMatch>(value).Where(predicate, alternative);
        }

        public static Unit Match<TValue, TAlternative>(this Either<TValue, TAlternative> either,
            Action<TValue> onSuccess,
            Action<TAlternative> onFailure) =>
                Match(either, onSuccess.ToFunction(), onFailure.ToFunction());

        public static TResult Match<TValue, TAlternative, TResult>(this Either<TValue, TAlternative> either,
            Func<TValue, TResult> onSuccess,
            Func<TAlternative, TResult> onFailure)
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

        /// <summary>
        ///     If a value is represented, that value will be returned as a singleton list
        ///     Otherwise it will return the empty list
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TAlternative"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        public static IEnumerable<TValue> Value<TValue, TAlternative>(this Either<TValue, TAlternative> either)
        {
            if (either is Either<TValue, TAlternative>.Success s) yield return s.Value;
        }

        /// <summary>
        ///     If a alternative is represented, it will return that alternative as a singleton list.
        ///     Otherwise it will return the empty list
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TAlternative"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        public static IEnumerable<TAlternative> Alternative<TValue, TAlternative>(
            this Either<TValue, TAlternative> either)
        {
            if (either is Either<TValue, TAlternative>.Alternative a) yield return a.Value;
        }


        public static IEnumerable<TResult> MatchAlternative<TValue, TAlternative, TResult>(
            this Either<TValue, TAlternative> either,
            Func<TAlternative, TResult> onAlternative)
        {
            if (either is Either<TValue, TAlternative>.Alternative f) yield return onAlternative(f.Value);
        }

        public static Either<TResult, TAlternative> Apply<TValue, TAlternative, TResult>(
            this Either<TValue, TAlternative> either, Either<Func<TValue, TResult>, TAlternative> function)
            where TAlternative : Monoid<TAlternative>
        {
            return function
                .Match(func =>
                    either
                        .Match(value =>
                                Return<TResult, TAlternative>(func(value)),
                            ReturnAlternative<TResult, TAlternative>), alternative =>
                    either
                        .Match(_ =>
                            ReturnAlternative<TResult, TAlternative>(alternative), alternative1 =>
                            ReturnAlternative<TResult, TAlternative>(alternative.Append(alternative1))));
        }

        /// <summary>
        ///     Alias of map for linq syntax support
        /// </summary>
        /// <typeparam name="TAlternative"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Either<TResult, TAlternative> Select<TAlternative, TValue, TResult>(
            this Either<TValue, TAlternative> either, Func<TValue, TResult> function)
        {
            return Map(either, function);
        }

        public static Either<TResult, TAlternative> SelectMany<TValue, TIntermediateResult, TResult, TAlternative>(
            this Either<TValue, TAlternative> either, Func<TValue, Either<TIntermediateResult, TAlternative>> bind,
            Func<TValue, TIntermediateResult, TResult> project)
        {
            return either.Match(value =>
                bind(value)
                    .Match(intermediateResult =>
                            Return<TResult, TAlternative>(project(value, intermediateResult)),
                        ReturnAlternative<TResult, TAlternative>), ReturnAlternative<TResult, TAlternative>);
        }
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