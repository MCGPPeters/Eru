using System;
using System.Data.SqlClient;
//using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Xunit.Assert;

// ReSharper disable EqualExpressionComparison

namespace Eru.Tests
{
    using static _;

    [SuppressMessage("ReSharper", "ConvertToConstant.Local",
        Justification = "Using a const integer as a divisor will cause a compile time error")]
    public class ExceptionHandlingProperties
    {
        private static void Fail() => True(false);
        private static void Succeed() => True(true);

        public enum Identifier
        {
            Exception
        }

        [Property(Verbose = true, DisplayName =
            "Dividing an integer by a non zero number returns the expected result")]
        public void Property1(PositiveInt dividend, PositiveInt divisor) =>
            dividend
                .Try(x => x.Get / divisor.Get)
                .Match(i => Equal(dividend.Get / divisor.Get, i), _ => Fail());

        [Property(Verbose = true, DisplayName =
            "Dividing an integer by zero return an exception")]
        public void Property2(int dividend) =>
            dividend
                .Try(x => x / 0)
                .Match(_ => Fail(), exception => IsType(typeof(DivideByZeroException), exception));

        [Property(Verbose = true, DisplayName =
            "Chaining of try blocks is possible for disparately typed return values")]
        public void Property3(PositiveInt dividend, PositiveInt divisor, PositiveInt secondDivisor) =>
            dividend
                .Try(x => x.Get / divisor.Get)
                .Try(x => x / secondDivisor.Get)
                .Match(i => Equal(dividend.Get / divisor.Get / secondDivisor.Get, i), _ => Fail());

        [Property(Verbose = true, DisplayName =
            "Retry should return the expected failure when the condition does not hold")]
        public void Property4(PositiveInt dividend)
            => dividend
                .Retry(x => x.Get / 0, TimeSpan.FromMilliseconds(20))
                .Match(
                    _ => Fail(),
                    exception => IsType(typeof(DivideByZeroException), exception));



        [Property(Verbose = true, DisplayName =
            "Retry only as long as the call fails")]
        public void Property5(PositiveInt numberOfCalls) =>
            numberOfCalls
                .Get
                .Retry(x => (x * 3) % 3 == 0
                    ? x
                    : throw new Exception())
                .Match(i => Succeed(), exception => Fail());

        [Fact(DisplayName =
            "Retry can run asynchrounously")]
        public void Property6() =>
            new SqlConnection()
                .OpenAsync()
                .Retry(TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(20))
                .Otherwise(exception => throw exception).Wait();
    }
}