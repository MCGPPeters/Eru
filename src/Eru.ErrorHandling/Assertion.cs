using System;

namespace Eru.ErrorHandling
{
    public class Assertion<TIdentifier, TSubject>
    {
        private readonly Predicate<TSubject> _rule;

        public Assertion(TIdentifier identifier, Predicate<TSubject> rule)
        {
            _rule = rule;
            Identifier = identifier;
        }

        public TIdentifier Identifier { get; set; }

        public bool IsBroken(TSubject subject)
        {
            return !_rule(subject);
        }
    }
}