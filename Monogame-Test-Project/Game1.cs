using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Collisions;
using viewStuff;
using tilemap;
using System.IO.MemoryMappedFiles;
using ECS;
using ECS.Systems;
using System.Reflection.Metadata.Ecma335;
using System.Linq;
using Microsoft.VisualBasic;
using bitmask;
using System.Net.Sockets;
using Microsoft.Xna.Framework.Content;
using resource;
using System.Text.Json;



namespace Monogame_Test_Project
{

    public class Game1 : Game
    {
        
        const int WIN_WIDTH = 1920;
        const int WIN_HEIGHT = 1080;

        const int TARGET_WIDTH = 480; //320;
        const int TARGET_HEIGHT = 270; //180; 

        Camera2D cam;

        Vector2 worldMousePos;
        Vector2 viewportMousePos;
        
        Vector2 player = Vector2.Zero;

        Vector2 entitySize = new Vector2(32f, 32f);

        float totalGameTime = 0f;

        float framesPerSecond = 0f;
        float secondsCounter = 0f;
        int numFrames = 0;

        float moveSpeed = 100f;

        EntityManager eMan;

        PhysicsSystem pSys;
        ActionSystem aSys;
        InputSystem iSys;
        LightingSystem lSys;
        AnimationSystem animSys;
        TilemapManager tileMan;

        RenderingSystem renderer;
        TextureManager texMan;

        Entity pEnt;
        Vector2 playerPos;

        private GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            //IsFixedTimeStep = false; // unlocks fps
            IsFixedTimeStep = true; // lock at 60fps

            // init entities
            int numEnts = 12;
            eMan = new EntityManager(numEnts);
            texMan = new TextureManager();

            tileMan = new TilemapManager(eMan,
                new Tilemap("atlas-dev", "normal-atlas-dev", "map-dev"));
            iSys = new InputSystem(eMan);
            aSys = new ActionSystem(eMan);
            animSys = new AnimationSystem(eMan);
            pSys = new PhysicsSystem(eMan, tileMan);


            cam = new Camera2D(new Viewport(0, 0, TARGET_WIDTH, TARGET_HEIGHT));

            cam.Zoom = 0.4f;
            renderer = new RenderingSystem(eMan, graphics, texMan);

            graphics.PreferredBackBufferWidth = WIN_WIDTH;
            graphics.PreferredBackBufferHeight = WIN_HEIGHT;
            graphics.ApplyChanges();
            // at this point the viweport is set to win width win height
            // ]

            // player entitiy controlled by an xbox, playerstation, or similar controller
            pEnt = eMan.CreateEntity();
            eMan.AddComponent<CController>(pEnt, new CController(PlayerIndex.One));
            eMan.AddComponent<CTransform>(pEnt, new CTransform(new Vector2(0, 0)));
            eMan.AddComponent<CRigidBody>(pEnt, new CRigidBody() { mass = 5f });
            eMan.AddComponent<CCollider>(pEnt, new CRectCollider(entitySize));
            eMan.AddComponent<CTexture>(pEnt, new CTexture("brick"));
            eMan.AddComponent<CPointLight>(pEnt, new CPointLight(100.0f, new Vector3(0.0f, 1.0f, 0.0f), new Vector2(16, 16)));

            // Example entities
            Entity lightBlock = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(lightBlock, 
                new CTransform() { position = new Vector2(-40f, 10f) });
            eMan.AddComponent<CCollider>(lightBlock, 
                new CRectCollider(entitySize));
            eMan.AddComponent<CRigidBody>(lightBlock, 
                new CRigidBody() { mass = 2.5f });
            eMan.AddComponent<CTexture>(lightBlock, 
                new CTexture("brick"));
            eMan.AddComponent<CPointLight>(lightBlock,
                new CPointLight(100.0f, new Vector3(3.0f, 0.0f, 0.0f), new Vector2(16, 16)));

            Entity heavyBlock = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(heavyBlock, 
                new CTransform() { position = new Vector2(40f, 40f) });
            eMan.AddComponent<CCollider>(heavyBlock, 
                new CRectCollider(entitySize));
            eMan.AddComponent<CRigidBody>(heavyBlock, 
                new CRigidBody() { mass = 10f });
            eMan.AddComponent<CTexture>(heavyBlock,
                new CTexture("brick"));
            eMan.AddComponent<CPointLight>(heavyBlock,
                new CPointLight(100.0f, new Vector3(0.0f, 0.0f, 3.0f), new Vector2(16, 16)));

            Entity ent3 = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(ent3, new CTransform() { position = new Vector2(-20f, -40f) });
            eMan.AddComponent<CPointLight>(ent3, 
                new CPointLight(100f, new Vector3(3.0f, 3.0f, 0.0f), new Vector2(16, 16)));
            eMan.AddComponent<CTexture>(ent3,
                new CTexture("brick"));
            eMan.AddComponent<CRigidBody>(ent3,
                new CRigidBody() { mass = 10f });
            eMan.AddComponent<CCollider>(ent3,
                new CRectCollider(entitySize));

            Entity ent4 = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(ent4, 
                new CTransform() { position = new Vector2(-20f, -80f) });
            eMan.AddComponent<CPointLight>(ent4, 
                new CPointLight(100f, new Vector3(3.0f, 3.0f, 0.0f), new Vector2(16, 16)));
            eMan.AddComponent<CTexture>(ent4,
                new CTexture("brick"));
            eMan.AddComponent<CRigidBody>(ent4,
                new CRigidBody() { mass = 10f });
            eMan.AddComponent<CCollider>(ent4,
                new CRectCollider(entitySize));

            Entity ent5 = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(ent5, new CTransform() { position = new Vector2(20f, 200f) });
            eMan.AddComponent<CPointLight>(ent5, 
                new CPointLight(100f, new Vector3(1.0f, 1.0f, 0.0f), new Vector2(16, 16)));
            eMan.AddComponent<CTexture>(ent5,
                new CTexture("brick"));
            eMan.AddComponent<CRigidBody>(ent5,
                new CRigidBody() { mass = 10f });
            eMan.AddComponent<CCollider>(ent5,
                new CRectCollider(entitySize));

            Entity e6 = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(e6, new CTransform(new Vector2(100f, 100f)));
            //eMan.AddComponent<CStaticBody>(e6, new CStaticBody());
            eMan.AddComponent<CTexture>(e6, new CTexture("brick"));
            eMan.AddComponent<CCollider>(e6, new CRectCollider(32f, 32f));

            

            base.Initialize();
        }


        protected override void LoadContent()
        {
            
            texMan.AddTexture("atlas-dev", Content.Load<Texture2D>("textures/atlas-dev"));
            texMan.AddTexture("normal-atlas-dev", Content.Load<Texture2D>("textures/normal-atlas-dev"));
            texMan.AddTexture("entity_tilesheet", Content.Load<Texture2D>("textures/smooth-brick"));
            // this type of information could be stored inside of json file
            texMan.AddTextureRect("brick", new Rectangle(0, 0, 32, 32)); 

            renderer.normalTex = Content.Load<Texture2D>("textures/smooth-brick-normal");
            renderer.font = Content.Load<SpriteFont>("type-face");
            renderer.pixelShader = Content.Load<Effect>("LightSpriteEffect");
            renderer.brickTex = Content.Load<Texture2D>("textures/smooth-brick");
            renderer.flatNormal = Content.Load<Texture2D>("textures/FlatNormal");

            // 12 is the number of lights the shader is expecting
            lSys = new LightingSystem(eMan, 12, renderer.pixelShader);
            
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            totalGameTime += dt;
            secondsCounter += dt;
            numFrames += 1;

            if (secondsCounter > 0)
            {
                framesPerSecond = numFrames / secondsCounter;
                secondsCounter = 0f;
                numFrames = 0;
            }

            // translate the world from it's ratio across the screen to the same ratio but across the renderTarget2D
            viewportMousePos = new Vector2(
                (float)Mouse.GetState().X / (float)graphics.GraphicsDevice.Viewport.Width * (float)TARGET_WIDTH,
                (float)Mouse.GetState().Y / (float)graphics.GraphicsDevice.Viewport.Height * (float)TARGET_HEIGHT);
            // transform the mouse position into the actual position that it has on the screen after the camera translate has been done
            worldMousePos = cam.screenToWorld(viewportMousePos);

            //float xDif = worldMousePos.X - playerPos.X;
            //float yDif = worldMousePos.Y - playerPos.Y;
            //theta = (float)Math.Atan2(yDif, xDif);

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.W))
            {
                player = player + new Vector2(0, -moveSpeed * dt);
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                player = player + new Vector2(0, moveSpeed * dt);
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                player = player + new Vector2(-moveSpeed * dt, 0);
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                player = player + new Vector2(moveSpeed * dt, 0);
            }

            if (keyState.IsKeyDown(Keys.G))
            {

            }

            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
            {
                CRigidBody rig = (CRigidBody)eMan.GetComponent<CRigidBody>(pEnt.id);

                if (rig.velocity.LengthSquared() > 0.00001f)
                {
                    rig.velocity += new Vector2(20f, 20f) * Vector2.Normalize(rig.velocity);
                }
            }

            cam.SmoothZoom(1.0f, 4f, dt);

            iSys.Update(gameTime);
            aSys.Update(gameTime);
            pSys.Update(gameTime);
            animSys.Update(gameTime);

            CTransform pTrans = (CTransform)eMan.GetComponent<CTransform>(pEnt.id);
            //CRigidBody pRig = (CRigidBody)eMan.GetComponent<CRigidBody>(pEnt.id);
            playerPos = pTrans.position;
            playerPos += new Vector2(16, 16);

            // set debug text for renderer
            renderer.debugText = new List<string> {
                "viewport: " + + graphics.GraphicsDevice.Viewport.Width + ", " + graphics.GraphicsDevice.Viewport.Height,
                "fps: " + Math.Round(framesPerSecond, 2),
                "cam pos: " + Math.Round(cam.Position.X, 1) +
                ", " + Math.Round(cam.Position.Y, 1),
            };

            //cam.Update(player, dt);
            cam.Update(playerPos, dt);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            lSys.SetShaderParameters(cam);
            renderer.Render(cam, tileMan.tilemap);
            base.Draw(gameTime);
        }
    }
}

