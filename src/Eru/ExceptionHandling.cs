using System.Threading.Tasks;

namespace Eru
{
    using System;
    using System.Linq;

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

        public static Either<TResult, Exception> Try<T, TResult>(this Either<T, Exception> either,
            Func<T, TResult> function) =>
            either.Bind(v => Try(v, function));

        public static Either<Unit, Exception> Try<TValue>(this TValue value, Action<TValue> action)
            => Try(value, action.ToFunction());

        public static Either<Unit, Exception> Try<TValue>(this Either<TValue, Exception> either,
            Action<TValue> action) =>
            either.Map(action.ToFunction());

        public static Either<T, TOtherwise> MapException<T, TOtherwise>(
            this Either<T, Exception> either, Func<Exception, TOtherwise> function) =>
            either.Match(AsEither<T, TOtherwise>, alternative =>
                AsEither<T, TOtherwise>(function(alternative)))();

        //Either<TResult, Func<Func<bool>, Either<TResult, Exception>>>
        public static Either<TResult, Exception> Retry<T, TResult>(this T @this,
            Func<T, TResult> function, params TimeSpan[] delaysBetweenRetries) =>
            delaysBetweenRetries.Length == 0
                ? @this.Try(function)
                : @this.Try(function)
                    .Otherwise(_ =>
                    {
                        Task.Delay(delaysBetweenRetries.First()).Wait();
                        return Retry(@this, function, delaysBetweenRetries.Skip(1).ToArray());
                    }).First();
    }
}