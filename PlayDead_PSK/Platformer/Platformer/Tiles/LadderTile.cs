using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Tiles
{    
    class LadderTile : Tile, IActivatable
    {
        private bool isActive;

        private Texture2D activated;
        private Texture2D deactivated;


        /// <summary>
        /// Constructor defaulting to inactive ladder
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="collision"></param>
        public LadderTile(Sprite sprite, TileCollision collision)
            : base(sprite, collision)
        {
            activated = sprite.Texture;
            deactivated = null;

            isActive = false;
            this.Sprite = sprite;
        }


        /// <summary>
        /// Need this so once the game has init the tile construction is loaded with a null sprite rather than the active sprite
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isActive)
            {
                Sprite.Texture = activated;
                Collision = TileCollision.Ladder;
            }
            else
            {
                Sprite.Texture = deactivated;
                Collision = TileCollision.Passable;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return isActive;
        }

        public void SetState(bool active)
        {
            if (active != isActive)
            {
                ChangeState();
            }

            isActive = active;
        }


        public void ChangeState()
        {
                       
            isActive = !isActive;

            /*
            // show
            if (isActive)
            {
                // Activate ladder collision detection
                Collision = TileCollision.Ladder;

                // Apply ladder sprite
                Sprite = activated;
            }
            // hide
            else
            {
                // Disable ladder collision detection
                Collision = TileCollision.Passable;

                // Remove sprite
                Sprite = deactivated;
            } 
             */
        }
    }
}
