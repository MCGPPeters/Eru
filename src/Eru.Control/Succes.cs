using System.Collections.Generic;

namespace Eru.Control
{
    public class Success<T> : Result<T>
    {
        protected bool Equals(Success<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public Success(T value) : base(value)
        {
        }
    }
}