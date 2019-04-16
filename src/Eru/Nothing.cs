namespace Eru
{
    public struct Nothing : IMonoid<Nothing>
    {
        internal static readonly Nothing Instance = new Nothing();

        public Nothing Identity => Instance;

        public Nothing Append(Nothing second) => Instance;

        public Nothing Concat(IMonoid<Nothing> other) => Instance;
    }
}
