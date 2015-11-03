//using System;
//using Eru.ExceptionHandling;
//using FluentAssertions;

//namespace Eru.Tests
//{
//    public class ControlFlowScenarios
//    {
//        public void Retry_should_retry_only_when_the_call_fails()
//        {
//            var expectedResult = new Either<Exception, int>(6);
//            var i = 0;
//            Predicate<int> predicate = i1 => 3 != i++;

//            var actualResult = 6
//                .When(new Condition<Conditions,int>(Conditions.OnlyRetryOnce, predicate)
//                .Do(condition => x => x/i)
//            );
                
//            actualResult.Should().Be(expectedResult, "6 / 1 = 1 which is called at the second iteration");
//            i.Should().Be(1, "The function call has to be retried 1 time before it doesn't throw any More");
//        }
//    }
//}