using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Flat1;
using Flat1.Graphics;
using Flat1.Input;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
using Flat1.Physics;

namespace FlatAsteriods
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Screen screen;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;

        private FlatTransform transform;

        private List<Entity> entities;

        private SoundEffect rocketSound;
        private bool displayCollisionCircles = false;


        private Vector2[] vertices;
        private int[] triangles;
        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            // 开启垂直同步
            this.graphics.SynchronizeWithVerticalRetrace = true;

            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = true;  // 每帧固定时长
        }

        protected override void Initialize()
        {
            // 让窗口始终是80%
            DisplayMode dm = this.GraphicsDevice.DisplayMode;
            this.graphics.PreferredBackBufferWidth = (int)(dm.Width * 0.8f);
            this.graphics.PreferredBackBufferHeight = (int)(dm.Height * 0.8f);
            this.graphics.ApplyChanges();
            this.screen = new Screen(this, 1280, 720);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);

#if false
            // 确保每次允许结果相同
            Random random = new Random(0);

            this.entities = new List<Entity>();

            Vector2[] vertices = new Vector2[5];
            vertices[0] = new Vector2(10, 0);
            vertices[1] = new Vector2(-10, -10);
            vertices[2] = new Vector2(-5, -3);
            vertices[3] = new Vector2(-5, 3);
            vertices[4] = new Vector2(-10, 10);
            // 变换的angle 是逆时针的 相对于x轴 正方向
            this.transform = new FlatTransform(new Vector2(-32, 32), MathHelper.PiOver4, 1f);

            // 第一个entity 就是 player
            MainShip player = new MainShip(vertices, new Vector2(0, 0), Color.LightGreen, CommonDensities.Steel, 0.6f);
            this.entities.Add(player);

            int asteriodsCount = 25;
            for (int i = 0; i < asteriodsCount; i++)
            {
                Asteroid asteroid = new Asteroid(random, this.camera, CommonDensities.Rock, 0.6f);
                this.entities.Add(asteroid);
            }
#endif

            this.vertices = new Vector2[9];
            this.vertices[0] = new Vector2(-4, 6);
            this.vertices[1] = new Vector2(0, 2);
            this.vertices[2] = new Vector2(2, 5);
            this.vertices[3] = new Vector2(7, 0);
            this.vertices[4] = new Vector2(5, -6);
            this.vertices[5] = new Vector2(3, 3);
            this.vertices[6] = new Vector2(0, -5);
            this.vertices[7] = new Vector2(-6, 0);
            this.vertices[8] = new Vector2(-2, 1);
            
            if(!PolygonHelper.Triangulate(this.vertices,out this.triangles,out string errorMessage))
            {
                throw new Exception("Unable Triangulate vertices");
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            FlatKeyBoard keyboard = FlatKeyBoard.Instance;
            keyboard.Update();

            FlatMouse mouse = FlatMouse.Instance;
            mouse.Update();

            if (keyboard.IsKeyClicked(Keys.A))
            {
                this.camera.InZoom();
            }
            if (keyboard.IsKeyClicked(Keys.Z))
            {
                this.camera.DecZoom();
            }
            if (keyboard.IsKeyClicked(Keys.Escape))
            {
                this.Exit();
            }
#if false
            if (keyboard.IsKeyClicked(Keys.B))
            {
                this.displayCollisionCircles = !this.displayCollisionCircles;
            }

            float playerRotationAmount = MathHelper.Pi * (float)gameTime.ElapsedGameTime.TotalSeconds;

            MainShip player = (MainShip)this.entities[0];

            if (keyboard.IsKeyDown(Keys.Left))
            {
                player.Rotate(playerRotationAmount);
            }

            if (keyboard.IsKeyDown(Keys.Right))
            {
                player.Rotate(-playerRotationAmount);
            }

            if (keyboard.IsKeyDown(Keys.Up))
            {
                player.ApplyForce(50f * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
            {
                // 这个会一直执行
                player.DisableRocketForce();
            }

            for (int i = 0; i < this.entities.Count; i++)
            {
                this.entities[i].Update(gameTime, this.camera);
            }

            // collision detected
            for (int i = 0; i < this.entities.Count - 1; i++)
            {
                Entity a = this.entities[i];
                Circle ac = new Circle(a.Position, a.Radius);

                for (int j = i + 1; j < entities.Count; j++)
                {
                    Entity b = entities[j];
                    Circle bc = new Circle(b.Position, b.Radius);
                    if (Collision.IntersectCircles(ac, bc, out float depth, out Vector2 normal))
                    {
                        Vector2 mtv = depth * normal;

                        a.Move(-mtv / 2f);
                        b.Move(mtv / 2f);

                        Game1.SolveCollision(a, b, normal);
                        a.CircleColor = Color.Red;
                        b.CircleColor = Color.Red;
                    }
                }
            }
#endif

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(Color.Black);

            this.shapes.Begin(this.camera);
            // this.shapes.DrawPolygon(this.vertices, this.transform, 1f, Color.LightSeaGreen);
#if false
            for (int i = 0; i < this.entities.Count; i++)
            {
                this.entities[i].Draw(this.shapes, this.displayCollisionCircles);
            }
#endif
            this.transform = new FlatTransform(new Vector2(0, 0), 0f, 20f);
            this.shapes.DrawPolygonTriangles(this.vertices, this.triangles, this.transform, Color.White);
            //this.shapes.DrawPolygon(this.vertices, transform, 1f, Color.White);
            this.shapes.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);

            base.Draw(gameTime);
        }

        // 这里并没有处理 物体旋转/摩檫力 
        public static void SolveCollision(Entity a, Entity b, Vector2 normal)
        {
            Vector2 relVec = b.Velocity - a.Velocity;

            // 这里是判断a,b是否是正在远离, 如果是正在远离,那么没必要再处理了
            if (Utils.Dot(relVec, normal) > 0)
            {
                return;
            }

            // 可恢复系数 由更小的决定
            float e = MathHelper.Min(a.Restitution, b.Restitution);

            float j = -(1f + e) * Utils.Dot(relVec, normal);
            j /= a.InvMass + b.InvMass;

            Vector2 impluse = j * normal;

            a.Velocity -= a.InvMass * impluse;
            b.Velocity += b.InvMass * impluse;
        }
    }
}