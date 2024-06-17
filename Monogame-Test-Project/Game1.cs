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


/*
    Merge dev branch into master
        1. checkout master
        2. right click on the dev branch you wish to merge into master
           and you will see the option to merge 'dev-branch-name' into 'master', click on this
           and there merging process will take place.
        3. then, in order to actually get that to sync with github on the master branch,
           you must then push all of these changes under "outgoing" in the master 
           branch like you would any other change
    
*/


/*
Explanations:

    Tilemap not in ECS:
        I chose not to make the tilemap a part of the ECS becuase it did not
        seem right to flood the ECS with all the tiles.
        Instead the tilemap is a different thing. This, however, does bring
        up several questions. Firstly, I want to be able to use TILED to create
        the maps and also create functionality for the tilemap through it like
        solid blocks and other things like water or lava for example. This means
        that certain parts of the tilemap must be put into the entity component
        system. To do that I am going to give the tilemap some functions which
        convert the data inside of itself into entities which the physics
        and other systems connected to the ECS can see.
*/

/*
=======================================
IDEAS:
=======================================

potential idea for Destructable items:
    the destructable component will have a counter which determines which 
    stage of destruction a thing is at and will be linked to a sprite sheet somehow
    which can be the different stages of damage which a thing undergoes(?)
   
Figuring out how to tell the renderer which index in a sprite sheet I am at 
    could pose potential issues because of how things are...

Idea for sprite sheet:
    every item that is to be rendererd has a sprite sheet component
    this sprite sheet component has a texture ID (string or int) and 
    an index into the sprite sheet which determines which of the items 
    in the sheet we are using. For a static item, the index will be 0
    and never change. For an animated item, the index will be changed
    as time goes on. Perhaps, the sprite sheet can contain a floating point
    number which represents the time in an animation frame. this will
    probably be standardized for many different things like animated map
    items will have a slower animation tick rate than the players sprite.
    For items that are static, the animation frame time will be -1 or 0,
    and the index will never change.

Animations:
    going off of the last paragraph, I now am beginning to understand
    how I might implement sprite sheet based animations into this engine.
    First off, I will need an AnimationSystem : UpdateSystem which can
    manage updating the index into the sprite sheet for each of the
    sprite sheet components. come to think of it, I could have sprite sheet
    component for all items which have a texture, and then use an additional
    animation component which signifies that this sprite sheet is animated.
    This could greatly increase performance for the animation system as it
    won't have to go through a bunch of spritesheet components that are not
    animated. The animatino system will simply get all entities which have both
    a sprite sheet component and an animation component, then, using
    the information from the animatino component, it will incrememnt the
    index in the sprite sheet component(?) (or I will store this index in
    the animation component which makes more sense perhaps... not sure yet).

    The sprite sheets will use a column or row wise traversal standard which
    means that an increased index into the sprite sheet will move the 
    source rectangle either 1 down or 1 to the right. I don't know which yet.
    

*/

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


ISSUES:
    potential issue involving the calculation of the movement. When the fps is lower (25-30), the 
    player glides farther than when the fps is higher (165+).

BUGS:
    figure out how to scale friction calculation by FPS. right now, when the fps is higher, things slow down faster which is not good.

CURRENT:
    
    1. draw new 32x32 spritesheet for the landscape
    
    (very good explanation)
    2. develop a way for the information in the tilemap to be included in the ECS and used in calculations
       My current idea is to have a tilemap system which, at load time, gets the information from the tilemap
       and then loads that into the ECS (things like trees or rocks which can collide the the player, cast shadows,
       or do other things). Depending on which texture is used for the specific thing in the midground layer (the
       layer that consists of things which the player can interact with potentially), the tilemap System (i'll 
       call it that) will create entities with the Entity Manager which consist of certain things that I will
       decide correspond for each tile placed in that layer. So lets say I place in a lightpost. The tilemap
       system will see that and then generate an entity with a collider, a texture, a point light, and a transform.
       The values for these things can be found in the TILED JSON file.


    FIXED: camera issue where normal texture would not line up with the 
           brick correctly. This would only happen when I would render things
           based on the camera's world view projection matrix.





[] entity manager
    IDEAS:
    - add a sub system which divides all the entities up by location (quadtree) and
      allows for querying the system to get all entities within a certain area.
    - this will make the process of process of sifting through entities a lot easier.
    - this is independent of the physics system and the physics system can use it if it needs to
    
    

[] Renderer
    - work on renderer class in system.cs
        * Will need to create lighting / particle / effect subsystem to manage
            changing the shader parameters depending on what things are present in
            the scene. create another class and put it in the renderer so that
            the functionality can be enclosed within the rendering system
        * try height map for shadows and other things(?)
        * brain storm how to allow for setting of shader parameters
          i.e. how can I set shader parameters without hard coding the parameter name


[] shaders
    - shadows (chose between hard / soft)
        * soft shadows can be difficult to my knowledge and may not be the best choice to implement starting out
    - choose between forward rendering and deferred lighting
        * forward rendering is what I am currently doing and is more costly but given that this is a 2d pixel game, I am not to worried about performance
        * deferred lighting means you only calculate the lighting information once on seperate render targets. Then with these you can go over each pixel
          and calculate the light for that pixel by sampling the previously draw render targets.
        * the other passes render the whole scene with just one piece of information
            1. normals
            2. ambient lighting
            3. diffuse lighting
          Then the total rendering cost for lighting is going to be brought down to O(num lights * num pixels)!!!
          this is much better than forward rendering which is O(num lights * num vertices)
        * however, it may not yield any major performance benefit becuase of the way that the shader is working

[] physics
    - when many objects are colliding, and moving together, they jitter. Check out website that you
      used in the past to figure out impulse in order to add some type of relaxation to the collision
      calculation, not sure what it is called.

[] Entity Component System
    
    - systems
        - movement system ->
                * gets all the controller components and updates the entities
                    physics based on the input recieved
                * decide how to store movement speed for each player (like in a component which stores base movement speed 
                    and then a powerup system could use this base value as well as any powerups the player has to calculate their new
                    movement speed)
        - collision / physics system -> 
                * allow the physics system to query some map manager for the coefficient of kinetic friction
                    or other information that would effect the player's movement based on where the player is in the world.
                * implement a quadtree for collision detection optimization
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
        // in future, read this in from JSON file
        //const int WIN_WIDTH = 1440;
        //const int WIN_HEIGHT = 810;

        const int WIN_WIDTH = 1920;
        const int WIN_HEIGHT = 1080;

        const int TARGET_WIDTH = 480; //320;
        const int TARGET_HEIGHT = 270; //180; 

        Camera2D cam;

        Vector2 worldMousePos;
        Vector2 viewportMousePos;
        // just so I don't get confused in the future, Vector2 player is the point that the camera is looking at
        //Vector2 player = new Vector2(300f, 250f);
        Vector2 player = Vector2.Zero;

        Vector2 entitySize = new Vector2(32f, 32f);

        float totalGameTime = 0f;

        float framesPerSecond = 0f;
        float secondsCounter = 0f;
        int numFrames = 0;

        float moveSpeed = 100f;
        //float theta = 0f;

        EntityManager eMan;

        PhysicsSystem pSys;
        ActionSystem aSys;
        InputSystem iSys;
        LightingSystem lSys;
        AnimationSystem animSys;

        RenderingSystem renderer;
        TextureManager tMan;

        Tilemap tilemap;

        Entity pEnt;
        Entity ent2;
        Vector2 playerPos;

        //Tilemap tilemap;
        //TilemapRenderer tRenderer;

        //EntityManagerDebug eManDebug;

        private GraphicsDeviceManager graphics;
        //private SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            IsFixedTimeStep = false; // lock at 60fps

            // init entities
            int numEnts = 12;
            eMan = new EntityManager(numEnts);
            tMan = new TextureManager();

            

            // must happen in this order for the camera to work properly
            // [

            // this is old, not going to change the viwport any more, just going to create
            // one on the spot and give it to camera class, does the same thing

            //graphics.GraphicsDevice.Viewport = new Viewport(0, 0, TARGET_WIDTH, TARGET_HEIGHT); // old
            //graphics.ApplyChanges(); // old

            // create viewport for camera
            
            //cam = new Camera2D(GraphicsDevice.Viewport); // old
            cam = new Camera2D(new Viewport(0, 0, TARGET_WIDTH, TARGET_HEIGHT));
            cam.Zoom = 0.4f;
            renderer = new RenderingSystem(eMan, graphics, tMan);
            
            graphics.PreferredBackBufferWidth = WIN_WIDTH;
            graphics.PreferredBackBufferHeight = WIN_HEIGHT;
            graphics.ApplyChanges();
            // at this point the viweport is set to win width win height
            // ]
            
            pEnt = eMan.CreateEntity();
            eMan.AddComponent<CController>(pEnt, new CController(PlayerIndex.One));
            eMan.AddComponent<CTransform>(pEnt, new CTransform() { position = new Vector2(0f, 0f) });
            eMan.AddComponent<CRigidBody>(pEnt, new CRigidBody() { mass = 5f });
            eMan.AddComponent<CCollider>(pEnt, new CRectCollider(entitySize));
            eMan.AddComponent<CTexture>(pEnt, new CTexture("brick"));
            eMan.AddComponent<CPointLight>(pEnt, new CPointLight(100.0f, new Vector3(0.0f, 1.0f, 0.0f), new Vector2(16, 16)));

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
            ent2 = lightBlock;

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

            pSys = new PhysicsSystem(eMan);
            iSys = new InputSystem(eMan);
            aSys = new ActionSystem(eMan);
            animSys = new AnimationSystem(eMan);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            //spriteBatch = new SpriteBatch(GraphicsDevice);

            //dirtTex = Content.Load<Texture2D>("dirt");
            //brickTex = Content.Load<Texture2D>("textures/smooth-brick");

            //lightEffect = Content.Load<Effect>("LightSpriteEffect");

            // (WRONG, but keeping it here so I don't completely forget
            // this is a possibility) set const parameters
            // here (or in "Initialize()") to save memory and time
            // do not do it in the drawing loop it wastes resources
            // instead set changing values in the update loop and constant ones here

            tMan.AddTexture("atlas-dev", Content.Load<Texture2D>("textures/atlas-dev"));
            tMan.AddTexture("normal-atlas-dev", Content.Load<Texture2D>("textures/normal-atlas-dev"));
            tMan.AddTexture("entity_tilesheet", Content.Load<Texture2D>("textures/smooth-brick"));
            // this type of information could be stored inside of json file
            tMan.AddTextureRect("brick", new Rectangle(0, 0, 32, 32)); 

            tilemap = new Tilemap("atlas-dev", "normal-atlas-dev");
            
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

            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
            {
                CRigidBody rig = (CRigidBody)eMan.GetComponent<CRigidBody>(pEnt.id);

                if (rig.velocity.LengthSquared() > 0.00001f)
                {
                    rig.velocity += new Vector2(20f, 20f) * Vector2.Normalize(rig.velocity);
                }
            }

            // round player position so that it exists only within whole numbered coordinates (removes texture distortion)
            //player = Vector2.Round(player); // IMPORTANT For pixel perfect camera to not bug out (!!!)


            cam.SmoothZoom(1.0f, 4f, dt);

            iSys.Update(gameTime);
            aSys.Update(gameTime);
            pSys.Update(gameTime);
            animSys.Update(gameTime);

            CTransform pTrans = (CTransform)eMan.GetComponent<CTransform>(pEnt.id);
            //CRigidBody pRig = (CRigidBody)eMan.GetComponent<CRigidBody>(pEnt.id);
            playerPos = pTrans.position;
            playerPos += new Vector2(16, 16);
            //pTrans.position = worldMousePos;

            // set debug text for renderer
            renderer.debugText = new List<string> {
                "viewport: " + + graphics.GraphicsDevice.Viewport.Width + ", " + graphics.GraphicsDevice.Viewport.Height,
                "fps: " + Math.Round(framesPerSecond, 2),
                "cam pos: " + Math.Round(cam.Position.X, 1) +
                ", " + Math.Round(cam.Position.Y, 1),
            };

            //cam.Update(playerPos, dt);

            //cam.Update(player, dt);
            cam.Update(playerPos, dt);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            lSys.SetShaderParameters(cam);
            renderer.Render(cam, tilemap);
            base.Draw(gameTime);
        }
    }
}


/*
namespace Monogame_Test_Project
{
    public class Game1 : Game
    {
        const int WIN_WIDTH = 1440;
        const int WIN_HEIGHT = 810;

        const int TARGET_WIDTH = 480; //320;
        const int TARGET_HEIGHT = 270; //180; 

        Camera2D cam;

        Vector2 worldMousePos;
        Vector2 viewportMousePos;

        Vector2 player; // just so I don't get confused in the future, Vector2 player is the point that the camera is looking at
        Vector2 entitySize = new Vector2(32f, 32f);

        float totalGameTime = 0f;

        float framesPerSecond = 0f;
        float secondsCounter = 0f;
        int numFrames = 0;

        float moveSpeed = 100f;
        float theta = 0f;

        Effect lightEffect;

        RenderTarget2D renderCanvas;

        EntityManager eMan;

        PhysicsSystem pSys;
        ActionSystem aSys;
        InputSystem iSys;

        RenderingSystem renderer;

        Entity pEnt;
        Vector2 playerPos;

        //EntityManagerDebug eManDebug;

        Texture2D dirtTex;
        Texture2D brickTex;

        SpriteFont spriteFont;

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

            IsFixedTimeStep = false;

            graphics.ApplyChanges();
           
            // test
            renderer = new RenderingSystem(eMan);
            renderer.textureMap.Add("brick", Content.Load<Texture2D>("textures/smooth-brick"));

            renderCanvas = new RenderTarget2D(
                GraphicsDevice,
                TARGET_WIDTH, TARGET_HEIGHT,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);

            // init entities
            int numEnts = 3;
            eMan = new EntityManager(numEnts);

            pEnt = eMan.CreateEntity();
            eMan.AddComponent<CController>(pEnt, new CController(PlayerIndex.One));
            eMan.AddComponent<CTransform>(pEnt, new CTransform() { position = new Vector2(0f, 0f) });
            eMan.AddComponent<CRigidBody>(pEnt, new CRigidBody(){ mass = 5f});
            eMan.AddComponent<CCollider>(pEnt, new CRectCollider(entitySize));
            eMan.AddComponent<CTexture>(pEnt, new CTexture("brick"));

            Entity lightBlock = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(lightBlock, new CTransform() { position = new Vector2(-40f, 10f) });
            eMan.AddComponent<CCollider>(lightBlock, new CRectCollider(entitySize));
            eMan.AddComponent<CRigidBody>(lightBlock, new CRigidBody() { mass = 2f });

            Entity heavyBlock = eMan.CreateEntity();
            eMan.AddComponent<CTransform>(heavyBlock, new CTransform() { position = new Vector2(40f, 40f)});
            eMan.AddComponent<CCollider>(heavyBlock, new CRectCollider(entitySize));
            eMan.AddComponent<CRigidBody>(heavyBlock, new CRigidBody() { mass = 20f });

            pSys = new PhysicsSystem(eMan);
            iSys = new InputSystem(eMan);
            aSys = new ActionSystem(eMan);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            dirtTex = Content.Load<Texture2D>("dirt");
            brickTex = Content.Load<Texture2D>("textures/smooth-brick");

            spriteFont = Content.Load<SpriteFont>("type-face");
            
            lightEffect = Content.Load<Effect>("LightSpriteEffect");

            // set const parameters here (or in "Initialize()") to save memory and time
            // do not do it in the drawing loop it wastes resources
            // instead set changing values in the update loop and constant ones here
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
                ((float)Mouse.GetState().X / (float)WIN_WIDTH) * (float)TARGET_WIDTH,
                ((float)Mouse.GetState().Y / (float)WIN_HEIGHT) * (float)TARGET_HEIGHT);

            // transform the mouse position into the actual position that it has on the screen after the camera translate has been done
            worldMousePos = cam.screenToWorld(viewportMousePos);

            float xDif = worldMousePos.X - playerPos.X;
            float yDif = worldMousePos.Y - playerPos.Y;

            theta = (float)Math.Atan2(yDif, xDif);

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

            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
            {                
                CRigidBody rig = (CRigidBody)eMan.GetComponent<CRigidBody>(pEnt.id);

                if (rig.velocity.LengthSquared() > 0.00001f)
                {
                    rig.velocity += new Vector2(20f, 20f) * Vector2.Normalize(rig.velocity);
                }
            }

            // round player position so that it exists only within whole numbered coordinates (removes texture distortion)
            //player = Vector2.Round(player); // IMPORTANT For pixel perfect camera to not bug out (!!!)

            iSys.Update(gameTime);
            aSys.Update(gameTime);
            pSys.Update(gameTime);
           
            CTransform pTrans = (CTransform)eMan.GetComponent<CTransform>(pEnt.id);
            playerPos = pTrans.position;

            //cam.Update(pTrans.position, dt);
            cam.Update(player, dt);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.SetRenderTarget(renderCanvas);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState =
                new DepthStencilState() { DepthBufferEnable = true };


            // setup shader

            lightEffect.CurrentTechnique.Passes[0].Apply();
            lightEffect.Parameters["AmbientLightColor"].SetValue(new Vector3(0.3f, 0.3f, 0.3f));

            //lightEffect.Parameters["PointLightPositions"].SetValue(new[] { new Vector3(viewportMousePos.X, viewportMousePos.Y, 0.0f), new Vector3(viewportMousePos.X , viewportMousePos.Y, 0.0f) });
            lightEffect.Parameters["PointLightPositions"].SetValue(new[] {
                new Vector3(viewportMousePos.X + 50.0f, viewportMousePos.Y, 0.0f),
                new Vector3(viewportMousePos.X - 50.0f, viewportMousePos.Y, 0.0f)});
            lightEffect.Parameters["PointLightColors"].SetValue(new[] {
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 0.0f, 0.0f) });
            lightEffect.Parameters["PointLightRadii"].SetValue(new[] { 30.0f, 100.0f });

            spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointClamp, transformMatrix: cam.TransformMatrix,
                effect: lightEffect);

            // Draw to the canvas

            Bitmask sig = new Bitmask((int)ComponentType.Count);
            sig[ComponentType.CTransform] = true;
            sig[ComponentType.CCollider] = true;
            List<int> posEnts = eMan.GetEntityIds(sig).ToList();

            foreach (var id in posEnts)
            {
                CTransform transform = (CTransform)eMan.GetComponent<CTransform>(id);
                CRectCollider collider = (CRectCollider)eMan.GetComponent<CRectCollider>(id);

                if (id == pEnt.id)
                {
                    spriteBatch.Draw(
                        brickTex,
                        new Rectangle(
                            (int)transform.X, (int)transform.Y,
                            (int)collider.Width, (int)collider.Height),
                        null,
                        Color.White
                        );
                }

                else if (collider != null)
                {
                    spriteBatch.Draw(
                        brickTex,
                        new Rectangle(
                            (int)transform.X, (int)transform.Y,
                            (int)collider.Width, (int)collider.Height),
                        null,
                        Color.White);
                }
            }

            spriteBatch.End();


            // draw canvas to screen
            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.Default,
                RasterizerState.CullNone);

            spriteBatch.Draw(
                renderCanvas,
                new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                Color.White);


            // draw debug text
            spriteBatch.DrawString(spriteFont, "fps " + framesPerSecond, new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(spriteFont, "x: " + playerPos.X + "\ny: " + playerPos.Y, new Vector2(10, 40), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
*/


/*
 * OLD SHADER CODE BEFORE INTRODUCED LOOP
float4 MainPS(VertexShaderOutput input) : COLOR
{
float3 ambientLight = AmbientLightColor * input.Color.rgb;
float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);



// add on the light value based on how far the point is from the light

// light source attenuation

//float b = 0.01;
//float radius = sqrt(1.0 / (b * minLight));







float dist = distance(PointLightPosition.xy, input.Pos.xy);

// attenuation technique 1
//float b = 0.01;
//float a = 0.1;
//float att = 1.0 / (1.0 + a * dist + b * dist * dist);

// attenuation technique 2
// float minLight = 0.01; // cuts light off when attenuation reaches this value
// float b = 1.0 / (PointLightRadius * PointLightRadius * minLight); // calculate b based on radius and minline value 
// float a = 0.1;

// attenuation technique 3
float att = clamp(1.0 - dist / PointLightRadius, 0.0, 1.0);


if (dist >= PointLightRadius)
{
return texColor * float4(ambientLight, input.Color.w);
}


//float gradient = smoothstep(0.0, 1.0, dist);

// add lights together
float3 finalLight = ambientLight + (PointLightColor * att);

// multiply texture color and light value
return texColor * float4(finalLight, input.Color.w);
}


*/




/*
protected override void Draw(GameTime gameTime)
{

    GraphicsDevice.SetRenderTarget(renderCanvas);
    GraphicsDevice.Clear(Color.CornflowerBlue);
    GraphicsDevice.DepthStencilState =
        new DepthStencilState() { DepthBufferEnable = true };

    spriteEffect.CurrentTechnique.Passes[0].Apply();


    spriteBatch.Begin(
        SpriteSortMode.Immediate, BlendState.AlphaBlend,
        SamplerState.PointClamp, transformMatrix: cam.TransformMatrix,
        effect: spriteEffect);

    // Draw to the canvas
    Bitmask sig = new Bitmask((int)ComponentType.Count);
    sig[ComponentType.CTransform] = true;
    sig[ComponentType.CCollider] = true;
    List<int> posEnts = eMan.GetEntityIds(sig).ToList();

    foreach (var id in posEnts)
    {
        CTransform transform = (CTransform)eMan.GetComponent<CTransform>(id);
        CRectCollider collider = (CRectCollider)eMan.GetComponent<CRectCollider>(id);

        if (collider != null)
        {
            spriteBatch.Draw(
                dirtTex,
                new Rectangle(
                    (int)transform.X, (int)transform.Y,
                    (int)collider.Width, (int)collider.Height),
                null,
                Color.White);
        }
    }


    // setup shaders




    //spriteEffect.CurrentTechnique.Passes[0].Apply();

    // render sprites with effects


    spriteBatch.End();

    graphics.GraphicsDevice.SetRenderTarget(null);
    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

    //lightEffect.CurrentTechnique.Passes[0].Apply();
    //lightEffect.Parameters["LightPosition"]?.SetValue();
    //lightEffect.Parameters["ScreenTexture"]?.SetValue(renderCanvas);
    //.Parameters["WorldViewProjection"]?.SetValue(cam.TransformMatrix);


    // draw canvas to screen
    spriteBatch.Begin(
        SpriteSortMode.Immediate, BlendState.AlphaBlend,
        SamplerState.PointClamp, DepthStencilState.Default,
        RasterizerState.CullNone);

    // draw to actual window
    spriteBatch.Draw(
        renderCanvas,
        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
        Color.White);

    // draw debug text
    spriteBatch.DrawString(spriteFont, "fps " + framesPerSecond, new Vector2(10, 10), Color.Black);
    spriteBatch.DrawString(spriteFont, "x: " + playerPos.X + "\ny: " + playerPos.Y, new Vector2(10, 40), Color.Black);
    spriteBatch.End();

    base.Draw(gameTime);
}
*/



























//protected override void Draw(GameTime gameTime)
//{
//    GraphicsDevice.Clear(Color.CornflowerBlue);

//    // render tilemap to world canvas

//    tilemapRenderer.render(graphics.GraphicsDevice);


//    graphics.GraphicsDevice.SetRenderTarget(renderCanvas);
//    graphics.GraphicsDevice.DepthStencilState = new DepthStencilState() 
//        { DepthBufferEnable = true };
//    graphics.GraphicsDevice.Clear(Color.CornflowerBlue); // clear canvas
//    // draw to render target
//    spriteBatch.Begin(
//        SpriteSortMode.Immediate, BlendState.AlphaBlend,
//        SamplerState.PointClamp, transformMatrix: cam.TransformMatrix);

//    spriteBatch.Draw(
//        tilemapRenderer.mapCanvas, 
//        new Rectangle(
//            0, 0, 
//            tilemapRenderer.mapCanvas.Width,
//            tilemapRenderer.mapCanvas.Height),
//        tilemapRenderer.mapCanvas.Bounds,
//        Color.White);

//    spriteEffect.CurrentTechnique.Passes[0].Apply(); // effect for player texture (nothing right now)

//    spriteBatch.Draw(
//        spriteMan.dSpriteSheets["tilesheet"],
//        player.getPosition(),
//        spriteMan.dSpriteRects["tilesheet"]["dirt"],
//        Color.White,
//        0f,
//        new Vector2(8f, 8f),
//        1f,
//        SpriteEffects.None,
//        0f);

//    //spriteBatch.Draw(
//    //     rectTexture,
//    //     player.getPosition(),
//    //     null,
//    //     Color.White,
//    //     0f, // theta
//    //     new Vector2(8f, 8f),
//    //     1f,
//    //     SpriteEffects.None,
//    //     0f);

//    spriteBatch.End();

//    graphics.GraphicsDevice.SetRenderTarget(null);
//    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

//    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
//    //    SamplerState.PointClamp, DepthStencilState.Default,
//    //    RasterizerState.CullNone);




//    // Draw map canvas ====================

//    spriteBatch.Begin(
//        SpriteSortMode.Immediate, 
//        BlendState.AlphaBlend,
//        SamplerState.PointClamp, 
//        DepthStencilState.Default,
//        RasterizerState.CullNone);

//    // draw to actual window
//    spriteBatch.Draw(
//        renderCanvas,
//        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
//        Color.White);

//    spriteBatch.End();

//    base.Draw(gameTime);
//}



/*

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


        Tilemap tilemap;
        TilemapRenderer tilemapRenderer;

        Vector2 worldMousePos;

        Effect spriteEffect;

        RenderTarget2D renderCanvas;

        //Entity playerEntity;
        //GameContext context;

        int pEntity;

        //RenderingSystem renderer;
        

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

            //context = new GameContext(25);

            //pEntity = context.CreateEntity();
            //context.AddComponent<CTransform>(pEntity, new CTransform(new Vector2(0f, 0f), new Vector2(1f, 1f), 0f, 0f));
            ////context.AddComponent<CRigidBody>(pEntity, new CRigidBody());
            //context.AddComponent<CTexture2D>(pEntity, new CTexture2D("dirt", "tilesheet", new Vector2(8f, 8f)));


            //renderer = new RenderingSystem(context, graphics.GraphicsDevice);



            //dTextures = new Dictionary<string, Texture2D>();
            //dTextures.Add("dirt", Content.Load<Texture2D>);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here

            spriteEffect = Content.Load<Effect>("spriteShader");

            //rectTexture = Content.Load<Texture2D>("dirt");
            //circleTexture = Content.Load<Texture2D>("test-ball");

            tilemap.textureSheet = Content.Load<Texture2D>("tilesheet");

            //context.spriteMan.AddSpriteSheet("tilesheet", Content.Load<Texture2D>("tilesheet"));
            //context.spriteMan.AddSprite("tilesheet", "dirt", new Rectangle(0, 5 * 16, 16, 16));
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

            //var keyState = Keyboard.GetState();
            //if (keyState.IsKeyDown(Keys.W))
            //{
            //    player.setPosition(player.getPosition() + 
            //        new Vector2(0, -moveSpeed * dt));
            //}
            //if (keyState.IsKeyDown(Keys.S))
            //{
            //    player.setPosition(player.getPosition() + 
            //        new Vector2(0, moveSpeed * dt));
            //}
            //if (keyState.IsKeyDown(Keys.A))
            //{
            //    player.setPosition(player.getPosition() + 
            //        new Vector2(-moveSpeed * dt, 0));
            //}
            //if (keyState.IsKeyDown(Keys.D))
            //{
            //    player.setPosition(player.getPosition() + 
            //        new Vector2(moveSpeed * dt, 0));
            //}


            //Vector2 playerPos = ((CTransform)context.GetComponent<CTransform>(pEntity)).position;

            //var keyState = Keyboard.GetState();
            //if (keyState.IsKeyDown(Keys.W))
            //{
            //    playerPos += new Vector2(0, -moveSpeed * dt);
            //}
            //if (keyState.IsKeyDown(Keys.S))
            //{
            //    playerPos += new Vector2(0, moveSpeed * dt);
            //}
            //if (keyState.IsKeyDown(Keys.A))
            //{
            //    playerPos += new Vector2(-moveSpeed * dt, 0);
            //}
            //if (keyState.IsKeyDown(Keys.D))
            //{
            //    playerPos += new Vector2(moveSpeed * dt, 0);
            //}

            // round player position so that it exists only within whole numbered coordinates (removes texture distortion)
            player.position = Vector2.Round(player.position); // IMPORTANT For pixel perfect camera to not bug out

            // check for collision with the rectangle colliders
            //foreach (var rect in rects)
            //{
            //    if (solver.checkCollision(player, rect))
            //    {
            //        solver.solveCollision(player, rect);
            //    }
            //}

            //foreach (var circle in circles)
            //{
            //    if (solver.checkCollision(player, circle))
            //    {
            //        solver.solveCollision(player, circle);
            //    }
            //}
            //context.Update();

            cam.Update(player.position, dt);
            
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
            

            base.Update(gameTime);
        }

        
        
        //protected override void Draw(GameTime gameTime)
        //{
        //    GraphicsDevice.Clear(Color.CornflowerBlue);

        //    //tilemapRenderer.render(graphics.GraphicsDevice);

        //    spriteEffect.CurrentTechnique.Passes[0].Apply();

        //    //renderer.Render(gameTime, renderCanvas, GraphicsDevice, cam);

        //    spriteBatch.Begin(
        //        SpriteSortMode.Immediate,
        //        BlendState.AlphaBlend,
        //        SamplerState.PointClamp,
        //        DepthStencilState.Default,
        //        RasterizerState.CullNone);

        //    // draw to actual window
        //    spriteBatch.Draw(
        //        renderCanvas,
        //        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
        //        Color.White);

        //    spriteBatch.End();

        //    base.Draw(gameTime);
        //}


        //protected override void Draw(GameTime gameTime)
        //{
        //    GraphicsDevice.Clear(Color.CornflowerBlue);

        //    // render tilemap to world canvas

        //    tilemapRenderer.render(graphics.GraphicsDevice);

            
        //    graphics.GraphicsDevice.SetRenderTarget(renderCanvas);
        //    graphics.GraphicsDevice.DepthStencilState = new DepthStencilState() 
        //        { DepthBufferEnable = true };
        //    graphics.GraphicsDevice.Clear(Color.CornflowerBlue); // clear canvas
        //    // draw to render target
        //    spriteBatch.Begin(
        //        SpriteSortMode.Immediate, BlendState.AlphaBlend,
        //        SamplerState.PointClamp, transformMatrix: cam.TransformMatrix);

        //    spriteBatch.Draw(
        //        tilemapRenderer.mapCanvas, 
        //        new Rectangle(
        //            0, 0, 
        //            tilemapRenderer.mapCanvas.Width,
        //            tilemapRenderer.mapCanvas.Height),
        //        tilemapRenderer.mapCanvas.Bounds,
        //        Color.White);

        //    spriteEffect.CurrentTechnique.Passes[0].Apply(); // effect for player texture (nothing right now)

        //    spriteBatch.Draw(
        //        spriteMan.dSpriteSheets["tilesheet"],
        //        player.getPosition(),
        //        spriteMan.dSpriteRects["tilesheet"]["dirt"],
        //        Color.White,
        //        0f,
        //        new Vector2(8f, 8f),
        //        1f,
        //        SpriteEffects.None,
        //        0f);

        //    //spriteBatch.Draw(
        //    //     rectTexture,
        //    //     player.getPosition(),
        //    //     null,
        //    //     Color.White,
        //    //     0f, // theta
        //    //     new Vector2(8f, 8f),
        //    //     1f,
        //    //     SpriteEffects.None,
        //    //     0f);
            
        //    spriteBatch.End();
            
        //    graphics.GraphicsDevice.SetRenderTarget(null);
        //    graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

        //    //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
        //    //    SamplerState.PointClamp, DepthStencilState.Default,
        //    //    RasterizerState.CullNone);




        //    // Draw map canvas ====================

        //    spriteBatch.Begin(
        //        SpriteSortMode.Immediate, 
        //        BlendState.AlphaBlend,
        //        SamplerState.PointClamp, 
        //        DepthStencilState.Default,
        //        RasterizerState.CullNone);

        //    // draw to actual window
        //    spriteBatch.Draw(
        //        renderCanvas,
        //        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
        //        Color.White);

        //    spriteBatch.End();

        //    base.Draw(gameTime);
        //}
    }
}
*/