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

namespace Platformer
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private List<MoveableTile> moveableTiles; // Tiles that span multiple cells
        private Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private Dictionary<String,Switch> switches = new Dictionary<string,Switch>();
        private Dictionary<String,Light> lights = new Dictionary<string,Light>();
        private Dictionary<String,Vector2> spawns_ = new Dictionary<string,Vector2>();

        private List<Vector2> spawns = new List<Vector2>();

        // Key locations in the level.        
        private Vector2 activeSpawn;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        private CameraDirector cameraDirector;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex, Camera2D camera)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Texture2D[3];
            for (int i = 0; i < layers.Length; ++i)
            {
                // Choose a random segment if each background layer for level variety.
                int segmentIndex = levelIndex;
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");

            // Set the camera
            TrackingDirector trackingDirector = new TrackingDirector(camera, Player);
            cameraDirector = trackingDirector;
        }

        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width = 0;
            List<string> lines = new List<string>();
            List<string> links = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                //Phase 1 - Read the level rows and determine the size
                string line = reader.ReadLine();
                while (line != null)
                {
                    //Be ready to break to phase 2
                    if (line == "END")
                    {
                        line = reader.ReadLine();
                        break;
                    }

                    string[] lineTiles = line.Split(',');
                    width = Math.Max(lineTiles.Length, width); 
                    lines.Add(line);
            
                    line = reader.ReadLine();
                }

                //Phase 2 - Read the level links
                while (line != null)
                {
                    links.Add(line);
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];
            moveableTiles = new List<MoveableTile>();

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                string line = lines[y];
                string[] lineTiles = line.Split(',');

                for (int x = 0; x < Width; ++x)
                {
                    if (x < lineTiles.Length)
                    {
                        string tileID = lineTiles[x].Trim();
                        char tileType = tileID[0]; //The type is always the first part of the string
                        //int uid = int.Parse(Regex.Match(tile, @"\d+").Value, NumberFormatInfo.InvariantInfo); //Hope this works

                        tiles[x, y] = LoadTile(tileType, tileID, x, y);
                    }
                    else //Just add empty tiles to make up the length
                    {
                        tiles[x, y] = LoadTile('.', ".", x, y);
                    }
                    tiles[x, y].Sprite.Position = new Vector2(x * Tile.Width, y * Tile.Height);
                }
            }

            //Link up stuff
            foreach (string link in links)
            {
                string[] linkGroup = link.Split(',');
                Switch sw = switches[linkGroup[0]];

                for(int i = 2; i < linkGroup.Length; i++)
                {
                    Console.WriteLine(linkGroup[i]);
                    sw.add(lights[linkGroup[i].Trim()]);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, string tileID, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Ladder
                case 'L':
                    return LoadTile("LadderBlock", TileCollision.Ladder);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // A spawn point
                case 'P':
                    return LoadSpawnTile(x, y);

                case 'I':
                    return LoadLight(x,y,tileID);

                case 'S':
                    return LoadSwitch(x,y,tileID);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Moveable block, horizontal
                case 'H':
                    return LoadMoveableTile("BlockA1", true, TileCollision.Impassable);

                // Moveable block, vertical
                case 'V':
                    return LoadMoveableTile("BlockA1", false, TileCollision.Impassable);

                // Unknown tile type character
                default:
                    return new Tile(null, TileCollision.Passable);
                    //throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            Sprite sprite = new Sprite(Content.Load<Texture2D>("Tiles/" + name),
                                       Tile.Width, Tile.Height);
            return new Tile(sprite, collision);
        }

        private Tile LoadMoveableTile(string name, bool isHorizontal, TileCollision collision)
        {
            Sprite sprite = new Sprite(Content.Load<Texture2D>("Tiles/" + name),
                                       Tile.Width, Tile.Height);
            float angle = (isHorizontal) ? 0.0f : (float)(Math.PI / 2.0);

            return new MoveableTile(sprite, collision, new Vector2(angle, 100), this);
        }

        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadSpawnTile(int x, int y)
        {
            Vector2 spawnPos = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            //Instatiate a player as needed, at the first discovered spawn point for now.
            if (Player == null)
            {
                activeSpawn = spawnPos;
                player = new Player(this, activeSpawn);
            }
            
            //Add spawn to array
            spawns.Add(spawnPos);

            return new Tile(new Sprite(Content.Load<Texture2D>("Tiles/Spawner"),Tile.Width, Tile.Height), TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        private Tile LoadLight(int x, int y, string uid)
        {
            Point Position = GetBounds(x, y).Center;
            lights.Add(uid, new Light(new Vector2(Position.X, Position.Y), this));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadSwitch(int x, int y, string uid)
        {
            Point Position = GetBounds(x, y).Center;
            switches.Add(uid, new Switch(new Vector2(Position.X, Position.Y), this));
            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

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
            if (x == Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y == Height)
                return TileCollision.Passable;
            return tiles[x, y].Collision;
        }

        public List<MoveableTile> getMoveableTiles()
        {
            return moveableTiles;
        }

        public Tile getTile(int x, int y)
        {
            Tile tile = null;
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                tile = tiles[x, y];
            }
            else
            {
                // Return an impassable tile to mark level boundries.
                Rectangle tilePos = new Rectangle(x * Tile.Width, y * Tile.Height,
                                                  Tile.Width, Tile.Height);
                tile = new Tile(new Sprite(null,tilePos), TileCollision.Impassable);
            }

            return tile;
        }



        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

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

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            //Do a bunch of stuff with the keyboard
            if (keyboardState.IsKeyDown(Keys.P))
            {
                OnPlayerKilled(null);
            }else if(keyboardState.IsKeyDown(Keys.O))
            {
                StartNewLife();
            }else if(keyboardState.IsKeyDown(Keys.Add))
            {
                int i = spawns.IndexOf(activeSpawn);
                if (i < spawns.Count - 1)
                {
                    activeSpawn = spawns[i + 1];
                }
                else
                {
                    activeSpawn = spawns[0];
                }
            }

            //Check activatables
            foreach (Switch a in switches.Values)
            {
                a.ChangeState(player, keyboardState);
            }

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                //timeRemaining -= gameTime.ElapsedGameTime;

                updateTiles(gameTime); 
                Player.Update(gameTime, keyboardState, gamePadState, touchState, accelState, orientation);
                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(new Vector2(exit.X, exit.Y)))
                {
                    //OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;

            // Invoke the camera
            if (cameraDirector is PanningDirector)
            {
                PanningDirector panningDirector = (PanningDirector)cameraDirector;
                if (panningDirector.Completed)
                    cameraDirector = new TrackingDirector(panningDirector.Camera, player);
            }
            cameraDirector.update(gameTime);
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            return;
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            return;
        }

        private void updateTiles(GameTime gameTime)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    tiles[x, y].update(gameTime);
                }
            }

            // Update non-atomic tiles
            foreach(Tile tile in moveableTiles)
                tile.update(gameTime);
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(activeSpawn);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i <= EntityLayer; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            DrawTiles(spriteBatch);

            foreach (Light light in lights.Values)
                light.Draw(gameTime, spriteBatch);

            foreach (Switch sw in switches.Values)
                sw.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    tiles[x, y].draw(spriteBatch);
                }
            }

            // Draw non-atomic tiles
            foreach (MoveableTile tile in moveableTiles)
            {
                tile.draw(spriteBatch);
            }

        }

        #endregion
    }
}
