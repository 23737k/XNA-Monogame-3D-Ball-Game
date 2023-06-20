using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Collisions;
namespace TGC.MonoGame.TP.Powerups 
{
    public class Powerup 
    {
        public readonly Vector3 Position;
        private BoundingBox BoundingBox {get;set;}
        private float StartTime {get;set;}
        private float Duration = 7;
        private bool Activated = false;
        public readonly float SpeedBoost = 0;
        public readonly float JumpBoost = 0;

        public Powerup(Vector3 position, float speedBoost, float jumpBoost)
        {
            this.Position = position;
            var world = Matrix.CreateScale(10,10,10) * Matrix.CreateTranslation(position);
            this.BoundingBox = BoundingVolumesExtensions.FromMatrix(world);
            this.SpeedBoost = speedBoost;
            this.JumpBoost = jumpBoost;
        }
    	
        public void Activate (GameTime gameTime)
        {
            Activated = true;
            StartTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);
        }

        public bool isActive(GameTime gameTime)
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
            var BoundingSphere = new BoundingSphere(position, 5f);
            if(BoundingBox.Intersects(BoundingSphere))
            {
                status = true;
                Activate(gameTime);
            }

            return status;
        }
        
    }
}