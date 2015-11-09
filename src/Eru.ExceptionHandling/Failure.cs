using System.Collections.Generic;

namespace Eru.ExceptionHandling
{
    public class Failure<T> : IEqualityComparer<Failure<T>>
    {
        public IEnumerable<T> CauseIdentifiers { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(this, (Failure<T>) obj);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public bool Equals(Failure<T> x, Failure<T> y)
        {
            return x.CauseIdentifiers.Equals(y.CauseIdentifiers);
        }

        public int GetHashCode(Failure<T> obj)
        {
            return (CauseIdentifiers != null ? CauseIdentifiers.GetHashCode() : 0);
        }
    }
}