using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Eru.Tests
{
    public class DomainTests
    {
        [Fact(DisplayName = "The name of the event can be retrieved")]
        public void Test1()
        {
            var accountCreated = new AccountCreated(DateTimeOffset.UtcNow);
            var account = AggregateRoot.Create<Account>(accountCreated, AccountExtensions.HandleEvent);


            Assert.Equal("AccountCreated", accountCreated.Name);
        }
    }

    public static class AccountExtensions
    {
        public static AggregateRoot<Account> HandleEvent(this AggregateRoot<Account> account, Event @event)
        {
            switch (@event)
            {
                case AccountCreated accountCreated:
                    return new Account(accountCreated.Status);
                default:
                    return account;
            }
        }
    }

    public class Account : AggregateRoot<Account>
    {
        public Account(Status status)
        {
            Status = status;
        }

        public Status Status { get; }
    }

    public class AggregateRoot<T> where T : AggregateRoot<T>
    {
        public AggregateRoot()
        {
            EntityIdentifier = Guid.NewGuid();
        }

        public EntityIdentifier EntityIdentifier { get; }
    }

    public static class AggregateRoot
    {
        public static AggregateRoot<T> Create<T>(Event initialEvent,
            Func<AggregateRoot<T>, Event, AggregateRoot<T>> handleEvent)
            where T : AggregateRoot<T>
        {
            return new AggregateRoot<T>().Apply(initialEvent, handleEvent);
        }

        public static AggregateRoot<TEntity> Apply<TEntity>(this AggregateRoot<TEntity> aggregateRoot,
            Event @event, Func<AggregateRoot<TEntity>, Event, AggregateRoot<TEntity>> handle)
            where TEntity : AggregateRoot<TEntity>
        {
            return @event.EntityIdentifier == aggregateRoot.EntityIdentifier
                ? handle(aggregateRoot, @event)
                : aggregateRoot;
        }

        public static AggregateRoot<TEntity> From<TEntity>(IEnumerable<Event> events, Event initialEvent,
            Func<AggregateRoot<TEntity>, Event, AggregateRoot<TEntity>> handleEvent)
            where TEntity : AggregateRoot<TEntity>
        {
            return events.Aggregate(Create(initialEvent, handleEvent), (current, next) =>
                current.Apply(next, handleEvent));
        }

        public static AggregateRoot<TEntity> Handle<TEntity>(this AggregateRoot<TEntity> aggregateRoot, Event @event,
            Func<AggregateRoot<TEntity>, Event, AggregateRoot<TEntity>> handleEvent)
            where TEntity : AggregateRoot<TEntity>
        {
            return @event.EntityIdentifier == aggregateRoot.EntityIdentifier
                ? handleEvent(aggregateRoot, @event)
                : aggregateRoot;
        }
    }
}

public struct EntityIdentifier
{
    public bool Equals(EntityIdentifier other)
    {
        return Value.Equals(other.Value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is EntityIdentifier && Equals((EntityIdentifier)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public Guid Value { get; }

    public EntityIdentifier(Guid value)
    {
        Value = value;
    }

    public static implicit operator EntityIdentifier(Guid identifier)
    {
        return new EntityIdentifier(identifier);
    }

    public static implicit operator Guid(EntityIdentifier identifier)
    {
        return identifier.Value;
    }

    public static bool operator ==(EntityIdentifier entityIdentifier, EntityIdentifier other)
    {
        return entityIdentifier.Equals(other.Value);
    }

    public static bool operator !=(EntityIdentifier entityIdentifier, EntityIdentifier other)
    {
        return !entityIdentifier.Equals(other.Value);
    }
}

public enum Status
{
    Active
}

public class AccountCreated : Event
{
    public AccountCreated(DateTimeOffset timestamp) : base(timestamp)
    {
        Status = Status.Active;
    }

    public Status Status { get; }
}

public class Event
{
    /// <summary>
    ///     Constructor that generates a new <see cref="EntityIdentifier"></see>.
    ///     Mostly intended for creating initial events. />
    /// </summary>
    /// <param name="timestamp"></param>
    protected Event(DateTimeOffset timestamp) : this(timestamp, Guid.NewGuid())
    {
    }

    protected Event(DateTimeOffset timestamp, EntityIdentifier entityIdentifier)
    {
        Timestamp = timestamp;
        EntityIdentifier = entityIdentifier;
        Name = GetType().Name;
    }

    public EntityIdentifier EntityIdentifier { get; }
    public DateTimeOffset Timestamp { get; }

    public string Name { get; }
}