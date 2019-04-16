using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru
{
    public static partial class _
    {
        public static Either<T, TOtherwise> AsEither<T, TOtherwise>(this T value) =>
            new EitherValue<T, TOtherwise>(value);

        public static Either<T, TOtherwise> AsEither<T, TOtherwise>(this TOtherwise
            alternative) => new EitherValue<T, TOtherwise>(alternative);

        public static Either<TResult, TOtherwise> Bind<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> eitherValue,
            Func<T, Either<TResult, TOtherwise>> function) => eitherValue.Match(
            function,
            alternatives =>
                alternatives.AsEither<TResult, TOtherwise>())();

        public static Either<TResult, TOtherwise> Then<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> eitherValue,
            Func<T, Either<TResult, TOtherwise>> function) => Bind(eitherValue, function);

        public static Either<TResult, TOtherwise> SelectMany<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> eitherValue,
            Func<T, Either<TResult, TOtherwise>> function) => Bind(eitherValue, function);

        public static Either<TResult, TOtherwise> Map<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> eitherValue,
            Func<T, TResult> function) => eitherValue.Bind(
            alternative =>
                AsEither<TResult, TOtherwise>(function(alternative)));

        public static Either<T, TOtherwiseResult> OtherwiseMap<T, TOtherwise, TOtherwiseResult>(
            this Either<T, TOtherwise> eitherValue, Func<TOtherwise, TOtherwiseResult> function) => eitherValue.Match(
            AsEither<T, TOtherwiseResult>,
            alternative =>
                AsEither<T, TOtherwiseResult>(function(alternative)))();

        public static Either<T, TOtherwise> Where<T, TOtherwise>(this Either<T, TOtherwise> eitherValue,
            Predicate<T> predicate, TOtherwise alternative) => eitherValue
            .Bind(
                success =>
                    predicate(success)
                        ? AsEither<T, TOtherwise>(success)
                        : AsEither<T, TOtherwise>(alternative));


        public static Either<T, Nothing> Where<T>(this T value, Predicate<T> predicate) => AsEither<
                T, Nothing>(value)
            .Where(predicate, Nothing);

        public static Either<T, Nothing> Where<T>(this Either<T, Nothing> eitherValue,
            Predicate<T> predicate) => eitherValue
            .OtherwiseMap(otherwise => Nothing)
            .Bind(
                value =>
                    predicate(value)
                        ? AsEither<T, Nothing>(value)
                        : AsEither<T, Nothing>(Nothing));


        public static Either<T, TOtherwise> Where<T, TOtherwise>(this T value, Predicate<T> predicate,
            TOtherwise alternative) => AsEither<T, TOtherwise>(value).Where(predicate, alternative);

        /// <summary>
        ///     Where a alternative is represented, it will return that alternative as a singleton list.
        ///     Otherwise it will return the empty list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOtherwise"></typeparam>
        /// <param name="eitherValue"></param>
        /// <returns></returns>
        public static IEnumerable<TOtherwise> Otherwise<T, TOtherwise>(
            this Either<T, TOtherwise> eitherValue) =>
            eitherValue.Match(
                t => Enumerable.Empty<TOtherwise>(),
                otherwise => new[] {otherwise})();


        public static IEnumerable<TResult> Otherwise<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> eitherValue,
            Func<TOtherwise, TResult> fallback) =>
            eitherValue.Match(
                t => Enumerable.Empty<TResult>(),
                otherwise => new[] {fallback(otherwise)})();


        public static IEnumerable<TResult> Otherwise<T, TOtherwise, TResult>(
            this T @this,
            Func<TOtherwise, TResult> fallback) =>
            @this.AsEither<T, TOtherwise>().Match(
                t => Enumerable.Empty<TResult>(),
                otherwise => new[] {fallback(otherwise)})();

        public static Either<TResult, TOtherwise> Apply<T, TOtherwise, TResult>(
            this Either<Func<T, TResult>, TOtherwise> function, Either<T, TOtherwise> eitherValue)
            where TOtherwise : Monoid<TOtherwise> => function
            .Match(
                func =>
                    eitherValue
                        .Match(
                            value =>
                                AsEither<TResult, TOtherwise>(func(value)),
                            AsEither<TResult, TOtherwise>)(),
                otherwise =>
                    eitherValue
                        .Match(
                            _ =>
                                AsEither<TResult, TOtherwise>(otherwise),
                            alternative =>
                                AsEither<TResult, TOtherwise>(otherwise.Append(alternative)))())();

        /// <summary>
        ///     Alias of map for linq syntax support
        /// </summary>
        /// <typeparam name="TOtherwise"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="eitherValue"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Either<TResult, TOtherwise> Select<T, TOtherwise, TResult>(
            this Either<T, TOtherwise> eitherValue, Func<T, TResult> function) => Map(eitherValue, function);

        public static Either<TResult, TOtherwise> SelectMany<T, TIntermediateResult, TResult, TOtherwise>(
            this Either<T, TOtherwise> eitherValue, Func<T, Either<TIntermediateResult, TOtherwise>> bind,
            Func<T, TIntermediateResult, TResult> project) => eitherValue.Match(
            value =>
                bind(value)
                    .Match(
                        intermediateResult =>
                            AsEither<TResult, TOtherwise>(project(value, intermediateResult)),
                        AsEither<TResult, TOtherwise>)(),
            AsEither<TResult, TOtherwise>)();
    }


    public interface Either<out T, out TOtherwise>
    {
        Func<TResult> Match<TResult>(Func<T, TResult> @do, Func<TOtherwise, TResult> otherwise);

        void Match(Action<T> @do, Action<TOtherwise> otherwise);
    }

    internal struct EitherValue<T, TOtherwise> : Either<T, TOtherwise>
    {
        private readonly bool _isJust;
        private readonly bool _isOtherwise;

        private TOtherwise Otherwise { get; }
        private T Just { get; }

        internal EitherValue(T just)
        {
            Just = just;
            _isJust = true;
            _isOtherwise = false;
            Otherwise = default(TOtherwise);
        }

        internal EitherValue(TOtherwise otherwise)
        {
            Otherwise = otherwise;
            _isJust = false;
            _isOtherwise = true;
            Just = default(T);
        }


        public static implicit operator EitherValue<T, TOtherwise>(T left) => new EitherValue<T, TOtherwise>(left);

        public static implicit operator EitherValue<T, TOtherwise>(TOtherwise right) => new EitherValue<T, TOtherwise>(right);

        public Func<TResult> Match<TResult>(Func<T, TResult> @do, Func<TOtherwise, TResult> otherwise)
        {
            if (_isJust)
            {
                var thisJust = Just;
                return () => @do(thisJust);

            }

            if (!_isOtherwise) throw new InvalidOperationException();

            var thisOtherwise = Otherwise;
            return () => otherwise(thisOtherwise);
        }

        public void Match(Action<T> @do, Action<TOtherwise> otherwise) =>
            Match(@do.ToFunction(), otherwise.ToFunction())();
    }
}
