using System;
using Microsoft.Xna.Framework;

namespace Flat1.Physics
{
    public static class Collision
    {
        // 很简单的判断两个圆有没有相交
        public static bool IntersectCircles(Circle a, Circle b)
        {
            float distSquared = Utils.DistanceSquared(a.Center, b.Center);
            float r2 = a.Radius + b.Radius;
            float radiusSquared = r2 * r2;

            if (distSquared >= radiusSquared)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断连个圆是否相交，返回深度和(单位向量)方向 a->b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="depth"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static bool IntersectCircles(Circle a, Circle b, out float depth, out Vector2 normal)
        {
            depth = 0;
            normal = Vector2.Zero;

            Vector2 n = b.Center - a.Center;
            float distSquared = Utils.DistanceSquared(a.Center, b.Center);
            float r2 = a.Radius + b.Radius;
            float radiusSquared = r2 * r2;

            if (distSquared >= radiusSquared)
            {
                return false;
            }

            float dist = MathF.Sqrt(distSquared);
            if (dist != 0)
            {
                depth = r2 - dist;
                normal = n / dist;
            }
            else
            {
                // 同心圆的情况
                depth = r2;
                normal = new Vector2(1f, 0);
            }

            return true;
        }

    }
}
