using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    class Sprite
    {
        // We represent position and size as floats rather than (int) rectangles to preserve presicion.
        private Vector2 position;
        private Vector2 size;
        private Rectangle? sourceBounds;

        public Texture2D Texture { get; set; }

        public Sprite() { }

        public Sprite(Texture2D texture, Rectangle destBounds)
        {
            Texture = texture;
            position = new Vector2(destBounds.X, destBounds.Y);
            size = new Vector2(destBounds.Width, destBounds.Height);
        }

        public Sprite(Texture2D texture, float width, float height)
        {
            Texture = texture;
            size.X = width;
            size.Y = height;

            position = Vector2.Zero;
        }

        public Sprite(Texture2D texture, Rectangle destBounds, Rectangle? sourceBounds)
            : this(texture, destBounds)
        {
            this.sourceBounds = sourceBounds;
        }

        public void draw(SpriteBatch spriteBatch, int layerDepth)
        {
            if(Texture != null)
                spriteBatch.Draw(Texture, Bounds, sourceBounds, Color.White, 0, Vector2.Zero, SpriteEffects.None, layerDepth);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
                spriteBatch.Draw(Texture, Bounds, sourceBounds, Color.White);
        }

        #region Bounds manipulation

        public float X
        {
            get { return position.X; }
            set { position.X = value; }
        }

        public float Y
        {
            get { return position.Y; }
            set { position.Y = value; }
        }

        public float Width
        {
            get { return size.X; }
            set { size.X = value; }
        }

        public float Height
        {
            get { return size.Y; }
            set { size.Y = value; }
        }

        public Vector2 Center
        {
            get { return new Vector2(position.X + (size.X / 2), position.Y + (size.Y / 2)); }
            set
            {
                position.X = value.X - (size.X / 2);
                position.Y = value.Y - (size.Y / 2);
            }
        }

        public Rectangle Bounds
        {
            get { return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y); }
            set
            {
                position.X = value.X;
                position.Y = value.Y;
                size.X = value.Width;
                size.Y = value.Height;
            }
        }

        // TODO Other nice methods as necessary
        #endregion

        #region Common Sprite operations

        public bool intersects(Sprite other)
        {
            bool doesIntersect = false;

            if (other != null)
                doesIntersect = this.Bounds.Intersects(other.Bounds);

            return doesIntersect;
        }

        // TODO Other nice methods as necessary
        #endregion
    }
}
