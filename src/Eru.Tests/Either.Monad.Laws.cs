using System;
using Xunit;

namespace Eru.Tests
{
    namespace Either.Monad
    {
        public class Laws
        {
            [Fact]
            private void Left_identity()
            {
                //Func<int, Either<int, bool>> k = i => new Either<int, bool>(2);
                //var bla = 1.AsEither<int, int>().Bind(k);
                //var foo = (1 == bla(1));
            }

            [Fact]
            private void Right_identity()
            {
                //nd(m, Unit) = m;
            }

            [Fact]
            private void Associativity()
            {
                //Bind(m, x => Bind(k(x), y => h(y)) = Bind(Bind(m, x => k(x)), y => h(y));//
            }
        }
    }
}