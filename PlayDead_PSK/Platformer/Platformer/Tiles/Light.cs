using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Platformer;

namespace Platformer.Tiles
{
    class Light : Activatable
    {

        public Light(Vector2 location, ContentManager content)
            : base(location)
        {
            this.initialise(content);
        }


        private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Activatable/light_on");
            this.deactivated = content.Load<Texture2D>("Activatable/light_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }


    }
}
