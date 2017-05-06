using System;
using System.Collections.Generic;
using System.Text;

namespace Eru
{
    public static partial class _
    {
        public static Either<TResult, Exception> TryCatch<TValue, TResult>(this TValue value, Func<TValue, TResult> function)
        {
            try
            {
                return Return<TResult, Exception>(function(value));
            }
            catch (Exception exception)
            {
                return ReturnAlternative<TResult, Exception>(exception);
            }
        }

        public static Either<TResult, Exception> Try<TValue, TResult>(this Either<TValue, Exception> either, Func<TValue, TResult> function) =>
            either.Bind(v => TryCatch(v, function));

        public static Either<Unit, Exception> Try<TValue>(this TValue value, Action<TValue> action)
            => TryCatch(value, action.ToFunction());

        public static Either<Unit, Exception> Try<TValue>(this Either<TValue, Exception> either, Action<TValue> action) =>
             either.Map(action.ToFunction());

        public static Either<TValue, TAlternative> MapException<TValue, TAlternative>(this Either<TValue, Exception> either, Func<Exception, TAlternative> function) =>
            either.Match(Return<TValue, TAlternative>, alternative =>
                ReturnAlternative<TValue, TAlternative>(function(alternative)));
    }
}
