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
        private const float CAMERA_FOLLOW_RADIUS = 65;
        private const float CAMERA_UP_DISTANCE = 30f;
        private  float SALTO_BUFFER_VALUE = 1000f;
        private const float GRAVITY = -450;
        //CHECKPOINTS DEBE ESTAR ORDENADO ASCENDENTEMENTE
        private Checkpoint[] Checkpoints {get;set;}
        private int CurrentCheckpoint {get;set;}
        private const float COORDENADA_Y_MAS_BAJA = -80f;
        private GraphicsDeviceManager Graphics { get; }
        //EFFECTS
        private Effect SimpleColor { get; set; }
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
        
            // Esfera
            SpherePosition = new Vector3(1720,10,6342);//new Vector3(1732,20, 8073);
            //
            SphereWorld = Matrix.CreateTranslation(SpherePosition);
            SphereVelocity = Vector3.Zero;
            SphereFrontDirection =  Vector3.Backward;
            SphereRotationMatrix = Matrix.Identity;

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

            //Texture Index
            //Elegimos el texture index que querramos para modificar los valores de la textura, salto, etc.
            textureIndex = 0;
            SpheresArray = new SphereType[]
            {   
                new SphereMarble(),
                new SphereMetal(),
                new SphereGround() 
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

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            
            var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/skybox/skybox");
            //var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/sun-in-space/sun-in-space");
            //var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, 1000);


            //Luz
            SimpleColor = Content.Load<Effect>(ContentFolderEffects + "ColorShader");
            SimpleColor.Parameters["lightPosition"].SetValue(LightPosition);
            SimpleColor.Parameters["ambientColor"]?.SetValue(new Vector3(0.8f, 0.95f,  1f));
            SimpleColor.Parameters["diffuseColor"]?.SetValue(new Vector3(0.8f, 0.95f,  1f));
            SimpleColor.Parameters["specularColor"]?.SetValue(new Vector3(1,1,1));

            SimpleColor.Parameters["KAmbient"]?.SetValue(0.3f);
            SimpleColor.Parameters["KDiffuse"]?.SetValue(0.7f);
            SimpleColor.Parameters["KSpecular"]?.SetValue(0.4f);
            SimpleColor.Parameters["shininess"]?.SetValue(10f);
            var texture = Content.Load<Texture2D>(ContentFolderTextures +"piso/" +"color");
            var normal = Content.Load<Texture2D>(ContentFolderTextures + "piso/" + "normal");
            SimpleColor.Parameters["ModelTexture"].SetValue(texture);
            SimpleColor.Parameters["NormalTexture"].SetValue(normal);

            InitializeLights();
            InitializeEffect();
            LoadTextures();
            SphereModel = Content.Load<Model>(ContentFolder3D + "sphere");
            SphereModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = SphereEffect;
            
            //Bepu
            LoadPhysics();

            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            base.LoadContent();
        }

        private void UpdateCamera()
        {
            var sphereBackDirection = -Vector3.Transform(Vector3.Forward, SphereRotationMatrix);
        
            var orbitalPosition = sphereBackDirection * CAMERA_FOLLOW_RADIUS;

            var upDistance = Vector3.Up * CAMERA_UP_DISTANCE;


            Camera.Position = SpherePosition + orbitalPosition + upDistance;

            Camera.TargetPosition = SpherePosition;

            Camera.BuildView();
        }
        protected override void Update(GameTime gameTime)
        {


            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            MovementManager(deltaTime);

            BodyReference body = Simulation.Bodies.GetBodyReference(SphereHandle);
            var prevLinearVelocity = body.Velocity.Linear.Y;
            var prevAngularVelocity = body.Velocity.Angular.Y;

            
            
            if(SpherePosition == Vector3.Clamp(SpherePosition, new Vector3(1673,9.9f,6439), new Vector3(1768,10,6517)) && FinalBossEnabled)
            {
                BossSphereHandle = Loader.LoadFinalBoss();
                FinalBossEnabled = false;
                FinalBossStage = true;
            }
            
            
            
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

            if(MathHelper.Distance(bodyRef.Velocity.Linear.Y, prevLinearVelocity) < 0.5 
               && MathHelper.Distance(bodyRef.Velocity.Angular.Y, prevAngularVelocity) < 0.5) OnGround = true;

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

            SphereRotationMatrix = Matrix.CreateRotationY(Mouse.GetState().X*-0.01f);
            SphereFrontDirection = Vector3.Transform(Vector3.Backward, SphereRotationMatrix);
            var SphereLateralDirection = Vector3.Transform(Vector3.Right, SphereRotationMatrix);

            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                if(!PelotaEstaEnElSuelo())
                    bodyRef.ApplyLinearImpulse(ToNumericVector3(-SphereFrontDirection * JUMPING_SPEED));
                else
                     bodyRef.ApplyLinearImpulse(ToNumericVector3(-SphereFrontDirection * LINEAR_SPEED));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) && !PelotaSeCayo())
            {
                //SphereVelocity += SphereFrontDirection * LINEAR_SPEED;
                bodyRef.ApplyLinearImpulse(ToNumericVector3(SphereFrontDirection * LINEAR_SPEED));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(ToNumericVector3(-SphereLateralDirection * LINEAR_SPEED*0.5f));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(ToNumericVector3(SphereLateralDirection * LINEAR_SPEED*0.5f));
            }
            
            AdministrarSalto(deltaTime);
        }
        protected void AdministrarSalto(float deltaTime){
            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            if ((Keyboard.GetState().IsKeyDown(Keys.Space)|| Keyboard.GetState().IsKeyDown(Keys.Up))&& PelotaEstaEnElSuelo())
            {
                //SphereVelocity += Vector3.Up * SALTO_BUFFER_VALUE;
                bodyRef.ApplyLinearImpulse(ToNumericVector3(Vector3.Up * SALTO_BUFFER_VALUE));
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
            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;

            //TODO why I have to set 1 in the alpha channel in the fx file?
            SkyBox.Draw(Camera.View, Camera.Projection, Camera.Position);

            GraphicsDevice.RasterizerState = originalRasterizerState;

            SphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);
            SimpleColor.Parameters["eyePosition"]?.SetValue(Camera.Position);
            SimpleColor.Parameters["Tiling"]?.SetValue(Vector2.One);

            //Dibujo el suelo
            foreach(StaticObstacle obstacle in StaticObstacles)   {obstacle.Render(SimpleColor,gameTime);}
             //Dibujo los cilindros
            foreach(MovingObstacle obstacle in MovingObstacles)    {obstacle.Render(SimpleColor,gameTime);}
            foreach(PeriodicObstacle obstacle in PeriodicObstacles)    {obstacle.Render(SimpleColor,gameTime);}

            var worldView = SphereWorld * Camera.View;
            SphereEffect.Parameters["matWorld"].SetValue(SphereWorld);
            SphereEffect.Parameters["matWorldViewProj"].SetValue(worldView * Camera.Projection);
            SphereEffect.Parameters["matInverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(SphereWorld)));
            SphereModel.Meshes.FirstOrDefault().Draw();

            
            if(FinalBossStage)
            {
            var pose = Simulation.Bodies.GetBodyReference(BossSphereHandle).Pose;
            var bossWorldView = Matrix.CreateScale(40) * Matrix.CreateFromQuaternion(pose.Orientation) * Matrix.CreateTranslation(pose.Position) * Camera.View;
            SphereEffect.Parameters["matWorld"].SetValue(bossWorldView);
            SphereEffect.Parameters["matWorldViewProj"].SetValue(bossWorldView * Camera.Projection);
            SphereEffect.Parameters["matInverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(bossWorldView)));
            SphereModel.Meshes.FirstOrDefault().Draw();
            }


            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            var position= new Vector3(MathF.Round(SpherePosition.X,1), MathF.Round(SpherePosition.Y,1), MathF.Round(SpherePosition.Z,1));
            SpriteBatch.DrawString(SpriteFont, "Position:" + position.ToString(), new Vector2(GraphicsDevice.Viewport.Width - 500, 0), Color.White);
            SpriteBatch.End();

            //InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * Matrix.CreateRotationX(-MathHelper.PiOver2)*Matrix.CreateRotationY(-MathHelper.PiOver2)*TrackWorld* Matrix.CreateTranslation(864.1f,100f,415f) ,Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }

/*
        private void DrawCoin(float x, float y, float z){
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x + 1f, y + 1f, z + 1f), 1f, 0, MathHelper.PiOver2);
        }
*/

         public static NumericVector3 ToNumericVector3(Vector3 v)
        {
            return new NumericVector3(v.X, v.Y, v.Z);
        }
        public static System.Numerics.Quaternion ToSysNumQuaternion(Quaternion v)
        {
            return new System.Numerics.Quaternion(v.X, v.Y, v.Z,v.W);
        }

        private void LoadPhysics()
        {
            BufferPool = new BufferPool();
            BepuSphere = new Sphere(5f);

             var targetThreadCount = Math.Max(1,
                Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

             Simulation = Simulation.Create(BufferPool,
                new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new NumericVector3(0, GRAVITY, 0)),
                new SolveDescription(8, 1));

            Loader = new Loader(Simulation,GraphicsDevice,Camera);
            StaticObstacles = Loader.LoadStatics();
            MovingObstacles=  Loader.LoadKinematics();
            PeriodicObstacles = Loader.LoadPeriodics();


            var bodyDescription = BodyDescription.CreateConvexDynamic(ToNumericVector3(SpherePosition), 5f,
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

        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();
            base.UnloadContent();
        }

        private void CheckpointManager()
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);
            if(PelotaSeCayo())
            {
                bodyRef.Pose.Position = ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
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
    

        private void ObstacleContact()
        {
            var boundingSphere = new BepuUtilities.BoundingSphere(ToNumericVector3(SpherePosition), 5f);
            for(int i=1; i<4; i++)
            {
                if(Simulation.Bodies.GetBodyReference(PeriodicObstacles[i].BodyHandle).BoundingBox.Intersects(
                     ref boundingSphere))
                    {
                        Simulation.Bodies.GetBodyReference(SphereHandle).Pose.Position = ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
                        FinalBossStage = false;
                        break;
                    }
            }


            var bossBoundingSphere = new BoundingSphere(ToNumericVector3(Simulation.Bodies.GetBodyReference(BossSphereHandle).Pose.Position), 40f);
            if(bossBoundingSphere.Intersects(new BoundingSphere(ToNumericVector3(SpherePosition), 5f)))
                {
                    Simulation.Bodies.GetBodyReference(SphereHandle).Pose.Position = ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
                    FinalBossEnabled = true;
                    FinalBossStage = false;
                }
        }
    }

}