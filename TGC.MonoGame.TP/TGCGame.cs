using System;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;

namespace TGC.MonoGame.TP
{
    /// <summary>
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
            //Cilindro
            Cylinder = new CylinderPrimitive(GraphicsDevice, CYLINDER_HEIGHT, CYLINDER_DIAMETER, 18);
            SmallCylinder = new CylinderPrimitive(GraphicsDevice, CYLINDER_HEIGHT *2, CYLINDER_DIAMETER/10, 18);    

            InclinedTrackModel = Content.Load<Model>(ContentFolder3D + "rampa");
            CurveTrackModel = Content.Load<Model>(ContentFolder3D + "curva");
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

             if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                SpherePosition += Vector3.Up* LINEAR_SPEED;
            }
             if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                SpherePosition -= Vector3.Up* LINEAR_SPEED;
            }

            CylinderYaw += deltaTime * 1.1f;
            PlatformHeight = 70* MathF.Cos(4*Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds))-60; 
            WallLength = 50* MathF.Cos(8*Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds))-50; 
            SphereRotationMatrix = Matrix.CreateRotationY(Rotation);

            World = SphereRotationMatrix * Matrix.CreateTranslation(SpherePosition);

            UpdateCamera();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.DarkBlue.ToVector3());
            var rotationMatrix = Matrix.CreateRotationY(Rotation);

            //foreach (var mesh in Model.Meshes)
            //{
            //    World = mesh.ParentBone.Transform * rotationMatrix;
            //    Effect.Parameters["World"].SetValue(World);
            //    mesh.Draw();
            //}

            DrawGeometry(Sphere, SpherePosition, -Yaw, Pitch, Roll);
            DrawPrincipalPlatform();
            DrawBridge(15f);
            //Se puede reemplazar con un for.
            DrawGeometry(Cylinder, new Vector3(30f *1.0f* TAMANIO_CUBO, 0f, 4.5f* 1.0f * TAMANIO_CUBO ), CylinderYaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(40f *1.0f* TAMANIO_CUBO, 0f, 4.5f* 1.0f * TAMANIO_CUBO ), -CylinderYaw, Pitch, Roll);
            DrawGeometry(Cylinder, new Vector3(50f *1.0f* TAMANIO_CUBO, 0f, 4.5f* 1.0f * TAMANIO_CUBO ), CylinderYaw, Pitch, Roll);
            //Se puede reemplazar con un for.
            DrawGeometry(SmallCylinder, new Vector3(30f *1.0f* TAMANIO_CUBO, TAMANIO_CUBO, 4.5f* 1.0f * TAMANIO_CUBO ), Yaw, Pitch, Roll);
            DrawGeometry(SmallCylinder, new Vector3(40f *1.0f* TAMANIO_CUBO, TAMANIO_CUBO, 4.5f* 1.0f * TAMANIO_CUBO ), Yaw, Pitch, Roll);
            DrawGeometry(SmallCylinder, new Vector3(50f *1.0f* TAMANIO_CUBO, TAMANIO_CUBO, 4.5f* 1.0f * TAMANIO_CUBO ), Yaw, Pitch, Roll);
            DrawBridge(55.5f);

            DrawXZRectangle(Box,5,5,new Vector3(721.1f,PlatformHeight,22f));
            DrawXZRectangle(Box,5,5,new Vector3(809.1f,-PlatformHeight,22f));
            DrawXZRectangle(Box,14,38,new Vector3(800f,100f,105f));
            //DrawRectangle(10,2,new Vector3(754.1f,100f,523));
            //DrawRectangle(10,2,new Vector3(754.1f,100f,523));
            //DrawRectangle(10,2,new Vector3(754.1f,100f,523));
            //Trato de dibujar rampa
            DrawGeometry(Box, new Vector3(23.7f * 1.0f* TAMANIO_CUBO,1f, 4.0f * 1.0f* TAMANIO_CUBO), Yaw, Pitch, 0.5f);
            DrawGeometry(Box, new Vector3(23.7f * 1.0f* TAMANIO_CUBO,1f, 5.0f * 1.0f* TAMANIO_CUBO), Yaw, Pitch, 0.5f);
            InclinedTrackModel.Draw(Matrix.CreateScale(1.5f) * TrackWorld* Matrix.CreateTranslation(864.1f,100f,505) ,Camera.View, Camera.Projection);
            DrawXZRectangle(Box,15,3,new Vector3(1160f,20f,495f));
            //Muro
            DrawXZRectangle(CyanBox,1,3,new Vector3(1360f,30f,495f));
            DrawXZRectangle(Box,15,8,new Vector3(1330f,20f,495f));
            DrawXZRectangle(Box,30,8,new Vector3(1520f,20f,495f));
            //Muro
            DrawXZRectangle(CyanBox,1,8,new Vector3(1560f,30f,495f));
            //Islas
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1840f,20,495f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1890,20,520f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1930,20,545f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(1980,20,545f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2030,20,520f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2080,20,495f), Yaw, Pitch, Roll);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 10, 20, 18), new Vector3(2130,20,520f), Yaw, Pitch, Roll);

           // DrawGeometry(Box, new Vector3(23.7f * 1.0f* TAMANIO_CUBO,1f, 5.0f * 1.0f* TAMANIO_CUBO), Yaw, Pitch, Roll);
            //DrawGeometry(Box, new Vector3(23.7f * 1.0f* TAMANIO_CUBO,1f, 5.0f * 1.0f* TAMANIO_CUBO), Yaw, Pitch, Roll);
            //DrawXZRectangle(ObstacleBox,1,2,new Vector3(1840f,30f,495f));
            
            /*
            //paredes que se mueven
            DrawYZRectangle(ObstacleBox, 5,4,new Vector3(1470,30,535f+WallLength));
            DrawYZRectangle(ObstacleBox, 5,4,new Vector3(1570,30,455-WallLength));
            DrawYZRectangle(ObstacleBox, 5,4,new Vector3(1670,30,535f+WallLength));
            DrawYZRectangle(ObstacleBox, 5,4,new Vector3(1770,30,455-WallLength));
            DrawYZRectangle(ObstacleBox, 5,4,new Vector3(1870,30,535f+WallLength));

            //Pared que aplastadw
            DrawXZRectangle(ObstacleBox,8,8,new Vector3(2070,-PlatformHeight+20,495f));
        
            //Paredes verticales que aplastan
            DrawXYRectangle(ObstacleBox, 8,8,new Vector3(2370,30,0.8f*WallLength+530));
            DrawXYRectangle(ObstacleBox, 8,8,new Vector3(2370,30,0.8f*-WallLength+530));
            */

            DrawXZRectangle(Box,10,8,new Vector3(2270f,20f,495f));
            //Muro
            DrawXZRectangle(CyanBox,1,8,new Vector3(2650,30f,495f));
            DrawXZRectangle(Box,35,8,new Vector3(2170,20f,495f));
            DrawXZRectangle(Box,35,8,new Vector3(2520,20f,495f));
            DrawXZRectangle(Box,8,40,new Vector3(2870,20f,495f));
            //
            DrawWalls(48f,0f,0f, 575f,485f, 1340f,Roll, Yaw, 20f);
            DrawWalls(70f,0f,0f,575f,485f,2170f,Roll, Yaw, 20f);
            //Muro insaltable (existe esa palabra?)
            DrawXYRectangle(CyanBox,4,4,new Vector3(2870,30f,605));
            DrawXYRectangle(CyanBox,4,4,new Vector3(2910,30f,685));
            DrawXYRectangle(CyanBox,4,4,new Vector3(2870,30f,765));
            DrawXZRectangle(Box,8,10,new Vector3(2870,30,915));
            DrawXZRectangle(Box,8,10,new Vector3(2870,40,1045));
            DrawXZRectangle(Box,8,10,new Vector3(2870,55,1185));
            DrawXZRectangle(Box,8,10,new Vector3(2870,70,1325));
            DrawXZRectangle(Box,8,10,new Vector3(2870,85,1465));
            DrawXZRectangle(Box,8,60,new Vector3(2870,85,1605));
            //cilindros que giran
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2930,100,1700), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2880,100,1750), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2930,100,1900), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawGeometry(new CylinderPrimitive(GraphicsDevice, 60, 10, 18),new Vector3(2880,100,1950), 3*CylinderYaw,Pitch,MathHelper.PiOver2);
            DrawXZRectangle(Box,8,20,new Vector3(2870,85,2260));
            DrawXZRectangle(Box,16,12,new Vector3(2790,85,2460));
            DrawXZRectangle(Box,80,4,new Vector3(1990,85,2460));
            DrawXZRectangle(Box,80,4,new Vector3(1990,85,2540));
            DrawXZRectangle(Box,80,4,new Vector3(1990,85,2540));
            DrawXZRectangle(Box,2,2,new Vector3(2000,-PlatformHeight,2510));
            DrawXZRectangle(Box,8,200,new Vector3(1970,0,2510));
            //paredes que se mueven
            DrawYZRectangle(CyanBox, 5,4,new Vector3(2050+WallLength,10,3010));  
            DrawYZRectangle(CyanBox, 5,4,new Vector3(1970-WallLength,10,3110));
            DrawYZRectangle(CyanBox, 5,4,new Vector3(2050+WallLength,10,3210));
            DrawYZRectangle(CyanBox, 5,4,new Vector3(1970-WallLength,10,3310));
            DrawYZRectangle(CyanBox, 5,4,new Vector3(2050+WallLength,10,3410));

            
            //Monedas
            DrawCoin(8f,2f,4.5f);
            DrawCoin(8f,2f, 2.5f);
            DrawCoin(8f,2f, 6.5f);
            DrawCoin(30f,2f, 2.5f);
            DrawCoin(40f,2f, 6.5f);
            DrawCoin(50f,2f, 2.5f);
            DrawCoin(150f, 3f,53.5f);
            DrawCoin(1605f, 3f,53.5f);
            DrawCoin(170.5f, 3f,53.5f);
            DrawCoin(180f, 3f,53.5f);
            DrawCoin(290f, 10f, 170f);
            DrawCoin(290f, 10, 190f);


            DrawChequeredFlag(8, 15);

            //CurveTrackModel.Draw(TrackWorld* Matrix.CreateTranslation(864.1f,100f,505),Camera.View, Camera.Projection);

            
        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw, float pitch, float roll)
        {
            var effect = geometry.Effect;
            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position);
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;
            geometry.Draw(effect);
        }


        private void DrawPrincipalPlatform(){
            const int CANTIDAD_LINEAS = 15;
            const int CANTIDAD_COLUMNAS = 10;
            DrawXZRectangle(Box,CANTIDAD_LINEAS,CANTIDAD_COLUMNAS, Vector3.Zero);
            DrawWalls(CANTIDAD_LINEAS, 10f, -1f, 0f, 0f, 0f, Roll, Yaw, 0.7f);
        }

        private void DrawBridge(float upOffset){
            const float bridgelength = 10f;

            for (var i = 0; i < bridgelength; i++){
                for (var j = 0; j < 2; j++){
                    DrawGeometry(Box,new Vector3(upOffset * 1.0f * TAMANIO_CUBO + i*1.0f*TAMANIO_CUBO, 0f, 4* 1.0f * TAMANIO_CUBO + j*1.0f*TAMANIO_CUBO), Yaw, Pitch, Roll);
                }
            }

            DrawWalls(bridgelength,0f,0f,6* 1.0f * TAMANIO_CUBO, 3* 1.0f * TAMANIO_CUBO, upOffset*1.0f*TAMANIO_CUBO, Roll, Yaw, 0.7f);

        }

        ///<summary>
        /// Dibuja una plataforma rectangular en el plano XZ (Horizontal)
        ///</summary>
        private void DrawXZRectangle (CubePrimitive BoxType ,int depth, int with, Vector3 position){
             for (var i = 0; i < depth; i++){
                for(var j=0; j < with; j++){
                DrawGeometry(BoxType,new Vector3(position.X + i*1.0f*TAMANIO_CUBO, position.Y, position.Z + j*1.0f*TAMANIO_CUBO), Yaw, Pitch, Roll);
                }
            }
        }

          ///<summary>
        /// Dibuja una plataforma rectangular en el plano YZ (Vertical)
        ///</summary>
        private void DrawYZRectangle (CubePrimitive BoxType, int height, int width, Vector3 position){
            for (var i = 0; i < height; i++){
                for(var j=0; j < width; j++){
                 DrawGeometry(BoxType,new Vector3(position.X , position.Y+ i*TAMANIO_CUBO, position.Z + j*TAMANIO_CUBO), Yaw, Pitch, Roll);
                }
            }
        }

        ///<summary>
        /// Dibuja una plataforma rectangular en el plano XY (Longitudinal)
        ///</summary>
         private void DrawXYRectangle (CubePrimitive BoxType, int depth, int height, Vector3 position){
            for (var i = 0; i < depth; i++){
                for(var j=0; j < height; j++){
                 DrawGeometry(BoxType,new Vector3(position.X + (j*TAMANIO_CUBO), position.Y+ i*TAMANIO_CUBO, position.Z), Yaw, Pitch, Roll);
                }
            }
        }

        private void DrawCoin(float x, float y, float z){
             DrawGeometry(new CoinPrimitive(GraphicsDevice,1,10,40), new Vector3(x * TAMANIO_CUBO, y * TAMANIO_CUBO, z * TAMANIO_CUBO), CylinderYaw, Pitch, MathHelper.PiOver2);
        }

        private void DrawWalls(float wallLength, float rightLimit, float leftLimit, float rightOffset, float leftOffset, float upOffset, float BridgeRoll, float BridgeYaw, float y){
            // Pared Derecha.
            for (var k = 0; k < wallLength; k++){
                for(var j = 0; j < 2; j++){
                    DrawGeometry(WallBox, new Vector3(upOffset + k*1.0f * TAMANIO_CUBO,y + j * 0.7f * TAMANIO_CUBO, rightOffset + rightLimit*1.0f * TAMANIO_CUBO), BridgeYaw, Pitch, BridgeRoll);
                }
            }

            // Pared Izquierda.
            for (var k = 0; k < wallLength; k++){
                for(var j = 0; j < 2; j++){
                    DrawGeometry(WallBox, new Vector3(upOffset + k*1.0f * TAMANIO_CUBO,y + j * 0.7f * TAMANIO_CUBO, leftOffset + leftLimit*1.0f * TAMANIO_CUBO), BridgeYaw, Pitch, BridgeRoll);
                }
            }
        }

        private void DrawChequeredFlag(float rows, float columns){
            for(var i = 0; i < rows; i++){
                for(var j = 0; j <= columns; j++){
                if((i%2==1 && j%2==1) || (i%2==0 && j%2==0))
                    DrawGeometry(WhiteBox, new Vector3(1970 + i*TAMANIO_CUBO, 0f,4510 + j*TAMANIO_CUBO), Yaw, Pitch, Roll);
                else
                    DrawGeometry(BlackBox, new Vector3(1970 + i*TAMANIO_CUBO, 0f,4510 + j* TAMANIO_CUBO), Yaw, Pitch, Roll);
                }
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