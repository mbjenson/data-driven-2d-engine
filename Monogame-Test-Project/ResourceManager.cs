using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ECS
{

    // Currently this is equivilent to a dictionary, might add more functionality in the future which would make
    // it worth using but as for right now, it is not worth using.
    public class IResourceManager<ID, T>
    {
        protected Dictionary<ID, T> items;

        public IResourceManager()
        {
            this.items = new Dictionary<ID, T>();
        }

        public void Add(ID identifier, T item)
        {
            if (identifier != null && item != null && items.ContainsKey(identifier))
            {
                items.Add(identifier, item);
            }
        }

        public T Get(ID identifier)
        {
            return items[identifier];
        }

        public void Remove(ID identifier)
        {
            items.Remove(identifier);
        }
    }


    
}