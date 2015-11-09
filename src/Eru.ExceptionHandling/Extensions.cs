using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru.ExceptionHandling
{
    public static class Extensions
    {
        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this TSource source, Dictionary<TCauseIdentifier, Failure<TCauseIdentifier>> expectedExceptions,
            Func<TSource, TResult> function)
        {
            return Try(new Either<Failure<TCauseIdentifier>, TSource>(source), expectedExceptions, function);
        }

        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this TSource source, Func<TSource, TResult> function, TCauseIdentifier causeIdentifier)
        {
            return Try(new Either<Failure<TCauseIdentifier>, TSource>(source), function, causeIdentifier);
        }

        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this Either<Failure<TCauseIdentifier>, TSource> either,
            Dictionary<TCauseIdentifier, Failure<TCauseIdentifier>> expectedExceptions, Func<TSource, TResult> function)
        {
            return either.Bind(item => TryCatch(item, expectedExceptions, function));
        }

        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this Either<Failure<TCauseIdentifier>, TSource> either, Func<TSource, TResult> function,
            TCauseIdentifier causeIdentifier)
        {
            return either.Bind(item => TryCatch(item, causeIdentifier, function));
        }

        private static Either<Failure<TCauseIdentifier>, TResult> TryCatch<TSource, TCauseIdentifier, TResult>(
            TSource source, Dictionary<TCauseIdentifier, Failure<TCauseIdentifier>> exceptions,
            Func<TSource, TResult> function)
        {
            try
            {
                return new Either<Failure<TCauseIdentifier>, TResult>(function(source));
            }
            catch (Exception ex)
            {
                var caughtExceptionType = ex.GetType();
                var failedRequirement = default(TCauseIdentifier);

                //check if the exception was meant to be handled
                foreach (var exception in exceptions)
                {
                    var exceptionType = exception.Value.GetType();
                    if (exceptionType.IsAssignableFrom(caughtExceptionType))
                        failedRequirement = exception.Key;
                }

                return
                    new Either<Failure<TCauseIdentifier>, TResult>(
                        new FailureBecauseOfException<TCauseIdentifier>(Enumerable.Repeat(failedRequirement, 1), ex));
            }
        }

        private static Either<Failure<TCauseIdentifier>, TResult> TryCatch<TSource, TCauseIdentifier, TResult>(
            TSource source, TCauseIdentifier causeIdentifier, Func<TSource, TResult> function)
        {
            try
            {
                return new Either<Failure<TCauseIdentifier>, TResult>(function(source));
            }
            catch (Exception ex)
            {
                return
                    new Either<Failure<TCauseIdentifier>, TResult>(
                        new FailureBecauseOfException<TCauseIdentifier>(Enumerable.Repeat(causeIdentifier, 1), ex));
            }
        }

        public static Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TResult> Retry
            <TSource, TCauseIdentifier, TResult>(this TSource source,
                Func<TSource, TResult> guarded, TCauseIdentifier causeIdentifier)
        {
            return
                source.AsEither<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TSource>()
                    .Retry(guarded, causeIdentifier);
        }

        public static Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TResult> Retry
            <TSource, TCauseIdentifier, TResult>(
            this Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TSource> source,
            Func<TSource, TResult> guarded, TCauseIdentifier causeIdentifier)
        {
            return source.Bind(value => RetryCatch(value, guarded, causeIdentifier));
        }

        private static Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TResult> RetryCatch
            <TSource, TCauseIdentifier, TResult>(TSource source,
                Func<TSource, TResult> guarded, TCauseIdentifier causeIdentifier)
        {
            try
            {
                return new Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TResult>(guarded(source));
            }
            catch (Exception)
            {
                return new Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TResult>(predicate =>
                {
                    var result = default(Either<Failure<TCauseIdentifier>, TResult>);
                    while (predicate())
                    {
                        result = source.Try(guarded, causeIdentifier);
                        if (result.RightHasValue) break;
                    }
                    return result;
                });
            }
        }
    }
}