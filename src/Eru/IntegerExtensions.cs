using System;
using System.Collections.Generic;
using System.Text;

namespace Eru
{
    public static partial class _
    {
        public static Either<int, Nothing> Parse<T>(this string s) =>
            int.TryParse(s, out int result)
                ? Return<int, Nothing>(result)
                : ReturnAlternative<int, Nothing>(Nothing);
    }
}
