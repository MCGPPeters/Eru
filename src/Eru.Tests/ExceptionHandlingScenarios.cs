using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Eru.Control;
using Eru.ErrorHandling;
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
        public enum Identifier
        {
            Exception
        }

        [Property(Verbose = true)]
        public void Dividing_an_integer_by_a_non_zero_number_returns_the_expected_result(int dividend,
            PositiveInt divisor)
        {
            dividend
                .Try(x => x/divisor.Item, Identifier.Exception)
                .ShouldBeEquivalentTo(new Either<Failure<Identifier>, int>(dividend/divisor.Item));
        }

        [Property(Verbose = true)]
        public void Dividing_an_integer_by_zero_returns_an_exception(int dividend)
        {
            dividend
                .Try(x => x/0, Identifier.Exception)
                .ShouldBeEquivalentTo(new Either<Failure<Identifier>, int>(
                    new Exception<Identifier>(Identifier.Exception, new DivideByZeroException())));
        }

        [Property(Verbose = true)]
        public void Chaining_of_try_blocks_is_possible_for_disparately_typed_return_values(int dividend,
            PositiveInt divisor, PositiveInt secondDivisor)
        {
            var expectedResult = new Either<Failure<Identifier>, int>(dividend/divisor.Item/secondDivisor.Item);
            dividend
                .Try(x => x/divisor.Item, Identifier.Exception)
                .Try(x => x/secondDivisor.Item, Identifier.Exception)
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Property(Verbose = true)]
        public void Retry_should_return_the_expected_failure_when_the_condition_does_not_hold(
            PositiveInt numberOfRetries, int dividend)
        {
            var expectedResult =
                new Either<Failure<Identifier>, int>(
                    new Exception<Identifier>(Identifier.Exception, new DivideByZeroException()));
            var retryCount = 0;

            dividend
                .Retry(x => x/0, Identifier.Exception)
                .While(() => numberOfRetries.Item != retryCount++)
                .ShouldBeEquivalentTo(expectedResult);
        }

        [Property(Verbose = true)]
        public void Retry_should_retry_only_as_long_as_the_call_fails(PositiveInt numberOfRetries)
        {
            var retryCount = 0;

            retryCount
                .Retry(_ =>
                {
                    if (numberOfRetries.Item != retryCount) throw new Exception();
                    return retryCount;
                }, Identifier.Exception)
                .While(() => numberOfRetries.Item != retryCount++)
                .Right.Should().Be(numberOfRetries.Item);
        }

        [Fact]
        public async Task Retry_can_run_asynchronously()
        {
            var expectedResult = new Either<Exception, int>(6);
            var i = 0;

            var actualResult = await 6
                .TryAsync(x => x/i++, 0, x => x < 1, x => x + 1);

            actualResult
                .ShouldBeEquivalentTo(expectedResult);
            actualResult.ShouldBeEquivalentTo(expectedResult, "6 / 1 = 1 which is called at the second iteration");
            i.ShouldBeEquivalentTo(2, "The function call has to be retried before it doesn't throw any More");
        }

        [Fact]
        public async Task Trying_to_connect_to_a_non_existing_SQL_database_as_a_real_world_example_will_fail()
        {
            var expectedResult = "The ConnectionString property has not been initialized.";

            var actualResult = await new SqlConnection()
                .TryAsync(connection =>
                {
                    connection.Open();
                    return connection;
                }, 0, i => i < 3, i => i + 1);

            actualResult
                .Left.Message.ShouldBeEquivalentTo(expectedResult);
        }
    }
}