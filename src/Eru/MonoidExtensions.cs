namespace Eru
{
    public static class MonoidExtensions
    {
        public static IMonoid<T> AsMonoid<T>(this T unit, BinaryOperator<T> binaryOperator)
        {
            return new Monoid<T>(unit, binaryOperator);
        }
    }
}