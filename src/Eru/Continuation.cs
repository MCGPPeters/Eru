using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eru
{
    public delegate TAnswer Continuation<out T, TAnswer>(Func<T, TAnswer> continuation);

    public static class Continuation
    {
        private static Continuation<T, TAnswer> Return<T, TAnswer>(this T value)
        {
            return next => next(value);
        }

        public static Continuation<T, TAnswer> AsContinuation<T, TAnswer>(this T value)
        {
            return Return<T, TAnswer>(value);
        }

        public static Continuation<TU, TAnswer> SelectMany<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation, Func<T, Continuation<TU, TAnswer>> function)
        {
            return next =>
            {
                return continuation(arg => function(arg)(next));
            };
        }

        public static Continuation<TV, TAnswer> SelectMany<T, TU, TV, TAnswer>(this Continuation<T, TAnswer> continuation, Func<T, Continuation<TU, TAnswer>> function, Func<T, TU, TV> map)
        {
            return next =>
            {
                return continuation(arg => function(arg)(u => next(map(arg, u))));
            };
        }

        public static Continuation<TU, TAnswer> Select<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation, Func<T, TU> map)
        {
            return Map(continuation, map);
        }

        private static Continuation<TU, TAnswer> Map<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation, Func<T, TU> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return next =>
            {
                return continuation(arg => next(map(arg)));
            };
        }

        public static Continuation<TU, TAnswer> Bind<T, TU, TAnswer>(this Continuation<T, TAnswer> continuation, Func<T, Continuation<TU, TAnswer>> function)
        {
            return SelectMany(continuation, function);
        }
    }
}
