using System.Collections.Generic;
using System.Collections.Immutable;

namespace Eru
{
    public static class Enumerable
    {
        public static IEnumerable<T> Return<T>(params T[] items) => items.ToImmutableList();

        public static IEnumerable<T> AsList<T>(this T item) => Return(item);
    }
}