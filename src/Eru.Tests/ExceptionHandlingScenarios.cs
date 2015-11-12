using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Eru.Control;
using Eru.ExceptionHandling;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
// ReSharper disable EqualExpressionComparison

namespace Eru.Tests
{
    [SuppressMessage("ReSharper", "ConvertToConstant.Local",
        Justification = "Using a const integer as a divisor will cause a compile time error")]
    public class ExceptionHandlingScenarios
    {

        [Fact]
        public void Dividing_an_integer_by_a_non_zero_number_returns_the_expected_result()
        {
            Prop.ForAll<int, int>((divisor, dividend) => 
                new Func<bool>(() =>
                {
                    var expectedResult = new Either<Failure<Requirements>, int>(dividend/divisor);

                    var actualResult = dividend
                        .Try(x => x/divisor, Requirements.ShouldNotThrowDevideByZeroException);

                    return actualResult.Equals(expectedResult);
                })
            .When(divisor != 0)
            .QuickCheckThrowOnFailure());
        }

        [Fact]
        public void Dividing_an_integer_by_zero_returns_an_exception()
        {
            Prop.ForAll<int, int>((divisor, dividend) =>
                new Func<bool>(() =>
                {
                    var expectedResult =
                        new Either<Failure<Requirements>, int>(
                            new FailureBecauseOfException<Requirements>(
                                Enumerable.Repeat(Requirements.ShouldNotThrowDevideByZeroException, 1),
                                new DivideByZeroException()));

                    var actualResult = dividend
                        .Try(x => x / divisor, Requirements.ShouldNotThrowDevideByZeroException);

                    return actualResult.Equals(expectedResult);
                })
            .When(divisor == 0)
            .QuickCheckThrowOnFailure());
        }

        [Fact]
        public void Chaining_of_try_blocks_is_possible_for_disparately_typed_return_values()
        {
            Prop.ForAll<int, int, int>((dividend, divisor, secondDivisor) => 
                new Func<bool>(() =>
                {
                    var expectedResult = new Either<Failure<Requirements>, int>(dividend/divisor/secondDivisor);

                    var actualResult = dividend
                        .Try(x => x / divisor, Requirements.ShouldNotThrowDevideByZeroException)
                        .Try(x => x / secondDivisor, Requirements.ShouldNotThrowDevideByZeroException);

                    return actualResult.Equals(expectedResult);
                })
            .When(divisor != 0)
            .QuickCheckThrowOnFailure());
        }

        [Fact]
        public void Retry_should_return_the_expected_failure_when_the_condition_does_not_hold()
        {
            Prop.ForAll<int, int>((retry, divisor) =>
                new Func<bool>(() =>
                {
                    var expectedResult =
                        new Either<Failure<Requirements>, int>(
                            new FailureBecauseOfException<Requirements>(
                                Enumerable.Repeat(Requirements.ShouldNotThrowDevideByZeroException, 1),
                                new DivideByZeroException()));

                    var retryCount = 0;
                    Func<bool> retryPolicy = () => retry != retryCount++;

                    var actualResult = 6
                        .Retry(x => x / divisor, Requirements.ShouldNotThrowDevideByZeroException)
                        .While(retryPolicy);

                    return actualResult.Equals(expectedResult) && retry == retryCount + 1;
                })
            .When(divisor != 0)
            .QuickCheckThrowOnFailure());
        }

        [Fact]
        public void Retry_should_retry_only_as_long_as_the_call_fails()
        {
            Prop.ForAll<int, int>((retry, dividend) =>
               new Func<bool>(() =>
               {
                   var retryCount = 1;
                   Func<bool> retryPolicy = () => retry != retryCount++;

                   var expectedResult = new Either<Failure<Requirements>, int>(dividend / retryCount);

                   var actualResult = dividend
                       .Retry(x => x / retryCount, Requirements.ShouldNotThrowDevideByZeroException)
                       .While(retryPolicy);

                   return actualResult.Equals(expectedResult) && retry == retryCount + 1;
               })
           .When(retry != 0)
           .QuickCheckThrowOnFailure());
        }

        [Fact]
        public async Task Retry_can_run_asynchronously()
        {
            var expectedResult = new Either<Failure<Requirements>, int>(6);
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await 6
                .Retry(x => x/i, Requirements.ShouldNotThrowDevideByZeroException)
                .WhileAsync(predicate);

            actualResult.Should().Be(expectedResult, "6 / 1 = 1 which is called at the second iteration");
            i.Should().Be(1, "The function call has to be retried 1 time before it doesn't throw any More");
        }

        [Fact]
        public async Task Trying_to_connect_to_a_non_existing_SQL_database_as_a_real_world_example_will_fail()
        {
            var expectedResult =
                new Either<Failure<Requirements>, SqlConnection>(
                    new FailureBecauseOfException<Requirements>(
                        Enumerable.Repeat(Requirements.ShouldNotThrowInvalidOperationException, 1),
                        new InvalidOperationException()));
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await new SqlConnection()
                .Retry(connection =>
                {
                    connection.Open();
                    return connection;
                }, Requirements.ShouldNotThrowInvalidOperationException)
                .WhileAsync(predicate);

            actualResult.Should()
                .Be(expectedResult, "Cannot open a connection without specifying a data source or server.");
        }
    }

    public enum Requirements
    {
        ShouldNotThrowDevideByZeroException,
        ShouldNotThrowInvalidOperationException
    }
}