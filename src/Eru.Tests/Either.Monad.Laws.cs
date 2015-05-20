using System;
using FluentAssertions;
using Xunit;

namespace Eru.Tests
{
    namespace Either.Monad
    {
        public class Laws
        {
            private Func<int, Either<Exception, int>> addOne = x => (x + 1).AsEither<Exception, int>();
            private Func<int, Either<Exception, int>> addTwo = x => (x + 2).AsEither<Exception, int>();
            private Either<Exception, int> M = 1.AsEither<Exception, int>();

            [Fact]
            private void Left_identity()
            {
                Either<Exception, int> left = 1.AsEither<Exception, int>().Bind(addOne);
                Either<Exception, int> right = addOne(1);

                left.Should().Be(right);
            }

            [Fact]
            private void Right_identity()
            {
                var left = M.Bind(m => m.AsEither<Exception, int>());
                var right = M;

                left.Should().Be(right);
            }

            [Fact]
            private void Associativity()
            {
                var left = M.Bind(addOne).Bind(addTwo);
                var right = M.Bind(x => addOne(x).Bind(addTwo));

                left.Should().Be(right);
            }
        }
    }
}