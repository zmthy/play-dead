using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Platformer.Camera
{
    public class TrackingDirector : CameraDirector
    {
        private ICameraTrackable target;

        public TrackingDirector(Camera2D camera, ICameraTrackable target)
            : base(camera)
        {
            this.target = target;
        }

        public override void update(GameTime gameTime)
        {
            if (target != null)
            {
                Camera.Position = target.getPosition();
            }
        }
    }
}
