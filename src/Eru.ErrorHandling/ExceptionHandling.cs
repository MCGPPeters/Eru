using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru.ErrorHandling
{
    public static class ExceptionHandling
    {
        public static Either<Failure<TExceptionIdentifier>, TResult> Try<TSource, TExceptionIdentifier, TResult>(
            this TSource source, IEnumerable<Exception<TExceptionIdentifier>> handleTheseExceptions,
            Func<TSource, TResult> function)
        {
            return Try(new Either<Failure<TExceptionIdentifier>, TSource>(source), handleTheseExceptions, function);
        }

        public static Either<Failure<TExceptionIdentifier>, TResult> Try<TSource, TExceptionIdentifier, TResult>(
            this TSource source, Func<TSource, TResult> function, TExceptionIdentifier handleThisException)
        {
            return Try(new Either<Failure<TExceptionIdentifier>, TSource>(source), function, handleThisException);
        }

        public static Either<Failure<TExceptionIdentifier>, TResult> Try<TSource, TExceptionIdentifier, TResult>(
            this Either<Failure<TExceptionIdentifier>, TSource> either,
            IEnumerable<Exception<TExceptionIdentifier>> handleTheseExceptions, Func<TSource, TResult> function)
        {
            return either.Bind(item => TryCatch(item, handleTheseExceptions, function));
        }

        public static Either<Failure<TExceptionIdentifier>, TResult> Try<TSource, TExceptionIdentifier, TResult>(
            this Either<Failure<TExceptionIdentifier>, TSource> either, Func<TSource, TResult> function,
            TExceptionIdentifier handleThisException)
        {
            return
                either.Bind(
                    item => TryCatch(item, new Exception<TExceptionIdentifier>(handleThisException, new Exception()), function));
        }

        private static Either<Failure<TExceptionIdentifier>, TResult> TryCatch<TSource, TExceptionIdentifier, TResult>(
            TSource source, IEnumerable<Exception<TExceptionIdentifier>> handleTheseExceptions,
            Func<TSource, TResult> function)
        {
            try
            {
                return new Either<Failure<TExceptionIdentifier>, TResult>(function(source));
            }
            catch (Exception ex)
            {
                var caughtExceptionType = ex.GetType();

                var exceptionsToHandle = handleTheseExceptions
                    .Where(exception => caughtExceptionType.IsInstanceOfType(ex))
                    .Select(error => error.Identifier).ToArray();

                if (exceptionsToHandle.Any())
                {
                    return
                        new Either<Failure<TExceptionIdentifier>, TResult>(
                            new Failure<TExceptionIdentifier>(exceptionsToHandle));
                }
                throw;
            }
        }

        private static Either<Failure<TExceptionIdentifier>, TResult> TryCatch<TSource, TExceptionIdentifier, TResult>(
            TSource source, Exception<TExceptionIdentifier> handleThisException, Func<TSource, TResult> function)
        {
            return TryCatch(source, Enumerable.Repeat(handleThisException, 1), function);
        }

        public static Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult> Retry
            <TSource, TExceptionIdentifier, TResult>(this TSource source,
                Func<TSource, TResult> guarded, TExceptionIdentifier handleThisException)
        {
            return
                Retry(source.AsEither<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TSource>(), guarded,
                    handleThisException);
        }

        public static Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult> Retry
            <TSource, TExceptionIdentifier, TResult>(
            this Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TSource> source,
            Func<TSource, TResult> guarded, TExceptionIdentifier handleThisException)
        {
            return source.Bind(value => RetryCatch(value, guarded, handleThisException));
        }

        private static Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult> RetryCatch
            <TSource, TExceptionIdentifier, TResult>(TSource source,
                Func<TSource, TResult> guarded, TExceptionIdentifier handleThisException)
        {
            try
            {
                return new Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult>(guarded(source));
            }
            catch (Exception)
            {
                return new Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult>(predicate =>
                {
                    var result = default(Either<Failure<TExceptionIdentifier>, TResult>);
                    while (predicate())
                    {
                        result = source.Try(guarded, handleThisException);
                        if (result.RightHasValue) break;
                    }
                    return result;
                });
            }
        }
    }
}