using bitmask;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using resource;
using System.Collections.Generic;
using System;
using tilemap;
using viewStuff;
using System.Linq;

namespace ECS 
{
    public class RenderingSystem
    {
        //const int WIN_WIDTH = 1440;
        //const int WIN_HEIGHT = 810;

        const int TARGET_WIDTH = 480; //320;
        const int TARGET_HEIGHT = 270; //180;

        public List<String> debugText;

        public Texture2D brickTex;
        public Texture2D normalTex;
        public Texture2D flatNormal;

        public Effect pixelShader = null;
        public SpriteFont font;


        // the virtual render target to which all things are
        // first draw before they are drawn to the screen.
        private RenderTarget2D renderCanvas;


        // getting display size (use this later)
        //int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        //int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        public TextureManager texMan;
        public GraphicsDeviceManager gMan;
        public Bitmask signature;
        public EntityManager eMan;

        public SpriteBatch spriteBatch;

        //public TilemapManager tilemapManager;
        //public Dictionary<string, Texture2D> textureMap; // temporary until I get a proper resource management class set up

        public RenderingSystem(EntityManager eMan, GraphicsDeviceManager gMan, TextureManager texMan)
        {
            //this.tileMan = tileMan;
            this.eMan = eMan;
            this.gMan = gMan;
            this.texMan = texMan;
            // this.AnimationManager = aMan; // Later...

            this.debugText = new List<String>();

            this.signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CTexture] = true;
            signature[ComponentType.CTransform] = true;

            renderCanvas = new RenderTarget2D(
                gMan.GraphicsDevice,
                TARGET_WIDTH, TARGET_HEIGHT,
                false, gMan.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            spriteBatch = new SpriteBatch(gMan.GraphicsDevice);

            //SamplerState sState = new SamplerState();

            //sState.AddressU = TextureAddressMode.Clamp;
            //sState.AddressV = TextureAddressMode.Clamp;
            //gMan.GraphicsDevice.SamplerStates[0] = sState;

            //SamplerState sState = new SamplerState() { Filter = TextureFilter.Point };
            //gMan.GraphicsDevice.SamplerStates[0] = sState;
        }


        //public void Render(Camera2D cam, Tilemap tilemap)
        public void Render(Camera2D cam, Tilemap tilemap)
        {
            gMan.GraphicsDevice.SetRenderTarget(renderCanvas);
            gMan.GraphicsDevice.Clear(Color.Black);

            //DrawTilemap(cam, tilemap);
            DrawTilemap(cam, tilemap);
            // tilemap drawing process
            // 1. draw background tiles
            // 2. draw tiles with greater or equal to height as entities and draw entities
            //    so that some entities can be drawn within the tiles
            DrawToCanvas(cam);

            // 2 perform post processing effects on the canvas
            // Post Processing();

            gMan.GraphicsDevice.SetRenderTarget(null);
            gMan.GraphicsDevice.Clear(Color.Black);
            // 3 draw to screen
            DrawToScreen(cam);
        }

        //private void DrawTilemapLayer(Camera2D cam, Tilemap tilemap, Dictionary<Vector2, int> layer, Texture2D textureAtlas)
        private void DrawTilemapLayer(Camera2D cam, Tilemap tilemap, Dictionary<Vector2, int> layer, Texture2D textureAtlas)
        {
            foreach (var item in layer)
            {
                Rectangle drect = new(
                    (int)item.Key.X * tilemap.tileDim,
                    (int)item.Key.Y * tilemap.tileDim,
                    tilemap.tileDim,
                    tilemap.tileDim);

                int x = item.Value % tilemap.atlasNumTilesPerRow;
                int y = item.Value / tilemap.atlasNumTilesPerRow;

                Rectangle source = new(
                    x * tilemap.tileDim,
                    y * tilemap.tileDim,
                    tilemap.tileDim,
                    tilemap.tileDim);

                spriteBatch.Draw(textureAtlas, drect, source, Color.White);
            }
        }


        /*
         * this removes draw functionality from the tilemap (goal)
         */
        private void DrawTilemap(Camera2D cam, Tilemap tilemap)
        {
            gMan.GraphicsDevice.SetRenderTarget(renderCanvas);

            //spriteBatch.Begin(samplerState: SamplerState.PointClamp,
            //    transformMatrix: cam.TransformMatrix);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                transformMatrix: cam.TransformMatrix, effect: pixelShader);

            pixelShader.CurrentTechnique = pixelShader.Techniques["LightEffect"];

            //Texture2D normalAtlas = textureManager.GetTexture(tilemap.normalAtlasId);
            //Texture2D textureAtlas = textureManager.GetTexture(tilemap.textureAtlasId);
            Texture2D textureAtlas = texMan.GetTexture(tilemap.textureAtlasId);

            // setting the normals for the tilemap as being flat for now so the light still affects them
            pixelShader.Parameters["NormalTexture"].SetValue(this.flatNormal);

            //DrawTilemapLayer(cam, tilemap, tilemap.GetLayer(Tilemap.LayerType.background), textureAtlas);
            //DrawTilemapLayer(cam, tilemap, tilemap.GetLayer(Tilemap.LayerType.midground), textureAtlas);
            //DrawTilemapLayer(cam, tilemap, tilemap.GetLayer(Tilemap.LayerType.foreground), textureAtlas);

            DrawTilemapLayer(cam, tilemap, tilemap.GetLayer(Tilemap.LayerType.background), textureAtlas);
            DrawTilemapLayer(cam, tilemap, tilemap.GetLayer(Tilemap.LayerType.midground), textureAtlas);
            DrawTilemapLayer(cam, tilemap, tilemap.GetLayer(Tilemap.LayerType.foreground), textureAtlas);

            spriteBatch.End();
        }


        private void DrawToCanvas(Camera2D cam)
        {
            List<Entity> ents = eMan.GetEntities(signature).ToList();
            CTexture[] textures = new CTexture[ents.Count];
            CTransform[] transforms = new CTransform[ents.Count];

            // get all necessary information into local arrays
            // (might be better for cache if they are used multiple times here,
            // not entirely sure yet)
            for (int i = 0; i < ents.Count; i++)
            {
                textures[i] = ((CTexture)eMan.GetComponent<CTexture>(ents[i].id));
                transforms[i] = ((CTransform)eMan.GetComponent<CTransform>(ents[i].id));
            }

            // DRAW ENTITIES
            spriteBatch.Begin(
                SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                transformMatrix: cam.TransformMatrix,
                effect: pixelShader);

            pixelShader.CurrentTechnique = pixelShader.Techniques["LightEffect"];
            pixelShader.Parameters["NormalTexture"].SetValue(normalTex);

            // TODO: LOAD IN THE ENTITY TEXTURE ATLAS HERE WHICH SHOULD CONTAIN ALL NECESSARY TEXTURES FOR ENTITIES
            Texture2D atlas = texMan.GetTexture("entity_tilesheet");

            for (int i = 0; i < ents.Count; i++)
            {
                Rectangle texRect = texMan.GetTextureRect(textures[i].textureId);
                // TODO: DRAW THE TEXTURE ATLAS HERE ALONGSIDE ITS CORRESPONDING RECTANGLE WHICH CAN BE GOTTEN FROM
                //       THE TEXTURE MANAGER

                spriteBatch.Draw(
                    atlas,
                    new Rectangle(
                        (int)transforms[i].X, (int)transforms[i].Y,
                        texRect.Width,
                        texRect.Height),
                    null,
                    Color.White
                    );

                //spriteBatch.Draw(
                //    atlas,
                //    new Rectangle(
                //        (int)transforms[i].X, (int)transforms[i].Y,
                //        texRect.Width,
                //        texRect.Height),
                //    null,
                //    Color.White,
                //    transforms[i].rotation,
                //    transforms[i].origin,
                //    SpriteEffects.None,
                //    0f
                //    );

                //spriteBatch.Draw(
                //    atlas,
                //    new Rectangle(
                //        (int)transforms[i].X, (int)transforms[i].Y,
                //        texWidth,
                //        texHeight),
                //    null,
                //    Color.White
                //    );

                //spriteBatch.Draw(
                //    brickTex,
                //    new Rectangle(
                //        (int)transforms[i].X, (int)transforms[i].Y,
                //        brickTex.Width, brickTex.Height),
                //    null,
                //    Color.White
                //    );

            }
            spriteBatch.End();
        }


        private void PostProcessing()
        {
            return;
        }


        private void DrawToScreen(Camera2D cam)
        {
            spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            spriteBatch.Draw(renderCanvas,
                new Rectangle(0, 0, gMan.PreferredBackBufferWidth, gMan.PreferredBackBufferHeight),
                new Rectangle(renderCanvas.Bounds.X, renderCanvas.Bounds.Y, renderCanvas.Bounds.Width, renderCanvas.Bounds.Height),
                Color.White);

            //spriteBatch.Draw(renderCanvas,
            //    new Rectangle(0, 0, gMan.PreferredBackBufferWidth, gMan.PreferredBackBufferHeight),
            //    Color.White);

            DrawDebugText();

            spriteBatch.End();
        }


        private void DrawDebugText()
        {
            for (int i = 0; i < debugText.Count; i++)
            {
                spriteBatch.DrawString(font, debugText[i], new Vector2(10, 10 + i * 40), Color.White);
            }
        }


        //private void SetShaderParameters()
        //{
        //    //pixelShader.Parameters["AmbientLightColor"].SetValue(new Vector3(0.3f, 0.3f, 0.3f));
        //    // set other values here that will be gleamed from the current scene

        //    // take point light locations and convert them to world coordinates

        //    return;
        //    // shader.ambientlight = scene.getlight
        //    // shader.pointlightpositions = scene.visibellights
        //}


    }
}

