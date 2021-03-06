﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Levels;

namespace Platformer.Tiles
{
    /// <summary>
    /// A tile that moves in a given direction.
    /// Once the tile collides with a solid object, it reverses its direction.
    /// 
    /// Code inspired from:
    /// http://robotfootgames.com/2011/05/movable-platforms/
    /// </summary>
    abstract class MoveableTile :  Tile
    {
        /// <summary>
        /// The movement velocity of the tile.
        /// The first component (.X) is the angle in radians.
        /// The second component (.Y) is the speed in pixels per second.
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            protected set { velocity = value; }
        }
        private Vector2 velocity;

        /// <summary>
        /// The movement velocity of the tile for the current frame
        /// The first component (.X) is the angle in radians.
        /// The second component (.Y) is the speed in pixels for the current frame.
        /// </summary>
        public Vector2 FrameVelocity
        {
            get { return frameVelocity; }
            protected set { frameVelocity = value; }
        }
        private Vector2 frameVelocity;

        /// <summary>
        /// A leading tile is one which watching tiles follow.
        /// If the current tile is the Leader, Leader should be this.
        /// Leader should be same type as sub-class! TODO: How to enforce this?
        /// </summary>
        public MoveableTile Leader { get; set; }

        /// <summary>
        /// The level the tile is on.
        /// </summary>
        private Level level;

        /// <summary>
        /// Amount of time remaining to wait upon collision with a tile.
        /// Waiting allows the player to more easily jump on the platform.
        /// </summary>
        protected float WaitTimeS { get; set; }

        /// <summary>
        /// TIme to wait upon collision with a tile.
        /// </summary>
        private const float MAX_WAIT_TIME_S = 0.2f;

        /// <summary>
        /// Creates a new Moveable tile.
        /// </summary>
        /// <param name="sprite">Sprite representing texture, size, and position of the tile. </param>
        /// <param name="collision">Type of collision for the tile.</param>
        /// <param name="velocity">Tile velocity: .X is the movement angle in radians and .Y is the speed in pixels per second.</param>
        public MoveableTile(Sprite sprite, TileCollision collision, Vector2 velocity)
            : base(sprite, collision)
        {
            this.velocity = velocity;

            Leader = this; // By default, tiles lead themselves
        }

        public void bindToLevel(Level level)
        {
            this.level = level;
        }

        protected Tile getCollidingTile()
        {
            Tile collidingTile = null;

            // Get adjacent tiles infront
            Vector2 currentCell = level.getGridPosition(Sprite.Center.X, Sprite.Center.Y);
            Vector2 nextLeft = getAdjacentCellAtAngle(currentCell, velocity.X - (float)(Math.PI / 4));
            Vector2 nextMiddle = getAdjacentCellAtAngle(currentCell, velocity.X);
            Vector2 nextRight = getAdjacentCellAtAngle(currentCell, velocity.X + (float)(Math.PI / 4));
            Vector2[] collidingCells = { nextLeft, nextMiddle, nextRight };

            // Check if we are colliding with a non-moving tile
            for (int i = 0; i < collidingCells.Length; i++)
            {
                Vector2 tileGridPos = collidingCells[i];
                Tile otherTile = level.getTile((int)tileGridPos.X, (int)tileGridPos.Y);
                TileCollision collision = otherTile.Collision;

                if (collision == TileCollision.Impassable || collision == TileCollision.Platform)
                {
                    // We do a bounds check because tiles can move in *any* direction.
                    if (Sprite.intersects(otherTile.Sprite))
                    {
                        collidingTile = otherTile;
                        break;
                    }
                }
            }

            // Check if we are colliding with a moving tile
            if (collidingTile == null)
            {
                List<MoveableTile> moveableTiles = level.MoveableTiles;
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

        private Vector2 getAdjacentCellAtAngle(Vector2 currentCell, float angleRadians)
        {
            int newCellX = (int)currentCell.X + (int)Math.Round(Math.Cos(angleRadians));
            int newCellY = (int)currentCell.Y + (int)Math.Round(Math.Sin(angleRadians));
            return new Vector2(newCellX, newCellY);
        }
    }
}
