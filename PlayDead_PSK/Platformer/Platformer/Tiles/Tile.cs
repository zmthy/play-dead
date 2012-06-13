#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Tiles
{
    /// <summary>
    /// Controls the collision detection and response behavior of a tile.
    /// </summary>
    enum TileCollision
    {
        /// <summary>
        /// A passable tile is one which does not hinder player motion at all.
        /// </summary>
        Passable = 0,

        /// <summary>
        /// An impassable tile is one which does not allow the player to move through
        /// it at all. It is completely solid.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// A platform tile is one which behaves like a passable tile except when the
        /// player is above it. A player can jump up through a platform as well as move
        /// past it to the left and right, but can not fall down through the top of it.
        /// </summary>
        Platform = 2,

        Ladder = 3,
    }

    /// <summary>
    /// Stores the appearance and collision behavior of a tile.
    /// </summary>
    class Tile
    {
        public Sprite Sprite { get; protected set; }
        public TileCollision Collision { get; protected set; }

        public const int Width = 40;
        public const int Height = 32;
        public const int Center = Width / 2;

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Tile(Sprite sprite, TileCollision collision)
        {
            if (sprite != null)
                Sprite = sprite;
            else
                Sprite = new Sprite();
            Collision = collision;
        }

        public virtual void update(GameTime gameTime) { }

        public void draw(SpriteBatch spriteBatch)
        {
            if (Sprite != null)
                Sprite.draw(spriteBatch);
        }
    }
}
