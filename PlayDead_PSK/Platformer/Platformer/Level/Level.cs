#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

using Platformer.Camera;
using Platformer.Tiles;

namespace Platformer.Levels
{

    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        #region Properties
        //Level Structure
        private Tile[,] tiles;
        private Dictionary<String, Platformer.Tiles.IActivatable> activatables;
        private Dictionary<String, Platformer.Tiles.Activator> activators;

        public List<MoveableTile> MoveableTiles
        {
            get
            {
                List<MoveableTile> allMTiles = new List<MoveableTile>();
                foreach (List<MoveableTile> mtList in moveableTiles.Values)
                    allMTiles.AddRange(mtList);
                return allMTiles;
            }
        }
        private Dictionary<String, List<MoveableTile>> moveableTiles; // Tiles that span multiple cells

        public String Name
        {
            get { return name; }
        }
        private String name;

        public ContentManager Content
        {
            get { return content; }
        }
        private ContentManager content;

        public Spawner ActiveSpawn
        {
            get { return activeSpawn; }
        }
        private Spawner activeSpawn;

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        public Level(String name, int width, int height, ContentManager content)
        {
            this.name = name;
            this.content = content;

            tiles = new Tile[width, height];
            activatables = new Dictionary<string, IActivatable>();
            activators = new Dictionary<string, Tiles.Activator>();
            moveableTiles = new Dictionary<string, List<MoveableTile>>();
        }

        public void Dispose()
        {
            content.Dispose();
        }

        /// <summary>
        /// Adds an empty passable tile into the tile array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void addTile(int x, int y)
        {
            addTile(x, y, new Tile(null, TileCollision.Passable));
        }

        /// <summary>
        /// Adds a tile into the tile array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newTile"></param>
        public void addTile(int x, int y, Tile newTile)
        {
            newTile.Sprite.Position = new Vector2(x * Tile.Width, y * Tile.Height);
            tiles[x, y] = newTile;
        }

        /// <summary>
        /// Adds an activator into the activators list.
        /// </summary>
        /// <param name="tileID">The ID of the tile.</param>
        /// <param name="actor">The activator instance.</param>
        public void addActivator(string tileID, Tiles.Activator actor)
        {
            activators.Add(tileID, actor);
        }

        /// <summary>
        /// Adds and activatable item to the activatables list.
        /// </summary>
        /// <param name="tileID">The ID of the tile.</param>
        /// <param name="active">The activatable instance.</param>
        public void addActivatable(string tileID, IActivatable active)
        {
            if(!activatables.ContainsKey(tileID))
                activatables.Add(tileID, active);
        }

        /// <summary>
        /// Adds a moveable tile to the level. If the ID of the tile already exists then the tile is linked to the exiting tile as its leader.
        /// </summary>
        /// <param name="tileID">The ID of the tile.</param>
        /// <param name="mTile">The moveable tile instance.</param>
        public void addMoveable(string tileID, MoveableTile mTile)
        {
            if (moveableTiles.ContainsKey(tileID))
            {
                mTile.Leader = moveableTiles[tileID][0];
            }
            else
            {
                moveableTiles.Add(tileID, new List<MoveableTile>());
            }

            moveableTiles[tileID].Add(mTile);
            mTile.bindToLevel(this);
        }

        /// <summary>
        /// Binds and activator to a particular activatable.
        /// </summary>
        /// <param name="actorName">The activator which will trigger.</param>
        /// <param name="activeName">The activatable which will  be activated.</param>
        public void addBind(string actorName, string activeName)
        {
            if (activatables.ContainsKey(activeName) && activators.ContainsKey(actorName))
            {
                Tiles.Activator actor = activators[actorName];
                actor.add(activatables[activeName]);
            }
        }

        /// <summary>
        /// Activates a particular activatable.
        /// </summary>
        /// <param name="active">The ID of the activatable.</param>
        public void activate(string active)
        {
            if(activatables.ContainsKey(active))
                activatables[active].SetState(true);
        }

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public TileCollision GetTileCollisionBehindPlayer(Vector2 playerPosition)
        {
            int x = (int)playerPosition.X / Tile.Width;
            int y = (int)(playerPosition.Y - 1) / Tile.Height;
            // Prevent escaping past the level ends.
            if (x == Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y == Height)
                return TileCollision.Passable;
            return tiles[x, y].Collision;
        }

        public TileCollision GetTileCollisionBelowPlayer(Vector2 playerPosition)
        {
            int x = (int)playerPosition.X / Tile.Width;
            int y = (int)(playerPosition.Y) / Tile.Height;
            // Prevent escaping past the level ends.
            if (x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public Tile getTile(int x, int y)
        {
            Tile tile = null;

            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                tile = new Tile(new Sprite(null, Tile.Width, Tile.Height), TileCollision.Impassable);

            // Allow jumping past the level top and falling through the bottom.
            if(y < 0 || y >= Height)
                tile = new Tile(new Sprite(null, Tile.Width, Tile.Height), TileCollision.Passable);

            if (tile == null)
                tile = tiles[x, y];

            return tile;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public RectangleF GetBounds(int x, int y)
        {
            return new RectangleF(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(Player player, GameTime gameTime, KeyboardState keyboardState, InputManager inputManager)
        {
            //Check activatables
            foreach (Platformer.Tiles.Activator a in activators.Values)
                a.ChangeState(player, keyboardState, inputManager);

            //Update each tile
            foreach (Tile tile in tiles)
                tile.Update(gameTime);

            // Update non-atomic tiles
            foreach(MoveableTile mTile in MoveableTiles)
                mTile.Update(gameTime);

            // Falling off the bottom of the level kills the player.
            if (player.BoundingRectangle.Top >= Height * Tile.Height)
                player.OnKilled();
        }

        /// <summary>
        /// Called by a Spawner when it has been activated.
        /// </summary>
        /// <param name="spawner">The spawner to set as the active spawn point.</param>
        public void UpdateSpawn(Spawner spawner)
        {
            //Turn off current spawn point
            if (activeSpawn != null)
                activeSpawn.SetSpawnState(false);

            //Turn on new spawn point
            activeSpawn = spawner;
            activeSpawn.SetSpawnState(true);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw Tiles
            foreach (Tile tile in tiles)
                tile.Draw(spriteBatch);

            // Draw non-atomic tiles
            foreach (MoveableTile tile in MoveableTiles)
                tile.Draw(spriteBatch);

            //Draw Activatables
            foreach (IActivatable active in activatables.Values)
                active.Draw(spriteBatch);

            //Draw Activators
            foreach (Platformer.Tiles.Activator actor in activators.Values)
                actor.Draw(gameTime, spriteBatch);
        }
        #endregion
    }
}
