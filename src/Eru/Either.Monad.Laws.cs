namespace Eru.Tests
{
    namespace Either.Monad
    {
        public class Laws
        {
            [Fact]
            private void Left_identity()
            {
                //Eru.Either.Bind(1.AsEither(), k) = k(e);
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