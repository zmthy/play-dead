using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Laser
{
    class Emitter : Platformer.Tiles.Activator
    {
        PrimitiveLine laser;

        Mirror next;

        List<Vector2> network;

        public Emitter(Vector2 location, GraphicsDevice gd, ContentManager content)
            : base(location)
        {
            this.initialise(content);
            next = null;
            laser = new PrimitiveLine(gd);
            laser.Colour = Color.Red;
            laser.Depth = 0;
            laser.Position = position;
            on = true;
        }

    private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Activator/mirror_on");
            this.deactivated = content.Load<Texture2D>("Activator/mirror_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }

        public void setTarget(Mirror m)
        {
            next = m;
        }

        public new void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            laser.Render(spriteBatch);
        }

        public override void ChangeState(Player c, Microsoft.Xna.Framework.Input.KeyboardState keyState, InputManager inputManager)
        {
            network = new List<Vector2>();
            network.Add(position);
            Mirror n = next;
            while (n != null)
            {
                network.Add(n.getLocation());
                n.trigger();
                n = n.getNext();
            }
            laser.ClearVectors();
            for (int i = 0; i < network.Count; i++)
            {
                laser.AddVector(network[i]-position);
            }
        }
    }
}
