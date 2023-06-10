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
                Matrix.CreateTranslation(new Vector3(1950,20,404)),
                Matrix.CreateTranslation(new Vector3(1890,20,434.5f)),
                Matrix.CreateTranslation(new Vector3(1930,20,454.5f)),
                Matrix.CreateTranslation(new Vector3(1980,20,454.5f)),
                Matrix.CreateTranslation(new Vector3(2030,20,434.5f)),
                Matrix.CreateTranslation(new Vector3(2080,20,404.5f)),
                Matrix.CreateTranslation(new Vector3(2130,20,414.5f)),

                //Cilindros que Giran  
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3100,95,1775)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3050,95,1845)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3100,95,1915)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3050,95,1995))
            };

            for(int i =0; i< 6; i++)
            {
                int orientation = (i % 2 == 0) ? 1 : -1;
                Obstacles.Add(new MovingObstacle(CylinderWorld[i],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y),Simulation,Camera,orientation*new Vector3(0,10f,0), Vector3.Zero));
            }

            for(int i =6; i< 13; i++)
            {
               Obstacles.Add(new MovingObstacle(CylinderWorld[i],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y),Simulation,Camera));
            }
            for(int i =13; i< 17; i++)
            {
                int orientation = (i % 2 == 0) ? 1 : -1;
               Obstacles.Add(new MovingObstacle(CylinderWorld[i],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y),Simulation,Camera, orientation* new Vector3(0,5,0), Vector3.Zero));
            }

            return Obstacles;
        }


        public List<Obstacle> LoadPeriodics()
        {
            var Obstacles = new List<Obstacle>();

            //Plataforma que baja
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(20,10,20) * Matrix.CreateTranslation(1720,42.5f,2405),new BoxPrimitive(GraphicsDevice),Simulation,
                                Camera,42.5f,3f,42.5f,0f, "Y"));

            //Pared que aplastan contra el suelo
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4240),new BoxPrimitive(GraphicsDevice),Simulation,
                    Camera,35f,4f,45,0f, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4430),new BoxPrimitive(GraphicsDevice),Simulation,
                    Camera,35f,4f,45,+MathHelper.PiOver4, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4620),new BoxPrimitive(GraphicsDevice),Simulation,
                    Camera,35f,4f,45,MathHelper.PiOver4, "Y"));
            //Plataformas que suben y bajan
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(50,10,50) * Matrix.CreateTranslation(600f,-60,4.5f),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,-70f,3f,-60f,0f, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(50,10,50) * Matrix.CreateTranslation(700f,60,4.5f),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,70f,3f,60f,0f, "Y"));
            
            //Pisos que se mueven
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(1740,0,5230),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5230f,0f, "Z"));
                Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(1690,0,5600),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5600f,MathHelper.Pi, "Z"));
                Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(1740,0,6010),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,6010f,0f, "Z"));

            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,5) * Matrix.CreateTranslation(1740,10,5190),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5190,0f, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,5) * Matrix.CreateTranslation(1740,10,5270),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5270,0f, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(5,10,80) * Matrix.CreateTranslation(1760,10,5230),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5230,0f, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(5,10,80) * Matrix.CreateTranslation(1720,10,5230),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5230,0f, "Z"));

            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,5) * Matrix.CreateTranslation(1690,10,5640),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5640,MathHelper.Pi, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,5) * Matrix.CreateTranslation(1690,10,5560),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5560,MathHelper.Pi, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(5,10,80) * Matrix.CreateTranslation(1710,10,5600),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5600,MathHelper.Pi, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(5,10,80) * Matrix.CreateTranslation(1670,10,5600),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5600,MathHelper.Pi, "Z"));

            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,5) * Matrix.CreateTranslation(1740,10,6050),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,6050,0f, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(40,10,5) * Matrix.CreateTranslation(1740,10,5970),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,5970,0f, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(5,10,80) * Matrix.CreateTranslation(1760,10,6010),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,6010,0f, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(5,10,80) * Matrix.CreateTranslation(1720,10,6010),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,200f,1.5f,6010,0f, "Z"));



/*
 

            WallsWorld[20].Decompose(out scale, out rotation, out position);
            max = new Vector3(position.X + 20,position.Y + 10.1f,position.Z+40);
            min = new Vector3(position.X - 20,position.Y - 10.1f,position.Z-40);
            isWithinLimits = SphereCollider.Center == Vector3.Clamp(SphereCollider.Center, min, max);
            WallsWorld[20] = Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1690,0,200*MathF.Cos(1.5f*totalTime+MathHelper.Pi)+5600));
            if(isWithinLimits)
                SphereCollider.Center.Z += 200*MathF.Cos(1.5f*totalTime+MathHelper.Pi)+5600 - position.Z;

            WallsWorld[21].Decompose(out scale, out rotation, out position);
            max = new Vector3(position.X + 20,position.Y + 10.1f,position.Z+40);
            min = new Vector3(position.X - 20,position.Y - 10.1f,position.Z-40);
            isWithinLimits = SphereCollider.Center == Vector3.Clamp(SphereCollider.Center, min, max);
            WallsWorld[21] = Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1740,0,200*MathF.Cos(1.5f*totalTime)+6010));
            if(isWithinLimits)
                SphereCollider.Center.Z += 200*MathF.Cos(1.5f*totalTime)+6010 - position.Z;

            CreateBoundingBoxes();

        }*/

            
            //Paredes que se mueven
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,2800),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,40f,5f,1720f,0f, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,2900),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,-40f,5f,1720f,MathF.PI/4, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,3000),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,40f,5f,1720f,MathF.PI/2f, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,3100),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,-40f,5f,1720f,MathHelper.PiOver4*3f, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(1720,30f,3200),new BoxPrimitive(GraphicsDevice),Simulation,
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
                Matrix.CreateScale(80,10f,100) * Matrix.CreateTranslation(new Vector3(3070f,70,1295f)),
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
                Matrix.CreateScale(40, 30f,10f) * Matrix.CreateTranslation(new Vector3(3090f,35f,510f)),
                Matrix.CreateScale(40, 30f,10f) * Matrix.CreateTranslation(new Vector3(3090f,35f,670f)),
                Matrix.CreateScale(40, 30f,10f) * Matrix.CreateTranslation(new Vector3(3050f,35f,590f)),

                //muros
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3320)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3460)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1700,25,3895)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1740, 25, 4020)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7825))
            };

            
            foreach(Matrix world in staticsWorld){
               Obstacles.Add(new StaticObstacle(world,new BoxPrimitive(GraphicsDevice),Simulation,Camera));
            }

            return Obstacles;

        }

        public List<Obstacle> LoadRollingCylinders() 
        {
            var Obstacles = new List<Obstacle>();
            var BasicCylindersMeasures = new Vector3[]
            {
                //Cilindros que giran
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 20, 18),                
                new Vector3(60, 20, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 20, 18),                
                new Vector3(60, 20, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18)
            };

            var CylinderWorld = new Matrix[]{
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(2100,93.8f,2365)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(2000,90,2365)),
               Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1900,85,2365)),
               Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1800,85,2365)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1700,90,2365)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1600,90,2365)),
               Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1500,90,2365)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1400,90,2365)),

                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(2200,85,2445)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(2100,95,2445)),
               Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(2000,95,2445)),
               Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1900,95,2445)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1800,95,2445)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1700,95,2445)),
               Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1600,95,2445)),
                Matrix.CreateFromYawPitchRoll(0,-MathHelper.PiOver2,0) * Matrix.CreateTranslation(new Vector3(1500,95,2445)),

            };

            for(int i =0; i< CylinderWorld.Length; i++)
            {
                Obstacles.Add(new MovingObstacle(CylinderWorld[i],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y),Simulation,Camera, Vector3.Zero, new Vector3(50,0,0)));
            }
            return Obstacles;
        }
    }
}