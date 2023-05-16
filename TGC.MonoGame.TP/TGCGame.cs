using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Collisions;


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


        private const float TAMANIO_CUBO = 10f;
        private const float LINEAR_SPEED= 100f;
        private const float ANGULAR_SPEED = 3f;
        private const float CAMERA_FOLLOW_RADIUS = 70f;
        private const float CAMERA_UP_DISTANCE = 30f;
        private const float CYLINDER_HEIGHT = 10F;
        private const float CYLINDER_DIAMETER = 10f * TAMANIO_CUBO;

        private const float SALTO_BUFFER_VALUE = 100f;

        private const float SALTO_BUFFER_DECREMENT_ALPHA = 25f;

        private const float GRAVITY = 200f;
        private Vector3 SPHERE_INITIAL_POSITION = new Vector3(1235f,30f,415f);

        //CHECKPOINTS DEBE ESTAR ORDENADO ASCENDENTEMENTE
        //EL PRIMER VALOR DEBE SER LA POSICION INICIAL DE LA ESFERA
        private Vector3[] CHECKPOINTS={new Vector3(0, 9.99f, 0)};
        private const float COORDENADA_Y_MAS_BAJA = -80f;
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Effect Effect { get; set; }
        private float Rotation { get; set; }
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        private SpherePrimitive Sphere { get; set; }
        private Matrix SphereRotationMatrix { get; set; }
        private CubePrimitive Box { get; set; }
        private CubePrimitive ObstacleBox { get; set; }
        private CubePrimitive WallBox { get; set; }
        private CubePrimitive WhiteBox { get; set; }
        private CubePrimitive  BlackBox { get; set; }
        private CubePrimitive CyanBox { get; set; }
        private CubePrimitive YellowBox { get; set; }
        private CylinderPrimitive Cylinder { get; set; }
        private CylinderPrimitive SmallCylinder { get; set; }
        private Vector3 BoxPosition { get; set; }
        private Vector3 SpherePosition { get; set; }
        private TargetCamera Camera { get; set; }

        private Model InclinedTrackModel { get; set; }
        private Matrix TrackWorld { get; set; }
        private float SaltoBuffer { get; set;}
        private Matrix[] GroundWorld { get; set; }
        private Matrix[] WallsWorld { get; set; }
        private Vector3[] BasicCylindersMeasures{get; set;}
        private Vector3[] BasicCylindersPositions{get; set;}
        private Vector3[] BasicCylindersRotation { get; set;}
        private BoundingCylinder[] CollidersCylinders { get; set; }
        private BoundingBox[] CollidersBoxes { get; set; }
        private BoundingBox[] PowerUpBoxes{ get; set; }
        private Matrix[] PowerUpsWorld { get; set;}
        private BoundingCylinder SphereCollider { get; set; }
        private Vector3 SphereVelocity { get; set; }
        private Vector3 SphereAcceleration { get; set; }
        private bool OnGround { get; set; }
        private Vector3 SphereFrontDirection { get; set; }
        private Matrix SphereScale {get; set; }
        private float time { get; set; } 
        private CubePrimitive LightBox { get; set; }
        private Vector3 LightPosition { get; set; } = new Vector3 (0,2500,0);
        private Matrix[] CylindersWorldAsBoxes { get; set; } 
        private SkyBox SkyBox { get; set; }
        private Matrix SkyBoxView { get; set; }
        private Matrix SkyBoxProjection { get; set; }
        private Matrix SkyBoxWorld = Matrix.Identity;
        private float SkyBoxAngel = 0.0f;
        private float SkyBoxDistance = 50;
        private Vector3 ViewVector { get; set; }
        private Vector3 CameraPosition { get; set; }
        private float DeltaX {get; set;} = 0;
        private MouseState prevMouseState = Mouse.GetState();

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
            Sphere = new SpherePrimitive(GraphicsDevice, 10);
            SpherePosition = SPHERE_INITIAL_POSITION;
            SphereCollider = new BoundingCylinder(SpherePosition, 2f, 5f);
            SphereVelocity = Vector3.Zero;
            SphereAcceleration = Vector3.Down * GRAVITY;
            SphereFrontDirection =  Vector3.Backward;
            SphereScale = Matrix.CreateScale(0.3f);

            // Configuramos nuestras matrices de la escena.
            Rotation=0;
            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);
            time = 0.0f;

            GroundWorld = new Matrix [] 
            {
                Matrix.CreateScale(150, 10, 100) * Matrix.CreateTranslation(Vector3.Zero),
                Matrix.CreateScale(500,100,500) * Matrix.CreateTranslation(600f,-60,4.5f),
                Matrix.CreateScale(500,100,500) * Matrix.CreateTranslation(700f,60,4.5f),
                Matrix.CreateScale(150,10f,40) * Matrix.CreateTranslation(new Vector3(150, 0f, 2f)),
                Matrix.CreateScale(140,10f,380) * Matrix.CreateTranslation(new Vector3(800f,100f,245f)),
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
        
            };

            PowerUpsWorld = new Matrix[]
            {
                Matrix.CreateScale(40, 10f, 80) * Matrix.CreateTranslation(new Vector3(2720,20f,435f))
            };

            WallsWorld = new Matrix[]
            {
                Matrix.CreateScale(10,10f,30) * Matrix.CreateTranslation(new Vector3(1360f,30f,410f)),
                Matrix.CreateScale(10,10f,30f) * Matrix.CreateTranslation(new Vector3(1565f,30f,435f)),
                Matrix.CreateScale(10,10f, 80f) * Matrix.CreateTranslation(new Vector3(2565f,30f,435f)),
                //Muro alto
                Matrix.CreateScale(40, 40f,10f) * Matrix.CreateTranslation(new Vector3(3090f,45f,510f)),
                Matrix.CreateScale(40, 40f,10f) * Matrix.CreateTranslation(new Vector3(3080f,45f,670f)),
                Matrix.CreateScale(40, 40f,10f) * Matrix.CreateTranslation(new Vector3(3060f,45f,590f)),
                //Paredes que se mueven
                Matrix.CreateScale(10, 50f,40f) * Matrix.CreateTranslation(new Vector3(40*MathF.Cos(5*time)+1720,40,2800)),
                Matrix.CreateScale(10, 50f,40f) * Matrix.CreateTranslation(new Vector3(-40*MathF.Cos(5*time)+1720,40,2900)),
                Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(new Vector3(40*MathF.Cos(5*time)+1720,40,3000)),
                Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(new Vector3(-40*MathF.Cos(5*time)+1720,40,3100)),
                Matrix.CreateScale(10,50,40) * Matrix.CreateTranslation(new Vector3(40*MathF.Cos(5*time)+1720,40,3200)),

                //muros
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3320)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 3460)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1700,25,3895)),
                Matrix.CreateScale(40,40,10) * Matrix.CreateTranslation(new Vector3(1740, 25, 4020)),
                Matrix.CreateScale(80,10,10) * Matrix.CreateTranslation(new Vector3(1720, 10, 7825)),

                //Plataformas que se mueven
                //Se tendrían que mover con time
                Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1740,0,200*MathF.Cos(2*time)+5230)),
                Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1690,0,200*MathF.Cos(2*time+MathHelper.Pi)+5600)),
                Matrix.CreateScale(40,10,80) * Matrix.CreateTranslation(new Vector3(1740,0,200*MathF.Cos(2*time)+6010)),

            };

            
            BasicCylindersMeasures = new Vector3[]
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
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
                new Vector3(60, 10, 18),
            };

            BasicCylindersPositions = new Vector3[]
            {
                new Vector3(300,0f,4.5f),
                new Vector3(400, 0f,4.5f),
                new Vector3(500, 0, 4.5f),
                new Vector3(400, 10, 4.5f ),
                new Vector3(500, 10, 4.5f ),
                new Vector3(300, 10, 4.5f ),

                //Islas
                new Vector3(1850,20,404),
                new Vector3(1890,20,434.5f),
                new Vector3(1930,20,454.5f),
                new Vector3(1980,20,454.5f),
                new Vector3(2030,20,434.5f),
                new Vector3(2080,20,404.5f),
                new Vector3(2130,20,414.5f),

                //Cilindros que Giran  
                new Vector3(3100,100,1775),
                new Vector3(3050,100,1820),
                new Vector3(3100,100,1870),
                new Vector3(3050,100,1920),
            };

            BasicCylindersRotation = new Vector3[]
            {
                //Los cambio a cero para probar
                new Vector3(0, 0.0f, 0.0f),
                new Vector3(0, 0.0f, 0.0f),
                new Vector3(0, 0.0f, 0.0f),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),

                //Islas
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),

                //Cilindros Que Giran
                //Los cambio a cero para probar..
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
                new Vector3(0,0,0),
            };

            CylindersWorldAsBoxes = new Matrix [BasicCylindersPositions.Length];

            for (int i = 0; i < BasicCylindersPositions.Length; i++)
            {
                CylindersWorldAsBoxes[i] = 
                   Matrix.CreateScale(BasicCylindersMeasures[i].Y, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y)
                    * Matrix.CreateTranslation(BasicCylindersPositions[i]); 
            }
                
            //Create bounding boxes
            int boxesLength = GroundWorld.Length + WallsWorld.Length + PowerUpsWorld.Length + CylindersWorldAsBoxes.Length;
            var lengthBeforeCylinders = GroundWorld.Length + WallsWorld.Length + PowerUpsWorld.Length;
            CollidersBoxes = new BoundingBox[boxesLength];
            CollidersCylinders = new BoundingCylinder[BasicCylindersPositions.Length];

            for(int i = 0; i < GroundWorld.Length; i++)
                CollidersBoxes[i] =  BoundingVolumesExtensions.FromMatrix(GroundWorld[i]);

            for(int i = GroundWorld.Length; i< lengthBeforeCylinders - PowerUpsWorld.Length; i++)
                CollidersBoxes[i] = BoundingVolumesExtensions.FromMatrix(WallsWorld[i - GroundWorld.Length]);

            for(int i = lengthBeforeCylinders - PowerUpsWorld.Length; i < lengthBeforeCylinders; i++)
                CollidersBoxes[i] = BoundingVolumesExtensions.FromMatrix(PowerUpsWorld[i - lengthBeforeCylinders + PowerUpsWorld.Length]);

            for(int i = lengthBeforeCylinders; i < boxesLength; i++)
                CollidersBoxes[i] = BoundingVolumesExtensions.FromMatrix(CylindersWorldAsBoxes[i + CylindersWorldAsBoxes.Length - boxesLength]);

            PowerUpBoxes = new BoundingBox[PowerUpsWorld.Length]; 

            for(int i = 0; i< PowerUpsWorld.Length; i++)
                PowerUpBoxes[i] = BoundingVolumesExtensions.FromMatrix(PowerUpsWorld[i]);

            for (int i = 0; i < BasicCylindersPositions.Length; i++)
            {
                CollidersCylinders[i] = new BoundingCylinder(BasicCylindersPositions[i],BasicCylindersMeasures[i].Y/2, BasicCylindersMeasures[i].X/2);
            }


            // Cubo
            Box = new CubePrimitive(GraphicsDevice, 1f, Color.MonoGameOrange, Color.MonoGameOrange, Color.MonoGameOrange,
            Color.MonoGameOrange, Color.MonoGameOrange, Color.MonoGameOrange);
            ObstacleBox = new CubePrimitive(GraphicsDevice, 1f, Color.BlueViolet,Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet);
            //BoxPosition = Vector3.Zero;
            WallBox = new CubePrimitive(GraphicsDevice, 1f, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet);
            WhiteBox = new CubePrimitive(GraphicsDevice, 1f, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White);
            BlackBox = new CubePrimitive(GraphicsDevice, 1f, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);
            CyanBox = new CubePrimitive(GraphicsDevice, 1f, Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan);
            YellowBox = new CubePrimitive(GraphicsDevice, 1f, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen);
            //Cilindro
            Cylinder = new CylinderPrimitive(GraphicsDevice, CYLINDER_HEIGHT, CYLINDER_DIAMETER, 18);
            SmallCylinder = new CylinderPrimitive(GraphicsDevice, CYLINDER_HEIGHT *2, CYLINDER_DIAMETER/10, 18);    
            TrackWorld= Matrix.CreateRotationX(-MathHelper.PiOver2)*Matrix.CreateRotationY(-MathHelper.PiOver2)*Matrix.CreateTranslation(Vector3.Zero);

            //SkyBox 
            SkyBoxView = Matrix.CreateLookAt(new Vector3(20,0,0), Vector3.Zero, Vector3.UnitY);
            SkyBoxProjection = 
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 800f / 600f, 0.1f, 100f);

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
            InclinedTrackModel = Content.Load<Model>(ContentFolder3D + "rampa");        

            //SkyBox
            var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "skyBox");
            
            SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, 300);



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


        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>

        protected override void Update(GameTime gameTime)
        {
            CameraPosition = 20 * new Vector3((float)Math.Sin(SkyBoxAngel), 0, (float)Math.Cos(SkyBoxAngel));
            SkyBoxView = Matrix.CreateLookAt(CameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);
    
            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            var totalTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            ModificarParametrosObjetosMoviles(deltaTime, totalTime); 

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            if (PelotaSeCayo()){
                VolverAlUltimoCheckpoint();
            }


            MovementManager(deltaTime);

            UpdateCamera();

            base.Update(gameTime);
        }     

        protected void ModificarParametrosObjetosMoviles(float deltaTime, float totalTime){
            GroundWorld[1] = Matrix.CreateScale(50,10,50) * Matrix.CreateTranslation(600f,+ 70*MathF.Cos(3*totalTime)-60,4.5f);
            CollidersBoxes[1] = BoundingVolumesExtensions.FromMatrix(GroundWorld[1]);
            GroundWorld[2] = Matrix.CreateScale(50,10,50)* Matrix.CreateTranslation(700f, 60-70*MathF.Cos(3*totalTime),4.5f);
            CollidersBoxes[2] = BoundingVolumesExtensions.FromMatrix(GroundWorld[2]);
            
        }

        protected void MovementManager(float deltaTime){

            if (Keyboard.GetState().IsKeyDown(Keys.W) && !PelotaSeCayo())
            {
                SphereVelocity -= SphereFrontDirection * LINEAR_SPEED;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S) && !PelotaSeCayo())
            {
                SphereVelocity += SphereFrontDirection * LINEAR_SPEED;
            }
            
            var mouseState = Mouse.GetState();
            var DeltaX = mouseState.X;

            Rotation = - DeltaX * 0.005f;
            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);
            SphereFrontDirection = Vector3.Transform(Vector3.Backward, SphereRotationMatrix);
           
            AdministrarSalto(deltaTime);

            SphereVelocity += SphereAcceleration * deltaTime;

            var scaledVelocity= SphereVelocity * deltaTime;

            SolveVerticalBoxesMovement(scaledVelocity);
            //SolveVerticalCylinderMovement(scaledVelocity);

            scaledVelocity = new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            SolveHorizontalBoxesCollisions(scaledVelocity);
            //SolveHorizontalCylinderPosition(scaledVelocity);
            
            SpherePosition = SphereCollider.Center;
            SphereVelocity = new Vector3(0f, SphereVelocity.Y, 0f);
        }

        private void SolveVerticalBoxesMovement(Vector3 scaledVelocity)
        {
             // If the Robot has vertical velocity
            if (scaledVelocity.Y == 0f)
                return;

            // Start by moving the Cylinder
            SphereCollider.Center += Vector3.Up * scaledVelocity.Y;
            // Set the OnGround flag on false, update it later if we find a collision
            OnGround = false;


            // Collision detection
            var collided = false;
            var foundIndex = -1;
            for (var index = 0; index < CollidersBoxes.Length; index++)
            {
                if (!SphereCollider.Intersects(CollidersBoxes[index]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;
                
                // If we collided with something, set our velocity in Y to zero to reset acceleration
                SphereVelocity = new Vector3(SphereVelocity.X, 0f, SphereVelocity.Z);

                // Set our index and collision flag to true
                // The index is to tell which collider the Robot intersects with
                collided = true;
                foundIndex = index;
                break;
            }


            // We correct based on differences in Y until we don't collide anymore
            // Not usual to iterate here more than once, but could happen
            while (collided)
            {
                var collider = CollidersBoxes[foundIndex];
                var colliderY = BoundingVolumesExtensions.GetCenter(collider).Y;
                var cylinderY = SphereCollider.Center.Y;
                var extents = BoundingVolumesExtensions.GetExtents(collider);

                float penetration;
                // If we are on top of the collider, push up
                // Also, set the OnGround flag to true
                if (cylinderY > colliderY)
                {
                    penetration = colliderY + extents.Y - cylinderY + SphereCollider.HalfHeight;
                    OnGround = true;
                }

                // If we are on bottom of the collider, push down
                else
                    penetration = -cylinderY - SphereCollider.HalfHeight + colliderY - extents.Y;

                // Move our Cylinder so we are not colliding anymore
                SolvePowerUps(collider);
                SphereCollider.Center += Vector3.Up * penetration;
                collided = false;
                
                // Check for collisions again
                for (var index = 0; index < CollidersBoxes.Length; index++)
                {
                    if (!SphereCollider.Intersects(CollidersBoxes[index]).Equals(BoxCylinderIntersection.Intersecting))
                        continue;

                    // Iterate until we don't collide with anything anymore
                    collided = true;
                    foundIndex = index;
                    break;
                }
            }
        }
        

        private void SolveHorizontalBoxesCollisions(Vector3 scaledVelocity)
        {
           
             // Has horizontal movement?
            if (Vector3.Dot(scaledVelocity, new Vector3(1f, 0f, 1f)) == 0f)
                return;
            
            // Start by moving the Cylinder horizontally
            SphereCollider.Center += new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            // Check intersection for every collider
            for (var index = 0; index < CollidersBoxes.Length; index++)
            {
                if (!SphereCollider.Intersects(CollidersBoxes[index]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;

                // Get the intersected collider and its center
                var collider = CollidersBoxes[index];
                var colliderCenter = BoundingVolumesExtensions.GetCenter(collider);

                // Get the cylinder center at the same Y-level as the box
                var sameLevelCenter = SphereCollider.Center;
                sameLevelCenter.Y = colliderCenter.Y;

                // Find the closest horizontal point from the box
                var closestPoint = BoundingVolumesExtensions.ClosestPoint(collider, sameLevelCenter);

                // Calculate our normal vector from the "Same Level Center" of the cylinder to the closest point
                // This happens in a 2D fashion as we are on the same Y-Plane
                var normalVector = sameLevelCenter - closestPoint;
                var normalVectorLength = normalVector.Length();

                // Our penetration is the difference between the radius of the Cylinder and the Normal Vector
                // For precission problems, we push the cylinder with a small increment to prevent re-colliding into the geometry
                var penetration = SphereCollider.Radius - normalVector.Length() + 0.00001f;

                // Push the center out of the box
                // Normalize our Normal Vector using its length first
                SphereCollider.Center += (normalVector / normalVectorLength * penetration);
            }
        }

    

        private void SolveHorizontalCylinderPosition(Vector3 scaledVelocity)
        {

            if(Vector3.Dot(scaledVelocity, new Vector3(1f,0f, 1f)) == 0f)
                return;

            SphereCollider.Center += new Vector3(scaledVelocity.X, 0f, scaledVelocity.Y);

            for (int i = 0; i < CollidersCylinders.Length; i++)
            {   
                if(!SphereCollider.Intersects(CollidersCylinders[i]).Equals(true))
                    continue;

                    var collider = CollidersCylinders[i];
                    var closestPoint = collider.ClosestPoint(SphereCollider.Center);

                    var sameLevelCenter = SphereCollider.Center;
                    sameLevelCenter.Y = collider.Center.Y;

                    var normalVector = sameLevelCenter - closestPoint;
                    var normalVectorLength = normalVector.Length();

                    var penetration = SphereCollider.Radius - normalVector.Length();

                    SphereCollider.Center += (normalVector / normalVectorLength * penetration);
                
            }
        }
        
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

        private void SolveVerticalCylinderMovement(Vector3 scaledVelocity)
        {
            
            SphereCollider.Center += Vector3.Up * scaledVelocity.Y;

            OnGround = false;

            var collided = false;
            var foundIndex = -1;
            for (var index = 0; index < CollidersCylinders.Length; index++)
            {   
                if (!SphereCollider.Intersects(CollidersCylinders[index]).Equals(true))
                    continue;
                
                // If we collided with something, set our velocity in Y to zero to reset acceleration
                SphereVelocity = new Vector3(SphereVelocity.X, 0f, SphereVelocity.Z);

                // Set our index and collision flag to true
                // The index is to tell which collider the Robot intersects with
                collided = true;
                foundIndex = index;
                break;
            }

            while (collided)
            {
                var collider = CollidersCylinders[foundIndex];
                var colliderY = collider.Center.Y;
                var cylinderY = SphereCollider.Center.Y;

                float penetration;

                // If we are on top of the collider, push up
                // Also, set the OnGround flag to true
                if (cylinderY > collider.HalfHeight + colliderY)
                {
                    penetration = (colliderY + cylinderY) * 2;
                    OnGround = true;
                }

                if(cylinderY + SphereCollider.HalfHeight < colliderY - collider.HalfHeight)
                {
                    penetration = -cylinderY;
                }
                // If we are on bottom of the collider, push down
                else
                    penetration = 0.00001f;

                // Move our Cylinder so we are not colliding anymore
                SphereCollider.Center += Vector3.Up * penetration;
                collided = false;

                // Check for collisions again
                for (var index = 0; index < CollidersCylinders.Length; index++)
                {
                if (!SphereCollider.Intersects(CollidersCylinders[index]).Equals(true))
                        continue;
                    // Iterate until we don't collide with anything anymore
                    collided = true;
                    foundIndex = index;
                    break;
                }
            }
        }
        

        protected void AdministrarSalto(float deltaTime){

            if ((Keyboard.GetState().IsKeyDown(Keys.Space)|| Keyboard.GetState().IsKeyDown(Keys.Up)) && PelotaEstaEnElSuelo() )
            {
                    SphereVelocity += Vector3.Up * SALTO_BUFFER_VALUE;
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
        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            SkyBox.Draw(SkyBoxView, SkyBoxProjection, CameraPosition);

            time += Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            Effect.Parameters["eyePosition"]?.SetValue(Camera.Position);
            Effect.Parameters["Tiling"]?.SetValue(Vector2.One);

            // Estos 3 parametros quedan fijos.
           // Effect.Parameters["View"].SetValue(Camera.View);
            //Effect.Parameters["Projection"].SetValue(Camera.Projection);

            //Dibujo el suelo
            for (int i = 0; i < GroundWorld.Length; i++)
            {
                // Get the World Matrix
                var matrix = GroundWorld[i];
                DrawGeometricPrimitive(matrix,Box);
            }

            for (int i = 0; i < WallsWorld.Length; i++)
            {
                var matrix = WallsWorld[i];
                DrawGeometricPrimitive(matrix, CyanBox);
            }

            for (int i = 0; i < PowerUpsWorld.Length; i++)
            {
                var matrix = PowerUpsWorld[i];
                DrawGeometricPrimitive(matrix, YellowBox);
            }

            //Dibujo los cilindros
            for (int i = 0; i < BasicCylindersPositions.Length; i++)
            {
                DrawGeometry(
                    new CylinderPrimitive(GraphicsDevice, BasicCylindersMeasures[i].X, BasicCylindersMeasures[i].Y, 32 ),
                    BasicCylindersPositions[i], BasicCylindersRotation[i].X, BasicCylindersRotation[i].Y, BasicCylindersRotation[i].Z);
            }

            DrawGeometry(Sphere, SpherePosition,0,0,0);

            InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * TrackWorld* Matrix.CreateTranslation(864.1f,100f,415f) ,Camera.View, Camera.Projection);

            //cilindros que giran
            //DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3100,100,1775), 3*CylinderYaw,0,MathHelper.PiOver2);
            //

            DrawRectangle(Box,20,10,20,new Vector3(1720,42.5f*MathF.Cos(3*time)+42.5f,2405));

            //Pared que aplastan contra el suelo
            DrawRectangle(ObstacleBox,80,10,80,new Vector3(1720,35*MathF.Cos(4*time)+45,4240));
            DrawRectangle(ObstacleBox,80,10,80,new Vector3(1720,35*MathF.Cos(4*time+MathHelper.PiOver2)+45,4430));
            DrawRectangle(ObstacleBox,80,10,80,new Vector3(1720,35*MathF.Cos(4*time+MathHelper.PiOver4)+45,4620));   

            base.Draw(gameTime);
        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw, float pitch, float roll)
        {
            var World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position);  
            DrawGeometricPrimitive(World, geometry);
        }

        private void DrawRectangle (CubePrimitive BoxType ,float length, float height, float width, Vector3 position){
            DrawGeometricPrimitive(Matrix.CreateScale(length,height, width) * Matrix.CreateTranslation(position.X, position.Y, position.Z ), BoxType);
        }

        private void DrawCoin(float x, float y, float z){
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x + 1f, y + 1f, z + 1f), 1f, 0, MathHelper.PiOver2);
        }

        private void DrawGeometricPrimitive(Matrix World, GeometricPrimitive geometricPrimitive){

            var viewProjection = Camera.View * Camera.Projection;
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));
            Effect.Parameters["WorldViewProjection"]?.SetValue(World * viewProjection);
            geometricPrimitive.Draw(Effect);
        }
       /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}