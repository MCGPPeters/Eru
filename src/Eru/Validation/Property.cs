namespace Eru.Validation
{
    using System;

    public class Property<TIdentifier, TSubject>
    {
        private readonly Predicate<TSubject> _predicate;

        public Property(TIdentifier identifier, Predicate<TSubject> predicate)
        {
            _predicate = predicate;
            Identifier = identifier;
        }

        public TIdentifier Identifier { get; set; }

        public bool Holds(TSubject subject)
        {
            return _predicate(subject);
        }
    }
}