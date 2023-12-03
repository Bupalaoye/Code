using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Flat1.Graphics;

namespace Flat1.Input
{
    public sealed class FlatMouse
    {
        // 懒汉单例模式
        private static readonly Lazy<FlatMouse> Lazy = new Lazy<FlatMouse>(() => new FlatMouse());

        public static FlatMouse Instance
        {
            get { return Lazy.Value; }
        }


        private MouseState prevMouseState;
        private MouseState currMouseState;

        // 这个position是相对于窗口的坐标
        public Point WindowPosition
        {
            get { return this.currMouseState.Position; }
        }

        public FlatMouse()
        {
            this.prevMouseState = Mouse.GetState();
            this.currMouseState = this.prevMouseState;
        }

        public void Update()
        {
            this.prevMouseState = this.currMouseState;
            this.currMouseState = Mouse.GetState();
        }

        public bool IsLeftButtonDown()
        {
            return this.currMouseState.LeftButton == ButtonState.Pressed;
        }
        public bool IsRightButtonDown()
        {
            return this.currMouseState.RightButton == ButtonState.Pressed;
        }
        public bool IsMiddleButtonDown()
        {
            return this.currMouseState.MiddleButton == ButtonState.Pressed;
        }


        public bool IsLeftButtonClicked()
        {
            return this.currMouseState.LeftButton == ButtonState.Pressed && this.prevMouseState.LeftButton == ButtonState.Released;
        }

        public bool IsRightButtonClicked()
        {
            return this.currMouseState.RightButton == ButtonState.Pressed && this.prevMouseState.RightButton == ButtonState.Released;
        }

        public bool IsMiddleButtonClicked()
        {
            return this.currMouseState.MiddleButton == ButtonState.Pressed && this.prevMouseState.MiddleButton == ButtonState.Released;
        }


        public Vector2 GetScreenPosition(Screen screen)
        {
            Rectangle screenDestinationRectangle = screen.CalculateDestinationRectangle();

            Point windowPosition = this.WindowPosition;

            float sx = windowPosition.X - screenDestinationRectangle.X;
            float sy = windowPosition.Y - screenDestinationRectangle.Y;

            sx /= (float)screenDestinationRectangle.Width;
            sy /= (float)screenDestinationRectangle.Height;

            sx *= (float)screen.Width;
            sy *= (float)screen.Height;
            // 需要反转 因为再Screen坐标系是左下角为原点的
            sy = (float)screen.Height - sy;
            return new Vector2(sx, sy);
        }
    }
}
