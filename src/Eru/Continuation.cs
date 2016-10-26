using System;

namespace Eru
{
    public delegate TAnswer Continuation<out T, TAnswer>(Func<T, TAnswer> continuation);

    public static class Continuation
    {
        private static Continuation<T, TAnswer> Return<T, TAnswer>(this T value) =>
            next => next(value);

        public static Continuation<T, TAnswer> AsContinuation<T, TAnswer>(this T value) =>
            Return<T, TAnswer>(value);

        public static Continuation<TU, TAnswer> SelectMany<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, Continuation<TU, TAnswer>> function)
        {
            return next =>
            {
                return continuation(arg => function(arg)(next));
            };
        }

        public static Continuation<TV, TAnswer> SelectMany<T, TU, TV, TAnswer>(
            this Continuation<T, TAnswer> continuation, Func<T, Continuation<TU, TAnswer>> function, Func<T, TU, TV> map)
        {
            return next =>
            {
                return continuation(arg => function(arg)(u => next(map(arg, u))));
            };
        }

        public static Continuation<TU, TAnswer> Select<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, TU> map)
        {
            return Map(continuation, map);
        }

        private static Continuation<TU, TAnswer> Map<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, TU> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return next =>
            {
                return continuation(arg => next(map(arg)));
            };
        }

        //Func<TRight, Either<TLeft, TResult>> function
        public static Continuation<TU, TAnswer> Bind<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, Continuation<TU, TAnswer>> function)
        {
            return SelectMany(continuation, function);
        }


        public static Continuation<T, TAnswer> If<T, TAnswer>(this Continuation<T, TAnswer> continuation,
                Func<bool> predicate) =>
            predicate()
                ? continuation
                : Bind(continuation, Return<T, TAnswer>);

        public static Continuation<T, TAnswer> If<T, TAnswer>(this Continuation<T, TAnswer> continuation,
                Predicate<T> predicate) =>
            Bind(continuation, arg =>
                predicate(arg)
                    ? continuation
                    : Return<T, TAnswer>(arg));

        public static Continuation<T, Either<Exception, TAnswer>> Try<T, TAnswer>(
            this Continuation<T, Either<Exception, TAnswer>> continuation,
            Func<T, TAnswer> @try)
        {
            return continuation.Bind(arg =>
            {
                try
                {
                    @try(arg);
                    return continuation;
                }
                catch (Exception ex)
                {
                    return c => new Either<Exception, TAnswer>(ex);
                }
            });
        }

        public static Continuation<T, Either<Exception, TAnswer>> Try<T, TAnswer>(this T value, Func<T, TAnswer> @try)
        {
            var continuation = value.AsContinuation<T, Either<Exception, TAnswer>>();
            return Try(continuation, @try);
        }
    }
}
