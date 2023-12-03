using System;
using Microsoft.Xna.Framework;

namespace Flat1
{
    public struct FlatTransform
    {
        public float PosX;
        public float PosY;

        public float CosScaleX;
        public float SinScaleX;
        public float SinScaleY;
        public float CosScaleY;

        public FlatTransform(Vector2 position, float angle, Vector2 Scale)
        {
            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);
            this.PosX = position.X;
            this.PosY = position.Y;
            this.SinScaleX = Scale.X * sin;
            this.CosScaleX = Scale.X * cos;
            this.SinScaleY = Scale.Y * sin;
            this.CosScaleY = Scale.Y * cos;
        }
        public FlatTransform(Vector2 position, float angle, float Scale)
        {
            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);
            this.PosX = position.X;
            this.PosY = position.Y;
            this.SinScaleX = Scale * sin;
            this.CosScaleX = Scale * cos;
            this.SinScaleY = Scale * sin;
            this.CosScaleY = Scale * cos;
        }

        public Matrix ToMatrix()
        {
            Matrix result = Matrix.Identity;
            result.M11 = this.CosScaleX;
            result.M12 = this.SinScaleX;
            result.M21 = -this.SinScaleX;
            result.M22 = this.CosScaleY;

            result.M41 = this.PosX;
            result.M42 = this.PosY;
            return result;
        }

    }
}
