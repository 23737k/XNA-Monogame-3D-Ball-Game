using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using BepuPhysics;
using BepuPhysics.Collidables;
using NumericVector3 = System.Numerics.Vector3;

namespace TGC.MonoGame.TP.MapObjects
{
    public class StaticObstacle : Obstacle
    {
        public Simulation Simulation {get;set;}
        public Matrix World {get;set;}
        public Camera Camera {get;set;}
        public BoxPrimitive GeometricPrimitive {get;set;}
        public StaticHandle StaticHandle {get;set;}


        public StaticObstacle (Matrix world, BoxPrimitive geometricPrimitive, Simulation simulation, Camera camera)
        {
            this.Simulation = simulation;
            this.World = world;
            this.Camera = camera;
            this.GeometricPrimitive = geometricPrimitive;
            loadObstacle();
        }

        public void Render(Effect effect, GameTime gameTime)
        {
            var viewProjection = Camera.View * Camera.Projection;
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));
            effect.Parameters["WorldViewProjection"]?.SetValue(World * viewProjection);
            GeometricPrimitive.Draw(effect);
        }

        private void loadObstacle()
        {
            Vector3 scale, translation;
            Quaternion rotation; 
            this.World.Decompose(out scale, out rotation , out translation);
            StaticHandle = this.Simulation.Statics.Add(new StaticDescription(new NumericVector3(translation.X, translation.Y, translation.Z),
            this.Simulation.Shapes.Add(new Box(scale.X, scale.Y, scale.Z))));   
        }
    }
}