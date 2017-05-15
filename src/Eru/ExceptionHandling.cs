using System;

namespace Eru
{
    public static partial class _
    {
        public static Either<TResult, Exception> Try<TValue, TResult>(this TValue value, Func<TValue, TResult> function)
        {
            try
            {
                return AsEither<TResult, Exception>(function(value));
            }
            catch (Exception exception)
            {
                return AsEither<TResult, Exception>(exception);
            }
        }

        public static Either<TResult, Exception> Try<TValue, TResult>(this Either<TValue, Exception> either,
            Func<TValue, TResult> function) =>
            either.Bind(v => Try(v, function));

        public static Either<Unit, Exception> Try<TValue>(this TValue value, Action<TValue> action)
            => Try(value, action.ToFunction());

        public static Either<Unit, Exception> Try<TValue>(this Either<TValue, Exception> either,
            Action<TValue> action) =>
            either.Map(action.ToFunction());

        public static Either<TValue, TOtherwise> MapException<TValue, TOtherwise>(
            this Either<TValue, Exception> either, Func<Exception, TOtherwise> function) =>
            either.Match(AsEither<TValue, TOtherwise>, alternative =>
                AsEither<TValue, TOtherwise>(function(alternative)));
    }
}