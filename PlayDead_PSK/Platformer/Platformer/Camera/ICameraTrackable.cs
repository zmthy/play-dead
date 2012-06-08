using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Platformer.Camera
{
    public interface ICameraTrackable
    {
        Vector2 getPosition();
    }
}
