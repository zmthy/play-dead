using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Platformer.Tiles;
using Platformer.Levels;

namespace Platformer.Tiles
{
    class WaterSource : Tile, IActivatable
    {
        private int initialWaterLevel;
        private bool fill;

        private Sprite emptySprite;
        private Sprite fullSprite;
        private Level level;

        public WaterSource(Sprite emptySprite, Sprite fullSprite)
            : base(fullSprite, TileCollision.Water)
        {
            this.emptySprite = emptySprite;
            this.fullSprite = fullSprite;

            initialWaterLevel = 3;
            fill = true;
        }

        public void increaseWaterLevel()
        {
            fill = true;
        }

        public void bindToLevel(Level level)
        {
            this.level = level;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!fill)
                return;

            fill = false;

            Vector2 gridPos = level.getGridPosition(Sprite.X, Sprite.Y);
            int currentCol = (int)gridPos.X;
            int currentRow = (int)gridPos.Y;

            if (initialWaterLevel >= 0)
            {
                // Fill the world upwards, row by row, with water.
                List<Vector2> newWaterTiles = new List<Vector2>();
                List<Vector2> waterTiles;
                for (int row = 0; row < initialWaterLevel; row++)
                {
                    // Fill left
                    waterTiles = fillRow(currentCol, currentRow - row, true);
                    newWaterTiles.AddRange(waterTiles);

                    // Fill right
                    waterTiles = fillRow(currentCol + 1, currentRow - row, false);
                    newWaterTiles.AddRange(waterTiles);
                }

                propogate(newWaterTiles);

                initialWaterLevel = -1;
            }
            else
            {
                // Only fill the top row
                int surfaceRow = currentRow;
                do
                {
                    surfaceRow--;
                } while (level.GetCollision(currentCol, surfaceRow) == TileCollision.Water);

                List<Vector2> newWaterTiles = new List<Vector2>();
                List<Vector2> waterTiles;

                // Fill left
                waterTiles = fillRow(currentCol, surfaceRow, true);
                newWaterTiles.AddRange(waterTiles);

                // Fill right
                waterTiles = fillRow(currentCol + 1, surfaceRow, false);
                newWaterTiles.AddRange(waterTiles);

                propogate(newWaterTiles);
            }
        }

        private List<Vector2> fillRow(int col, int row, bool lookLeft)
        {
            List<Vector2> newWaterTiles = new List<Vector2>();

            Tile tile = level.getTile(col, row);
            while (tile.Collision == TileCollision.Passable)
            {
                if (level.isTileInBounds(col, row))
                {
                    tile.Collision = TileCollision.Water;
                    tile.Sprite.Texture = fullSprite.Texture;
                    newWaterTiles.Add(new Vector2(col, row));

                    if (lookLeft)
                        col--;
                    else
                        col++;

                    tile = level.getTile(col, row);
                }
                else
                {
                    // We have left the level bounds
                    break;
                }
            }

            return newWaterTiles;
        }

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
