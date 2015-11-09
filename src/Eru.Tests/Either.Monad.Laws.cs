using System;
using FluentAssertions;
using Xunit;

namespace Eru.Tests
{
    namespace Either.Monad
    {
        public class Laws
        {
            private static readonly Func<int, int, Either<Exception, int>> Add =
                (x, y) => (x + y).AsEither<Exception, int>();

            private static readonly Func<int, Either<Exception, int>> AddOne = x => Add(x, 1);
            private static readonly Func<int, Either<Exception, int>> AddTwo = x => Add(x, 2);
            private static readonly Either<Exception, int> M = 1.AsEither<Exception, int>();

            [Fact]
            public void Left_identity()
            {
                var left = 1.AsEither<Exception, int>().Bind(AddOne);
                var right = AddOne(1);

                left.Should().Be(right);
            }

            [Fact]
            public void Right_identity()
            {
                var left = M.Bind(m => m.AsEither<Exception, int>());
                var right = M;

                left.Should().Be(right);
            }

            [Fact]
            public void Associativity()
            {
                var left = M.Bind(AddOne).Bind(AddTwo);
                var right = M.Bind(x => AddOne(x).Bind(AddTwo));

                left.Should().Be(right);
            }
        }
    }
}