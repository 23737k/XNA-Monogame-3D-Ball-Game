using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using System.Collections.Generic;
using BepuPhysics;
using TGC.MonoGame.TP.MapObjects;
using BepuPhysics.Collidables;
using NumericVector3 = System.Numerics.Vector3;

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
        
        public List<MovingObstacle> LoadKinematics()
        {
            var Obstacles = new List<MovingObstacle>();
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

                new Vector3(120, 10, 18),
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
                Matrix.CreateTranslation(new Vector3(1880,20,434.5f)),
                Matrix.CreateTranslation(new Vector3(1930,20,454.5f)),
                Matrix.CreateTranslation(new Vector3(1980,20,454.5f)),
                Matrix.CreateTranslation(new Vector3(2030,20,434.5f)),
                Matrix.CreateTranslation(new Vector3(2080,20,404.5f)),
                Matrix.CreateTranslation(new Vector3(2130,20,414.5f)),

                //Cilindros que Giran  
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3100,95,1775)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3050,95,1845)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3100,95,1915)),
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(3050,95,1995)),
                
                Matrix.CreateFromYawPitchRoll(0,0,-MathHelper.PiOver2) * Matrix.CreateTranslation(new Vector3(2960,95,2405)),

            };

            for(int i =0; i< 6; i++)
            {
                int orientation = (i % 2 == 0) ? -1 : 1;
                Obstacles.Add(new MovingObstacle(CylinderWorld[i],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y),Simulation,Camera,orientation*new Vector3(0,-10f,0), Vector3.Zero));
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
            Obstacles.Add(new MovingObstacle(CylinderWorld[17],
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[17].X, BasicCylindersMeasures[17].Y),Simulation,Camera, new Vector3(0,-12,0), Vector3.Zero));

            return Obstacles;
        }


        public List<PeriodicObstacle> LoadPeriodics()
        {
            var Obstacles = new List<PeriodicObstacle>();

            //Plataforma que baja
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(20,10,20) * Matrix.CreateTranslation(1720,42.5f,2405),new BoxPrimitive(GraphicsDevice),Simulation,
                                Camera,42.5f,3f,42.5f,0f, "Y"));

            //Pared que aplastan contra el suelo
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4240),new BoxPrimitive(GraphicsDevice),Simulation,
                    Camera,35f,7f,45,0f, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4430),new BoxPrimitive(GraphicsDevice),Simulation,
                    Camera,35f,7f,45,+MathHelper.PiOver4, "Y"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(80,10,80) * Matrix.CreateTranslation(1720,45,4620),new BoxPrimitive(GraphicsDevice),Simulation,
                    Camera,35f,7f,45,MathHelper.PiOver4, "Y"));
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

            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(2674,115f,2363),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,135f,2.3f,2674,MathF.PI, "X"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(2720,115f,2447),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,135f,2.3f,2720,0, "X"));

            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(30,30,10) * Matrix.CreateTranslation(2462,100,2345),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,35,3.2f,2345,0, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(30,30,10) * Matrix.CreateTranslation(2382,100,2345),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,35,3.4f,2345,MathF.PI/6, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(30,30,10) * Matrix.CreateTranslation(2302,100,2345),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,35,3.8f,2345,MathF.PI/3, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(30,30,10) * Matrix.CreateTranslation(2222,100,2345),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,35,4.2f,2345,MathF.PI/2, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(30,30,10) * Matrix.CreateTranslation(2142,100,2345),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,35,4.6f,2345,2*MathF.PI/3, "Z"));
            Obstacles.Add(new PeriodicObstacle(Matrix.CreateScale(30,30,10) * Matrix.CreateTranslation(2062,100,2345),new BoxPrimitive(GraphicsDevice),Simulation,
                Camera,35,4.8f,2345,5*MathF.PI/6, "Z"));
            return Obstacles;
        }
        public List<StaticObstacle> LoadStatics()
        {
            var Obstacles = new List<StaticObstacle>();
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
                
                Matrix.CreateScale(300,10f,120) * Matrix.CreateTranslation(new Vector3(2960,85,2405)),

                Matrix.CreateScale(1100,10f,40) * Matrix.CreateTranslation(new Vector3(2260,85,2365)),
                Matrix.CreateScale(350,10f,40) * Matrix.CreateTranslation(new Vector3(2635,85,2445)),

                Matrix.CreateScale(80,10f,1500) * Matrix.CreateTranslation(new Vector3(1720,0,3175)),

                Matrix.CreateScale(60,20,10) * Matrix.CreateTranslation(new Vector3(2630,100,2425)),

                Matrix.CreateScale(80,10,1000) * Matrix.CreateTranslation(new Vector3(1720,0,4455)),
                Matrix.CreateScale(70,10f,1125) * Matrix.CreateTranslation(new Vector3(1720,0,6852)),
                Matrix.CreateScale(70,10f,150) * Matrix.CreateTranslation(new Vector3(1720,0,8995)),

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

                Matrix.CreateScale(80,10,5) * Matrix.CreateTranslation(new Vector3(1720, 10, 6424)),
                Matrix.CreateScale(30,30,5) * Matrix.CreateTranslation(new Vector3(1720, 20, 6529)),
                Matrix.CreateScale(25,30,5) * Matrix.CreateTranslation(new Vector3(1700, 20, 6600)),
                Matrix.CreateScale(25,30,5) * Matrix.CreateTranslation(new Vector3(1740, 20, 6600)),
                Matrix.CreateScale(50,30,5) * Matrix.CreateTranslation(new Vector3(1710, 20, 6665)),
                Matrix.CreateScale(20,5,5) * Matrix.CreateTranslation(new Vector3(1745, 7.5f, 6665)),
                Matrix.CreateScale(25,30,5) * Matrix.CreateTranslation(new Vector3(1740, 20, 6741)),
                Matrix.CreateScale(25,30,5) * Matrix.CreateTranslation(new Vector3(1700, 20, 6809)),
                Matrix.CreateScale(25,30,5) * Matrix.CreateTranslation(new Vector3(1740, 20, 6879)),
                Matrix.CreateScale(25,30,5) * Matrix.CreateTranslation(new Vector3(1700, 20, 6949)),
                Matrix.CreateScale(70,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7007)),
                Matrix.CreateScale(70,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7134)),
                Matrix.CreateScale(45,30,10) * Matrix.CreateTranslation(new Vector3(1720, 20, 7308)),
                Matrix.CreateScale(70,5,10) * Matrix.CreateTranslation(new Vector3(1720, 7.5f, 7348)),

//islas boss
                Matrix.CreateScale(20,5,30) * Matrix.CreateTranslation(new Vector3(1720, 10, 7484)),
                //Matrix.CreateScale(20,5,30) * Matrix.CreateTranslation(new Vector3(1720, 10, 7514)),
                Matrix.CreateScale(20,5,30) * Matrix.CreateTranslation(new Vector3(1740, 10, 7564)),
                //Matrix.CreateScale(20,5,30) * Matrix.CreateTranslation(new Vector3(1720, 10, 7615)),
                Matrix.CreateScale(20,5,30) * Matrix.CreateTranslation(new Vector3(1720, 10, 7644)),
                Matrix.CreateScale(20,5,30) * Matrix.CreateTranslation(new Vector3(1690,10, 7724)),

                Matrix.CreateScale(70,10,100) * Matrix.CreateTranslation(new Vector3(1720,0, 7815)),
                Matrix.CreateScale(70,10,200) * Matrix.CreateTranslation(new Vector3(1720,0, 8005)),

                Matrix.CreateScale(50,20,10) * Matrix.CreateTranslation(new Vector3(1732,15, 8053)),

                Matrix.CreateScale(30,10,110) * Matrix.CreateTranslation(new Vector3(1740,0, 8160)),
                Matrix.CreateScale(70,10,300) * Matrix.CreateTranslation(new Vector3(1720,0, 8365)),

                Matrix.CreateScale(70,15,10) * Matrix.CreateTranslation(new Vector3(1720,12.5f, 8285)),
                Matrix.CreateScale(70,10,10) * Matrix.CreateTranslation(new Vector3(1720,35, 8285)),
                
                //Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7825)),

                Matrix.CreateScale(10,15,30) * Matrix.CreateTranslation(new Vector3(2497, 90, 2365)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(2417, 100, 2375)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(2337, 100, 2355)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(2257, 100, 2375)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(2177, 100, 2355)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(2097, 100, 2375)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(2017, 100, 2355)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(1937, 100, 2375)),
                Matrix.CreateScale(10,20,15) * Matrix.CreateTranslation(new Vector3(1887, 100, 2355)),

                Matrix.CreateScale(10,10,30) * Matrix.CreateTranslation(new Vector3(2497, 95, 2447)),

                Matrix.CreateScale(30,5,30) * Matrix.CreateTranslation(new Vector3(2403, 95, 2447)),
                Matrix.CreateScale(30,5,30) * Matrix.CreateTranslation(new Vector3(2343, 105, 2507)),
                Matrix.CreateScale(30,5,30) * Matrix.CreateTranslation(new Vector3(2283, 115, 2567)),
                Matrix.CreateScale(30,5,30) * Matrix.CreateTranslation(new Vector3(2223, 105, 2567)),
                Matrix.CreateScale(30,5,30) * Matrix.CreateTranslation(new Vector3(2163, 95, 2507)),
                Matrix.CreateScale(30,5,30) * Matrix.CreateTranslation(new Vector3(2103, 85, 2447)),
                
                Matrix.CreateScale(5,20,15) * Matrix.CreateFromYawPitchRoll(0, 0,MathHelper.Pi/13) * Matrix.CreateTranslation(new Vector3(2043, 85, 2452)),
                Matrix.CreateScale(5,20,15) * Matrix.CreateFromYawPitchRoll(0, 0,MathHelper.Pi/13) * Matrix.CreateTranslation(new Vector3(1983, 70, 2438)),
                Matrix.CreateScale(5,20,15) * Matrix.CreateFromYawPitchRoll(0, 0,MathHelper.Pi/13) * Matrix.CreateTranslation(new Vector3(1923, 55, 2452)),
                Matrix.CreateScale(5,20,15) * Matrix.CreateFromYawPitchRoll(0, 0,MathHelper.Pi/13) * Matrix.CreateTranslation(new Vector3(1863, 40, 2438)),
                Matrix.CreateScale(5,20,15) * Matrix.CreateFromYawPitchRoll(0, 0,MathHelper.Pi/13) * Matrix.CreateTranslation(new Vector3(1803, 25, 2452)),

                Matrix.CreateScale(350,5,30)  * Matrix.CreateFromYawPitchRoll(0, 0,MathHelper.Pi/13)* Matrix.CreateTranslation(new Vector3(1915, 40, 2444))

            };

            
            foreach(Matrix world in staticsWorld){
               Obstacles.Add(new StaticObstacle(world,new BoxPrimitive(GraphicsDevice),Simulation,Camera));
            }

            return Obstacles;

        }


        public BodyHandle LoadFinalBoss()
        {
            var collidableDescription =  new CollidableDescription(Simulation.Shapes.Add(new Sphere(40f)), 0.1f, ContinuousDetection.Continuous(1e-4f, 1e-4f));
            var position = new NumericVector3(1720,35,6323);
            var bodyHandle = Simulation.Bodies.Add(BodyDescription.
                            CreateKinematic(new RigidPose(position), 
            new CollidableDescription(Simulation.Shapes.Add(new Sphere(20f)), 0.1f, ContinuousDetection.Continuous(1e-4f, 1e-4f)), new BodyActivityDescription(-0.1f)));

            Simulation.Bodies.GetBodyReference(bodyHandle).Velocity.Linear= new NumericVector3(0,0,120);
            Simulation.Bodies.GetBodyReference(bodyHandle).Velocity.Angular= new NumericVector3(15,0,0);
            return bodyHandle;
        }
    }
}