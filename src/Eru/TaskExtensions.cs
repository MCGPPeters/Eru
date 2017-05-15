using System;
using System.Threading.Tasks;

namespace Eru
{
    public static partial class _
    {
        public static Task<Unit> AsTask(this Action action)
            => new Task<Unit>(action.ToFunction());

        public static Task<T> AsTask<T>(this T t)
            => Task.FromResult(t);

        public static async Task<TResult> Map<T, TResult>
            (this Task<T> task, Func<T, TResult> f)
            => f(await task);

        public static Task<TResult> Map<T, TResult>
            (this Task<T> task, Func<Exception, TResult> faulted, Func<T, TResult> completed)
            => task.ContinueWith(t =>
                t.Status == TaskStatus.Faulted
                    ? faulted(t.Exception)
                    : completed(t.Result));

        public static async Task<TResult> Bind<T, TResult>
            (this Task<T> task, Func<T, Task<TResult>> f)
            => await f(await task);

        /// <summary>
        ///     Try to execute the first task, otherwise use the fallback method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static Task<T> Otherwise<T>
            (this Task<T> task, Func<Task<T>> fallback)
            => task.ContinueWith(t =>
                    t.Status == TaskStatus.Faulted
                        ? fallback()
                        : t.Result.AsTask()
                )
                .Unwrap();

        /// <summary>
        ///     Try to execute the first task, otherwise handle the exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static Task<T> Otherwise<T>
            (this Task<T> task, Func<Exception, T> fallback)
            => task.ContinueWith(t =>
                t.Status == TaskStatus.Faulted
                    ? fallback(t.Exception)
                    : t.Result);
    }
}