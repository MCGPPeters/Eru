using System;
using System.Collections.Generic;

namespace Eru
{
    public static partial class _
    {
        public static Either<T, TOtherwise> AsEither<T, TOtherwise>(this T value) =>
            new Either<T, TOtherwise>.Just(value);

        public static Either<T, TOtherwise> AsEither<T, TOtherwise>(
            this TOtherwise alternative) => new Either<T, TOtherwise>.Otherwise(alternative);

        public static Either<TResult, TOtherwise> Bind<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either,
            Func<T, Either<TResult, TOtherwise>> function) => either.Match(function, alternatives =>
            new Either<TResult, TOtherwise>.Otherwise(alternatives));

        public static Either<TResult, TOtherwise> Then<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either,
            Func<T, Either<TResult, TOtherwise>> function) => Bind(either, function);

        public static Either<TResult, TOtherwise> SelectMany<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either,
            Func<T, Either<TResult, TOtherwise>> function) => Bind(either, function);

        public static Either<TResult, TOtherwise> Map<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either,
            Func<T, TResult> function) => either.Bind(alternative =>
            AsEither<TResult, TOtherwise>(function(alternative)));

        public static Either<T, TOtherwiseResult> MapOtherwise<T, TOtherwise, TOtherwiseResult>(
            this Either<T, TOtherwise> either, Func<TOtherwise, TOtherwiseResult> function) => either.Match(
            AsEither<T, TOtherwiseResult>, alternative =>
                AsEither<T, TOtherwiseResult>(function(alternative)));

        public static Either<T, TOtherwise> Where<T, TOtherwise>(this Either<T, TOtherwise> either,
            Predicate<T> predicate, TOtherwise alternative) => either
            .Bind(success =>
                predicate(success)
                    ? AsEither<T, TOtherwise>(success)
                    : AsEither<T, TOtherwise>(alternative));


        public static Either<T, Nothing> Where<T>(this T value, Predicate<T> predicate) => AsEither<
                T, Nothing>(value)
            .Where(predicate, Nothing);

        public static Either<T, Nothing> Where<T>(this Either<T, Nothing> either,
            Predicate<T> predicate) => either
            .MapOtherwise(otherwise => Nothing)
            .Bind(value =>
                predicate(value)
                    ? AsEither<T, Nothing>(value)
                    : AsEither<T, Nothing>(Nothing));


        public static Either<T, TOtherwise> Where<T, TOtherwise>(this T value, Predicate<T> predicate,
            TOtherwise alternative) => AsEither<T, TOtherwise>(value).Where(predicate, alternative);

        public static Unit Match<T, TOtherwise>(this Either<T, TOtherwise> either,
            Action<T> @do,
            Action<TOtherwise> otherwise) => Match(either, @do.ToFunction(), otherwise.ToFunction());

        public static TResult Match<T, TOtherwise, TResult>(this Either<T, TOtherwise> either,
            Func<T, TResult> @do,
            Func<TOtherwise, TResult> otherwise)
        {
            switch (either)
            {
                case Either<T, TOtherwise>.Just a:
                    return @do(a.Value);
                case Either<T, TOtherwise>.Otherwise other:
                    return otherwise(other.Value);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     Where a do is represented, that do will be returned as a singleton list
        ///     Otherwise it will return the empty list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOtherwise"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        public static IEnumerable<T> Value<T, TOtherwise>(this Either<T, TOtherwise> either)
        {
            if (either is Either<T, TOtherwise>.Just a) yield return a.Value;
        }

        /// <summary>
        ///     Where a alternative is represented, it will return that alternative as a singleton list.
        ///     Otherwise it will return the empty list
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TOtherwise"></typeparam>
        /// <param name="either"></param>
        /// <returns></returns>
        public static IEnumerable<TOtherwise> Otherwise<TValue, TOtherwise>(
            this Either<TValue, TOtherwise> either)
        {
            if (either is Either<TValue, TOtherwise>.Otherwise a) yield return a.Value;
        }


        public static IEnumerable<TResult> Otherwise<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either,
            Func<TOtherwise, TResult> fallback)
        {
            if (either is Either<T, TOtherwise>.Otherwise f) yield return fallback(f.Value);
        }

        public static Either<TResult, TOtherwise> Apply<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either, Either<Func<T, TResult>, TOtherwise> function)
            where TOtherwise : Monoid<TOtherwise> => function
            .Match(func =>
                either
                    .Match(value =>
                            AsEither<TResult, TOtherwise>(func(value)),
                        AsEither<TResult, TOtherwise>), otherwise =>
                either
                    .Match(_ =>
                        AsEither<TResult, TOtherwise>(otherwise), alternative =>
                            AsEither<TResult, TOtherwise>(otherwise.Append(alternative))));

        /// <summary>
        ///     Alias of map for linq syntax support
        /// </summary>
        /// <typeparam name="TOtherwise"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Either<TResult, TOtherwise> Select<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> either, Func<T, TResult> function) => Map(either, function);

        public static Either<TResult, TOtherwise> SelectMany<T, TIntermediateResult, TResult, TOtherwise>(
            this Either<T, TOtherwise> either, Func<T, Either<TIntermediateResult, TOtherwise>> bind,
            Func<T, TIntermediateResult, TResult> project) => either.Match(value =>
            bind(value)
                .Match(intermediateResult =>
                        AsEither<TResult, TOtherwise>(project(value, intermediateResult)),
                    AsEither<TResult, TOtherwise>), AsEither<TResult, TOtherwise>);
    }


    public abstract class Either<T, TOtherwise>
    {
        public sealed class Just : Either<T, TOtherwise>
        {
            public Just(T value) => Value = value;

            public T Value { get; }
        }

        public sealed class Otherwise : Either<T, TOtherwise>
        {
            public Otherwise(TOtherwise alternative) => Value = alternative;

            public TOtherwise Value { get; }
        }
    }
}