﻿
using bitmask;
using ECS.Systems;
using ECS;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;


/*
    Checks input that is happening from the different controllers
    sets values (perhaps in the controller component soon) so
    that the entity with the controller component can respond to the input

    The reason i have this seperate from the action system is because the
    input system only gets the input components while the action system 
    uses the controller component information and turns it into action.
    That may not be the best way to do it but I don't have any other
    ideas right now.
 */
namespace ECS.Systems
{
    public class InputSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        private const int MAX_PLAYER_COUNT = 4;
        //List<CController> controllerList;

        public InputSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CController] = true;

        }


        public override void Update(GameTime gameTime)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            foreach (Entity e in entities)
            {
                CController cont = (CController)eMan.GetComponent<CController>(e.id);
                UpdatePlayerController(cont);
            }
        }

        private void UpdatePlayerController(CController cont)
        {
            // get controller gamepad state
            GamePadState gamePadState = GamePad.GetState(cont.controllerIndex);

            // set controller component values based on input
            // left thumb stick
            gamePadState.ThumbSticks.Left.Normalize();
            gamePadState.ThumbSticks.Right.Normalize();
            Vector2 leftStickVals = new Vector2(
                        gamePadState.ThumbSticks.Left.X,
                        -gamePadState.ThumbSticks.Left.Y);
            cont.leftStick = leftStickVals;

            Vector2 rightStickVals = new Vector2(
                gamePadState.ThumbSticks.Right.X,
                -gamePadState.ThumbSticks.Right.Y);
            cont.rightStick = rightStickVals;
        }
    }
}
