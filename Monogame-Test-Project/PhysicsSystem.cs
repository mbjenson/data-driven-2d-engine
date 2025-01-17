﻿using bitmask;
using ECS;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using System.Security.Principal;
using System.Reflection.Metadata;



namespace ECS.Systems
{

    /*
     * Physics System
     * 
     * This system combines other systems together to perform a physics simulation
     * It currently uses:
     *  Collision system
     *  movement system
     *  
     */
    public class PhysicsSystem : UpdateSystem
    {
        private EntityManager eMan;
        private MovementSystem movementSubsystem;
        private CollisionSystem collisionSubsystem;

        public PhysicsSystem(EntityManager eMan, TilemapManager tileMan)
        {
            Debug.Assert(eMan != null);
            this.eMan = eMan;

            movementSubsystem = new MovementSystem(eMan);
            collisionSubsystem = new CollisionSystem(eMan, tileMan);
        }

        public override void Update(GameTime gameTime)
        {
            movementSubsystem.Update(gameTime);
            collisionSubsystem.Update(gameTime);
        }
    }




    /*
     * Collision system
     * 
     * Handles simulating physical collisions between objects in the
     * entity component system.
     * 
     * Notes: It currently uses some class based objects to pack data 
     * which was originally structs but has changed over time. I may
     * change this in the future to make it more clear or to 
     * increase efficiency. But for now, the class based packing
     * worked and makes sense to me. (6/16/2024)
     * 
     */
    public class CollisionSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;
        private Bitmask dynamicRectSig;

        private TilemapManager tileMan;

        private class Rect
        {
            public CTransform transform;
            public CRectCollider collider;
            public Rect(CTransform transform, CRectCollider collider)
            {
                this.transform = transform;
                this.collider = collider;
            }
        }


        private class DynamicRect : Rect
        {
            public CRigidBody rigidbody;
            public DynamicRect(CTransform transform, CRectCollider collider,
                CRigidBody rigidbody) : base(transform, collider)
            {
                this.rigidbody = rigidbody;
            }
        }


        public CollisionSystem(EntityManager eMan, TilemapManager tileMan)
        {
            Debug.Assert(eMan != null);
            Debug.Assert(tileMan != null);
            this.eMan = eMan;
            this.tileMan = tileMan;

            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CCollider] = true;
            signature[ComponentType.CTransform] = true;

            dynamicRectSig = new Bitmask((int)ComponentType.Count);
            dynamicRectSig[ComponentType.CCollider] = true;
            dynamicRectSig[ComponentType.CTransform] = true;
            dynamicRectSig[ComponentType.CRigidBody] = true;
        }



        public override void Update(GameTime gameTime)
        {
            //float dt = (float)gameTime.ElapsedGameTime.TotalSeconds
            SolveCollisions();
            SolveTilemapCollisions();
        }


        private void SolveTilemapCollisions()
        {
            List<Entity> entities = eMan.GetEntities(dynamicRectSig).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                CTransform tA = (CTransform)eMan.GetComponent<CTransform>(entities[i].id);
                CRectCollider cA = (CRectCollider)eMan.GetComponent<CRectCollider>(entities[i].id);
                CRigidBody rA = (CRigidBody)eMan.TryGetComponent<CRigidBody>(entities[i].id);

                Vector2 thisPos = tA.position;
                Rectangle tileRect;
                Rect tilePhysicsRect;

                if (tileMan.IsSolidAt(thisPos))
                {
                    tileRect = tileMan.GetTileRect(thisPos);
                    tilePhysicsRect = new Rect(
                            new CTransform(new Vector2(tileRect.X, tileRect.Y)),
                            new CRectCollider(tileRect.Width, tileRect.Height));
                    ResolveCollision(tilePhysicsRect, new DynamicRect(tA, cA, rA));
                }
                else if (tileMan.IsSolidAt(new Vector2(thisPos.X + cA.Width, thisPos.Y)))
                {
                    tileRect = tileMan.GetTileRect(new Vector2(thisPos.X + cA.Width, thisPos.Y));
                    tilePhysicsRect = new Rect(
                            new CTransform(new Vector2(tileRect.X, tileRect.Y)),
                            new CRectCollider(tileRect.Width, tileRect.Height));
                    ResolveCollision(tilePhysicsRect, new DynamicRect(tA, cA, rA));
                }
                else if (tileMan.IsSolidAt(new Vector2(thisPos.X + cA.Width, thisPos.Y + cA.Height)))
                {
                    tileRect = tileMan.GetTileRect(new Vector2(thisPos.X + cA.Width, thisPos.Y + cA.Height));
                    tilePhysicsRect = new Rect(
                            new CTransform(new Vector2(tileRect.X, tileRect.Y)),
                            new CRectCollider(tileRect.Width, tileRect.Height));
                    ResolveCollision(tilePhysicsRect, new DynamicRect(tA, cA, rA));
                }
                else if (tileMan.IsSolidAt(new Vector2(thisPos.X, thisPos.Y + cA.Height)))
                {
                    tileRect = tileMan.GetTileRect(new Vector2(thisPos.X, thisPos.Y + cA.Height));
                    tilePhysicsRect = new Rect(
                            new CTransform(new Vector2(tileRect.X, tileRect.Y)),
                            new CRectCollider(tileRect.Width, tileRect.Height));
                    ResolveCollision(tilePhysicsRect, new DynamicRect(tA, cA, rA));
                }

            }


        }


        // in the future when there are different types of colliders, the system of trying to pack the
        // information into structs/classes will need to be altered to accomodate the new types.
        private void SolveCollisions()
        {
            Dictionary<int, Rect> physicsRects = new Dictionary<int, Rect>();
            Dictionary<int, int> intersections = new Dictionary<int, int>();
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            for (int i = 0; i < entities.Count; i++)
            {
                CTransform tA = (CTransform)eMan.GetComponent<CTransform>(entities[i].id);
                CCollider cA = (CCollider)eMan.GetComponent<CRectCollider>(entities[i].id);
                CRigidBody rA = (CRigidBody)eMan.TryGetComponent<CRigidBody>(entities[i].id);

                if (!physicsRects.ContainsKey(entities[i].id))
                {
                    if (rA != null)
                    {
                        physicsRects.Add(entities[i].id, new DynamicRect(tA, cA as CRectCollider, rA));
                    }
                    else
                    {
                        physicsRects.Add(entities[i].id, new Rect(tA, cA as CRectCollider));
                    }
                }

                for (int j = 0; j < entities.Count; j++)
                {
                    if (j == i) { continue; }
                    CTransform tB =
                        (CTransform)eMan.GetComponent<CTransform>(entities[j].id);
                    CCollider cB =
                        (CCollider)eMan.GetComponent<CRectCollider>(entities[j].id);

                    // rect vs rect
                    if (cA.GetType() == typeof(CRectCollider) &&
                        cB.GetType() == typeof(CRectCollider))
                    {
                        // if the two rectangles are overlapping
                        if (AABBvsAABB(tA, cA as CRectCollider, tB, cB as CRectCollider))
                        {
                            int outVal;
                            if (intersections.TryGetValue(entities[i].id, out outVal) &&
                                outVal == entities[j].id)
                            {
                                continue;
                            }
                            else if (intersections.TryGetValue(entities[j].id, out outVal) &&
                                outVal == entities[i].id)
                            {
                                continue;
                            }
                            else
                            {
                                if (!intersections.ContainsKey(entities[i].id))
                                {
                                    intersections.Add(entities[i].id, entities[j].id);
                                }
                                if (!physicsRects.ContainsKey(entities[j].id))
                                {
                                    CRigidBody rB =
                                        (CRigidBody)eMan.TryGetComponent<CRigidBody>(entities[j].id);

                                    if (rB != null)
                                    {
                                        physicsRects.Add(
                                            entities[j].id,
                                            new DynamicRect(tB, cB as CRectCollider, rB));
                                    }
                                    else
                                    {
                                        physicsRects.Add(
                                            entities[j].id,
                                            new Rect(tB, cB as CRectCollider));
                                    }
                                }
                            }
                        }
                    }

                    // circle vs circle
                    // else if (cA.GetType() == typeof(CCircleCollider) &&
                    //          cB.GetType() == typeof(CCircleCollider)) {}

                    // rect vs circle
                    // else if (cA.GetType() == typeof(CCircleCollider) &&
                    //          cB.GetType() == typeof(CRectCollider)) {}

                }
            }
            foreach (var i in intersections.Keys)
            {
                int j = intersections[i];

                Rect rectA = physicsRects[i];
                Rect rectB = physicsRects[j];
                if (rectA.GetType() == typeof(DynamicRect) && rectB.GetType() == typeof(DynamicRect))
                {
                    ResolveCollision(physicsRects[i] as DynamicRect, physicsRects[j] as DynamicRect);
                }
                else if (rectA.GetType() == typeof(DynamicRect))
                {
                    // just rectA
                    ResolveCollision(rectB as Rect, rectA as DynamicRect);
                }
                else if (rectB.GetType() == typeof(DynamicRect))
                {
                    // just rectB
                    ResolveCollision(rectA as Rect, rectB as DynamicRect);
                }
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

        private Vector2 GetCollisionNormal(Rect A, Rect B)
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


        // this version uses a physics rect struct to get all of the data about
        // each object instead of accessing the raw components
        private void ResolveCollision(DynamicRect A, DynamicRect B)
        {
            // calculate relative velocity
            Vector2 relativeVel = B.rigidbody.velocity - A.rigidbody.velocity;

            // Calculate relative velocity in terms of the normal direction
            //      collision normal can be got by finding in which direction
            //      the objects have moved in to overlap

            Vector2 colNormal = GetCollisionNormal(A, B);

            float velAlongNormal = Vector2.Dot(relativeVel, colNormal);

            // calculate resitution ( I will be using this hard coded for simplicity)
            //                              later add physics info class or
            //                              simply add it to rigidbody class
            // float eps = 0.5f; // TEMP
            float eps = 4f;

            // Calculate impulse scalar
            float j = -(1 + eps) * velAlongNormal;
            j = j / 1 / A.rigidbody.mass + 1 / B.rigidbody.mass;

            // apply impulse
            Vector2 impulse = j * colNormal;

            float massSum = A.rigidbody.mass + B.rigidbody.mass;
            float massRatio = B.rigidbody.mass / massSum;
            A.rigidbody.velocity -= massRatio * impulse;

            massRatio = A.rigidbody.mass / massSum;
            B.rigidbody.velocity += massRatio * impulse;



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



            float massBPerc = B.rigidbody.mass / massSum;
            float massAPerc = A.rigidbody.mass / massSum;

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
        }

        private void ResolveCollision(Rect A, DynamicRect B)
        {
            Vector2 colNormal = GetCollisionNormal(A, B as Rect);

            if (colNormal.X == 0)
            {
                B.rigidbody.velocity.Y *= -0.7f;
            }
            else if (colNormal.Y == 0)
            {
                B.rigidbody.velocity.X *= -0.7f;
            }

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

            if (penDepth < 0)
            {
                B.transform.position += penDepth * colNormal;
            }
            else
            {
                B.transform.position -= penDepth * colNormal;
            }
        }


    }


    public class MovementSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        public MovementSystem(EntityManager eMan)
        {
            Debug.Assert(eMan != null);
            this.eMan = eMan;

            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CCollider] = true;
            signature[ComponentType.CTransform] = true;
            signature[ComponentType.CRigidBody] = true;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateMovement(dt);
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
                // (TEMP) surface coefficient of dynamic friction
                //                 direction and size * intensity of friction * related to mass

                //float kN = 0.025f;
                float kN = 2.5f * dt; // this helps scale the fps and the friction better but it is not perfect

                //rA.acceleration += -rA.velocity * kN * (1f - (1f / rA.mass));
                rA.acceleration += -rA.velocity * kN * (1f - (1f / rA.mass));

                // calculate velocity
                rA.velocity += rA.acceleration;

                // reset acceleration
                rA.acceleration = Vector2.Zero;

                // update position from velocity
                tA.position += rA.velocity * dt;
            }
        }
    }

    
}
    