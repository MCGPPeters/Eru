using System.Collections.Generic;

namespace Eru.Control
{
    public class Result<T>
    {
        public Result(T value)
        {
            Value = value;
        }

        public T Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Result<T>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }

        public static bool operator ==(Result<T> first, Result<T> second)
            // implicit digit to byte conversion operator
        {
            return first != null && first.Equals(second);
        }

        public static bool operator !=(Result<T> first, Result<T> second)
        {
            return !(first == second);
        }

        public static implicit operator Result<T>(T value)
        {
            return new Success<T>(value);
        }

        public static implicit operator T(Result<T> success)
        {
            return success.Value;
        }
    }
}