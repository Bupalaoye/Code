using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Flat1.Input
{
    public sealed class FlatKeyBoard
    {
        // 懒汉单例模式
        private static readonly Lazy<FlatKeyBoard> Lazy = new Lazy<FlatKeyBoard>(() => new FlatKeyBoard());

        public static FlatKeyBoard Instance 
        {
            get { return Lazy.Value; }
        }


        private KeyboardState prevKeyboardState;
        private KeyboardState currKeyboardState;

        public FlatKeyBoard()
        {
            this.prevKeyboardState = Keyboard.GetState();
            this.currKeyboardState = this.prevKeyboardState;
        }

        public void Update()
        {
            this.prevKeyboardState = this.currKeyboardState;
            this.currKeyboardState = Keyboard.GetState();
        }

        public bool IsKeyDown(Keys key) 
        { 
            return this.currKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyClicked(Keys key)
        {
            // prev 不是down  curr 为down 才为true
            return this.currKeyboardState.IsKeyDown(key) && !this.prevKeyboardState.IsKeyDown(key);
        }


    }
}
