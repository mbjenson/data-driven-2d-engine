using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using viewStuff;



namespace tilemap
{


    /*
    Tilemap in general

    All map files must follow a standard naming convension

    Layering and rendering
    The tilemap will be drawn from back to front, layer by layer. For the layer of the tilemap
    that is the same layer that the player is on, those items will be somehow put into the
    rendering system and rendered alongside all of the entities in the world so ordering
    is done correctly.

    1. draw background (something that could be below/behind the floor)
    2. draw floor (grass, stone, etc)
    3. draw interactable layer (the player and trees and rocks etc)
    4. draw foreground (things that are between the camera and the player)



    TODO:
        come up with naming convension so that by naming each layer in accordance to a naming convension
        and then exporting as CSV to the correct folder, the base name for the tilemap can be given and all other
        things inside of it can be loaded in without hard coding the layer names. Just take base map name
        and concatenate the convension layer names onto the end.
    */
    

    public class Tilemap
    {
        public int tileDim = 16;
        public int atlasNumTilesPerRow = 4;

        // tilemap layers loaded from file

        private Dictionary<LayerType, Dictionary<Vector2, int>> layers;
        //public List<Dictionary<Vector2, int>> layers;
        
        public string textureAtlasId; // string name of texture stored in resourcemanager
        public string normalAtlasId; // probably going to change this


        // this enum provides a more accesible way to concretely define which indecies within the layer
        // list correspond to which layer in the map, functionally.
        // currently used
        public enum LayerType // to be adjusted, I am not a huge fan of the way this is organized.
        {
            // background is like the floor (walking paths, stone, grass, etc)
            background,
            // midground is things that the player is level with (standing stones, trees, chests, etc)
            midground, 
            // represents the normal maps that are ascribed to each texture in the midground layer
            // instead of this, simply use another texture with all the normal map stuff on it (might be expensive)
            midground_normal, 
            // defines the collidable tiles in the tilemap. Right now I have this so that I can check if
            // a tile is collidable in O(1) time instead of checking against every tile. I think in the future I will have
            // the tilemap system hold onto this data(?) and make it available to the physics system upon load.
            collision,
            // foreground is something that is between the player and the camera.
            foreground,
            // number of layers in the tilemap
            Count
        }

        public Tilemap(string textureAtlasId, string normalAtlasId)
        {
            this.textureAtlasId = textureAtlasId;
            this.normalAtlasId = normalAtlasId;

            layers = new Dictionary<LayerType, Dictionary<Vector2, int>>((int)LayerType.Count);
            LoadLayer(LayerType.background, "../../../Content/MapData/map1/map1_background.csv");
            LoadLayer(LayerType.midground, "../../../Content/MapData/map1/map1_midground.csv");
            LoadLayer(LayerType.midground_normal, "../../../Content/MapData/map1/map1_midground_normal.csv");
            LoadLayer(LayerType.collision, "../../../Content/MapData/map1/map1_collision.csv");
            LoadLayer(LayerType.foreground, "../../../Content/MapData/map1/map1_foreground.csv");



            //layers = new List<Dictionary<Vector2, int>>((int)LayerType.COUNT);

            //layers = new Dictionary<string, Dictionary<Vector2, int>>();

            // now we can simply add a layer (in order) for the tilemap which are rendered in order
            // and the layer indecies correspond to the enum present above (not yet implemented)
            //layers.Add("midground", LoadMap("../../../Content/MapData/test-map/test-map_test-mg.csv"));
            //layers.Add(LoadMap("../../../Content/MapData/test-map/test-map_test-mg.csv"));

            // this must be done in correct order (I will automate this later using the map name + _layername or something like that).

            //layers.Add(LoadMap("../../../Content/MapData/map1/map1_background.csv"));
            //layers.Add(LoadMap("../../../Content/MapData/map1/map1_midground.csv"));
            //layers.Add(LoadMap("../../../Content/MapData/map1/map1_midground_normal.csv"));
            //layers.Add(LoadMap("../../../Content/MapData/map1/map1_collision.csv"));
            //layers.Add(LoadMap("../../../Content/MapData/map1/map1_foreground.csv"));


            //layers.Add(LoadMap("../../../Content/MapData/test-map/test-map_test-fg.csv"));

        }

        private void LoadLayer(LayerType layerType, string filepath)
        {
            if (!layers.ContainsKey(layerType))
            {
                layers.Add(layerType, LoadMap(filepath));
            }
        }


        public Dictionary<Vector2, int> GetLayer(LayerType layerType)
        {
            if (layers.ContainsKey(layerType))
            {
                return layers[layerType];
            }
            return null;
        }
        

        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();
            StreamReader reader = new(filepath);
            int y = 0;
            string line;

            while((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value)) 
                    {
                        if (value > -1)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }
                y++;
            }
            return result;
        }


        public bool isSolidAt(Vector2 pos)
        {
            if (layers[LayerType.collision] == null)
            {
                throw new Exception("Tilemap:isSolidAt(...) -> collision layer is null");
            }
            if (layers[LayerType.collision][pos] > 0)
            {
                return true;
            }
            return false;
        }


        //public bool isSolidAt(int x, int y)
        //{
        //    if (layers.ContainsKey("collisions"))
        //    {
        //        if (layers["collisions"][new Vector2(x, y)] > 0)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}




    }
}







// old draw function for tilemap that I want to keep around becuase the method was
// novel to me at the time I wrote it.

//public void Draw(SpriteBatch spriteBatch, Camera2D cam, 
//    RenderTarget2D target, GraphicsDevice graphicsDevice, Texture2D textureAtlas)
//{
//    //graphicsDevice.Clear(Color.CornflowerBlue);
//    graphicsDevice.SetRenderTarget(target);

//    spriteBatch.Begin(samplerState: SamplerState.PointClamp,
//        transformMatrix: cam.TransformMatrix);

//    int tileNumPixels = 16;

//    foreach (var item in mg)
//    {
//        Rectangle drect = new(
//            (int)item.Key.X * tileDim,
//            (int)item.Key.Y * tileDim,
//            tileDim,
//            tileDim);


//        int x = item.Value % atlasNumTilesPerRow;
//        int y = item.Value / atlasNumTilesPerRow;

//        Rectangle source = new(
//            x * tileNumPixels,
//            y * tileNumPixels,
//            tileNumPixels,
//            tileNumPixels);

//        spriteBatch.Draw(textureAtlas, drect, source, Color.White);
//    }

//    spriteBatch.End();
//}




// test comment from laptop

/**
 * Idea: don't  have a bunch of tilemap objects floating around in your game instead use
 *       a tilemap renderer which can take a tilemap object (which itself should load it's
 *       information from a JSON object), and properly render the information from it.
 *       This will decouple the tilemap structure from the actual rendering process
 */

/*
namespace tilemap
{
    public class TilemapManager
    {
        int[] tileTypes = new int[]
        {
            2, 8, 2, 8, 8, 8, 8, 8, 8, 8, 2, 8, 2, 2, 2, 8,
            2, 8, 2, 8, 8, 8, 8, 8, 8, 8, 2, 8, 2, 2, 2, 8,
            2, 8, 2, 8, 8, 8, 8, 8, 8, 8, 2, 8, 2, 2, 2, 8,
            2, 8, 2, 8, 8, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
            2, 4, 2, 4, 4, 4, 4, 4, 4, 4, 2, 4, 2, 2, 2, 4,
        };

        public Texture2D spriteSheet;
        public RenderTarget2D target;
        public SpriteBatch spriteBatch;

        private GraphicsDeviceManager gMan;

        public Vector2 tileDim;
        public Vector2 mapTileDim;

        public TilemapManager(Vector2 tileDim, Vector2 mapTileDim, Texture2D spriteSheet, GraphicsDeviceManager gMan)
        {
            this.spriteSheet = spriteSheet;
            this.tileDim = tileDim;
            this.mapTileDim = mapTileDim;

            this.gMan = gMan;

            this.spriteBatch = new SpriteBatch(gMan.GraphicsDevice);

            this.target = new RenderTarget2D(
                gMan.GraphicsDevice, (int)(mapTileDim.X * tileDim.X), (int)(mapTileDim.Y * tileDim.Y));
        }

        public void Update(Vector2 playerPos)
        {
            
            gMan.GraphicsDevice.SetRenderTarget(target);
            gMan.GraphicsDevice.Clear(Color.White);

            this.spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            for (int i = 0; i < tileTypes.Length; i++)
            {
                int x = i % (int)mapTileDim.X;
                int y = (i - x) / (int)mapTileDim.X;

                spriteBatch.Draw(
                    spriteSheet,
                    new Rectangle(
                        x * (int)tileDim.X, y * (int)tileDim.X, (int)tileDim.X, (int)tileDim.Y),
                    new Rectangle(
                        0, (int)(tileTypes[i] * tileDim.Y), (int)tileDim.X, (int)tileDim.Y),
                    Color.White);
            }

            this.spriteBatch.End();

        }

    }

}






*/

/*
public class TilemapRenderer
{
    public Tilemap tilemap;
    public RenderTarget2D mapCanvas;
    public SpriteBatch tileBatch;

    public TilemapRenderer(Tilemap tilemap, GraphicsDeviceManager graphicsDeviceManager)
    {
        this.tilemap = tilemap;

        tileBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);

        // init map render target
        mapCanvas = new RenderTarget2D(
            graphicsDeviceManager.GraphicsDevice,
            graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight,
            false,
            graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
    }

    public void render(GraphicsDevice gDevice)
    {
        gDevice.SetRenderTarget(mapCanvas);
        gDevice.Clear(Color.White);

        tileBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

        drawMapToTexture();

        tileBatch.End();

        gDevice.SetRenderTarget(null);
    }

    private void drawMapToTexture()
    {
        for (int row = 0; row < tilemap.mapHeight; row++)
        {
            for (int col = 0; col < tilemap.mapWidth; col++)
            {
                tileBatch.Draw(
                    tilemap.textureSheet,
                    new Rectangle(
                        col * tilemap.tileWidth,
                        row * tilemap.tileHeight,
                        tilemap.tileWidth,
                        tilemap.tileHeight),
                    new Rectangle(
                        0,
                        tilemap.tileTypes[row * tilemap.mapWidth + col] * tilemap.tileHeight,
                        tilemap.tileWidth,
                        tilemap.tileHeight),
                    Color.White);
            }
        }
    }




}
*/



// class logic:
/*
 * tilemap has its own spritebatch which it uses to draw to the rendertarget
 * 
 * when creating a new tilemap, a JSON file will be used to initialize all the values
 * that will be used for the tilemap such as the actual tilemap data, the dimensions
 * of the map, the size of each tile, etc.
*/

/**
 * Tilemap class handles storing information about tiles and drawing them to the screen
*/
/*
public class Tilemap
{
    public Texture2D textureSheet;
    public List<int> tileTypes;

    public int mapWidth;
    public int mapHeight;

    public int tileWidth;
    public int tileHeight;

    //public RenderTarget2D mapCanvas; // later use many dynamic map chunks to get most efficient rendering
    //private SpriteBatch tileSpriteBatch;

    // later change this to take JSON object information
    public Tilemap(int mapWidth, int mapHeight, int tileWidth, int tileHeight) 
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.tileWidth = tileWidth;
        this.tileHeight = tileHeight;

        tileTypes = new List<int>
        {
            3, 2, 4, 3, 3, 4, 4, 2, 4, 3, 2, 2, 4, 4, 2, 4,
            1, 3, 2, 2, 4, 3, 2, 1, 2, 2, 2, 2, 2, 1, 3, 4,
            2, 5, 5, 6, 4, 3, 4, 3, 2, 1, 1, 2, 3, 1, 2, 3,
            3, 2, 4, 3, 3, 4, 4, 2, 4, 3, 2, 2, 4, 4, 2, 4,
            1, 3, 2, 2, 4, 3, 2, 1, 2, 2, 2, 2, 2, 1, 3, 4,
            2, 5, 5, 6, 4, 3, 4, 3, 2, 1, 1, 2, 3, 1, 2, 3,
            3, 2, 4, 3, 3, 4, 4, 2, 4, 3, 2, 2, 4, 4, 2, 4,
            1, 3, 2, 2, 4, 3, 2, 1, 2, 2, 2, 2, 2, 1, 3, 4,
            2, 5, 5, 6, 4, 3, 4, 3, 2, 1, 1, 2, 3, 1, 2, 3,
            3, 2, 4, 3, 3, 4, 4, 2, 4, 3, 2, 2, 4, 4, 2, 4,
            1, 3, 2, 2, 4, 3, 2, 1, 2, 2, 2, 2, 2, 1, 3, 4,
            2, 5, 5, 6, 4, 3, 4, 3, 2, 1, 1, 2, 3, 1, 2, 3,
            3, 2, 4, 3, 3, 4, 4, 2, 4, 3, 2, 2, 4, 4, 2, 4,
            1, 3, 2, 2, 4, 3, 2, 1, 2, 2, 2, 2, 2, 1, 3, 4,
            2, 5, 5, 6, 4, 3, 4, 3, 2, 1, 1, 2, 3, 1, 2, 3,
            3, 2, 4, 3, 3, 4, 4, 2, 4, 3, 2, 2, 4, 4, 2, 4,
        };
    }




    // Draw to the texture once
    // then draw the texture to the screen using Game1's spritebatch

}
}
*/

// tyring to draw tilemap to it's own render texture
//if (!tilemap.isDrawn)
//{
//    GraphicsDevice.SetRenderTarget(tilemap.mapCanvas);
//    graphics.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
//    graphics.GraphicsDevice.Clear(Color.CadetBlue);

//    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
//        SamplerState.PointClamp);

//    tilemap.Draw(spriteBatch);

//    spriteBatch.End();
//    GraphicsDevice.SetRenderTarget(null);
//    tilemap.isDrawn = true;
//}
