using System;

namespace Eru
{
    public static partial class _
    {
        public static Func<Unit> ToFunction(this Action action)
            => () =>
            {
                action();
                return Unit;
            };

        public static Func<T, Unit> ToFunction<T>(this Action<T> action)
            => t =>
            {
                action(t);
                return Unit;
            };

        public static Func<T1, T2, Unit> ToFunction<T1, T2>(this Action<T1, T2> action)
            => (t1, t2) =>
            {
                action(t1, t2);
                return Unit;
            };
    }
}
