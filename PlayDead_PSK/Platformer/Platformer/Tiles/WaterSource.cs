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
    /// A water source is a tap that pushes water into an area.
    /// Any change of state invoked by the IActivatable interface will
    /// always raise the water level.
    /// </summary>
    class WaterSource : Tile, IActivatable
    {
        /// <summary>
        /// The water level (in tiles) to add on creation.
        /// </summary>
        private int initialWaterLevel;

        /// <summary>
        /// Whether or not the water level should be incremented by
        /// one (tile) on the next update.
        /// </summary>
        private bool fill;

        /// <summary>
        /// Sprite to indicate a non-water background.
        /// </summary>
        private Sprite emptySprite;

        /// <summary>
        /// Sprite to indicate a water background.
        /// </summary>
        private Sprite fullSprite;

        /// <summary>
        /// The level the water source belongs to.
        /// </summary>
        private Level level;

        /// <summary>
        /// Creates a new water source.
        /// </summary>
        /// <param name="emptySprite">Sprite to indicate a non-water background.</param>
        /// <param name="fullSprite">prite to indicate a water background.</param>
        public WaterSource(Sprite emptySprite, Sprite fullSprite)
            : base(fullSprite, TileCollision.Water)
        {
            this.emptySprite = emptySprite;
            this.fullSprite = fullSprite;

            this.IsFlooded = true;

            initialWaterLevel = 3;
            fill = true;
        }

        /// <summary>
        /// Increase the water level by one (tile).
        /// </summary>
        public void increaseWaterLevel()
        {
            fill = true;
        }

        /// <summary>
        /// Assigns the level the water source belongs to.
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
            if (!fill)
                return;

            fill = false; // Don't fill on the next frame.

            Vector2 gridPos = level.getGridPosition(Sprite.X, Sprite.Y);
            int currentX = (int)gridPos.X;
            int currentY = (int)gridPos.Y;

            if (initialWaterLevel >= 0)
            {
                // Fill the world upwards, row by row, with water.
                List<Vector2> newWaterTiles = new List<Vector2>();
                List<Vector2> waterTiles;
                for (int y = 0; y < initialWaterLevel; y++)
                {
                    // Fill left
                    waterTiles = fillRow(currentX, currentY - y, true);
                    newWaterTiles.AddRange(waterTiles);

                    // Fill right
                    waterTiles = fillRow(currentX + 1, currentY - y, false);
                    newWaterTiles.AddRange(waterTiles);
                }

                propogate(newWaterTiles);

                initialWaterLevel = -1;
            }
            else
            {
                // Only fill the top row
                int surfaceY = currentY;
                do
                {
                    surfaceY--;
                } while (level.GetCollision(currentX, surfaceY) == TileCollision.Water);

                List<Vector2> newWaterTiles = new List<Vector2>();
                List<Vector2> waterTiles;

                // Fill left
                waterTiles = fillRow(currentX, surfaceY, true);
                newWaterTiles.AddRange(waterTiles);

                // Fill right
                waterTiles = fillRow(currentX + 1, surfaceY, false);
                newWaterTiles.AddRange(waterTiles);

                // Propogate the movement of any new tiles
                propogate(newWaterTiles);
            }
        }

        /// <summary>
        /// Fills a row of tiles, turing all Passable tiles into Water tiles.
        /// 
        /// The process stops when the first non-Passable tile is found.
        /// </summary>
        /// <param name="x">The column to start filling from.</param>
        /// <param name="y">The row to fill.</param>
        /// <param name="lookLeft">True to fill left. False to fill right.</param>
        /// <returns></returns>
        private List<Vector2> fillRow(int x, int y, bool lookLeft)
        {
            List<Vector2> newWaterTiles = new List<Vector2>();

            Tile tile = level.getTile(x, y);
            while (tile.Collision == TileCollision.Passable)
            {
                if (level.isTileInBounds(x, y))
                {
                    tile.Collision = TileCollision.Water;
                    tile.Sprite.Texture = fullSprite.Texture;
                    tile.IsFlooded = true;
                    newWaterTiles.Add(new Vector2(x, y));

                    if (lookLeft)
                        x--;
                    else
                        x++;

                    tile = level.getTile(x, y);
                }
                else
                {
                    // We have left the level bounds
                    break;
                }
            }

            return newWaterTiles;
        }

        /// <summary>
        /// Propogates water tiles by filling adjacent Passable tiles with water.
        /// For all provided water tiles, if a Passable tile is found below, to
        /// the right, or to the left, that tile will be made Water and will be
        /// propogated.
        /// </summary>
        /// <param name="waterTiles">The water tiles to propogate.</param>
        private void propogate(List<Vector2> waterTiles)
        {
            List<Vector2> newWaterTiles = new List<Vector2>();
            Queue<Vector2> remainingWater = new Queue<Vector2>(waterTiles);

            while (remainingWater.Count > 0)
            {
                Vector2 tilePos = remainingWater.Dequeue();
                int tileX = (int)tilePos.X;
                int tileY = (int)tilePos.Y;

                // Check beneath tile
                if(level.GetCollision(tileX, tileY + 1) == TileCollision.Passable)
                {
                    // Make the tile a water tile
                    Tile tile = level.getTile(tileX, tileY + 1);
                    tile.Collision = TileCollision.Water;
                    tile.Sprite.Texture = fullSprite.Texture;
                    tile.IsFlooded = true;

                    // Remember to propogate the new water tile later.
                    newWaterTiles.Add(new Vector2(tileX, tileY + 1));
                }

                // Fill to the left of the tile
                newWaterTiles.AddRange(fillRow(tileX - 1, tileY, true));

                // Fill to the right of the tile
                newWaterTiles.AddRange(fillRow(tileX + 1, tileY, true));
            }

            // Propogate any new water tiles that were created
            if(waterTiles.Count > 0)
                propogate(newWaterTiles);
        }

        public bool IsActive()
        {
            // Nothing to do here
            return true;
        }

        public void SetState(bool active)
        {
            increaseWaterLevel();
        }

        public void ChangeState()
        {
            increaseWaterLevel();
        }
    }
}
