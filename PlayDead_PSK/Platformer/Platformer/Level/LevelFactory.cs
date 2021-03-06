﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Platformer.Camera;
using Platformer.Tiles;
using Platformer.Laser;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Platformer.Levels
{
    class LevelFactory
    {
        #region Properties

        private IServiceProvider services;
        private ContentManager themedContent;
        private DynamicMap dynamicMap;

        private Random random;
        private GraphicsDevice gd;

        #endregion

        public LevelFactory(IServiceProvider services, DynamicMap dynamicMap, GraphicsDevice gd)
        {
            this.services = services;
            this.dynamicMap = dynamicMap;
            this.gd = gd;
        }

        /// <summary>
        /// Factory Method for level creation. Creates a new content manager for 
        /// loading a particular theme and loads the level from the provided file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="theme"></param>
        /// <returns>The newly created level.</returns>
        public Level CreateLevel(String name, String filepath, String theme)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("The level could not be loaded", filepath);

            random = new Random(354668); // Arbitrary, but constant seed

            //Setup the content manager for the level
            themedContent = new ContentManager(services);
            if (theme.Length == 0)
                themedContent.RootDirectory = "Content/Theme/default/";
            else
                themedContent.RootDirectory = "Content/Theme/" + theme + "/";


            Stream fileStream = TitleContainer.OpenStream(filepath);
            List<String> levelRows;
            List<String> levelBindings;
            using (StreamReader reader = new StreamReader(fileStream))
            {
                //Phase 1 - Read the level rows
                levelRows = ParseLevel(reader);
                //Phase 2 - Read the level links
                levelBindings = ParseBindings(reader);
            }

            int width = 0;
            if (levelRows.Count > 0)
                width = levelRows[0].Split(',').Length;

            int height = levelRows.Count;
            Level level = new Level(name, width * 2, height, themedContent);

            BuildLevel(levelRows, level);

            BindLevel(levelBindings, level);

            //Clean-up
            themedContent = null;
            random = null;

            return level;
        }

        #region Loading

        /// <summary>
        /// Reads in all the lines related to the tile layout from the level file.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<String> ParseLevel(StreamReader reader)
        {
            List<string> lines = new List<string>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                //Be ready to break to phase 2
                if (line.StartsWith("END"))
                    break;

                lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Reads in all the bindings for the level.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<String> ParseBindings(StreamReader reader)
        {
            List<string> links = new List<string>();
            while (!reader.EndOfStream)
            {
                string link = reader.ReadLine();
                links.Add(link);
            }

            return links;
        }

        /// <summary>
        /// Builds the level based on the tile layout. This method setups the 
        /// basic tile grid, along with creating activators, activatables and moving tiles.
        /// </summary>
        /// <param name="levelRows"></param>
        /// <param name="level"></param>
        private void BuildLevel(List<String> levelRows, Level level)
        {
            // Load the tiles and components
            for (int y = 0; y < level.Height; ++y)
            {
                string line = levelRows[y];
                string[] lineTiles = line.Split(',');

                for (int x = 0; x < level.Width; ++x)
                {
                    if (x < lineTiles.Length)
                    {
                        string tileID = lineTiles[x].Trim();
                        char tileType = '.';

                        if (tileID.Length > 0)
                             tileType = tileID[0]; //The type is always the first part of the string

                        //Create level tiles;
                        Tile newTile = CreateTile(tileType, x, y);
                        level.addTile(x, y, newTile);

                        if (newTile is WaterSource)
                        {
                            WaterSource waterSource = (WaterSource)newTile;
                            waterSource.bindToLevel(level);
                            level.addActivatable(tileID, waterSource);
                        }

                        if (newTile is WaterDrain)
                        {
                            WaterDrain waterDrain = (WaterDrain)newTile;
                            waterDrain.bindToLevel(level);
                            level.addActivatable(tileID, waterDrain);
                        }

                        int xDrawPostion = x * Tile.Width;
                        int yDrawPostion = y * Tile.Height;

                        //Create Activator
                        Platformer.Tiles.Activator actor = CreateActivator(tileType, xDrawPostion, yDrawPostion);
                        if (actor != null)
                        {
                            level.addActivator(tileID, actor);

                            //Special case to link exits
                            if (actor is Exit)
                            {
                                int uid = int.Parse(Regex.Match(tileID, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                                Exit exit = (Exit)actor;
                                exit.LevelIndex = uid;
                            }
                            if (actor is Mirror)
                            {
                                level.addActivatable(tileID, (Mirror)actor);
                            }
                        }

                        //Create Activatable
                        IActivatable active = CreateActivatable(tileType, xDrawPostion, yDrawPostion);
                        if (active != null)
                        {
                            level.addActivatable(tileID, active);

                            //Special case to link spawn points into the levels, could do this inside level itself
                            if (active is Spawner)
                            {
                                Spawner sp = (Spawner)active;
                                sp.bindToLevel(level);
                            }

                            // Special case to create ladder tiles (which are both tiles and activatable)
                            if (active is LadderTile)
                            {
                                level.addTile(x, y, (LadderTile) active);
                            }
                        }

                        //Create Moving Platform
                        MoveableTile mTile = CreateMoveable(tileType, xDrawPostion, yDrawPostion);
                        if (mTile != null)
                        {
                            level.addMoveable(tileID, mTile);
                            if (mTile is IActivatable)
                                level.addActivatable(tileID, (IActivatable)mTile);
                        }

                    }
                    else //Just add empty tiles to make up the length
                    {
                        level.addTile(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Adds bindings to the activators for their activatables.
        /// </summary>
        /// <param name="levelBinds"></param>
        /// <param name="level"></param>
        private void BindLevel(List<String> levelBinds, Level level)
        {
            //Link up stuff
            foreach (string link in levelBinds)
            {
                string[] linkGroup = link.Split(',');
                string actor = linkGroup[0].Trim();
                int msDelay = int.Parse(linkGroup[1]);

                if (actor.Equals("ACTIVE")) //Special case
                {
                    for (int i = 2; i < linkGroup.Length; i++)
                    {
                        string active = linkGroup[i].Trim();
                        level.activate(active);
                    }
                }
                else
                {
                    for (int i = 2; i < linkGroup.Length && linkGroup[i].Length > 0; i++)
                    {
                        string active = linkGroup[i].Trim();
                        level.addBind(actor, active);
                    }
                }
            }
        }

        #region Tile Creation
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
        private Tile CreateTile(char tileType, int x, int y)
        {
            int which;
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(new Sprite(null, Tile.Width, Tile.Height), TileCollision.Passable);

                // Ladder - now with lowercase l for just standard ladders, Capital implies it is an activatible ladder that starts inactive by default
                //          This ladder tile will start activated by default - should be that if there is no matching activatable switch then it is on by default
                case 'l':
                    return CreateTile("Ladder", TileCollision.Ladder);

                // Platform block
                case '~':
                    return CreateVarietyTile("BlockA", 4, TileCollision.Platform);

                // Passable block
                case ':':
                    return CreateVarietyTile("BlockA", 4, TileCollision.Passable);

                // Impassable block
                case '#':
                    return CreateVarietyTile("BlockA", 4, TileCollision.Impassable);

                // Spike block
                case '^':
                    return CreateVarietyTile("Spikes", 2, TileCollision.Death);

                // Water source block
                case 'W':
                    //which = random.Next(8);
                    return CreateWaterSource("Water0");

                // Water drain block
                case 'E':
                    //which = random.Next(8);
                    return CreateWaterDrain("Water0");

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
        private Tile CreateTile(string name, TileCollision collision)
        {
            Sprite sprite = new Sprite(themedContent.Load<Texture2D>("Tiles/" + name), Tile.Width, Tile.Height);
            return new Tile(sprite, collision);
        }

        private WaterSource CreateWaterSource(string name)
        {
            Sprite emptySprite = new Sprite(null, Tile.Width, Tile.Height);
            Sprite fullSprite = new Sprite(themedContent.Load<Texture2D>("Tiles/" + name), Tile.Width, Tile.Height);
            return new WaterSource(emptySprite, fullSprite);
        }

        public WaterDrain CreateWaterDrain(string name)
        {
            Sprite sprite = new Sprite(themedContent.Load<Texture2D>("Tiles/" + name), Tile.Width, Tile.Height);
            return new WaterDrain(sprite);
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
        private Tile CreateVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return CreateTile(baseName + index, collision);
        }

        #endregion

        #region Activator Creation
        /// <summary>
        /// Creates the different kinds of activators.
        /// </summary>
        /// <param name="tileType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Platformer.Tiles.Activator CreateActivator(char tileType, int x, int y)
        {
            switch (tileType)
            {
                case 'S':
                    return CreateSwitch(x, y);

                case 'T':
                    return CreateWheel(x, y);

                case 'B':
                    return CreateSwitch(x, y);

                case 'X':
                    return CreateExit(x, y);

                case 'M':
                    return CreateMirror(x, y);

                case 'Z':
                    return CreateLaser(x, y);

                default:
                    return null;
            }
        }

        private Platformer.Tiles.Activator CreateSwitch(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            Switch sw = new Switch(tileCenter, themedContent); 
            return sw;
        }

        private Platformer.Tiles.Activator CreateWheel(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            Switch sw = new Switch(tileCenter, themedContent, "Wheel1", "Wheel2");
            return sw;
        }

        private Platformer.Tiles.Activator CreateExit(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            Exit exit = new Exit(tileCenter, themedContent);
            return exit;
        }

        private Platformer.Tiles.Activator CreateButton(int x, int y)
        {
            //To be implemented
            return CreateSwitch(x, y);
        }

        private Platformer.Tiles.Activator CreateMirror(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            Mirror m = new Mirror(tileCenter,themedContent);
            return m;
        }

        private Platformer.Tiles.Activator CreateLaser(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            Emitter l = new Emitter(tileCenter,gd,themedContent);
            return l;
        }
        #endregion

        #region Activatable Creation
        /// <summary>
        /// Creates the different kinds of activatables.
        /// </summary>
        /// <param name="tileType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Platformer.Tiles.IActivatable CreateActivatable(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // A spawn point
                case 'P':
                    return CreateSpawner(x, y);

                // A Light
                case 'I':
                    return CreateLight(x, y);
                    
                case 'L':
                    return CreateLadder(x, y);

                case 'M':
                    //return CreateMirror(x, y);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private IActivatable CreateSpawner(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            return new Spawner(tileCenter, themedContent);
        }

        private IActivatable CreateLight(int x, int y)
        {
            Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
            return new Light(tileCenter, themedContent);
        }

        //private IActivatable CreateMirror(int x, int y)
        //{
        //    Vector2 tileCenter = new Vector2(x + Tile.Width / 2, y + Tile.Height / 2);
        //    return new Mirror(tileCenter, themedContent);
        //}

        private IActivatable CreateLadder(int x, int y)
        {
            //ladder tile initialised to be inactive
            Sprite ladderSprite = new Sprite(themedContent.Load<Texture2D>("Tiles/Ladder"), Tile.Width, Tile.Height);
            return new LadderTile(ladderSprite, TileCollision.Passable);            
        }

        #endregion

        #region Moveable Creation
        /// <summary>
        /// Creates the different kinds of moveable tiles.
        /// </summary>
        /// <param name="tileType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private MoveableTile CreateMoveable(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Moveable block, horizontal
                case 'H':
                    return CreateSlidingTile(x, y, true, TileCollision.Platform);

                // Moveable block, vertical
                case 'V':
                    return CreateSlidingTile(x, y, false, TileCollision.Platform);

                // Moveable block, vertical
                case 'D':
                    return CreateDoorTile(x, y, false, TileCollision.Impassable);

                default:
                    return null;
            }
        }

        private MoveableTile CreateDoorTile(int x, int y, bool p, TileCollision tileCollision)
        {
            Sprite sprite = new Sprite(themedContent.Load<Texture2D>("Tiles/BlockA0"), Tile.Width, Tile.Height);
            sprite.Position = new Vector2(x, y);

            return new DoorTile(sprite, tileCollision, 100);
        }

        private MoveableTile CreateSlidingTile(int x, int y, bool isHorizontal, TileCollision collision)
        {
            Sprite sprite = new Sprite(themedContent.Load<Texture2D>("Tiles/BlockA0"), Tile.Width, Tile.Height);
            sprite.Position = new Vector2(x, y);
            float angle = (isHorizontal) ? 0.0f : (float)(Math.PI / 2.0);

            return new SlidingTile(sprite, collision, new Vector2(angle, 100));
        }
        #endregion

        #endregion
    }
}
