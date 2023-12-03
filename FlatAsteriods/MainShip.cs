using Flat1;
using Flat1.Graphics;
using Microsoft.Xna.Framework;
using System;


namespace FlatAsteriods
{
    public class MainShip : Entity
    {
        private bool isRocketForce;
        private Vector2[] rocketVertices;
        private double randomRocketTime;
        private double randomRocketStartTime;

        public MainShip(Vector2[] vertices, Vector2 position, Color color, float density, float restitution)
            : base(vertices, position, color, density, restitution)
        {
            this.isRocketForce = false;

            this.rocketVertices = new Vector2[3];
            this.rocketVertices[0] = this.vertices[3];
            this.rocketVertices[1] = this.vertices[2];
            this.rocketVertices[2] = new Vector2(-20f, 0f);

            this.randomRocketStartTime = 0d;
            this.randomRocketTime = 100d;

            float area = MathHelper.Pi * this.Radius * this.Radius;
            this.mass = area * density;
            this.invMass = 1 / mass;
        }

        public override void Draw(Shapes shapes, bool displayCollisionCircle)
        {
            if (this.isRocketForce)
            {
                FlatTransform transform = new FlatTransform(this.position, this.angle, 1f);
                shapes.DrawPolygon(this.rocketVertices, transform, 1f, Color.Yellow);
            }

            base.Draw(shapes, displayCollisionCircle);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            double now = gameTime.TotalGameTime.TotalMilliseconds;
            // 尾巴改变的时间间隔
            if (now - this.randomRocketStartTime >= this.randomRocketTime)
            {
                this.randomRocketStartTime = now;

                float rocketMinX = -24f;
                float rocketMaxX = -16f;
                float rocketMinY = -2f;
                float rocketMaxY = 2f;

                this.rocketVertices[2] = new Vector2(
                    RandomHelper.RandomSingle(rocketMinX, rocketMaxX),
                    RandomHelper.RandomSingle(rocketMinY, rocketMaxY));
            }

            base.Update(gameTime, camera);
        }

        public void Rotate(float amount)
        {
            this.angle += amount;

            if (this.angle < 0)
            {
                this.angle += MathHelper.TwoPi;
            }
            if (this.angle >= MathHelper.TwoPi)
            {
                this.angle -= MathHelper.TwoPi;
            }
        }

        public void ApplyForce(float amount)
        {
            // 其实就是单位向量
            Vector2 forceDir = new Vector2(MathF.Cos(this.angle), MathF.Sin(this.angle));
            this.velocity += forceDir * amount;

            this.isRocketForce = true;
        }

        public void DisableRocketForce()
        {
            this.isRocketForce = false;
        }
    }
}
