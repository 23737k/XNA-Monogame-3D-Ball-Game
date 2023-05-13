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

        private bool ShowGizmos { get; set; } = true;
        private const float TAMANIO_CUBO = 10f;
        private const float LINEAR_SPEED= 6f;
        private const float ANGULAR_SPEED = 3f;
        private const float CAMERA_FOLLOW_RADIUS = 70f;
        private const float CAMERA_UP_DISTANCE = 30f;
        private const float CYLINDER_HEIGHT = 10F;
        private const float CYLINDER_DIAMETER = 10f * TAMANIO_CUBO;

        private const float SALTO_BUFFER_VALUE = 20f;

        private const float SALTO_BUFFER_DECREMENT_ALPHA = 25f;

        private const float GRAVITY = 200f;
        private Vector3 SPHERE_INITIAL_POSITION = new Vector3(1235f,30f,415f);

        //CHECKPOINTS DEBE ESTAR ORDENADO ASCENDENTEMENTE
        //EL PRIMER VALOR DEBE SER LA POSICION INICIAL DE LA ESFERA
        private Vector3[] CHECKPOINTS={new Vector3(0, 9.99f, 0)};
        private const float COORDENADA_Y_MAS_BAJA = 0f;

        private float CylinderYaw = 0.0f;
        private float PlatformHeight = 0f;
        private float WallLength = 0f;
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
        private float CaidaBuffer {get; set;}
        private bool EstaSubiendoEnSalto { get; set;}
        private bool EstaBajandoEnSalto { get; set;}
        private Matrix[] GroundWorld { get; set; }
        private BoundingBox[] CollidersBoxes { get; set; }
        private BoundingCylinder SphereCollider { get; set; }
        private Vector3 SphereVelocity { get; set; }
        private Vector3 SphereAcceleration { get; set; }
        private bool OnGround { get; set; }
        private Vector3 SphereFrontDirection { get; set; }
        private Matrix SphereScale {get; set; }
        private float EPSILON = 0.000001f;

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
            SpherePosition = SPHERE_INITIAL_POSITION;;
            SphereCollider = new BoundingCylinder(SpherePosition, 5f, 5f);
            SphereVelocity = Vector3.Zero;
            SphereAcceleration = Vector3.Down * GRAVITY;
            SphereFrontDirection =  Vector3.Backward;
            SphereScale = Matrix.CreateScale(0.3f);

            // Configuramos nuestras matrices de la escena.
            Rotation=-MathHelper.PiOver2;
            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);

            GroundWorld = new Matrix [] 
            {
                Matrix.CreateScale(150, 10, 100) * Matrix.CreateTranslation(Vector3.Zero),
                Matrix.CreateScale(150,10f,40) * Matrix.CreateTranslation(new Vector3(150, 0f, 2f)),
                Matrix.CreateScale(140,10f,380) * Matrix.CreateTranslation(new Vector3(800f,100f,245f)),
                Matrix.CreateScale(150,10f,30) * Matrix.CreateTranslation(new Vector3(1235f,20f,415f)),
                Matrix.CreateScale(150,10f,80) * Matrix.CreateTranslation(new Vector3(1405,20f,435f)),
                Matrix.CreateScale(300,10f,80) * Matrix.CreateTranslation(new Vector3(1670f,20f,435f)),
                Matrix.CreateScale(100,10f,80) * Matrix.CreateTranslation(new Vector3(2230f,20f,435f)),
                Matrix.CreateScale(350,10f,80) * Matrix.CreateTranslation(new Vector3(2475f,20f,435f)),
                Matrix.CreateScale(350,10f,80) * Matrix.CreateTranslation(new Vector3(2855f,20f,435f)),
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
                Matrix.CreateScale(100,10f,150) * Matrix.CreateTranslation(new Vector3(1720,0,8995))
                
            };



            //Create bounding boxes
            CollidersBoxes = new BoundingBox[GroundWorld.Length];

            for(int i = 0; i < GroundWorld.Length; i++)
                CollidersBoxes[i] =  BoundingVolumesExtensions.FromMatrix(GroundWorld[i]);

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
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
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

            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

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
            AdministrarSalto(deltaTime);

            CylinderYaw += deltaTime * 1.1f;
            PlatformHeight = 70* MathF.Cos(4*Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds))-60; 
            WallLength = 50* MathF.Cos(8*Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds))-50; 

            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);

            //Habría que ver el tema de la aceleración.
            //Esta línea sería la gravedad

            SphereVelocity += SphereAcceleration * deltaTime;

            var scaledVelocity= SphereVelocity * deltaTime;

            SolveVerticalMovement(scaledVelocity);

            scaledVelocity = new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            //SolveHorizontalMovementSliding(SphereVelocity);

            SpherePosition = SphereCollider.Center;
        
            SphereVelocity = new Vector3(0f, SphereVelocity.Y, 0f);

            World = SphereRotationMatrix * Matrix.CreateTranslation(SpherePosition);
            UpdateCamera();
            base.Update(gameTime);
        }     

        protected void MovementManager(float deltaTime){
            if (Keyboard.GetState().IsKeyDown(Keys.W)) //&& PelotaEstaEnElSuelo() && !PelotaSeCayo()
            {
                //SphereVelocity += SphereRotationMatrix.Forward * LINEAR_SPEED ;
                SphereCollider.Center += SphereRotationMatrix.Forward * LINEAR_SPEED ;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S)) //&& PelotaEstaEnElSuelo() && !PelotaSeCayo()
            {
                //SphereVelocity -= SphereRotationMatrix.Forward * LINEAR_SPEED;
                SphereCollider.Center -= SphereRotationMatrix.Forward * LINEAR_SPEED ;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Rotation += ANGULAR_SPEED * deltaTime;
                SphereFrontDirection = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(Rotation));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Rotation -= ANGULAR_SPEED * deltaTime;
                SphereFrontDirection = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(Rotation));
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                SphereCollider.Center += Vector3.Up* LINEAR_SPEED;
            }
             if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                SphereCollider.Center -= Vector3.Up* LINEAR_SPEED;
            }



            AdministrarSalto(deltaTime); //HABILITAR CUANDO FUNCIONE PelotaEstaEnElSuelo()
            AdministrarCaida(deltaTime); //HABILITAR CUANDO FUNCIONE PelotaEstaEnElSuelo()
        }

        private void SolveVerticalMovement(Vector3 scaledVelocity)
        {
            SphereCollider.Center += Vector3.Up * scaledVelocity.Y;
            OnGround = false;

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


            /*
            for (var i = 0; i < CollidersBoxes.Length; i++)
            {   
                BoundingBox aCollider = CollidersBoxes[i];
                bool didColilde = !SphereCollider.Intersects(aCollider).Equals(BoxCylinderIntersection.Intersecting);
                if(!didColilde){
                    continue;
                }
                    
                //If we collided with something we do something
                SphereVelocity = new Vector3(SphereVelocity.X, 0f, SphereVelocity.Z);

                collided = true;
                foundIndex = i;
                break;
            }
            */
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

        private void SolveHorizontalMovementSliding(Vector3 scaledVelocity)
        {
            //Has horizontal movement
            if (Vector3.Dot(scaledVelocity, new Vector3(20f, 10f, 20f)) == 0f)
            return;

            SphereCollider.Center += new Vector3(scaledVelocity.X, 0f, scaledVelocity.Z);

            //Check intersection for every collider
            for (int i = 0; i < CollidersBoxes.Length; i++) 
            {
                if(!SphereCollider.Intersects(CollidersBoxes[i]).Equals(BoxCylinderIntersection.Intersecting))
                    continue;
                
                var collider = CollidersBoxes[i];
                var colliderCenter = BoundingVolumesExtensions.GetCenter(collider);

                var sameLevelCenter = SphereCollider.Center;
                sameLevelCenter.Y = colliderCenter.Y;

                var closestPoint = BoundingVolumesExtensions.ClosestPoint(collider, sameLevelCenter);

                var normalVector = sameLevelCenter - closestPoint;
                var normalVectorLength = normalVector.Length();

                var penetration = SphereCollider.Radius - normalVector.Length() + EPSILON;

                SphereCollider.Center += (normalVector / normalVectorLength * penetration);
            }
        }


        protected void AdministrarSalto(float deltaTime){

       
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (PelotaEstaEnElSuelo()) SaltoBuffer = SALTO_BUFFER_VALUE;
            }
        
            SpherePosition += Vector3.Up* LINEAR_SPEED * SaltoBuffer * deltaTime;

            if (SaltoBuffer > 0) SaltoBuffer -= GRAVITY * deltaTime;
            else if (SaltoBuffer < 0) SaltoBuffer = 0;
            
        }

        protected void AdministrarCaida(float deltaTime){
            if (!PelotaEstaEnElSuelo()){
                CaidaBuffer += GRAVITY * deltaTime;
                SpherePosition -= Vector3.Up* LINEAR_SPEED * CaidaBuffer * deltaTime;   
            }
            else CaidaBuffer = 0;
        }
        protected bool PelotaEstaEnElSuelo(){
            // Chequea si colisiona con el suelo
            
            for (var i = 0; i < CollidersBoxes.Length; i++)
            {   
                BoundingBox aCollider = CollidersBoxes[i];
                if (SphereCollider.Intersects(aCollider).Equals(BoxCylinderIntersection.Intersecting)) return true;
            }
            
            return false;
        }

        protected bool PelotaSeCayo(){
            return SpherePosition.Y < (COORDENADA_Y_MAS_BAJA - 50f);
        }

        protected void VolverAlUltimoCheckpoint(){
            //Reconoce el último checkpoint por el valor de coordenada X
            //más cercano y menor a la coordenada X de la posición actual de la esfera

            //Supone que CHECKPOINT esta en orden ascendente
            for(int i = 0; i < CHECKPOINTS.Length; i++){
                if (CHECKPOINTS[i].X <= SpherePosition.X) 
                SphereCollider.Center = CHECKPOINTS[i];
            }
        }
        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            var time= Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);

            // Estos 3 parametros quedan fijos.
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);

            //Dibujo el suelo
            for (int i = 0; i < GroundWorld.Length; i++)
            {
                // Get the World Matrix
                var matrix = GroundWorld[i];
                DrawGeometricPrimitive(matrix,Box);
            }


            DrawGeometry(Sphere, SpherePosition,0,0,0);


            DrawGeometry(Cylinder, new Vector3(300 , 0f, 4.5f ), CylinderYaw, 0,0);
            DrawGeometry(Cylinder, new Vector3(400, 0f, 4.5f ), -CylinderYaw, 0,0);
            DrawGeometry(Cylinder, new Vector3(500, 0f, 4.5f ), CylinderYaw, 0,0);

            DrawGeometry(SmallCylinder, new Vector3(300, 10, 4.5f ),0,0,0);
            DrawGeometry(SmallCylinder, new Vector3(400, 10, 4.5f ), 0,0,0);
            DrawGeometry(SmallCylinder, new Vector3(500, 10, 4.5f ), 0,0,0);

            DrawRectangle(Box,50,10,50,new Vector3(600f,70*MathF.Cos(3*time)-60,4.5f));
            DrawRectangle(Box,50,10,50,new Vector3(700f,-70*MathF.Cos(3*time)+60,4.5f));

            InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * TrackWorld* Matrix.CreateTranslation(864.1f,100f,415f) ,Camera.View, Camera.Projection);

            //Muro
            DrawRectangle(CyanBox,10,10,30,new Vector3(1360f,30f,410f));
            //
           
            //Muro
            DrawRectangle(CyanBox,10,10,80,new Vector3(1565f,30f,435f));
            //

            //Islas
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1850,20,404), 0,0,0);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1890,20,434.5f),0,0,0);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1930,20,454.5f),0,0,0);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1980,20,454.5f),0,0,0);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2030,20,434.5f),0,0,0);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2080,20,404.5f), 0,0,0);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2130,20,414.5f), 0,0,0);
            //


            //Muro
            DrawRectangle(CyanBox,10,10,80,new Vector3(2565f,30f,435f));
            //
            //Muro alto 
            DrawRectangle(CyanBox,40,40,10,new Vector3(3090f,45f,510f));
            DrawRectangle(CyanBox,40,40,10,new Vector3(3050f,45f,590f));
            DrawRectangle(CyanBox,40,40,10,new Vector3(3090f,45f,670f));
            //

            //cilindros que giran
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3100,100,1775), 3*CylinderYaw,0,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3050,100,1820), 3*CylinderYaw,0,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3100,100,1870), 3*CylinderYaw,0,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3050,100,1920), 3*CylinderYaw,0,MathHelper.PiOver2);
            //

            DrawRectangle(Box,20,10,20,new Vector3(1720,42.5f*MathF.Cos(3*time)+42.5f,2405));

            //paredes que se mueven
            DrawRectangle(CyanBox,10, 50,40,new Vector3(40*MathF.Cos(5*time)+1720,40,2800));  
            DrawRectangle(CyanBox, 10,50,40,new Vector3(-40*MathF.Cos(5*time)+1720,40,2900));
            DrawRectangle(CyanBox,10, 50,40,new Vector3(40*MathF.Cos(5*time)+1720,40,3000));
            DrawRectangle(CyanBox,10, 50,40,new Vector3(-40*MathF.Cos(5*time)+1720,40,3100));
            DrawRectangle(CyanBox,10, 50,40,new Vector3(40*MathF.Cos(5*time)+1720,40,3200));
            
            //muros
            DrawRectangle(CyanBox,80,10,10,new Vector3(1720,10,3320));
            DrawRectangle(CyanBox,80,10,10,new Vector3(1720,10,3460));

            //muro insaltable
            DrawRectangle(CyanBox,40,40,10,new Vector3(1700,25,3895));
            //Muro insaltable 
            DrawRectangle(CyanBox,40,40,10,new Vector3(1740,25,4020));

             //Pared que aplastan contra el suelo
            DrawRectangle(ObstacleBox,80,10,80,new Vector3(1720,35*MathF.Cos(4*time)+45,4240));
            DrawRectangle(ObstacleBox,80,10,80,new Vector3(1720,35*MathF.Cos(4*time+MathHelper.PiOver2)+45,4430));
            DrawRectangle(ObstacleBox,80,10,80,new Vector3(1720,35*MathF.Cos(4*time+MathHelper.PiOver4)+45,4620));            
            //Muro 
            DrawRectangle(CyanBox,80,10,10,new Vector3(1720,10,4825));
            
            //Plataformas que se mueven
            DrawRectangle(CyanBox,40,10,80,new Vector3(1740,0,200*MathF.Cos(2*time)+5230));  
            DrawRectangle(CyanBox,40,10,80,new Vector3(1690,0,200*MathF.Cos(2*time+MathHelper.Pi)+5600));
            DrawRectangle(CyanBox,40,10,80,new Vector3(1740,0,200*MathF.Cos(2*time)+6010));
            base.Draw(gameTime);
        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw, float pitch, float roll)
        {
            DrawGeometricPrimitive(Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position), geometry);
        }

        ///<summary>
        /// Dibuja una plataforma rectangular en el plano XZ (Horizontal)
        ///</summary>
        private void DrawRectangle   (CubePrimitive BoxType ,float length, float height, float width, Vector3 position){
            DrawGeometricPrimitive(Matrix.CreateScale(length,height, width) * Matrix.CreateTranslation(position.X, position.Y, position.Z ), BoxType);
        }

        private void DrawCoin(float x, float y, float z){
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x + 1f, y + 1f, z + 1f), 1f, 0, MathHelper.PiOver2);
        }

        private void DrawGeometricPrimitive(Matrix World, GeometricPrimitive geometricPrimitive){
            
            //Usa el VertexBuffer y el IndexBuffer generado por la clase GeometricPrimitive.
            //Pero no utilizamos el metodo Draw de dicha clase para no utilizar el shader BasicEffect.
            //En cambio, dibujamos la primitiva mediante este metodo.

            Effect.Parameters["World"].SetValue(World);

            GraphicsDevice.SetVertexBuffer(geometricPrimitive.VertexBuffer);

            GraphicsDevice.Indices = geometricPrimitive.IndexBuffer;

            foreach (var effectPass in Effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                var primitiveCount = geometricPrimitive.Indices.Count / 3;

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }
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