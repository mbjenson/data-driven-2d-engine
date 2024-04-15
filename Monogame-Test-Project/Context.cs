using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;


namespace ECS
{

    // 4/14/24 : I have officially given up trying to get a really efficient entity
    //           component system. I am going to implement a bad one for now.

    public class EntityManager
    {
        /*
        Entity Manager

        What it will do:

            The entity manager will manage the creation, storage, and retreval
            of entities in the scene

        How will the entities be represented:

            The entities will be represented as integer IDs

        how will the components be represented

            the components will be stored as PODs (plain ol' data) or structs

        how will the components be retreived

            systems will query the entity manager for all the components it needs
            by given a signiature (bitstring) which is compared to all of the items
            to see which of them have the correct items
            and the manager will return a list of references to the actual data perhaps

        how will the components be stored

            the components will be stored in arrays which contain the data




        ****** BELOW HERE IS GOLD ******* GOLD!!! ******
        
        **************************** LORD THANK YOU ********************************
        
        contains list of entities which contain all the correct types of components
        loop through this list of entity id's which you can then use to query the
        manager and get the corresponding components from the manager

        ****************************************************************************

        requires a dictionary mapping from ComponentType to bitmask index

        
        enum ComponentEnum:
            CTransform,
            CRigidBody,
            CController,
            CTexture,

        Cool trick: place the ComponentEnum.Count to be the place item therefore containing
        the number of enums in the enum


        entity:
            int id
            bitmask componentTypes = {} (length = num component types, init to all 0s)

        
        the components will be stored such that an entityId will index into
        the array where an index represented an entity and each index is a list
        of IComponents.



        will store array of entities which will either store the components themselves or
        store a bitset which tells which types of entites it has or not. benefit of using
        a bitarray (bitset in c++) is that you can use the & bitwise operator on the
        set to check if the entity contains the desired components

        idea: entity is an int id (index into table of components)
        when querying to get the components which contain the correct components


        
        */

        // TODO: complete implementation described above and online

        public List<Entity> mEntities;
        public int Count = 0;

        public Dictionary<int, List<IComponent>> mComponents;

        public EntityManager(int numEntities)
        {
            Count = numEntities;
            mEntities = new List<Entity>(numEntities);
        }

        /*
        
        IEnumerable<int> GetEntities(int signature)
            List<int> entities = new List<int>();
            foreach (var entity in mEntities) {
                if (entity.componentMask & signature) {
                    entities.Add(entity);
                }
            }
            return entities;
        */

        // TODO figure out how to use a bit mask to check if an item has the matching
        //              components
        public IEnumerable<Entity> GetEntities(int signature)
        {
            List<Entity> entities = new List<Entity>();
            foreach (var entity in entities)
            {
                if (entity.componentMask & signature))
                {
                    entities.Add(entity);
                }
            }
            return entities;
        }


        // Add component

        /*
         * (dictionary<Type, ComponentEnum>)
         * must have a way to map T to ComponentEnum
        public void AddComopnent<T>(int entityId)
            // update bitmask
            mEntities[entityId].componentMask | TypeToEnumDictionary[T];
            // add component
            mComponents[entityId].Add(new T());
        */

    }





    /*
    public class Context
    {
        protected HashSet<int> mEntities;
        // dictionaries relating components in differnt ways
        protected Dictionary<int, HashSet<IComponent>> dComponentsByEntity;
        protected Dictionary<Type, HashSet<IComponent>> dComponentsByType;

        protected List<UpdateSystem> updateSystems;
        
        protected Queue<int> availableIds;
        protected int maxEntities;
        
        public Context(int maxEntities)
        {
            this.maxEntities = maxEntities;
            availableIds = new Queue<int>();
            for (int i = 0; i < maxEntities; i++) { availableIds.Enqueue(i); }

            mEntities = new HashSet<int>();

            dComponentsByEntity = new Dictionary<int, HashSet<IComponent>>();
            dComponentsByType = new Dictionary<Type, HashSet<IComponent>>();

            updateSystems = new List<UpdateSystem>();
        }
        
        // DEBUG
        public void PrintAllEntities()
        {
            foreach (var e in mEntities)
            {
                PrintEntityComponents(e);
            }
        }

        // DEBUG
        public void PrintEntityComponents(int id)
        {
            var components = dComponentsByEntity[id];
            Debug.WriteLine("Entity id: " + id.ToString() + ", Component Count: " + 
                components.Count + "\nComponent Type(s):");
            foreach (var component in components)
            {
                Debug.WriteLine(component.GetType().ToString());
            }
        }



        public UpdateSystem getUpdateSystem<T>() where T : UpdateSystem
        {
            return updateSystems.Find(x => x.GetType() == typeof(T));
        }


        public void AddUpdateSystem<T>(T system) where T : UpdateSystem
        {
            updateSystems.Add(system);
        }


        public void RemoveUpdateSystem<T>(T system) where T : UpdateSystem
        {
            updateSystems.Remove(system);
        }

        /// <summary>
        /// Create a new entity in context
        /// </summary>
        /// <returns>id of created entity if there is space in context or -1 if there is not space</returns>
        public int CreateEntity()
        {
            if (availableIds.Count <= 0)
            {
                return -1;
            }

            int id = availableIds.Dequeue();
            if (mEntities.Contains(id))
            {
                return -1;
            }
            mEntities.Add(id);
            return id;
        }



        /// <summary>
        /// Removes entity from the context
        /// </summary>
        /// <param name="entityId">Id of entity to be removed</param>
        public void RemoveEntity(int entityId)
        {
            if (mEntities.Remove(entityId)) // check if entity exists in context
            {
                _RemoveComponentsByEntity(entityId); // if so remove all components associated with it
            }
            availableIds.Enqueue(entityId);
        }



        /// <summary>
        /// Add an existing component to an entity in the context
        /// </summary>
        /// <typeparam name="T">Type of component to be added</typeparam>
        /// <param name="entityId">id of entity to add component to</param>
        /// <param name="component">Actual object to add</param>
        /// <returns>the component</returns>
        /// <exception cref="ArgumentNullException">if entity not present in context</exception>
        public T AddComponent<T>(int entityId, T component) where T : IComponent
        {
            if (!mEntities.Contains(entityId))
            {
                throw new ArgumentNullException("AddComponent");
            }

            // assign entity to new component
            component.entityId = entityId;
            // store component
            _StashComponent(component);

            return component;
        }


        /// <summary>
        /// Add component with default constructor to entity
        /// </summary>
        /// <typeparam name="T">Type of comopnent to add</typeparam>
        /// <param name="entityId">entity to add component to</param>
        /// <returns>component if added successfully</returns>
        /// <exception cref="ArgumentNullException">if entity not present in context</exception>
        public T AddComponent<T>(int entityId) where T : IComponent, new()
        {
            // capture error if entity does not exist
            if (!mEntities.Contains(entityId))
            {
                throw new ArgumentNullException("AddComponent");
            }

            // create new component
            var newComponent = new T();
            // assign to entity
            newComponent.entityId = entityId;

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


            return newComponent;
        }



        // The removal of components and entities are costly operations which require a kind of batching system
        // in their current state. 
        // This means that I will implement a batch which contains a list of the entities that are to be removed.
        // Then, every so often, the batch will be processed by the context and all the entities and their corresponding 
        // components will be removed from the context fully.
        // In the mean time, the entities that are going to be removed should be marked as "dead" so they are not
        // processed.

        /// <summary>
        /// Removes component of type T from the 
        /// </summary>
        /// <typeparam name="T">IComponent type to be removed</typeparam>
        /// <param name="entityId">Entity to remove component from</param>
        public void RemoveComponent<T>(int entityId) where T : IComponent
        {
            foreach (var component in dComponentsByEntity[entityId])
            {
                if (component.GetType() == typeof(T))
                {
                    dComponentsByEntity[entityId].Remove(component);
                    dComponentsByType[component.GetType()].Remove(component); // does this work?
                }
            }
        }



        // NEEDS TESTING
        /// <summary>
        /// Remove component from context using object reference
        /// </summary>
        /// <param name="component">component to be removed</param>
        public void RemoveComponent(IComponent component)
        {
            if (component == null)
            {
                return;
            }
            dComponentsByEntity[component.entityId].Remove(component);
            dComponentsByType[component.GetType()].Remove(component);
        }



        // SLOW DON"T USE IN FUTURE
        /// <summary>
        /// Attempts to retreive component of type T if present
        /// </summary>
        /// <typeparam name="T">component type to get</typeparam>
        /// <param name="entityId">id of entity to get component from</param>
        /// <returns>Component if found, null if not found</returns>
        public IComponent GetComponent<T>(int entityId) where T : IComponent
        {
            var eComponents = dComponentsByEntity[entityId].ToList();
            foreach (var component in eComponents)
            {
                if (typeof(T) == component.GetType())
                {
                    return component;
                }
            }
            return null;
        }



        /// <summary>
        /// gets all components of entity
        /// </summary>
        /// <param name="id">entity id</param>
        /// <returns>IEnumerable if entity is present in context, null if not</returns>
        public IEnumerable<IComponent> GetComponentsOfEntity(int id)
        {
            HashSet<IComponent> components = null;

            if (dComponentsByEntity.TryGetValue(id, out components))
                return components;

            return null;
        }


        // note* use 
        // "context.GetComponentsOfType<CTexture2D>().Cast<CTexture2D>().ToList();"
        // to get a list of components of specified type


        /// <summary>
        /// gets a IEnumerable of object contains all components of type T
        /// </summary>
        /// <typeparam name="T">component type</typeparam>
        /// <returns>IEnumerable object containing all components of type T</returns>
        public IEnumerable<IComponent> GetComponentsOfType<T>() where T : IComponent
        {
            HashSet<IComponent> components = null;

            if (dComponentsByType.TryGetValue(typeof(T), out components))
                return components;

            return null;
        }

        
        // checks if context contains component
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



        // could cause issues
        // removes all components associated with this entity id
        private void _RemoveComponentsByEntity(int id)
        {
            // remove all components from the type list assocated with this entity
            foreach (var comp in dComponentsByEntity[id])
            { // hmm may not work i dont know though
                dComponentsByType[comp.GetType()].Remove(comp);
            }
            // remove all components from this entities entry
            dComponentsByEntity.Remove(id);
        }


        // stash component in the two dictionaries
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
    }


    */









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

