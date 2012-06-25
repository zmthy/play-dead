using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Platformer.Tiles;
using Platformer.Levels;

namespace Platformer.Tiles
{
    /// <summary>
    /// A water drain is a sink that pulls water from an area.
    /// Any change of state invoked by the IActivatable interface will
    /// always lower the water level.
    /// </summary>
    class WaterDrain : Tile, IActivatable
    {
        /// <summary>
        /// Whether or not the water level should be decremented by
        /// one (tile) on the next update.
        /// </summary>
        private bool drain;

        /// <summary>
        /// The level the water drain belongs to.
        /// </summary>
        private Level level;

        /// <summary>
        /// Creates a new water drain.
        /// </summary>
        /// <param name="sprite">The look and position of the water drain.</param>
        public WaterDrain(Sprite sprite)
            : base(sprite, TileCollision.Passable) { }

        /// <summary>
        /// Decrease the water level by one (tile).
        /// </summary>
        public void decreaseWaterLevel()
        {
            drain = true;
        }

        /// <summary>
        /// Assigns the level the water drain belongs to.
        /// </summary>
        /// <param name="level">Housing level.</param>
        public void bindToLevel(Level level)
        {
            this.level = level;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Check if the water level should be raised.
            if (drain)
            {
                // Find the surface of the water.
                Vector2 gridPos = level.getGridPosition(Sprite.X, Sprite.Y);
                int currentX = (int)gridPos.X;
                int currentY = (int)gridPos.Y;
                int surfaceY = currentY;
                do
                {
                    surfaceY--;
                } while (level.GetCollision(currentX, surfaceY) == TileCollision.Water);

                surfaceY++;

                // Drain left
                drainRow(currentX    , surfaceY, true);

                // Drain right
                drainRow(currentX + 1, surfaceY, false);

                drain = false; // Don't drain on the next frame.
            }
        }

        /// <summary>
        /// Drains a row of tiles, turing all Water tiles into Passable tiles.
        /// 
        /// The process stops when the first non-Water tile is found.
        /// </summary>
        /// <param name="x">The column to start draining from.</param>
        /// <param name="y">The row to drain.</param>
        /// <param name="lookLeft">True to drain left. False to drain right.</param>
        /// <returns></returns>
        private void drainRow(int x, int y, bool lookLeft)
        {
            Tile tile = level.getTile(x, y);

            while (tile.Collision == TileCollision.Water)
            {
                // Avoid important tiles
                if (!(tile is WaterSource) && !(tile is WaterDrain))
                {
                    tile.Collision = TileCollision.Passable;
                    tile.Sprite.Texture = null;
                }

                if (lookLeft)
                    x--;
                else
                    x++;

                if (level.isTileInBounds(x, y))
                {
                    tile = level.getTile(x, y);
                }
                else
                {
                    break;
                }
            }
        }

        public bool IsActive()
        {
            // Nothing to do here
            return true;
        }

        public void SetState(bool active)
        {
            decreaseWaterLevel();
        }

        public void ChangeState()
        {
            decreaseWaterLevel();
        }
    }
}
