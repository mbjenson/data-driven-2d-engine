﻿


using bitmask;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogame_Test_Project;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Principal;
using System.Xml;
using viewStuff;


namespace ECS.Systems
{
    /*
    a system will request a set of entities from the entity manager
    and then perform operations on those entities 
    */

    public abstract class UpdateSystem
    {
        public abstract void Update(GameTime gameTime);
    }


    /*
        Checks input that is happening from the different controllers
        sets values (perhaps in the controller component soon) so
        that the entity with the controller component can respond to the input
     
        currently:
            gets all entities with input component and rigidbody and 
            moves them around based on controller input
     */
    public class InputSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        public InputSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CController] = true;
            signature[ComponentType.CTransform] = true;
            // used to be cRigid
        }

        public override void Update(GameTime gameTime)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            
            foreach (Entity e in entities)
            {
                CTransform transA = (CTransform)eMan.GetComponent<CTransform>(e.id);
                CController contA = (CController)eMan.GetComponent<CController>(e.id);
                //CRigidBody rigA = (CRigidBody)eMan.GetComponent<CRigidBody>(e.id);
                
                // update controller with gamepad state
                GamePadState gamePadState = GamePad.GetState(contA.controllerIndex);
                // perform actions based on input

                gamePadState.ThumbSticks.Left.Normalize();
                // Debug.WriteLine(contA.gamePadState.ThumbSticks.Left);
                
                Vector2 stickVals = new Vector2(
                            gamePadState.ThumbSticks.Left.X,
                            -gamePadState.ThumbSticks.Left.Y);

                contA.movement = stickVals;
                // FUNCTION END HERE
                // temporarily I will change the player's actual position here
                // but later implement movement system which handles
                // actually putting the controller parts into movement / physics
                transA.Move(contA.movement);
                //rigA.velocity = contA.movement * 100f;
            }
        }
    }


    // TODO: introduce verlet integration
    public class PhysicsSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        public PhysicsSystem(EntityManager eMan)
        {
            Debug.Assert(eMan != null);
            this.eMan = eMan;

            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CCollider] = true;
            signature[ComponentType.CTransform] = true;
            signature[ComponentType.CRigidBody] = true;
            // signature[ComponentType.CPhysInfo] = true;
            // * perhaps this component will carry information about
            // * whether objects are static or dynamic in the world
            // * this may help when solving collisions against a lot of objects
            
        }

        public override void Update(GameTime gameTime)
        {
            UpdatePhysics(gameTime);
        }

        // https://code.tutsplus.com/how-to-create-a-custom-2d-physics-engine-the-basics-and-impulse-resolution--gamedev-6331t
        private void UpdatePhysics(GameTime gameTime)
        {
            float dt = gameTime.ElapsedGameTime.Seconds;
            // get all entities with rigidbody, collider, and transform
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            foreach (Entity e in entities)
            {
                CTransform trans = 
                    (CTransform)eMan.GetComponent<CTransform>(e.id);
                CCollider col = 
                    (CCollider)eMan.GetComponent<CCollider>(e.id);
                CRigidBody rig = 
                    (CRigidBody)eMan.GetComponent<CRigidBody>(e.id);


                trans.lastPosition = trans.position;
                trans.position +=
                    (trans.position - trans.lastPosition) + rig.acceleration * (dt * dt);

                rig.acceleration = Vector2.Zero;

                //rig.velocity = rig.velocity + rig.acceleration * dt;
                //trans.position = trans.position + rig.velocity * dt;






                //rig.velocity = trans.position - trans.lastPosition;

                //trans.position = 
                //    trans.position * 2 - trans.lastPosition + rig.acceleration * dt * dt;

                //rig.acceleration = trans.position - trans.lastPosition;
                //rig.velocity = trans.position - trans.lastPosition;

                //trans.position += rig.velocity *
                //    gameTime.ElapsedGameTime.Seconds + 0.5f *
                //    rig.acceleration * gameTime.ElapsedGameTime.Seconds *
                //    gameTime.ElapsedGameTime.Seconds;

                //rig.velocity += rig.acceleration * gameTime.ElapsedGameTime.Seconds;
                
                //trans.position +=
                //    rig.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
    }






    /*
     * note: Move to different file eventually
     * TODO: figure out how to manage both static and dynamic objects
     *       in the world. I want to be able to handle dynamic objects
     *       colliding with the world and dynamic objects colliding with one
     *       another in a similar way.
     *       
     *       Quadtree: I want to place all things with colliders into
     *       a quadtree. When querying the quadtree for collisions that
     *       are occuring, in the same way my other proj did, returns
     *       a list of each object and the items colliding with them
     *       then, it is a simple matter of going through the list
     *       and resolving each collision. These lists may contain
     *       static map boundaries static objects as well as dynamic things.
     *       when given this list, be able to handle these collisions
     *       all in a homogenous way as to avoid type checking and 
     *       if else statements. we can simply throw pairs of
     *       objects into a function that knows how to handle it
     */
    public class CollisionSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;
        public CollisionSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CTransform] = true;
            signature[ComponentType.CCollider] = true;
            signature[ComponentType.CRigidBody] = true;
        }
        
        public override void Update(GameTime gameTime)
        {
            SolveAllCollisions(gameTime);
        }

        /*
         Need to make it so that only the dynamic entities solve
         collisions against the static ones and dynamic one
         against other dynamic ones (like player against wall).
         there is no need to solve collisions for static objects against static
         objects.
         */
        private void SolveAllCollisions(GameTime gameTime)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            for (int i = 0; i < entities.Count; i++)
            {
                CTransform transform1 =
                    (CTransform)eMan.GetComponent<CTransform>(entities[i].id);
                CCollider collider1 =
                    (CCollider)eMan.GetComponent<CRectCollider>(entities[i].id);
                CRigidBody rig1 =
                    (CRigidBody)eMan.GetComponent<CRigidBody>(entities[i].id);

                for (int j = 0; j < entities.Count; j++)
                {
                    if (j == i) { continue; }

                    CTransform transform2 =
                        (CTransform)eMan.GetComponent<CTransform>(entities[j].id);
                    CCollider collider2 =
                        (CCollider)eMan.GetComponent<CRectCollider>(entities[j].id);
                    CRigidBody rig2 =
                        (CRigidBody)eMan.GetComponent<CRigidBody>(entities[j].id);
                    
                    
                    if (collider1.GetType() == typeof(CRectCollider) &&
                        collider2.GetType() == typeof(CRectCollider))
                    {
                        ResolveDynamicAABB(
                            transform1, collider1 as CRectCollider, rig1,
                            transform2, collider2 as CRectCollider, rig2);
                    }
                }

                // TODO: figure out how to differentiate from each type
                //       of collider here so that circle-rect, rect-rect, 
                //       and circle-circle collisions can be resolved 
            }
        }

        // two ideas:
        //  use verlet integration where only the position will be altered here,
        //      but this altering in position is used by the physics system to 
        //      calculate the object's new velocity (by using previous and current
        //      position)
        //
        //  the other is to do what I am doing right now and to let the system take
        //      can of adjusting the velocity like it is.
        //
        //  I am leaning towards the first one for a more fun experience.
        //
        //  these two would be similar except that the second would
        //  make it so that if an object stops, its velocity would reflect this
        //  
        //  
        //  
        //  for some reason the object is not moving as expected, I think it is
        //  becuase it is not being pushed as far in a given instance as it would if
        //  only velocity adjustments were used and not both velocity and transform
        //  position

        private void ResolveDynamicAABB(
            CTransform transA, CRectCollider colA, CRigidBody rigA,
            CTransform transB, CRectCollider colB, CRigidBody rigB)
        {
            float rightA = transA.X + colA.Width;
            float bottomA = transA.Y + colA.Height;
            float rightB = transB.X + colB.Width;
            float bottomB = transB.Y + colB.Height;

            float toRightDist = 0, toLeftDist = 0, toBottomDist = 0, toTopDist = 0;

            float massRatioA = rigA.mass / (rigA.mass + rigB.mass);
            float massRatioB = rigB.mass / (rigA.mass + rigB.mass);

            // detetct overlap
            if (transA.X >= transB.X && transA.X <= rightB)
            {   
                toRightDist = rightB - transA.X;
            }
            if (rightA <= rightB && rightA >= transB.X)
            {
                toLeftDist = transB.X - rightA;
            }
            if (transA.Y >= transB.Y && transA.Y <= bottomB)
            {
                toBottomDist = bottomB - transA.Y;
            }
            if (bottomA <= bottomB && bottomA >= transB.Y)
            {
                toTopDist = transB.Y - bottomA;
            }
            
            if (Math.Abs(toRightDist) <= Math.Abs(toBottomDist))
            {
                transA.Move(toRightDist * massRatioB, 0);
                transB.Move(-toRightDist * massRatioA, 0);
                //rigA.velocity += new Vector2(toRightDist * massRatioB, 0);
                //rigB.velocity += new Vector2(-toRightDist * massRatioA, 0);
            }
            else
            {
                transA.Move(0, toBottomDist * massRatioB);
                transB.Move(0, -toBottomDist * massRatioA);
                //rigA.velocity += new Vector2(0, toBottomDist * massRatioB);
                //rigB.velocity += new Vector2(0, -toBottomDist * massRatioA);
            }
            
            if (Math.Abs(toRightDist) <= Math.Abs(toTopDist))
            {
                transA.Move(toRightDist * massRatioB, 0);
                transB.Move(-toRightDist * massRatioA, 0);
                //rigA.velocity += new Vector2(toRightDist * massRatioB, 0);
                //rigB.velocity += new Vector2(-toRightDist * massRatioA, 0);
            }
            else
            {
                transA.Move(0, toTopDist * massRatioB);
                transB.Move(0, -toTopDist * massRatioA);
                //rigA.velocity += new Vector2(0, toTopDist * massRatioB);
                //rigB.velocity += new Vector2(0, -toTopDist * massRatioA);
            }

            if (Math.Abs(toLeftDist) <= Math.Abs(toBottomDist))
            {
                transA.Move(toLeftDist * massRatioB, 0);
                transB.Move(-toLeftDist * massRatioA, 0);
                //rigA.velocity += new Vector2(toLeftDist * massRatioB, 0);
                //rigB.velocity += new Vector2(-toLeftDist * massRatioA, 0);
            }
            else
            {
                transA.Move(0, toBottomDist * massRatioB);
                transB.Move(0, -toBottomDist * massRatioA);
                //rigA.velocity += new Vector2(0, toBottomDist * massRatioB);
                //rigB.velocity += new Vector2(0, -toBottomDist * massRatioA);
            }

            if (Math.Abs(toLeftDist) <= Math.Abs(toTopDist))
            {
                transA.Move(toLeftDist * massRatioB, 0);
                transB.Move(-toLeftDist * massRatioA, 0);
                //rigA.velocity += new Vector2(toLeftDist * massRatioB, 0);
                //rigB.velocity += new Vector2(-toLeftDist * massRatioA, 0);
            }
            else
            {
                transA.Move(0, toTopDist * massRatioB);
                transB.Move(0, -toTopDist * massRatioA);
                //rigA.velocity += new Vector2(0, toTopDist * massRatioB);
                //rigB.velocity += new Vector2(0, -toTopDist * massRatioA);
            }
        }


        private void ResolveDynamicCollision(
            CTransform transA, CRectCollider colA, CRigidBody rigA, 
            CTransform transB, CRectCollider colB, CRigidBody rigB)
        {
            bool[] bEdges = { false, false, false, false };

            float toRightDist = 0f;
            float toLeftDist = 0f;
            float toTopDist = 0f;
            float toBottomDist = 0f;
            
            float rightA = transA.X + colA.Width;
            float bottomA = transA.Y + colA.Height;
            float rightB = transB.X + colB.Width;
            float bottomB = transB.Y + colB.Height;

            
            float massRatioA = rigA.mass / (rigA.mass+ rigB.mass);
            float massRatioB = rigB.mass / (rigA.mass + rigB.mass);


            if (transA.X >= transB.X && transA.X <= rightB)
            //if (dynamicRect.position.X >= staticRect.position.X &&
            //    dynamicRect.position.X <= staticRect.getRight())
            {
                bEdges[0] = true;
                //toRightDist = staticRect.getRight() - dynamicRect.position.X;
                toRightDist = rightB - transA.X;
            }
            if (rightA <= rightB && rightA >= transB.X)
            //if (dynamicRect.getRight() <= staticRect.getRight() &&
            //    dynamicRect.getRight() >= staticRect.position.X)
            {
                bEdges[1] = true;
                //toLeftDist = staticRect.position.X - dynamicRect.getRight();
                toLeftDist = transB.X - rightA;
            }
            if (transA.Y >= transB.Y && transA.Y <= bottomB)
            //if (dynamicRect.position.Y >= staticRect.position.Y &&
            //    dynamicRect.position.Y <= staticRect.getBottom())
            {
                bEdges[2] = true;
                //toBottomDist = staticRect.getBottom() - dynamicRect.position.Y;
                toBottomDist = bottomB - transA.Y;
            }
            if (bottomA <= bottomB && bottomA >= transB.Y)
            //if (dynamicRect.getBottom() <= staticRect.getBottom() &&
            //    dynamicRect.getBottom() >= staticRect.position.Y)
            {
                bEdges[3] = true;
                //toTopDist = staticRect.position.Y - dynamicRect.getBottom();
                toTopDist = transB.Y - bottomA;
            }

            int totalEdges = 0;
            foreach (bool edgeBool in bEdges)
            {
                if (edgeBool)
                {
                    totalEdges++;
                }
            }

            float absToRightDist = Math.Abs(toRightDist);
            float absToLeftDist = Math.Abs(toLeftDist);
            float absToTopDist = Math.Abs(toTopDist);
            float absToBottomDist = Math.Abs(toBottomDist);

            if (totalEdges > 1) // must evaluate which direction to resolve the collision by
            {
                if (bEdges[0] && bEdges[2])
                {
                    if (absToRightDist <= absToBottomDist)
                    {
                        transA.Move(toRightDist * massRatioB, 0);
                        transB.Move(-toRightDist * massRatioA, 0);
                    }
                    else
                    {
                        transA.Move(0, toBottomDist * massRatioB);
                        transB.Move(0, -toBottomDist * massRatioA);
                    }
                }
                else if (bEdges[0] && bEdges[3])
                {
                    if (absToRightDist <= absToTopDist)
                    {
                        transA.Move(toRightDist * massRatioB, 0);
                        transB.Move(-toRightDist * massRatioA, 0);
                    }
                    else
                    {
                        transA.Move(0, toTopDist * massRatioB);
                        transB.Move(0, -toTopDist * massRatioA);
                    }
                }
                else if (bEdges[1] && bEdges[2])
                {
                    if (absToLeftDist <= absToBottomDist)
                    {
                        transA.Move(toLeftDist * massRatioB, 0);
                        transB.Move(-toLeftDist * massRatioA, 0);

                    }
                    else
                    {
                        transA.Move(0, toBottomDist * massRatioB);
                        transB.Move(0, -toBottomDist * massRatioA);
                    }
                }
                else if (bEdges[1] && bEdges[3])
                {
                    if (absToLeftDist <= absToTopDist)
                    {
                        transA.Move(toLeftDist * massRatioB, 0);
                        transB.Move(-toLeftDist * massRatioA, 0);
                    }
                    else
                    {
                        transA.Move(0, toTopDist * massRatioB);
                        transB.Move(0, -toTopDist * massRatioA);
                    }
                }
            }
            else if (totalEdges == 1)
            {
                if (bEdges[0])
                {
                    transA.Move(toRightDist * massRatioB, 0);
                    transB.Move(-toRightDist * massRatioA, 0);
                }
                if (bEdges[1])
                {
                    transA.Move(toLeftDist * massRatioB, 0);
                    transB.Move(-toLeftDist * massRatioA, 0);
                }
                if (bEdges[2])
                {
                    transA.Move(0, toBottomDist * massRatioB);
                    transB.Move(0, -toBottomDist * massRatioA);
                }
                if (bEdges[3])
                {
                    transA.Move(0, toTopDist * massRatioB);
                    transB.Move(0, -toTopDist * massRatioA);
                }
            }
            else
            {
                return;
            }
        }

        // solve collision between a dynamic rect and static rect
        private void ResolveStaticCollision(
            CTransform dTrans, CRectCollider dCol, 
            CTransform sTrans, CRectCollider sCol)
        {
            bool[] bEdges = { false, false, false, false };

            float toRightDist = 0f;
            float toLeftDist = 0f;
            float toTopDist = 0f;
            float toBottomDist = 0f;

            float sRight = sTrans.X + sCol.Width;
            float sBottom = sTrans.Y + sCol.Height;
            float dRight = dTrans.X + dCol.Width;
            float dBottom = dTrans.Y + dCol.Height;
            
            if (dTrans.X >= sTrans.X && dTrans.X <= sRight)
            //if (dynamicRect.position.X >= staticRect.position.X &&
            //    dynamicRect.position.X <= staticRect.getRight())
            {
                bEdges[0] = true;
                //toRightDist = staticRect.getRight() - dynamicRect.position.X;
                toRightDist = sRight - dTrans.X;
            }
            if (dRight <= sRight && dRight >= sTrans.X)
            //if (dynamicRect.getRight() <= staticRect.getRight() &&
            //    dynamicRect.getRight() >= staticRect.position.X)
            {
                bEdges[1] = true;
                //toLeftDist = staticRect.position.X - dynamicRect.getRight();
                toLeftDist = sTrans.X - dRight;
            }
            if (dTrans.Y >= sTrans.Y && dTrans.Y <= sBottom)
            //if (dynamicRect.position.Y >= staticRect.position.Y &&
            //    dynamicRect.position.Y <= staticRect.getBottom())
            {
                bEdges[2] = true;
                //toBottomDist = staticRect.getBottom() - dynamicRect.position.Y;
                toBottomDist = sBottom - dTrans.Y;
            }
            if (dBottom <= sBottom && dBottom >= sTrans.Y)
            //if (dynamicRect.getBottom() <= staticRect.getBottom() &&
            //    dynamicRect.getBottom() >= staticRect.position.Y)
            {
                bEdges[3] = true;
                //toTopDist = staticRect.position.Y - dynamicRect.getBottom();
                toTopDist = sTrans.Y - dBottom;
            }

            int totalEdges = 0;
            foreach (bool edgeBool in bEdges)
            {
                if (edgeBool)
                {
                    totalEdges++;
                }
            }

            float absToRightDist = Math.Abs(toRightDist);
            float absToLeftDist = Math.Abs(toLeftDist);
            float absToTopDist = Math.Abs(toTopDist);
            float absToBottomDist = Math.Abs(toBottomDist);

            if (totalEdges > 1) // must evaluate which direction to resolve the collision by
            {
                if (bEdges[0] && bEdges[2])
                {
                    if (absToRightDist <= absToBottomDist)
                    {
                        dTrans.Move(toRightDist, 0);
                    }
                    else
                    {
                        dTrans.Move(0, toBottomDist);
                    }
                }
                else if (bEdges[0] && bEdges[3])
                {
                    if (absToRightDist <= absToTopDist)
                    {
                        dTrans.Move(toRightDist, 0);
                    }
                    else
                    {
                        dTrans.Move(0, toTopDist);
                    }
                }
                else if (bEdges[1] && bEdges[2])
                {
                    if (absToLeftDist <= absToBottomDist)
                    {
                        dTrans.Move(toLeftDist, 0);
                    }
                    else
                    {
                        dTrans.Move(0, toBottomDist);
                    }
                }
                else if (bEdges[1] && bEdges[3])
                {
                    if (absToLeftDist <= absToTopDist)
                    {
                        dTrans.Move(toLeftDist, 0);
                    }
                    else
                    {
                        dTrans.Move(0, toTopDist);
                    }
                }
            }
            else if (totalEdges == 1)
            {

                if (bEdges[0])
                {
                    dTrans.Move(toRightDist, 0);
                }
                if (bEdges[1])
                {
                    dTrans.Move(toLeftDist, 0);
                }
                if (bEdges[2])
                {
                    dTrans.Move(0, toBottomDist);
                }
                if (bEdges[3])
                {
                    dTrans.Move(0, toTopDist);
                }
            }
            else
            {
                return;
            }
        }
    }





























    //public abstract class UpdateSystem
    //{
    //    protected GameContext context;
    //    public abstract void Update(GameTime gameTime);
    //}



    //public class InputSystem : UpdateSystem
    //{
    //    //public KeyboardState keyState;
    //    public InputSystem(GameContext context) { this.context = context; }

    //    public override void Update(GameTime gameTime)
    //    {
    //        //KeyboardState keyState = Keyboard.GetState();

    //        //List<CController> controllers =
    //        //    context.GetComponentsOfType<CController>().Cast<CController>().ToList();

            

    //        //foreach (var controller in controllers)
    //        //{
    //        //    controller.buttonMap[]
    //        //}
            
    //    }
    //}



    //public class TransformSystem : UpdateSystem
    //{
    //    public TransformSystem(GameContext context) 
    //    {
    //        this.context = context;
    //    }

    //    public override void Update(GameTime gameTime)
    //    {
    //        List<CTransform> transforms = 
    //            context.GetComponentsOfType<CTransform>().Cast<CTransform>().ToList();

    //        foreach (var transform in transforms)
    //        {
    //            float x = (float)Math.Sin(gameTime.ElapsedGameTime.TotalSeconds);
    //            float y = (float)Math.Cos(gameTime.ElapsedGameTime.TotalSeconds);
    //            transform.position += new Vector2(x, y);
    //        }
    //    }
    //}


    //// not an UpdateSystem (some other type)
    //public class RenderingSystem
    //{
    //    GameContext context;

    //    SpriteBatch spriteBatch;

    //    public RenderingSystem(GameContext context, GraphicsDevice device)
    //    {
    //        this.context = context;
    //        spriteBatch = new SpriteBatch(device);
    //    }

    //    // does all of the rendering. Right now only works with textures
    //    public void Render(
    //        GameTime gameTime, RenderTarget2D target, 
    //        GraphicsDevice device, Camera2D cam)
    //    {
    //        List<CTexture2D> textures = 
    //            context.GetComponentsOfType<CTexture2D>().Cast<CTexture2D>().ToList();

    //        // prep device
    //        device.SetRenderTarget(target);
    //        device.DepthStencilState = 
    //            new DepthStencilState() { DepthBufferEnable = true };
    //        device.Clear(Color.CornflowerBlue);

    //        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
    //            SamplerState.PointClamp, transformMatrix: cam.TransformMatrix);

    //        foreach (var thisTexture in textures)
    //        {
    //            CTransform thisTransform = 
    //                (CTransform)context.GetComponent<CTransform>(thisTexture.entityId);

    //            if (thisTransform == null)
    //            {
    //                continue;
    //            }

    //            spriteBatch.Draw(
    //                context.spriteMan.dSpriteSheets[thisTexture.spriteSheetId],
    //                thisTransform.position,
    //                context.spriteMan.dSpriteRects
    //                    [thisTexture.spriteSheetId][thisTexture.spriteId],
    //                Color.White,
    //                thisTransform.rotation,
    //                thisTexture.textureOffset,
    //                thisTransform.scale,
    //                SpriteEffects.None,
    //                thisTransform.layerDepth
    //            );
    //        }

    //        spriteBatch.End();

    //        device.SetRenderTarget(null);
    //        device.Clear(Color.CornflowerBlue);
    //    }
    //}
    


}


