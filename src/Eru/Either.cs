using System;

namespace Eru
{
    public class Either<TLeft, TRight>
    {
        private TLeft _left;
        private TRight _right;

        public Either(TRight right)
        {
            Right = right;
        }

        public Either(TLeft left)
        {
            Left = left;
        }

        public bool LeftHasValue { get; set; }
        public bool RightHasValue { get; set; }

        public TLeft Left
        {
            get { return _left; }
            private set
            {
                _left = value;
                LeftHasValue = true;
            }
        }

        public TRight Right
        {
            get { return _right; }
            private set
            {
                _right = value;
                RightHasValue = true;
            }
        }
    }

    public static class Either
    {
        public static Either<TLeft, TRight> Return<TLeft, TRight>(this TRight value)
            => new Either<TLeft, TRight>(value);

        public static Either<TLeft, TRight> AsEither<TLeft, TRight>(this TRight value)
            => Return<TLeft, TRight>(value);

        public static Either<TLeft, TRight> Fail<TLeft, TRight>(this TLeft value)
            => new Either<TLeft, TRight>(value);

        public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
            Func<TRight, Either<TLeft, TResult>> function)
            => Match(either, left => left.Fail<TLeft, TResult>(), function);

        public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
            Func<TRight, TResult> function)
            => Match(either,
                left => left.Fail<TLeft, TResult>(),
                right => function(right).Return<TLeft, TResult>());

        public static Either<TLeft, TResult> Select<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
            Func<TRight, TResult> function)
            => Map(either, function);

        public static TResult Match<TLeft, TRight, TResult>(this Either<TLeft, TRight> either, Func<TLeft, TResult> left,
            Func<TRight, TResult> right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));
            if (left == null) throw new ArgumentNullException(nameof(left));
            return either.LeftHasValue
                ? left(either.Left)
                : right(either.Right);
        }

        public static void Match<TLeft, TRight>(this Either<TLeft, TRight> either, Action<TLeft> left)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            left(either.Left);
        }
    }
}