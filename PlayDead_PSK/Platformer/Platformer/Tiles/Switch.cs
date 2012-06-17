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

        public Switch(ArrayList list, Vector2 location, ContentManager content)
            : base(location)
        {
            this.list = list;
            this.initialise(content);
        }

        public Switch(Vector2 location, ContentManager content)
            : base(location)
        {
            this.initialise(content);
        }

        public Switch(Activatable responder, Vector2 location, ContentManager content)
            : base(location)
        {
            this.add(responder);
            this.initialise(content);
        }

        private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Activator/switch_on");
            this.deactivated = content.Load<Texture2D>("Activator/switch_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }

        // Switch -  the player has to be near and respond to a key press
        public override void ChangeState(Player p, KeyboardState state)
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
