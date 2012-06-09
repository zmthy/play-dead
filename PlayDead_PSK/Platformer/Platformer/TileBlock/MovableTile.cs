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

        private float waitTimeS;
        private const float MAX_WAIT_TIME_S = 0.1f;

        public MovableTile(Sprite sprite, TileCollision collision, Vector2 velocity,
                           Level level)
            : base(sprite, collision)
        {
            this.velocity = velocity;
            this.level = level;
        }

        public override void update(GameTime gameTime)
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
                //If we're about to run into a wall that isn't a MovableTile move in other direction.

                // Get adjacent tiles infront
                Vector2 currentCell = getGridPosition(Sprite.X, Sprite.Y);
                Vector2 nextLeft = getAdjacentCellAtAngle(currentCell, velocity.X - (float)(Math.PI / 4));
                Vector2 nextMiddle = getAdjacentCellAtAngle(currentCell, velocity.X);
                Vector2 nextRight = getAdjacentCellAtAngle(currentCell, velocity.X + (float)(Math.PI / 4));
                Vector2[] collidingCells = { nextLeft, nextMiddle, nextRight };

                //If we're about to run into a wall that isn't a MovableTile move in other direction.
                bool collided = false;
                for (int i = 0; i < collidingCells.Length; i++)
                {
                    Vector2 tileGridPos = collidingCells[i];
                    TileCollision collision = level.GetCollision((int)tileGridPos.X, (int)tileGridPos.Y);

                    if (collision == TileCollision.Impassable || collision == TileCollision.Platform)
                    {
                        collided = true;
                        waitTimeS = MAX_WAIT_TIME_S;
                        break;
                    }
                }

                if (!collided)
                {
                    // Move in the current direction.
                    Sprite.X += (float)(Math.Cos(velocity.X) * (velocity.Y * elapsedS));
                    Sprite.Y += (float)(Math.Sin(velocity.X) * (velocity.Y * elapsedS));
                }
            }

            //If we're about to run into a MovableTile move in other direction.
            List<Tile> nonAtomicTiles = level.getNonAtomicTiles();
            foreach (Tile tile in nonAtomicTiles)
            {
                if (tile != this && Sprite.intersects(tile.Sprite))
                {
                    reverseMovement();
                }
            }
        }

        private Vector2 getGridPosition(float x, float y)
        {
            Vector2 gridPos = new Vector2((int)Math.Floor(x / Tile.Width),
                                          (int)Math.Floor(y / Tile.Height));
            return gridPos;
        }

        private Vector2 getAdjacentCellAtAngle(Vector2 currentCell, float angleRadians)
        {
            int newCellX = (int)currentCell.X + (int)Math.Round(Math.Cos(angleRadians));
            int newCellY = (int)currentCell.Y + (int)Math.Round(Math.Sin(angleRadians));
            return new Vector2(newCellX, newCellY);
        }

        private void reverseMovement()
        {
            velocity.X -= (float)Math.PI;
        }
    }
}
