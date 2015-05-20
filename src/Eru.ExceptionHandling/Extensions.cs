using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eru;

namespace Eru.ExceptionHandling
{
    public static class Extensions
    {
        public static Either<Exception, TResult> Try<TSource, TResult>(this TSource source,
            Func<TSource, TResult> guarded)
        {
            return source.AsEither<Exception, TSource>().Try(guarded);
        }

        public static Either<Exception, TResult> Try<TSource, TResult>(this Either<Exception, TSource> source,
            Func<TSource, TResult> guarded)
        {
            return source.Bind(value => TryCatch(value, guarded));
        }

        private static Either<Exception, TResult> TryCatch<TSource, TResult>(TSource source,
            Func<TSource, TResult> guarded)
        {
            try
            {
                return new Either<Exception, TResult>(guarded(source));
            }
            catch (Exception ex)
            {
                return new Either<Exception, TResult>(ex);
            }
        }

        public static Either<Func<Func<bool>, Either<Exception, TResult>>, TResult> Retry<TSource, TResult>(this TSource source,
            Func<TSource, TResult> guarded)
        {
            return source.AsEither<Func<Func<bool>, Either<Exception, TResult>>, TSource>().Retry(guarded);
        }

        public static Either<Func<Func<bool>, Either<Exception, TResult>>, TResult> Retry<TSource, TResult>(this Either<Func<Func<bool>, Either<Exception, TResult>>, TSource> source,
            Func<TSource, TResult> guarded)
        {
            return source.Bind(value => RetryCatch(value, guarded));
        }

        private static Either<Func<Func<bool>, Either<Exception, TResult>>, TResult> RetryCatch<TSource, TResult>(TSource source,
            Func<TSource, TResult> guarded)
        {
            try
            {
                return new Either<Func<Func<bool>, Either<Exception, TResult>>, TResult>(guarded(source));
            }
            catch (Exception)
            {
                return new Either<Func<Func<bool>, Either<Exception, TResult>>, TResult>(predicate =>
                {
                    Either<Exception, TResult> result = default(Either<Exception, TResult>);
                    while (predicate())
                    {
                        result = Try(source, guarded);
                        if (result.RightHasValue) break;
                    }
                    return result;
                });
            }
        }

        public static Either<Exception, TResult> While<TResult>(this Either<Func<Func<bool>, Either<Exception, TResult>>, TResult> source,
            Func<bool> predicate)
        {
            return source.LeftHasValue ? source.Left(predicate) : new Either<Exception, TResult>(source.Right);
        }

        public static async Task<Either<Exception, TResult>> WhileAsync<TResult>(this Either<Func<Func<bool>, Either<Exception, TResult>>, TResult> source,
            Func<bool> predicate)
        {
            return source.LeftHasValue ? await Task.Run(() => source.Left(predicate)) : await Task.FromResult(new Either<Exception, TResult>(source.Right));
        }
    }
}