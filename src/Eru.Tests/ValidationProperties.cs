﻿using System;
using FsCheck.Xunit;
using Xunit;


namespace Eru.Tests
{
    using static Eru._;

    public class ValidationProperties
    {
        private static void Fail()
        {
            Assert.True(false);
        }


        [Fact(DisplayName =
            "Check will only aggregate failed validations")]
        public void Test3()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };

            person
                .Check(p => p.Age >= 0, "Must have a valid age")
                .Check(p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                .Match(success => {}, error =>
                {
                    Assert.Equal(Error("Must have a name"), error, new ErrorEqualityComparer());
                });
            ;
        }

        [Fact(DisplayName =
            "Check will aggregate all failed validations")]
        public void Test4()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };

            person
                .Check
                    (
                        (p => p.Age >= 18, "Must have a valid age"),
                        (p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                    )
                .Match(
                    _ => Fail(),
                    error =>
                    {
                        Assert.Equal(Error("Must have a valid age", "Must have a name"), error, new ErrorEqualityComparer());
                    });
            ;
        }

        [Fact(DisplayName =
            "CheckQuick will only return the first error")]
        public void Test5()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };


            person
                .CheckQuick
                (
                    (p => p.Age >= 18, "Must have a valid age"),
                    (p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                )
                .Match(_ => Fail(), error =>
                {
                    Assert.Equal(Error("Must have a valid age"), error, new ErrorEqualityComparer());
                });
            ;
        }

        [Fact(DisplayName =
            "Check will aggregate all failed validations")]
        public void Test6()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };


            person
                .Check(p => p.Age >= 18, "Must have a valid age")
                .Check(p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                .Match(_ => Fail(), error =>
                {
                    Assert.Equal(Error("Must have a valid age", "Must have a name"), error, new ErrorEqualityComparer());
                });
            ;
        }

        [Fact(DisplayName =
            "Check will aggregate all failed validations using LINQ")]
        public void Test7()
        {
            var person = new Person
            {
                Age = 11,
                Name = ""
            };

            var result = from x in person.Check(p => p.Age >= 18, "Must have a valid age")
                         from y in x.Check(p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                         select y;


            result.Match(_ => Fail(), error =>
            {
                Assert.Equal(Error("Must have a valid age", "Must have a name"), error, new ErrorEqualityComparer());
            });
            ;
        }

        [Property(DisplayName =
            "Check will execute valid computations using LINQ", Verbose = true)]
        public void Test8(int ageToAdd)
        {
            var person = new Person
            {
                Age = 19,
                Name = "John"
            };

            var result = from x in person.Check(p => p.Age >= 18, "Must have a valid age")
                         from y in x.Check(p => !string.IsNullOrWhiteSpace(p.Name), "Must have a name")
                         select y.Age + ageToAdd;


            result.Match(p =>
            {
                Assert.Equal(19 + ageToAdd, p);
            }, error => Fail());
            ;
        }
    }

    public class Person
    {
        public int Age { get; internal set; }
        public string Name { get; internal set; }
    }
}