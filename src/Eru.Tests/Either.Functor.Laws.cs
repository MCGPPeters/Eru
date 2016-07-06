using System;
using FluentAssertions;
using Xunit;

namespace Eru.Tests
{
    namespace Either.Functor
    {
        public class Laws
        {
            private static readonly Func<int, int, int> Add = (x, y) => x + y;
            private static readonly Func<int, int> AddOne = x => Add(x, 1);
            private static readonly Func<int, int> AddTwo = x => Add(x, 2);
            private static readonly Either<Exception, int> F = 1.Return<Exception, int>();

            private static Func<TSource, TResult> Compose<TSource, TIntermediate, TResult>(
                Func<TSource, TIntermediate> func1, Func<TIntermediate, TResult> func2)
            {
                return source => func2(func1(source));
            }

            [Fact]
            public void Left_identity()
            {
                var left = 1.Return<Exception, int>().Map(AddOne);
                var right = AddOne(1).Return<Exception, int>();

                left.ShouldBeEquivalentTo(right);
            }

            [Fact]
            public void Right_identity()
            {
                var left = F.Map(m => m);
                var right = F;

                left.ShouldBeEquivalentTo(right);
            }

            [Fact]
            public void Composable()
            {
                var resultFromChaining = F.Map(AddOne).Map(AddTwo);
                var resultFromComposing = F.Map(Compose(AddOne, AddTwo));

                resultFromChaining.ShouldBeEquivalentTo(resultFromComposing);
            }
        }
    }
}