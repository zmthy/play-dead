using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Platformer;

using Microsoft.Xna.Framework.Audio;


namespace Platformer.Tiles
{
    class Switch : Activator
    {

        private SoundEffect buttonDown;
        private SoundEffect buttonUp;

        /*public Switch(A list, Vector2 location, ContentManager content)
            : base(location)
        {
            this.list = list;
            this.initialise(content);
        }*/

        public Switch(Vector2 location, ContentManager content)
            : base(location)
        {
            this.initialise(content);
        }

        /*public Switch(IActivatable responder, Vector2 location, ContentManager content)
            : base(location)
        {
            this.add(responder);
            this.initialise(content);
        }*/

        private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Activator/switch_on");
            this.deactivated = content.Load<Texture2D>("Activator/switch_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);

            buttonDown = content.Load<SoundEffect>("Activator/94121__bmaczero__mechanical2");
            buttonUp = content.Load<SoundEffect>("Activator/94127__bmaczero__clank1");
        }

        // Switch -  the player has to be near and respond to a key press
        public override void ChangeState(Player p, KeyboardState keyState, InputManager inputManager)
        {
            RectangleF other = p.BoundingRectangle;
            Boolean touching = other.Intersects(this.BoundingRectangle);
            if (inputManager.IsNewPress(Keys.E) && touching)
            {
                on = !on;

                if (on)
                {
                    buttonDown.Play();
                }
                else
                {
                    buttonUp.Play();
                }

                foreach (IActivatable responder in list)
                {
                    responder.ChangeState();
                }
            }
        }

    }
}
