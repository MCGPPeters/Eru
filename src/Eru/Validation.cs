using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Eru.Validation
{
    using static _;

    public class Error : Error<string>
    {
        public Error(string identity) : base(identity)
        {
        }

        public string Message => Identity;

        public static implicit operator Error(string message)
            => new Error(message);
        public static implicit operator string(Error error)
            => error.Identity;


        public override string AppendCore(string t) => Identity + t;
    }


    public class Error<T> : Monoid<T>
    {
        public Error(T identity) : base(identity) { }
        public override T Append(T t) => AppendCore(t);

        public virtual T AppendCore(T t) => t;

        public static implicit operator Error<T>(T message)
            => new Error<T>(message);
        public static implicit operator T(Error<T> error)
            => error.Identity;
        public static T operator +(Error<T> first, Error<T> second)
        {
            return first.Append(second);
        }
    }

    public static class ErrorExtensions
    {
        public static Error<T> Return<T>(this T identity) => new Error<T>(identity);

        public static Error<R> Map<T, R>(this Error<T> error, Func<T, R> function) =>
            Return(function(error.Identity));

        public static Error<R> Apply<T, R>(this Error<Func<T, R>> errorFunction, Error<T> error)
            => errorFunction.Map(f => f(error.Identity));
    }

    public static class ValidationExtensions
    {
        public static Func<TValue, Either<TValue, Error<TError>>> FailFast<TValue, TError>(
            Func<TValue, Either<TValue, Error<TError>>>[] validations) =>
                value =>
                    validations.Aggregate(value.Return<TValue, Error<TError>>(), (current, validation) =>
                        current.Bind(_ => validation(value)));

        public static Func<TValue, Either<TValue, Error<TError>>> HarvestErrors<TValue, TError>(Func<TValue, Either<TValue, Error<TError>>>[] validations)
             =>
            value =>
            {
                var errors = validations.Select(validate => validate(value))
                    .Select(validationResult => validationResult.Map(_ => value)).ToList();
                return errors.Count == 0
                    ? value.Return<TValue, Error<TError>>()
                    : errors.Aggregate(default(Error<TError>).ReturnAlternative<TValue, Error<TError>>(), (current, next) =>
                    {
                        var currentError = current.MatchAlternative(error => error);
                        var nextError = next.MatchAlternative(error => error);
                        return _.ReturnAlternative<TValue, Error<TError>>(currentError.Append(nextError));
                    });
            };

        /// <summary>
        /// Validate and fail as soon as one validation fails and do not aggragate any error messages
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<TValue, Error<TError>> CheckQuick<TValue, TError>(this TValue value,
            params Func<TValue, Either<TValue, Error<TError>>>[] validations)
           =>
                Check(value, FailFast, validations);

        /// <summary>
        /// Validate and aggregate all validation errors that may occur
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="value"></param>
        /// <param name="validations"></param>
        /// <returns></returns>
        public static Either<TValue, Error<TError>> Check<TValue, TError>(this TValue value,
            params Func<TValue, Either<TValue, Error<TError>>>[] validations) =>
                Check(value, HarvestErrors, validations);

        public static Either<TValue, TError> Check<TValue, TError>(this TValue value,
            Func<Func<TValue, Either<TValue, TError>>[], Func<TValue, Either<TValue, TError>>> reduceValidations,
            params Func<TValue, Either<TValue, TError>>[] validations) =>
                reduceValidations(validations)(value);


        public static Either<TValue, Error<TError>> Check<TValue, TError>(this TValue value, Predicate<TValue> validate,
            TError validationError) =>
                    value
                        .Return<TValue, Error<TError>>()
                        .Check(validate, validationError);

        public static Either<TValue, Error<TError>> Check<TValue, TError>(this Either<TValue, Error<TError>> either,
            params Func<TValue, Either<TValue, Error<TError>>>[] validations) =>
                    either.Bind(value => Check(value, validations));

        public static Either<TValue, Error<TError>> Check<TValue, TError>(this Either<TValue, Error<TError>> either, Predicate<TValue> validation, Error<TError> validationError) =>
                either.Bind(value => Check(value, v =>
                    validation(v)
                        ? v.Return<TValue, Error<TError>>()
                        : validationError.ReturnAlternative<TValue, Error<TError>>()));

    }
}
