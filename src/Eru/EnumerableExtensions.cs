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

        public static IEnumerable<T> AsList<T>(this T item) => Return(item);

    }
}
