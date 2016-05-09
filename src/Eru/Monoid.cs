using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace Eru
{
    public delegate T BinaryOperator<T>(T firstArgument, T secondArgument);

    public class Monoid<T> : IMonoid<T>
    {
        private readonly BinaryOperator<T> _binaryOperator;

        public Monoid(T identity, BinaryOperator<T> binaryOperator)
        {
            Identity = identity;
            _binaryOperator = binaryOperator;
        }

        public T Identity { get; }

        public T Append(T second) => _binaryOperator(Identity, second);

        public static T operator +(Monoid<T> first, Monoid<T> second)
        {
            return first.Append(second.Identity);
        }

        public T Concat(IEnumerable<T> ts) { return ts.Aggregate(Identity, _binaryOperator.Invoke); }
    }
}
