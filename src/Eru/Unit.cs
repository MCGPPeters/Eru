﻿using System;

namespace Eru
{
    public struct Unit : IComparable<Unit>, IEquatable<Unit>
    {
        public int CompareTo(Unit other)
        {
            return 0;
        }

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
    }
}
