using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monogame_Test_Project;
using System.Collections.Generic;


/**
 * Idea: don't  have a bunch of tilemap objects floating around in your game instead use
 *       a tilemap renderer which can take a tilemap object (which itself should load it's
 *       information from a JSON object), and properly render the information from it.
 *       This will decouple the tilemap structure from the actual rendering process
 */
namespace tilemap
{
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
        }

        /*
        public void RenderToTarget(ref GraphicsDevice device)
        {
            // function that renders to the render texture
            device.SetRenderTarget(mapCanvas);
            tileSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            for (int row = 0; row < mapHeight; row++)
            {
                for (int col = 0; col < mapWidth; col++)
                {
                    tileSpriteBatch.Draw(
                        textureSheet,
                        new Rectangle(col * tileWidth, row * tileHeight, tileWidth, tileHeight),
                        new Rectangle(0, tileTypes[row * mapWidth + col] * tileHeight, tileWidth, tileHeight),
                        Color.White);
                }
            }
            tileSpriteBatch.End();
            device.SetRenderTarget(mapCanvas);
        }
        */

        /*
        public void Draw(SpriteBatch batch)
        {
            //Game1.graphics.GraphicsDevice.SetRenderTarget(mapCanvas);

            //tileSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            for (int row = 0; row < mapHeight; row++)
            {
                for (int col = 0; col < mapWidth; col++)
                {
                    batch.Draw(
                        textureSheet,
                        new Rectangle(col * tileWidth, row * tileHeight, tileWidth, tileHeight),
                        new Rectangle(0, tileTypes[row * mapWidth + col] * tileHeight, tileWidth, tileHeight),
                        Color.White);
                }
            }

            //tileSpriteBatch.End();

            //Game1.graphics.GraphicsDevice.SetRenderTarget(null);
        }
        */

        // Draw to the texture once
        // then draw the texture to the screen using Game1's spritebatch
        /*
        public void Render()
        {
            if (!isDrawn)
            {
                drawToRenderTarget();
                isDrawn = true;
            }

        }
        */
    }
}


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