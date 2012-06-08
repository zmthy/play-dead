using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Platformer.Camera
{
    class PanningDirector : CameraDirector
    {
        private ICameraTrackable target;
        private float travelTimeS;

        private Vector2 origin;
        private double elapsedTimeS;
        public float StartDelayS { get; set; }
        public float PauseDelayS { get; set; }
        public float ReturnTimeS { get; set; }

        private bool waiting;
        private bool returning;
        private bool completed;

        public PanningDirector(Camera2D camera, ICameraTrackable panTo, float travelTimeS)
            : base(camera)
        {
            target = panTo;
            this.travelTimeS = travelTimeS;
            ReturnTimeS = travelTimeS;

            origin = camera.Position;
            elapsedTimeS = 0;
            waiting = true;
            returning = false;
            completed = false;
        }

        public bool Waiting
        {
            get { return waiting; }
        }

        public bool Returning
        {
            get { return returning; }
        }

        public bool Completed
        {
            get { return completed; }
        }


        public override void update(GameTime gameTime)
        {
            if (completed)
                return;

            elapsedTimeS += gameTime.ElapsedGameTime.TotalSeconds;

            if (waiting)
            {
                if (elapsedTimeS > StartDelayS)
                {
                    waiting = false;
                    elapsedTimeS = 0;
                }
            }
            else
            {
                double angle = Math.Atan2(target.getPosition().Y - origin.Y, target.getPosition().X - origin.X);
                double totalDistance = Vector2.Distance(origin, target.getPosition());

                if (!returning)
                {
                    if (elapsedTimeS < travelTimeS)
                    {
                        double currentDistance = totalDistance * (elapsedTimeS / travelTimeS);
                        Camera.X = origin.X + (float)(Math.Cos(angle) * currentDistance);
                        Camera.Y = origin.Y + (float)(Math.Sin(angle) * currentDistance);
                    }
                    else if (elapsedTimeS > travelTimeS + PauseDelayS)
                    {
                        returning = true;
                        elapsedTimeS = 0;
                    }
                }
                else
                {
                    if (elapsedTimeS < ReturnTimeS)
                    {
                        double currentDistance = totalDistance * (elapsedTimeS / ReturnTimeS);
                        currentDistance = totalDistance - currentDistance;
                        Camera.X = origin.X + (float)(Math.Cos(angle) * currentDistance);
                        Camera.Y = origin.Y + (float)(Math.Sin(angle) * currentDistance);
                    }
                    else
                    {
                        completed = true;
                    }
                }
            }
        }
    }
}
