using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using BepuPhysics;
using BepuPhysics.Collidables;
using NumericVector3 = System.Numerics.Vector3;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.MapObjects
{
    public class StaticObstacle
    {
        public Simulation Simulation {get;set;}
        public Matrix World {get;set;}
        public Camera Camera {get;set;}
        public BoxPrimitive GeometricPrimitive {get;set;}
        public StaticHandle StaticHandle {get;set;}      
        public BoundingBox BoundingBox {get;set;}

        public StaticObstacle (Matrix world, BoxPrimitive geometricPrimitive, Simulation simulation, Camera camera)
        {
            this.Simulation = simulation;
            this.World = world;
            this.Camera = camera;
            this.GeometricPrimitive = geometricPrimitive;
            this.BoundingBox = BoundingVolumesExtensions.FromMatrix(world);
            loadObstacle();
        }

        public void Render(Effect effect, GameTime gameTime)
        {
            Utils.SetEffect(Camera,effect,World);
            GeometricPrimitive.Draw(effect);
        }

        private void loadObstacle()
        {
            Vector3 scale, translation;
            Quaternion rotation; 
            this.World.Decompose(out scale, out rotation , out translation);
            StaticHandle = this.Simulation.Statics.Add(new StaticDescription(new NumericVector3(translation.X, translation.Y, translation.Z),new System.Numerics.Quaternion(rotation.X,
                rotation.Y, rotation.Z, rotation.W),
            this.Simulation.Shapes.Add(new Box(scale.X, scale.Y, scale.Z))));   
        }
    }
}