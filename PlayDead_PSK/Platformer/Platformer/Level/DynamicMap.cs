using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Platformer.Camera;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer.Levels
{
    /// <summary>
    /// A Map which holds a number of levels and can switch between them. It also holds the player instance.
    /// </summary>
    class DynamicMap : IDisposable
    {
        #region Properties
        private LevelFactory levelFactory;
        private Dictionary<string, Level> existingLevels;

        public Level Level
        {
            get { return activeLevel; }
        }
        private Level activeLevel;

        public Player Player
        {
            get { return player; }
        }
        private Player player;

        public ContentManager Content
        {
            get { return content; }
        }
        private ContentManager content;

        public CameraDirector Camera
        {
            get { return camera; }
        }
        private CameraDirector camera;

        #endregion

        public DynamicMap(IServiceProvider serviceProvider, Camera2D camera)
        {
            this.content = new ContentManager(serviceProvider, "Content");
            this.levelFactory = new LevelFactory(serviceProvider);
            this.player = new Player(Content, new Vector2());
            this.camera = new TrackingDirector(camera, Player);
            this.existingLevels = new Dictionary<string, Level>();
        }

        /// <summary>
        /// Load a story and create all of the levels listed in the file by using the LevelFactory.
        /// </summary>
        /// <param name="storyPath">The filepath of the story file.</param>
        public void LoadStory(string storyPath)
        {
            if (!File.Exists(storyPath))
                throw new FileNotFoundException("The story could not be found", storyPath);

            existingLevels.Clear();

            Stream fileStream = TitleContainer.OpenStream(storyPath);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                //Just loads all the levels
                while(!reader.EndOfStream)
                {
                    string newLine = reader.ReadLine();
                    string[] tuple = newLine.Split(','); //Should be level name, filepath, theme

                    Level newLevel = levelFactory.CreateLevel("Content/" + tuple[1].Trim(), tuple[2].Trim());
                    existingLevels.Add(tuple[0].Trim(), newLevel);

                    if (activeLevel == null)
                        activeLevel = newLevel;
                }

            }

            //Vector2 SpawnPoint = activeLevel.ActiveSpawn.Position;
            player.EnterLevel(activeLevel);
        }

        /// <summary>
        /// Update the Player state, the tracking camera, and the currently active level.
        /// </summary>
        /// <param name="gameTime">The current time step of the game.</param>
        /// <param name="keyboardState">The current state of the PC keyboard.</param>
        /// <param name="gamePadState"></param>
        /// <param name="touchState"></param>
        /// <param name="accelState"></param>
        /// <param name="orientation"></param>
        public void Update(GameTime gameTime, 
            KeyboardState keyboardState,
            GamePadState gamePadState,
            TouchCollection touchState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {

            //Update the Player
            if (!Player.IsAlive)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);

                if(keyboardState.IsKeyDown(Keys.R))
                    Player.Reset(activeLevel.ActiveSpawn.Position);
            }
            else
            {
                Player.Update(gameTime, keyboardState, gamePadState, touchState, accelState, orientation);
            }

            // Update the camera
            if (camera is PanningDirector)
            {
                PanningDirector panningDirector = (PanningDirector)camera;
                if (panningDirector.Completed)
                    camera = new TrackingDirector(panningDirector.Camera, player);
            }
            camera.update(gameTime);

            //Update the current level
            activeLevel.Update(Player, gameTime, keyboardState);
        }

        /// <summary>
        /// Draws the player and the currently active level to the screen.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            activeLevel.Draw(gameTime, spriteBatch);
            Player.Draw(gameTime, spriteBatch);
        }
        
        /// <summary>
        /// Will unload all of the loaded content.
        /// </summary>
        public void  Dispose()
        {
            Content.Unload();
            foreach (Level level in existingLevels.Values)
            {
                level.Dispose();
            }
        }
}
}
