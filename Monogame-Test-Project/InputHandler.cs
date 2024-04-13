

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Collections.Generic;

namespace InputManagement
{
    /*
    Singleton

    I want this class to be the manager for the input so that input
    can be bound to an in game context (menu, game, etc) and can be monitored
    through a central pipeline access point instead of being scattered around the
    program

     the basic idea here is to read all incoming input of any kind and
     put it in some kind of data structure which is updated every frame with
     the current inputs
     */
    public class InputHandler
    {
        //enum KeyboardEnum
        //{
        //    None = 0,
        //}

        private KeyboardState keyState;
        
        public void GetInput()
        {
            keyState = Keyboard.GetState();
        }

        public bool iskeyPressed(Keys key)
        {
            return keyState.IsKeyDown(key);
        }
    }
}