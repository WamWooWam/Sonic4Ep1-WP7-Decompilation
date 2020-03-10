﻿using System;
using System.Diagnostics;
using System.Security.Cryptography;
using GameFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using mpp;
using Sonic4Episode1.Abstraction;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;

// Token: 0x020003E1 RID: 993
public class Sonic4Ep1 : Game
{
    private bool _resizePending = true;

    // Token: 0x06002879 RID: 10361 RVA: 0x00152E78 File Offset: 0x00151078
    public Sonic4Ep1()
    {
        pInstance = this;
        this.graphics = new GraphicsDeviceManager(this);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChange;
        this.graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        this.graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        this.graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
        this.graphics.PreparingDeviceSettings += this.graphics_PreparingDeviceSettings;
        this.graphics.SynchronizeWithVerticalRetrace = true;
#if UWP || __IOS__ || __ANDROID__
        this.graphics.IsFullScreen = true;
#else
        this.graphics.IsFullScreen = false;
#endif
        base.IsMouseVisible = true;
        base.Content.RootDirectory = "Content";
        base.TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        base.Activated += this.OnActivated;
        base.Deactivated += this.OnDeactivated;
    }

    public void SetControllerSource(IControllerSource controllerSource)
    {
        this.controllerSource = controllerSource;
    }

    public void SetAccelerometer(IAccelerometer accelerometer)
    {
        if (accelerometer.IsSupported())
        {
            this.accelerometer = accelerometer;
            this.accelerometer.ReadingChanged += accelerometer_ReadingChanged;
            this.accelerometer.Initialize();
        }
    }

    public void SetSaveContentPath(string path)
    {
        this.saveContentPath = path;
    }

    private void OnClientSizeChange(object sender, EventArgs e)
    {
        _resizePending = true;
    }

    // Token: 0x0600287A RID: 10362 RVA: 0x00152F69 File Offset: 0x00151169
    private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        PresentationParameters presentationParameters = e.GraphicsDeviceInformation.PresentationParameters;
    }

    // Token: 0x0600287B RID: 10363 RVA: 0x00152F78 File Offset: 0x00151178
    protected override void Initialize()
    {
        this.scissorState = new RasterizerState
        {
            ScissorTestEnable = true,
            CullMode = CullMode.None
        };
        Guide.IsScreenSaverEnabled = false;
        base.Initialize();
    }

    // Token: 0x0600287C RID: 10364 RVA: 0x00152FB8 File Offset: 0x001511B8
    protected override void LoadContent()
    {
        this.spriteBatch = new SpriteBatch(base.GraphicsDevice);
        this.fntKootenay = base.Content.Load<SpriteFont>("Kootenay");
        this.fnts[0] = base.Content.Load<SpriteFont>("small");
        this.fnts[1] = base.Content.Load<SpriteFont>("medium");
        this.fnts[2] = base.Content.Load<SpriteFont>("large");

//#if DEBUG
        this.benchmarkObject = new BenchmarkObject(this, fntKootenay, new Vector2(0, 0), Color.Red);
//#endif
        try
        {
            this.appMain = new AppMain(this, this.graphics, base.GraphicsDevice);
            this.appMain.AppInit(saveContentPath, this.controllerSource, Window.ClientBounds);
        }
        catch (Exception)
        {
        }
    }

    // Token: 0x0600287D RID: 10365 RVA: 0x001530B8 File Offset: 0x001512B8
    protected override void OnDeactivated(object sender, EventArgs args)
    {
        AppMain.isForeground = false;
        if (SaveState.saveLater)
        {
            SaveState._saveFile(SaveState.save);
        }

        //if (!Guide.IsVisible)
        //{
        //	this.storeSystemVolume = true;
        //	try
        //	{
        //		if (!AppMain.g_ao_sys_global.is_playing_device_bgm_music)
        //		{
        //			MediaPlayer.Pause();
        //		}
        //		MediaPlayer.Volume = this.deviceMusicVolume;
        //		return;
        //	}
        //	catch (Exception)
        //	{
        //		return;
        //	}
        //}                                   
        //this.storeSystemVolume = false;
    }

    // Token: 0x0600287E RID: 10366 RVA: 0x00153128 File Offset: 0x00151328
    protected override void OnActivated(object sender, EventArgs args)
    {
        AppMain.isForeground = true;
        if (this.storeSystemVolume)
        {
            this.deviceMusicVolume = XnaMediaPlayer.Volume;
        }

        if ((AppMain.g_gm_main_system.game_flag & 64U) == 0U)
        {
            AppMain.g_pause_flag = true;
        }
    }

    // Token: 0x0600287F RID: 10367 RVA: 0x00153158 File Offset: 0x00151358
    private void accelerometer_ReadingChanged(object sender, AccelerometerChangeEventArgs e)
    {
        this.accel.X = (float) e.X;
        this.accel.Y = (float) e.Y;
        this.accel.Z = (float) e.Z;
    }

    // Token: 0x06002880 RID: 10368 RVA: 0x00153190 File Offset: 0x00151390
    protected override void UnloadContent()
    {
    }

    // Token: 0x06002881 RID: 10369 RVA: 0x00153194 File Offset: 0x00151394
    protected override void Update(GameTime gameTime)
    {
        AppMain.lastGameTime = gameTime;
        if (Sonic4Ep1.inputDataRead)
        {
            Sonic4Ep1.inputDataRead = false;


            AppMain.onTouchEvents();
            AppMain.amIPhoneAccelerate(ref this.accel);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                AppMain.back_key_is_pressed = true;
            }
        }

        benchmarkObject?.Update(gameTime);

        base.Update(gameTime);
    }

    // Token: 0x06002882 RID: 10370 RVA: 0x00153224 File Offset: 0x00151424
    protected override void Draw(GameTime gameTime)
    {
        if (_resizePending)
        {
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            graphics.ApplyChanges();

            appMain?.amIPhoneInitNN(Window.ClientBounds);

            _resizePending = false;
        }

        Sonic4Ep1.inputDataRead = true;
        OpenGL.drawPrimitives_Count = 0;
        OpenGL.drawVertexBuffer_Count = 0;
        this.appMain.AppMainLoop();

        if (benchmarkObject != null)
        {
            // no point initing a sprite batch if no cunt uses it
            this.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            benchmarkObject.Draw(gameTime, this.spriteBatch);
            this.spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    // Token: 0x040062A9 RID: 25257
    public static Sonic4Ep1 pInstance;

    // Token: 0x040062AA RID: 25258
    private GraphicsDeviceManager graphics;

    // Token: 0x040062AB RID: 25259
    private SpriteFont fntKootenay;

    // Token: 0x040062AC RID: 25260
    public SpriteFont[] fnts = new SpriteFont[3];

    // Token: 0x040062AD RID: 25261
    private int GCCount;

    // Token: 0x040062AE RID: 25262
    private WeakReference wr = new WeakReference(new object());

    // Token: 0x040062AF RID: 25263
    private double _lastUpdateMilliseconds;

    // Token: 0x040062B0 RID: 25264
    public static bool cheat = false;

    // Token: 0x040062B1 RID: 25265
    public RasterizerState scissorState;

    // Token: 0x040062B2 RID: 25266
    private IAccelerometer accelerometer;

    // Token: 0x040062B3 RID: 25267
    private AppMain appMain;

    // Token: 0x040062B4 RID: 25268
    public SpriteBatch spriteBatch;

    // Token: 0x040062B5 RID: 25269
    protected float deviceMusicVolume;

    // Token: 0x040062B6 RID: 25270
    protected bool storeSystemVolume = true;

    // Token: 0x040062B7 RID: 25271
    private Vector3 accel;

    // Token: 0x040062B8 RID: 25272
    private static bool inputDataRead = true;
    private BenchmarkObject benchmarkObject;
    private string saveContentPath;
    private IControllerSource controllerSource;
}