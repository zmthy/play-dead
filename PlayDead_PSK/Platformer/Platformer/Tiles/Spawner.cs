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
using Platformer.Camera;

namespace Platformer.Tiles
{
    class Spawner : IActivatable, ICameraTrackable
    {
        /// <summary>
        /// The level which the spawner is tied too.
        /// </summary>
        private Level level;

        private Animation activated;
        private Animation deactivated;
        private Animation spawn;
        private AnimationPlayer animation;

        private bool isActive;

        public Vector2 Position
        {
            get { return position; }
        }
        private Vector2 position;

        public Spawner(Vector2 position, ContentManager content)
        {
            this.position = position + new Vector2(0, 24);
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
            this.activated = new Animation(content.Load<Texture2D>("Activatable/spawner_on"), 0.15f, true);
            this.deactivated = new Animation(content.Load<Texture2D>("Activatable/spawner_off"), 0.15f, false);
            this.spawn = new Animation(content.Load<Texture2D>("Activatable/spawner_spawn"), 0.12f, delegate
            {
                animation.PlayAnimation(activated);
            });

            SetSpawnState(isActive);
        }

        /// <summary>
        /// When the spawner is swicthed on it will notify the Level to update it's spawn.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetState(Boolean active)
        {
            if (!isActive) //Activators can only turn a spawn point on, not off as we don't know the previous spawn point
            {
                if (active && level != null)
                {
                    level.UpdateSpawn(this);
                }

                SetSpawnState(isActive);
            }
        }

        public bool IsActive()
        {
            return isActive;
        }

        public void ChangeState()
        {
            SetState(!isActive);
        }

        /// <summary>
        /// Overrides the state of the spawner so the level can switch old spawns back off.
        /// </summary>
        /// <param name="isActive"></param>
        public void SetSpawnState(Boolean isActive)
        {
            this.isActive = isActive;
            animation.PlayAnimation(isActive ? activated : deactivated);
        }

        public void Spawn()
        {
            animation.PlayAnimation(spawn);
        }


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            animation.Draw(gameTime, spriteBatch, position, SpriteEffects.None);
        }

        public void Update(GameTime gameTime) {}

        public Vector2 getPosition()
        {
            return Position;
        }
    }
}
