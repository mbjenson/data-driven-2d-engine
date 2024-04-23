using ECS;
using System.Collections.Generic;
using bitmask;
using System.Diagnostics;
using System;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Graphics;

namespace ECS
{
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

        private List<Entity> mEntities;

        private Dictionary<int, List<IComponent>> mComponents;

        private static Dictionary<ComponentType, Type> mEnumToComponent =
            new Dictionary<ComponentType, Type>()
            {
            { ComponentType.CTransform, typeof(CTransform) },
            { ComponentType.CCollider, typeof(CTransform) },
            { ComponentType.CRigidBody, typeof(CRigidBody) },
            { ComponentType.CController, typeof(CController) },
            };

        private static Dictionary<Type, ComponentType> mComponentToEnum =
            new Dictionary<Type, ComponentType>()
            {
            { typeof(CTransform), ComponentType.CTransform },
            { typeof(CCollider), ComponentType.CCollider },
            { typeof(CRigidBody), ComponentType.CRigidBody },
            { typeof(CController), ComponentType.CController },
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


        // don't have this for now too complicated and unecessary

        //public void RemoveComponent<T>(Entity entity) where T : IComponent
        //{
        //    Debug.Assert(mComponents.ContainsKey(entity.id
        //    for (int i = 0; i < mComponents[entity.id].Count; i++)
        //    {
        //        if (mComponents[entity.id][i].GetType() == typeof(T))
        //        {
        //            mComponents[entity.id].RemoveAt(i);
        //        }
        //    }
        //}

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

        public IEnumerable<Entity> GetEntities(Bitmask signature)
        {
            List<Entity> entities = new List<Entity>();
            foreach (var entity in mEntities)
            {
                if (entity.cMask.AND(signature).isEqual(signature))
                {
                    entities.Add(entity);
                }
                //if (entity.cMask.AND(signature))
                //{
                //    Debug.WriteLine("after AND: ");
                //    entity.cMask.AND(signature).Print();
                //    Debug.WriteLine("entity mask: ");
                //    entity.cMask.Print();
                //    Debug.WriteLine("signature mask: ");
                //    signature.Print();

                //    entities.Add(entity);
                //}
            }
            return entities;
        }


        public IEnumerable<int> GetEntityIds(Bitmask signature)
        {
            List<int> entityIds = new List<int>();
            foreach (var entity in mEntities)
            {
                if (entity.cMask.AND(signature))
                {
                    entityIds.Add(entity.id);
                }
            }
            return entityIds;
        }

    }

}

