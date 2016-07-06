using Eru.Control;
using Xunit;

namespace Eru.Tests
{
    public class PureAndStatefullComputationScenarios
    {
        [Fact]
        public void For_loop()
        {
            var bar = "bar";
            var foobar = "foo";
            var @for = "".For(1, i => i < 3, i => i + 1, _ => foobar = foobar + bar);

            Assert.Equal("foobarbarbar", foobar);
            Assert.Equal(4, @for.Item2);
        }

        //[Fact]
        //public void For_loop_linq()
        //{
        //    var bar = "bar";
        //    var foobar = "foo";
        //    var @for = "".ForLinq(1, i => i < 3, i => i + 1, _ => foobar = foobar + bar);

        //    Assert.Equal("foobarbarbar", foobar);
        //    Assert.Equal(4, @for.Item2);
        //}
    }
}

