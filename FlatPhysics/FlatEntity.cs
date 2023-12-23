using System;
using Flat1;
using Flat1.Graphics;
using Microsoft.Xna.Framework;

namespace FlatPhysics
{
    public sealed class FlatEntity
    {
        public readonly FlatBody Body;
        public readonly Color Color;

        public FlatEntity(FlatBody body)
        {
            this.Body = body;
            this.Color = RandomHelper.RandomColor();
        }

        public FlatEntity(FlatBody body, Color color)
        {
            this.Body = body;
            this.Color = color;
        }

        public FlatEntity(FlatWorld world, float radius, bool isStatic, FlatVector position)
        {
            if (!FlatBody.CreateCircleBody(radius, 1f, isStatic, 0.5f,
                out FlatBody body, out string errorMsg))
            {
                throw new ArgumentException(errorMsg);
            }

            body.MoveTo(position);
            this.Body = body;
            this.Color = RandomHelper.RandomColor();
            world.AddBody(body);
        }

        public FlatEntity(FlatWorld world, float width, float height, bool isStatic, FlatVector position)
        {
            if (!FlatBody.CreateBoxBody(width, height, 1f, isStatic, 0.5f,
                out FlatBody body, out string errorMsg))
            {
                throw new ArgumentException(errorMsg);
            }

            body.MoveTo(position);
            this.Body = body;
            this.Color = RandomHelper.RandomColor();
            world.AddBody(body);
        }

        public void Draw(Shapes shapes)
        {
            Vector2 position = FlatConverter.ToVector2(Body.Position);
            if (this.Body.shapeType == ShapeType.Circle)
            {
                FlatVector va = FlatVector.Zero;
                FlatVector vb = new FlatVector(Body.Radius,0f);
                FlatTransform transform = new FlatTransform(Body.Position,Body.Angle);
                va = FlatVector.Transform(va, transform);
                vb = FlatVector.Transform(vb, transform);
                shapes.DrawCircleFill(position, this.Body.Radius, 26, this.Color);
                shapes.DrawCircle(position, this.Body.Radius, 26, 1f, Color.White);
                shapes.DrawLine(FlatConverter.ToVector2(va), FlatConverter.ToVector2(vb), Color.White);
            }
            else if (this.Body.shapeType == ShapeType.Box)
            {
                shapes.DrawBoxFill(position, this.Body.Width, this.Body.Height, this.Body.Angle, this.Color);
                shapes.DrawBox(position, this.Body.Width, this.Body.Height, this.Body.Angle, Color.White);
            }
        }
    }
}
