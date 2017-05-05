using System;
using System.Collections.Generic;

namespace Eru
{
    public interface IMonoid<T>
    {
        T Identity { get; }
        T Append(T t);
        T Concat(IMonoid<T> other);
    }
}