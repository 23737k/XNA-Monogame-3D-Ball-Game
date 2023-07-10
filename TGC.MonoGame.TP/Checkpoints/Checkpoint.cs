using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Checkpoints
{
    public class Checkpoint 
    {
        public Vector3 Position {get;set;}
        public BoundingBox BoundingBox {get;set;}
        public Checkpoint (Vector3 position)
        {
            this.Position = position;
            var world = Matrix.CreateScale(6,6,6) * Matrix.CreateTranslation(position);
            this.BoundingBox = BoundingVolumesExtensions.FromMatrix(world);
        }
        public bool IsWithinBounds(Vector3 position)
        {
            var BoundingSphere = new BoundingSphere(position, 5f);
            return BoundingBox.Intersects(BoundingSphere);
        }

    }
}