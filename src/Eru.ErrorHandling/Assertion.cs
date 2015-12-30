using System;

namespace Eru.ErrorHandling
{
    public class Assertion<TIdentifier, TSubject>
    {
        private readonly Predicate<TSubject> _predicate;

        public Assertion(TIdentifier identifier, Predicate<TSubject> predicate)
        {
            _predicate = predicate;
            Identifier = identifier;
        }

        public TIdentifier Identifier { get; set; }

        public bool IsBroken(TSubject subject)
        {
            return !_predicate(subject);
        }
    }
}