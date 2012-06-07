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

namespace PlayDead
{
    class Tile
    {
        //COLLIDES_WITH_X == the player will collide with that side of the tile if they try to pass through
        //_from_ that direction. IE: COLLIDES_WITH_TOP means the player will land on the top of it if they jump
        //down onto it, but can pass through it if they are heading upwards (ie: platforms, et al)
        //TODO: FIND A BETTER WAY TO DO THIS.
        public static int NO_COLLISION = 0;
        public static int COLLIDES_WITH_TOP = 1;
        public static int COLLIDES_WITH_BOTTOM = 2;
        public static int COLLIDES_WITH_LEFT = 4;
        public static int COLLIDES_WITH_RIGHT = 8;
        public static int SOLID_BLOCK = 15;

        private Texture2D tex;
        private Rectangle[] clip;
        private Color tint = Color.White;
        private int collision = 0;
        private Boolean animated = false;
        private int count = 0;
        private int delay = 4;

        /// <summary>
        /// Creates a new Tile object
        /// </summary>
        /// <param name="texture">The tile texture sheet</param>
        /// <param name="clip">The portion of the sheet that contains this tile</param>
        /// <param name="collisionMask">The collision bitmask</param>
        public Tile(Texture2D texture, Rectangle clip, int collisionMask)
        {
            this.tex = texture;
            this.clip = new Rectangle[]{clip};
            this.collision = collisionMask;
        }

        /// <summary>
        /// Creates a new Tile Object
        /// </summary>
        /// <param name="texture">The tile texture sheet</param>
        /// <param name="clip">The portion of the sheet that contains this tile</param>
        /// <param name="collisionMask">The collision bitmask</param>
        /// <param name="tint">The tint to apply to this tile (White -> none)</param>
        public Tile(Texture2D texture, Rectangle clip, int collisionMask, Color tint)
        {
            this.tex = texture;
            this.clip = new Rectangle[]{clip};
            this.tint = tint;
            this.collision = collisionMask;
        }

        /// <summary>
        /// Constructs an *animated* Tile
        /// </summary>
        /// <param name="texture">The tile texture sheet</param>
        /// <param name="clips">An array of clips that specific this tiles animation</param>
        /// <param name="collisionMask">The collision bitmask</param>
        /// <param name="tint">The tint to apply to this tile (White -> none)</param>
        public Tile(Texture2D texture, Rectangle[] clips, int collisionMask, Color tint, int delay)
        {
            this.tex = texture;
            this.clip = clips;
            this.tint = tint;
            this.collision = collisionMask;
            animated = true;
            this.delay = delay;
        }

        /// <summary>
        /// Draws this tile on the screen
        /// </summary>
        /// <param name="sb">The SpriteBatch to use to render this tile</param>
        /// <param name="x">The x position to draw it at</param>
        /// <param name="y">The y position to draw it at</param>
        public void DrawTile(SpriteBatch sb, Rectangle bounds)
        {
            if (!animated)
            {
                sb.Draw(tex, bounds, clip[0], tint);
            }
            else
            {
                int frame = (count / delay) % clip.Length;
                sb.Draw(tex, bounds, clip[frame], tint);
                count++;
            }
        }

        public int getCollisionMask()
        {
            return collision;
        }

    }
}
