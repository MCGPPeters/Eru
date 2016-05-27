using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Eru.Control;
using Eru.ErrorHandling;
using FluentAssertions;
using FsCheck;
using Xunit;

// ReSharper disable EqualExpressionComparison

namespace Eru.Tests
{
    [SuppressMessage("ReSharper", "ConvertToConstant.Local",
        Justification = "Using a const integer as a divisor will cause a compile time error")]
    public class ExceptionHandlingScenarios
    {
        public enum Identifier
        {
            Exception
        }

        [Fact]
        public void Dividing_an_integer_by_a_non_zero_number_returns_the_expected_result()
        {
            Prop.ForAll(Arb.From(Gen.Choose(1, int.MaxValue)), Arb.From(Gen.Choose(1, int.MaxValue)),
                (dividend, divisor) => dividend
                    .Try(x => x/divisor, Identifier.Exception)
                    .Equals(new Either<Failure<Identifier>, int>(dividend/divisor)))
                .VerboseCheckThrowOnFailure();
        }

        [Fact]
        public void Dividing_an_integer_by_zero_returns_an_exception()
        {
            Prop.ForAll(Arb.From(Gen.Choose(0, int.MaxValue)), dividend => dividend
                .Try(x => x/0, Identifier.Exception)
                .Equals(new Either<Failure<Identifier>, int>(
                    new Exception<Identifier>(Identifier.Exception, new DivideByZeroException()))))
                .VerboseCheckThrowOnFailure();
        }

        [Fact]
        public void Chaining_of_try_blocks_is_possible_for_disparately_typed_return_values()
        {
            Prop.ForAll(Arb.From(Gen.Choose(0, int.MaxValue)), Arb.From(Gen.Choose(1, int.MaxValue)),
                Arb.From(Gen.Choose(1, int.MaxValue)), (dividend, divisor, secondDivisor) =>
                {
                    var expectedResult = new Either<Failure<Identifier>, int>(dividend/divisor/secondDivisor);
                    return dividend
                        .Try(x => x/divisor, Identifier.Exception)
                        .Try(x => x/secondDivisor, Identifier.Exception)
                        .Equals(expectedResult);
                })
                .VerboseCheckThrowOnFailure();
        }

        [Fact]
        public void Retry_should_return_the_expected_failure_when_the_condition_does_not_hold()
        {
            var expectedResult =
                new Either<Failure<Identifier>, int>(
                    new Exception<Identifier>(Identifier.Exception, new DivideByZeroException()));
            var arbitraryNumberOfRetries = Arb.From(Gen.Choose(1, 50));
            var arbitraryDividend = Arb.From(Gen.Choose(1, 50));
            Prop.ForAll(arbitraryNumberOfRetries, arbitraryDividend,
                (retry, dividend) =>
                {
                    var retryCount = 0;
                    return dividend
                        .Retry(x => x/0, Identifier.Exception)
                        .While(() => retry != retryCount++)
                        .Equals(expectedResult);
                })
                .VerboseCheckThrowOnFailure();
        }

        [Fact]
        public void Retry_should_retry_only_as_long_as_the_call_fails()
        {
            var arbitraryNumberOfRetries = Arb.From(Gen.Choose(1, 50));
            var arbitraryDividend = Arb.From(Gen.Choose(1, 50));
            Prop.ForAll(arbitraryNumberOfRetries, arbitraryDividend, (retry, dividend) =>
            {
                var retryCount = 0;
                var expectedResult = new Either<Failure<Identifier>, int>(0);
                if (retryCount != 0)
                    expectedResult = new Either<Failure<Identifier>, int>(dividend/retryCount);

                return dividend
                    .Retry(x => x/retryCount, Identifier.Exception)
                    .While(() => retry != retryCount++)
                    .Equals(expectedResult);
            })
                .VerboseCheckThrowOnFailure();
        }

        //[Fact]
        //public void Retry_policies_can_be_statefull_without_using_any_additional_variables_from_the_outer_scope()
        //{
        //    var arbitraryNumberOfRetries = Arb.From(Gen.Choose(1, 50));
        //    var arbitraryDividend = Arb.From(Gen.Choose(1, 50));

        //    Prop.ForAll(arbitraryNumberOfRetries, arbitraryDividend, (retry, dividend) =>
        //    {
        //        var retryCount = 0;
        //        var expectedResult = new Either<Failure<Identifier>, int>(0);
        //        if(retryCount != 0)
        //            expectedResult = new Either<Failure<Identifier>, int>(dividend/retryCount);

        //        return dividend
        //            .Retry(x => x / retryCount, exceptionIdentifier: Identifier.Exception)
        //            .While(() => retry != retryCount++)
        //            .Equals(expectedResult);
        //    })
        //        .VerboseCheckThrowOnFailure();
        //}

        [Fact]
        public async Task Retry_can_run_asynchronously()
        {
            var expectedResult = new Either<Failure<Identifier>, int>(6);
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await 6
                .Retry(x => x/i, Identifier.Exception)
                .WhileAsync(predicate);

            actualResult.Should().Be(expectedResult, "6 / 1 = 1 which is called at the second iteration");
            i.Should().Be(1, "The function call has to be retried 1 time before it doesn't throw any More");
        }

        [Fact]
        public async Task Trying_to_connect_to_a_non_existing_SQL_database_as_a_real_world_example_will_fail()
        {
            var expectedResult =
                new Either<Failure<Identifier>, SqlConnection>(
                    new Failure<Identifier>(Identifier.Exception));
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await new SqlConnection()
                .Retry(connection =>
                {
                    connection.Open();
                    return connection;
                }, Identifier.Exception)
                .WhileAsync(predicate);

            actualResult
                .Should()
                .Be(expectedResult, "Cannot open a connection without specifying a data source or server.");
        }
    }
}