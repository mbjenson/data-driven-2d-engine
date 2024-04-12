using ECS;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameContext
{
    /*
    GameContext
    
    contains specialized systems like a texture manager, sound manager, component systems, etc which are specific for rendering

     
    */
    public class GameContext : Context
    {
        Dictionary<string, Dictionary<string, Texture2D>> textureManager;

        public GameContext(int maxEntities) : base(maxEntities)
        {
            
        }

        

    }
}