using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;
using System.Collections.Generic;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using TGC.MonoGame.TP.Physics.Bepu;
using TGC.MonoGame.TP.MapObjects;
using NumericVector3 = System.Numerics.Vector3;


namespace TGC.MonoGame.TP.MapObjects
{
    public class PeriodicObstacle : Obstacle
    {
        public Simulation Simulation {get;set;}
        public Matrix World {get;set;}
        public Camera Camera {get;set;}
        public CubePrimitive CubePrimitive {get;set;}
        public BodyHandle BodyHandle {get;set;}
        public float Amplitude {get;set;}
        public float Frecuency {get;set;}
        public float VerticalOffset {get;set;}
        public float HorizontallOffset {get;set;}
        public string Axis {get;set;}


        public PeriodicObstacle (Matrix world, CubePrimitive cubePrimitive, Simulation simulation, Camera camera,
                                float amplitude, float frecuency, float verticalOffset, float horizontalOffset, string axis)
        {
            this.Simulation = simulation;
            this.World = world;
            this.Camera = camera;
            this.CubePrimitive = cubePrimitive;
            this.Amplitude=amplitude;
            this.Frecuency=frecuency;
            this.VerticalOffset=verticalOffset;
            this.HorizontallOffset=horizontalOffset;
            this.Axis=axis;
            this.previousPosition = new NumericVector3(World.Translation.X, World.Translation.Y,World.Translation.Z); 
            loadObstacle();
        }
        private  NumericVector3 previousPosition;
        public void UpdateMovement(GameTime gameTime)
        {
            var time = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds); 
            var deltaTiempo = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var bodyRef = Simulation.Bodies.GetBodyReference(BodyHandle);
            
            NumericVector3 currentPosition;

              switch (Axis.ToLower())
            {
                case "x":
                    currentPosition =  new NumericVector3(MathF.Cos(time* Frecuency + HorizontallOffset) * Amplitude + VerticalOffset, previousPosition.Y, previousPosition.Z);
                    break;
                case "y":
                    currentPosition = new NumericVector3(previousPosition.X, MathF.Cos(time* Frecuency+ HorizontallOffset) * Amplitude + VerticalOffset, previousPosition.Z);
                    break;
                case "z":
                    currentPosition = new NumericVector3(previousPosition.X, previousPosition.Y, MathF.Cos(time* Frecuency + HorizontallOffset) * Amplitude + VerticalOffset);
                    break;
                default:
                    throw new ArgumentException("Eje no válido");
            }
            bodyRef.Pose.Position = currentPosition;
            bodyRef.Velocity.Linear = (currentPosition- previousPosition)* (1 / deltaTiempo);
            previousPosition = currentPosition;
        }
        
        public void Render(Effect effect, GameTime gameTime)
        {
            UpdateMovement(gameTime);
            var bodyPosition = Simulation.Bodies.GetBodyReference(BodyHandle).Pose.Position;
            World.Decompose(out var scale, out var rotation, out var pos);
        
            World = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(bodyPosition);
            var viewProjection = Camera.View * Camera.Projection;
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));
            effect.Parameters["WorldViewProjection"]?.SetValue(World * viewProjection);
            CubePrimitive.Draw(effect);
        }
        private void loadObstacle()
        {  
            World.Decompose(out var scale, out Quaternion rot, out var pos); 
            NumericVector3 position = new NumericVector3(pos.X, pos.Y, pos.Z);
            System.Numerics.Quaternion rotation = new System.Numerics.Quaternion(rot.X, rot.Y, rot.Z, rot.W);
            BodyHandle = Simulation.Bodies.Add(BodyDescription.
                            CreateKinematic(new RigidPose(position,
                            rotation), 
            new CollidableDescription(Simulation.Shapes.Add(new Box(scale.X, scale.Y, scale.Z)), 0.1f, ContinuousDetection.Continuous(1e-4f, 1e-4f)), new BodyActivityDescription(-0.1f)));
        }
    }
}