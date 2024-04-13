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
using System.Reflection.Metadata.Ecma335;
using System.Linq;
using Microsoft.VisualBasic;

/*
 * 
=======================================
NOTES:
=======================================

Smoother camera:
    to implement a very smooth camera (not pixel perfect), you have to 
    have the camera not exist within the pixel world but outside of it. So after you render
    everything in the game to the 320x180 render target, instead of rendering everything to that render target
    under the camera's matrix transform, render the world as it would be to the texture, then move
    the actual camera's view to be looking at the correct spot on the texture (where the player is).
    note that this only involve 1 matrix multiplication and some other transformations rather than transforming every
    vertex by the camera (this may help performance if that was ever an issue). Then you can update the camera based on
    the player's currect position and that way the camera can go between pixels in floating point world coordinates
    rather than only existing in the pixel grid. Now there are benefits to both and I am still leaning
    towards sticking to the pixel perfect camera to really sell the retro look for the game.

Entity Component System:
    The entity component system will ideally look something like this in practice:
        
        Entity entity = new Entity();
        entity.addComponent<Physics.RigidBody>(new RigidBody); // physics properties for object (rigidbody contains velocity and acceleration)

    The basic idea is to allow for more centralized management of entities and remove the use
    of inheritance bounded, class based, and coupled system. This will allow for dynamic creation of entities.
    Take the following example for how I would implement a player with this code:
    
        Entity player = new Entity();
        player.addComponent<RigidBody>(new RigidBody()); // physics properties for object (rigidbody contains velocity and acceleration)
        player.addComponent<Controller>(new Controller()); // allows the user to control the entity
        player.addComponent<Animation>(new Animation(loaded acesprite file)); // display animation on entity
        player.addComponent<Transform>(new Transform()); // gives entity position, rotation, and scale
        // this code would be put in to initialize an entity called player
        
        // similarly more entities could be added using the same principle
        List<Entity> enemies(a number of entities);
        //... load enemies from some external file ...
        for (int i = 0; i < numEnemies; i++)
        {
            enemies[i] = new Entity();
            enemies[i].addComponent<RigidBody>(new RigidBody()); // physics information
            enemies[i].addComponent<Transform>(new Transform(initialized according to some pre-existing data)); 
            enemies[i].addComponent<Animation>(new Animation(some acesprite file or something)); 
            enemies[i].addComponent<MovementInfo>(new MovementInfo(info about movement speed, etc)); 
            enemies[i].addComponent<HealthInfo>(new HealthInfo(info about totalLife, etc)); 
            enemies[i].addComponent<MovementStrategy>(new MovementStrategy(how the entity moves in the world)); 
            enemies[i].addComponent<Collider>(new RectCollider(dimensions according to enemy specs)); 
        }
        // the above is just a rough outline, still subject to changing (hopefully it changes)

    Hopefully this pretty much shows the type of system I am going for with this project. One idea
    that I have is that one might have several lists of entities, each of which contains entities
    with different components. For example, one might have things related to particle physics, 
    while the others might be related to how a wooden crate exists in the world.

    Different kinds of managing systems will need to be implemented for this to work properly.
    For example, to manage collisions between objects, a quadtree can be implemented which can store the 
    entity IDs (to be more efficient) and then using this, collision_manager.update(deltaTime) can be
    called to solve all of the collisions with entities that use such colliders.

    Still, I am not sure how one would divide up such tasks becuase certainly there are some tasks that
    would be done on standard entities like player and enemy that would not be done on the particles. I am
    leaning towards just using lists of things which can be updated using the different managers to update
    the different components in the system.
        
        
=======================================
TODO
=======================================

[] Entity Component System
    current: test the transform system (test system)
    
    - game context
        - implement texture container to store textures so entities can simply store a texture id and not an actual texture

[] implement lights

    - implement a directional light (like the sun)

    - implement colored light

    - implement some kind of normal mapping for the objects
        so that light in 2d interacts with them how it would in 3D

[] implement shadows
    
    - real time shadow casting with objects in the world

    - entities have real time shadows that are cast which resemble their actual texture shape

[] Get Animated Tiles and entities working.
    
    - implement a nice animation system which simply takes in the acesprite file
        and automatically sets up the animation without manual input
    
    - allow for animated tiles in the tilemap which will require a standard tickrate for the
        map in which the animated tiles only (or everything idk yet) will be redrawn to match
        the current frame of animation

[] (later): Use a physics based system to solve collisions between different kinds of physical
    objects using things like acceleration, mass, etc. (long term don't use the CollisionSolver
    class to solve collisions).


*/


/*
Texture Manager

first load in textures to texture manager (which will live in game context I believe)
then give access to the other parts of the context so they can reference the textures



*/






namespace Monogame_Test_Project
{
    public class Game1 : Game
    {
        Camera2D cam;

        const int WIN_WIDTH = 1440;
        const int WIN_HEIGHT = 810;

        const int TARGET_WIDTH = 320; // 480;
        const int TARGET_HEIGHT = 180; // 270;

        public List<RectCollider> rects;
        public List<CircleCollider> circles;

        RectCollider player;
        float moveSpeed = 70f;

        float theta = 0f;

        CollisionSolver solver;

        Texture2D rectTexture;
        Texture2D circleTexture;

        Tilemap tilemap;
        TilemapRenderer tilemapRenderer;

        Vector2 worldMousePos;

        Effect spriteEffect;

        RenderTarget2D renderCanvas;

        //Entity playerEntity;
        Context context;


        SpriteSheetManager spriteMan;
        

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            GraphicsDevice.Viewport = new Viewport(0, 0, TARGET_WIDTH, TARGET_HEIGHT);

            cam = new Camera2D(GraphicsDevice.Viewport);

            graphics.PreferredBackBufferWidth = WIN_WIDTH;
            graphics.PreferredBackBufferHeight = WIN_HEIGHT;
            
            graphics.ApplyChanges();

            renderCanvas = new RenderTarget2D(
                GraphicsDevice,
                TARGET_WIDTH, TARGET_HEIGHT,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            player = new RectCollider(0, 0, 16f, 16f);

            circles = new List<CircleCollider> { 
                new (60f, 60f, 16f),
                new (90f, 90f, 16f)};

            rects = new List<RectCollider> {
                new (40f, 100f, 16f, 16f),
                new (200f, 50f, 16f, 16f)};

            solver = new CollisionSolver();


            tilemap = new Tilemap(16, 16, 16, 16);
            tilemap.tileTypes = new List<int>{
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 3, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 3, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 3, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,};

            tilemapRenderer = new TilemapRenderer(tilemap, graphics);

            context = new Context(25);
            int pEntity = context.CreateEntity();
            context.AddComponent<CTransform>(pEntity);
            context.AddComponent<CRigidBody>(pEntity, new CRigidBody());

            spriteMan = new SpriteSheetManager();
            

            //dTextures = new Dictionary<string, Texture2D>();
            //dTextures.Add("dirt", Content.Load<Texture2D>);


            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here

            spriteEffect = Content.Load<Effect>("spriteShader");

            rectTexture = Content.Load<Texture2D>("dirt");
            circleTexture = Content.Load<Texture2D>("test-ball");

            tilemap.textureSheet = Content.Load<Texture2D>("tilesheet");

            spriteMan.AddSpriteSheet("tilesheet", Content.Load<Texture2D>("tilesheet"));
            spriteMan.AddSprite("tilesheet", "dirt", new Rectangle(0, 5 * 16, 16, 16));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // TODO: Add your update logic


            



            // translate the world from it's ratio across the screen to the same ratio but across the renderTarget2D
            Vector2 viewportMousePos = new Vector2(
                ((float)Mouse.GetState().X / (float)WIN_WIDTH) * (float)TARGET_WIDTH,
                ((float)Mouse.GetState().Y / (float)WIN_HEIGHT) * (float)TARGET_HEIGHT);

            // transform the mouse position into the actual position that it has on the screen after the camera translate has been done
            worldMousePos = cam.screenToWorld(viewportMousePos);

            float xDif = worldMousePos.X - player.position.X;
            float yDif = worldMousePos.Y - player.position.Y;

            theta = (float)Math.Atan2(yDif, xDif);

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.W))
            {
                player.setPosition(player.getPosition() + new Vector2(0, -moveSpeed * dt));
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                player.setPosition(player.getPosition() + new Vector2(0, moveSpeed * dt));
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                player.setPosition(player.getPosition() + new Vector2(-moveSpeed * dt, 0));
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                player.setPosition(player.getPosition() + new Vector2(moveSpeed * dt, 0));
            }
            
            // round player position so that it exists only within whole numbered coordinates (removes texture distortion)
            player.position = Vector2.Round(player.position); // IMPORTANT For pixel perfect camera to not bug out

            // check for collision with the rectangle colliders
            foreach (var rect in rects)
            {
                if (solver.checkCollision(player, rect))
                {
                    solver.solveCollision(player, rect);
                }
            }

            foreach (var circle in circles)
            {
                if (solver.checkCollision(player, circle))
                {
                    solver.solveCollision(player, circle);
                }
            }

            cam.Update(player.position, dt);


            /*
            if (keyState.IsKeyDown(Keys.W))
            {
                cam.Position = cam.Position + new Vector2(0, -moveSpeed * dt);
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                cam.Position = cam.Position + new Vector2(0, moveSpeed * dt);
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                cam.Position = cam.Position + new Vector2(-moveSpeed * dt, 0);
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                cam.Position = cam.Position + new Vector2(moveSpeed * dt, 0);
            }
            */

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // render tilemap to render texture

            tilemapRenderer.render(graphics.GraphicsDevice);

            // TODO: Add your drawing code here
            graphics.GraphicsDevice.SetRenderTarget(renderCanvas);
            graphics.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue); // clear canvas
            // draw to render target
            spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointClamp, transformMatrix: cam.TransformMatrix);

            spriteBatch.Draw(
                tilemapRenderer.mapCanvas, 
                new Rectangle(0, 0, tilemapRenderer.mapCanvas.Width, tilemapRenderer.mapCanvas.Height),
                tilemapRenderer.mapCanvas.Bounds,
                Color.White);

            //spriteBatch.Draw(
            //    //playerEntity.GetComponent<TextureComponent>().texture,
            //    //playerEntity.GetComponent<TransformComponent>().position,
            //    Color.White);
            
            foreach (var rect in rects)
            {
                spriteBatch.Draw(
                    rectTexture,
                    new Rectangle((int)rect.position.X, (int)rect.position.Y, 16, 16),
                    null,
                    Color.White,
                    0f,
                    new Vector2(8f, 8f),
                    SpriteEffects.None,
                    0f);
            }

            foreach (var circle in circles)
            {
                spriteBatch.Draw(
                    circleTexture,
                    new Rectangle(
                        (int)(circle.position.X - circle.radius), 
                        (int)(circle.position.Y - circle.radius), 
                        (int)circle.radius * 2, (int)circle.radius * 2), 
                    null, 
                    Color.White, 
                    0f, 
                    new Vector2(8, 8), 
                    SpriteEffects.None, 0f);
            }

            spriteEffect.CurrentTechnique.Passes[0].Apply(); // effect for player texture

            spriteBatch.Draw(
                spriteMan.dSpriteSheets["tilesheet"],
                player.getPosition(),
                spriteMan.dSpriteRects["tilesheet"]["dirt"],
                Color.White,
                0f,
                new Vector2(8f, 8f),
                1f,
                SpriteEffects.None,
                0f);

            //spriteBatch.Draw(
            //     rectTexture,
            //     player.getPosition(),
            //     null,
            //     Color.White,
            //     0f, // theta
            //     new Vector2(8f, 8f),
            //     1f,
            //     SpriteEffects.None,
            //     0f);
            
            spriteBatch.End();

            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
            //    SamplerState.PointClamp, DepthStencilState.Default,
            //    RasterizerState.CullNone);

            spriteBatch.Begin(
                SpriteSortMode.Immediate, 
                BlendState.AlphaBlend,
                SamplerState.PointClamp, 
                DepthStencilState.Default,
                RasterizerState.CullNone);

            // draw to actual window
            spriteBatch.Draw(
                renderCanvas,
                new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
