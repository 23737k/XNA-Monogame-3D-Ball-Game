using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.UI
{   
    public class Button
    {
        private Rectangle NormalSize;
        private Rectangle PressedSize;
        private Vector2 Position;
        private Texture2D Texture;
        private bool Pressed;
        
        public Button(Texture2D texture, Vector2 position, float scale)
        {
            float scaleFactor = 0.9f; 
            int originalWidth = (int)(texture.Width * scale);
            int originalHeight = (int)(texture.Height * scale);

            int reducedWidth = (int)(originalWidth * scaleFactor);
            int reducedHeight = (int)(originalHeight * scaleFactor);

            int widthDifference = originalWidth - reducedWidth;
            int heightDifference = originalHeight - reducedHeight;

            this.NormalSize = new Rectangle((int)position.X, (int)position.Y, (int)(texture.Width * scale), (int)(texture.Height * scale));
            this.PressedSize =new Rectangle((int)position.X+widthDifference/2, (int)position.Y+heightDifference/2, (int)(texture.Width * (scale*scaleFactor)), (int)(texture.Height * (scale*scaleFactor)));
            this.Texture = texture;
            this.Position = position;
            this.Pressed = false;
        }
        
        public bool IsPressed(MouseState prev, MouseState cur)
        {
            var mouseClickRect = new Rectangle(cur.X, cur.Y, 10, 10);
            Pressed = cur.LeftButton == ButtonState.Pressed && mouseClickRect.Intersects(NormalSize);
            return prev.LeftButton == ButtonState.Pressed && cur.LeftButton == ButtonState.Released && mouseClickRect.Intersects(NormalSize);
        }

        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Pressed?PressedSize:NormalSize, Color.White);
        }
    }
}