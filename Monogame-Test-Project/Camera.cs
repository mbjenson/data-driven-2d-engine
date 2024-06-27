using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace viewStuff
{
    
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
                    Matrix.CreateTranslation(new Vector3((int)-Position.X, (int)-Position.Y, 0)) *
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


        public void SmoothZoom(float goalZoom, float speed, float dt)
        {
            if (Math.Abs(this.Zoom) - goalZoom < 0.0001f)
            {
                float zoomToGo = goalZoom - this.Zoom;
                this.Zoom += zoomToGo * speed * dt;
            }
            else
            {
                this.Zoom = goalZoom;
            }
        }

        public void Update(Vector2 followPoint, float dt)
        {
            Vector2 calcPos = Vector2.Lerp(this.Position, followPoint, 0.1f);
            this.Position = new Vector2((float)Math.Round(calcPos.X), (float)Math.Round(calcPos.Y));
        }

        public void Update(Vector2 followPoint, Vector2 entityVel, float dt)
        {
            Vector2 calcPos = Vector2.Lerp(this.Position, followPoint, 0.1f);

            this.Position = new Vector2((float)Math.Round(calcPos.X), (float)Math.Round(calcPos.Y));
        }
    }
}

