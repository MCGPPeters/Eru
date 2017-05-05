using Xunit;

namespace Eru.Tests
{
    using static _;

    using Eru.Validation;
    using Eru;

    public class ValidationProperties
    {
        private static Unit Fail()
        {
            Assert.True(false);
            return Unit;
        }


        [Fact(DisplayName =
            "Validation can be chained")]
        public void Test3()
        {
            var person = new Person
            {
                Age = -1,
                Name = ""
            };

            person
                .Check(p => p.Age >= 0, "Must have a valid age")
                .Check(p => string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                .Match(_ => Fail(), error =>
                {
                    Assert.Equal("Must have a name", error);
                    return Unit;
                });
            ;
        }
    }

    public class Person
    {
        public int Age { get; internal set; }
        public string Name { get; internal set; }
    }
}