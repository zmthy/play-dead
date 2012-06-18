using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Tiles
{
    interface IActivatable
    {
        bool IsActive();

        void SetState(bool active);

        void ChangeState();

        void Draw(SpriteBatch spriteBatch);

        void Update(GameTime gameTime);
    }
}
