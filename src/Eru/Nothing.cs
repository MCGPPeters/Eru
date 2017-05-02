using System;

namespace Eru
{
    public struct Nothing : IMonoid<Nothing>
    {
        internal static readonly Nothing Instance = new Nothing();

        public Nothing Identity => throw new NotImplementedException();

        public Nothing Append(Nothing second) => Instance;

        public Nothing Concat(IMonoid<Nothing> other) => Instance;
    }
}