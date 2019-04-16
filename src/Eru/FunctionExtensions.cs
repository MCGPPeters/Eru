using System;

namespace Eru
{
    public static partial class _
    {
        public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(this Func<T1, T2, TResult> func)
            => t1 => t2 => func(t1, t2);

        public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func)
            => t1 => t2 => t3 => func(t1, t2, t3);
    }
}
