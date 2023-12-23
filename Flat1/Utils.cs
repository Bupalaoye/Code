using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Flat1
{
    public static class Utils
    {
        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("The value of \"min\" is greater then \"max\". ");
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("The value of \"min\" is greater then \"max\". ");
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        // 优化计算 通常除法比浮点数相乘慢很多
        public static void Normalize(ref float x, ref float y)
        {
            float invLen = 1f / MathF.Sqrt(x * x + y * y);
            x *= invLen;
            y *= invLen;
        }

        public static void ToggleFullScreen(GraphicsDeviceManager graphics)
        {
            // 无边框全屏 : '可以通过 alt + tab 快速再不同应用间切换'
            graphics.HardwareModeSwitch = false;
            graphics.ToggleFullScreen();
        }


        public static Vector2 Transform(Vector2 position, FlatTransform transform)
        {
            // Scale and Rotation and Translation

            return new Vector2(
                position.X * transform.CosScaleX - position.Y * transform.SinScaleY + transform.PosX,
                position.X * transform.SinScaleX + position.Y * transform.CosScaleY + transform.PosY);
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static float DistanceSquared(Vector2 a, Vector2 b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return dx * dx + dy * dy;
        }

        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            // 这个公式是由三位变量推导出来的,因为a和b的z向量为0 , 结果向量就只剩下一个z轴了
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// 循环地取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetItem<T>(T[] array, int index)
        {
            if (index >= array.Length)
            {
                return array[index % array.Length];
            }
            else if (index < 0)
            {
                return array[index % array.Length + array.Length];
            }
            else
            {
                return array[index];
            }
        }

        public static T GetItem<T>(List<T> array, int index)
        {
            if (index >= array.Count)
            {
                return array[index % array.Count];
            }
            else if (index < 0)
            {
                return array[index % array.Count + array.Count];
            }
            else
            {
                return array[index];
            }
        }

        internal static void Swap(ref int min, ref int max)
        {
            int t = min;
            min = max;
            max = t;
        }
    }
}
