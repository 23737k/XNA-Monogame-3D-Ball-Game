using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using System.Collections.Generic;
using BepuPhysics;
using TGC.MonoGame.TP.MapObjects;

namespace TGC.MonoGame.TP
{
    public class Loader {

        public Loader (Simulation simulation, GraphicsDevice graphicsDevice, Camera camera)
        {
            this.Simulation = simulation;
            this.GraphicsDevice = graphicsDevice;
            this.Camera = camera;
        }
        public Simulation Simulation {get;set;}
        public GraphicsDevice GraphicsDevice {get;set;}
        public Camera Camera {get;set;}
        
        public List<Obstacle> LoadKinematics()
        {
            var Obstacles = new List<Obstacle>();
            var BasicCylindersMeasures = new Vector3[]
            {
                new Vector3(10f,80f,60f),
                new Vector3(10f,80f,60f),
                new Vector3(10f,80f,60f),
                new Vector3(30, 10, 60),
                new Vector3(30, 10, 60),
                new Vector3(30, 10, 60),

                //Islas
                new Vector3(10,20,18),
                new Vector3(10,20,18),
                new Vector3(10,20,18),
                new Vector3(10,20,18),
                new Vector3(10,20,18),
                new Vector3(10,20,18),
                new Vector3(10,20,18),

                //Cilindros que giran
                new Vector3(80, 10, 18),
                new Vector3(80, 10, 18),
                new Vector3(80, 10, 18),
                new Vector3(80, 10, 18),
            };

            var CylinderWorld = new Matrix[]{
                Matrix.CreateTranslation(new Vector3(300,0f,4.5f)),
                Matrix.CreateTranslation(new Vector3(400, 0f,4.5f)),
                Matrix.CreateTranslation(new Vector3(500, 0, 4.5f)),
                Matrix.CreateTranslation(new Vector3(400, 10, 4.5f )),
                Matrix.CreateTranslation(new Vector3(500, 10, 4.5f )),
                Matrix.CreateTranslation(new Vector3(300, 10, 4.5f )),

                //Islas
                Matrix.CreateTranslation(new Vector3(1850,20,404)),
                Matrix.CreateTranslation(new Vector3(1890,20,434.5f)),
                Matrix.CreateTranslation(new Vector3(1930,20,454.5f)),
                Matrix.CreateTranslation(new Vector3(1980,20,454.5f)),
                Matrix.CreateTranslation(new Vector3(2030,20,434.5f)),
                Matrix.CreateTranslation(new Vector3(2080,20,404.5f)),
                Matrix.CreateTranslation(new Vector3(2130,20,414.5f)),

                //Cilindros que Giran  
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3100,94,1775)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3050,94,1845)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3100,94,1915)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3050,94,1985))
            };
            for(int i =0; i< CylinderWorld.Length; i++){
               Obstacles.Add(new MovingObstacle(CylinderWorld[i],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y),Simulation,Camera));
            }

            return Obstacles;
        }


        public List<Obstacle> LoadPeriodics()
        {
            var Obstacles = new List<Obstacle>();

            //Plataforma que baja
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(20,10,20) * Matrix.CreateTranslation(1720,42.5f,2405),new CubePrimitive(GraphicsDevice),Simulation,
                                Camera,42.5f,3f,42.5f,0f, "Y"));

            //Pared que aplastan contra el suelo
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4240),new CubePrimitive(GraphicsDevice),Simulation,
                    Camera,35f,4f,45,0f, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4430),new CubePrimitive(GraphicsDevice),Simulation,
                    Camera,35f,4f,45,+MathHelper.PiOver4, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4620),new CubePrimitive(GraphicsDevice),Simulation,
                    Camera,35f,4f,45,MathHelper.PiOver4, "Y"));
            //Plataformas que suben y bajan
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(50,10,50) * Matrix.CreateTranslation(600f,-60,4.5f),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,-70f,3f,-60f,0f, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(50,10,50) * Matrix.CreateTranslation(700f,60,4.5f),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,70f,3f,60f,0f, "Y"));
            
            //Pisos que se mueven
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(1740,0,5230),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,200f,2f,5230f,0f, "Z"));
                Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(1690,0,5600),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,200f,2f,5600f,0f, "Z"));
                Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(1740,0,6010),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,200f,2f,6010f,0f, "Z"));
            
            //Paredes que se mueven
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,2800),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,40f,5f,1720f,0f, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,2900),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,-40f,5f,1720f,MathF.PI/4, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,3000),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,40f,5f,1720f,MathF.PI/2f, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,3100),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,-40f,5f,1720f,MathHelper.PiOver4*3f, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,3200),new CubePrimitive(GraphicsDevice),Simulation,
                Camera,40f,5f,1720f,MathF.PI, "X"));

            return Obstacles;
        }
        public List<Obstacle> LoadStatics()
        {
            var Obstacles = new List<Obstacle>();
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

                //muros
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3320)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3460)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1700,25,3895)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1740, 25, 4020)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7825))
            };

            
            foreach(Matrix world in staticsWorld){
               Obstacles.Add(new StaticObstacle(world,new CubePrimitive(GraphicsDevice),Simulation,Camera));
            }

            return Obstacles;

        }
    }
}