using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Collisions;

namespace ECS
{


    public class Entity
    {
        public uint id;
        readonly List<Component> components = new List<Component>();

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

    public class CircleColliderComponent : Component
    {
        public float radius;
        public CircleColliderComponent(float radius)
        {
            this.radius = radius;
        }
    }

    public class RectColliderComponent : Component
    {
        public Vector2 size = new Vector2(8f, 8f);
        public RectColliderComponent(Vector2 size)
        {
            this.size = size;
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
