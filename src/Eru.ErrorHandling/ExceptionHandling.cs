using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru.ErrorHandling
{
    public static class ExceptionHandling
    {
        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this TSource source, IEnumerable<Error<TCauseIdentifier>> knownPossibleErrors,
            Func<TSource, TResult> function)
        {
            return Try(new Either<Failure<TCauseIdentifier>, TSource>(source), knownPossibleErrors, function);
        }

        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this TSource source, Func<TSource, TResult> function, TCauseIdentifier causeIdentifier)
        {
            return Try(new Either<Failure<TCauseIdentifier>, TSource>(source), function, causeIdentifier);
        }

        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this Either<Failure<TCauseIdentifier>, TSource> either,
            IEnumerable<Error<TCauseIdentifier>> knownPossibleErrors, Func<TSource, TResult> function)
        {
            return either.Bind(item => TryCatch(item, knownPossibleErrors, function));
        }

        public static Either<Failure<TCauseIdentifier>, TResult> Try<TSource, TCauseIdentifier, TResult>(
            this Either<Failure<TCauseIdentifier>, TSource> either, Func<TSource, TResult> function,
            TCauseIdentifier causeIdentifier)
        {
            return
                either.Bind(
                    item => TryCatch(item, new Error<TCauseIdentifier>(causeIdentifier, new Exception()), function));
        }

        private static Either<Failure<TCauseIdentifier>, TResult> TryCatch<TSource, TCauseIdentifier, TResult>(
            TSource source, IEnumerable<Error<TCauseIdentifier>> knownPossibleErrors,
            Func<TSource, TResult> function)
        {
            try
            {
                return new Either<Failure<TCauseIdentifier>, TResult>(function(source));
            }
            catch (Exception ex)
            {
                var caughtExceptionType = ex.GetType();

                var errorsToHandle = knownPossibleErrors
                    .Where(exception => caughtExceptionType.IsInstanceOfType(ex))
                    .Select(error => error.Identifier).ToArray();

                if (errorsToHandle.Any())
                {
                    return
                        new Either<Failure<TCauseIdentifier>, TResult>(
                            new Failure<TCauseIdentifier>(errorsToHandle));
                }
                throw;
            }
        }

        private static Either<Failure<TCauseIdentifier>, TResult> TryCatch<TSource, TCauseIdentifier, TResult>(
            TSource source, Error<TCauseIdentifier> knownPossibleError, Func<TSource, TResult> function)
        {
            return TryCatch(source, Enumerable.Repeat(knownPossibleError, 1), function);
        }

        public static Either<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TResult> Retry
            <TSource, TCauseIdentifier, TResult>(this TSource source,
                Func<TSource, TResult> guarded, TCauseIdentifier causeIdentifier)
        {
            return
                Retry(source.AsEither<Func<Func<bool>, Either<Failure<TCauseIdentifier>, TResult>>, TSource>(), guarded,
                    causeIdentifier);
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