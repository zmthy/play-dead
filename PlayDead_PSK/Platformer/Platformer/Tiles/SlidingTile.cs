using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Platformer.Levels;

namespace Platformer.Tiles
{
    class SlidingTile : MoveableTile
    {
        public const float MAX_WAIT_TIME_S = 0.2f;

        public float WaitTimeS
        {
            get { return waitTimeS; }
            protected set
            {
                waitTimeS = MathHelper.Clamp(value, 0, MAX_WAIT_TIME_S);
            }
        }
        private float waitTimeS;

        private bool evenReverse;

        public SlidingTile(Sprite sprite, TileCollision collision, Vector2 velocity)
            : base(sprite, collision, velocity) { }

        public override void Update(GameTime gameTime)
        {
            // Get the elapse time since the last frame
            float elapsedS = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SlidingTile leader = getLeader();
            if (Leader != this) // If we are following the leader
            {
                Velocity = Leader.Velocity;
                FrameVelocity = Leader.FrameVelocity;
                Sprite.Position = Sprite.Position + FrameVelocity;

                if (leader.WaitTimeS <= 0)
                {
                    Tile collidingTile = getCollidingTile();
                    if (collidingTile is MoveableTile)
                    {
                        // We have collided, notify the leader
                        leader.reverseDirection();
                    }
                    else if (collidingTile is Tile)
                    {
                        // We have collided, notify the leader
                        leader.reverseDirection(MAX_WAIT_TIME_S);
                    }
                }
            }
            else if (Leader == this) // If we are the leader
            {
                FrameVelocity = Vector2.Zero;

                if (WaitTimeS > 0)
                {
                    // Wait for some amount of time.
                    WaitTimeS = Math.Max(0.0f, WaitTimeS - elapsedS);
                }

                if (WaitTimeS <= 0)
                {
                    Tile collidingTile = getCollidingTile();

                    if (collidingTile is MoveableTile)
                    {
                        // We have collided, notify the leader
                        reverseDirection();
                    }
                    else if (collidingTile is Tile)
                    {
                        // We have collided, notify the leader
                        reverseDirection(MAX_WAIT_TIME_S);
                    }

                    // Move in the current direction.
                    FrameVelocity = new Vector2((float)(Math.Cos(Velocity.X) * Velocity.Y * elapsedS),
                                                (float)(Math.Sin(Velocity.X) * Velocity.Y * elapsedS));
                    Sprite.Position = Sprite.Position + FrameVelocity;
                }
            }
        }

        private void reverseDirection(float waitTimeS = 0)
        {
            WaitTimeS = waitTimeS;

            float newAngle = Velocity.X + ((evenReverse) ? (float)Math.PI : -(float)Math.PI);
            evenReverse = !evenReverse;

            Velocity = new Vector2(newAngle, Velocity.Y);
        }

        private SlidingTile getLeader()
        {
            SlidingTile leader = this;

            if (Leader is SlidingTile)
                leader = (SlidingTile)Leader;

            return leader;
        }
    }
}
