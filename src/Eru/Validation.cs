using System;
using System.Collections.Generic;
using System.Linq;
using Eru;

namespace Eru
{
    public static partial class _
    {
        public static Error Error(params string[] messages)
        {
            return new Error(messages);
        }
    }

    public class Error : Monoid<Error>
    {

        public string[] Messages { get; }

        public static implicit operator Error(string message)
        {
            return new Error(message);
        }

        public static implicit operator Error(string[] messages)
            => messages.Select(m => new Error(m)).Aggregate((error, error1) => error.Append(error1));

        public static implicit operator string[] (Error error)
        {
            return error.Messages;
        }

        public override Error Identity => Messages;
        public override Error Append(Error t) => new Error(t);

        public Error(params string[] messages) => Messages = messages;
    }
}

public class ErrorEqualityComparer : EqualityComparer<Error>
{
    public override bool Equals(Error x, Error y)
    {
        return x.Messages.SequenceEqual(x.Messages);
    }

    public override int GetHashCode(Error obj)
    {
        return obj.GetHashCode();
    }
}

public static class ValidationExtensions
{
    private static Func<TValue, Either<TValue, Error>> FailFast<TValue>(
        Func<TValue, Either<TValue, Error>>[] validations)
    {
        return value =>
            validations.Aggregate(value.Return<TValue, Error>(), (current, validation) =>
                current.Bind(_ => validation(value)));
    }

    private static Func<TValue, Either<TValue, Error>> HarvestErrors<TValue>(
        Func<TValue, Either<TValue, Error>>[] validations)
    {
        return value =>
        {
            var errors = validations
                .Select(validate => validate(value))
                .SelectMany(either => either.Alternative())
                .ToList();

            return errors.Count == 0
                ? value.Return<TValue, Error>()
                : errors.Aggregate((current, next) => _.Error(current.Append(next)))
                    .ReturnAlternative<TValue, Error>();
        };
    }

    private static Either<TValue, Error> Check<TValue>(this TValue value,
        Func<Func<TValue, Either<TValue, Error>>[], Func<TValue, Either<TValue, Error>>> reduceValidations,
        params Func<TValue, Either<TValue, Error>>[] validations)
    {
        return reduceValidations(validations)(value);
    }

    private static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
        params Func<TValue, Either<TValue, Error>>[] validations)
    {
        return either.Bind(value => Check(value, validations));
    }

    /// <summary>
    ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <param name="validations"></param>
    /// <returns></returns>
    public static Either<TValue, Error> CheckQuick<TValue>(this TValue value,
        params Func<TValue, Either<TValue, Error>>[] validations)
    {
        return Check(value, FailFast, validations);
    }

    /// <summary>
    ///     Validate and aggregate all validation errors that may occur into 1 error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <param name="validations"></param>
    /// <returns></returns>
    public static Either<TValue, Error> Check<TValue>(this TValue value,
        params Func<TValue, Either<TValue, Error>>[] validations)
    {
        return Check(value, HarvestErrors, validations);
    }

    /// <summary>
    ///     Validate and aggregate all validation errors that may occur into 1 error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <param name="validations"></param>
    /// <returns></returns>
    public static Either<TValue, Error> Check<TValue>(this TValue value,
        params (Predicate<TValue> rule, Error error)[] validations)
    {
        return value
            .Return<TValue, Error>()
            .Check(validations);
    }

    /// <summary>
    ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <param name="validations">A list rules and corresponding errors</param>
    /// <returns></returns>
    public static Either<TValue, Error> CheckQuick<TValue>(this TValue value,
        params (Predicate<TValue> rule, Error error)[] validations)
    {
        return value
            .Return<TValue, Error>()
            .CheckQuick(validations);
    }

    /// <summary>
    ///     Validate and aggregate all validation errors that may occur into 1 error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="either"></param>
    /// <param name="validations"></param>
    /// <returns></returns>
    public static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
        params (Predicate<TValue> rule, Error error)[] validations)
    {
        return either.Bind(value =>
            Check(value, validations
                .Select<(Predicate<TValue> rule, Error error), Func<TValue, Either<TValue, Error>>>(tuple =>
                    v => tuple.rule(v)
                        ? v.Return<TValue, Error>()
                        : tuple.error.ReturnAlternative<TValue, Error>()).ToArray()));
    }

    /// <summary>
    ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="either"></param>
    /// <param name="validations"></param>
    /// <returns></returns>
    public static Either<TValue, Error> CheckQuick<TValue>(this Either<TValue, Error> either,
        params (Predicate<TValue> rule, Error error)[] validations)
    {
        return either.Bind(value =>
            CheckQuick(value, validations
                .Select<(Predicate<TValue> rule, Error error), Func<TValue, Either<TValue, Error>>>(tuple =>
                    v => tuple.rule(v)
                        ? v.Return<TValue, Error>()
                        : tuple.error.ReturnAlternative<TValue, Error>()).ToArray()));
    }

    /// <summary>
    ///     Validate and aggregate all validation errors that may occur into 1 error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="either"></param>
    /// <param name="rule"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
        Predicate<TValue> rule, Error error)
    {
        Func<TValue, TValue> identity = value => value;

        var validation = identity
            .Return<Func<TValue, TValue>, Error>().MapAlternative(_ => error);

        return either.Apply(validation);
    }

    /// <summary>
    ///     Validate and aggregate all validation errors that may occur into 1 error
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <param name="rule"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Either<TValue, Error> Check<TValue>(this TValue value,
        Predicate<TValue> rule, Error error)
    {
        return Check(value, (rule, error));
    }

    /// <summary>
    ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="either"></param>
    /// <param name="rule"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Either<TValue, Error> CheckQuick<TValue>(this Either<TValue, Error> either,
        Predicate<TValue> rule, Error error)
    {
        return CheckQuick(either, (rule, error));
    }


}