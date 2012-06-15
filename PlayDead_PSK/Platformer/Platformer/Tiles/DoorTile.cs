using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Platformer.Tiles
{
    class DoorTile : MoveableTile
    {
        public const float CLAMP_DISTANCE = 2;

        public bool Moving
        {
            get { return moving; }
        }
        private bool moving;

        public bool Open
        {
            get { return isOpen; }
            set
            {
                if (isOpen != value)
                {
                    isOpen = value;
                    moving = true;
                }
            }
        }
        public bool Closed
        {
            get { return !isOpen; }
            set
            {
                if (isOpen == value)
                {
                    isOpen = !value;
                    moving = true;
                }
            }
        }
        private bool isOpen;

        private Vector2 closedPosition;

        public DoorTile(Sprite sprite, TileCollision collision, float speed,
                            Level level)
            : base(sprite, collision, new Vector2(0, speed), level)
        {
            closedPosition = sprite.Position;
            Open = true;
        }

        public override void update(GameTime gameTime)
        {
            // Get the elapse time since the last frame
            float elapsedS = (float)gameTime.ElapsedGameTime.TotalSeconds;

            DoorTile leader = getLeader();
            
            if (Leader != this && moving) // If we are not the leader and we are moving
            {
                Vector2 openPosition = getLeader().Sprite.Position;
                float angleToOpenPosition = (float)Math.Atan2(openPosition.Y - closedPosition.Y,
                                                           openPosition.X - closedPosition.X);

                if (isOpen) // Door opening
                {
                    // Calculate the movement
                    float xMovement = (float)Math.Cos(angleToOpenPosition) * Velocity.Y * elapsedS;
                    float yMovement = (float)Math.Sin(angleToOpenPosition) * Velocity.Y * elapsedS;
                    Vector2 movementVelocity = new Vector2(xMovement, yMovement);

                    // Apply the movement
                    Sprite.Position = Sprite.Position + movementVelocity;

                    // Clamp to target and stop movement when open
                    if (Vector2.Distance(Sprite.Position, openPosition) < CLAMP_DISTANCE)
                    {
                        Sprite.Position = openPosition;
                        moving = false;
                    }
                }
                else // Door closing
                {
                    // Calculate the movement
                    float xMovement = -(float)Math.Cos(angleToOpenPosition) * Velocity.Y * elapsedS;
                    float yMovement = -(float)Math.Sin(angleToOpenPosition) * Velocity.Y * elapsedS;
                    Vector2 movementVelocity = new Vector2(xMovement, yMovement);

                    // Apply the movement
                    Sprite.Position = Sprite.Position + movementVelocity;

                    // Clamp to target and stop movement when closed
                    if (Vector2.Distance(Sprite.Position, closedPosition) < CLAMP_DISTANCE)
                    {
                        Sprite.Position = closedPosition;
                        moving = false;
                    }
                }
            }
        }

        private DoorTile getLeader()
        {
            DoorTile leader = this;

            if (Leader is DoorTile)
                leader = (DoorTile)Leader;

            return leader;
        }
    }
}
