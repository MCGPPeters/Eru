using System.Collections.Generic;

namespace Eru
{
    public interface IMonoid<T>
    {
        T Identity { get; }
        T Append(T second);
        T Concat(IMonoid<T> other);
    }
}