using System.Linq;
using Eru.ErrorHandling;
using Xunit;

namespace Eru.Tests
{
    public class ValidationScenarios
    {
        public enum Constraint
        {
            AgeMustBePositive,
            NameIsRequired
        }

        public ValidationScenarios()
        {
            Assertions = new[]
            {
                new Assertion<Constraint, Person>(Constraint.AgeMustBePositive, p => p.Age >= 0),
                new Assertion<Constraint, Person>(Constraint.NameIsRequired, p => !string.IsNullOrEmpty(p.Name))
            };
        }

        public Assertion<Constraint, Person>[] Assertions { get; private set; }

        [Fact]
        public void Validating_a_subject_that_is_invalid_returns_the_broken_rules()
        {
            var person = new Person { Age = -1, Name = "" };

            person
                .Assert(Assertions)
                .Match(
                    failure =>
                    {
                        var expectedFailure =
                            new Failure<Constraint>(Assertions.Select(assertion => assertion.Identifier));
                        Assert.True(
                            failure.Equals(expectedFailure));
                        return Unit.Instance;
                    },
                    _ =>
                    {
                        Fail();
                        return Unit.Instance;
                    });
        }

        [Fact]
        public void Simple_string_validation()
        {
            "".Assert("String must have a value",
                    x => !string.IsNullOrWhiteSpace(x),
                    x => x.Equals("hasValue"))
                    .Match(
                        failure =>
                        {
                            var expectedFailure =
                                new Failure<string>("String must have a value");
                            Assert.True(
                                failure.Equals(expectedFailure));
                            return Unit.Instance;
                        },
                        _ =>
                        {
                            Fail();
                            return Unit.Instance;
                        });
        }

        private static void Fail()
        {
            Assert.False(true);
        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}