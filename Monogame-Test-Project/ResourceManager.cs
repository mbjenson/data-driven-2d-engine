using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace resource {

    
    // public class AnimationManager {} // Later...

    
    public class TextureManager
    {
        private Dictionary<string, Texture2D> textures;
        private Dictionary<string, Rectangle> textureRects;
        
        public TextureManager()
        {
            textureRects = new();
            textures = new();
        }

        public void AddTexture(string name, Texture2D newTexture)
        {
            textures.Add(name, newTexture);
        }

        public Texture2D GetTexture(string name)
        {
            Debug.Assert(textures.ContainsKey(name), "TextureManager: does not contain texture with name: '" + name + "'. Aborting");
            return textures[name];
        }

        /*
         * Add named texture rect into which corresponds to a piece of the texture atlas
         */
        public void AddTextureRect(string name, Rectangle rect)
        {
            textureRects.Add(name, rect);
        }

        public Rectangle GetTextureRect(string name)
        {
            Debug.Assert(textureRects.ContainsKey(name), "TextureManager: GetTextureRect(name) name: " + name + ". TextureManager" +
                "does not contain texture rect with that name");
            return textureRects[name];
        }
    }
}




//namespace ECS
//{






//    /*
//        Redo this class, it is unintuitive and does not really help in the long term
//        make it so that textures can be accessed without having to lookup the corresponding texture sheet that it lives on(?)
//        The goal is to use sprite sheets so that few textures need to be loaded onto the GPU at a time becuase it is
//        very expensive. So related things can live on the same sprite sheet (materials, entities, etc)
//    */


//    /*
//    the general idea is to have a texture that has a bunch of rects paired with it that 
//    represent where sprites are located within a large texture sheet
//     */
//    public class SpriteSheetManager
//    {
//        //              spriteSheet name, texture
//        public Dictionary<string, Texture2D> dSpriteSheets;
//        //              spriteSheet name,      sprite name, dimensions
//        public Dictionary<string, Dictionary<string, Rectangle>> dSpriteRects;
//        // one will add textures to the dTextures then add the corresponding item 
//        // dimensions which live within that texture 
        
//        public SpriteSheetManager()
//        {
//            dSpriteSheets = new Dictionary<string, Texture2D> ();
//            dSpriteRects = new Dictionary<string, Dictionary<string, Rectangle>> ();
//        }

//        // add sprite associated with a spritesheet
//        public void AddSprite(string spriteSheetName, string spriteName, 
//            Rectangle spriteDimensions)
//        {
//            if (dSpriteSheets.ContainsKey(spriteSheetName))
//            {
//                dSpriteRects[spriteSheetName].Add(spriteName, spriteDimensions);
//            }
//        }

//        // remove sprite
//        public void RemoveSprite(string spriteSheetName, string spriteName,
//            Rectangle spriteDimensions)
//        {
//            if (dSpriteSheets.ContainsKey(spriteSheetName) && 
//                dSpriteRects.ContainsKey(spriteName))
//            {
//                dSpriteRects.Remove(spriteName);
//            }
//        }


//        public void AddSpriteSheet(string spriteSheetName, Texture2D spriteSheet)
//        {
//            if (!dSpriteSheets.ContainsKey(spriteSheetName))
//            {
//                dSpriteSheets.Add(spriteSheetName, spriteSheet);
//                dSpriteRects.Add(spriteSheetName, new Dictionary<string, Rectangle>());
//            }
//        }

//        // removeSpriteSheet and all associated sprites
//        public void RemoveSpriteSheet(string spriteSheetName)
//        {
//            if (dSpriteSheets.ContainsKey(spriteSheetName))
//            {
//                dSpriteSheets.Remove(spriteSheetName);
//                dSpriteRects.Remove(spriteSheetName);
//            }
//        }


//        public Texture2D getSpriteSheet(string spriteSheetName)
//        {
//            return dSpriteSheets[spriteSheetName];
//        }


//        public Rectangle getSpriteRect(string spriteSheetName, string spriteName)
//        {
//            return dSpriteRects[spriteSheetName][spriteName];
//        }


//    }



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
//}