using System;
using FsCheck.Xunit;
using Xunit;

namespace Eru.Tests
{
    namespace Continuation.Monad
    {
        public class Properties
        {
            private readonly Func<int, int> _addOne = i => i + 1;
            private readonly Func<int, int> _addTwo = i => i + 2;

            [Property(Verbose = true)]
            public void Left_identity(int argument)
            {
                Func<int, Continuation<int, int>> function = i => (func => func(i + 3));
                var continuation = argument.AsContinuation<int, int>();

                var expected = continuation.Bind(function)(_addTwo);
                var actual = function(argument)(_addTwo);

                Assert.Equal(expected, actual);
            }

            [Property(Verbose = true)]
            public void Right_identity(int argument)
            {
                var continuation = argument.AsContinuation<int, int>();

                var expected = continuation.Bind(i => i.AsContinuation<int, int>())(_addTwo);
                var actual = continuation(_addTwo);

                Assert.Equal(expected, actual);
            }

            [Property(Verbose = true)]
            public void Associativity(int argument)
            {
                var continuation = argument.AsContinuation<int, int>();

                var expected =
                    continuation.Bind(x => x.AsContinuation<int, int>())
                        .Bind(y => y.AsContinuation<int, int>())(_addOne);
                var actual =
                    continuation
                        .Bind(x => x.AsContinuation<int, int>()
                            .Bind(y => y.AsContinuation<int, int>()))(_addOne);

                Assert.Equal(expected, actual);
            }

            [Property(Verbose = true)]
            public void Continuation_returns_input_if_predicate_does_not_hold(int value)
            {
                Predicate<int> predicate = _ => false;
                var expected = value;

                var actual = value.If(predicate)(_addOne);

                Assert.Equal(expected, actual);
            }

            [Property(Verbose = true)]
            public void Continuation_executes_if_predicate_holds(int value)
            {
                Predicate<int> predicate = _ => true;
                var expected = value + 1;

                var actual = value.If(predicate)(_addOne);

                Assert.Equal(expected, actual);
            }
        }
    }
}