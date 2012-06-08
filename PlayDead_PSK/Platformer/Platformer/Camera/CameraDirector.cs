using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Platformer.Camera
{
    public abstract class CameraDirector
    {
        public Camera2D Camera { get; set; }

        public CameraDirector(Camera2D camera)
        {
            Camera = camera;
        }

        public abstract void update(GameTime gameTime);
    }
}
