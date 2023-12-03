using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flat1.Graphics
{
    public sealed class Screen : IDisposable
    {
        private readonly static int MinDim = 64;
        private readonly static int MaxDim = 4096;

        private bool isDisposed;
        private Game game;
        private RenderTarget2D target;
        private bool isSet;

        public int Width
        {
            get
            {
                return this.target.Width;
            }
        }

        public int Height
        {
            get
            {
                return this.target.Height;
            }
        }

        public Screen(Game game, int width, int height)
        {
            width = Utils.Clamp(width, Screen.MinDim, Screen.MaxDim);
            height = Utils.Clamp(height, Screen.MinDim, Screen.MaxDim);

            this.game = game ?? throw new ArgumentNullException("game");
            this.target = new RenderTarget2D(this.game.GraphicsDevice, width, height);
            this.isSet = false;
        }


        public void Dispose()
        {
            if (isDisposed)
                return;

            this.target?.Dispose();

            this.isDisposed = true;
        }

        public void Set()
        {
            if (this.isSet)
            {
                throw new Exception("Render target is aready set.");
            }
            this.game.GraphicsDevice.SetRenderTarget(this.target);
            this.isSet = true;
        }

        public void Unset()
        {
            if (!this.isSet)
            {
                throw new Exception("Render target is not aready set.");
            }
            this.game.GraphicsDevice.SetRenderTarget(null);
            this.isSet = false;
        }

        public void Present(Sprites sprites, bool textureFiltering = true)
        {
            if (sprites is null)
            {
                throw new ArgumentNullException("sprites");
            }

#if DEBUG
            this.game.GraphicsDevice.Clear(Color.HotPink);
#else
            this.game.GraphicsDevice.Clear(Color.Black);
#endif

            Rectangle destinationRectangle = CalculateDestinationRectangle();
            sprites.Begin(null, textureFiltering);
            sprites.Draw(this.target, null, destinationRectangle, Color.White);
            sprites.End();
        }

        // 这样整个程序集都可以访问
        internal Rectangle CalculateDestinationRectangle()
        {
            Rectangle backbufferBounds = this.game.GraphicsDevice.PresentationParameters.Bounds;
            float backBufferAspectRatio = (float)backbufferBounds.Width / backbufferBounds.Height;
            float screenAspectRatio = (float)this.Width / this.Height;

            float rx = 0f;
            float ry = 0f;
            float rw = backbufferBounds.Width;
            float rh = backbufferBounds.Height;

            // 长宽比 就是更长的占优势
            if (backBufferAspectRatio > screenAspectRatio)
            {
                rw = rh * screenAspectRatio;
                rx = ((float)backbufferBounds.Width - rw) / 2f;
            }
            else if (backBufferAspectRatio < screenAspectRatio)
            {
                rh = rw / screenAspectRatio;
                ry = ((float)backbufferBounds.Height - rh) / 2f;
            }

            Rectangle result = new Rectangle((int)rx, (int)ry, (int)rw, (int)rh);
            return result;
        }
    }
}
