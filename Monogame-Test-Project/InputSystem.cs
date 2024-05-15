
using bitmask;
using ECS.Systems;
using ECS;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;


/*
    Checks input that is happening from the different controllers
    sets values (perhaps in the controller component soon) so
    that the entity with the controller component can respond to the input

    currently:
        gets all entities with input component and rigidbody and 
        moves them around based on controller input
 */
namespace ECS.Systems
{
    public class InputSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        public InputSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CController] = true;
        }


        //public void RegisterController(PlayerIndex playerIndex)
        //{

        //}

        public override void Update(GameTime gameTime)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            foreach (Entity e in entities)
            {
                CController contA = (CController)eMan.GetComponent<CController>(e.id);
                // get controller gamepad state
                GamePadState gamePadState = GamePad.GetState(contA.controllerIndex);

                // set controller component values based on input

                // left thumb stick
                gamePadState.ThumbSticks.Left.Normalize();
                Vector2 stickVals = new Vector2(
                            gamePadState.ThumbSticks.Left.X,
                            -gamePadState.ThumbSticks.Left.Y);
                contA.movement = stickVals;
            }
        }
    }
}
