using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Flat1.Graphics
{
    public class Sprites : IDisposable
    {
        private bool isDisposable;
        private Game game;
        private SpriteBatch sprites;
        private BasicEffect effect;

        public Sprites(Game game)
        {
            if (game is null)
            {
                throw new ArgumentNullException("game");
            }

            this.game = game;
            this.isDisposable = false;
            this.sprites = new SpriteBatch(this.game.GraphicsDevice);
            this.effect = new BasicEffect(this.game.GraphicsDevice);
            this.effect.FogEnabled = false;
            this.effect.TextureEnabled = true;
            this.effect.LightingEnabled = false;
            this.effect.VertexColorEnabled = true;
            // mvp : beacuse this is 2D  so it's identity
            this.effect.World = Matrix.Identity;
            this.effect.Projection = Matrix.Identity;
            this.effect.View = Matrix.Identity;
        }

        public void Dispose()
        {
            if (this.isDisposable)
            {
                return;
            }

            this.effect?.Dispose();
            // if is not null then call dispose
            this.sprites?.Dispose();
            this.isDisposable = true;
        }

        public void Begin(Camera camera, bool isTextureFilteringEnabled)
        {
            SamplerState sampler = SamplerState.PointClamp;
            if (isTextureFilteringEnabled)
            {
                sampler = SamplerState.LinearClamp;
            }
            if (camera is null)
            {
                Viewport vp = this.game.GraphicsDevice.Viewport;
                this.effect.Projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, 0, vp.Height, 0f, 1f);
                this.effect.View = Matrix.Identity;
            }
            else
            {
                camera.UpdateMatrices();
                this.effect.View = camera.View;
                this.effect.Projection = camera.Projection;
            }

            this.sprites.Begin(blendState: BlendState.AlphaBlend, samplerState: sampler, rasterizerState: RasterizerState.CullNone, effect: this.effect);
        }

        public void End()
        {
            this.sprites.End();
        }

        public void Draw(Texture2D texture, Vector2 origin, Vector2 position, Color color)
        {
            // SpriteEffects.FlipVertically 垂直反转
            this.sprites.Draw(texture, position, null, color, 0f, origin, 1f, SpriteEffects.FlipVertically, 0f);
        }


        // 如果sourceRectangle 为null 则绘制整个纹理
        public void Draw(Texture2D texture, Rectangle? sourceRectangle, Vector2 origin, Vector2 position, float roation, Vector2 scale, Color color)
        {
            // 最后一个参数是 深度 depth
            this.sprites.Draw(texture, position, sourceRectangle, color, roation, origin, scale, SpriteEffects.FlipVertically, 0f);
        }

        public void Draw(Texture2D texture, Rectangle? sourceRectangle, Rectangle destinationRectangle, Color color)
        {
            // 注意这里的顺序是先 dest  再 source
            this.sprites.Draw(texture, destinationRectangle, sourceRectangle, color, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
        }
    }
}
