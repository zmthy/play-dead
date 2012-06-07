using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace PlayDead
{
    class Map
    {
        public const int size = 32; //The size of each tile in pixels
        public static int height; //in tiles
        public static int width; //in tiles

        static Tile[,] tileMap;

        public Map(GraphicsDevice graphics,Texture2D tileSheet) //Creates a new map based on the size of the screen
        {
            height = graphics.DisplayMode.Height / size;
            width = graphics.DisplayMode.Width / size;

            tileMap = new Tile[height, width]; //Initializes the new map

            for (int y = 0; y < height; y++)  //Defaults all values to -1 (saves processing time)
                for (int x = 0; x < width; x++)
                    tileMap[y, x] = null;
            for (int x = 0; x < width; x++) //Better put a floor down!
                tileMap[12, x] = new Tile(tileSheet, new Rectangle(size*1,size*0,size,size),Tile.COLLIDES_WITH_TOP);
            for (int y = 13; y < height; y++)  //Fill everything under the floor so it looks nice
                for (int x = 0; x < width; x++)
                    tileMap[y, x] = new Tile(tileSheet, new Rectangle(size * 1, size * 1, size, size), Tile.SOLID_BLOCK);
            
        }

        public static Tile getType(int y, int x)
        {
            if (!(x < 0 || y < 0) && !(x > width || y > height))
            {
                return tileMap[y, x];
            }
            else
                return null;
        }

        public void Set(int y, int x, Tile currentTile) //SAFELY sets the value of a clicked tile based on the currently selected tile in the editor
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
                    Tile cell = tileMap[y, x]; //Cell = value of tile at map position (y,x)

                    if (cell != null)//If cell exists; otherwise, continue (saves processing time)
                    {
                        Rectangle bounds = new Rectangle(x * size, y * size, size, size); //Sets where and what size the tile will be in the game
                        cell.DrawTile(spriteBatch, bounds);
                        //spriteBatch.Draw(tileSheet, bounds, sourceBounds, Color.White); //Draws the tile
                    }
                }
        }

    }
}
