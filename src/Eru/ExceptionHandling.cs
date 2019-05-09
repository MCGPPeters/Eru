using System;
using System.Linq;
using System.Threading.Tasks;

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

        public static Either<TResult, Exception> Try<T, TResult>(this Either<T, Exception> eitherValue,
            Func<T, TResult> function) =>
            eitherValue.Bind(v => Try(v, function));

        public static Either<Unit, Exception> Try<TValue>(this TValue value, Action<TValue> action)
            => Try(value, action.ToFunction());

        public static Either<Unit, Exception> Try<TValue>(this Either<TValue, Exception> eitherValue,
            Action<TValue> action) =>
            eitherValue.Map(action.ToFunction());

        public static Either<T, TOtherwise> MapException<T, TOtherwise>(
            this Either<T, Exception> eitherValue, Func<Exception, TOtherwise> function) =>
            eitherValue.Match(
                AsEither<T, TOtherwise>,
                alternative =>
                    AsEither<T, TOtherwise>(function(alternative)));

        //EitherValue<TResult, Func<Func<bool>, EitherValue<TResult, Exception>>>
        public static Either<TResult, Exception> Retry<T, TResult>(this T @this,
            Func<T, TResult> function, params TimeSpan[] delaysBetweenRetries) =>
            delaysBetweenRetries.Length == 0
                ? @this.Try(function)
                : @this.Try(function)
                    .Otherwise(
                        _ =>
                        {
                            Task.Delay(delaysBetweenRetries.First()).Wait();
                            return Retry(@this, function, delaysBetweenRetries.Skip(1).ToArray());
                        }).FirstOrDefault();
    }
}
