using bitmask;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ECS
{
    public class EntityManagerDebug
    {
        public EntityManager eMan;
        public EntityManagerDebug(EntityManager eMan) { this.eMan = eMan; }

        public void PrintEntityPositions()
        {
            Bitmask sig = new Bitmask((int)ComponentType.Count);
            sig[ComponentType.CTransform] = true;
            List<Entity> ents = eMan.GetEntities(sig).ToList();
            if (ents.Count <= 0 ) 
            {
                Debug.WriteLine("no entities to print");
                return;
            }
            Debug.WriteLine(ents.Count);
            foreach (Entity ent in ents)
            {
                
                CTransform thisTrans = (CTransform)eMan.GetComponent<CTransform>(ent.id);

                Debug.WriteLine("Entity: " + ent.id + ": " + thisTrans.position.X + ", " + thisTrans.position.Y);
            }
        }
    }
}