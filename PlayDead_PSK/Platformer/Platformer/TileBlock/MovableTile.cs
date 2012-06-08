using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.TileBlock
{
    /// <summary>
    /// A tile that moves in a given direction.
    /// Once the tile collides with a solid object, it reverses its direction.
    /// 
    /// Code inspired from:
    /// http://robotfootgames.com/2011/05/movable-platforms/
    /// </summary>
    class MovableTile :  Tile
    {
        /// <summary>
        /// The movement velocity of the tile.
        /// The first component (.X) is the angle in radians.
        /// The second component (.Y) is the speed in pixels per second.
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
        }
        private Vector2 velocity;

        private Level level;
        private Vector2[] collidingCells = { };

        private float waitTimeS;
        private const float MAX_WAIT_TIME_S = 0.1f;
        private const float MOVE_SPEED_S = 120.0f;

        public MovableTile(Texture2D texture, TileCollision collision, Vector2 velocity,
                           Level level)
            : base(texture, collision)
        {
            this.velocity = velocity;
            this.level = level;
            Position = Vector2.Zero;
        }

        public void update(GameTime gameTime)
        {
            // Get the elapse time since the last frame
            float elapsedS = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (waitTimeS > 0)
            {
                // Wait for some amount of time.
                waitTimeS = Math.Max(0.0f, waitTimeS - elapsedS);
                if (waitTimeS <= 0.0f)
                {
                    // Then turn around.
                    reverseMovement();
                }
            }
            else
            {
                // Get colliding tiles
                for (int i = 0; i < collidingCells.Length; i++)
                    level.setTile((int)collidingCells[i].X, (int)collidingCells[i].Y, new Tile(null, TileCollision.Passable));

                Vector2 topLeftTile = getGridPosition(Position.X, Position.Y);
                Vector2 topRightTile = getGridPosition(Position.X + Tile.Width, Position.Y);
                Vector2 bottomLeftTile = getGridPosition(Position.X, Position.Y + Tile.Height);
                Vector2 bottomRightTile = getGridPosition(Position.X + Tile.Width, Position.Y + Tile.Height);
                collidingCells = new Vector2[]{topLeftTile, topRightTile, bottomLeftTile, bottomRightTile};

                for (int i = 0; i < collidingCells.Length; i++)
                    level.setTile((int)collidingCells[i].X, (int)collidingCells[i].Y, this);

                //If we're about to run into a wall that isn't a MovableTile move in other direction.
                bool collided = false;
                for (int i = 0; i < collidingCells.Length; i++)
                {
                    Vector2 tileGridPos = collidingCells[i];
                    TileCollision collision = level.GetCollision((int)tileGridPos.X, (int)tileGridPos.Y);

                    if (collision == TileCollision.Impassable || collision == TileCollision.Platform)
                    {
                        waitTimeS = MAX_WAIT_TIME_S;
                        collided = true;
                        break;
                    }
                }

                if (!collided)
                    Position += velocity;
            }
        }

        private Vector2 getGridPosition(float x, float y)
        {
            Vector2 gridPos = new Vector2((int)Math.Floor(x / Tile.Width),
                                          (int)Math.Floor(y / Tile.Height));
            return gridPos;
        }

        private void reverseMovement()
        {
            Console.WriteLine("Reversing direction from " + velocity.X + " to " + (velocity.X - (float)Math.PI) + '.');
            velocity.X -= (float)Math.PI;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
