
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.ExceptionServices;



// if (RectA.Left < RectB.Right && RectA.Right > RectB.Left &&
//      RectA.Top > RectB.Bottom && RectA.Bottom < RectB.Top)

namespace Collisions
{
    class CollisionSolver
    {
        
        public CollisionSolver() { }

        public void solveCollision(CircleCollider dynamicCircle, CircleCollider staticCircle)
        {
            float distance = Vector2.Distance(staticCircle.position, dynamicCircle.position);
            float moveDist = (dynamicCircle.radius + staticCircle.radius) - distance;

            float xDif = dynamicCircle.position.X - staticCircle.position.X;
            float yDif = dynamicCircle.position.Y - staticCircle.position.Y;

            float theta = (float)Math.Atan2(yDif, xDif);
            
            float xMove = moveDist * (float)Math.Cos(theta);
            float yMove = moveDist * (float)Math.Sin(theta);

            dynamicCircle.move(xMove, yMove);
        }

        // solve collision against a static object
        public void solveCollision(RectCollider dynamicRect, RectCollider staticRect)
        {
            // keep track of which edges are overlapping
            bool[] bEdges = { false, false, false, false };

            float toRightDist = 0f;
            float toLeftDist = 0f;
            float toTopDist = 0f;
            float toBottomDist = 0f;

            if (dynamicRect.position.X >= staticRect.position.X && 
                dynamicRect.position.X <= staticRect.getRight())
            {
                bEdges[0] = true;
                toRightDist = staticRect.getRight() - dynamicRect.position.X;
            }
            if (dynamicRect.getRight() <= staticRect.getRight() &&
                dynamicRect.getRight() >= staticRect.position.X)
            {
                bEdges[1] = true;
                toLeftDist = staticRect.position.X - dynamicRect.getRight();
            }
            if (dynamicRect.position.Y >= staticRect.position.Y && 
                dynamicRect.position.Y <= staticRect.getBottom())
            {
                bEdges[2] = true;
                toBottomDist = staticRect.getBottom() - dynamicRect.position.Y;
            }
            if (dynamicRect.getBottom() <= staticRect.getBottom() && 
                dynamicRect.getBottom() >= staticRect.position.Y)
            {
                bEdges[3] = true;
                toTopDist = staticRect.position.Y - dynamicRect.getBottom();
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
                        dynamicRect.move(toRightDist, 0);
                    }
                    else
                    {
                        dynamicRect.move(0, toBottomDist);
                    }
                }
                else if (bEdges[0] && bEdges[3])
                {
                    if (absToRightDist <= absToTopDist)
                    {
                        dynamicRect.move(toRightDist, 0);
                    }
                    else
                    {
                        dynamicRect.move(0, toTopDist);
                    }
                }
                else if (bEdges[1] && bEdges[2])
                {
                    if (absToLeftDist <= absToBottomDist)
                    {
                        dynamicRect.move(toLeftDist, 0);
                    }
                    else
                    {
                        dynamicRect.move(0, toBottomDist);
                    }
                }
                else if (bEdges[1] && bEdges[3])
                {
                    if (absToLeftDist <= absToTopDist)
                    {
                        dynamicRect.move(toLeftDist, 0);
                    }
                    else
                    {
                        dynamicRect.move(0, toTopDist);
                    }
                }
            }
            else if (totalEdges == 1)
            {
                
                if (bEdges[0])
                {
                    dynamicRect.move(toRightDist, 0);
                }
                if (bEdges[1])
                {
                    dynamicRect.move(toLeftDist, 0);
                }
                if (bEdges[2])
                {
                    dynamicRect.move(0, toBottomDist);
                }
                if (bEdges[3])
                {
                    dynamicRect.move(0, toTopDist);
                }
            }
            else
            {
                return;
            }
        }

        // solve collision against a static circle
        public void solveCollision(RectCollider dynamicRect, CircleCollider staticCircle)
        {
            Vector2 testPoint = staticCircle.position;

            if (staticCircle.position.X < dynamicRect.position.X)
            {
                testPoint.X = dynamicRect.position.X;
            }
            else if (staticCircle.position.X > dynamicRect.position.X + dynamicRect.size.X)
            {
                testPoint.X = dynamicRect.position.X + dynamicRect.size.X;
            }

            if (staticCircle.position.Y < dynamicRect.position.Y)
            {
                testPoint.Y = dynamicRect.position.Y;
            }
            else if (staticCircle.position.Y > dynamicRect.position.Y + dynamicRect.size.Y)
            {
                testPoint.Y = dynamicRect.position.Y + dynamicRect.size.Y;
            }

            float distance = Vector2.Distance(staticCircle.position, testPoint);

            if (distance < staticCircle.radius)
            {
                float moveDistance = staticCircle.radius - distance;

                float xDif = testPoint.X - staticCircle.position.X;
                float yDif = testPoint.Y - staticCircle.position.Y;
                
                float theta = (float)Math.Atan2(yDif, xDif);

                float xMove = moveDistance * (float)Math.Cos(theta);
                float yMove = moveDistance * (float)Math.Sin(theta);

                dynamicRect.move(xMove, yMove);
            }
        }

        public bool checkCollision(RectCollider rectA, RectCollider rectB)
        {
            return !(
                rectA.position.X >= rectB.position.X + rectB.size.X || 
                rectA.position.X + rectA.size.X <= rectB.position.X ||
                rectA.position.Y >= rectB.position.Y + rectB.size.Y || 
                rectA.position.Y + rectA.size.Y <= rectB.position.Y);
        }

        public bool checkCollision(CircleCollider circleA, CircleCollider circleB)
        {
            return (
                Vector2.Distance(
                    circleA.position, circleB.position) < 
                    circleA.radius + circleB.radius);
        }

        public bool checkCollision(RectCollider rect, CircleCollider circle)
        {
            Vector2 testPoint = circle.position;

            if (circle.position.X < rect.position.X)
            {
                testPoint.X = rect.position.X;
            }
            else if (circle.position.X > rect.position.X + rect.size.X)
            {
                testPoint.X = rect.position.X + rect.size.X;
            }

            if (circle.position.Y < rect.position.Y)
            {
                testPoint.Y = rect.position.Y;
            }
            else if (circle.position.Y > rect.position.Y + rect.size.Y)
            {
                testPoint.Y = rect.position.Y + rect.size.Y;
            }

            float distance = Vector2.Distance(circle.position, testPoint);
            if (distance < circle.radius)
            {
                return true;
            }
            return false;
        }
    }


    public class RectCollider
    {
        public Vector2 position;
        public Vector2 size;
        public RectCollider(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }

        public RectCollider(float x, float y, float width, float height)
        {
            this.position = new Vector2(x, y);
            this.size = new Vector2(width, height);
        }

        public void setPosition(Vector2 position)
        {
            this.position = position;
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public float getRight()
        {
            return position.X + size.X;
        }

        public float getBottom()
        {
            return position.Y + size.Y;
        }

        public void move(Vector2 vec)
        {
            this.position += vec;
        }

        public void move(float x, float y)
        {
            this.position += new Vector2(x, y);
        }

        public bool contains(Vector2 point)
        {
            return (point.X > this.position.X & point.X < this.position.X + this.size.X &
                point.Y > this.position.Y & point.Y < this.position.Y + this.size.Y);
        }
    }
    
    public class CircleCollider
    {
        public Vector2 position;
        public float radius;

        public CircleCollider()
        {
            this.position = Vector2.Zero;
            this.radius = 1f;
        }

        public CircleCollider(Vector2 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public CircleCollider(float x, float y, float radius)
        {
            this.position = new Vector2(x, y);
            this.radius = radius;
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public void setPosition(Vector2 position) 
        {
            this.position = position;
        }

        public float getRadius()
        {
            return radius;
        }

        public void setRadius(float radius)
        {
            this.radius = radius;
        }

        public void move(Vector2 vec)
        {
            this.position += vec;
        }

        public void move(float x, float y)
        {
            this.position += new Vector2(x, y);
        }

        public bool contains(Vector2 point)
        {
            return (Vector2.Distance(this.position, point) <= this.radius);
        }
    }
}
