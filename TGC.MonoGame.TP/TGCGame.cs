using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
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
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using TGC.MonoGame.TP.UI;
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
        private Texture2D FloorT, FloorN;
        private Texture2D GameOver;
        //Texture index
        private int textureIndex;
        private SphereType[] SpheresArray; 
        //
        private SpriteBatch SpriteBatch { get; set; }
        private SpriteFont SpriteFont {get;set;}
        //SPHERE
        private Model SphereModel  {get; set;}
        private Matrix SphereRotationMatrix { get; set; }
        private Vector3 SpherePosition { get; set; }
        private TargetCamera Camera { get; set; }
        private bool OnGround { get; set; }
        private Vector3 SphereFrontDirection { get; set; }
        private Vector3 SphereLateralDirection {get;set;}
        private Vector3 LightPosition { get; set; } = new Vector3 (1600,2500,1600);

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
        private bool FinalBossEnded = false;

        //Skybox
        private SkyBox SkyBox { get; set; }
        private Model CylinderModel {get;set;}

        //Music
        private Song Song { get; set; }
        private SoundEffect JumpSound { get; set; }
        private SoundEffect LoseSound { get; set; }
        private SoundEffect ButtonSound;
        private SoundEffect PowerupSound;
        private SoundEffect CheckpointSound;
        //Menu
        private Button PlayButton;
        private Button QuitButton;
        private Button RestartButton;
        private Button LeftButton;
        private Button RightButton;
        private Button MusicEnabledButton;
        private Button MusicDisabledButton;
        private MouseState  MouseState;
        private MouseState  PreviousMouseState;
        private KeyboardState KeyboardState;
        private KeyboardState PreviousKeyboardState;
        enum GameState
        {
            StartMenu,
            Loading,
            Playing,
            Paused,
            Ended
        }
        private GameState gameState;

        private bool GodMode;
        private int respawn=0;
        private BoundingFrustum BoundingFrustum;

        //POST-PROCESING
        private RenderTargetCube EnviromentMapRenderTarget;
        private RenderTarget2D ShadowMapRenderTarget;
        private StaticCamera CubeMapCamera;
        private TargetCamera LightCamera;

        private Effect CheckpointEffect;

        private Vector2 RegionABottomLeft = new Vector2(3030,3109);
        private Vector2 RegionATopRight= new Vector2(1724,2044);
        private Vector2 RegionBBottomLeft= new Vector2(2892,3018);
        private Vector2 RegionBTopRight= new Vector2(2347,2463);
        private Vector2 RegionCBottomLeft= new Vector2(2573,2820);
        private Vector2 RegionCTopRight= new Vector2(2347,2384);
        private Vector2 RegionDBottomLeft= new Vector2(2527,2820);
        private Vector2 RegionDTopRight= new Vector2(2424,2463);

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
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //Graphics.IsFullScreen= true;
            EnviromentMapRenderTarget =new RenderTargetCube(GraphicsDevice, 64, false,
                SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, 3072, 3072, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
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
                new Checkpoint(new Vector3(1720,10,6302)),
                new Checkpoint(new Vector3(1719,15,8396)),
                new Checkpoint(new Vector3(1720,15,12505))
            };
            CurrentCheckpoint = 0;
        
            // Esfera
            SpherePosition =Checkpoints[CurrentCheckpoint].Position;
            //
            SphereWorld = Matrix.CreateTranslation(SpherePosition);
            SphereFrontDirection =  Vector3.Backward;
            SphereRotationMatrix = Matrix.Identity;

            //Texture Index
            //Elegimos el texture index que querramos para modificar los valores de la textura, salto, etc.
            textureIndex = 0;
            SpheresArray = new SphereType[]
            {   
                new SphereMarble(),
                new SphereMetal(),
                new SpherePlastic() 
            };

            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 20, 60), Vector3.Zero,1f,1000);
            CubeMapCamera = new StaticCamera(1f, SpherePosition, Vector3.UnitX, Vector3.Up);
            
            LightCamera = new TargetCamera(1f, new Vector3(SpherePosition.X ,SpherePosition.Y+1000, SpherePosition.Z+200),SpherePosition ,1f,250);
            
            CubeMapCamera.BuildProjection(1f, 1f, 2000, MathHelper.PiOver2);
            LightCamera.BuildProjection(1f, 5f, 3000, MathHelper.PiOver2);
            LightCamera.BuildView();


            GodMode = false;
            BoundingFrustum = new BoundingFrustum(Camera.View * Camera.Projection);
            UpdateCamera();

            //Menu
            gameState = GameState.StartMenu;
            PreviousKeyboardState = KeyboardState = Keyboard.GetState();
            PreviousMouseState = MouseState = Mouse.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            
            var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect,500);

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
            FloorT = Content.Load<Texture2D>(ContentFolderTextures +"piso/" +"color");
            FloorN = Content.Load<Texture2D>(ContentFolderTextures + "piso/" + "normal");
            GameOver = Content.Load<Texture2D>(ContentFolderTextures + "gameover");
            DefaultEffect.Parameters["ModelTexture"].SetValue(FloorT);
            DefaultEffect.Parameters["NormalTexture"].SetValue(FloorN);

            DefaultEffect.CurrentTechnique = DefaultEffect.Techniques["DepthPass"];

            InitializeLights();
            InitializeEffect();
            LoadTextures();
            SphereModel = Content.Load<Model>(ContentFolder3D + "sphere");
            SphereModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = SphereEffect; 

            CylinderModel = Content.Load<Model>(ContentFolder3D + "cylinder");
            CylinderModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = DefaultEffect;

            var speedTexture = Content.Load<Texture2D>(ContentFolderTextures + "speed-powerup");
            var normalTexture = Content.Load<Texture2D>(ContentFolderTextures + "normal");
            var jumpTexture = Content.Load<Texture2D>(ContentFolderTextures + "jump-powerup");
            Powerups = new List<Powerup>();
            Powerups.Add(new Powerup(new Vector3(30,15,0),100,0,CylinderModel,speedTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(1590,60,434),0,800,CylinderModel,jumpTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(3080,40,707),0,800,CylinderModel,jumpTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(3064,100,2415),0,1000,CylinderModel,jumpTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(2347,108,2354),0,700,CylinderModel,jumpTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(1722,30,2733),100,0,CylinderModel,speedTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(1733,15,4934   ),0,900,CylinderModel,jumpTexture,normalTexture,Camera));
            Powerups.Add(new Powerup(new Vector3(1717,15,4153 ),100,0,CylinderModel,speedTexture,normalTexture,Camera));
            //Bepu
            LoadPhysics();
            //Music
            Song = Content.Load<Song>(ContentFolderMusic + "soundtrack");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            JumpSound = Content.Load<SoundEffect>(ContentFolderMusic + "jumpEffect");
            LoseSound = Content.Load<SoundEffect>(ContentFolderMusic + "loosingEffect");
            ButtonSound = Content.Load<SoundEffect>(ContentFolderMusic + "buttonEffect");
            PowerupSound = Content.Load<SoundEffect>(ContentFolderMusic + "powerupEffect");
            CheckpointSound = Content.Load<SoundEffect>(ContentFolderMusic + "checkpointEffect");

            SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

            //Menu
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;
            PlayButton = new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/play"), new Vector2(width/7f, height/5),0.3f,ButtonSound);
            QuitButton = new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/quit"), new Vector2(width/7f, height/2),0.3f,ButtonSound);
            RestartButton = new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/restart"), new Vector2(width/7f, height/2.9f),0.3f,ButtonSound);
            LeftButton= new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/left-button"), new Vector2(width*0.42f, height*0.7f),0.1f,ButtonSound);
            RightButton = new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/right-button"), new Vector2(width*0.52f, height*0.7f),0.1f,ButtonSound);
            MusicEnabledButton = new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/music"), new Vector2(width*0.01f, height*0.01f),0.1f,ButtonSound);
            MusicDisabledButton = new Button(Content.Load<Texture2D>(ContentFolderTextures + "menu/music-disabled"), new Vector2(width*0.01f, height*0.01f),0.1f,ButtonSound);
            
            CheckpointEffect = Content.Load<Effect>(ContentFolderEffects + "CheckpointEffect");
            //CheckpointEffect.Parameters["lightPosition"].SetValue(LightPosition);

            base.LoadContent();
        }

        private void UpdateCamera()
        {
            var sphereBackDirection = -Vector3.Transform(Vector3.Forward, SphereRotationMatrix);
            var camera_follow_radius= CAMERA_FOLLOW_RADIUS;
            var camera_up_distance = CAMERA_UP_DISTANCE;
            
            if(FinalBossStage)
            {
                sphereBackDirection = Vector3.UnitZ;
                camera_follow_radius += 40;
            }

            var orbitalPosition = sphereBackDirection * camera_follow_radius;

            var upDistance = Vector3.Up * camera_up_distance;

            Camera.Position = SpherePosition + orbitalPosition + upDistance;
            Camera.TargetPosition = SpherePosition;
            
            LightCamera.Position = new Vector3(SpherePosition.X +1,SpherePosition.Y+1000, SpherePosition.Z+500); //SpherePosition + -sphereBackDirection * 1000 + Vector3.Up * 2500;// new Vector3(SpherePosition.X ,SpherePosition.Y+2500, SpherePosition.Z+200);
            LightCamera.TargetPosition =  SpherePosition;
            
            BoundingFrustum.Matrix = Camera.View * Camera.Projection;
            CubeMapCamera.Position = SpherePosition;
            Camera.BuildView();
            LightCamera.BuildView();
            }
        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            LINEAR_SPEED = SpheresArray[textureIndex].speed();
            SALTO_BUFFER_VALUE = SpheresArray[textureIndex].jump();

            if(PreviousKeyboardState.IsKeyDown(Keys.F10) && KeyboardState.IsKeyUp(Keys.F10))    GodMode = !GodMode;  
            PowerupManager(gameTime);

            //Menu
            if(gameState == GameState.Playing)
            {
                MovementManager();
            }
            else if(gameState == GameState.StartMenu)
            {
                if (PlayButton.IsPressed(PreviousMouseState,MouseState))    gameState = GameState.Playing;
                else if(QuitButton.IsPressed(PreviousMouseState,MouseState))    Exit();
                else if(RightButton.IsPressed(PreviousMouseState,MouseState))   textureIndex = textureIndex >=2? 0:textureIndex+1;
                else if(LeftButton.IsPressed(PreviousMouseState,MouseState))    textureIndex = textureIndex <=0? 2:textureIndex-1;
                else if(MusicEnabledButton.IsPressed(PreviousMouseState,MouseState)&& MediaPlayer.State == MediaState.Playing)  MediaPlayer.Pause();
                else if(MusicEnabledButton.IsPressed(PreviousMouseState,MouseState)&& MediaPlayer.State == MediaState.Paused)      MediaPlayer.Resume();
            }
            else if(gameState == GameState.Paused)
            {
                if (PlayButton.IsPressed(PreviousMouseState,MouseState))    gameState = GameState.Playing;
                else if(RestartButton.IsPressed(PreviousMouseState,MouseState))     {RestoreGame();}
                else if(QuitButton.IsPressed(PreviousMouseState,MouseState))    Exit();
                else if(MusicEnabledButton.IsPressed(PreviousMouseState,MouseState)&& MediaPlayer.State == MediaState.Playing)  MediaPlayer.Pause();
                else if(MusicEnabledButton.IsPressed(PreviousMouseState,MouseState)&& MediaPlayer.State == MediaState.Paused)      MediaPlayer.Resume();
            }
            else if(gameState == GameState.Ended)
            {
                if(RestartButton.IsPressed(PreviousMouseState,MouseState))     {RestoreGame();}
                else if(QuitButton.IsPressed(PreviousMouseState,MouseState))    Exit();
                else if(MusicEnabledButton.IsPressed(PreviousMouseState,MouseState)&& MediaPlayer.State == MediaState.Playing)  MediaPlayer.Pause();
                else if(MusicEnabledButton.IsPressed(PreviousMouseState,MouseState)&& MediaPlayer.State == MediaState.Paused)      MediaPlayer.Resume();
            }

            if (PreviousKeyboardState.IsKeyDown(Keys.Escape) && KeyboardState.IsKeyUp(Keys.Escape) )
            {
                if(gameState == GameState.Playing)
                    gameState = GameState.Paused;
                else if(gameState == GameState.Paused)
                    gameState = GameState.Playing;
            }
            //

            BodyReference body = Simulation.Bodies.GetBodyReference(SphereHandle);
            var prevLinearVelocity = body.Velocity.Linear.Y;
            var prevAngularVelocity = body.Velocity.Angular.Y;
            
            
            if(SpherePosition.Z > 8437 && SpherePosition.Z < 8500 && FinalBossEnabled)
            {
                BossSphereHandle = Loader.LoadFinalBoss();
                FinalBossEnabled = false;
                FinalBossStage = true;
            }

            if(SpherePosition.X>2448 && SpherePosition.Z>12450 )
            {
                gameState = GameState.Ended;
            }
            if(FinalBossStage)
            {
                var bossBodyRef = Simulation.Bodies.GetBodyReference(BossSphereHandle);
                if(bossBodyRef.Pose.Position.Z >12425)
                {
                    bossBodyRef.Velocity.Linear = NumericVector3.Zero;
                    bossBodyRef.Velocity.Angular = NumericVector3.Zero;
                    FinalBossEnabled = false;
                    FinalBossStage = false;
                    FinalBossEnded = true;
                }
            }


            
            CheckpointManager();
            ObstacleContact();

            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            foreach(PeriodicObstacle obstacle in PeriodicObstacles)    {obstacle.UpdateMovement(gameTime);}

            var pose = Simulation.Bodies.GetBodyReference(SphereHandle).Pose;
            SpherePosition = pose.Position;
            SphereWorld = Matrix.CreateScale(BepuSphere.Radius) * Matrix.CreateFromQuaternion(pose.Orientation) * Matrix.CreateTranslation(SpherePosition);
            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);
            var pos2d = new Vector2(SpherePosition.X, SpherePosition.Z);
            if(Vector2.Clamp(pos2d,RegionABottomLeft, RegionATopRight)==pos2d||
            Vector2.Clamp(pos2d,RegionBBottomLeft, RegionBTopRight)==pos2d || Vector2.Clamp(pos2d,RegionCBottomLeft, RegionCTopRight)==pos2d 
            ||Vector2.Clamp(pos2d,RegionDBottomLeft, RegionDTopRight)==pos2d)
            {
                if(SpherePosition.Y<94.9f)
                    {
                        bodyRef.Pose.Position.Y = 95;
                    }
            }

            if(MathHelper.Distance(bodyRef.Velocity.Linear.Y, prevLinearVelocity) < 0.2f 
               && MathHelper.Distance(bodyRef.Velocity.Angular.Y, prevAngularVelocity) < 0.2f) OnGround = true;
            
            if(MediaPlayer.State == MediaState.Stopped)
                MediaPlayer.Play(Song,new TimeSpan(0,0,14));
            
            PreviousMouseState = MouseState;
            PreviousKeyboardState = KeyboardState;

            UpdateCamera();

            base.Update(gameTime);
        }     
        protected void MovementManager(){
            var FinalSpeed = LINEAR_SPEED;
            if(CurrentPowerUp != null)
            {
                FinalSpeed += CurrentPowerUp.SpeedBoost;
            }

            
            if(FinalBossStage)
            {
                SphereRotationMatrix = Matrix.Identity;
                SphereFrontDirection = Vector3.UnitZ;
                SphereLateralDirection = Vector3.UnitX;
            }
            else
            {
                SphereRotationMatrix = Matrix.CreateRotationY(Mouse.GetState().X*-0.01f);
                SphereFrontDirection = Vector3.Transform(Vector3.Backward, SphereRotationMatrix);
                SphereLateralDirection = Vector3.Transform(Vector3.Right, SphereRotationMatrix);
            }
            

            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                if(!PelotaEstaEnElSuelo())
                    bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(-SphereFrontDirection * JUMPING_SPEED));
                else
                     bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(-SphereFrontDirection * FinalSpeed));
            }
            if (KeyboardState.IsKeyDown(Keys.S) && !PelotaSeCayo())
            {
                //SphereVelocity += SphereFrontDirection * LINEAR_SPEED;
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(SphereFrontDirection * FinalSpeed));
            }

            if (KeyboardState.IsKeyDown(Keys.A) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(-SphereLateralDirection * FinalSpeed*0.5f));
            }
            if (KeyboardState.IsKeyDown(Keys.D) && !PelotaSeCayo())
            {
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(SphereLateralDirection * FinalSpeed*0.5f));
            }
            if(GodMode)
            {
                if(PreviousKeyboardState.IsKeyDown(Keys.Right) && KeyboardState.IsKeyUp(Keys.Right)) 
                {
                    respawn = respawn>=8? 0:respawn+1;
                    bodyRef.Pose.Position =   Utils.ToNumericVector3(Checkpoints[respawn].Position);
                }
                
                if(PreviousKeyboardState.IsKeyDown(Keys.Left) && KeyboardState.IsKeyUp(Keys.Left))  
                {
                    respawn = respawn <=0? 2:respawn-1;
                    bodyRef.Pose.Position =   Utils.ToNumericVector3(Checkpoints[respawn].Position);
                }
                   
            }
            
            AdministrarSalto();
        }
        protected void AdministrarSalto(){
            var bodyRef= Simulation.Bodies.GetBodyReference(SphereHandle);
            var FinalImpulse = SALTO_BUFFER_VALUE;
            if(CurrentPowerUp != null)
            {
                FinalImpulse += CurrentPowerUp.JumpBoost;
            }

            if ((KeyboardState.IsKeyDown(Keys.Space)|| KeyboardState.IsKeyDown(Keys.Up))&& PelotaEstaEnElSuelo())
            {
                //SphereVelocity += Vector3.Up * SALTO_BUFFER_VALUE;
                bodyRef.ApplyLinearImpulse(Utils.ToNumericVector3(Vector3.Up * FinalImpulse));
                OnGround = false;
                JumpSound.Play();
            }  
        }

        protected bool PelotaEstaEnElSuelo(){
            return OnGround;
        }

        protected bool PelotaSeCayo(){
            var state = SpherePosition.Y < COORDENADA_Y_MAS_BAJA;
            if(state) LoseSound.Play();
            return state;
        }
        
        private bool drawSkybox= true;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Draw to our cubemap from the Sphere position
            for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ; face++)
            {
                // Set the render target as our cubemap face, we are drawing the scene in this texture
                GraphicsDevice.SetRenderTarget(EnviromentMapRenderTarget, face);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);
                SetCubemapCameraForOrientation(face);
                CubeMapCamera.BuildView();
                DrawScene(gameTime, CubeMapCamera);
            }

            // ShadowMap
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            DefaultEffect.CurrentTechnique = DefaultEffect.Techniques["DepthPass"];
            drawSkybox  = false;
            DrawScene(gameTime, LightCamera);
            // Draw sphere
            SphereModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = DefaultEffect;
            SphereEffect.CurrentTechnique = SphereEffect.Techniques["DepthPass"];
            Utils.SetEffect(LightCamera, DefaultEffect, SphereWorld);
            SphereModel.Meshes.FirstOrDefault().Draw();
            SphereModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = SphereEffect;
            
            // Set the render target as null, we are drawing on the screen!
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
            DefaultEffect.CurrentTechnique = DefaultEffect.Techniques["NormalMapping"];
            DefaultEffect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            DefaultEffect.Parameters["lightPosition"].SetValue(LightCamera.Position);
            DefaultEffect.Parameters["shadowMapSize"].SetValue(Vector2.One * 4096);
            DefaultEffect.Parameters["LightViewProjection"]?.SetValue(LightCamera.View * LightCamera.Projection);
            DrawSkybox(Camera);

            DrawScene(gameTime, Camera);

            // Draw sphere
            string technique = textureIndex==2?"PBR":"EnvironmentMap";
            SphereEffect.CurrentTechnique = SphereEffect.Techniques[technique];
            SphereEffect.Parameters["environmentMap"].SetValue(EnviromentMapRenderTarget);
            SphereEffect.Parameters["ActivePowerUp"]?.SetValue(CurrentPowerUp != null);
            SphereEffect.Parameters["Time"]?.SetValue(Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds));
            SphereEffect.Parameters["eyePosition"].SetValue(Camera.Position);
            Utils.SetEffect(Camera, SphereEffect, SphereWorld);
            SphereModel.Meshes.FirstOrDefault().Draw();

            if(FinalBossStage || FinalBossEnded)
            {
                var pose = Simulation.Bodies.GetBodyReference(BossSphereHandle).Pose;
                var bossWorld = Matrix.CreateScale(45f) * Matrix.CreateFromQuaternion(pose.Orientation) * Matrix.CreateTranslation(pose.Position);
                Utils.SetEffect(Camera,SphereEffect,bossWorld);
                SphereModel.Meshes.FirstOrDefault().Draw();
            }
            
            
            CylinderModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = CheckpointEffect;
            CheckpointEffect.Parameters["Time"]?.SetValue(Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds));
            for(int i = CurrentCheckpoint; i<Checkpoints.Length;i++)
            {
                if(i>CurrentCheckpoint)
                {
                    Utils.SetEffect(Camera,CheckpointEffect,Matrix.CreateScale(6,6,6)*Matrix.CreateTranslation(Checkpoints[i].Position));
                    CylinderModel.Meshes.FirstOrDefault().Draw();
                }
            }
            CylinderModel.Meshes.FirstOrDefault().MeshParts.FirstOrDefault().Effect = DefaultEffect;
            foreach (var powerUp in Powerups)   powerUp.Render(DefaultEffect, gameTime);

            DrawUI(gameTime);
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
            SphereEffect.CurrentTechnique = SphereEffect.Techniques["EnvironmentMap"];

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
            foreach(var sphere in SpheresArray)
            {
                sphere.Ao = Content.Load<Texture2D>(ContentFolderTextures + sphere.folder() + "ao");
                sphere.Metalness = Content.Load<Texture2D>(ContentFolderTextures +  sphere.folder() + "metalness");
                sphere.Roughness = Content.Load<Texture2D>(ContentFolderTextures + sphere.folder() + "roughness");
                sphere.Color = Content.Load<Texture2D>(ContentFolderTextures + sphere.folder() + "color");
                sphere.Normal = Content.Load<Texture2D>(ContentFolderTextures + sphere.folder() +"normal");
            }
            var mainSphere = SpheresArray[textureIndex];
            SphereEffect.Parameters["albedoTexture"]?.SetValue(mainSphere.Color);
            SphereEffect.Parameters["normalTexture"]?.SetValue(mainSphere.Normal);
            SphereEffect.Parameters["metallicTexture"]?.SetValue(mainSphere.Metalness);
            SphereEffect.Parameters["roughnessTexture"]?.SetValue(mainSphere.Roughness);
            SphereEffect.Parameters["aoTexture"]?.SetValue(mainSphere.Ao);
        }

        private void DrawSkybox(Camera camera)
        {
            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;
            SkyBox.Draw(camera.View, camera.Projection, SpherePosition+new Vector3(0,-100,0) );
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
            for(int i= CurrentCheckpoint+1; i< Checkpoints.Length; i++)
            {
                if(Checkpoints[i].IsWithinBounds(bodyRef.Pose.Position))
                {
                    CheckpointSound.Play();
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
                    if(powerUp.IsWithinBounds(bodyRef.Pose.Position, gameTime)&&!powerUp.Used)
                    {
                        CurrentPowerUp = powerUp;
                        PowerupSound.Play();
                        return;
                    }
                }
                return;
            }

            else if(!CurrentPowerUp.IsActive(gameTime))
            {
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

        private void RestoreGame ()
        {
            OnGround = false;
            GodMode = false;
            FinalBossEnabled = true;
            FinalBossEnded = false;
            CurrentCheckpoint = 0;
            SpherePosition = Checkpoints[0].Position;
            foreach (var powerUp in Powerups)    powerUp.Used=false;
            SphereFrontDirection =  Vector3.Backward;
            SphereRotationMatrix = Matrix.Identity;
            var bodyRef = Simulation.Bodies.GetBodyReference(SphereHandle);
            bodyRef.Pose.Position = Utils.ToNumericVector3(SpherePosition);
            bodyRef.Pose.Orientation = Utils.ToSysNumQuaternion(Quaternion.CreateFromRotationMatrix(SphereRotationMatrix));
            gameState = GameState.StartMenu;
        }

        private void SetCubemapCameraForOrientation(CubeMapFace face)
        {            
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    CubeMapCamera.FrontDirection = -Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    CubeMapCamera.FrontDirection = Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    CubeMapCamera.FrontDirection = Vector3.Down;
                    CubeMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    CubeMapCamera.FrontDirection = Vector3.Up;
                    CubeMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    CubeMapCamera.FrontDirection = -Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    CubeMapCamera.FrontDirection = Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }
    
        private void DrawScene(GameTime gameTime, Camera camera)
        {
                if(drawSkybox)
                    DrawSkybox(camera);
                DefaultEffect.Parameters["ModelTexture"].SetValue(FloorT);
                DefaultEffect.Parameters["NormalTexture"].SetValue(FloorN);
                //Dibujo el suelo
                foreach(StaticObstacle obstacle in StaticObstacles)   
                {
                    if(BoundingFrustum.Intersects(obstacle.BoundingBox))
                        obstacle.Render(DefaultEffect,camera,gameTime);
                }            
                //Dibujo los cilindros
                foreach(MovingObstacle obstacle in MovingObstacles)    {obstacle.Render(DefaultEffect,camera,gameTime);}
                foreach(PeriodicObstacle obstacle in PeriodicObstacles)    
                {
                    if(BoundingFrustum.Intersects(obstacle.BoundingBox))
                        obstacle.Render(DefaultEffect,camera,gameTime);
                }
        } 
        private void DrawUI(GameTime gameTime)
        {
             //Hud 
            var Height = GraphicsDevice.Viewport.Height;
            var Width = GraphicsDevice.Viewport.Width;
            var fps = MathF.Round(1/Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds),1);
            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            var position= new Vector3(MathF.Round(SpherePosition.X,1), MathF.Round(SpherePosition.Y,1), MathF.Round(SpherePosition.Z,1));
            SpriteBatch.DrawString(SpriteFont, "GODMODE (F10) :" + (GodMode?"ON":"OFF"), new Vector2(GraphicsDevice.Viewport.Width/4, 0), Color.Black);
            if(GodMode)    SpriteBatch.DrawString(SpriteFont, "<-USE THE ARROW KEYS TO MOVE TO THE NEXT CHECKPOINT->", new Vector2(Width/3, Height*0.9F), Color.Black);
            SpriteBatch.DrawString(SpriteFont, "Position:" + position.ToString(), new Vector2(Width - 500, 0), Color.Black);
            SpriteBatch.DrawString(SpriteFont, "FPS " + fps.ToString(), new Vector2(Width-1000, 0), Color.Black);
            //Menu
            //draw the start menu
            if (gameState == GameState.StartMenu)
            {
                QuitButton.ChangePosition(new Vector2(Width/7f, Height/2));
                PlayButton.Render(SpriteBatch);
                QuitButton.Render(SpriteBatch);
                RightButton.Render(SpriteBatch);
                LeftButton.Render(SpriteBatch);
                if(MediaPlayer.State == MediaState.Paused)
                    MusicDisabledButton.Render(SpriteBatch);
                else
                    MusicEnabledButton.Render(SpriteBatch);
                var mainSphere = SpheresArray[textureIndex];
                SphereEffect.Parameters["albedoTexture"]?.SetValue(mainSphere.Color);
                SphereEffect.Parameters["normalTexture"]?.SetValue(mainSphere.Normal);
                SphereEffect.Parameters["metallicTexture"]?.SetValue(mainSphere.Metalness);
                SphereEffect.Parameters["roughnessTexture"]?.SetValue(mainSphere.Roughness);
                SphereEffect.Parameters["aoTexture"]?.SetValue(mainSphere.Ao);

                
                SpriteBatch.DrawString(SpriteFont, SpheresArray[textureIndex].Name, new Vector2(Width*0.6f, Height*0.45f), Color.Black);
                SpriteBatch.DrawString(SpriteFont, "SPEED: " + SpheresArray[textureIndex].speed().ToString(), new Vector2(Width*0.6f, Height*0.5f), Color.Black);
                SpriteBatch.DrawString(SpriteFont, "JUMP: " + SpheresArray[textureIndex].jump().ToString(), new Vector2(Width*0.6f, Height*0.55f), Color.Black);

            }
 
            if (gameState == GameState.Paused)
            {
                RestartButton.ChangePosition(new Vector2(Width/7f, Height/2.9f)); 
                QuitButton.ChangePosition(new Vector2(Width/7f, Height/2));
                PlayButton.Render(SpriteBatch);
                RestartButton.Render(SpriteBatch);
                QuitButton.Render(SpriteBatch);
                if(MediaPlayer.State == MediaState.Paused)
                    MusicDisabledButton.Render(SpriteBatch);
                else
                    MusicEnabledButton.Render(SpriteBatch);
            }

            if(gameState == GameState.Ended)
            {
                RestartButton.ChangePosition(new Vector2(Width/4f, Height*0.7f));
                RestartButton.Render(SpriteBatch);
                QuitButton.ChangePosition(new Vector2((int)(Width*0.55f), Height*0.7f));
                QuitButton.Render(SpriteBatch);
                if(MediaPlayer.State == MediaState.Paused)
                    MusicDisabledButton.Render(SpriteBatch);
                else
                    MusicEnabledButton.Render(SpriteBatch);

                SpriteBatch.Draw(GameOver, new Rectangle((int)(Width*0.27f),(int)(Height*0.1f),GameOver.Width/5,GameOver.Height/5), Color.White);

                
            }

            SpriteBatch.End();
        }
    }
}