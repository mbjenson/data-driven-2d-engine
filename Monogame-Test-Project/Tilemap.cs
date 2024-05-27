using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;








/**
 * Idea: don't  have a bunch of tilemap objects floating around in your game instead use
 *       a tilemap renderer which can take a tilemap object (which itself should load it's
 *       information from a JSON object), and properly render the information from it.
 *       This will decouple the tilemap structure from the actual rendering process
 */
namespace tilemap
{
    /*
    TilemapManager
        * manages the tile map pieces
        * goal is to ensure that every single tile is not drawn every frame
    */
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
