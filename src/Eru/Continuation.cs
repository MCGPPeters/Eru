using System;

namespace Eru
{
    public delegate TAnswer Continuation<out T, TAnswer>(Func<T, TAnswer> continuation);

    public static partial class _
    {
        private static Continuation<T, TAnswer> AsContinuation<T, TAnswer>(this T value) =>
            next =>
                next(value);

        public static Continuation<TU, TAnswer> SelectMany<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, Continuation<TU, TAnswer>> function) =>
            next =>
                continuation(arg => function(arg)(next));

        public static Continuation<TV, TAnswer> SelectMany<T, TU, TV, TAnswer>(
            this Continuation<T, TAnswer> continuation, Func<T, Continuation<TU, TAnswer>> function,
            Func<T, TU, TV> map) =>
            next =>
                continuation(arg => function(arg)(u => next(map(arg, u))));

        public static Continuation<TU, TAnswer> Select<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, TU> map) =>
            Map(continuation, map);

        private static Continuation<TU, TAnswer> Map<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, TU> map) =>
            next =>
                continuation(arg => next(map(arg)));

        public static Continuation<TU, TAnswer> Bind<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation,
            Func<T, Continuation<TU, TAnswer>> function) =>
            SelectMany(continuation, function);

        public static Continuation<T, T> Where<T>(this Continuation<T, T> continuation,
            Predicate<T> predicate) =>
            Bind(continuation, arg =>
                predicate(arg)
                    ? AsContinuation<T, T>(arg)
                    : (func => arg));
    }
}