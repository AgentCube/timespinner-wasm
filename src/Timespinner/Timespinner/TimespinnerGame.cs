using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Constants;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.MainMenu;

namespace Timespinner;

public class TimespinnerGame : Game
{
	private const int TargetFrameRate = 60;

	private readonly PlatformHelper _platformHelper;

	private readonly GraphicsDeviceManager _graphics;

	private readonly ScreenManager _screenManager;

	internal EGameResolutionType StartingResolution { get; private set; }

	internal GraphicsDeviceManager Graphics => _graphics;

	public TimespinnerGame(PlatformHelper platformHelper)
	{
		_platformHelper = platformHelper;
		StartingResolution = EGameResolutionType.R1280_720;
		Constants.InGameZoom = GraphicsEx.GetGameZoomFromType(StartingResolution);
		Point gameResolutionFromType = GraphicsEx.GetGameResolutionFromType(StartingResolution);
		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = gameResolutionFromType.X,
			PreferredBackBufferHeight = gameResolutionFromType.Y,
			GraphicsProfile = GraphicsProfile.Reach
		};
		base.Content.RootDirectory = "Content";
		base.TargetElapsedTime = TimeSpan.FromTicks(166666L);
		base.IsFixedTimeStep = false;
		_screenManager = new ScreenManager(this, _platformHelper);
		_graphics.DeviceReset += OnGraphicsDeviceReset;
		base.Window.AllowUserResizing = true;
		base.Components.Add(_screenManager);
		_screenManager.AddScreen(new PublisherSplashScreen(), null);
	}

	protected override void Draw(GameTime gameTime)
	{
		_graphics.GraphicsDevice.Clear(Color.Black);
		base.Draw(gameTime);
	}

	private void OnExitGame(object sender, EventArgs e)
	{
		_platformHelper.Shutdown();
	}

	private void OnGraphicsDeviceReset(object sender, EventArgs e)
	{
		_screenManager.OnScreenSizeChanged();
	}

	protected override void Initialize()
	{
		_platformHelper.Initialize();
		base.Exiting += OnExitGame;
		base.Initialize();
	}

	protected override void Update(GameTime gameTime)
	{
		_platformHelper.Update();
		base.Update(gameTime);
	}
}
