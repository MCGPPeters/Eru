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
        public enum Exception
        {
            DivisionByZero,
            InvalidOperation
        }

        public ExceptionHandlingScenarios()
        {
            Exceptions = new[]
            {
                new Exception<Exception>(Exception.DivisionByZero, new DivideByZeroException()),
                new Exception<Exception>(Exception.InvalidOperation, new InvalidOperationException())
            };
        }

        public Exception<Exception>[] Exceptions { get; set; }

        [Fact]
        public void Dividing_an_integer_by_a_non_zero_number_returns_the_expected_result()
        {
            Prop.ForAll<int, int>((divisor, dividend) =>
                new Func<bool>(() =>
                {
                    var expectedResult = new Either<Failure<Exception>, int>(dividend/divisor);

                    var actualResult = dividend
                        .Try(x => x/divisor, Exception.DivisionByZero);

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
                        new Either<Failure<Exception>, int>(
                            new Failure<Exception>(new[] {Exception.DivisionByZero}));

                    var actualResult = dividend
                        .Try(x => x/divisor, handleThisException: Exception.DivisionByZero);

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
                    var expectedResult = new Either<Failure<Exception>, int>(dividend/divisor/secondDivisor);

                    var actualResult = dividend
                        .Try(x => x/divisor, handleThisException: Exception.DivisionByZero)
                        .Try(x => x/secondDivisor, handleThisException: Exception.DivisionByZero);

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
                        new Either<Failure<Exception>, int>(
                            new Failure<Exception>(new[] {Exception.DivisionByZero}));

                    var retryCount = 0;
                    Func<bool> retryPolicy = () => retry != retryCount++;

                    var actualResult = 6
                        .Retry(x => x/divisor, Exception.DivisionByZero)
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

                    var expectedResult = new Either<Failure<Exception>, int>(dividend/retryCount);

                    var actualResult = dividend
                        .Retry(x => x/retryCount, Exception.DivisionByZero)
                        .While(retryPolicy);

                    return actualResult.Equals(expectedResult) && retry == retryCount + 1;
                })
                    .When(retry != 0)
                    .QuickCheckThrowOnFailure());
        }

        [Fact]
        public async Task Retry_can_run_asynchronously()
        {
            var expectedResult = new Either<Failure<Exception>, int>(6);
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await 6
                .Retry(x => x/i, Exception.DivisionByZero)
                .WhileAsync(predicate);

            actualResult.Should().Be(expectedResult, "6 / 1 = 1 which is called at the second iteration");
            i.Should().Be(1, "The function call has to be retried 1 time before it doesn't throw any More");
        }

        [Fact]
        public async Task Trying_to_connect_to_a_non_existing_SQL_database_as_a_real_world_example_will_fail()
        {
            var expectedResult =
                new Either<Failure<Exception>, SqlConnection>(
                    new Failure<Exception>(Exception.InvalidOperation));
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await new SqlConnection()
                .Retry(connection =>
                {
                    connection.Open();
                    return connection;
                }, Exception.InvalidOperation)
                .WhileAsync(predicate);

            actualResult.Should()
                .Be(expectedResult, "Cannot open a connection without specifying a data source or server.");
        }
    }
}