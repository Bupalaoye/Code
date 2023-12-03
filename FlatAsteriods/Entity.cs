using Microsoft.Xna.Framework;
using Flat1;
using Flat1.Graphics;
using Flat1.Physics;
using System;

namespace FlatAsteriods
{

    // 类似于Unreal Engine 里的 Actor 基类
    public abstract class Entity
    {
        protected Vector2[] vertices;
        protected Vector2 position;
        protected Vector2 velocity;
        protected float angle;
        protected Color color;

        public Color CircleColor;
        protected float collisionCircleRadius;

        protected float mass;
        protected float invMass;
        protected float density;  // 密度
        protected float restitution;  // 恢复原状: value (0~1) 比如橡皮和泥土的区别

        public float InvMass
        {
            get { return invMass; }
        }

        public float Restitution
        {
            get { return restitution; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public Vector2 Velocity
        {
            get { return this.velocity; }
            set { this.velocity = value; }
        }

        public float Radius
        {
            get { return this.collisionCircleRadius; }
        }


        public Entity(Vector2[] vertices, Vector2 position, Color color, float density, float restitution)
        {
            this.vertices = vertices;
            this.position = position;
            this.velocity = Vector2.Zero;
            this.angle = 0f;
            this.color = color;

            if (vertices != null)
            {
                this.collisionCircleRadius = Entity.FindCollisionCircleRadius(vertices);
            }

            this.CircleColor = Color.White;
            this.density = Utils.Clamp(density, CommonDensities.MinDensity, CommonDensities.MaxDensity);
            this.restitution = Utils.Clamp(restitution, 0f, 1f);
            this.mass = 0f;
            this.invMass = 1f;
        }

        protected static float FindCollisionCircleRadius(Vector2[] vetices)
        {
            float polygonArea = PolygonHelper.FindPolygonArea(vetices);
            return MathF.Sqrt(polygonArea / MathHelper.Pi);
        }

        public virtual void Update(GameTime gameTime, Camera camera)
        {
            this.position += this.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 越界处理
            camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

            float cameraViewWidth = camMax.X - camMin.X;
            float cameraViewHeight = camMax.Y - camMin.Y;

            if (this.position.X < camMin.X) { this.position.X += cameraViewWidth; }
            if (this.position.X > camMax.X) { this.position.X -= cameraViewWidth; }
            if (this.position.Y < camMin.Y) { this.position.Y += cameraViewHeight; }
            if (this.position.Y > camMax.Y) { this.position.Y -= cameraViewHeight; }

            this.CircleColor = Color.White;
        }

        public virtual void Draw(Shapes shapes, bool displayCollisionCircle)
        {
            FlatTransform transform = new FlatTransform(this.position, this.angle, 1f);
            shapes.DrawPolygon(this.vertices, transform, 1f, color);

            if (displayCollisionCircle)
            {
                shapes.DrawCircle(this.position.X, this.position.Y, this.collisionCircleRadius, 32, 1f, this.CircleColor);
            }
        }

        public void Move(Vector2 amount)
        {
            this.position += amount;
        }
    }
}
