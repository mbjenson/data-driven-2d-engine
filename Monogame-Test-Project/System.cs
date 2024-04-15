


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using viewStuff;


namespace ECS
{

    public abstract class UpdateSystem
    {
        protected GameContext context;
        public abstract void Update(GameTime gameTime);
    }



    public class InputSystem : UpdateSystem
    {
        //public KeyboardState keyState;
        public InputSystem(GameContext context) { this.context = context; }

        public override void Update(GameTime gameTime)
        {
            //KeyboardState keyState = Keyboard.GetState();

            //List<CController> controllers =
            //    context.GetComponentsOfType<CController>().Cast<CController>().ToList();

            

            //foreach (var controller in controllers)
            //{
            //    controller.buttonMap[]
            //}
            
        }
    }



    public class TransformSystem : UpdateSystem
    {
        public TransformSystem(GameContext context) 
        {
            this.context = context;
        }

        public override void Update(GameTime gameTime)
        {
            List<CTransform> transforms = 
                context.GetComponentsOfType<CTransform>().Cast<CTransform>().ToList();

            foreach (var transform in transforms)
            {
                float x = (float)Math.Sin(gameTime.ElapsedGameTime.TotalSeconds);
                float y = (float)Math.Cos(gameTime.ElapsedGameTime.TotalSeconds);
                transform.position += new Vector2(x, y);
            }
        }
    }


    // not an UpdateSystem (some other type)
    public class RenderingSystem
    {
        GameContext context;

        SpriteBatch spriteBatch;

        public RenderingSystem(GameContext context, GraphicsDevice device)
        {
            this.context = context;
            spriteBatch = new SpriteBatch(device);
        }

        // does all of the rendering. Right now only works with textures
        public void Render(
            GameTime gameTime, RenderTarget2D target, 
            GraphicsDevice device, Camera2D cam)
        {
            List<CTexture2D> textures = 
                context.GetComponentsOfType<CTexture2D>().Cast<CTexture2D>().ToList();

            // prep device
            device.SetRenderTarget(target);
            device.DepthStencilState = 
                new DepthStencilState() { DepthBufferEnable = true };
            device.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointClamp, transformMatrix: cam.TransformMatrix);

            foreach (var thisTexture in textures)
            {
                CTransform thisTransform = 
                    (CTransform)context.GetComponent<CTransform>(thisTexture.entityId);

                if (thisTransform == null)
                {
                    continue;
                }

                spriteBatch.Draw(
                    context.spriteMan.dSpriteSheets[thisTexture.spriteSheetId],
                    thisTransform.position,
                    context.spriteMan.dSpriteRects
                        [thisTexture.spriteSheetId][thisTexture.spriteId],
                    Color.White,
                    thisTransform.rotation,
                    thisTexture.textureOffset,
                    thisTransform.scale,
                    SpriteEffects.None,
                    thisTransform.layerDepth
                );
            }

            spriteBatch.End();

            device.SetRenderTarget(null);
            device.Clear(Color.CornflowerBlue);
        }
    }
    


}


