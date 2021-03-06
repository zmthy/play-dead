﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace PlayDead
{
    public enum Direction
    {
        STANDING,
        UP,
        LEFT,
        RIGHT
    }

    public enum Facing
    {
        LEFT,
        RIGHT
    }

    class Character
    {
        Texture2D texture;                                 //Character's texture file
        Rectangle bounds = new Rectangle(57, 337, 40, 48); //Collision box (in starting position)
        Rectangle clip = new Rectangle(0, 0, 40, 48);      //Determines which frame is showing

        Direction direction = Direction.STANDING;   //Direction character is moving
        Facing facing = Facing.RIGHT;               //Direction character is facing

        int frameCount = 0;     //Used to determine when to move to the next frame
        const int delay = 4;    //Delay between animation frames (less->faster animation speed)

        float speed = 0f;               //Current velocity of character. Ranges from -maxSpeed (moving left) to +maxSpeed (moving right)
        const float maxspeed = 12f;     //Max character speed

        int startY = 0;                 //Starting Y-value for jumps
        int jumpCount = 0;              //Used to determine animation for jumps

        public Character(Texture2D texture)
        {
            this.texture = texture;
        }

        /// <summary>
        /// The KeyInput method handles all keyboard input for the character.
        /// </summary>
        public void KeyInput()
        {
            KeyboardState keyState = Keyboard.GetState();

            //If we're not jumping handle things normally
            if (direction != Direction.UP)
            {
                if (keyState.IsKeyDown(Keys.D))
                {
                    direction = Direction.RIGHT;
                    facing = Facing.RIGHT;
                }
                if (keyState.IsKeyDown(Keys.A))
                {
                    direction = Direction.LEFT;
                    facing = Facing.LEFT;
                }
                if (keyState.IsKeyDown(Keys.W))
                {
                    direction = Direction.UP;
                }
                if (keyState.IsKeyUp(Keys.D) && direction == Direction.RIGHT)
                {
                    direction = Direction.STANDING;
                    facing = Facing.RIGHT;
                }
                if (keyState.IsKeyUp(Keys.A) && direction == Direction.LEFT)
                {
                    direction = Direction.STANDING;
                    facing = Facing.LEFT;
                }
            }
            //Otherwise, speed is greatly reduced to allow *some* manouvering
            else
            {
                if (keyState.IsKeyDown(Keys.D))
                {
                    if (speed < maxspeed)
                        speed += .2f;
                    facing = Facing.RIGHT;
                }
                if (keyState.IsKeyDown(Keys.A))
                {
                    if (speed > -maxspeed)
                        speed -= .2f;
                    facing = Facing.LEFT;
                }
            }
        }

        bool debug = false;  //I forgot why this is here...

        /// <summary>
        /// Handles the actual movement of the character, and animation thereof
        /// </summary>
        public void Move()
        {
            if (direction != Direction.UP && !CheckCollision(bounds) && debug)
            {
                //fall
                startY = bounds.Y + 99;
                direction = Direction.UP;
                jumpCount = 11;
            }

            //Only change sprite frames every DELAY frames. Otherwise, it runs too quickly
            if (frameCount % delay == 0)
            {
                switch (direction)
                {
                    case Direction.STANDING:
                        SlowDown(2);
                        bounds.X += (int)speed;
                        //There are only 4 frames in the idle animation, so reset after 4
                        if (frameCount / delay >= 4)
                            frameCount = 0;
                        //Idle animation is at y=0
                        clip = new Rectangle(frameCount / delay * 40, 0, 40, 48);
                        break;
                    case Direction.LEFT:
                        if (speed > -maxspeed)
                            speed -= 1;
                        bounds.X += (int)speed;
                        //8 frames in the running animation
                        if (frameCount / delay >= 8)
                            frameCount = 0;
                        //Running animation is at y=48
                        clip = new Rectangle(frameCount / delay * 40, 48, 40, 48);
                        break;
                    case Direction.RIGHT:
                        if (speed < maxspeed)
                            speed += 1;
                        bounds.X += (int)speed;
                        //8 frames in the running animation
                        if (frameCount / delay >= 8)
                            frameCount = 0;
                        //Running animation is at y=48
                        clip = new Rectangle(frameCount / delay * 40, 48, 40, 48);
                        break;
                    case Direction.UP:
                        //This might get a little confusing...
                        //Currently uses the Y value of srcBounds to tell what state the animation is in
                        debug = true;

                        //If the character is moving slow enough, switch to the standing-jump animation
                        //Otherwise, just use the normal running animation
                        if (speed > -4 && speed < 4)
                            clip.Y = 96;
                        else
                            clip.Y = 48;

                        //If we're currently in the idle animation or the standing-jump animation
                        if (clip.Y == 0 || clip.Y == 96)
                        {
                            /*
                             *  Different parts of the jump use different animation frames, and
                             *  repeat a different set of those frames.
                             */ 
                            if (jumpCount < 2) 
                            {
                                if (frameCount / delay >= 9)
                                    frameCount = 0;
                            }
                            else if (jumpCount > 2 && jumpCount <= 10) 
                            {
                                if (frameCount / delay > 3)
                                    frameCount = 2 * delay;
                            }
                            else if (jumpCount > 10 && jumpCount <= 18)
                            {
                                if (frameCount / delay > 5)
                                    frameCount = 4 * delay;
                            }
                            else if (jumpCount > 18 && CheckCollision(new Rectangle(bounds.X, bounds.Y + Map.size, bounds.Width, bounds.Height)))
                            {
                                if (frameCount / delay >= 9)
                                    frameCount = 0;
                            }
                            else if (jumpCount > 18) //if he's falling a long time, don't loop the animation where he hits the ground
                                frameCount = 4 * delay;

                            clip = new Rectangle(frameCount / delay * 40, 96, 40, 48);
                        }
                        //Otherwise, we're doing a running jump
                        else if (clip.Y == 48)
                        {
                            if (frameCount / delay >= 8)
                                frameCount = 0;
                            if (jumpCount <= 10)
                                clip = new Rectangle((frameCount / delay) / 2 * 40, 48, 40, 48);
                            else
                                clip = new Rectangle(frameCount / delay * 40, 48, 40, 48);
                        }
                        if (jumpCount == 0)
                            startY = bounds.Y;

                        //Sets function for determining jump y-position
                        float jump_positionY = (jumpCount - 10) * (jumpCount - 10) - 100 + startY;
                        //Sets terminal velocity
                        float max_velocityY = 2 * (20 - 10); //derivative of jump_positionY where 20 is max jumpCount value
                        float max_positionY = startY + max_velocityY * (jumpCount - 20);

                        if (jumpCount >= 20)
                            jump_positionY = max_positionY;

                        bounds = new Rectangle(bounds.X + (int)speed,
                            (int)jump_positionY, 40, 48);
                        jumpCount++;
                        break;
                }
            }

            //Check collision
            if (jumpCount >= 10 && CheckCollision(bounds))
            {
                //Stop
                direction = Direction.STANDING;
                jumpCount = 0;
            }

            frameCount++;
        }

        //Checks collision
        private bool CheckCollision(Rectangle bounds)
        {
            int cutoff = bounds.Y + Map.size;

            //Create a rectangle for the current block, if it is not -1
            Rectangle tempBounds = new Rectangle(bounds.X, cutoff, bounds.Width, cutoff - bounds.Y);
            tempBounds.Y += tempBounds.Height - Map.size;

            int x = (tempBounds.X + bounds.Width / 2) / Map.size;
            int y = tempBounds.Y / Map.size + 1;
            Tile type = Map.getType(y, x);

            if (type != null)
            {
                //Test to see if the character collides
                Rectangle tileBounds = new Rectangle(x * Map.size, y * Map.size, Map.size, Map.size);
                if (tempBounds.Intersects(tileBounds))
                {
                    this.bounds = new Rectangle(bounds.X, tileBounds.Y - bounds.Height, bounds.Width, bounds.Height);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gradually slows the character down if the character stops running
        /// </summary>
        /// <param name="i">How much slowdown to apply. Larger numbers stop the character instantly.</param>
        private void SlowDown(int i)
        {
            for (int k = 0; k < i; k++)
            {
                if (speed < 0f)
                    speed += 1;
                else if (speed > 0f)
                    speed -= 1;
            }
        }

        /// <summary>
        /// Draws the character onto the screen
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to use to render this sprite</param>
        /// <param name="texture">The character's texture sheet (this should really be local to the class...)</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            //If the character is facing left, we just flip the texture using SpriteEffects
            if (facing == Facing.RIGHT)
                spriteBatch.Draw(texture, bounds, clip, Color.White);
            else
                spriteBatch.Draw(texture, bounds, clip, Color.White,
                    0f, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0f);
        }
    }
}
