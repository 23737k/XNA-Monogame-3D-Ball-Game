using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using BepuPhysics;
using BepuPhysics.Collidables;
using NumericVector3 = System.Numerics.Vector3;
using System.Linq;


namespace TGC.MonoGame.TP.MapObjects
{
    public class MovingObstacle
    {
        public Simulation Simulation {get;set;}
        public Matrix World {get;set;}
        public Camera Camera {get;set;}
        public Model Cylinder {get;set;}
        public BodyHandle BodyHandle {get;set;}
        public NumericVector3 AngularVelocity {get;set;}
        public NumericVector3 LinearVelocity {get;set;}
        public Matrix Scale {get;set;}


        public MovingObstacle (Matrix world, Model cylinder, Simulation simulation, Camera camera)
        {
            this.Simulation = simulation;
            this.World = world;
            this.Camera = camera;
            this.Cylinder = cylinder;
            this.AngularVelocity = NumericVector3.Zero;
            this.LinearVelocity = NumericVector3.Zero;

            World.Decompose(out var scale, out var rot, out var pos);
            this.Scale = Matrix.CreateScale(scale);
            loadObstacle();
        }

        public MovingObstacle (Matrix world, Model cylinder, Simulation simulation, Camera camera, Vector3 angularVelocity, Vector3 linearVelocity)
        {
            this.Simulation = simulation;
            this.World = world;
            this.Camera = camera;
            this.Cylinder = cylinder;
            this.AngularVelocity = new NumericVector3(angularVelocity.X,angularVelocity.Y,angularVelocity.Z);
            this.LinearVelocity = new NumericVector3(linearVelocity.X,linearVelocity.Y,linearVelocity.Z);
            loadObstacle();
        }

        public void Render(Effect effect, GameTime gameTime)
        {
            var bodyPosition = Simulation.Bodies.GetBodyReference(BodyHandle).Pose;
            World.Decompose(out var scale, out var rotation, out var pos);
            World = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(bodyPosition.Position);
            var world = Matrix.CreateScale(scale.X/5,scale.Y/5,scale.Z/5) * Matrix.CreateFromQuaternion(bodyPosition.Orientation) * Matrix.CreateTranslation(bodyPosition.Position);
            Utils.SetEffect(Camera,effect,world);
            Cylinder.Meshes.FirstOrDefault().Draw();
        }
        private void loadObstacle()
        {  
            World.Decompose(out var scale, out Quaternion rot, out var pos); 
            NumericVector3 position = new NumericVector3(pos.X, pos.Y, pos.Z);
            System.Numerics.Quaternion rotation = new System.Numerics.Quaternion(rot.X, rot.Y, rot.Z, rot.W);
            BodyHandle = Simulation.Bodies.Add(BodyDescription.
                            CreateKinematic(new RigidPose(position,
                            rotation), 
            new CollidableDescription(Simulation.Shapes.Add(new Cylinder(scale.X/2.05f ,scale.Y/1.005f)), 0.1f, ContinuousDetection.Continuous(1e-4f, 1e-4f)), new BodyActivityDescription(-0.1f)));
            Simulation.Bodies.GetBodyReference(BodyHandle).Velocity.Angular = AngularVelocity;
            Simulation.Bodies.GetBodyReference(BodyHandle).Velocity.Linear = LinearVelocity;
        }
    }
}