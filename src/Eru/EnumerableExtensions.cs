using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Eru
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Return<T>(params T[] items) => items.ToImmutableList();

        public static IEnumerable<T> AsList<T>(this T item) => Return(item);

        public static async Task<TAccumulate> Aggregate<T, TAccumulate>(this IEnumerable<T> source, TAccumulate seed, Func<TAccumulate, T, Task<TAccumulate>> func)
        {
            var result = seed;
            // ReSharper disable once LoopCanBeConvertedToQuery => Does not work here. Will be ambiguous invocation
            foreach (var element in source) result = await func(result, element);
            return result;
        }
    }
}