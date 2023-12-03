using System;
using Flat1;
using Flat1.Graphics;
using Microsoft.Xna.Framework;

namespace FlatAsteriods
{
    public class Asteroid : Entity
    {
        public Asteroid(Random rand, Camera camera, float density, float restitution)
            : base(null, Vector2.Zero, Color.Brown, density, restitution)
        {

            int minPoints = 6;
            int maxPoints = 10;

            int points = rand.Next(minPoints, maxPoints);

            this.vertices = new Vector2[points];

            float deltaAngle = MathHelper.TwoPi / (float)points;
            float angle = 0f;

            float minDist = 12f;
            float maxDist = 24f;
            for (int i = 0; i < points; i++)
            {
                float dist = RandomHelper.RandomSingle(rand, minDist, maxDist);

                float x = MathF.Cos(angle) * dist;
                float y = MathF.Sin(angle) * dist;

                angle += deltaAngle;

                this.vertices[i] = new Vector2(x, y);
            }

            camera.GetExtents(out Vector2 camMin, out Vector2 camMax);
            // 这样就可以避免出现'全屏'的小行星
            camMin *= 0.75f;
            camMax *= 0.75f;

            float px = RandomHelper.RandomSingle(rand, camMin.X, camMax.X);
            float py = RandomHelper.RandomSingle(rand, camMin.Y, camMax.Y);

            this.position = new Vector2(px, py);

            float minSpeed = 20f;
            float maxSpeed = 40f;

            Vector2 velDir = RandomHelper.RandomDirection(rand);
            float speed = RandomHelper.RandomSingle(rand, minSpeed, maxSpeed);

            this.velocity = velDir * speed;

            this.collisionCircleRadius = Entity.FindCollisionCircleRadius(vertices);

            float area = MathHelper.Pi * this.Radius * this.Radius;
            this.mass = area * this.density;
            this.invMass = 1 / this.mass;
        }
    }
}
