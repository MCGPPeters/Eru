using System;
using System.Threading.Tasks;

namespace Eru.Control
{
    public static class Extensions
    {
        public static Either<TLeft, TRight> While<TLeft, TRight>(
            this Either<Func<Func<bool>, Either<TLeft, TRight>>, TRight> source,
            Func<bool> predicate)
        {
            return When(source, predicate);
        }

        private static Either<TLeft, TRight> When<TLeft, TRight>(
            Either<Func<Func<bool>, Either<TLeft, TRight>>, TRight> source, Func<bool> predicate)
        {
            return source
                .Match(
                    func => source.Left(predicate),
                    right => new Either<TLeft, TRight>(source.Right));
        }

        public static async Task<Either<TLeft, TRight>> WhileAsync<TLeft, TRight>(
            this Either<Func<Func<bool>, Either<TLeft, TRight>>, TRight> source,
            Func<bool> predicate)
        {
            return await WhenAsync(source, predicate);
        }

        private static async Task<Either<TLeft, TRight>> WhenAsync<TLeft, TRight>(
            Either<Func<Func<bool>, Either<TLeft, TRight>>, TRight> source, Func<bool> predicate)
        {
            return await source.Match(
                func => Task.Run(() => source.Left(predicate)),
                right => Task.FromResult(new Either<TLeft, TRight>(source.Right)));
        }
    }
}