using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.MapObjects
{
    public interface Obstacle
    {
        void Render(Effect effect, GameTime gameT);
    }
}