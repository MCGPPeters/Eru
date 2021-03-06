﻿using System;
using System.Linq;
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
            (this Task<T> task, Func<AggregateException, TResult> faulted, Func<T, TResult> completed)
            => task.ContinueWith(
                t =>
                    t.Status == TaskStatus.Faulted
                        ? faulted(t.Exception)
                        : completed(t.Result));


    public static async Task<TResult> Bind<T, TResult>
            (this Task<T> task, Func<T, Task<TResult>> f)
            => await f(await task);

        public static async Task<TResult> SelectMany<T, TResult>
            (this Task<T> task, Func<T, Task<TResult>> f)
            => await f(await task);

        public static async Task<TResult> SelectMany<T, TResult>
            (this Task task, Func<Unit, Task<T>> bind, Func<Unit, T, TResult> project)
        {
            await task;
            var r = await bind(Unit);
            return project(Unit, r);
        }

        /// <summary>
        ///     Try to execute the first task, otherwise use the fallback method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static Task<T> Otherwise<T>
            (this Task<T> task, Func<Task<T>> fallback)
            => task.ContinueWith(
                    t =>
                        t.Status == TaskStatus.Faulted
                            ? fallback()
                            : t.Result.AsTask()
                )
                .Unwrap();

        public static Task Otherwise
            (this Task task, Func<Task> fallback)
            => task.ContinueWith(
                    t =>
                        t.Status == TaskStatus.Faulted
                            ? fallback()
                            : t
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
            (this Task<T> task, Func<AggregateException, T> fallback)
            => task.ContinueWith(
                t =>
                    t.Status == TaskStatus.Faulted
                        ? fallback(t.Exception)
                        : t.Result);

        /// <summary>
        ///     Retry with delays as long as the task is in a faulted state. 
        ///     The number of delays also indicate the number of retries
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <param name="delaysBetweenRetries">Delays between retries</param>
        /// <returns></returns>
        public static Task<T> Retry<T>
            (this Func<Task<T>> function, params TimeSpan[] delaysBetweenRetries)
            => delaysBetweenRetries.Length == 0
                ? function()
                : function().Otherwise(
                    () =>
                        from _ in Task.Delay(delaysBetweenRetries.First().Milliseconds)
                        from t in Retry(function, delaysBetweenRetries.Skip(1).ToArray())
                        select t);
    }
}
