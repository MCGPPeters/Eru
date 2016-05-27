using System;

namespace Eru.ErrorHandling
{
    public static class ExceptionHandling
    {
        public static Either<Failure<TExceptionIdentifier>, TResult> Try<TSource, TExceptionIdentifier, TResult>(
            this TSource source, Func<TSource, TResult> function, TExceptionIdentifier exceptionIdentifier)
        {
            return Try(new Either<Failure<TExceptionIdentifier>, TSource>(source), function, exceptionIdentifier);
        }

        public static Either<Failure<TExceptionIdentifier>, TResult> Try<TSource, TExceptionIdentifier, TResult>(
            this Either<Failure<TExceptionIdentifier>, TSource> either, Func<TSource, TResult> function,
            TExceptionIdentifier exceptionIdentifier)
        {
            return
                either.Bind(
                    item => TryCatch(item, exceptionIdentifier, function));
        }

        private static Either<Failure<TExceptionIdentifier>, TResult> TryCatch<TSource, TExceptionIdentifier, TResult>(
            TSource source, TExceptionIdentifier exceptionIdentifier,
            Func<TSource, TResult> function)
        {
            try
            {
                return new Either<Failure<TExceptionIdentifier>, TResult>(function(source));
            }
            catch (Exception ex)
            {
                return
                    new Either<Failure<TExceptionIdentifier>, TResult>(
                        new Exception<TExceptionIdentifier>(exceptionIdentifier, ex));
            }
        }

        public static Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult> Retry
            <TSource, TExceptionIdentifier, TResult>(this TSource source,
                Func<TSource, TResult> guarded, TExceptionIdentifier exceptionIdentifier)
        {
            return
                Retry(source.Return<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TSource>(),
                    guarded,
                    exceptionIdentifier);
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
                Func<TSource, TResult> guarded, TExceptionIdentifier exceptionIdentifier)
        {
            try
            {
                return
                    new Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult>(guarded(source));
            }
            catch (Exception)
            {
                return
                    new Either<Func<Func<bool>, Either<Failure<TExceptionIdentifier>, TResult>>, TResult>(predicate =>
                    {
                        var result = default(Either<Failure<TExceptionIdentifier>, TResult>);
                        while (predicate())
                        {
                            result = source.Try(guarded, exceptionIdentifier);
                            if (result.RightHasValue) break;
                        }
                        return result;
                    });
            }
        }
    }
}