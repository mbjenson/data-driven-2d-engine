
using Microsoft.Xna.Framework;
using bitmask;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


/*
Note:
    How to add a component:
        1. Add name to ComponentType enum before "Count". Ensure name starts with 'C'. (e.g. CPointLight)
        2. Define class inheriting from IComponent
        3. In Entity Manager, add Component Type to both dictionaries allowing for conversion between enum and type
*/

namespace ECS
{
    // TODO: add physics info?
    // TODO: add controller input info?
    //      movement info containing a list of pairs which map
    //      controller input to player actions in game.
    //      the player actions is an enum containing things like:
    //              MOVEUP, MOVELEFT, MOVERIGHT, MOVEDOWN, 
    //      a player with a CController will have the necessary information to know how to handle it
    //      the CController binds that entity to the controller's input
    //      so that the input system can take the input given 
    //      and apply it to the player according to the wishes specific in the controller(?)

    // TODO: add camera component

    public enum ComponentType
    {
        CTransform,
        CCollider,
        CRigidBody,
        CController,
        // CMovement, // contains information like movement speed (pondering if I will use this...)
        // CCamera, // lives in the scene and can follow a point I think
        CPointLight,
        CTexture,
        CAnimation,
        CStaticBody,
        Count,
    }

    
    public class Entity
    {
        public int id;
        public Bitmask cMask; // component mask

        public Entity(int id) 
        {
            this.id = id;
            this.cMask = new Bitmask((int)ComponentType.Count);
        }

        public Entity(int id, Bitmask cMask)
        {
            this.id = id;
            this.cMask = cMask;
        }


        //public void AddComponentType(ComponentType type)
        //{
        //    this.cMask[(int)type] = true;
        //}

        //public void RemoveComponentType(ComponentType type)
        //{
        //    this.cMask[(int)type] = false;
        //}

    }

    // base component class
    public class IComponent { }


    public class CController : IComponent
    {
        public PlayerIndex controllerIndex;
        //public GamePadState gamePadState;
        public Vector2 movement;
        

        public CController() { }
        public CController(PlayerIndex controllerIndex)
        {
            this.controllerIndex = controllerIndex;
        }
    }


    public class CTransform : IComponent
    {
        public Vector2 position;
        // public (int / float?) height;
         //public Vector2 origin; // not sure if I am going to use it

        public float X
        {
            get { return position.X; }
            set { position.X = value; }
        }

        public float Y
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

        public CTransform() 
        {
            this.position = new Vector2(0, 0);
        }

        public CTransform(Vector2 position)
        {
            this.position = position;
        }

        public CTransform(Vector2 position, Vector2 origin)
        {
            this.position = position;
        }


        public void Move(Vector2 vec)
        {
            this.position += vec;
        }
        public void Move(float x, float y)
        {
            this.position.X += x;
            this.position.Y += y;
        }
    }

    public class CAnimation : IComponent 
    {
        public string animationID;
        public int frame;
        public int numFrames;
        public Vector2 offset;

        public CAnimation() { }

        public CAnimation(string animationID, int numFrames)
        {
            this.animationID = animationID;
            this.offset = Vector2.Zero;
            frame = 0;
            this.numFrames = numFrames;
        }

        public CAnimation(string animationID, Vector2 offset, int numFrames)
        {
            this.animationID = animationID;
            this.offset = offset;
            frame = 0;
            this.numFrames = numFrames;
        }
    }


    public class CTexture : IComponent
    {
        public string textureId; // some way to id this texture (subject to change)
        //public Vector2 offset;

        public CTexture() { }
        public CTexture(string textureId)
        {
            this.textureId = textureId;
            //offset = new Vector2(0, 0);
        }

        //public CTexture(string textureId, Vector2 offset)
        //{
        //    this.textureId = textureId;
        //    this.offset = offset;
        //}
    }




    public class CPointLight : IComponent
    {
        public float radius;
        // float a; // not sure what to call this but it is the thing that affects intensity of light falloff in the shader. it is called "a" right now.
        public Vector3 color;
        public Vector2 offset;
        public CPointLight()
        {
            radius = 30.0f;
            color = new Vector3(1.0f, 1.0f, 1.0f);
            offset = Vector2.Zero;
        }

        public CPointLight(float radius, Vector3 color)
        {
            this.radius = radius;
            this.color = color;
            offset = Vector2.Zero;
        }

        public CPointLight(float radius)
        {
            this.radius = radius;
            this.color = new Vector3(1.0f, 1.0f, 1.0f);
            offset = Vector2.Zero;
        }

        public CPointLight(float radius, Vector3 color, Vector2 offset)
        {
            this.radius = radius;
            this.color = color;
            this.offset = offset;
        }
    }

    //public class CAmbientLight : IComponent
    //{
    //    Vector3 color;
    //}


    public class CCollider : IComponent
    {

    }

    public class CRectCollider : CCollider
    {
        public Vector2 size;
        //public float X
        //{
        //    get { return size.X; }
        //    set { size.X = value; }
        //}

        //public float Y
        //{
        //    get { return size.Y; }
        //    set { size.Y = value; }
        //}

        public float Width
        {
            get { return size.X; }
            set { size.X = value; }
        }

        public float Height
        {
            get { return size.Y; }
            set { size.Y = value; }
        }

        public CRectCollider() { }
        public CRectCollider(float width, float height)
        {
            this.size.X = width;
            this.size.Y = height;
        }

        public CRectCollider(Vector2 size)
        {
            this.size = size;
        }
    }

    public class CCircleCollider : CCollider
    {
        public float radius;

        public CCircleCollider() { }
        public CCircleCollider(float radius)
        {
            this.radius = radius;
        }
    }

    // idea: maybe create a rigid body component which incorporates the 
    // collider? nvm bad idea, that I am working upon
    public class CRigidBody : IComponent
    {
        public Vector2 velocity;
        public Vector2 acceleration;
        public float mass;

        public CRigidBody() { }

        public CRigidBody(float mass)
        {
            this.velocity = Vector2.Zero;
            this.acceleration = Vector2.Zero;
            this.mass = mass;
        }

        public CRigidBody(Vector2 velocity, Vector2 acceleration, float mass)
        {
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.mass = mass;
        }

        //public void Update()
        //{
        //    velocity += acceleration;
        //}
    }


}






































/*
 * when creating an entity that has a some type of component that accesses a resource (texture, sound, etc),
 *  the entity will be given a resource handle which only contains an ID for that resource. 
 *  eg:
 *  
 *  Entity entity = new Entity(id);
 *  entity.addComponent<Texture>("texture.dirt"); // this texture.dirt is just one way that you can specify which
 *                                                  // resource the entity will be accessing without having to store
 *                                                  // the resource in the entity. The different systems will then have
 *                                                  // access to the centralized loaded resources and be able to use the 
 *                                                  // the resource handle to use the actual resource when necessary
 *                                                  // (whether that means indexing into an array or using a lookup function
 *                                                  // in some data structure IDK yet).
 *
 *
 *
*/
namespace ECS
{
    ///*
    //class IComponent:
    //    public abstract void update
    //*/
    //public class IComponent
    //{
    //    public int entityId = -1;

    //    //public IComponent(int entityId) { this.entityId = entityId; }
    //    //public virtual void Update() { }
    //}

    
    //public class CTransform : IComponent
    //{
    //    public Vector2 position;
    //    public Vector2 scale;
    //    public float rotation;
    //    public float layerDepth;

    //    public CTransform() { }
    //    public CTransform(Vector2 position, Vector2 scale, float rotation, float layerDepth)
    //    {
    //        this.position = position;
    //        this.scale = scale;
    //        this.rotation = rotation;
    //        this.layerDepth = layerDepth;
    //    }

    //    //public override void Update() { }
    //}

    //public class CRigidBody : IComponent
    //{
    //    public Vector2 velcocity;
    //    public Vector2 acceleration;

    //    public CRigidBody() { }
    //    //public override void Update() { }
    //}


    //public class CTexture2D : IComponent
    //{
    //    public string spriteId = "";
    //    public string spriteSheetId = "";
    //    public Vector2 textureOffset = Vector2.Zero;
    //    public CTexture2D() { }
    //    public CTexture2D(string spriteId, string spriteSheetId)
    //    {
    //        this.spriteId = spriteId;
    //        this.spriteSheetId = spriteSheetId;
    //    }

    //    public CTexture2D(string spriteId, string spriteSheetId, Vector2 textureOffset) : this(spriteId, spriteSheetId)
    //    {
    //        this.spriteId = spriteId;
    //        this.spriteSheetId = spriteSheetId;
    //        this.textureOffset = textureOffset;
    //    }
    //}


    //public class CController : IComponent
    //{
       
    //}

    ////public class CController : IComponent
    ////{
    ////    Action moveRight;
    ////    Action moveLeft;
    ////    Action moveUp;
    ////    Action moveDown;

        
    ////    public CController() // get key input (primitive for now)
    ////    {
            
    ////    }


    ////}
    

    
    //// animation component
    //public class CAnimation : IComponent
    //{
        
    //}

}   

    /*


    public class Entity
    {
        public uint id;
        private List<Component> components = new List<Component>();

        public Entity() { }
        public Entity(uint id)
        {
            this.id = id;
        }

        public void AddComponent(Component component)
        {
            components.Add(component);
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var comp in components)
            {
                if (comp.GetType().Equals(typeof(T)))
                {
                    return (T)comp;
                }
            }
            return null;
        }
    }


    public class Component
    {
        public virtual void Update(GameTime gametime) { }
    }


    public class TransformComponent : Component
    {

        public Vector2 position = Vector2.Zero;
        public Vector2 scale = Vector2.Zero;
        public float layerDepth = 0;
        public float rotation = 0;

        public TransformComponent() { }
        public TransformComponent(Vector2 position, Vector2 scale, float layerDepth, float rotation)
        {
            this.position = position;
            this.scale = scale;
            this.layerDepth = layerDepth;
            this.rotation = rotation;
        }
    }


    // create a texture system which centrally stores the textures for objects
    // and objects only store information regarding which texture they will use.
    // storing textures on a per-entity basis is a terrible idea and wastes tons of space
    public class TextureComponent : Component
    {
        public Texture2D texture;

        public TextureComponent() { }
    }

    public class RigidBodyComponent : Component
    {
        public Vector2 acceleration = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public RigidBodyComponent() { }

    }

    
    public class ColliderComponent : Component { }

    public class RectColliderComponent : ColliderComponent
    {
        public Vector2 size = new Vector2(8f, 8f);
        public RectColliderComponent(Vector2 size)
        {
            this.size = size;
        }
    }

    public class CircleColliderComponent : ColliderComponent
    {
        public float radius;
        public CircleColliderComponent(float radius)
        {
            this.radius = radius;
        }
    }

    






    // implement an Entity Component System
    // "ECS"

    // this means that the game does not have to be built out of lots of confusing heirarchies
    // or inherited traits but is made up of composition of components.

    // the ECS then can evaluate the type of an entity based on which components it has and
    // can use a "____handler" to handle those types of entities.

    // for example, if we are dealing with enemies, I may have a handler which handles all the 
    // enemies that have position, velocity, attacks, etc. But I would not use the same handler
    // to handle the enemies who do not have velocity (i.e. a stationary enemy object)


    // will have texture array containing the appropriate textures for the entities so that
    // they can be accessed via index rather than via storing the texture in each entity



}
    */