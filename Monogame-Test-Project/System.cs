


using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ECS
{

    public abstract class System
    {
        protected Context context;
        public abstract void Update(GameTime gameTime);
    }





    public class TransformSystem : System
    {
        public TransformSystem(Context context) 
        {
            this.context = context;
        }

        public override void Update(GameTime gameTime)
        {
            List<CTransform> transforms = context.GetComponentsOfType<CTransform>().Cast<CTransform>().ToList();

            foreach (var transform in transforms)
            {
                float x = (float)Math.Sin(gameTime.ElapsedGameTime.TotalSeconds);
                float y = (float)Math.Cos(gameTime.ElapsedGameTime.TotalSeconds);
                transform.position += new Vector2(x, y);
            }
        }
    }


    
    


}


