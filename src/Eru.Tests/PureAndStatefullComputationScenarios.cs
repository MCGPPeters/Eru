using Eru.Control;
using Xunit;

namespace Eru.Tests
{
    public class PureAndStatefullComputationScenarios
    {
        [Fact]
        public void For_loop()
        {
            var foo = "foo";
            var @for = "bar".For(1, i => i < 3, i => i + 1, s => foo += s);

            Assert.Equal("foobarbarbar", @for.Item1);
            Assert.Equal(4, @for.Item2);
        }
    }
}