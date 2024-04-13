using ECS;
using InputManagement;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ECS
{
    /*
    GameContext
    
    contains specialized systems like a texture manager, sound manager, component systems, etc which are specific for rendering

     
    */
    public class GameContext : Context
    {
        public SpriteSheetManager spriteMan;
        public InputHandler inputHandler;

        public GameContext(int maxEntities) : base(maxEntities)
        {
            spriteMan = new SpriteSheetManager();
            inputHandler = new InputHandler();
        }

        public void Update()
        {
            inputHandler.GetInput();
        }

    }
}