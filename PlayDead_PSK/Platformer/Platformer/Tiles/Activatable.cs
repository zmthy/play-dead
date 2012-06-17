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

namespace Platformer.Tiles
{
    abstract class Activatable
    {

        protected Vector2 position;

        protected Boolean on;

        protected Texture2D activated;
        protected Texture2D deactivated;
        protected Vector2 origin;

        public Activatable(Vector2 location)
        {
            this.position = location;
        }

        //can be overridden to make more interesting things happen, for example a door should go from stopping a player to letting them through
        public virtual void SetState(Boolean on)
        {
            this.on = on;
        }

        public virtual void changeState()
        {
            if (on) 
                SetState(false);
            else
                SetState(true);
        }


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (on)
                spriteBatch.Draw(activated, position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            else
                spriteBatch.Draw(deactivated, position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

    }
}
