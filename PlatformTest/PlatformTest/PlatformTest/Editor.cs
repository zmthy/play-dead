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
    class Editor
    {
        const int size = 32; //The size of each tile in pixels (this will be later stored in an XML loading file)
        int height; //in tiles
        int width; //in tiles

        int currentTile = 0; //The current tile being edited by the user. Can be changed by holding the spacebar while clicking a tile in the editor panel
        Vector2 editorPanel = new Vector2(19, 4); //The location of the editor panel (in tiles) NOTE: NOT the location of the window surrounding the tilesheet
        bool drag = false; //Whether the user is dragging the editor panel or not

        public Editor(GraphicsDevice graphics)
        {
            height = graphics.DisplayMode.Height / size; //Also temporary, this will later be changed to incorporate the map being edited
            width = graphics.DisplayMode.Width / size;
        }

        public void DrawEditor(SpriteBatch spriteBatch, Texture2D crosshair, Texture2D gridCell, Texture2D tileSheet, Texture2D windowColor)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    spriteBatch.Draw(gridCell, new Vector2(x * size, y * size), Color.White); //Draws a cell in the editor grid, does NOT draw the map
                }
            }

            MouseState mouseState = Mouse.GetState();
            Vector2 cursorLoc = new Vector2((mouseState.X / size) * size - 2, (mouseState.Y / size) * size - 2); //Gets the PRECISE cursor location

            spriteBatch.Draw(windowColor, new Rectangle((int)editorPanel.X * size - 10, //Draws the blue window that surrounds the editor panel
                (int)editorPanel.Y * size - size,
                tileSheet.Width + 30,
                tileSheet.Height + 40),
                Color.White);
            spriteBatch.Draw(tileSheet, new Vector2((int)editorPanel.X * size, //Draws the tilesheet, a.k.a. editor panel
                (int)editorPanel.Y * size),
                Color.White);

            spriteBatch.Draw(crosshair, cursorLoc, Color.White); //Draws the crosshair on the currently selected tile
        }

        public void Click(Vector2 cursorLoc, Map tileMap, Texture2D tileSheet)
        {
            KeyboardState keyState = Keyboard.GetState();
            int x = (int)(cursorLoc.X + 2) / size; //Why the +2? Because cursorLoc is modified by -2. Duh.
            int y = (int)(cursorLoc.Y + 2) / size; //Same goes for this.
            int tsWidth = tileSheet.Width / size; //The width of the tilesheet in tiles

            if (drag)
                editorPanel = new Vector2(x, y); //If dragging the editor panel, set its location to cursor location
            else
            {
                if ((x >= (int)editorPanel.X && x <= (int)editorPanel.X + tsWidth) && //Uhhmm...well basically this is checking if the crosshair
                    (y >= (int)editorPanel.Y && y <= (int)editorPanel.Y + tileSheet.Height / size) && //is within the editor panel, and the
                    keyState.IsKeyDown(Keys.Space))                                                   //spacebar is pressed
                {
                    currentTile = ((y - (int)editorPanel.Y) * tsWidth) + (x - (int)editorPanel.X); //Sets the current tile as a function of its location on the tilesheet (focus if you want to understand this one)
                }
                else if ((x >= (int)editorPanel.X && x <= (int)editorPanel.X + tsWidth) &&  //Same as the last one, only simpler. If the
                    (y == (int)editorPanel.Y - 1) && keyState.IsKeyDown(Keys.Space)) //crosshair is on the top of the editor window, allow user to drag
                {
                    drag = true;
                }
                else //User wants to change the tile
                {
                    tileMap.Set(y, x, currentTile);
                }
            }
        }

        public void Release() //Stops editor panel from moving after the user has released the left button.
        {
            drag = false;
        }

        public void Erase(Vector2 cursorLoc, Map tileMap) //Simplified version of the Click() method. Just erases the currently selected tile.
        {
            int x = (int)(cursorLoc.X + 2) / size;
            int y = (int)(cursorLoc.Y + 2) / size;

            tileMap.Set(y, x, -1);
        }
    }
}
