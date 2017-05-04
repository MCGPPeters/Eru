using System;
using System.Collections.Generic;
using System.Text;

namespace Eru
{
    public static partial class _
    {
        public static IMonoid<string> AsMonoid(this string s) => s.AsMonoid(
            (argument, secondArgument) => argument + secondArgument);

    }
}
