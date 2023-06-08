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


        private const float LINEAR_SPEED= 12000f;
        private const float CAMERA_FOLLOW_RADIUS = 70f;
        private const float CAMERA_UP_DISTANCE = 30f;
        private const float SALTO_BUFFER_VALUE = 12000f;
        private const float GRAVITY = -350f;

        //CHECKPOINTS DEBE ESTAR ORDENADO ASCENDENTEMENTE
        //EL PRIMER VALOR DEBE SER LA POSICION INICIAL DE LA ESFERA
        private Vector3[] CHECKPOINTS={new Vector3(0, 10.001f, 0)};
        private const float COORDENADA_Y_MAS_BAJA = -80f;
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Effect Effect { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        private SpherePrimitive Sphere { get; set; }
        private Matrix SphereRotationMatrix { get; set; }
        private CubePrimitive ObstacleBox { get; set; }
        private CubePrimitive YellowBox { get; set; }
        private Vector3 SpherePosition { get; set; }
        private TargetCamera Camera { get; set; }

        private Matrix[] PowerUpsWorld { get; set;}
        private BoundingSphere SphereCollider;
        private Vector3 SphereVelocity { get; set; }
        private bool OnGround { get; set; }
        private Vector3 SphereFrontDirection { get; set; }
        private Matrix SphereScale {get; set; }
        private CubePrimitive LightBox { get; set; }
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

            OnGround = true;
        
            // Esfera
            Sphere = new SpherePrimitive(GraphicsDevice);
            SpherePosition = new Vector3(3100,94,1700);
            SphereWorld = Matrix.CreateTranslation(SpherePosition);
            SphereVelocity = Vector3.Zero;
            SphereFrontDirection =  Vector3.Backward;
            SphereScale = Matrix.CreateScale(0.3f);

            SphereRotationMatrix = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);
         

              PowerUpsWorld = new Matrix[]
            {
                Matrix.CreateScale(40, 10f, 80) * Matrix.CreateTranslation(new Vector3(2720,20f,435f))
            };            

            YellowBox = new CubePrimitive(GraphicsDevice, 1f, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen);
            ObstacleBox = new CubePrimitive(GraphicsDevice,1f,Color.Blue);

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
            LightBox = new CubePrimitive(GraphicsDevice, 1f, Color.White);
            Effect = Content.Load<Effect>(ContentFolderEffects + "BlinnPhongTypes");
            Effect.Parameters["lightPosition"].SetValue(LightPosition);
            Effect.Parameters["ambientColor"]?.SetValue(new Vector3(1f, 1f, 1f));
            Effect.Parameters["diffuseColor"]?.SetValue(new Vector3(1f, 1f, 1f));
            Effect.Parameters["specularColor"]?.SetValue(new Vector3(1,1,1));
            Effect.Parameters["KAmbient"]?.SetValue(0.3f);
            Effect.Parameters["KDiffuse"]?.SetValue(0.7f);
            Effect.Parameters["KSpecular"]?.SetValue(0.4f);
            Effect.Parameters["shininess"]?.SetValue(10f);
            
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


        protected override void Update(GameTime gameTime)
        {
            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            MovementManager(deltaTime);

            BodyReference body = Simulation.Bodies.GetBodyReference(SphereHandle);
            var prevLinearVelocity = body.Velocity.Linear.Y;
            var prevAngularVelocity = body.Velocity.Angular.Y;

            body.ApplyLinearImpulse(ToNumericVector3(SphereVelocity));
            //body.ApplyAngularImpulse(ToNumericVector3(SphereVelocity))

            Simulation.Timestep(1 / 60f, ThreadDispatcher);

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            if (PelotaSeCayo()){
                VolverAlUltimoCheckpoint();
            }

            var pose = Simulation.Bodies.GetBodyReference(SphereHandle).Pose;
            SpherePosition = pose.Position;
            var quaternion = pose.Orientation;
            var world = Matrix.CreateScale(BepuSphere.Radius*2) *
                            Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                                quaternion.W)) *
                            Matrix.CreateTranslation(new Vector3(SpherePosition.X, SpherePosition.Y, SpherePosition.Z));
            SphereWorld = world;
            SpherePosition = pose.Position;

            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);

            if(MathHelper.Distance(bodyRef.Velocity.Linear.Y, prevLinearVelocity) < 0.5 
               && MathHelper.Distance(bodyRef.Velocity.Angular.Y, prevAngularVelocity) < 0.5) OnGround = true;

            SphereVelocity = new Vector3(0f,0f,0f);
           
            UpdateCamera();

            base.Update(gameTime);
        }     
        protected void MovementManager(float deltaTime){

            SphereRotationMatrix = Matrix.CreateRotationY(Mouse.GetState().X*-0.005f);
            SphereFrontDirection = Vector3.Transform(Vector3.Backward, SphereRotationMatrix);

            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                SphereVelocity -= SphereFrontDirection * LINEAR_SPEED;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S) && !PelotaSeCayo())
            {
                SphereVelocity += SphereFrontDirection * LINEAR_SPEED;
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
            if ((Keyboard.GetState().IsKeyDown(Keys.Space)|| Keyboard.GetState().IsKeyDown(Keys.Up)&& PelotaEstaEnElSuelo() ))
            {
                SphereVelocity += Vector3.Up * SALTO_BUFFER_VALUE;
                OnGround = false;
            }  
        }

        protected bool PelotaEstaEnElSuelo(){
            return OnGround;
        }

        protected bool PelotaSeCayo(){
            return SpherePosition.Y < (COORDENADA_Y_MAS_BAJA - 50f);
        }

        protected void VolverAlUltimoCheckpoint(){
            //Reconoce el último checkpoint por el valor de coordenada X
            //más cercano y menor a la coordenada X de la posición actual de la esfera

            //Supone que CHECKPOINT esta en orden ascendente
            bool found = false;

            for(int i = 0; i < CHECKPOINTS.Length; i++){
                if (CHECKPOINTS[i].X <= SpherePosition.X){
                    SphereCollider.Center = CHECKPOINTS[i];
                    found = true;
                }
            }
            //Si se cae atras del primer checkpoint
            if (!found) SphereCollider.Center = CHECKPOINTS[0];
        }
        
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            Effect.Parameters["eyePosition"]?.SetValue(Camera.Position);
            Effect.Parameters["Tiling"]?.SetValue(Vector2.One);

            //Dibujo el suelo
            foreach(Obstacle obstacle in StaticObstacles)   {obstacle.Render(Effect,gameTime);}
             //Dibujo los cilindros
            foreach(Obstacle obstacle in KinematicObstacles)    {obstacle.Render(Effect,gameTime);}
            foreach(Obstacle obstacle in PeriodicObstacles)    {obstacle.Render(Effect,gameTime);}

            for (int i = 0; i < PowerUpsWorld.Length; i++)
            {
                var matrix = PowerUpsWorld[i];
                DrawGeometricPrimitive(matrix, YellowBox);
            }

            DrawGeometricPrimitive(SphereWorld, Sphere);

            //InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * Matrix.CreateRotationX(-MathHelper.PiOver2)*Matrix.CreateRotationY(-MathHelper.PiOver2)*TrackWorld* Matrix.CreateTranslation(864.1f,100f,415f) ,Camera.View, Camera.Projection);

            base.Draw(gameTime);
        }

/*
        private void DrawCoin(float x, float y, float z){
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x + 1f, y + 1f, z + 1f), 1f, 0, MathHelper.PiOver2);
        }
*/
        private void DrawGeometricPrimitive(Matrix World, GeometricPrimitive geometricPrimitive){
            var viewProjection = Camera.View * Camera.Projection;
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));
            Effect.Parameters["WorldViewProjection"]?.SetValue(World * viewProjection);
            geometricPrimitive.Draw(Effect);
        }

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

            var position = ToNumericVector3(SpherePosition);
            var bodyDescription = BodyDescription.CreateConvexDynamic(position,
                    new BodyVelocity(ToNumericVector3(SphereVelocity)),
                    600f, Simulation.Shapes, BepuSphere);
            bodyDescription.Collidable.Continuity = ContinuousDetection.Continuous(1e-4f, 1e-4f);
            bodyDescription.Activity.SleepThreshold=-1;
            var bodyHandle = Simulation.Bodies.Add(bodyDescription);
            SphereHandle = bodyHandle;
        }
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();
            base.UnloadContent();
        }
    }
}