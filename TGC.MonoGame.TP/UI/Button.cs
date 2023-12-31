using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace TGC.MonoGame.TP.UI
{   
    public class Button
    {
        private Rectangle NormalSize;
        private Rectangle PressedSize;
        private Texture2D Texture;
        private bool Pressed;
        private SoundEffect SoundEffect;
        private float Scale;
        public Vector2 Position;
        
        public Button(Texture2D texture, Vector2 position, float scale,SoundEffect soundEffect = null)
        {
            this.Scale = scale;
            this.Texture = texture;
            this.Pressed = false;
            this.SoundEffect = soundEffect;
            ChangePosition(position);
        }
        
        public bool IsPressed(MouseState prev, MouseState cur)
        {
            var mouseClickRect = new Rectangle(cur.X, cur.Y, 10, 10);
            Pressed = cur.LeftButton == ButtonState.Pressed && mouseClickRect.Intersects(NormalSize);
            var state = prev.LeftButton == ButtonState.Pressed && cur.LeftButton == ButtonState.Released && mouseClickRect.Intersects(NormalSize);
            if(state && SoundEffect!= null)
                SoundEffect.Play(); 

            return state;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Pressed?PressedSize:NormalSize, Color.White);
        }

        public void ChangePosition(Vector2 position)
        {
            float scaleFactor = 0.9f; 
            int originalWidth = (int)(Texture.Width * Scale);
            int originalHeight = (int)(Texture.Height * Scale);

            int reducedWidth = (int)(originalWidth * scaleFactor);
            int reducedHeight = (int)(originalHeight * scaleFactor);

            int widthDifference = originalWidth - reducedWidth;
            int heightDifference = originalHeight - reducedHeight;

            this.NormalSize = new Rectangle((int)position.X, (int)position.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            this.PressedSize =new Rectangle((int)position.X+widthDifference/2, (int)position.Y+heightDifference/2, (int)(Texture.Width * (Scale*scaleFactor)), (int)(Texture.Height * (Scale*scaleFactor)));
            this.Position = position;
        }
    }
}