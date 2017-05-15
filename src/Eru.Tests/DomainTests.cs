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
            var account = new Account(accountCreated.Status);


            Assert.Equal("AccountCreated", accountCreated.Name);
        }
    }

    public static class AccountExtensions
    {
        public static Account HandleEvent(this Account account, Event @event)
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

    public class Account
    {
        public Account(Status status) => Status = status;

        public Status Status { get; }
    }

    public static class AggregateRoot
    {
        public static TEntity From<TEntity>(this TEntity seed, IEnumerable<Event> events,
            Func<TEntity, Event, TEntity> apply) =>
            events.Aggregate(seed, apply);
    }
}

public struct EntityIdentifier
{
    public bool Equals(EntityIdentifier other) => Value.Equals(other.Value);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is EntityIdentifier && Equals((EntityIdentifier) obj);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public Guid Value { get; }

    public EntityIdentifier(Guid value) => Value = value;

    public static implicit operator EntityIdentifier(Guid identifier) => new EntityIdentifier(identifier);

    public static implicit operator Guid(EntityIdentifier identifier) => identifier.Value;

    public static bool operator ==(EntityIdentifier entityIdentifier, EntityIdentifier other) =>
        entityIdentifier.Equals(other.Value);

    public static bool operator !=(EntityIdentifier entityIdentifier, EntityIdentifier other) => !entityIdentifier
        .Equals(other.Value);
}

public enum Status
{
    Active
}

public class AccountCreated : Event
{
    public AccountCreated(DateTimeOffset timestamp) : base(timestamp) => Status = Status.Active;

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