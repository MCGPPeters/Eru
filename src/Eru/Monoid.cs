namespace Eru
{
    public delegate T BinaryOperator<T>(T firstArgument, T secondArgument);

    public abstract class Monoid<T> : IMonoid<T>
    {
        public abstract T Identity { get; }
        public abstract T Append(T t);

        public T Concat(IMonoid<T> other) => Append(other.Identity);

        public static T operator +(Monoid<T> first, Monoid<T> second) => first.Concat(second);
    }
}