using System.Linq;
using Eru.Validation;
using Xunit;

namespace Eru.Tests
{
    public class ValidationProperties
    {
        public ValidationProperties()
        {
            Properties = new[]
            {
                new Property<PropertyIdentifier, Person>(PropertyIdentifier.AgeMustBePositive, p => p.Age >= 0),
                new Property<PropertyIdentifier, Person>(PropertyIdentifier.NameIsRequired,
                    p => !string.IsNullOrEmpty(p.Name))
            };
        }

        public enum PropertyIdentifier
        {
            AgeMustBePositive,
            NameIsRequired
        }

        public Property<PropertyIdentifier, Person>[] Properties { get; }

        private static void Fail()
        {
            Assert.True(false);
        }

        [Fact(DisplayName =
            "When a subject that is invalid is validated, the correct broken Properties should be returned")]
        public void Test1()
        {
            var person = new Person
            {
                Age = -1,
                Name = ""
            };

            person.Check(Properties)
                .Match(
                    failure =>
                    {
                        var expectedPropertyIdentifiers =
                            Properties.Select(property => property.Identifier);

                        Assert.Equal(expectedPropertyIdentifiers, failure);
                        return Unit.Instance;
                    },
                    _ =>
                    {
                        Fail();
                        return Unit.Instance;
                    });
        }

        [Fact(DisplayName =
            "Properties can be expressed as plain predicates")]
        public void Test2()
        {
            "".Check("String must have a value",
                    x => !string.IsNullOrWhiteSpace(x),
                    x => x.Equals("hasValue"))
                .Match(
                    actualPropertyIdentifiers =>
                    {
                        const string expectedPropertyIdentifier = "String must have a value";
                        Assert.Contains(expectedPropertyIdentifier, actualPropertyIdentifiers);
                        return Unit.Instance;
                    },
                    _ =>
                    {
                        Fail();
                        return Unit.Instance;
                    });
        }

        [Fact(DisplayName =
            "Properties can be chained")]
        public void Test3()
        {
            var person = new Person
            {
                Age = -1,
                Name = ""
            };

            person
                .Check("Has a valid age", p => p.Age >= 0)
                .Check("Has a valid name", p => string.IsNullOrWhiteSpace(p.Name))
                .Match(
                    actualPropertyIdentifiers =>
                    {
                        const string expectedPropertyIdentifier = "Has a valid age";
                        Assert.Contains(expectedPropertyIdentifier, actualPropertyIdentifiers);
                        return Unit.Instance;
                    },
                    _ =>
                    {
                        Fail();
                        return Unit.Instance;
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