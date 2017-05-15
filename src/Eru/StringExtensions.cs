namespace Eru
{
    public static partial class _
    {
        public static Either<int, Nothing> Parse(this string s) =>
            int.TryParse(s, out int result)
                ? result.AsEither<int, Nothing>()
                : Nothing.AsEither<int, Nothing>();
    }
}