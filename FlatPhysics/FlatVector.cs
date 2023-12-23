using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using FlatTransform = FlatPhysics.FlatTransform;

namespace FlatPhysics
{
    public readonly struct FlatVector
    {
        public readonly float X;
        public readonly float Y;

        public static readonly FlatVector Zero = new FlatVector(0f, 0f);

        public FlatVector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        internal static FlatVector Transform(FlatVector v, FlatTransform transform)
        {
            return new FlatVector(transform.Cos * v.X - transform.Sin * v.Y + transform.PositionX,
                transform.Sin * v.X + transform.Cos * v.Y + transform.PositionY);
        }

        public static FlatVector operator +(FlatVector lhs, FlatVector rhs)
        {
            return new FlatVector(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static FlatVector operator -(FlatVector lhs, FlatVector rhs)
        {
            return new FlatVector(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static FlatVector operator -(FlatVector v)
        {
            return new FlatVector(-v.X, -v.Y);
        }

        public static FlatVector operator *(FlatVector v, float s)
        {
            return new FlatVector(v.X * s, v.Y * s);
        }

        public static FlatVector operator *(float s, FlatVector v)
        {
            return new FlatVector(v.X * s, v.Y * s);
        }

        public static FlatVector operator /(FlatVector v, float s)
        {
            return new FlatVector(v.X / s, v.Y / s);
        }

        public bool Equals(FlatVector other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            // 把这个理解为 cast 如果成功就赋值为other
            if (obj is FlatVector other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return new { this.X, this.Y }.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {this.X}, Y: {this.Y}";
        }
    }
}
