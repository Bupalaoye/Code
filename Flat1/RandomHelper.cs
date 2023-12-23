using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Flat1
{
    public static class RandomHelper
    {
        private static Random StaticRand = new Random();

        public static float RandomSingle(Random rand, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("min");
            }

            return min + (float)rand.NextDouble() * (max - min);
        }

        public static int RandomInterage(int min, int max)
        {
            if (min == max)
            {
                return min;
            }

            if (min > max)
            {
                Utils.Swap(ref min, ref max);
            }

            int result = min + StaticRand.Next() % (max - min);
            return result;
        }

        public static bool RandomBoolean() 
        {
            int value = RandomHelper.RandomInterage(0, 2);
            return value == 1;
        }
        public static float RandomSingle(float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("min");
            }

            return min + (float)StaticRand.NextDouble() * (max - min);
        }

        public static Vector2 RandomDirection(Random rand)
        {
            float angle = RandomHelper.RandomSingle(rand, 0f, MathHelper.TwoPi);
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        public static Color RandomColor()
        {
            return new Color((float)StaticRand.NextDouble(), (float)StaticRand.NextDouble(), (float)StaticRand.NextDouble());
        }
    }
}
