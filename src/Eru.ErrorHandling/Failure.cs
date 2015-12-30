using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru.ErrorHandling
{
    public class Failure<TCauseIdentifier> : IEqualityComparer<Failure<TCauseIdentifier>>,
        IEquatable<Failure<TCauseIdentifier>>
    {
        public Failure(IEnumerable<TCauseIdentifier> causeIdentifiers)
        {
            CauseIdentifiers = causeIdentifiers;
        }

        public Failure(TCauseIdentifier causeIdentifier)
        {
            CauseIdentifiers = Enumerable.Repeat(causeIdentifier, 1);
        }

        public IEnumerable<TCauseIdentifier> CauseIdentifiers { get; }

        public bool Equals(Failure<TCauseIdentifier> x, Failure<TCauseIdentifier> y)
        {
            return x.CauseIdentifiers
                .All(identifier => y.CauseIdentifiers
                    .Any(causeIdentifier => causeIdentifier.Equals(identifier)));
        }

        public int GetHashCode(Failure<TCauseIdentifier> obj)
        {
            return CauseIdentifiers?.GetHashCode() ?? 0;
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