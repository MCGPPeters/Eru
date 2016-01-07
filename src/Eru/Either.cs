using System;
using System.Collections.Generic;

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

        public bool LeftHasValue { get; private set; }
        public bool RightHasValue { get; private set; }

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

        private bool Equals(Either<TLeft, TRight> other)
        {
            return EqualityComparer<TLeft>.Default.Equals(_left, other._left) ||
                   EqualityComparer<TRight>.Default.Equals(_right, other._right) && LeftHasValue == other.LeftHasValue ||
                   RightHasValue == other.RightHasValue;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<TLeft>.Default.GetHashCode(_left);
                hashCode = (hashCode*397) ^ EqualityComparer<TRight>.Default.GetHashCode(_right);
                hashCode = (hashCode*397) ^ LeftHasValue.GetHashCode();
                hashCode = (hashCode*397) ^ RightHasValue.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals((Either<TLeft, TRight>) obj);
        }
    }

    public static class Either
    {
        public static Either<TLeft, TRight> Return<TLeft, TRight>(this TRight value)
        {
            return new Either<TLeft, TRight>(value);
        }

        public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
            Func<TRight, Either<TLeft, TResult>> function)
        {
            return Match(either,
                left => new Either<TLeft, TResult>(left),
                function);
        }

        public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(this Either<TLeft, TRight> either,
            Func<TRight, TResult> function)
        {
            return Match(either,
                left => new Either<TLeft, TResult>(left),
                right => new Either<TLeft, TResult>(function(right)));
        }

        public static TResult Match<TLeft, TRight, TResult>(this Either<TLeft, TRight> either, Func<TLeft, TResult> left,
            Func<TRight, TResult> right)
        {
            if (right == null) throw new ArgumentNullException("right");
            if (left == null) throw new ArgumentNullException("left");
            return either.LeftHasValue
                ? left(either.Left)
                : right(either.Right);
        }
    }
}