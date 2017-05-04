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
        public Error(string message) : base(message)
        {
        }
    }

    public class Error<T>
    {
        public Error(T message) => Message = message;

        public T Message { get; }


        public static implicit operator Error<T>(T message)
            => new Error<T>(message);
        public static implicit operator T(Error<T> error)
            => error.Message;
    }

    public static class ValidationExtensions
    {

        public static Either<TValue, TError> Check<TValue, TError>(this TValue value,
            params Func<TValue, Either<TValue, TError>>[] validations)
                where TError : Error<TError> =>
                    validations
                        .Aggregate(value.Return<TValue, TError>(), (current, validation) =>
                            current.Bind(_ => validation(value)));

        public static Either<TValue, TError> Check<TValue, TError>(this TValue value, Predicate<TValue> validate, TError validationError)
            where TError : Error<TError> =>
                validate(value)
                    ? value.Return<TValue, TError>()
                    : validationError.ReturnAlternative<TValue, TError>();

        public static Either<TValue, TError> Check<TValue, TError>(this Either<TValue, TError> either,
            params Func<TValue, Either<TValue, TError>>[] validations) where TError : Error<TError> =>
                either.Bind(value => Check(value, validations));

        public static Either<TValue, TError> Check<TValue, TError>(this Either<TValue, TError> either, Predicate<TValue> validate, TError validationError)
            where TError : Error<TError> =>
                either.Bind(value => Check(value, v =>
                    validate(v)
                        ? v.Return<TValue, TError>()
                        : validationError.ReturnAlternative<TValue, TError>()));

        public static Either<TValue, Error> Check<TValue>(this TValue value,
            Func<TValue, Either<TValue, Error>> validate) =>
                value.Check(validate);

        public static Either<TValue, Error> Check<TValue>(this TValue value, Predicate<TValue> validate, string validationError) =>
            value.Check(validate, validationError);

        public static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either,
            Func<TValue, Either<TValue, Error>> validate) =>
                either.Check(validate);

        public static Either<TValue, Error> Check<TValue>(this Either<TValue, Error> either, Predicate<TValue> validate, string validationError) =>
            either.Check(validate, validationError);

    }
}
