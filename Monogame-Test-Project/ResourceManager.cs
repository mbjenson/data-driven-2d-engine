using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace ECS
{

    // Currently this is equivilent to a dictionary, might add more functionality in the future which would make
    // it worth using but as for right now, it is not worth using.
    //public class IResourceManager<ID, T>
    //{
    //    protected Dictionary<ID, T> items;

    //    public IResourceManager()
    //    {
    //        this.items = new Dictionary<ID, T>();
    //    }

    //    public void Add(ID identifier, T item)
    //    {
    //        if (identifier != null && item != null && items.ContainsKey(identifier))
    //        {
    //            items.Add(identifier, item);
    //        }
    //    }

    //    public T Get(ID identifier)
    //    {
    //        return items[identifier];
    //    }

    //    public void Remove(ID identifier)
    //    {
    //        items.Remove(identifier);
    //    }
    //}


    /*
    the general idea is to have a texture that has a bunch of rects paired with it that 
    represent where sprites are located within a large texture sheet
     */
    public class SpriteSheetManager
    {
        //              spriteSheet name, texture
        public Dictionary<string, Texture2D> dSpriteSheets;
        //              spriteSheet name,      sprite name, dimensions
        public Dictionary<string, Dictionary<string, Rectangle>> dSpriteRects;
        // one will add textures to the dTextures then add the corresponding item 
        // dimensions which live within that texture 
        
        public SpriteSheetManager()
        {
            dSpriteSheets = new Dictionary<string, Texture2D> ();
            dSpriteRects = new Dictionary<string, Dictionary<string, Rectangle>> ();
        }

        // add sprite associated with a spritesheet
        public void AddSprite(string spriteSheetName, string spriteName, 
            Rectangle spriteDimensions)
        {
            if (dSpriteSheets.ContainsKey(spriteSheetName))
            {
                dSpriteRects[spriteSheetName].Add(spriteName, spriteDimensions);
            }
        }

        // remove sprite
        public void RemoveSprite(string spriteSheetName, string spriteName,
            Rectangle spriteDimensions)
        {
            if (dSpriteSheets.ContainsKey(spriteSheetName) && 
                dSpriteRects.ContainsKey(spriteName))
            {
                dSpriteRects.Remove(spriteName);
            }
        }

        public void AddSpriteSheet(string spriteSheetName, Texture2D spriteSheet)
        {
            if (!dSpriteSheets.ContainsKey(spriteSheetName))
            {
                dSpriteSheets.Add(spriteSheetName, spriteSheet);
                dSpriteRects.Add(spriteSheetName, new Dictionary<string, Rectangle> ());
            }
        }

        // removeSpriteSheet and all associated sprites
        public void RemoveSpriteSheet(string spriteSheetName)
        {
            if (dSpriteSheets.ContainsKey(spriteSheetName))
            {
                dSpriteSheets.Remove(spriteSheetName);
                dSpriteRects.Remove(spriteSheetName);
            }
        }
    }
}