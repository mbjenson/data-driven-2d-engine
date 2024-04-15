using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Collisions;
using System.ComponentModel;
using Microsoft.VisualBasic;
using System;
using System.Globalization;
using Microsoft.Xna.Framework.Input;
using System.Collections;






namespace ECS
{

    enum ComponentTypes
    {
        CTransform,
        Count,
    }
    
    public class Entity
    {
        public int id;
        public BitArray componentMask = new BitArray((int)ComponentTypes.Count, false);
    }

    public class IComponent { }

    public class CTransform : IComponent
    {
        Vector2 position;
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