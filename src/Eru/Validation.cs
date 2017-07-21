using System;
using System.Collections.Generic;
using System.Linq;

namespace Eru
{
    public static partial class _
    {
        public static Error Error(params string[] messages) => new Error(messages);
    }

    public class Error : Monoid<Error>
    {
        public Error(params string[] messages) => Messages = messages;

        public string[] Messages { get; }

        public override Error Identity => Messages;

        public static implicit operator Error(string message) => new Error(message);

        public static implicit operator Error(string[] messages) => messages.Select(m => new Error(m))
            .Aggregate((current, next) => current.Append(next));

        public static implicit operator string[] (Error error) => error.Messages;

        public override Error Append(Error t) => new Error(t);
    }

    public static partial class _
    {
        private static Func<T, Either<T, Error>> FailFast<T>(
            Func<T, Either<T, Error>>[] validations) => value =>
            validations.Aggregate(value.AsEither<T, Error>(), (current, validation) =>
                current.Bind(_ => validation(value)));

        private static Func<T, Either<T, Error>> HarvestErrors<T>(
            Func<T, Either<T, Error>>[] validations) => value =>
        {
            var errors = validations
                .Select(validate => validate(value))
                .SelectMany(either => either.Otherwise())
                .ToList();

            return errors.Count == 0
                ? value.AsEither<T, Error>()
                : errors.Aggregate((current, next) => Error(current.Append(next)))
                    .AsEither<T, Error>();
        };

        private static Either<T, Error> Check<T>(this T value,
            Func<Func<T, Either<T, Error>>[], Func<T, Either<T, Error>>> reduceValidations,
            params Func<T, Either<T, Error>>[] validations) => reduceValidations(validations)(value);

        private static Either<T, Error> Check<T>(this Either<T, Error> either,
            params Func<T, Either<T, Error>>[] validations) =>
            either.Bind(value => Check(value, validations));

        /// <summary>
        ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<T, Error> CheckQuick<T>(this T value,
            params Func<T, Either<T, Error>>[] validations) => Check(value, FailFast, validations);

        /// <summary>
        ///     Validate and aggregate all validation errors that may occur into 1 error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<T, Error> Check<T>(this T value,
            params Func<T, Either<T, Error>>[] validations) => Check(value, HarvestErrors, validations);

        /// <summary>
        ///     Validate and aggregate all validation errors that may occur into 1 error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<T, Error> Check<T>(this T value,
            params (Predicate<T> rule, Error error)[] validations) => value
            .AsEither<T, Error>()
            .Check(validations);

        /// <summary>
        ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations">A list rules and corresponding errors</param>
        /// <returns></returns>
        public static Either<T, Error> CheckQuick<T>(this T value,
            params (Predicate<T> rule, Error error)[] validations) => value
            .AsEither<T, Error>()
            .CheckQuick(validations);

        /// <summary>
        ///     Validate and aggregate all validation errors that may occur into 1 error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="either"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<T, Error> Check<T>(this Either<T, Error> either,
            params (Predicate<T> rule, Error error)[] validations) => either.Bind(value =>
            Check(value, validations
                .Select<(Predicate<T> rule, Error error), Func<T, Either<T, Error>>>(tuple =>
                    v => tuple.rule(v)
                        ? v.AsEither<T, Error>()
                        : tuple.error.AsEither<T, Error>()).ToArray()));

        /// <summary>
        ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="either"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<T, Error> CheckQuick<T>(this Either<T, Error> either,
            params (Predicate<T> rule, Error error)[] validations) => either.Bind(value =>
            CheckQuick(value, validations
                .Select<(Predicate<T> rule, Error error), Func<T, Either<T, Error>>>(tuple =>
                    v => tuple.rule(v)
                        ? v.AsEither<T, Error>()
                        : tuple.error.AsEither<T, Error>()).ToArray()));

        /// <summary>
        ///     Validate and aggregate all validation errors that may occur into 1 error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="either"></param>
        /// <param name="rule"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Either<T, Error> Check<T>(this Either<T, Error> either,
            Predicate<T> rule, Error error)
        {
            Func<T, T> identity = value => value;

            var validation = identity
                .AsEither<Func<T, T>, Error>()
                .OtherwiseMap(_ => error);

            return either.Apply(validation);
        }

        /// <summary>
        ///     Validate and aggregate all validation errors that may occur into 1 error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="rule"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Either<T, Error> Check<T>(this T value,
            Predicate<T> rule, Error error) => Check(value, (rule, error));

        /// <summary>
        ///     Validate and fail as soon as one validation fails and do not aggragate any error messages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="either"></param>
        /// <param name="rule"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Either<T, Error> CheckQuick<T>(this Either<T, Error> either,
            Predicate<T> rule, Error error) => CheckQuick(either, (rule, error));
    }

    public class ErrorEqualityComparer : EqualityComparer<Error>
    {
        public override bool Equals(Error x, Error y) => x.Messages.SequenceEqual(y.Messages);

        public override int GetHashCode(Error obj) => obj.GetHashCode();
    }
}