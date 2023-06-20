using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Collisions;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using System.Linq;
namespace TGC.MonoGame.TP.Powerups 
{
    public class Powerup 
    {
        public readonly Vector3 Position;
        private BoundingBox BoundingBox {get;set;}
        private Model Model {get;set;}
        private Texture2D Texture {get;set;}
        private Texture2D Normal {get;set;}
        private Camera Camera {get;set;}
        private float StartTime {get;set;}
        private float Duration = 7;
        private bool Activated = false;
        public readonly float SpeedBoost = 0;
        public readonly float JumpBoost = 0;

        public Powerup(Vector3 position, float speedBoost, float jumpBoost, Model model, Texture2D texture, Texture2D normal, Camera camera)
        {
            this.Position = position;
            var world = Matrix.CreateScale(10,10,10) * Matrix.CreateTranslation(position);
            this.BoundingBox = BoundingVolumesExtensions.FromMatrix(world);
            this.SpeedBoost = speedBoost;
            this.JumpBoost = jumpBoost;
            this.Model= model;
            this.Texture = texture;
            this.Normal = normal;
            this.Camera = camera;
        }
    	
        public void Activate (GameTime gameTime)
        {
            Activated = true;
            StartTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);
        }

        public bool IsActive(GameTime gameTime)
        {
            var status = false;
            if(Activated)
            {
                var currentTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);
                status = (currentTime - StartTime) < Duration;
            }
            return status;
        }
        public bool IsWithinBounds(Vector3 position,GameTime gameTime)
        {
            var status = false;
            var BoundingSphere = new BoundingSphere(position, 2.5f);
            if(BoundingBox.Intersects(BoundingSphere))
            {
                status = true;
                Activate(gameTime);
            }

            return status;
        }

        public void Render (Effect effect,GameTime gameTime)
        {
            var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);
            var world = Matrix.CreateScale(2,0.2f,2) * Matrix.CreateFromYawPitchRoll(time*2,0,MathF.PI/2)* Matrix.CreateTranslation(Position);
            effect.Parameters["ModelTexture"].SetValue(Texture);
            effect.Parameters["NormalTexture"].SetValue(Normal);
            Utils.SetEffect(Camera,effect,world);
            Model.Meshes.FirstOrDefault().Draw();
        }
        
    }
}