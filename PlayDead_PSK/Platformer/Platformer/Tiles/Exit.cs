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
    class Exit : Activator
    {
        public int LevelIndex
        {
            set { levelIndex = value; }
            get { return levelIndex; }
        }
        int levelIndex; 

        public Exit(Vector2 location, ContentManager content)
            : base(location - new Vector2(0, 12))
        {
            this.initialise(content);
        }

        private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Tiles/exit");
            this.deactivated = content.Load<Texture2D>("Tiles/exit");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }

        // Switch -  the player has to be near and respond to a key press
        public override void ChangeState(Player p, KeyboardState keyState, InputManager inputManager)
        {
            RectangleF other = p.BoundingRectangle;
            Boolean touching = other.Intersects(this.BoundingRectangle);
            if (inputManager.IsNewPress(Keys.E) && touching)
                p.GotoLevel(levelIndex);
        }

    }
}
