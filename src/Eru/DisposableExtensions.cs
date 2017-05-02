namespace Eru
{
    using System;

    public static partial class _
    {
        public static TResult Using<TDisposable, TResult>(TDisposable disposable, Func<TDisposable, TResult> function)
            where TDisposable : IDisposable
        {
            using (disposable) return function(disposable);
        }

        public static TResult Use<TDisposable, TResult>(this TDisposable disposable, Func<TDisposable, TResult> function)
            where TDisposable : IDisposable
        {
            using (disposable) return function(disposable);
        }
    }
}
