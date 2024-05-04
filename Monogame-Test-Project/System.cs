


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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
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
            signature[ComponentType.CRigidBody] = true; // temp till I get movement system working
            // used to be cRigid
        }

        public override void Update(GameTime gameTime)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            
            foreach (Entity e in entities)
            {
                CTransform transA = (CTransform)eMan.GetComponent<CTransform>(e.id);
                CController contA = (CController)eMan.GetComponent<CController>(e.id);
                CRigidBody rigA = (CRigidBody)eMan.GetComponent<CRigidBody>(e.id);
                
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

                // GOOD player movemnt code which uses physics and
                // not just hard coded change in position


                // calculate additional velocity as scalar times stick input
                //rigA.velocity += contA.movement * 10f; // movement system

                // calculate friction proportional to velocity (and mass later)
                //rigA.acceleration += rigA.velocity * -0.065f; // physics system

                // apply friction to velocity
                //rigA.velocity += rigA.acceleration; // Physics system

                // set acceleration to zero
                //rigA.acceleration = Vector2.Zero; // physics system



                // later, this movement code must be implemented in the movement system.
                // The movement system will handle taking all entities with a controller
                //      component and ensuring that their velocity is set according to 
                //      the directional input (Vector2 CController.movement)
                // The physics system will ensure that the acceleration is altered according
                //      to the friction of the room the player is in (perhaps this can be done
                //      (using force)
            }
        }
    }


    


    public class MovementSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        public MovementSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CController] = true;
            signature[ComponentType.CRigidBody] = true;
        }


        public override void Update(GameTime gameTime)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            foreach (Entity e in entities)
            {
                CController cont = (CController)eMan.GetComponent<CController>(e.id);
                CRigidBody rig = (CRigidBody)eMan.GetComponent<CRigidBody>(e.id);

                if (cont == null || rig == null)
                {
                    throw new Exception("MovementSystem.Update: controller or rigidbody null");
                }

                // for now, all players have the same movement speed
                float moveSpeed = 10f;
                rig.velocity += cont.movement * moveSpeed;
                // limit player speed gained by input this way
                rig.acceleration += rig.velocity * -0.06f;
            }
        }
    }



    

    // have universal quadtree which contains things like force fields, 
    // bounding boxes, etc. This quadtree can be queried for all intersecting
    // objects and other things that can be affected depending on position in space

    public class PhysicsSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;


        // used to pack data
        private struct PhysicsRect
        {
            public CTransform transform;
            public CRectCollider collider;
            public CRigidBody rigidBody;
            
            public PhysicsRect(CTransform t, CRectCollider c, CRigidBody r)
            {
                this.transform = t;
                this.collider = c;
                this.rigidBody = r;
            }
        }

        private struct PhysicsCircle
        {
            public CTransform transform;
            public CCircleCollider collider;
            public CRigidBody rigidBody;

            public PhysicsCircle(CTransform t, CCircleCollider c, CRigidBody r)
            {
                this.transform = t;
                this.collider = c;
                this.rigidBody = r;
            }
        }


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

        private void UpdatePhysics(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            UpdateMovement(dt);
            
            SolveCollisions(dt);

            



            // get all entities with rigidbody, collider, and transform
            //List<Entity> entities = eMan.GetEntities(signature).ToList();
            //foreach (Entity e in entities)
            //{



                //CTransform trans = 
                //    (CTransform)eMan.GetComponent<CTransform>(e.id);
                //CCollider col = 
                //    (CCollider)eMan.GetComponent<CCollider>(e.id);
                //CRigidBody rig = 
                //    (CRigidBody)eMan.GetComponent<CRigidBody>(e.id);


                //trans.lastPosition = trans.position;
                //trans.position +=
                //    (trans.position - trans.lastPosition) + rig.acceleration * (dt * dt);

                //rig.acceleration = Vector2.Zero;



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
            //}
        }


        // TODO: send data to other classes in chunks like structs
        // this will allow for easier sending of data that does not require 8 parameters
        private void SolveCollisions(float dt)
        {
            Dictionary<int, PhysicsRect> physicsObjects =
                new Dictionary<int, PhysicsRect>();

            // temporary to prevent double resolution
            Dictionary<int, int> intersections = new Dictionary<int, int>();
            
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            for (int i = 0; i < entities.Count; i++)
            {
                CTransform tA =
                    (CTransform)eMan.GetComponent<CTransform>(entities[i].id);
                CCollider cA =
                    (CCollider)eMan.GetComponent<CRectCollider>(entities[i].id);
                CRigidBody rA =
                    (CRigidBody)eMan.GetComponent<CRigidBody>(entities[i].id);

                // testing if I can pack data in struct
                PhysicsRect rectA = new PhysicsRect(tA, cA as CRectCollider, rA);
                if (!physicsObjects.ContainsKey(i))
                {
                    physicsObjects.Add(i, rectA);
                }

                for (int j = 0; j < entities.Count; j++)
                {
                    if (j == i) { continue; }

                    CTransform tB =
                        (CTransform)eMan.GetComponent<CTransform>(entities[j].id);
                    CCollider cB =
                        (CCollider)eMan.GetComponent<CRectCollider>(entities[j].id);
                    CRigidBody rB =
                        (CRigidBody)eMan.GetComponent<CRigidBody>(entities[j].id);

                    if (cA.GetType() == typeof(CRectCollider) &&
                        cB.GetType() == typeof(CRectCollider))
                    {
                        if (AABBvsAABB(
                            tA, cA as CRectCollider,
                            tB, cB as CRectCollider))
                        {
                            if (intersections.ContainsKey(i) && intersections[i] == j)
                            {
                                continue;
                            }
                            else if (intersections.ContainsKey(j) && intersections[j] == i)
                            {
                                continue;
                            }
                            else
                            {
                                // TODO: don't use a dictionary for this.
                                if (!intersections.ContainsKey(i))
                                {
                                    intersections.Add(i, j);
                                }

                                // testing if i can pack data into structs
                                if (!physicsObjects.ContainsKey(j))
                                {
                                    PhysicsRect rectB = new PhysicsRect(tB, cB as CRectCollider, rB);
                                    physicsObjects.Add(j, rectB);
                                }
                            }


                            //ResolveCollision(tA, cA as CRectCollider,
                            //    rA, tB, cB as CRectCollider, rB);
                            
                        }
                    }
                }

                // TODO: figure out how to differentiate from each type
                //       of collider here so that circle-rect, rect-rect, 
                //       and circle-circle collisions can be resolved 
            }

            foreach (int i in intersections.Keys)
            {

                int j = intersections[i];
                // not very cache friendly i don't think
                ResolveCollision(physicsObjects[i], physicsObjects[j]);
            }

            // temporary, in future we will use the quadtree for getting intersections            
            //foreach (int i in intersections.Keys)
            //{

            //    int j = intersections[i];
            //    CTransform tA =
            //        (CTransform)eMan.GetComponent<CTransform>(entities[i].id);
            //    CCollider cA =
            //        (CCollider)eMan.GetComponent<CRectCollider>(entities[i].id);
            //    CRigidBody rA =
            //        (CRigidBody)eMan.GetComponent<CRigidBody>(entities[i].id);

            //    CTransform tB =
            //            (CTransform)eMan.GetComponent<CTransform>(entities[j].id);
            //    CCollider cB =
            //        (CCollider)eMan.GetComponent<CRectCollider>(entities[j].id);
            //    CRigidBody rB =
            //        (CRigidBody)eMan.GetComponent<CRigidBody>(entities[j].id);

            //    ResolveCollision(tA, cA as CRectCollider, rA, tB, cB as CRectCollider, rB);
            //}
        }

        // basic updating movement from velocity function
        private void UpdateMovement(float dt)
        {
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            for (int i = 0; i < entities.Count; i++)
            {
                CTransform tA =
                    (CTransform)eMan.GetComponent<CTransform>(entities[i].id);
                CRigidBody rA =
                    (CRigidBody)eMan.GetComponent<CRigidBody>(entities[i].id);


                // calculate friction
                float kN = 0.025f; // (TEMP) surface coefficient of dynamic friction
                //                 direction and size * intensity of friction * related to mass
                rA.acceleration += -rA.velocity * kN * (1f - (1f / rA.mass));

                // calculate velocity
                rA.velocity += rA.acceleration;

                // reset acceleration
                rA.acceleration = Vector2.Zero;

                // update position from velocity
                tA.position += rA.velocity * dt;
            }
        }


        private bool AABBvsAABB(
            CTransform transA, CRectCollider colA,
            CTransform transB, CRectCollider colB)
        {
            // ensure no seperating axis
            if (transA.X + colA.Width < transB.X || transA.X > transB.X + colB.Width)
            {
                return false;
            }

            if (transA.Y + colA.Height < transB.Y || transA.Y > transB.Y + colB.Height)
            {
                return false;
            }

            return true;
        }
        

        private bool CirclevsCircle(
            CTransform transA, CCircleCollider colA,
            CTransform transB, CCircleCollider colB)
        {
            float r = colA.radius + colB.radius;
            r *= r;
            return r < Math.Pow(transA.X + transB.X, 2) + Math.Pow(transA.Y + transB.Y, 2);
        }


        private Vector2 GetCollisionNormal(PhysicsRect A, PhysicsRect B)
        {
            Vector2 colNormal = Vector2.Zero;

            float toRightDist = Math.Abs(B.transform.X - A.transform.X + A.collider.Width);
            float toLeftDist = Math.Abs(-1f * (A.transform.X - B.transform.X + B.collider.Width));
            float toBotDist = Math.Abs(B.transform.Y - A.transform.Y + A.collider.Height);
            float toTopDist = Math.Abs(-1f * (A.transform.Y - B.transform.Y + B.collider.Height));

            float xOffset = Math.Min(toRightDist, toLeftDist);
            float yOffset = Math.Min(toTopDist, toBotDist);


            if (xOffset < yOffset)
            {
                if (A.transform.X - B.transform.X > 0f)
                {
                    //Debug.Print("box b is to the right of box a");
                    colNormal.X = 1f;

                    //colNormal.X = B.transform.X - A.transform.X + cA.Width;
                }
                else if (A.transform.X - B.transform.X < 0f)
                {
                    //Debug.Print("box b is to the left of box a");
                    colNormal.X = -1f;
                    //colNormal.X = -1f * (A.transform.X - B.transform.X + cB.Width);
                }
            }
            else
            {
                if (A.transform.Y - B.transform.Y < 0f)
                {
                    //Debug.Print("box b is on top of box a");
                    colNormal.Y = -1f;
                    //colNormal.Y = -1f * (A.transform.Y - B.transform.Y + cB.Height);
                }
                else if (A.transform.Y - B.transform.Y > 0f)
                {
                    //Debug.Print("box b is below box a");
                    colNormal.Y = 1f;
                    //colNormal.Y = B.transform.Y - A.transform.Y + cA.Height;
                }
            }

            return colNormal;
        }


        //private Vector2 GetCollisionNormal(CTransform tA, CRectCollider cA, CRigidBody rA,
        //    CTransform tB, CRectCollider cB, CRigidBody rB)
        //{
        //    Vector2 colNormal = Vector2.Zero;

        //    float toRightDist = Math.Abs(tB.X - tA.X + cA.Width);
        //    float toLeftDist = Math.Abs(-1f * (tA.X - tB.X + cB.Width));
        //    float toBotDist = Math.Abs(tB.Y - tA.Y + cA.Height);
        //    float toTopDist = Math.Abs(-1f * (tA.Y - tB.Y + cB.Height));

        //    float xOffset = Math.Min(toRightDist, toLeftDist);
        //    float yOffset = Math.Min(toTopDist, toBotDist);


        //    if (xOffset < yOffset)
        //    {
        //        if (tA.X - tB.X > 0f)
        //        {
        //            //Debug.Print("box b is to the right of box a");
        //            colNormal.X = 1f;

        //            //colNormal.X = tB.X - tA.X + cA.Width;
        //        }
        //        else if (tA.X - tB.X < 0f)
        //        {
        //            //Debug.Print("box b is to the left of box a");
        //            colNormal.X = -1f;
        //            //colNormal.X = -1f * (tA.X - tB.X + cB.Width);
        //        }
        //    }
        //    else
        //    {
        //        if (tA.Y - tB.Y < 0f)
        //        {
        //            //Debug.Print("box b is on top of box a");
        //            colNormal.Y = -1f;
        //            //colNormal.Y = -1f * (tA.Y - tB.Y + cB.Height);
        //        }
        //        else if (tA.Y - tB.Y > 0f)
        //        {
        //            //Debug.Print("box b is below box a");
        //            colNormal.Y = 1f;
        //            //colNormal.Y = tB.Y - tA.Y + cA.Height;
        //        }
        //    }

        //    return colNormal;
        //}

        // this version uses a physics rect struct to get all of the data about
        // each object instead of accessing the raw components

        // TODO: collisions don't take mass into account when player makes them.
        private void ResolveCollision(PhysicsRect A, PhysicsRect B)
        {
            // calculate relative velocity
            Vector2 relativeVel = B.rigidBody.velocity - A.rigidBody.velocity;

            // Calculate relative velocity in terms of the normal direction
            //      collision normal can be got by finding in which direction
            //      the objects have moved in to overlap

            //Vector2 colNormal = GetCollisionNormal(
            //    A.transform, A.collider, A.rigidBody, 
            //    B.transform, B.collider, B.rigidBody);

            Vector2 colNormal = GetCollisionNormal(A, B);

            float velAlongNormal = Vector2.Dot(relativeVel, colNormal);

            // calculate resitution ( I will be using this hard coded for simplicity)
            //                              later add physics info class or
            //                              simply add it to rigidbody class
            //float eps = 0.5f; // TEMP
            float eps = 4f;

            // Calculate impulse scalar
            float j = -(1 + eps) * velAlongNormal;
            j = j / 1 / A.rigidBody.mass + 1 / B.rigidBody.mass;

            // apply impulse
            Vector2 impulse = j * colNormal;

            float massSum = A.rigidBody.mass + B.rigidBody.mass;
            float massRatio = B.rigidBody.mass / massSum;
            A.rigidBody.velocity -= massRatio * impulse;

            massRatio = A.rigidBody.mass / massSum;
            B.rigidBody.velocity += massRatio * impulse;



            // position correction
            float toRightDist = 
                B.transform.X - A.transform.X + A.collider.Width;
            float toLeftDist = 
                -1f * (A.transform.X - B.transform.X + B.collider.Width);

            float toBotDist = 
                B.transform.Y - A.transform.Y + A.collider.Height;
            float toTopDist = 
                -1f * (A.transform.Y - B.transform.Y + B.collider.Height);

            float xMin = 0f;
            float yMin = 0f;

            if (Math.Abs(toRightDist) < Math.Abs(toLeftDist))
            {
                xMin = toRightDist;
            }
            else
            {
                xMin = toLeftDist;
            }
            if (Math.Abs(toTopDist) < Math.Abs(toBotDist))
            {
                yMin = toTopDist;
            }
            else
            {
                yMin = toBotDist;
            }

            float penDepth = 0f;
            if (Math.Abs(xMin) < Math.Abs(yMin))
            {
                penDepth = xMin;
            }
            else
            {
                penDepth = yMin;
            }

            float massBPerc = B.rigidBody.mass / massSum;
            float massAPerc = A.rigidBody.mass / massSum;

            if (penDepth < 0)
            {
                A.transform.position -= penDepth * massBPerc * colNormal;
                B.transform.position += penDepth * massAPerc * colNormal;
            }
            else
            {
                A.transform.position += penDepth * massBPerc * colNormal;
                B.transform.position -= penDepth * massAPerc * colNormal;
            }

            //Vector2 correction =
            //    penDepth / ((1f / A.rigidBody.mass) + (1f / B.rigidBody.mass)) * massAPerc * colNormal;
            //if (penDepth < 0)
            //{
            //    A.transform.position -= correction * massBPerc;
            //    B.transform.position += correction * massAPerc;
            //}
            //else
            //{
            //    A.transform.position += correction * massAPerc;
            //    B.transform.position -= correction * massBPerc;
            //}

        }


        //// does not take into account time steps
        //// to take into account time steps, take dt and divide it up into n parts,
        ////   run the collision check with that object at each of the n time steps
        ////   to do this, take A.curPos += A.velocity * (dt / n) * i;
        ////   then check for collision. if none, advance to next time step and check
        ////   again. If there is collision, solve it and move to next objects
        ///

        // this version just recieves raw data instead of structs
        //private void ResolveCollision(CTransform tA, CRectCollider cA, CRigidBody rA,
        //    CTransform tB, CRectCollider cB, CRigidBody rB)
        //{
        //    // calculate relative velocity
        //    Vector2 relativeVel = rB.velocity - rA.velocity;

        //    // Calculate relative velocity in terms of the normal direction
        //    //      collision normal can be got by finding in which direction
        //    //      the objects have moved in to overlap
        //    Vector2 colNormal = GetCollisionNormal(tA, cA, rA, tB, cB, rB);

        //    float velAlongNormal = Vector2.Dot(relativeVel, colNormal);

        //    // calculate resitution ( I will be using this hard coded for simplicity)
        //    //                              later add physics info class or
        //    //                              simply add it to rigidbody class
        //    float eps = 50f; // TEMP

        //    // Calculate impulse scalar
        //    float j = -(1 + eps) * velAlongNormal;
        //    j = j / 1 / rA.mass + 1 / rB.mass;

        //    // apply impulse
        //    Vector2 impulse = j * colNormal;

        //    float massSum = rA.mass + rB.mass;
        //    float massRatio = rB.mass / massSum;
        //    rA.velocity -= massRatio * impulse;

        //    massRatio = rA.mass / massSum;
        //    rB.velocity += massRatio * impulse;

        //    float toRightDist = tB.X - tA.X + cA.Width;
        //    float toLeftDist = -1f * (tA.X - tB.X + cB.Width);

        //    float toBotDist = tB.Y - tA.Y + cA.Height;
        //    float toTopDist = -1f * (tA.Y - tB.Y + cB.Height);

        //    float xMin = 0f;
        //    float yMin = 0f;

        //    if (Math.Abs(toRightDist) < Math.Abs(toLeftDist))
        //    {
        //        xMin = toRightDist;
        //    }
        //    else
        //    {
        //        xMin = toLeftDist;
        //    }
        //    if (Math.Abs(toTopDist) < Math.Abs(toBotDist))
        //    {
        //        yMin = toTopDist;
        //    }
        //    else
        //    {
        //        yMin = toBotDist;
        //    }

        //    Debug.WriteLine("xMin: " + xMin);
        //    Debug.WriteLine("yMin: " + yMin);

        //    float penDepth = 0f;
        //    if (Math.Abs(xMin) < Math.Abs(yMin))
        //    {
        //        penDepth = xMin;
        //    }
        //    else
        //    {
        //        penDepth = yMin;
        //    }
        //    Debug.WriteLine("penDepth: " + penDepth);

        //    float massBPerc = rB.mass / massSum;
        //    float massAPerc = rA.mass / massSum;


        //    if (penDepth < 0)
        //    {


        //        tA.position -= penDepth * massBPerc * colNormal;
        //        tB.position += penDepth * massAPerc * colNormal;
        //    }
        //    else
        //    {
        //        tA.position += penDepth * massBPerc * colNormal;
        //        tB.position -= penDepth * massAPerc * colNormal;
        //    }

        //}







        // quadtree functionality 
        // the quadtree will update every frame and contain all of the
        // items that need to be updated
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

     


    /*
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

        
         //Need to make it so that only the dynamic entities solve
         //collisions against the static ones and dynamic one
         //against other dynamic ones (like player against wall).
         //there is no need to solve collisions for static objects against static
         //objects.
         
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
                        if (AABBvsAABB(
                            transform1, collider1 as CRectCollider, 
                            transform2, collider2 as CRectCollider))
                        {
                            Debug.WriteLine("collision occuring");
                        }
                        //ResolveDynamicAABB(
                        //    transform1, collider1 as CRectCollider, rig1,
                        //    transform2, collider2 as CRectCollider, rig2);
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

        private bool AABBvsAABB(
            CTransform transA, CRectCollider colA,
            CTransform transB, CRectCollider colB)
        {
            // ensure no seperating axis
            if (transA.X + colA.Width < transB.X || transA.X > transB.X + colB.Width)
            {
                return false;
            }

            if (transB.Y + colA.Height < transB.Y || transA.Y > transB.Y + colB.Height)
            {
                return false;
            }

            return true;
        }

        private bool CirclevsCircle(CTransform transA, CCircleCollider colA,
            CTransform transB, CCircleCollider colB)
        {
            float r = colA.radius + colB.radius;
            r *= r;
            return r < Math.Pow(transA.X + transB.X, 2) + Math.Pow(transA.Y + transB.Y, 2);
        }



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
    */




























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


