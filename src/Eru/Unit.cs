using System;

namespace Eru
{
    public struct Unit : IEquatable<Unit>
    {
        public bool Equals(Unit other)
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is Unit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "{}";
        }


        /// <summary>
        /// Gets the single unit value.
        /// </summary>
        public static Unit Instance { get; } = new Unit();
    }
}
