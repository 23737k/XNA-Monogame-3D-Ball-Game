using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using BepuPhysics;
using BepuPhysics.Collidables;
using NumericVector3 = System.Numerics.Vector3;


namespace TGC.MonoGame.TP.MapObjects
{
    public class MovingObstacle : Obstacle
    {
        public Simulation Simulation {get;set;}
        public Matrix World {get;set;}
        public Camera Camera {get;set;}
        public CylinderPrimitive CylinderPrimitive {get;set;}
        public BodyHandle BodyHandle {get;set;}


        public MovingObstacle (Matrix world, CylinderPrimitive cylinderPrimitive, Simulation simulation, Camera camera)
        {
            this.Simulation = simulation;
            this.World = world;
            this.Camera = camera;
            this.CylinderPrimitive = cylinderPrimitive;

            loadObstacle();
        }

        public void Render(Effect effect, GameTime gameTime)
        {
            var bodyPose = Simulation.Bodies.GetBodyReference(BodyHandle).Pose;

            World = Matrix.CreateFromQuaternion(bodyPose.Orientation) * Matrix.CreateTranslation(bodyPose.Position);
            var viewProjection = Camera.View * Camera.Projection;
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));
            effect.Parameters["WorldViewProjection"]?.SetValue(World * viewProjection);
            CylinderPrimitive.Draw(effect);
        }
        private void loadObstacle()
        {  
            World.Decompose(out var scale, out Quaternion rot, out var pos); 
            NumericVector3 position = new NumericVector3(pos.X, pos.Y, pos.Z);
            System.Numerics.Quaternion rotation = new System.Numerics.Quaternion(rot.X, rot.Y, rot.Z, rot.W);
            BodyHandle = Simulation.Bodies.Add(BodyDescription.
                            CreateKinematic(new RigidPose(position,
                            rotation), 
            new CollidableDescription(Simulation.Shapes.Add(new Cylinder(CylinderPrimitive.Diameter/2 ,CylinderPrimitive.Height)), 0.1f, ContinuousDetection.Continuous(1e-4f, 1e-4f)), new BodyActivityDescription(-0.1f)));
            Simulation.Bodies.GetBodyReference(BodyHandle).Velocity.Angular = new NumericVector3(0,3f,0);
        }
    }
}