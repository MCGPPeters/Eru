using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Eru.ExceptionHandling;
using FluentAssertions;
using Xunit;

namespace Eru.Tests
{
    [SuppressMessage("ReSharper", "ConvertToConstant.Local", Justification = "Using a const integer as a divisor will cause a compile time error")]
    public class ExceptionHandlingScenarios
    {
        [Theory]
        [InlineData(6, 2, 3)]
        [InlineData(8, 2, 4)]
        public void Dividing_an_integer_by_a_non_zero_number_returns_the_expected_result(int dividend, int divisor,
            int result)
        {
            var expectedResult = new Either<Exception, int>(result);

            var actualResult = dividend.Try(x => x/divisor);

            actualResult.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(13)]
        public void Dividing_an_integer_by_zero_returns_an_exception(int dividend)
        {
            var expectedResult = new Either<Exception, int>(new DivideByZeroException());
            int divisor = 0;

            var actualResult = dividend.Try(x => x/divisor);

            actualResult.Should().Be(expectedResult, "Integers may not be divided by zero");
        }

        [Theory]
        [InlineData(6, 2, 3.0, 1.0)]
        [InlineData(16, 2, 4.0, 2.0)]
        public void Chaining_of_try_blocks_is_possible_for_disparately_typed_return_values(int dividend, int divisor, double secondDivisor,
            double result)
        {
            var expectedResult = new Either<Exception, double>(result);

            var actualResult = dividend
                .Try(x => x/divisor)
                .Try(x => x/secondDivisor);

            actualResult.Should().Be(expectedResult);
        }

        [Fact]
        public void Retry_should_retry_calling_a_function_the_specified_number_of_times()
        {
            var expectedResult = new Either<Exception, int>(new DivideByZeroException());
            var i = 0;
            var divisor = 0;
            Func<bool> predicate = () => 3 != i++;
            
            var actualResult = 6
                .Retry(x => x/divisor)
                .While(predicate);
                
            actualResult.Should().Be(expectedResult, "Division by zero is not allowed");
            i.Should().Be(4, "The function call should be retried 3 times");
        }

        [Fact]
        public void Retry_should_retry_only_as_long_as_the_call_fails()
        {
            var expectedResult = new Either<Exception, int>(6);
            var i = 0;
            Func<bool> predicate = () => 3 != i++;
            
            var actualResult = 6
                .Retry(x => x/i)
                .While(predicate);
                
            actualResult.Should().Be(expectedResult, "6 / 1 = 1 which is called at the second iteration");
            i.Should().Be(1, "The function call has to be retried 1 time before it doesn't throw any more");
        }

        [Fact]
        public async Task Retry_can_run_asynchronously()
        {
            var expectedResult = new Either<Exception, int>(6);
            var i = 0;
            Func<bool> predicate = () => 3 != i++;
            
            var actualResult = await 6
                .Retry(x => x/i)
                .WhileAsync(predicate);
                
            actualResult.Should().Be(expectedResult, "6 / 1 = 1 which is called at the second iteration");
            i.Should().Be(1, "The function call has to be retried 1 time before it doesn't throw any more");
        }

        [Fact]
        public async Task Trying_to_connect_to_a_non_existing_SQL_database_as_a_real_world_example_will_fail()
        {
            var expectedResult = new Either<Exception, SqlConnection>(new InvalidOperationException());
            var i = 0;
            Func<bool> predicate = () => 3 != i++;

            var actualResult = await new SqlConnection()
                .Retry(connection =>
                {
                    connection.Open();
                    return connection;
                })
                .WhileAsync(predicate);

            actualResult.Should().Be(expectedResult, "Cannot open a connection without specifying a data source or server.");
        }
    }
}