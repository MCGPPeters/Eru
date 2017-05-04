using System;

namespace Eru
{
    using static _;

    public static class Argument
    {
        public static Either<T, ArgumentNullException> Assure<T>(this T argument) => argument == null
            ? throw new ArgumentNullException(nameof(argument))
            : argument.Return<T, ArgumentNullException>();
    }
}
