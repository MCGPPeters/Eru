using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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

        /// <summary>
        /// Partition the sequence in parts of <paramref name="length"/>. The operation is O(1)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>>
            Partition<T>(this IEnumerable<T> source, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var partition = new List<T>(length);

            foreach (var item in source)
            {
                partition.Add(item);

                if (partition.Count == length)
                {
                    yield return partition.AsReadOnly();
                    partition = new List<T>(length);
                }
            }

            if (partition.Count > 0)
                yield return partition.AsReadOnly();
        }
    }
}