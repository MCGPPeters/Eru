using System.Collections;
using System.Collections.Generic;

namespace Eru
{
    public struct Optional<T> : IEnumerable<T>
    {
        public bool Equals(Optional<T> other) => EqualityComparer<T>.Default.Equals(_value, other._value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Optional<T> && Equals((Optional<T>) obj);
        }

        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_value);

        private readonly T _value;

        public Optional(T value) => _value = value;

        public IEnumerator<T> GetEnumerator()
        {
            yield return _value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);

        public static bool operator ==(Optional<T> @this, Optional<T> other) => @this.Equals(other);

        public static bool operator !=(Optional<T> @this, Optional<T> other) => !(@this == other);
    }
}
