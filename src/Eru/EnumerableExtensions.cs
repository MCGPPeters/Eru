using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Eru
{
    public static class Enumerable
    {
        public static IEnumerable<T> Return<T>(params T[] items) => items.ToImmutableList();

        public static IEnumerable<T> Return<T>(this T item) => Return(item);

        public static IEnumerable<T> AsList<T>(this T item) => Return(item);

        public static IMonoid<IEnumerable<T>> AsMonoid<T>(this IEnumerable<T> enumerable) =>
            enumerable.AsMonoid((first, second) => first.Concat(second));
    }
}
