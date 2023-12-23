using System;
using Microsoft.Xna.Framework;

namespace FlatPhysics
{
    public static class FlatConverter
    {
        public static Vector2 ToVector2(FlatVector v)
        {
            return new Vector2(v.X, v.Y);
        }
        public static FlatVector ToFlatVector(Vector2 v)
        {
            return new FlatVector(v.X, v.Y);
        }


        public static void ToVector2Array(FlatVector[] src, ref Vector2[] dst)
        {
            if (dst == null || src.Length != dst.Length)
            {
                dst = new Vector2[src.Length];
            }

            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = new Vector2(src[i].X, src[i].Y);
            }
        }
    }
}
