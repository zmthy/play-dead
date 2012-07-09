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
    class Light : IActivatable
    {
        private Texture2D activated;
        private Texture2D deactivated;
        private Vector2 origin;

        private bool isActive;

        public Vector2 Position
        {
            get { return position; }
        }
        private Vector2 position;

        public Light(Vector2 position, ContentManager content)
        {
            this.position = position;
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

        public bool IsActive()
        {
            return isActive;
        }

        public void SetState(bool active)
        {
            this.isActive = active;
        }

        public void ChangeState()
        {
            SetState(!isActive);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (isActive)
                spriteBatch.Draw(activated, position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            else
                spriteBatch.Draw(deactivated, position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        public void Update(GameTime gameTime) { }

    }
}
