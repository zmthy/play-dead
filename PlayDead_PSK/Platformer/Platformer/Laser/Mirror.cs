using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

using Platformer.Tiles;


namespace Platformer.Laser
{
    class Mirror : Platformer.Tiles.Activator, IActivatable
    {

        private List<Mirror> nextTarget = new List<Mirror>();
        private int next = 0;

        public void addTarget(Mirror m)
        {
            nextTarget.Add(m);
        }

        public Mirror(Vector2 location, ContentManager content)
            : base(location)
        {
            position = location;
            this.initialise(content);
        }

        private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Activator/mirror_on");
            this.deactivated = content.Load<Texture2D>("Activator/mirror_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }

        public bool IsActive()
        {
            return true;
        }

        public void SetState(bool active)
        {
            //Ignore this
        }

        public void ChangeState()
        {
            if (nextTarget.Count != 0)
            {
                nextTarget[next].untrigger();
                next = (next + 1) % nextTarget.Count;
                if(on)
                    nextTarget[next].trigger();
            }
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(activated, position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        public void trigger()
        {
            on = true;
            if (nextTarget.Count != 0)
            {
                nextTarget[next].trigger();
            }
        }

        public void untrigger()
        {
            on = false;
            if (nextTarget.Count != 0)
            {
                nextTarget[next].untrigger();
            }
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
           
        }

        public Mirror getNext()
        {
            if (nextTarget.Count == 0)
                return null;
            else
                return nextTarget[next];
        }

        public Vector2 getLocation()
        {
            return position;
        }

        public override void ChangeState(Player c, Microsoft.Xna.Framework.Input.KeyboardState keyState, InputManager inputManager)
        {
            if (on)
            {
              foreach (IActivatable a in list)
              { 
                  if (!a.IsActive() && !(a is Mirror))
                      a.ChangeState();
              }
            }
            else
            {
                foreach (IActivatable a in list)
                {
                    if (a.IsActive() &&  !(a is Mirror))
                        a.ChangeState();
                }
            }
        }
    }
}
