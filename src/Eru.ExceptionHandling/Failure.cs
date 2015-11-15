using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru.ExceptionHandling
{
    public class Failure<TCauseIdentifier> : IEqualityComparer<Failure<TCauseIdentifier>>, IEquatable<Failure<TCauseIdentifier>>
    {
        public IEnumerable<TCauseIdentifier> CauseIdentifiers { get; private set; }

        public Failure(IEnumerable<TCauseIdentifier> causeIdentifiers)
        {
            CauseIdentifiers = causeIdentifiers;
        }

        public Failure(TCauseIdentifier causeIdentifiers)
        {
            CauseIdentifiers = Enumerable.Repeat(causeIdentifiers, 1);
        }

        public bool Equals(Failure<TCauseIdentifier> x, Failure<TCauseIdentifier> y)
        {
            return x.CauseIdentifiers
                .All(identifier => y.CauseIdentifiers
                .Any(causeIdentifier => causeIdentifier.Equals(identifier)));
        }

        public int GetHashCode(Failure<TCauseIdentifier> obj)
        {
            return (CauseIdentifiers != null ? CauseIdentifiers.GetHashCode() : 0);
        }

        public bool Equals(Failure<TCauseIdentifier> other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(this, (Failure<TCauseIdentifier>) obj);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }
    }
}