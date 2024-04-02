using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace ECS
{
    
    

    // the coordinator contains the systems (PhysicsSystem, TextureSystem, etc...) that 
    // will act upon the entities

    // In the constructor for Any Component, the component created will be registered in the system.
    // EG:
    // public Transform() {
    //   TransformSystem.Register(this);
    // }
    //
    // then when the coordinator updates the system, it will update all of the components of that type
    // without having to consider other ones


    // WIP, NEED TO FINISH THE BASIC COORDINATOR THEN ADD IN SYSTEMS
    public class Coordinator
    {
        private uint entityIterator = 0;

        public List<Entity> mEntities = new List<Entity>(64);
        public Coordinator() { }

        public void addEntity()
        {
            mEntities.Add(new Entity(entityIterator));
            entityIterator++;
        }
    }

    
    public class BaseSystem<T> where T : Component
    {

    }
}

