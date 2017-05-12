using System;

namespace Eru.Control
{
    public delegate Tuple<T, TState> StateFunc<T, TState>(TState state);
}