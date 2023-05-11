using System;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
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
        private const int CANTIDAD_CUBOS = 10;
        private const float LINEAR_SPEED= 2f;
        private const float ANGULAR_SPEED = 3f;
        private const float CAMERA_FOLLOW_RADIUS = 70f;
        private const float CAMERA_UP_DISTANCE = 30f;
        private const float CYLINDER_HEIGHT = 10F;
        private const float CYLINDER_DIAMETER = 10f * TAMANIO_CUBO;

        private const float SALTO_BUFFER_VALUE = 20f;

        private const float SALTO_BUFFER_DECREMENT_ALPHA = 25f;

        private float CylinderYaw = 0.0f;
        private float PlatformHeight = 0f;
        private float WallLength = 0f;
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
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
        private float Yaw { get; set; }
        private float Pitch { get; set; }
        private float Roll { get; set; }
        private TargetCamera Camera { get; set; }

        private Model InclinedTrackModel { get; set; }
        private Model CurveTrackModel { get; set; }
        private Matrix TrackWorld { get; set; }

        private float SaltoBuffer { get; set;}
        private bool EstaSubiendoEnSalto { get; set;}
        private bool EstaBajandoEnSalto { get; set;}
        private Matrix[] StairsWorld { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();
            // Seria hasta aca.

            // Configuramos nuestras matrices de la escena.
            Rotation=-MathHelper.PiOver2;
            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);
            World = SphereRotationMatrix;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero);
           
           //
            StairsWorld = new Matrix[]
            {
                Matrix.CreateScale(20f, 20f, 15f) * Matrix.CreateTranslation(0f, 3f, 125f),
                Matrix.CreateScale(20f, 20f, 15f) * Matrix.CreateTranslation(0f, 9f, 140f),
                Matrix.CreateScale(0f, 20f, 15f) * Matrix.CreateTranslation(0f, 15f, 155f),
                Matrix.CreateScale(0f, 20f, 40f) * Matrix.CreateTranslation(0f, 21f, 182.5f),
                Matrix.CreateScale(15f, 20f, 40f) * Matrix.CreateTranslation(-42.5f, 27f, 182.5f),
                Matrix.CreateScale(15f, 20f, 40f) * Matrix.CreateTranslation(-57.5f, 33f, 182.5f),
                Matrix.CreateScale(15f, 20f, 40f) * Matrix.CreateTranslation(-72.5f, 39f, 182.5f),
                Matrix.CreateScale(40f, 20f, 100f) * Matrix.CreateTranslation(-130f, 45f, 152.5f),
            };
            
            
            // Esfera
            Sphere = new SpherePrimitive(GraphicsDevice, 10);
            SpherePosition = new Vector3(0, 10, 0);

            // Cubo
            //en este momento no se estan usando ni box ni boxposition
            Box = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.MonoGameOrange, Color.MonoGameOrange, Color.MonoGameOrange,
            Color.MonoGameOrange, Color.MonoGameOrange, Color.MonoGameOrange);
            ObstacleBox = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.BlueViolet,Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet);
            //BoxPosition = Vector3.Zero;
            WallBox = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet, Color.BlueViolet);
            WhiteBox = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White);
            BlackBox = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);
            CyanBox = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan);
            YellowBox = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen, Color.YellowGreen);
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

            // Cargo el modelo del logo.
            //Model = Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            InclinedTrackModel = Content.Load<Model>(ContentFolder3D + "rampa");        

            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            //foreach (var mesh in Model.Meshes)
            //{
            //    // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            //    foreach (var meshPart in mesh.MeshParts)
            //    {
            //        meshPart.Effect = Effect;
            //    }
            //}

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
            // Aca deberiamos poner toda la logica de actualizacion del juego.

            var deltaTime= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }

            AdministrarMovimientoDePelota(deltaTime);

            CylinderYaw += deltaTime * 1.1f;
            PlatformHeight = 70* MathF.Cos(4*Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds))-60; 
            WallLength = 50* MathF.Cos(8*Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds))-50; 
            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);

            World = SphereRotationMatrix * Matrix.CreateTranslation(SpherePosition);

            UpdateCamera();

            base.Update(gameTime);
        }

        protected void AdministrarMovimientoDePelota(float deltaTime){
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                SpherePosition += SphereRotationMatrix.Forward * LINEAR_SPEED;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                SpherePosition -= SphereRotationMatrix.Forward * LINEAR_SPEED;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Rotation += ANGULAR_SPEED * deltaTime;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Rotation -= ANGULAR_SPEED * deltaTime;
            }

            //AdministrarSalto(deltaTime); HABILITAR CUANDO FUNCIONE PelotaEstaEnElSuelo()
        }

        protected void AdministrarSalto(float deltaTime){

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (PelotaEstaEnElSuelo()){
                    SaltoBuffer = SALTO_BUFFER_VALUE;
                    EstaSubiendoEnSalto = true;
                    EstaBajandoEnSalto = false;
                }
            }

            if(EstaSubiendoEnSalto && SaltoBuffer == 0){
                EstaSubiendoEnSalto = false;
                EstaBajandoEnSalto = true;
            }

            if(EstaSubiendoEnSalto){
                SpherePosition += Vector3.Up* LINEAR_SPEED * SaltoBuffer * deltaTime;

                if (SaltoBuffer > 0) SaltoBuffer -= SALTO_BUFFER_DECREMENT_ALPHA * deltaTime;
                else if (SaltoBuffer < 0) SaltoBuffer = 0;

                if(SaltoBuffer == 0){
                    EstaSubiendoEnSalto = false;
                    EstaBajandoEnSalto = true;
                }
            }

            if (EstaBajandoEnSalto && !PelotaEstaEnElSuelo()){
                SaltoBuffer += SALTO_BUFFER_DECREMENT_ALPHA * deltaTime;
                SpherePosition -= Vector3.Up* LINEAR_SPEED * SaltoBuffer * deltaTime;
            }
            
        }

        protected bool PelotaEstaEnElSuelo(){
            //TODO
            return true;
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
            
            var rotationMatrix = Matrix.CreateRotationY(Rotation);

            //foreach (var mesh in Model.Meshes)
            //{
            //    World = mesh.ParentBone.Transform * rotationMatrix;
            //    Effect.Parameters["World"].SetValue(World);
            //    mesh.Draw();
            //}

            DrawGeometry(Sphere, SpherePosition, -Yaw, Pitch, Roll);
            DrawXZRectangle(Box,15,10, Vector3.Zero);
            DrawXZRectangle(Box,15,4, new Vector3(15* TAMANIO_CUBO, 0f, 2f));
            //Se puede reemplazar con un for.
            DrawGeometry(Cylinder, new Vector3(30f * TAMANIO_CUBO , 0f, 4.5f ), CylinderYaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(40f * TAMANIO_CUBO, 0f, 4.5f ), -CylinderYaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(50f * TAMANIO_CUBO, 0f, 4.5f ), CylinderYaw, Pitch, Roll);
            //Se puede reemplazar con un for.
            DrawGeometry(SmallCylinder, new Vector3(30f * TAMANIO_CUBO, TAMANIO_CUBO, 4.5f ), Yaw, Pitch, Roll);
            DrawGeometry(SmallCylinder, new Vector3(40f * TAMANIO_CUBO, TAMANIO_CUBO, 4.5f ), Yaw, Pitch, Roll);
            DrawGeometry(SmallCylinder, new Vector3(50f * TAMANIO_CUBO, TAMANIO_CUBO, 4.5f ), Yaw, Pitch, Roll);

            DrawXZRectangle(Box,5,5,new Vector3(600f,70*MathF.Cos(3*time)-60,4.5f));
            DrawXZRectangle(Box,5,5,new Vector3(700f,-70*MathF.Cos(3*time)+60,4.5f));
            DrawXZRectangle(Box,14,38,new Vector3(800f,100f,105f));

            //DrawRectangle(10,2,new Vector3(754.1f,100f,523));
            //DrawRectangle(10,2,new Vector3(754.1f,100f,523));
            //DrawRectangle(10,2,new Vector3(754.1f,100f,523));
            InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * TrackWorld* Matrix.CreateTranslation(864.1f,100f,230f) ,Camera.View, Camera.Projection);
            DrawXZRectangle(Box,20,3,new Vector3(1260f,20f,230f));
            //Muroa
            DrawXZRectangle(CyanBox,1,3,new Vector3(1400f,30f,230f));
            DrawXZRectangle(Box,15,8,new Vector3(1430f,20f,230f));
            DrawXZRectangle(Box,30,8,new Vector3(1520f,20f,230f));
            //Muro
            DrawXZRectangle(CyanBox,1,8,new Vector3(1560f,30f,230f));
            //Islas
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1700f,20,230f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1750,20,260f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1800,20,280f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1850,20,280f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1900,20,260f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1950,20,230f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1980,20,240f), Yaw, Pitch, Roll);
            DrawXZRectangle(Box,10,8,new Vector3(2050f,20f,240f));
            //Paredes verticales que aplastan
            //DrawXYRectangle(ObstacleBox, 8,8,new Vector3(2370,30,0.8f*WallLength+530));
            //DrawXYRectangle(ObstacleBox, 8,8,new Vector3(2370,30,0.8f*-WallLength+530));
            
            //Muro
            DrawXZRectangle(CyanBox,1,8,new Vector3(2250,30f,240f));
            //
            DrawXZRectangle(Box,35,8,new Vector3(2170,20f,240f));
            DrawXZRectangle(Box,35,8,new Vector3(2520,20f,240f));
            DrawXZRectangle(Box,8,40,new Vector3(2670,20f,400f));
            //
            //DrawWalls(48f,0f,0f, 575f,485f, 1340f,Roll, Yaw, 20f);
            //DrawWalls(70f,0f,0f,575f,485f,2170f,Roll, Yaw, 20f);
            //Muro insaltable (existe esa palabra?)
            DrawXYRectangle(CyanBox,4,4,new Vector3(2670,50f,405));
            DrawXYRectangle(CyanBox,4,4,new Vector3(2610,50f,485));
            DrawXYRectangle(CyanBox,4,4,new Vector3(2670,50f,565));
            DrawXZRectangle(Box,8,10,new Vector3(2670,30,680));
            DrawXZRectangle(Box,8,10,new Vector3(2670,40,800));
            DrawXZRectangle(Box,8,10,new Vector3(2670,55,920));
            DrawXZRectangle(Box,8,10,new Vector3(2670,70,1040));
            DrawXZRectangle(Box,8,10,new Vector3(2670,85,1160));
            DrawXZRectangle(Box,8,60,new Vector3(2670,85,1450));
            //cilindros que giran
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2700,100,1400), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2650,100,1500), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2700,100,1600), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2650,100,1700), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawXZRectangle(Box,8,20,new Vector3(2670,85,1700));
            //Pista dividida
            DrawXZRectangle(Box,80,4,new Vector3(2300,85,1780));
            DrawXZRectangle(Box,80,4,new Vector3(2300,85,1700));
            DrawXZRectangle(Box,2,2,new Vector3(2000,42.5f*MathF.Cos(3*time)+42.5f,2510));
            DrawXZRectangle(Box,8,150,new Vector3(1970,0,2510));
            //paredes que se mueven
            DrawYZRectangle(CyanBox, 5,4,new Vector3(40*MathF.Cos(5*time)+2010,10,3010));  
            DrawYZRectangle(CyanBox, 5,4,new Vector3(-40*MathF.Cos(5*time)+2010,10,3110));
            DrawYZRectangle(CyanBox, 5,4,new Vector3(40*MathF.Cos(5*time)+2010,10,3210));
            DrawYZRectangle(CyanBox, 5,4,new Vector3(-40*MathF.Cos(5*time)+2010,10,3310));
            DrawYZRectangle(CyanBox, 5,4,new Vector3(40*MathF.Cos(5*time)+2010,10,3410));
            //muros
            DrawXZRectangle(CyanBox,8,1,new Vector3(1970,10,3490));
            DrawXZRectangle(CyanBox,8,1,new Vector3(1970,10,3610));

            //
            DrawXZRectangle(Box,8,100,new Vector3(1970,0,4030));
            //muro insaltable
            DrawXYRectangle(CyanBox,4,4,new Vector3(1970,10,3940));
            //Muro insaltable (ya fue, asi le voy a decir)
            DrawXYRectangle(CyanBox,4,4,new Vector3(2010,10,4080));

             //Pared que aplastan contra el suelo
            DrawXZRectangle(ObstacleBox,8,8,new Vector3(1970,35*MathF.Cos(4*time)+30,4300));
            DrawXZRectangle(ObstacleBox,8,8,new Vector3(1970,35*MathF.Cos(4*time+MathHelper.PiOver2)+30,4430));
            DrawXZRectangle(ObstacleBox,8,8,new Vector3(1970,35*MathF.Cos(4*time+MathHelper.PiOver4)+30,4540));
            //Muro 
            DrawXZRectangle(CyanBox,8,1,new Vector3(1970,10,4700));
            //Plataformas que se mueven
            DrawXZRectangle(CyanBox,4,8,new Vector3(1970,0,200*MathF.Cos(2*time)+5230));  
            DrawXZRectangle(CyanBox,4,8,new Vector3(2020,0,200*MathF.Cos(2*time+MathHelper.Pi)+5600));
            DrawXZRectangle(CyanBox,4,8,new Vector3(1970,0,200*MathF.Cos(2*time)+6010));
            //
            DrawXZRectangle(Box,10,40,new Vector3(1970,0,6320));
            DrawXZRectangle(YellowBox,10,10,new Vector3(1970,0,6510));
            DrawXZRectangle(Box,10,200,new Vector3(1970,0,6720));
            
            //Monedas
            DrawCoin(35,10,4f);
            DrawCoin(55,10,4f);
            DrawCoin(75,10,4f);
            DrawCoin(95,10,4f);
            DrawCoin(275,10,4f);
            DrawCoin(305,10,4f);
            DrawCoin(38,10,4f);
            //DrawCoin(380,10,60);
            //DrawCoin(170.5f, 3f,53.5f);
            // DrawCoin(180f, 3f,53.5f);
            //DrawCoin(290f, 10f, 170f);
            //DrawCoin(290f, 10, 190f);
            DrawCoin(1970, 10, 6660);
            DrawCoin(1970, 10, 6690);
            DrawCoin(1970, 10, 6720);
            DrawCoin(1970, 10, 6750);
            DrawCoin(2000, 10, 6780);
            DrawCoin(2000, 10, 6810);
            DrawCoin(2000, 10, 6840);
            DrawCoin(2000, 10, 6870);
            DrawCoin(2030, 10, 6900);
            DrawCoin(2030, 10, 6930);
            DrawCoin(2030, 10, 6960);
            DrawCoin(2030, 10, 6990);
            DrawCoin(2030, 10, 7010);

            DrawCoin(2000, 10, 7050);
            DrawCoin(2000, 10, 7080);
            DrawCoin(2000, 10, 7110);
            DrawCoin(2000, 10, 7140);
            DrawCoin(2000, 10, 7170);
            DrawCoin(2000, 10, 7200);
            DrawCoin(2000, 10, 7230);
            DrawCoin(2000, 10, 7260);
            DrawCoin(2000, 10, 7290);
            DrawCoin(2000, 10, 7320);
            DrawCoin(2000, 10, 7350);

            DrawCoin(1970, 10, 7380);
            DrawCoin(1970, 10, 7410);
            DrawCoin(1970, 10, 7440);
            DrawCoin(1970, 10, 7470);
            DrawCoin(1970, 10, 7500);
            DrawCoin(1970, 10, 7530);
            DrawCoin(1970, 10, 7560);
            DrawCoin(1970, 10, 7590);

            



            //Troncos que cuando la pelota llegue a cierto punto arrancan a rodar.
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 70f,20f, 18),new Vector3(800f, 115f, 240f), Yaw, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 70f,20f, 18),new Vector3(800f, 115f, 220f), Yaw, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 70f,20f, 18),new Vector3(800f, 115f, 200f), Yaw, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 70f,20f, 18),new Vector3(800f, 135f, 240f), Yaw, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 70f,20f, 18),new Vector3(800f, 135f, 220f), Yaw, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 70f,20f, 18),new Vector3(800f, 155f, 240f), Yaw, Pitch, MathHelper.PiOver2);
            // En pista dividida
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2000, 98, 1780), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2030, 98, 1780), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2060, 98, 1780), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2090, 98, 1780), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2000, 98, 1700), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2030, 98, 1700), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2060, 98, 1700), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);
            DrawGeometry(new TrunkPrimitive(GraphicsDevice, 40f,20f, 18),new Vector3(2090, 98, 1700), MathHelper.PiOver2, Pitch, MathHelper.PiOver2);



            DrawChequeredFlag(10, 15);


            
        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw, float pitch, float roll)
        {
            DrawGeometricPrimitive(Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position), geometry);
        }

        ///<summary>
        /// Dibuja una plataforma rectangular en el plano XZ (Horizontal)
        ///</summary>
        private void DrawXZRectangle (CubePrimitive BoxType ,int length, int width, Vector3 position){
            DrawGeometricPrimitive(Matrix.CreateScale(length,1f, width) * Matrix.CreateTranslation(position.X, position.Y, position.Z), BoxType);
        }

          ///<summary>
        /// Dibuja una plataforma rectangular en el plano YZ (Vertical)
        ///</summary>
        private void DrawYZRectangle (CubePrimitive BoxType, int height, int width, Vector3 position){
            DrawGeometricPrimitive(Matrix.CreateScale(1f,height, width) * Matrix.CreateTranslation(position.X, position.Y, position.Z), BoxType);
        }

        ///<summary>
        /// Dibuja una plataforma rectangular en el plano XY (Longitudinal)
        ///</summary>
         private void DrawXYRectangle (CubePrimitive BoxType, int depth, int height, Vector3 position){
            DrawGeometricPrimitive(Matrix.CreateScale(depth,height,1f) * Matrix.CreateTranslation(position), BoxType);
        }

        private void DrawCoin(float x, float y, float z){
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x + TAMANIO_CUBO, y + TAMANIO_CUBO, z + TAMANIO_CUBO), CylinderYaw, Pitch, MathHelper.PiOver2);
        }


        private void DrawWall(float wallLength, Vector3 position){
            // Pared Derecha.
            DrawGeometricPrimitive(Matrix.CreateScale(wallLength, 2f, 1f) * Matrix.CreateTranslation(position), WallBox);
        }

        private void DrawChequeredFlag(float rows, float columns){
            for(var i = 0; i < rows; i++){
                for(var j = 0; j <= columns; j++){
                if((i%2==1 && j%2==1) || (i%2==0 && j%2==0))
                    DrawGeometry(WhiteBox, new Vector3(1970 + i*TAMANIO_CUBO, 0f,8720 + j*TAMANIO_CUBO), Yaw, Pitch, Roll);
                else
                    DrawGeometry(BlackBox, new Vector3(1970 + i*TAMANIO_CUBO, 0f,8720 + j* TAMANIO_CUBO), Yaw, Pitch, Roll);
                }
            }
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