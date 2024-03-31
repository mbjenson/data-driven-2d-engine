using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace viewStuff
{
    /*
     * Idea: camera contains a matrix which holds translate, scale, and rotation within it. 
     *       Camera also contains several vectors which can be interacted with to change the
     *       values which will be used to calculate the matrix.
     *       -> Update() will calculate the view matrix for the camera when called
     *       -> SetPosition(Vector2 position) changes the Vector2 position value for camera
     *       -> CalculateView() will contain the code to assemble the view matrix from the given pieces of information
    */
    /*
    public class Camera
    {
        public Matrix transform;
        public Vector2 position;

        public Camera()
        {
            transform = Matrix.Identity;
            position = Vector2.Zero;
        }
        public void Update()
        {
            transform = Matrix.CreateTranslation(new Vector3(position.X, position.Y, 0));
        }

        public void setPosition(Vector2 position)
        {
            this.position = position;
        }

        public Vector2 getPosition()
        {
            return this.position;
        }

    }
    */

    public class Camera2D
    {
        public float Zoom { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        private Rectangle Bounds { get; set; }
        public float lag = 10f;

        public Matrix TransformMatrix
        {
            get
            {
                return
                    Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
            }
        }

        public Rectangle VisibleArea
        {
            get
            {
                var inverseViewMatrix = Matrix.Invert(TransformMatrix);

                var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
                var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
                var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
                var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), inverseViewMatrix);

                var min = new Vector2(
                    MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                    MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));

                var max = new Vector2(
                    MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                    MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));

                return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
            }
        }

        public Camera2D(Viewport viewport)
        {
            Bounds = viewport.Bounds;
            Position = Vector2.Zero;
            Rotation = 0f;
            Zoom = 1f;
        }

        public Vector2 screenToWorld(Vector2 screenCoord)
        {
            return Vector2.Transform(screenCoord, Matrix.Invert(TransformMatrix));
        }

        public Vector2 worldToScreen(Vector2 worldCoord)
        {
            return Vector2.Transform(worldCoord, TransformMatrix);
        }

        public void Update(Vector2 followPoint, float dt)
        {
            Vector2 lookAtDist = followPoint - this.Position;

            this.Position += lookAtDist * this.lag * dt;
        }


    }
}

