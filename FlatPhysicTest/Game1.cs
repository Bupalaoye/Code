using Flat1;
using Flat1.Graphics;
using Flat1.Input;
using FlatPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FlatPhysicTest
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private Screen screen;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;
        private SpriteFont fontConsolas18;

        private List<FlatEntity> entityList;
        private List<FlatEntity> entityRemoveList;
        private FlatWorld world;
        private Stopwatch watch;

        private double totalWorldStepTime = 0;
        private int totalBodyCount = 0;
        private int totalSampleCount = 0;
        private Stopwatch samperTimer = new Stopwatch();

        private string worldStepTimeString = string.Empty;
        private string bodyCountString = string.Empty;

        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.SynchronizeWithVerticalRetrace = true;   // 开启垂直同步

            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = true;  // 每帧间隔时间固定

            const double UpdatesPreSecond = 60d;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatesPreSecond));
        }

        protected override void Initialize()
        {
            this.Window.Position = new Point(10, 40);
            // 让窗口始终是80%
            DisplayMode dm = this.GraphicsDevice.DisplayMode;
            this.graphics.PreferredBackBufferWidth = (int)(dm.Width * 0.8f);
            this.graphics.PreferredBackBufferHeight = (int)(dm.Height * 0.8f);
            this.graphics.ApplyChanges();
            this.screen = new Screen(this, 1280, 768);
            this.sprites = new Sprites(this);
            this.shapes = new Shapes(this);
            this.camera = new Camera(this.screen);
            this.world = new FlatWorld();
            this.camera.SetZoom(24);

            this.camera.GetExtents(out float left, out float right, out float bottom, out float top);
            float padding = (right - left) * 0.1f;   // 间隔 使其不会挨着'windows' 的边框

            this.entityList = new List<FlatEntity>();
            this.entityRemoveList = new List<FlatEntity>();

            if (!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, 1f, true,
                0.5f, out FlatBody groundBody, out string errorMessage))
            {
                throw new Exception(errorMessage);
            }

            groundBody.MoveTo(new FlatVector(0, -10));
            this.world.AddBody(groundBody);
            this.entityList.Add(new FlatEntity(groundBody, Color.DarkGreen));

            if (!FlatBody.CreateBoxBody(20f, 2f, 1f, true, 0.5f, out FlatBody ledgeBody1, out errorMessage))
            {
                throw new Exception(errorMessage);
            }

            ledgeBody1.MoveTo(new FlatVector(-10, 5));
            ledgeBody1.Rotate(-MathHelper.TwoPi / 20f);
            this.world.AddBody(ledgeBody1);
            this.entityList.Add(new FlatEntity(ledgeBody1, Color.DarkGray));

            if (!FlatBody.CreateBoxBody(20f, 2f, 1f, true, 0.5f, out FlatBody ledgeBody2, out errorMessage))
            {
                throw new Exception(errorMessage);
            }

            ledgeBody2.MoveTo(new FlatVector(10, 10));
            ledgeBody2.Rotate(MathHelper.TwoPi / 20f);
            this.world.AddBody(ledgeBody2);
            this.entityList.Add(new FlatEntity(ledgeBody2, Color.DarkRed));

            this.watch = new Stopwatch();
            this.samperTimer.Start();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.fontConsolas18 = this.Content.Load<SpriteFont>("Consolas18");
        }

        protected override void Update(GameTime gameTime)
        {
            FlatKeyBoard keyBoard = FlatKeyBoard.Instance;
            FlatMouse mouse = FlatMouse.Instance;

            keyBoard.Update();
            mouse.Update();

            // add box body
            if (mouse.IsLeftButtonClicked())
            {
                float width = RandomHelper.RandomSingle(1f, 2f);
                float height = RandomHelper.RandomSingle(1f, 2f);

                FlatVector position = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this.screen, this.camera));
                this.entityList.Add(new FlatEntity(world, width, height, false, position));
            }
            // add circle body
            if (mouse.IsRightButtonClicked())
            {
                float radius = RandomHelper.RandomSingle(0.8f, 1.5f);

                FlatVector position = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this.screen, this.camera));
                this.entityList.Add(new FlatEntity(world, radius, false, position));
            }

            if (keyBoard.IsKeyAvailable)
            {
                if (keyBoard.IsKeyClicked(Keys.OemTilde))
                {
                    Console.WriteLine("The world total body : " + this.world.BodyCount);
                    Console.WriteLine("The StopWatch        : " + Math.Round(this.watch.Elapsed.TotalMilliseconds, 4));
                }
                if (keyBoard.IsKeyClicked(Keys.Escape))
                {
                    this.Exit();
                }
                if (keyBoard.IsKeyClicked(Keys.A))
                {
                    this.camera.InZoom();
                }
                if (keyBoard.IsKeyClicked(Keys.Z))
                {
                    this.camera.DecZoom();
                }

#if false
                float dx = 0f;
                float dy = 0f;
                float forceMagnitude = 100f;

                if (keyBoard.IsKeyDown(Keys.Left)) { dx--; }
                if (keyBoard.IsKeyDown(Keys.Right)) { dx++; }
                if (keyBoard.IsKeyDown(Keys.Up)) { dy++; }
                if (keyBoard.IsKeyDown(Keys.Down)) { dy--; }

                if (!this.world.GetBody(0, out FlatBody body))
                {
                    throw new Exception($"can't get the {0} index body in flat world!");
                }
                if (dx != 0f || dy != 0f)
                {
                    FlatVector direction = FlatMath.Normalize(new FlatVector(dx, dy));
                    // 因为再flatWorld 的step 会和 gameTime 相乘
                    FlatVector force = direction * forceMagnitude;
                    body.AddForce(force);
                }
                if (keyBoard.IsKeyDown(Keys.R))
                {
                    body.Rotate(MathF.PI / 2 * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
#endif
            }

            if (this.samperTimer.Elapsed.TotalSeconds > 1d)
            {
                this.bodyCountString = "BodyCount     : " + Math.Round(this.totalBodyCount / (double)this.totalSampleCount, 4).ToString();
                this.worldStepTimeString = "WorldStepTime : " + Math.Round(this.totalWorldStepTime / (double)this.totalSampleCount, 4).ToString();
                this.totalBodyCount = 0;
                this.totalWorldStepTime = 0d;
                this.totalSampleCount = 0;
                this.samperTimer.Restart();
            }

            watch.Restart();
            this.world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 20);
            watch.Stop();

            this.totalWorldStepTime += this.watch.Elapsed.TotalMilliseconds;
            this.totalBodyCount += this.world.BodyCount;
            this.totalSampleCount++;

            this.camera.GetExtents(out _, out _, out float viewBottom, out _);
            this.entityRemoveList.Clear();

            for (int i = 0; i < this.entityList.Count; i++)
            {
                FlatEntity entity = this.entityList[i];
                FlatBody body = entity.Body;
                // static object not remove
                if (body.IsStatic)
                {
                    continue;
                }

                FlatAABB box = body.GetAABB();
                if (box.Max.Y < viewBottom)
                {
                    this.entityRemoveList.Add(entity);
                }
            }

            for(int i=0;i<this.entityRemoveList.Count;i++)
            {
                FlatEntity entity = this.entityRemoveList[i];
                this.world.RemoveBody(entity.Body);
                this.entityList.Remove(entity);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            this.GraphicsDevice.Clear(new Color(50, 60, 70));

            this.shapes.Begin(this.camera);
            for (int i = 0; i < this.entityList.Count; i++)
            {
                this.entityList[i].Draw(this.shapes);
            }
            this.shapes.End();

            Vector2 stringSize = this.fontConsolas18.MeasureString(this.bodyCountString);

            this.sprites.Begin(null, false);
            this.sprites.DrawString(this.fontConsolas18, this.bodyCountString, new Vector2(2, 0), Color.White);
            this.sprites.DrawString(this.fontConsolas18, this.worldStepTimeString, new Vector2(2, stringSize.Y), Color.White);
            this.sprites.End();

            this.screen.Unset();
            this.screen.Present(this.sprites);
            base.Draw(gameTime);
        }

    }
}