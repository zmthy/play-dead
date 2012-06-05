using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace PlatformTest
{
    class Map
    {
        public const int size = 32; //The size of each tile in pixels
        public static int height; //in tiles
        public static int width; //in tiles

        static int[,] tileMap;

        public Map(GraphicsDevice graphics) //Creates a new map based on the size of the screen
        {
            height = graphics.DisplayMode.Height / size;
            width = graphics.DisplayMode.Width / size;

            tileMap = new int[height, width]; //Initializes the new map

            for (int y = 0; y < height; y++)  //Defaults all values to -1 (saves processing time)
                for (int x = 0; x < width; x++)
                    tileMap[y, x] = -1;
            for (int x = 0; x < width; x++) //Better put a floor down!
                tileMap[12, x] = 1;
            for (int y = 13; y < height; y++)  //Fill everything under the floor so it looks nice
                for (int x = 0; x < width; x++)
                    tileMap[y, x] = 4;
            
        }

        public static int getType(int y, int x)
        {
            if (!(x < 0 || y < 0) && !(x > width || y > height))
            {
                return tileMap[y, x];
            }
            else
                return -1;
        }

        public void Set(int y, int x, int currentTile) //SAFELY sets the value of a clicked tile based on the currently selected tile in the editor
        {
            if (!(x < 0 || y < 0))
            {
                tileMap[y, x] = currentTile;
            }
        }

        public void DrawMap(SpriteBatch spriteBatch, Texture2D tileSheet)
        {
            height = tileMap.GetLength(0); //Map height (in tiles)
            width = tileMap.GetLength(1); //Map width (in tiles)

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int cell = tileMap[y, x]; //Cell = value of tile at map position (y,x)

                    if (cell > -1)//If cell exists; otherwise, continue (saves processing time)
                    {
                        Rectangle sourceBounds = SourcePoint(cell, tileSheet); //Gets coodinates of tile on the tilesheet
                        Rectangle bounds = new Rectangle(x * size, y * size, size, size); //Sets where and what size the tile will be in the game

                        spriteBatch.Draw(tileSheet, bounds, sourceBounds, Color.White); //Draws the tile
                    }
                }
        }

        private Rectangle SourcePoint(int cell, Texture2D tileSheet) //Takes an integer value and converts it to its corresponding tile location
        {
            //Example of the data processed:
            //0  1  2
            //3  4  5
            //6  7  8
            //9  10 11
            //12 13 14
            //15 16 17
            //
            //If cell = 7, for example:
            //tileWidth = 3
            //y = 7 / 3 = 2
            //x = 7 % 3 (the remainder of 7 / 3) = 1
            //(x,y) value returned = (32,64)

            Rectangle point = new Rectangle(0, 0, size, size); //The point to be returned
            int tileWidth = tileSheet.Width / size;              //How many tiles can fit in the tileset horizontally
            int y = cell / tileWidth;                         //Returns the row of the tile
            int x = cell % tileWidth;                         //Returns how many tiles "cell" is displaced by

            point = new Rectangle(x * size, y * size, size, size); //Compacts the above data into a source rectangle

            return point; //returns value
        }

    }
}
