using FsCheck.Xunit;
using Xunit;
using static Xunit.Assert;

namespace Eru.Tests
{
    using static _;

    public class ValidationProperties
    {
        private static void Fail() => True(false);

        [Property(
            DisplayName =
                "Check will execute valid computations using LINQ",
            Verbose = true)]
        public void Test8(int ageToAdd)
        {
            var person = new Person
            {
                Age = 19,
                Name = "John"
            };

            var result = from x in person.Check(p => p.Age >= 18, _ => "Must have a valid age")
                from y in x.Check(p => !string.IsNullOrWhiteSpace(p.Name), _ => "Must have a name")
                select y.Age + ageToAdd;


            result.Match(p => { Equal(19 + ageToAdd, p); }, error => Fail());
            ;
        }


        [Fact(
            DisplayName =
                "Check will only aggregate failed validations")]
        public void Test3()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };

            person
                .Check(p => p.Age >= 0, _ => "Must have a valid age")
                .Check(p => !string.IsNullOrWhiteSpace(p.Name), _ => "Must have a name")
                .Match(
                    success => { },
                    error => { Equal(Error("Must have a name"), error, new ErrorEqualityComparer()); });
            ;
        }

        [Fact(
            DisplayName =
                "Check will aggregate all failed validations")]
        public void Test4()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };

            person
                .Check(
                    (p => p.Age >= 18, p1 => "Must have a valid age"),
                    (p => !string.IsNullOrWhiteSpace(p.Name), p1 => "Must have a name")
                )
                .Match(
                    _ => Fail(),
                    error =>
                    {
                        Equal(
                            Error("Must have a valid age", "Must have a name"),
                            error,
                            new ErrorEqualityComparer());
                    });
            ;
        }

        [Fact(
            DisplayName =
                "CheckQuick will only return the first error")]
        public void Test5()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };


            person
                .CheckQuick(
                    (p => p.Age >= 18, "Must have a valid age"),
                    (p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                )
                .Match(
                    _ => Fail(),
                    error => { Equal(Error("Must have a valid age"), error, new ErrorEqualityComparer()); });
            ;
        }

        [Fact(
            DisplayName =
                "Check will aggregate all failed validations")]
        public void Test6()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };


            person
                .Check(
                    (p => p.Age >= 18, person1 => $"Must have a valid age. Actual age : {person1.Age}"),
                    (p => !string.IsNullOrWhiteSpace(p.Name), person1 => "Must have a name"))
                .Match(
                    _ => Fail(),
                    error =>
                    {
                        Equal(
                            Error("Must have a valid age. Actual age : 11", "Must have a name"),
                            error,
                            new ErrorEqualityComparer());
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
