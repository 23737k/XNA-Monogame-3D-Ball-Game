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
        private const float LINEAR_SPEED= 2f;
        private const float ANGULAR_SPEED = 2f;
        private const float CAMERA_FOLLOW_RADIUS = 70f;
        private const float CAMERA_UP_DISTANCE = 60f;
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
        private Vector3 BoxPosition { get; set; }
        private Vector3 SpherePosition { get; set; }
        private float Yaw { get; set; }
        private float Pitch { get; set; }
        private float Roll { get; set; }
        private TargetCamera Camera { get; set; }
        //Pistas
        private Model LCurveTrackModel { get; set; }
        private Model RCurveTrackModel { get; set; }
        private Model LinearTrackModel { get; set; }
        private Model TrackModel { get; set; }
        private Model RampModel { get; set; }
        //Esta matriz es comun a todos los modelos estaticos. Es utilizada para posicionar los objetos correctamente.
        private Matrix TrackWorld { get; set; }

        //Rocas
        private Model RockModel { get; set; } 


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
            Box = new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.DarkCyan, Color.DarkMagenta, Color.DarkGreen,
                Color.MonoGameOrange, Color.Black, Color.DarkGray);
            BoxPosition = Vector3.Zero;

            //Pongo todos los modelos en la posicion correcta.
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

            LCurveTrackModel = Content.Load<Model>(ContentFolder3D + "curva-izquierda");
            RCurveTrackModel = Content.Load<Model>(ContentFolder3D + "curva-derecha");
            LinearTrackModel = Content.Load<Model>(ContentFolder3D + "pista-recta");
            RockModel = Content.Load<Model>(ContentFolder3D + "Rock");
            RampModel =  Content.Load<Model>(ContentFolder3D + "loma");

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
            
            for (var i = 0; i < CANTIDAD_CUBOS; i++){
                
                DrawGeometry(new CubePrimitive(GraphicsDevice, TAMANIO_CUBO, Color.DarkCyan, Color.DarkMagenta, Color.DarkGreen,
                Color.MonoGameOrange, Color.Black, Color.DarkGray),new Vector3(i*1.1f*TAMANIO_CUBO, 0f, 0f), Yaw, Pitch, Roll);
            }

           // LCurveTrackModel.Draw(Matrix.CreateTranslation(new Vector3(22f,0f,0f)),Camera.View, Camera.Projection);
    
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(110f,0f,0f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(141f,0f,0f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(172f,0f,0f)), Camera.View,Camera.Projection);
            RCurveTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(203f,0f,20f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,0f,31f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,10f,62f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,20f,93f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,20f,124f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,20f,155f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,20f,186f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,20f,217f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(264f,30f,248f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,40f,279f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(202f,50f,310f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,60f,341f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(202f,70f,372f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,60f,403f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(202f,50f,434f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(233f,40f,465f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(202f,30f,496f)), Camera.View,Camera.Projection);
            RampModel.Draw(TrackWorld*Matrix.CreateRotationY(MathHelper.PiOver2)*Matrix.CreateTranslation(new Vector3(351f,31f,557f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateTranslation(new Vector3(528f,32f,532f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateRotationX(MathHelper.PiOver4)*Matrix.CreateTranslation(new Vector3(559f,32f,532f)), Camera.View,Camera.Projection);
            LinearTrackModel.Draw(TrackWorld*Matrix.CreateRotationX(-MathHelper.PiOver4)*Matrix.CreateTranslation(new Vector3(590,32f,563f)), Camera.View,Camera.Projection);
           // LinearTrackModel.Draw(TrackWorld*Matrix.CreateRotationZ(-0.5f)*Matrix.CreateTranslation(new Vector3(559f,32f,532f)), Camera.View,Camera.Projection);
           // LinearTrackModel.Draw(TrackWorld*Matrix.CreateRotationZ(-0.5f)*Matrix.CreateTranslation(new Vector3(559f,32f,532f)), Camera.View,Camera.Projection);
            //LinearTrackModel.Draw(TrackWorld*Matrix.CreateRotationZ(-0.5f)*Matrix.CreateTranslation(new Vector3(559f,32f,532f)), Camera.View,Camera.Projection);


            RockModel.Draw(Matrix.CreateScale(10f)*Matrix.CreateRotationZ(-MathHelper.PiOver2)*Matrix.CreateRotationX(-MathHelper.PiOver2)*Matrix.CreateTranslation(new Vector3(50f,10f,40f)), Camera.View,Camera.Projection);

        }

        private void DrawGeometry(GeometricPrimitive geometry, Vector3 position, float yaw, float pitch, float roll)
        {
            var effect = geometry.Effect;

            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position);
            effect.View = Camera.View;
            effect.Projection = Camera.Projection;

            geometry.Draw(effect);
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