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

        public void Render(Effect effect, Camera camera, GameTime gameTime)
        {
            Utils.SetEffect(camera,effect,World);
            GeometricPrimitive.Draw(effect);
        }

        private void loadObstacle()
        {
            
            Vector3 scale, translation;
            Quaternion rotation; 
            this.World.Decompose(out scale, out rotation , out translation);
            //new CollidableDescription(Simulation.Shapes.Add(new Cylinder(scale.X/2.05f ,scale.Y/1.005f)), 0.1f, ContinuousDetection.Continuous(1e-4f, 1e-4f));
            //, new BodyActivityDescription(-0.1f))
            var description = new StaticDescription(new NumericVector3(translation.X, translation.Y, translation.Z),new System.Numerics.Quaternion(rotation.X,
                rotation.Y, rotation.Z, rotation.W),
            this.Simulation.Shapes.Add(new Box(scale.X, scale.Y, scale.Z)),ContinuousDetection.Continuous(0.1f, 0.1f));
           
            
            StaticHandle = this.Simulation.Statics.Add(description);   
        }
    }
}