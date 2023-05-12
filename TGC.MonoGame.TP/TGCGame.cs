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
        private const float LINEAR_SPEED= 10f;
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

        private Matrix[] GroundWorld { get; set; }

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
            
            GroundWorld = new Matrix [] 
            {
                Matrix.CreateScale(15,1,10) * Matrix.CreateTranslation(Vector3.Zero),
                Matrix.CreateScale(15,1,4) * Matrix.CreateTranslation(new Vector3(150, 0f, 2f)),
                Matrix.CreateScale(14,1,38) * Matrix.CreateTranslation(new Vector3(800f,100f,245f)),
                Matrix.CreateScale(15,1,3) * Matrix.CreateTranslation(new Vector3(1235f,20f,415f)),
                Matrix.CreateScale(15,1,8) * Matrix.CreateTranslation(new Vector3(1405,20f,435f)),
                Matrix.CreateScale(30,1,8) * Matrix.CreateTranslation(new Vector3(1670f,20f,435f)),
                Matrix.CreateScale(10,1,8) * Matrix.CreateTranslation(new Vector3(2230f,20f,435f)),
                Matrix.CreateScale(35,1,8) * Matrix.CreateTranslation(new Vector3(2475f,20f,435f)),
                Matrix.CreateScale(35,1,8) * Matrix.CreateTranslation(new Vector3(2855f,20f,435f)),
                Matrix.CreateScale(8,1,40) * Matrix.CreateTranslation(new Vector3(3070f,20f,595f)),
                Matrix.CreateScale(8,1,10) * Matrix.CreateTranslation(new Vector3(3070f,30,865f)),
                Matrix.CreateScale(8,1,10) * Matrix.CreateTranslation(new Vector3(3070f,40,1005f)),
                Matrix.CreateScale(8,1,10) * Matrix.CreateTranslation(new Vector3(3070f,55,1145f)),
                Matrix.CreateScale(8,1,10) * Matrix.CreateTranslation(new Vector3(3070f,70,1285f)),
                Matrix.CreateScale(8,1,10) * Matrix.CreateTranslation(new Vector3(3070f,85,1425f)),
                Matrix.CreateScale(8,1,60) * Matrix.CreateTranslation(new Vector3(3070f,85,1825f)),
                Matrix.CreateScale(8,1,20) * Matrix.CreateTranslation(new Vector3(3070f,85,2245)),
                Matrix.CreateScale(60,1,12) * Matrix.CreateTranslation(new Vector3(2810,85,2405)),
                Matrix.CreateScale(80,1,4) * Matrix.CreateTranslation(new Vector3(2110,85,2365)),
                Matrix.CreateScale(80,1,4) * Matrix.CreateTranslation(new Vector3(2110,85,2445)),
                Matrix.CreateScale(8,1,150) * Matrix.CreateTranslation(new Vector3(1720,0,3175)),
                Matrix.CreateScale(8,1,100) * Matrix.CreateTranslation(new Vector3(1720,0,4455)),
                Matrix.CreateScale(10,1,260) * Matrix.CreateTranslation(new Vector3(1720,0,7620)),
                Matrix.CreateScale(10,1,15) * Matrix.CreateTranslation(new Vector3(1720,0,8995))

            };
            

            // Esfera
            Sphere = new SpherePrimitive(GraphicsDevice, 10);
            SpherePosition = new Vector3(0, 10, 0);

            // Cubo
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


            //Dibujo el suelo
            for (int i = 0; i < 23; i++)
            {
                // Get the World Matrix
                var matrix = GroundWorld[i];
                DrawGeometricPrimitive(matrix,Box);
            }


            DrawGeometry(Sphere, SpherePosition, -Yaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(30f * TAMANIO_CUBO , 0f, 4.5f ), CylinderYaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(40f * TAMANIO_CUBO, 0f, 4.5f ), -CylinderYaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(50f * TAMANIO_CUBO, 0f, 4.5f ), CylinderYaw, Pitch, Roll);

            DrawGeometry(SmallCylinder, new Vector3(30f * TAMANIO_CUBO, TAMANIO_CUBO, 4.5f ), Yaw, Pitch, Roll);
            DrawGeometry(SmallCylinder, new Vector3(40f * TAMANIO_CUBO, TAMANIO_CUBO, 4.5f ), Yaw, Pitch, Roll);
            DrawGeometry(SmallCylinder, new Vector3(50f * TAMANIO_CUBO, TAMANIO_CUBO, 4.5f ), Yaw, Pitch, Roll);

            DrawRectangle(Box,5,1,5,new Vector3(600f,70*MathF.Cos(3*time)-60,4.5f));
            DrawRectangle(Box,5,1,5,new Vector3(700f,-70*MathF.Cos(3*time)+60,4.5f));

            InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * TrackWorld* Matrix.CreateTranslation(864.1f,100f,415f) ,Camera.View, Camera.Projection);

            //Muro
            DrawRectangle(CyanBox,1,1,3,new Vector3(1360f,30f,410f));
            //
           
            //Muro
            DrawRectangle(CyanBox,1,1,8,new Vector3(1565f,30f,435f));
            //

            //Islas
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1850,20,404), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1890,20,434.5f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1930,20,454.5f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1980,20,454.5f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2030,20,434.5f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2080,20,404.5f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2130,20,414.5f), Yaw, Pitch, Roll);
            //


            //Muro
            DrawRectangle(CyanBox,1,1,8,new Vector3(2565f,30f,435f));
            //
            //Muro alto 
            DrawRectangle(CyanBox,4,4,1,new Vector3(3090f,45f,510f));
            DrawRectangle(CyanBox,4,4,1,new Vector3(3050f,45f,590f));
            DrawRectangle(CyanBox,4,4,1,new Vector3(3090f,45f,670f));
            //

            //cilindros que giran
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3100,100,1775), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3050,100,1820), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3100,100,1870), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(3050,100,1920), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            //

            DrawRectangle(Box,2,1,2,new Vector3(1720,42.5f*MathF.Cos(3*time)+42.5f,2405));

            //paredes que se mueven
            DrawRectangle(CyanBox,1, 5,4,new Vector3(40*MathF.Cos(5*time)+1720,40,2800));  
            DrawRectangle(CyanBox, 1,5,4,new Vector3(-40*MathF.Cos(5*time)+1720,40,2900));
            DrawRectangle(CyanBox,1, 5,4,new Vector3(40*MathF.Cos(5*time)+1720,40,3000));
            DrawRectangle(CyanBox,1, 5,4,new Vector3(-40*MathF.Cos(5*time)+1720,40,3100));
            DrawRectangle(CyanBox,1, 5,4,new Vector3(40*MathF.Cos(5*time)+1720,40,3200));
            
            //muros
            DrawRectangle(CyanBox,8,1,1,new Vector3(1720,10,3320));
            DrawRectangle(CyanBox,8,1,1,new Vector3(1720,10,3460));

            //muro insaltable
            DrawRectangle(CyanBox,4,4,1,new Vector3(1700,25,3895));
            //Muro insaltable 
            DrawRectangle(CyanBox,4,4,1,new Vector3(1740,25,4020));

             //Pared que aplastan contra el suelo
            DrawRectangle(ObstacleBox,8,1,8,new Vector3(1720,35*MathF.Cos(4*time)+45,4240));
            DrawRectangle(ObstacleBox,8,1,8,new Vector3(1720,35*MathF.Cos(4*time+MathHelper.PiOver2)+45,4430));
            DrawRectangle(ObstacleBox,8,1,8,new Vector3(1720,35*MathF.Cos(4*time+MathHelper.PiOver4)+45,4620));            
            //Muro 
            DrawRectangle(CyanBox,8,1,1,new Vector3(1720,10,4825));
            
            //Plataformas que se mueven
            DrawRectangle(CyanBox,4,1,8,new Vector3(1740,0,200*MathF.Cos(2*time)+5230));  
            DrawRectangle(CyanBox,4,1,8,new Vector3(1690,0,200*MathF.Cos(2*time+MathHelper.Pi)+5600));
            DrawRectangle(CyanBox,4,1,8,new Vector3(1740,0,200*MathF.Cos(2*time)+6010));
            
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
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x + TAMANIO_CUBO, y + TAMANIO_CUBO, z + TAMANIO_CUBO), CylinderYaw, Pitch, MathHelper.PiOver2);
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