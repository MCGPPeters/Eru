using System.Collections.Generic;

namespace Eru.Control
{
    public class Failure<T>
    {
        public Failure(T cause) 
        {
            Cause = cause;
        }

        public T Cause { get; }
    }
}