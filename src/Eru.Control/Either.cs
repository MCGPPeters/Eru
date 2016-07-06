using System;
using System.Threading.Tasks;

namespace Eru.Control
{
    public static class Either
    {
        public static Either<TLeft, TRight> While<TLeft, TRight>(
            this Either<Func<Func<bool>, Either<TLeft, TRight>>, TRight> source,
            Func<bool> predicate)
        {
            return Where(source, predicate);
        }

        public static Either<TLeft, TRight> Where<TLeft, TRight>(
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
            return await WhereAsync(source, predicate);
        }

        public static async Task<Either<TLeft, TRight>> WhereAsync<TLeft, TRight>(
            Either<Func<Func<bool>, Either<TLeft, TRight>>, TRight> source, Func<bool> predicate)
        {
            return await source.Match(
                func => Task.Run(() => source.Left(predicate)),
                right => Task.FromResult(new Either<TLeft, TRight>(source.Right)));
        }
    }
}