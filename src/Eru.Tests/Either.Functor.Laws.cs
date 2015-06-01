using System;
using FluentAssertions;
using Xunit;

namespace Eru.Tests
{
    namespace Either.Functor
    { 
        public class Laws
        {
            private static readonly Func<int, int, int> Add = (x, y) => (x + y);
            private static readonly Func<int, int> AddOne = x => Add(x, 1);
            private static readonly Func<int, int> AddTwo = x => Add(x, 2);
            private static readonly Either<Exception, int> F = 1.AsEither<Exception, int>();

            private static Func<TSource, TResult> Compose<TSource, TIntermediate, TResult>(Func<TSource, TIntermediate> func1, Func<TIntermediate, TResult> func2)
            {
                return source => func2(func1(source));
            }

            [Fact]
            public void Left_identity()
            {
                Either<Exception, int> left = 1.AsEither<Exception, int>().Map(AddOne);
                Either<Exception, int> right = AddOne(1).AsEither<Exception, int>();

                left.Should().Be(right);
            }

            [Fact]
            public void Right_identity()
            {
                Either<Exception, int> left = F.Map(m => m);
                Either<Exception, int> right = F;

                left.Should().Be(right);
            }

            [Fact]
            public void Composable()
            {
                Either<Exception, int> resultFromChaining = F.Map(AddOne).Map(AddTwo);
                Either<Exception, int> resultFromComposing = F.Map(Compose(AddOne, AddTwo));

                resultFromChaining.Should().Be(resultFromComposing);
            }
        }
    }
}