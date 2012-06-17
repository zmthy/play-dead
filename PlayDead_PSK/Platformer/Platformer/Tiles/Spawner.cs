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
using Platformer.Levels;

namespace Platformer.Tiles
{
    class Spawner : Activatable
    {
        /// <summary>
        /// The level which the spawner is tied too.
        /// </summary>
        private Level level;

        public Vector2 Position
        {
            get { return position; }
        }

        public Spawner(Vector2 location, ContentManager content)
            : base(location)
        {
            this.initialise(content);
        }

        /// <summary>
        /// Binds the spawner to a particular level so it can notify the level if it gets turned on.
        /// </summary>
        /// <param name="level"></param>
        public void bindToLevel(Level level)
        {
            this.level = level;
        }

        /// <summary>
        /// Load the texture assets and position the spawner.
        /// </summary>
        /// <param name="content">The content manager for loading assets,</param>
        private void initialise(ContentManager content)
        {
            //load textures
            this.activated = content.Load<Texture2D>("Activatable/spawner_on");
            this.deactivated = content.Load<Texture2D>("Activatable/spawner_off");
            //initialise origin
            origin = new Vector2(activated.Width / 2.0f, activated.Height / 2.0f);
        }

        /// <summary>
        /// When the spawner is swicthed on it will notify the Level to update it's spawn.
        /// </summary>
        /// <param name="isActive"></param>
        public override void SetState(Boolean isActive)
        {
            if (!on) //Activators can only turn a spawn point on, not off as we don't know the previous spawn point
            {
                if (isActive && level != null)
                {
                    level.UpdateSpawn(this);
                }
                this.on = isActive;
            }
        }

        /// <summary>
        /// Overrides the state of the spawner so the level can switch old spawns back off.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetSpawnState(Boolean isActive)
        {
            this.on = isActive;
        }
    }
}
