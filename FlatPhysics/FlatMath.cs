using System;


namespace FlatPhysics
{
    public static class FlatMath
    {
        private static readonly float VerySmallAmount = 0.0005f;
        public static float Clamp(float value, float min, float max)
        {
            if (min == max)
            {
                return min;
            }
            if (min > max)
            {
                throw new ArgumentException("min is greater then max.");
            }
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (min == max)
            {
                return min;
            }
            if (min > max)
            {
                throw new ArgumentException("min is greater then max.");
            }
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        public static float Length(FlatVector v)
        {
            return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public static float Distance(FlatVector v1, FlatVector v2)
        {
            float dx = v1.X - v2.X;
            float dy = v1.Y - v2.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static FlatVector Normalize(FlatVector v)
        {
            float len = FlatMath.Length(v);
            return new FlatVector(v.X / len, v.Y / len);
        }

        public static float Dot(FlatVector v1, FlatVector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static float Cross(FlatVector v1, FlatVector v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        public static float LengthSquared(FlatVector ab)
        {
            return ab.X * ab.X + ab.Y * ab.Y;
        }

        public static float DistanceSquared(FlatVector va, FlatVector vb)
        {
            float dx = va.X - vb.X;
            float dy = va.Y - vb.Y;
            return dx * dx + dy * dy;
        }

        public static bool NearlyEqual(float a, float b)
        {
            return MathF.Abs(a - b) < FlatMath.VerySmallAmount;
        }

        public static bool NearlyEqual(FlatVector a, FlatVector b)
        {
            // 距离的比较 
            return FlatMath.DistanceSquared(a, b) < FlatMath.VerySmallAmount * FlatMath.VerySmallAmount;
        }
    }
}
