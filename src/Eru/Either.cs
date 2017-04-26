namespace Eru
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Either
    {
        public static Either<TSuccess, TMessage> Return<TSuccess, TMessage>(this TSuccess value, IEnumerable<TMessage> messages)
        {
            return new Either<TSuccess, TMessage>.Success(value, messages);
        }

        public static Either<TSuccess, TMessage> Return<TSuccess, TMessage>(this TSuccess value)
        {
            return new Either<TSuccess, TMessage>.Success(value);
        }

        public static Either<TSuccess, TMessage> AsEither<TSuccess, TMessage>(this TSuccess value, IEnumerable<TMessage> messages)
        {
            return value.Return(messages);
        }

        public static Either<TSuccess, TMessage> Fail<TSuccess, TMessage>(this IEnumerable<TMessage> messages)
        {
            return new Either<TSuccess, TMessage>.Failure(messages.ToArray());
        }

        public static Either<TResult, TMessage> Bind<TMessage, TSuccess, TResult>(this Either<TSuccess, TMessage> either,
            Func<TSuccess, Either<TResult, TMessage>> function)
        {
            return either.Match(
                (success, messages) => function(success).MergeMessages(messages),
                messages => new Either<TResult, TMessage>.Failure(messages)
            );
        }

        public static Either<TResult, TMessage> Map<TMessage, TSuccess, TResult>(this Either<TSuccess, TMessage> either,
            Func<TSuccess, TResult> function)
        {
            return either.Match(
                (success, messages) => Return(function(success), messages),
                messages => new Either<TResult, TMessage>.Failure(messages));
        }

        public static Either<TSuccess, TResult> MapMessages<TMessage, TSuccess, TResult>(this Either<TSuccess, TMessage> either,
            Func<TMessage, TResult> function)
        {
            return either.Match(
                (success, messages) => Return(success, messages.Select(function)),
                messages => Fail<TSuccess, TResult>(messages.Select(function)));
        }

        public static TResult Match<TSuccess, TMessage, TResult>(this Either<TSuccess, TMessage> either, Func<TSuccess, IEnumerable<TMessage>, TResult> onSuccess,
            Func<IEnumerable<TMessage>, TResult> onFailure)
        {
            switch (either)
            {
                case Either<TSuccess, TMessage>.Success s:
                    return onSuccess(s.Value, s.Messages);
                case Either<TSuccess, TMessage>.Failure f:
                    return onFailure(f.Messages);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Either<TSuccess, TMessage> MergeMessages<TSuccess, TMessage>(
            this Either<TSuccess, TMessage> either, IEnumerable<TMessage> messages)
        {
            return either
                .Match(
                    (success, msgs) => Return(success, messages.Concat(msgs)),
                    msgs => Fail<TSuccess, TMessage>(messages.Concat(msgs))
                );
        }

        public static Either<TResult, TMessage> Select<TMessage, TSuccess, TResult>(this Either<TSuccess, TMessage> either,
            Func<TSuccess, TResult> function)
        {
            return Map(either, function);
        }
    }

    public abstract class Either<TSuccess, TMessage>
    {
        public sealed class Success : Either<TSuccess, TMessage>
        {
            public Success(TSuccess value)
            {
                Value = value;
                Messages = new List<TMessage>();
            }

            public Success(TSuccess value, IEnumerable<TMessage> messages)
            {
                Value = value;
                Messages = messages;
            }

            public IEnumerable<TMessage> Messages { get; }
            public TSuccess Value { get; }
        }

        public sealed class Failure : Either<TSuccess, TMessage>
        {
            public Failure(IEnumerable<TMessage> messages)
            {
                Messages = messages;
            }

            public IEnumerable<TMessage> Messages { get; }
        }
    }
}