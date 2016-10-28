using System;
using FluentAssertions;
using FsCheck.Xunit;

namespace Eru.Tests
{
    namespace Either.Functor
    {
        public class Properties
        {
            private static readonly Func<int, int, int> Add = (x, y) => x + y;
            private static readonly Func<int, int> AddOne = x => Add(x, 1);
            private static readonly Func<int, int> AddTwo = x => Add(x, 2);
            private static readonly Either<Exception, int> F = 1.Return<Exception, int>();

            private static Func<TSource, TResult> Compose<TSource, TIntermediate, TResult>(
                Func<TSource, TIntermediate> func1, Func<TIntermediate, TResult> func2)
            {
                return source => func2(func1(source));
            }

            [Property(Verbose = true)]
            public void Left_identity(int arg)
            {
                var left = arg.Return<Exception, int>().Map(AddOne);
                var right = AddOne(arg).Return<Exception, int>();

                left.ShouldBeEquivalentTo(right);
            }

            [Property(Verbose = true)]
            public void Right_identity(int arg)
            {
                var left = arg.Return<Exception, int>().Map(m => m);
                var right = arg.Return<Exception, int>();

                left.ShouldBeEquivalentTo(right);
            }

            [Property(Verbose = true)]
            public void Composable(int arg)
            {
                var resultFromChaining = arg.Return<Exception, int>().Map(AddOne).Map(AddTwo);
                var resultFromComposing = arg.Return<Exception, int>().Map(Compose(AddOne, AddTwo));

                resultFromChaining.ShouldBeEquivalentTo(resultFromComposing);
            }
        }
    }
}