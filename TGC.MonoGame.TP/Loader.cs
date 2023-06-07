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

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Handles all of the aspects of working with a SkyBox.
    /// </summary>
    public class Loader {

        public Loader (Simulation simulation, GraphicsDevice graphicsDevice, Camera camera)
        {
            this.Simulation = simulation;
            this.Obstacles = new List<Obstacle>();
            this.GraphicsDevice = graphicsDevice;
            this.Camera = camera;
        }
        public Simulation Simulation {get;set;}
        public List<Obstacle> Obstacles {get;set;} 
        public GraphicsDevice GraphicsDevice {get;set;}
        public Camera Camera {get;set;}
        
        public List<BodyHandle> LoadKinematics()
        {
            return null;
        }

        public List<Obstacle> LoadStatics()
        {
            var staticsWorld = new List<Matrix> 
            {
                //Ground

                Matrix.CreateScale(150, 10, 100) * Matrix.CreateTranslation(Vector3.Zero),
                Matrix.CreateScale(150,10f,40) * Matrix.CreateTranslation(new Vector3(150, 0f, 2f)),
                Matrix.CreateScale(140,10f,380) * Matrix.CreateTranslation(new Vector3(800f,100f,245f)),
                Matrix.CreateScale(50,10f,50) * Matrix.CreateTranslation(new Vector3(950,60,415f)),
                Matrix.CreateScale(50,10f,50) * Matrix.CreateTranslation(new Vector3(1090,20,415f)),
                Matrix.CreateScale(150,10f,30) * Matrix.CreateTranslation(new Vector3(1235f,20f,415f)),
                Matrix.CreateScale(150,10f,80) * Matrix.CreateTranslation(new Vector3(1405,20f,435f)),
                Matrix.CreateScale(300,10f,80) * Matrix.CreateTranslation(new Vector3(1670f,20f,435f)),
                Matrix.CreateScale(100,10f,80) * Matrix.CreateTranslation(new Vector3(2230f,20f,435f)),
                Matrix.CreateScale(350,10f,80) * Matrix.CreateTranslation(new Vector3(2475f,20f,435f)),
                Matrix.CreateScale(310,10f,80) * Matrix.CreateTranslation(new Vector3(2895f,20f,435f)),
                Matrix.CreateScale(80,10f,400) * Matrix.CreateTranslation(new Vector3(3070f,20f,595f)),
                Matrix.CreateScale(80,10f,100) * Matrix.CreateTranslation(new Vector3(3070f,30,865f)),
                Matrix.CreateScale(80,10f,100) * Matrix.CreateTranslation(new Vector3(3070f,40,1005f)),
                Matrix.CreateScale(80,10f,100) * Matrix.CreateTranslation(new Vector3(3070f,55,1145f)),
                Matrix.CreateScale(80,10f,100) * Matrix.CreateTranslation(new Vector3(3070f,70,1285f)),
                Matrix.CreateScale(80,10f,100) * Matrix.CreateTranslation(new Vector3(3070f,85,1425f)),
                Matrix.CreateScale(80,10f,600) * Matrix.CreateTranslation(new Vector3(3070f,85,1825f)),
                Matrix.CreateScale(80,10f,200) * Matrix.CreateTranslation(new Vector3(3070f,85,2245)),
                Matrix.CreateScale(600,10f,120) * Matrix.CreateTranslation(new Vector3(2810,85,2405)),
                Matrix.CreateScale(800,10f,40) * Matrix.CreateTranslation(new Vector3(2110,85,2365)),
                Matrix.CreateScale(800,10f,40) * Matrix.CreateTranslation(new Vector3(2110,85,2445)),
                Matrix.CreateScale(80,10f,1500) * Matrix.CreateTranslation(new Vector3(1720,0,3175)),
                Matrix.CreateScale(80,10,1000) * Matrix.CreateTranslation(new Vector3(1720,0,4455)),
                Matrix.CreateScale(100,10f,2600) * Matrix.CreateTranslation(new Vector3(1720,0,7620)),
                Matrix.CreateScale(100,10f,150) * Matrix.CreateTranslation(new Vector3(1720,0,8995)),

                //Walls

                Matrix.CreateScale(10,10f,30) * Matrix.CreateTranslation(new Vector3(1360f,30f,410f)),
                Matrix.CreateScale(10,10f,30f) * Matrix.CreateTranslation(new Vector3(1565f,30f,435f)),
                Matrix.CreateScale(10,10f, 80f) * Matrix.CreateTranslation(new Vector3(2565f,30f,435f)),
                //Muro alto
                Matrix.CreateScale(40, 40f,10f) * Matrix.CreateTranslation(new Vector3(3090f,45f,510f)),
                Matrix.CreateScale(40, 40f,10f) * Matrix.CreateTranslation(new Vector3(3080f,45f,670f)),
                Matrix.CreateScale(40, 40f,10f) * Matrix.CreateTranslation(new Vector3(3060f,45f,590f)),
                //Paredes que se mueven
                Matrix.CreateScale(10, 50f,40f) * Matrix.CreateTranslation(new Vector3(40*MathF.Cos(5*1)+1720,40,2800)),
                Matrix.CreateScale(10, 50f,40f) * Matrix.CreateTranslation(new Vector3(-40*MathF.Cos(5*1)+1720,40,2900)),
                Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(new Vector3(40*MathF.Cos(5*1)+1720,40,3000)),
                Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(new Vector3(-40*MathF.Cos(5*1)+1720,40,3100)),
                Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(new Vector3(40*MathF.Cos(5*1)+1720,40,3200)),

                //muros
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3320)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3460)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1700,25,3895)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1740, 25, 4020)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7825)),

                //Plataformas que se mueven
                //Se tendrían que mover con time
                Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1740,0,200*MathF.Cos(2*1)+5230)),
                Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1690,0,200*MathF.Cos(2*1+MathHelper.Pi)+5600)),
                Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1740,0,200*MathF.Cos(2*1)+6010)),
            };

            
            foreach(Matrix world in staticsWorld){
               Obstacles.Add(new StaticObstacle(world,new CubePrimitive(GraphicsDevice),Simulation,Camera));
            }

            return Obstacles;

        }
    }
}