﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Linq;

namespace ECS
{
    // for systems to operate on the components, lists of all possible component types will be constructed
    // 
    // when a system, for example the physics system, queries the main system for components, it will
    // initially choose a list of a single kind of component which all entities to be manipulated by the system have in common,
    // (preferrably the smallest of these lists), loop through each entity, querying each one for the remaining 
    // components that allow the system to manipulate the entity. 

    // each component contains within it a uint which refers to the entity it belongs to.
    // this can then be used to look up the corresponding entity from the mEntities list in O(1) time using
    // the id (uint) as the index into the mEntities

    /*
    
    list = system.GetList<a component type>();
    
    foreach (var component in list) {
        var comp1 = mEntities[component.getEntityId()].getComponent<desired component type1>();
        var comp2 = mEntities[component.getEntityId()].getComponent<desired component type2>();
        var comp3 = mEntities[component.getEntityId()].getComponent<desired component type3>();
        // the function getComponent<desired component type> returns null if the entity does not have the desired component
        if (comp1 && comp2 && comp3) { // if the entity has the 3 reminaing component types attatched to it
            // either system update or add all of the components into a list of packed data and operate on that data
            // as the system
        }
    }
        

    */

    /*
    Class ComponentArray:
        contains a list of lists of components
        allows for new types of components to be added to the list
        
        adding a component goes as follows:
            check if a list of this type exists
            if so, add it to that list
            if not, add a new list of that type to the list of lists
            
        removing an element requires more thought which I dont have right now
    */

    //public class ComponentArray<T> // where T : IComponent // , new()
    //{
    //    private List<T> contents;
    //    public ComponentArray()
    //    {
    //        contents = new List<T>();
    //    }

    //    public void RemoveComponent(T component)
    //    {
    //        contents.Remove(component);
    //    }

    //    public void AddComponent(T component)
    //    {
    //        contents.Add(component);
    //    }

    //    public Type GetArrayType()
    //    {
    //        return typeof(T);
    //    }
    //}









    //public class ComponentArray<T> where T : IComponent
    //{
    //    private List<T> contents;

    //    public ComponentArray()
    //    {
    //        contents = new List<T>();
    //    }

    //    public void Add(T item) { contents.Add(item); }
    //    public void Clear() { contents.Clear(); }
    //    public void Remote(T item) { contents.Remove(item); }

    //    public ref List<T> GetItems()
    //    {
    //        return ref contents;
    //    }

    //    public Type GetArrayType()
    //    {
    //        return typeof(T);
    //    }
    //}









    //public class ComponentHandler<T>
    //{
    //    private List<List<T>> mArrays;

    //    public ComponentHandler() { }


    //    public T AddComponent<CompType>(int entityId)
    //    {
    //        if (!this.ContainsType<T>())
    //        {

    //            mArrays.Add(new List<T>());
    //        }
    //        mArrays.Find(x => x.GetType() == typeof(T)).Add(new T());
    //    }

    //    private bool ContainsType<T>()
    //    {
    //        for (int i = 0; i < mArrays.Count; i++)
    //        {
    //            var arrType = mArrays[i].GetType();
    //            if (arrType == typeof(T))
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    //public void AddType<T>() where T : IComponent
    //    //{
    //    //    for (int i = 0; i < mArrays.Count; i++)
    //    //    {
    //    //        var arrType = mArrays[i].GetType();
    //    //        if (arrType == typeof(T))
    //    //        {
    //    //            return;
    //    //        }
    //    //    }
    //    //    var newArray = new ComponentArray<T>();
    //    //    arrays.Add(newArray);
    //    //}
    //}




    // IDEAS FOR MOVING FORWARD FROM HERE 4/7/2024

    // next step: (implementing a way that systems can interact with the context, querying the context for types of objects)
    // given a system that wants all the entity components with 3 different components, do this:
    // query the context for the list of components that should be the smallest of the 3,
    // go through each component, query the context to check if the entity associated with the first component
    //      contains the next 2 types of components.
    // (idea: just put this information into an array for the future maybe so faster next time (would make removing an entity very costly))
    // store this information in an array (or just do atomic operations on each one as you get it)
    // 
    // this is the best way that I can think of doing this without trying to implement some type of archetype system
    // It is not the absolute fastest way to do it, however it's the best I can do right now and I want to move forward.
    // this will allow me to start working on other parts of the game.



    //
    //              idea: when it comes time to render, the rendering system gets a list of all of the entities which have
    //              the transform components (or texture or something. whatever qualifies them to be rendered), and 
    //              draws them all to the dislpay on it's own as though nothing else in the world matters. (system 
    //              isolation will bring about a new era of prosperity).
    //
    //              idea: maybe write a function that finds all of the entities which contain all of the required types
    //              (and maybe cache them for later ...)
    //              and pack them into an array which can be used by a system. Each index of the array will
    //              represent one entities components. This way, the system can quickly query the context
    //              for all of the information that it needs, operate on that data as if nothing else in the world
    //              matters.
    // layout             
    //          entity1 : [Transform, RigidBody, Texture,
    //          entity2 :  Transform, RigidBody, Texture,
    //          entity3 :  Transform, RigidBody, Texture,
    //          entity4 :  Transform, RigidBody, Texture,
    //          entity5 :  Transform, RigidBody, Texture,
    //          entity6 :  Transform, RigidBody, Texture],


    public class Context
    {
        public HashSet<int> mEntities;
        //public List<List<IComponent>> mComponentArrays;

        // dictionaries relating components in differnt ways
        private Dictionary<int, HashSet<IComponent>> dComponentsByEntity;
        private Dictionary<Type, HashSet<IComponent>> dComponentsByType;
        
        //private List<int> mEntities;
        //private List<ComponentArray<IComponent>> mComponentArrays;
        public Queue<int> availableIds;
        private int maxEntities;
        
        public Context(int maxEntities)
        {
            this.maxEntities = maxEntities;
            availableIds = new Queue<int>();
            for (int i = 0; i < maxEntities; i++) { availableIds.Enqueue(i); }

            mEntities = new HashSet<int>();

            dComponentsByEntity = new Dictionary<int, HashSet<IComponent>>();
            dComponentsByType = new Dictionary<Type, HashSet<IComponent>>();
        }

        
        // DEBUG
        public void PrintAllEntities()
        {
            foreach (var e in mEntities)
            {
                PrintEntityComponents(e);
            }
        }

        public void PrintEntityComponents(int id)
        {
            var components = dComponentsByEntity[id];
            Debug.WriteLine("Entity id: " + id.ToString() + ", Component Count: " + components.Count + "\nComponent Type(s):");
            foreach (var component in components)
            {
                Debug.WriteLine(component.GetType().ToString());
            }
        }
        // END DEBUG

        // get all components belonging to entity id
        public IEnumerable<IComponent> GetComponentsOfEntity(int id)
        {
            HashSet<IComponent> components = null;

            if (dComponentsByEntity.TryGetValue(id, out components))
                return components;

            return null;
        }

        // get all components of type T
        public IEnumerable<IComponent> GetComponentsOfType<T>() where T : IComponent
        {
            HashSet<IComponent> components = null;

            if (dComponentsByType.TryGetValue(typeof(T), out components))
                return components;

            return null;
        }

        // create a new entity 
        public int CreateEntity()
        {
            if (availableIds.Count <= 0)
            {
                throw new ArgumentNullException("CreateEntity");
            }

            int id = availableIds.Dequeue();
            if (mEntities.Contains(id))
            {
                return -1;
            }
            mEntities.Add(id);
            return id;
        }

        // The removal of components and entities are costly operations which require a kind of batching system
        // in their current state. 
        // This means that I will implement a batch which contains a list of the entities that are to be removed.
        // Then, every so often, the batch will be processed by the context and all the entities and their corresponding 
        // components will be removed from the context fully.
        // In the mean time, the entities that are going to be removed should be marked as "dead" so they are not
        // processed.
        public void RemoveEntity(int id)
        {
            if (mEntities.Remove(id)) // check if entity exists in context
            {
                RemoveComponentsByEntity(id); // if so remove all components associated with it
            }
            availableIds.Enqueue(id);
        }

        public void RemoveComponent(IComponent component)
        {
            if (component == null)
            {
                return;
            }
            dComponentsByEntity[component.entityId].Remove(component);
            dComponentsByType[component.GetType()].Remove(component);
        }

        // removes all components associated with this entity id
        private void RemoveComponentsByEntity(int id)
        {
            // remove all components from the type list assocated with this entity
            foreach (var comp in dComponentsByEntity[id])
            {
                dComponentsByType[comp.GetType()].Remove(comp);
            }
            // remove all components from this entities entry
            dComponentsByEntity.Remove(id);
        }

        

        // in the future this will not be used much at all becuase it is costly and inefficient
        // instead the systems alone will interact with the components. (as far as I can see from here)
        public IComponent GetComponent<T>(int id) where T : IComponent
        {
            var eComponents = dComponentsByEntity[id].ToList();
            foreach (var component in eComponents)
            {
                if (typeof(T) == component.GetType())
                {
                    return component;
                }
            }
            return null;
        }

        //public List<IComponent> GetComponents(int id)
        //{
        //    if (!mEntities.Contains(id))
        //    {
        //        throw new ArgumentNullException("GetComponent");
        //    }

        //    List<IComponent> components = new List<IComponent>();
        //    // fetch all of the components that belong to 'id'
        //    foreach (var list in mComponentArrays)
        //    {
        //        foreach (var item in list)
        //        {

        //            components.Add(item);
        //        }
        //    }
        //}


        private bool _ContainsComponent(IComponent component)
        {
            // maybe add asserts here
            if (component == null)
            {
                return false;
            }
            // not sure if the program will seg fault if I check the entityId when
            // component is null so I am using two check statements
            else if (component.entityId < 0) 
            {
                return false;
            }
            // the context should never hold a component in just one of the dictionaries so we are assuming
            // perhaps dangerously so, that the component will be stored in both dictionaries
            if (dComponentsByEntity.ContainsKey(component.entityId) && dComponentsByType.ContainsKey(component.GetType()))
            {
                return (dComponentsByEntity[component.entityId].Contains(component) &&
                    dComponentsByType[component.GetType()].Contains(component));
            }
            return false;
            
        }
        
        private void _StashComponent(IComponent newComponent)
        {
            Debug.Assert(newComponent != null);
            Debug.Assert(newComponent.entityId >= 0);
            // check if component already exists in context
            if (_ContainsComponent(newComponent))
            {
                return;
            }
            // add to components by type dictionary
            var componentType = newComponent.GetType();
            HashSet<IComponent> typeComponents;

            if (!dComponentsByType.TryGetValue(componentType, out typeComponents))
            {
                typeComponents = new HashSet<IComponent>();
                dComponentsByType.Add(componentType, typeComponents);
            }

            dComponentsByType[componentType].Add(newComponent);

            // add to components by entity dictionary
            HashSet<IComponent> entityComponents;

            if (!dComponentsByEntity.TryGetValue(newComponent.entityId, out entityComponents))
            {
                entityComponents = new HashSet<IComponent>();
                dComponentsByEntity.Add(newComponent.entityId, entityComponents);
            }

            dComponentsByEntity[newComponent.entityId].Add(newComponent);
        }

        // add a component to a given entity (id)
        public T AddComponent<T>(int id, T component) where T : IComponent
        {
            if (!mEntities.Contains(id))
            {
                throw new ArgumentNullException("AddComponent");
            }

            // assign entity to new component
            component.entityId = id;
            // store component
            _StashComponent(component);

            return component;
        }
        

        // add new, default component to given entity (id)
        public T AddComponent<T>(int id) where T : IComponent, new()
        {
            // capture error if entity does not exist
            if (!mEntities.Contains(id))
            {
                throw new ArgumentNullException("AddComponent");
            }

            // create new component
            var newComponent = new T();
            // assign to entity
            newComponent.entityId = id;

            _StashComponent(newComponent);

            // GOOOOD
            // add to components by type dictionary
            //var componentType = newComponent.GetType();
            //HashSet<IComponent> typeComponents;

            //if (!dComponentsByType.TryGetValue(componentType, out typeComponents))
            //{
            //    typeComponents = new HashSet<IComponent>();
            //    dComponentsByType.Add(componentType, typeComponents);
            //}

            //dComponentsByType[componentType].Add(newComponent);

            //// add to components by entity dictionary
            //HashSet<IComponent> entityComponents;
            
            //if (!dComponentsByEntity.TryGetValue(id, out entityComponents))
            //{
            //    entityComponents = new HashSet<IComponent>();
            //    dComponentsByEntity.Add(id, entityComponents);
            //}

            //dComponentsByEntity[id].Add(newComponent);
            






            // BADDDDDD
            //foreach (var list in mComponentArrays)
            //{
            //    if (list.GetType() == typeof(T))
            //    {
            //        list.Add(newComponent);
            //        return newComponent;
            //    }
            //}
            //// if no list of that type yet
            //mComponentArrays.Add(new List<IComponent> { newComponent });
            
            //List<T> genericList = new List<T>() { newComponent };
            //List<T> newList = genericList as List<T>;
            //mComponentArrays.Add(newList);

            // return new component
            return newComponent;
        }



        //public T AddComponent<T>(int id) where T : new()
        //{
        //    var newComponent = new T();
        //    int entity;
        //    mEntities.TryGetValue(id, out entity);

            
        //}

        //public void RemoveComponent<T>(int id) where T : IComponent
        //{

        //}


        //public void GetComponents(int id)
        //{
            
        //}
        

        


        
        

    }

    //public class EntityCoordinator
    //{
    //    // private List<uint> mEntities = new List<uint>();
    //    private List<Entity> mEntities;
    //    private List<ComponentArray<IComponent>> mComponentArrays;
    //    // quere containing the available id's for the entities

    //    public EntityCoordinator() 
    //    {
    //        mComponentArrays = new List<ComponentArray<IComponent>>();
    //        mEntities = new List<Entity>();
    //    }

    //    public Entity CreateEntity<A, B, C, D, E, F, G>()
    //    {

    //    }
        
    //    public void AddEntity(Entity entity) 
    //    {
    //        foreach (var component in entity.components)
    //        {
    //            if (ContainsType<component.GetType()>())
    //            {
                    
    //                var arr = new ComponentArray<
    //            }
    //        }
            
            
    //    }

    //    public void RemoveEntity(Entity entity) 
    //    {
    //        mEntities.Remove(entity); 
    //    }

    //    // check if a component array of type T exists yet
    //    //private bool ContainsType<T>() where T : IComponent
    //    //{
    //    //    // Type type = component.GetType();
    //    //    foreach (var array in mComponentArrays)
    //    //    {
    //    //        if (array.GetArrayType() == typeof(T))
    //    //        {
    //    //            return true;
    //    //        }
    //    //    }
    //    //    return false;
    //    //}


    //    private bool ContainsType<T>()
    //    {
    //        foreach (var array in mComponentArrays)
    //        {
    //            if (array.GetArrayType() == typeof(T))
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //    //private bool ContainsType(IComponent component)
    //    //{
    //    //    // Type type = component.GetType();
    //    //    foreach (var array in mComponentArrays)
    //    //    {
    //    //        if (array.GetArrayType() == component.GetType())
    //    //        {
    //    //            return true;
    //    //        }
    //    //    }
    //    //    return false;
    //    //}












    //}












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


    // need arrays of components that can be accessed using the entities ID
    // in O(1) time

    // also be mindful of the cache

    // issue: don't know how to make several different types of components available to one
    //          system instead of just having one system per component
    // solution: have ComponentManagers which manage each type of component
    //           and have systems, like a physics system, which can have access to these different
    //           componentManagers and can access data from them.
    //          
    // cache: ensure that the data is being stored back to back so as to optimize the cache.
    //        If we can ID each entity starting from 0 and working the way up, we can then optimize
    //        the cache and reduce cache misses.
    // For Example:
    // public void PhysicsSystem.Update(GameTime gameTime) {
    //      foreach (var entity in mEntities) {
    //          // physics system contains a list of components that are used only by it
    //          // a component may be modified by several different systems in one 
    //      }
    // }



    // PLAN:
    // create an entity manager that uses the Entity class and upon creation of a component
    // it is then added to the correct system which the uses it to do stuff idk im too tired.

    // USES ENTITIES:

    //public class EntityManager
    //{
    //    const int MAX_ENTITIES = 5000;
    //    uint numAliveEntities = 0;

    //    private Queue<uint> availableEntityIds = new Queue<uint>();
    //    public List<Entity> mEntities = new List<Entity>();

    //    public EntityManager()
    //    {
    //        for (uint i = 0; i < MAX_ENTITIES; i++)
    //        {
    //            availableEntityIds.Enqueue(i);
    //        }
    //    }

    //    public Entity CreateEntity()
    //    {
    //        Debug.Assert(numAliveEntities < MAX_ENTITIES);

    //        uint id = availableEntityIds.Dequeue();
    //        Entity entity = new Entity(id);

    //        return entity;
    //    }

    //    public bool RemoveEntity(uint id)
    //    {

    //    }
    //}


    // USES UINTS instead of ENTITIES
    //public class EntityManager
    //{
    //    const int MAX_ENTITIES = 5000;
    //    uint numAliveEntities = 0;

    //    // add a queue of available entity id's (lets say it starts as being a queue containing numbers from 0 to 5000)
    //    private Queue<uint> availableEntityIds = new Queue<uint>();
    //    public List<uint> mEntities = new List<uint>();

    //    //public List<Entity> mEntities = new List<Entity>();

    //    public EntityManager()
    //    {
    //        // fill queue will available ids
    //        for (uint i = 0; i < MAX_ENTITIES; i++)
    //        {
    //            availableEntityIds.Enqueue(i);
    //        }
    //    }

    //    public uint CreateEntity()
    //    {
    //        Debug.Assert(numAliveEntities < MAX_ENTITIES);

    //        uint id = availableEntityIds.Dequeue();
    //        mEntities.Add(id);
    //        numAliveEntities++;

    //        return id;
    //    }

    //    public void RemoveEntity(uint id)
    //    {
    //        if (mEntities.Remove(id))
    //        {
    //            numAliveEntities--;
    //            availableEntityIds.Enqueue(id);
    //        }
    //        else
    //        {
    //            Debug.WriteLine("Cannot remove entity " + id + ", not found\n");
    //        }
    //    }

    //    public void AddComponent(uint id, Component component)
    //    {

    //    }

    //}
    /*
    public class BaseSystem<T> where T : Component
    {
        protected static List<T> components = new List<T>();

        public static void Register(T component)
        {
            components.Add(component);
        }f

        public virtual void Update(GameTime gameTime)
        {
            foreach (T component in components)
            {
                component.Update(gameTime);
            }
        }
    }


    public class CollisionSystem : BaseSystem<ColliderComponent>
    {
        public CollisionSystem() { }

        override public void Update(GameTime gameTime)
        {
            foreach (var collider in components)
            {
                
            }
        }
    }
    */
}
