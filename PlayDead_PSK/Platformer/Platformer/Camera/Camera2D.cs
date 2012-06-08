using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Camera
{
    /// <summary>
    /// A camera for a 2D scene.
    /// 
    /// Initial source from:
    /// http://www.david-amador.com/2009/10/xna-camera-2d-with-zoom-and-rotation/
    /// </summary>
    public class Camera2D
    {
        private float zoom; // Camera zoom scale (default = 1)
        private Matrix transform; // Perspective transform
        private Vector2 position; // Camera position
        private float rotation; // View rotation in radians

        public Camera2D()
        {
            zoom = 1.0f;
            rotation = 0.0f;
            position = Vector2.Zero;
        }

        #region Camera manipulation

        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                // Negative zoom will flip image
                if (zoom < 0.1f)
                    zoom = 0.1f;
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float X
        {
            get { return position.X; }
            set { position.X = value; }
        }

        public float Y
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public void move(Vector2 amount)
        {
            position += amount;
        }

        #endregion

        public Matrix getTransform(GraphicsDevice graphicsDevice)
        {
            // Thanks to o KB o for this solution
            transform = Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0)) *
                                                    Matrix.CreateRotationZ(Rotation) *
                                                    Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                                    Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f,graphicsDevice.Viewport.Height * 0.5f, 0));

            return transform;
        }
    }
}
