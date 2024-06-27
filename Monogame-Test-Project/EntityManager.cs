using ECS;
using System.Collections.Generic;
using bitmask;
using System.Diagnostics;
using System;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ECS
{
    public class EntityManager
    {
       
        /*Entity Manager
         * The entity manager is the backbone of this entity component system.
         * It manages the creation, storage, and retrieval of entities aswell as their components.
         * This ECS uses a bitmask in order to retrieve the entities which have the correct components for each system.
         * Each system contains a reference to an entity manager. Each system queries the entity manager for
         *  a list of entities which have the correct components which it can then use to retrieve the actual 
         *  components
         */
        
        private List<Entity> mEntities;
        private Dictionary<int, List<IComponent>> mComponents;

        private static Dictionary<ComponentType, Type> mEnumToComponent =
            new Dictionary<ComponentType, Type>()
            {
            { ComponentType.CTransform, typeof(CTransform) },
            { ComponentType.CCollider, typeof(CTransform) },
            { ComponentType.CRigidBody, typeof(CRigidBody) },
            { ComponentType.CController, typeof(CController) },
            { ComponentType.CPointLight, typeof(CPointLight) },
            { ComponentType.CTexture, typeof(CTexture) },
            { ComponentType.CAnimation, typeof(CAnimation) },
            };

        private static Dictionary<Type, ComponentType> mComponentToEnum =
            new Dictionary<Type, ComponentType>()
            {
            { typeof(CTransform), ComponentType.CTransform },
            { typeof(CCollider), ComponentType.CCollider },
            { typeof(CRigidBody), ComponentType.CRigidBody },
            { typeof(CController), ComponentType.CController },
            { typeof(CPointLight), ComponentType.CPointLight },
            { typeof(CTexture), ComponentType.CTexture },
            { typeof(CAnimation), ComponentType.CAnimation },
            };

        //private Dictionary<int, List<IComponent>> mComponents;

        private Queue<int> availableIds;

        public int EntityCount
        {
            get { return mEntities.Count; }
        }

        public int maxEntities = 0;

        public EntityManager(int maxEntities)
        {
            this.maxEntities = maxEntities;

            availableIds = new Queue<int>();
            for (int i = 0; i < maxEntities; i++)
            {
                availableIds.Enqueue(i);
            }
            mEntities = new List<Entity>();
            mComponents = new Dictionary<int, List<IComponent>>();
        }

        // create new entity
        public Entity CreateEntity()
        {
            Entity newEnt = new Entity(availableIds.Dequeue());
            mEntities.Add(newEnt);
            mComponents.Add(newEnt.id, new List<IComponent>());
            return newEnt;
        }

        public void RemoveEntity(Entity entity)
        {
            mComponents.Remove(entity.id);
            mEntities.Remove(entity);
        }

        // add component to entity by entity id
        public T AddComponent<T>(int eId) where T : IComponent, new()
        {

            Debug.Assert(mEntities.Find(x => x.id == eId) != null);
            Debug.Assert(mComponents.ContainsKey(eId));

            T newComp = new T();
            mComponents[eId].Add(newComp);

            // adjust entity component bit mask
            Entity e = mEntities.Find(x => x.id == eId);
            e.cMask[(int)mComponentToEnum[typeof(T)]] = true;

            return newComp;
        }

        // add existing component instance to entity via entity id
        public T AddComponent<T>(int eId, T component) where T : IComponent, new()
        {
            Debug.Assert(mEntities.Find(x => x.id == eId) != null);
            Debug.Assert(mComponents.ContainsKey(eId));

            mComponents[eId].Add(component);

            Entity e = mEntities.Find(x => x.id == eId);
            e.cMask[(int)mComponentToEnum[typeof(T)]] = true;
            return component;
        }

        // add component of type T to entity 
        public T AddComponent<T>(Entity entity) where T : IComponent, new()
        {
            Debug.Assert(mEntities.Contains(entity));
            Debug.Assert(mComponents.ContainsKey(entity.id));

            T newComp = new T();
            mComponents[entity.id].Add(newComp);


            // adjust entity component bit mask
            entity.cMask[(int)mComponentToEnum[typeof(T)]] = true;

            return newComp;
        }

        // add existing component of type T to entity
        public T AddComponent<T>(Entity entity, T component) where T : IComponent
        {
            Debug.Assert(mEntities.Contains(entity));
            Debug.Assert(mComponents.ContainsKey(entity.id));
            Debug.Assert(component != null);

            mComponents[entity.id].Add(component);

            entity.cMask[(int)mComponentToEnum[typeof(T)]] = true;
            return component;
        }


        public Entity GetEntity(int index)
        {
            Debug.Assert(index < mEntities.Count);
            return mEntities[index];
        }


        public bool HasComponent<T>(int entityId) where T : IComponent 
        {
            Debug.Assert(mComponents.ContainsKey(entityId));

            if (mComponents[entityId].OfType<T>().Any()) {
                return true;
            }
            return false;
        }

        public IComponent TryGetComponent<T>(int entityId) where T : IComponent
        {
            if (mComponents.TryGetValue(entityId, out var component))
            {
                foreach(var item in mComponents[entityId])
                {
                    if (item.GetType() == typeof(T))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public IComponent GetComponent<T>(int entityId) where T : IComponent
        {
            Debug.Assert(mComponents.ContainsKey(entityId));

            return mComponents[entityId].Find(x => x.GetType() == typeof(T));
        }

        public void SetComponent<T>(int entityId, T component) where T : IComponent
        {
            Debug.Assert(mComponents.ContainsKey(entityId));

            for (int i = 0; i < mComponents[entityId].Count; i++)
            {
                if (mComponents[entityId][i].GetType() == typeof(T))
                {
                    mComponents[entityId][i] = component;
                }
            }

        }



        

        public IEnumerable<Entity> GetEntities(Bitmask signature)
        {
            List<Entity> entities = new List<Entity>();
            foreach (var entity in mEntities)
            {
                if (entity.cMask.AND(signature).isEqual(signature))
                {
                    entities.Add(entity);
                }
                
            }
            return entities;
        }


        public IEnumerable<int> GetEntityIds(Bitmask signature)
        {
            List<int> entityIds = new List<int>();
            foreach (var entity in mEntities)
            {
                if (entity.cMask.AND(signature).isEqual(signature))
                {
                    entityIds.Add(entity.id);
                }
            }
            return entityIds;
        }

    }

}

