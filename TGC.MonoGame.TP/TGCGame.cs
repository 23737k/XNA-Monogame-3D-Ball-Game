using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using System.Collections.Generic;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using TGC.MonoGame.TP.Physics.Bepu;
using TGC.MonoGame.TP.MapObjects;
using TGC.MonoGame.TP.Spheres;
using NumericVector3 = System.Numerics.Vector3;
using TGC.MonoGame.TP.PBR;
using TGC.MonoGame.TP.Checkpoints;
using TGC.MonoGame.TP.Powerups;
using System.Linq;

//
namespace TGC.MonoGame.TP
{
    /// <summary>d
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }


        private float LINEAR_SPEED= 100;
        private float JUMPING_SPEED = 50F;
        private  float SALTO_BUFFER_VALUE = 1000f;
        private const float CAMERA_FOLLOW_RADIUS = 65f;
        private const float CAMERA_UP_DISTANCE = 30f;
        private const float COORDENADA_Y_MAS_BAJA = -100;
        private const float GRAVITY = -450;
        //CHECKPOINTS
        private Checkpoint[] Checkpoints {get;set;}
        private int CurrentCheckpoint;

        private List<Powerup> Powerups {get;set;}
        private Powerup CurrentPowerUp = null;
        private GraphicsDeviceManager Graphics { get; }
        //EFFECTS
        private Effect DefaultEffect { get; set; }
        private Effect SphereEffect { get; set; }
        private List<Light> Lights {get;set;} 
        //TEXTURES
        private Texture2D albedo, ao, metalness, roughness, normals;
        //Texture index
        private int textureIndex;
        private SphereType[] SpheresArray; 
        //
        private SpriteBatch SpriteBatch { get; set; }
        private SpriteFont SpriteFont {get;set;}
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        //SPHERE
        private Model SphereModel  {get; set;}

        private Model SpeedModel {get;set;}
        private Matrix SphereRotationMatrix { get; set; }
        private BoxPrimitive ObstacleBox { get; set; }
        private BoxPrimitive YellowBox { get; set; }
        private Vector3 SpherePosition { get; set; }
        private TargetCamera Camera { get; set; }

        private Matrix[] PowerUpsWorld { get; set;}
        private Vector3 SphereVelocity { get; set; }
        private bool OnGround { get; set; }
        private Vector3 SphereFrontDirection { get; set; }
        private Vector3 LightPosition { get; set; } = new Vector3 (0,2500,0);

        //Bepu
        private BodyHandle SphereHandle { get; set; }
        private Matrix SphereWorld { get; set; }
        private BufferPool BufferPool { get; set; }
        private Simulation Simulation { get; set; }
        private SimpleThreadDispatcher ThreadDispatcher { get; set; }
        private Sphere BepuSphere { get; set; }

        private List<PeriodicObstacle> PeriodicObstacles { get; set; }
        private List<StaticObstacle> StaticObstacles {get;set;} 
        private List<MovingObstacle> MovingObstacles { get; set; }
        private bool FinalBossEnabled {get;set;} = true;
        private BodyHandle BossSphereHandle {get;set;}
        private Loader Loader {get;set;}

        private bool FinalBossStage {get;set;} =false;

        //Skybox
        private float Angle { get; set; }
        private Vector3 CameraPosition { get; set; }
        private Vector3 CameraTarget { get; set; }
        private float Distance { get; set; }
        private Vector3 ViewVector { get; set; }
        private SkyBox SkyBox { get; set; }

        private Model PowerupModel {get;set;}
        private Model CylinderModel {get;set;}

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();
            // Seria hasta aca.

            OnGround = false;

            Checkpoints = new Checkpoint[]
            {
                new Checkpoint(new Vector3(0, 10.001f, 0)),
                new Checkpoint(new Vector3(800f,110f,245f)),
                new Checkpoint(new Vector3(2230f,30,435f)),
                new Checkpoint(new Vector3(3050,95,2245)),
                new Checkpoint(new Vector3(1723,10,2537)),
                new Checkpoint(new Vector3(1725,10,4800)),
                new Checkpoint(new Vector3(1720,10,6302))
            };
            CurrentCheckpoint = 3;

            Powerups = new List<Powerup>();
            Powerups.Add(new Powerup(new Vector3(30,15,0),100,0));

        
            // Esfera
            SpherePosition = new Vector3(0,15,0);//new Vector3(0,50,0);//new Vector3(1732,20, 8073);
            //
            SphereWorld = Matrix.CreateTranslation(SpherePosition);
            SphereVelocity = Vector3.Zero;
            SphereFrontDirection =  Vector3.Backward;
            SphereRotationMatrix = Matrix.Identity;

            //Texture Index
            //Elegimos el texture index que querramos para modificar los valores de la textura, salto, etc.
            textureIndex = 2;
            SpheresArray = new SphereType[]
            {   
                new SphereMarble(),
                new SphereMetal(),
                new SpherePlastic() 
            };
    

            LINEAR_SPEED = SpheresArray[textureIndex].speed();
            SALTO_BUFFER_VALUE = SpheresArray[textureIndex].jump();
/*
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
                         
*/
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);


            UpdateCamera();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            
            var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, 1000);

            //Luz
            DefaultEffect = Content.Load<Effect>(ContentFolderEffects + "ColorShader");
            DefaultEffect.Parameters["lightPosition"].SetValue(LightPosition);
            DefaultEffect.Parameters["ambientColor"]?.SetValue(new Vector3(0.8f, 0.95f,  1f));
            DefaultEffect.Parameters["diffuseColor"]?.SetValue(new Vector3(0.8f, 0.95f,  1f));
            DefaultEffect.Parameters["specularColor"]?.SetValue(new Vector3(1,1,1));

            DefaultEffect.Parameters["KAmbient"]?.SetValue(0.3f);
            DefaultEffect.Parameters["KDiffuse"]?.SetValue(0.7f);
            DefaultEffect.Parameters["KSpecular"]?.SetValue(0.4f);
            DefaultEffect.Parameters["shininess"]?.SetValue(10f);
            var texture = Content.Load<Texture2D>(ContentFolderTextures +"piso/" +"color");
            var normal = Content.Load<Texture2D>(ContentFolderTextures + "piso/" + "normal");
            DefaultEffect.Parameters["ModelTexture"].SetValue(texture);
            DefaultEffect.Parameters["NormalTexture"].SetValue(normal);

            DefaultEffect.CurrentTechnique = DefaultEffect.Techniques["NormalMapping"];

            InitializeLights();
            InitializeEffect();
            LoadTextures();
            SphereModel = Content.Load<Model>(ContentFolder3D + "sphere");
            SphereModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = SphereEffect;     

            SpeedModel = Content.Load<Model>(ContentFolder3D + "speedPower");     
            SpeedModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = DefaultEffect;  

            CylinderModel = Content.Load<Model>(ContentFolder3D + "cylinder");
            CylinderModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = DefaultEffect;
            //Bepu
            LoadPhysics();

            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            base.LoadContent();
        }

        private void UpdateCamera()
        {
            var sphereBackDirection = -Vector3.Transform(Vector3.Forward, SphereRotationMatrix);
            var camera_follow_radius= CAMERA_FOLLOW_RADIUS;
            var camera_up_distance = CAMERA_UP_DISTANCE;
            
            var orbitalPosition = sphereBackDirection * camera_follow_radius;

            var upDistance = Vector3.Up * camera_up_distance;

            Camera.Position = SpherePosition + orbitalPosition + upDistance;

            Camera.TargetPosition = SpherePosition;

            Camera.BuildView();
        }
        protected override void Update(GameTime gameTime)
        {
            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            PowerupManager(gameTime);
            MovementManager(deltaTime);

            BodyReference body = Simulation.Bodies.GetBodyReference(SphereHandle);
            var prevLinearVelocity = body.Velocity.Linear.Y;
            var prevAngularVelocity = body.Velocity.Angular.Y;

            
            /*
            if(SpherePosition == Vector3.Clamp(SpherePosition, new Vector3(1673,9.9f,6439), new Vector3(1768,40,6517)) && FinalBossEnabled)
            {
                BossSphereHandle = Loader.LoadFinalBoss();
                FinalBossEnabled = false;
                FinalBossStage = true;
            }
            */
            
            CheckpointManager();
            ObstacleContact();


            Simulation.Timestep(1 / 60f, ThreadDispatcher);

            var pose = Simulation.Bodies.GetBodyReference(SphereHandle).Pose;
            SpherePosition = pose.Position;
            var quaternion = pose.Orientation;
            var world = Matrix.CreateScale(BepuSphere.Radius) *
                            Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                                quaternion.W)) *
                            Matrix.CreateTranslation(new Vector3(SpherePosition.X, SpherePosition.Y, SpherePosition.Z));
            SphereWorld = world;
            SpherePosition = pose.Position;
               

            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);

            if(MathHelper.Distance(bodyRef.Velocity.Linear.Y, prevLinearVelocity) < 0.2f 
               && MathHelper.Distance(bodyRef.Velocity.Angular.Y, prevAngularVelocity) < 0.2f) OnGround = true;

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            UpdateCamera();

            base.Update(gameTime);
        }     
        protected void MovementManager(float deltaTime){
            var FinalSpeed = LINEAR_SPEED;
            if(CurrentPowerUp != null)
            {
                FinalSpeed += CurrentPowerUp.SpeedBoost;
            }

            
            SphereRotationMatrix = Matrix.CreateRotationY(Mouse.GetState().X*-0.01f);
            SphereFrontDirection = Vector3.Transform(Vector3.Backward, SphereRotationMatrix);
            var SphereLateralDirection = Vector3.Transform(Vector3.Right, SphereRotationMatrix);

            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                if(!PelotaEstaEnElSuelo())
                    bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(-SphereFrontDirection * JUMPING_SPEED));
                else
                     bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(-SphereFrontDirection * FinalSpeed));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) && !PelotaSeCayo())
            {
                //SphereVelocity += SphereFrontDirection * LINEAR_SPEED;
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(SphereFrontDirection * FinalSpeed));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(-SphereLateralDirection * FinalSpeed*0.5f));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(SphereLateralDirection * FinalSpeed*0.5f));
            }
            
            AdministrarSalto(deltaTime);
        }
        protected void AdministrarSalto(float deltaTime){
            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            if ((Keyboard.GetState().IsKeyDown(Keys.Space)|| Keyboard.GetState().IsKeyDown(Keys.Up))&& PelotaEstaEnElSuelo())
            {
                //SphereVelocity += Vector3.Up * SALTO_BUFFER_VALUE;
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(Vector3.Up * SALTO_BUFFER_VALUE));
                OnGround = false;
            }  
        }

        protected bool PelotaEstaEnElSuelo(){
            return OnGround;
        }

        protected bool PelotaSeCayo(){
            return SpherePosition.Y < COORDENADA_Y_MAS_BAJA;
        }
        
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            DrawSkybox();

            //Dibujo el suelo
            foreach(StaticObstacle obstacle in StaticObstacles)   {obstacle.Render(DefaultEffect,gameTime);}
             //Dibujo los cilindros
            foreach(MovingObstacle obstacle in MovingObstacles)    {obstacle.Render(DefaultEffect,gameTime);}
            foreach(PeriodicObstacle obstacle in PeriodicObstacles)    {obstacle.Render(DefaultEffect,gameTime);}

            Utils.SetEffect(Camera, SphereEffect, SphereWorld);
            SphereModel.Meshes.FirstOrDefault().Draw();

            var powerUpWorld = Matrix.CreateScale(2f) * Matrix.CreateFromYawPitchRoll(Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds)*3,0,MathF.PI/2)* Matrix.CreateTranslation(30,15,0);
            Utils.SetEffect(Camera,DefaultEffect,powerUpWorld);
            SpeedModel.Meshes.FirstOrDefault().Draw();

            var powerUpWorld2 = Matrix.CreateScale(2f) * Matrix.CreateFromYawPitchRoll(Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds)*3,0,MathF.PI/2)* Matrix.CreateTranslation(30,15,-7);
            Utils.SetEffect(Camera,DefaultEffect,powerUpWorld2);
            SpeedModel.Meshes.FirstOrDefault().Draw();

            
            if(FinalBossStage)
            {
            var pose = Simulation.Bodies.GetBodyReference(BossSphereHandle).Pose;
            var bossWorld = Matrix.CreateScale(45f) * Matrix.CreateFromQuaternion(pose.Orientation) * Matrix.CreateTranslation(pose.Position);
            Utils.SetEffect(Camera,SphereEffect,bossWorld);
            SphereModel.Meshes.FirstOrDefault().Draw();
            }

            var fps = MathF.Round(1/Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds),1);

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            var position= new Vector3(MathF.Round(SpherePosition.X,1), MathF.Round(SpherePosition.Y,1), MathF.Round(SpherePosition.Z,1));
            SpriteBatch.DrawString(SpriteFont, "Position:" + position.ToString(), new Vector2(GraphicsDevice.Viewport.Width - 500, 0), Color.White);
            SpriteBatch.DrawString(SpriteFont, "FPS " + fps.ToString(), new Vector2(GraphicsDevice.Viewport.Width-1000, 0), Color.White);
            SpriteBatch.End();


            base.Draw(gameTime);
        }

        private void LoadPhysics()
        {
            BufferPool = new BufferPool();
            BepuSphere = new Sphere(5f);

             var targetThreadCount = Math.Max(1,
                Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

             Simulation = Simulation.Create(new BufferPool(),
                new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new NumericVector3(0, GRAVITY, 0)),
                new SolveDescription(8, 1));

            Loader = new Loader(Simulation,GraphicsDevice,Camera, CylinderModel);
            StaticObstacles = Loader.LoadStatics();
            MovingObstacles=  Loader.LoadKinematics();
            PeriodicObstacles = Loader.LoadPeriodics();


            var bodyDescription = BodyDescription.CreateConvexDynamic(Utils.ToNumericVector3(SpherePosition), 5f,
               Simulation.Shapes, BepuSphere);
            bodyDescription.Collidable.Continuity = ContinuousDetection.Continuous(1e-4f, 1e-4f);
            bodyDescription.Activity.SleepThreshold=-1;
            var bodyHandle = Simulation.Bodies.Add(bodyDescription);
            SphereHandle = bodyHandle;
        }

         private void InitializeLights()
        {
            Lights = new List<Light>();
            var lightOne = new Light();

            lightOne.Position = new Vector3(1500,25000,1775);
            lightOne.Color = new Vector3(255,255,255);

            var lightTwo = new Light();
            lightTwo.Position = new Vector3(4500,25000,1775);
            lightTwo.Color = new Vector3(255,255,255);

            Lights.Add(lightOne);
            Lights.Add(lightTwo);
            Lights = Lights.ConvertAll(light => light.GenerateShowColor());
        }
        private void InitializeEffect()
        {
            SphereEffect = Content.Load<Effect>(ContentFolderEffects + "PBR");
            SphereEffect.CurrentTechnique = SphereEffect.Techniques["PBR"];

            var positions = SphereEffect.Parameters["lightPositions"].Elements;
            var colors = SphereEffect.Parameters["lightColors"].Elements;

            for (var index = 0; index < Lights.Count; index++)
            {
                var light = Lights[index];
                positions[index].SetValue(light.Position);
                colors[index].SetValue(light.Color);
            }
        }
        private void LoadTextures()
        {
            String folder = SpheresArray[textureIndex].folder();

            normals = Content.Load<Texture2D>(ContentFolderTextures + folder +"normal");
            ao = Content.Load<Texture2D>(ContentFolderTextures + folder + "ao");
            metalness = Content.Load<Texture2D>(ContentFolderTextures +  folder + "metalness");
            roughness = Content.Load<Texture2D>(ContentFolderTextures + folder + "roughness");
            albedo = Content.Load<Texture2D>(ContentFolderTextures + folder + "color");

            SphereEffect.Parameters["albedoTexture"]?.SetValue(albedo);
            SphereEffect.Parameters["normalTexture"]?.SetValue(normals);
            SphereEffect.Parameters["metallicTexture"]?.SetValue(metalness);
            SphereEffect.Parameters["roughnessTexture"]?.SetValue(roughness);
            SphereEffect.Parameters["aoTexture"]?.SetValue(ao);
        }

        private void DrawSkybox()
        {
            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;
            SkyBox.Draw(Camera.View, Camera.Projection, SpherePosition+new Vector3(0,-300,0) );
           GraphicsDevice.RasterizerState = originalRasterizerState;
        }
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Simulation.Dispose();
            BufferPool.Clear();
            ThreadDispatcher.Dispose();
            Content.Unload();
            base.UnloadContent();
        }

        private void CheckpointManager()
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);
            if(PelotaSeCayo())
            {
                bodyRef.Pose.Position = Utils.ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
                FinalBossEnabled = true;
                FinalBossStage = false;
                return;
            }
            for(int i= CurrentCheckpoint; i< Checkpoints.Length; i++)
            {
                if(Checkpoints[i].IsWithinBounds(bodyRef.Pose.Position))
                {
                    CurrentCheckpoint = i;
                    break;
                }
            }
        }

        private void PowerupManager(GameTime gameTime)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);

            if(CurrentPowerUp == null)
            {
                foreach (var powerUp in Powerups)
                {
                    if(powerUp.IsWithinBounds(bodyRef.Pose.Position, gameTime))
                    {
                        CurrentPowerUp = powerUp;
                        return;
                    }
                }
                return;
            }

            else if(!CurrentPowerUp.isActive(gameTime))
            {
                Powerups.Remove(CurrentPowerUp);
                CurrentPowerUp = null;
            }
        }
    

        private void ObstacleContact()
        {
            var boundingSphere = new BepuUtilities.BoundingSphere(Utils.ToNumericVector3(SpherePosition), 5f);
            for(int i=1; i<4; i++)
            {
                if(Simulation.Bodies.GetBodyReference(PeriodicObstacles[i].BodyHandle).BoundingBox.Intersects(
                     ref boundingSphere))
                    {
                        Simulation.Bodies.GetBodyReference(SphereHandle).Pose.Position = Utils.ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
                        FinalBossStage = false;
                        break;
                    }
            }

            var bossBoundingSphere = new BoundingSphere(Utils.ToNumericVector3(Simulation.Bodies.GetBodyReference(BossSphereHandle).Pose.Position), 40f);
            if(bossBoundingSphere.Intersects(new BoundingSphere(Utils.ToNumericVector3(SpherePosition), 5f))&&FinalBossStage)
                {
                    Simulation.Bodies.GetBodyReference(SphereHandle).Pose.Position = Utils.ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
                    FinalBossEnabled = true;
                    FinalBossStage = false;
                }
        }
    }

}