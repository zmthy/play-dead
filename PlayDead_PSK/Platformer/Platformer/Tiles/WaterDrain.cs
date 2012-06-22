using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Platformer.Tiles;
using Platformer.Levels;

namespace Platformer.Tiles
{
    class WaterDrain : Tile, IActivatable
    {
        private bool drain;
        private Level level;

        public WaterDrain(Sprite sprite)
            : base(sprite, TileCollision.Passable)
        {
        }

        public void decreaseWaterLevel()
        {
            drain = true;
        }

        public void bindToLevel(Level level)
        {
            this.level = level;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (drain)
            {
                // Find the surface of the water.
                Vector2 gridPos = level.getGridPosition(Sprite.X, Sprite.Y);
                int currentCol = (int)gridPos.X;
                int currentRow = (int)gridPos.Y;
                int surfaceRow = currentRow;
                do
                {
                    surfaceRow--;
                } while (level.GetCollision(currentCol, surfaceRow) == TileCollision.Water);

                surfaceRow++;
;
                // Drain left
                drainRow(currentCol    , surfaceRow, true);

                // Drain right
                drainRow(currentCol + 1, surfaceRow, false);

                drain = false;
            }
        }

        private void drainRow(int col, int row, bool lookLeft)
        {
            Tile tile = level.getTile(col, row);

            while (tile.Collision == TileCollision.Water)
            {
                // Avoid important tiles
                if (!(tile is WaterSource) && !(tile is WaterDrain))
                {
                    tile.Collision = TileCollision.Passable;
                    tile.Sprite.Texture = null;
                }

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
            decreaseWaterLevel();
        }

        public void ChangeState()
        {
            decreaseWaterLevel();
        }
    }
}
