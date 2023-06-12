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
        private const float CAMERA_FOLLOW_RADIUS = 70f;
        private const float CAMERA_UP_DISTANCE = 30f;
        private  float SALTO_BUFFER_VALUE = 1000f;
        private const float GRAVITY = -350f;
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

        private List<Obstacle> PeriodicObstacles { get; set; }
        private List<Obstacle> StaticObstacles {get;set;} 
        private List<Obstacle> KinematicObstacles { get; set; }

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
            SpherePosition =new Vector3(3050,95,2245);
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
                new Checkpoint(new Vector3(1720,10,3175)),
                new Checkpoint(new Vector3(1700,60,4800)),
            };
            CurrentCheckpoint = 0;

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

            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);
         

              PowerUpsWorld = new Matrix[]
            {
                Matrix.CreateScale(40, 10f, 80) * Matrix.CreateTranslation(new Vector3(2720,20f,435f))
            };            


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
           /*
            InclinedTrackBox.Center = (TrackWorld* Matrix.CreateTranslation(864.1f,100f,415f)).Translation;
            // Then set its orientation!
            InclinedTrackBox.Orientation =Matrix.CreateRotationX(-MathHelper.PiOver2)*Matrix.CreateRotationY(-MathHelper.PiOver2);    
*/

            //Luz
            SimpleColor = Content.Load<Effect>(ContentFolderEffects + "ColorShader");
            SimpleColor.Parameters["lightPosition"].SetValue(LightPosition);
            SimpleColor.Parameters["ambientColor"]?.SetValue(new Vector3(1f, 1f, 1f));
            SimpleColor.Parameters["diffuseColor"]?.SetValue(new Vector3(1f, 1f, 1f));
            SimpleColor.Parameters["specularColor"]?.SetValue(new Vector3(1,1,1));

            SimpleColor.Parameters["KAmbient"]?.SetValue(0.3f);
            SimpleColor.Parameters["KDiffuse"]?.SetValue(0.7f);
            SimpleColor.Parameters["KSpecular"]?.SetValue(0.4f);
            SimpleColor.Parameters["shininess"]?.SetValue(10f);
            var texture = Content.Load<Texture2D>(ContentFolderTextures +"tiles/" +"color");
            var normal = Content.Load<Texture2D>(ContentFolderTextures + "tiles/" + "normal");
            SimpleColor.Parameters["ModelTexture"].SetValue(texture);
            SimpleColor.Parameters["NormalTexture"].SetValue(normal);

            InitializeLights();
            InitializeEffect();
            LoadTextures();
            SphereModel = Content.Load<Model>(ContentFolder3D + "sphere");
            SphereModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = SphereEffect;
            
            //Bepu
            LoadPhysics();
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
        private bool canDraw = true;
        protected override void Update(GameTime gameTime)
        {


            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            MovementManager(deltaTime);

            BodyReference body = Simulation.Bodies.GetBodyReference(SphereHandle);
            var prevLinearVelocity = body.Velocity.Linear.Y;
            var prevAngularVelocity = body.Velocity.Angular.Y;
            CheckpointManager();

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

            if(SpherePosition.X > 3000 && SpherePosition.Z > 2365 && canDraw)
            {
                KinematicObstacles.AddRange(new Loader(Simulation,GraphicsDevice, Camera).LoadRollingCylinders());
                canDraw=false;
            }
                

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
                bodyRef.ApplyLinearImpulse(ToNumericVector3(-SphereLateralDirection * LINEAR_SPEED));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(ToNumericVector3(SphereLateralDirection * LINEAR_SPEED));
            }
            
            AdministrarSalto(deltaTime);
        }
/*
        private void SolvePowerUps(BoundingBox boxCollider)
        {       
            bool isApowerUp = false;

            for(int i = 0; i < PowerUpsWorld.Length; i++){
                if(PowerUpBoxes[i] == boxCollider)
                    {
                        isApowerUp = true;
                        break;
                    }
            }

            if(isApowerUp)
                SphereVelocity -= SphereFrontDirection * LINEAR_SPEED * 5; 
        }    
*/
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

            SphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);
            SimpleColor.Parameters["eyePosition"]?.SetValue(Camera.Position);
            SimpleColor.Parameters["Tiling"]?.SetValue(Vector2.One);

            //Dibujo el suelo
            foreach(Obstacle obstacle in StaticObstacles)   {obstacle.Render(SimpleColor,gameTime);}
             //Dibujo los cilindros
            foreach(Obstacle obstacle in KinematicObstacles)    {obstacle.Render(SimpleColor,gameTime);}
            foreach(Obstacle obstacle in PeriodicObstacles)    {obstacle.Render(SimpleColor,gameTime);}

            var worldView = SphereWorld * Camera.View;
            SphereEffect.Parameters["matWorld"].SetValue(SphereWorld);
            SphereEffect.Parameters["matWorldViewProj"].SetValue(worldView * Camera.Projection);
            SphereEffect.Parameters["matInverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(SphereWorld)));
            SphereModel.Meshes.FirstOrDefault().Draw();

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

            var loader = new Loader(Simulation,GraphicsDevice,Camera);
            StaticObstacles = loader.LoadStatics();
            KinematicObstacles=  loader.LoadKinematics();
            PeriodicObstacles = loader.LoadPeriodics();


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
    }
}