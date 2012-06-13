using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Platformer;

namespace Platformer.Tiles
{
    class Switch : Activator
    {

        public Switch(ArrayList list, Vector2 location, Level level)
            : base(location)
        {
            this.list = list;
            this.initialise(level);
        }

        public Switch(Vector2 location, Level level)
            : base(location)
        {
            this.initialise(level);
        }

        public Switch(Activatable responder, Vector2 location, Level level)
            : base(location)
        {
            this.add(responder);
            this.initialise(level);
        }

        private void initialise(Level level)
        {
            //load textures
            this.activated = level.Content.Load<Texture2D>("Activator/switch_on");
            this.deactivated = level.Content.Load<Texture2D>("Activator/switch_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }

        // Switch -  the player has to be near and respond to a key press
        override public void ChangeState(Player p, KeyboardState state)
        {

            RectangleF other = p.BoundingRectangle;

            Boolean touching = other.Intersects(this.BoundingRectangle);


            if (!on) // check if it should be turned on
            {
                if (state.IsKeyDown(Keys.K) && touching)
                {
                    on = true;
                    foreach (Activatable responder in list)
                    {
                        responder.changeState();
                    }
                }
            }
            else if (on) //check if it can be turned off
            {
                if (state.IsKeyDown(Keys.K) && touching)
                {
                    on = false;
                    foreach (Activatable responder in list)
                    {
                        responder.changeState();
                    }
                }
            }
        }

    }
}
