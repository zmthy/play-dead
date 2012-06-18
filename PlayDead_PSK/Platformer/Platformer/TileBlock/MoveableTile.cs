﻿using System;
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
    class MoveableTile :  Tile
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

        public Vector2 FrameVelocity
        {
            get { return frameVelocity; }
        }
        private Vector2 frameVelocity;

        public MoveableTile Leader { get; set; }

        private Level level;

        protected float WaitTimeS { get; set; }
        private const float MAX_WAIT_TIME_S = 0.2f;

        public MoveableTile(Sprite sprite, TileCollision collision, Vector2 velocity,
                           Level level)
            : base(sprite, collision)
        {
            this.velocity = velocity;
            this.level = level;

            Leader = this; // By default, tiles lead themselves
        }

        public override void update(GameTime gameTime)
        {
            // Get the elapse time since the last frame
            float elapsedS = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Leader != this && Leader.WaitTimeS <= 0) // If we are folloing the leader
            {
                velocity = Leader.Velocity;
                frameVelocity = Leader.FrameVelocity;
                Sprite.Position = Sprite.Position + FrameVelocity;

                Tile collidingTile = getCollidingTile();
                if (collidingTile is MoveableTile)
                {
                    // We have collided, notify the leader
                    Leader.reverseDirection();
                }
                else if (collidingTile is Tile)
                {
                    // We have collided, notify the leader
                    Leader.reverseDirection(MAX_WAIT_TIME_S);
                }
            }
            else if(Leader == this) // If we are the leader
            {
                if (WaitTimeS > 0)
                {
                    // Wait for some amount of time.
                    WaitTimeS = Math.Max(0.0f, WaitTimeS - elapsedS);
                }

                if (WaitTimeS <= 0)
                {
                    Tile collidingTile = getCollidingTile();

                    if (collidingTile is MoveableTile)
                    {
                        // We have collided, notify the leader
                        reverseDirection();
                    }
                    else if (collidingTile is Tile)
                    {
                        // We have collided, notify the leader
                        reverseDirection(MAX_WAIT_TIME_S);
                    }
                    else
                    {
                        // Move in the current direction.
                        frameVelocity = new Vector2((float)(Math.Cos(velocity.X) * velocity.Y * elapsedS),
                                                    (float)(Math.Sin(velocity.X) * velocity.Y * elapsedS));
                        Sprite.Position = Sprite.Position + frameVelocity;
                    }
                }
            }
        }

        public void reverseDirection(float waitTimeS = 0)
        {
            WaitTimeS = waitTimeS;
            velocity.X -= (float)Math.PI;
        }

        private Tile getCollidingTile()
        {
            Tile collidingTile = null;

            // Get adjacent tiles infront
            Vector2 currentCell = getGridPosition(Sprite.Center.X, Sprite.Center.Y);
            Vector2 nextLeft = getAdjacentCellAtAngle(currentCell, velocity.X - (float)(Math.PI / 4));
            Vector2 nextMiddle = getAdjacentCellAtAngle(currentCell, velocity.X);
            Vector2 nextRight = getAdjacentCellAtAngle(currentCell, velocity.X + (float)(Math.PI / 4));
            Vector2[] collidingCells = { nextLeft, nextMiddle, nextRight };

            // Check if we are colliding with a non-moving tile
            for (int i = 0; i < collidingCells.Length; i++)
            {
                Vector2 tileGridPos = collidingCells[i];
                Tile otherTile = level.getTile((int)tileGridPos.X, (int)tileGridPos.Y);
                TileCollision collision = level.GetCollision((int)tileGridPos.X, (int)tileGridPos.Y);

                if (collision == TileCollision.Impassable || collision == TileCollision.Platform)
                {
                    // We do a bounds check because tiles can move in *any* direction.
                    if (otherTile == null || Sprite.intersects(otherTile.Sprite))
                    {
                        collidingTile = otherTile;
                        break;
                    }
                }
            }

            // Check if we are colliding with a moving tile
            if (collidingTile == null)
            {
                List<MoveableTile> moveableTiles = level.getMoveableTiles();
                foreach (MoveableTile tile in moveableTiles)
                {
                    if (tile.Leader != this.Leader && Sprite.intersects(tile.Sprite))
                    {
                        collidingTile = tile;
                        break;
                    }
                }
            }

            return collidingTile;
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
    }
}