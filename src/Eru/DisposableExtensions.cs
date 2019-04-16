using System;

namespace Eru
{
    public static partial class _
    {
        public static TResult Using<TDisposable, TResult>(TDisposable disposable, Func<TDisposable, TResult> function)
            where TDisposable : IDisposable
        {
            using (disposable)
            {
                return function(disposable);
            }
        }

        public static TResult Use<TDisposable, TResult>(this TDisposable disposable,
            Func<TDisposable, TResult> function)
            where TDisposable : IDisposable
        {
            using (disposable)
            {
                return function(disposable);
            }
        }

        public static Unit Use<TDisposable>(this TDisposable disposable,
            Action<TDisposable> action)
            where TDisposable : IDisposable => Use(disposable, action.ToFunction());
    }
}
