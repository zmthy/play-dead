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
                for (int row = 0; row < initialWaterLevel; row++)
                {
                    // Fill left
                    fillRow(currentCol, currentRow - row, true);

                    // Fill right
                    fillRow(currentCol + 1, currentRow - row, false);
                }

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

                // Fill left
                fillRow(currentCol    , surfaceRow, true);

                // Fill right
                fillRow(currentCol + 1, surfaceRow, false);
            }
        }

        private void fillRow(int col, int row, bool lookLeft)
        {
            Tile tile = level.getTile(col, row);

            while (tile.Collision == TileCollision.Passable)
            {
                tile.Collision = TileCollision.Water;
                tile.Sprite.Texture = fullSprite.Texture;

                if (lookLeft)
                    col--;
                else
                    col++;

                if (level.isTileInBounds(col, row))
                {
                    tile = level.getTile(col, row);
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
            increaseWaterLevel();
        }

        public void ChangeState()
        {
            increaseWaterLevel();
        }
    }
}
