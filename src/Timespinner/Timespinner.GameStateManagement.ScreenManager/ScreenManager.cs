using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Saving;

namespace Timespinner.GameStateManagement.ScreenManager;

public class ScreenManager : DrawableGameComponent
{
	private const float GameVersionDrawOffsetX = -8f;

	private const float GameVersionDrawOffsetY = -12f;

	private const float DefaultTitleSafeAreaMultiplier = 0.05f;

	private const float HDTitleSafeAreaMultiplier = 0.025f;

	private readonly InputState _input = new InputState();

	private readonly SaveFileManager _saveFileManager;

	private readonly PlatformHelper _platformHelper;

	private readonly TimespinnerGame _game;

	private readonly List<GameScreen> _screens = new List<GameScreen>();

	private readonly List<GameScreen> _screensToUpdate = new List<GameScreen>();

	private bool _isInitialized;

	private bool _isConfigFileLoaded;

	private bool _isLoadingConfig;

	private bool _hasRefreshScreenAfterLoad;

	private readonly bool _doesDrawGameVersion;

	private bool _doesDrawTitleSafeArea;

	private bool _isScreenDebugTraceEnabled;

	private readonly string _gameVersionText;

	private Point _screenSize;

	private Vector2 _screenCenter;

	private Vector2 _gameVersionDrawPosition;

	private Rectangle _titleSafeArea;

	private Rectangle _viewPortArea;

	private Rectangle _smallScreenRect;

	private SpriteBatch _spriteBatch;

	private SpriteFont _menuFont;

	private ContentManager _content;

	private ContentManager _localizedContent;

	internal bool IsFullscreen => _game.Graphics.IsFullScreen;

	internal EGameResolutionType CurrentResolution { get; private set; }

	public Point ScreenSize => _screenSize;

	public Rectangle TitleSafeArea => _titleSafeArea;

	public Rectangle ViewPortArea => _viewPortArea;

	public Rectangle SmallScreenRect => _smallScreenRect;

	public Vector2 ScreenCenter => _screenCenter;

	public ControllerMapping MenuControllerMapping => _saveFileManager.ConfigSave.MenuControllerMapping;

	public Texture2D BlankTexture { get; private set; }

	public Texture2D UIBlackGradientBox { get; private set; }

	public SpriteSheet UIMenuArrow { get; private set; }

	public SpriteSheet UITextBox { get; private set; }

	public SpriteSheet UIControllerButtons { get; private set; }

	public SpriteFont MenuFont => _menuFont;

	public Jukebox Jukebox { get; private set; }

	internal GCM GCM { get; private set; }

	internal ContentManager GeneralContentManager => _content;

	internal ContentManager LocalizedContentManager => _localizedContent;

	public SpriteBatch SpriteBatch => _spriteBatch;

	internal SaveFileManager SaveFileManager => _saveFileManager;

	public ScreenManager(TimespinnerGame game, PlatformHelper platformHelper)
		: base(game)
	{
		_game = game;
		_platformHelper = platformHelper;
		CurrentResolution = _game.StartingResolution;
		_saveFileManager = new SaveFileManager(this);
	}

	public GameScreen[] GetScreens()
	{
		return _screens.ToArray();
	}

	public override void Initialize()
	{
		GCM = new GCM();
		Jukebox = new Jukebox(shouldLoad: true, base.Game.Services);
		base.Initialize();
		_isInitialized = true;
		OnScreenSizeChanged();
	}

	protected override void LoadContent()
	{
		_content = base.Game.Content;
		_localizedContent = new ContentManager(base.Game.Services, "Content");
		GCM.LoadUI(_content);
		GCM.LoadShaders(_content);
		_spriteBatch = new SpriteBatch(base.GraphicsDevice);
		ChangeResolution(CurrentResolution, isForcingWindowed: true);
		_menuFont = GCM.ActiveFont;
		BlankTexture = GCM.TxBlankSquare;
		UIBlackGradientBox = GCM.TxUIBlackGradient;
		UIMenuArrow = GCM.SpMenuCursor;
		UITextBox = GCM.SpTextBox;
		UIControllerButtons = GCM.SpUIButtons;
		SaveFileManager.ReleaseDevice();
		SaveFileManager.StorageAsyncResult = StorageDevice.BeginShowSelector(null, null);
		SaveFileManager.TryGetStorageDevice();
		SaveFileManager.LoadConfigFile();
		_isConfigFileLoaded = true;
		OnLoadConfigFile(shouldLoadLoc: true);
		foreach (GameScreen screen in _screens)
		{
			screen.LoadContent();
		}
	}

	protected override void UnloadContent()
	{
		foreach (GameScreen screen in _screens)
		{
			screen.UnloadContent();
		}
		Jukebox.UnloadContent();
		GCM.Dispose();
		if (_localizedContent != null)
		{
			_localizedContent.Unload();
		}
	}

	internal void OnLoadConfigFile(bool shouldLoadLoc)
	{
		GameConfigSave configSave = SaveFileManager.ConfigSave;
		if (shouldLoadLoc)
		{
			ELanguageLocale loc = (ELanguageLocale)(configSave.HasPickedLocale ? configSave.LocaleType : (configSave.LocaleType = (int)_platformHelper.GuessLocale()));
			SwitchLocale(loc);
		}
		RefreshMenuControllerMapping();
		Jukebox.SetVolumesBySave(configSave);
		if (configSave.IsFullScreen)
		{
			ToggleFullscreen();
		}
		else if (configSave.ScreenResolutionType != 0)
		{
			ChangeResolution((EGameResolutionType)configSave.ScreenResolutionType, isForcingWindowed: true);
		}
		Constants.IsKeyboardPreferred = configSave.IsKeyboardPreferred;
		Constants.ButtonDisplayType = configSave.ButtonDisplayType;
	}

	internal void SwitchLocale(ELanguageLocale loc)
	{
		Loc.SwitchLoc(loc);
		if (_localizedContent != null)
		{
			_localizedContent.Unload();
			GCM.LoadLocalizedContent(_localizedContent, loc);
		}
		_menuFont = GCM.ActiveFont;
	}

	internal void RefreshMenuControllerMapping()
	{
		_input.ControllerMapping = _saveFileManager.ConfigSave.MenuControllerMapping;
	}

	public override void Update(GameTime gameTime)
	{
		UpdateInput();
		float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
		Jukebox.Update(delta);
		UpdateScreens(gameTime);
		_saveFileManager.Update(delta);
		if (_isScreenDebugTraceEnabled)
		{
			DebugTraceScreens();
		}
	}

	private void UpdateInput()
	{
		bool doesAllowControllerVibration = false;
		int count = _screens.Count;
		if (count > 0 && _screens[count - 1].IsVibrationEnabled)
		{
			doesAllowControllerVibration = true;
		}
		_input.Update(doesAllowControllerVibration);
	}

	private void UpdateScreens(GameTime gameTime)
	{
		_screensToUpdate.Clear();
		foreach (GameScreen screen in _screens)
		{
			_screensToUpdate.Add(screen);
		}
		bool flag = !base.Game.IsActive;
		bool isCoveredByOtherScreen = false;
		while (_screensToUpdate.Count > 0)
		{
			GameScreen gameScreen = _screensToUpdate[_screensToUpdate.Count - 1];
			_screensToUpdate.RemoveAt(_screensToUpdate.Count - 1);
			gameScreen.Update(gameTime, flag, isCoveredByOtherScreen);
			if (gameScreen.ScreenState != 0 && gameScreen.ScreenState != EScreenState.Active)
			{
				continue;
			}
			if (!flag)
			{
				gameScreen.HandleInput(_input);
				if (!gameScreen.IsOverlayScreen)
				{
					flag = true;
				}
			}
			if (!gameScreen.IsPopupScreen)
			{
				isCoveredByOtherScreen = true;
			}
		}
	}

	private void DebugTraceScreens()
	{
		List<string> list = new List<string>();
		foreach (GameScreen screen in _screens)
		{
			list.Add(screen.GetType().Name);
		}
		Console.WriteLine(string.Join(", ", list.ToArray()));
	}

	public override void Draw(GameTime gameTime)
	{
		foreach (GameScreen screen in _screens)
		{
			if (screen.ScreenState != EScreenState.Hidden)
			{
				screen.Draw(gameTime);
			}
		}
		if (SpriteBatch != null && _saveFileManager.ConfigSave != null && _saveFileManager.ConfigSave.DoesDrawBorderFrame)
		{
			SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			GCM.DrawScreenBorderFrame(SpriteBatch, _smallScreenRect, _screenSize);
			SpriteBatch.End();
		}
		DrawDebug();
	}

	public void DrawTextBox(SpriteBatch spriteBatch, Rectangle backgroundRectangle, Color color)
	{
		DrawingEx.DrawTextBox(spriteBatch, backgroundRectangle, color, UITextBox);
	}

	private void DrawDebug()
	{
		if (_doesDrawTitleSafeArea)
		{
			_spriteBatch.Begin();
			_spriteBatch.DrawRectangleBorder(BlankTexture, _titleSafeArea, 1, Color.Red);
			_spriteBatch.End();
		}
		if (_doesDrawGameVersion)
		{
			int inGameZoom = Constants.InGameZoom;
			_gameVersionDrawPosition = new Vector2((float)_titleSafeArea.Left + -8f * (float)Constants.InGameZoom, (float)_titleSafeArea.Top + -12f * (float)Constants.InGameZoom);
			Vector2 drawPos = new Vector2(_gameVersionDrawPosition.X, _gameVersionDrawPosition.Y + (float)inGameZoom);
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			DrawingEx.DrawString(_spriteBatch, _menuFont, _gameVersionText, drawPos, Color.Black * 0.5f, Vector2.Zero, inGameZoom);
			DrawingEx.DrawString(_spriteBatch, _menuFont, _gameVersionText, _gameVersionDrawPosition, new Color(208, 200, 152) * 0.5f, Vector2.Zero, inGameZoom);
			_spriteBatch.End();
		}
	}

	public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
	{
		Console.WriteLine($"[ScreenManager] +AddScreen: {screen.GetType().Name} (Total: {_screens.Count + 1})");
		screen.ControllingPlayer = controllingPlayer;
		screen.ScreenManager = this;
		screen.IsExiting = false;
		if (_isInitialized)
		{
			screen.LoadContent();
		}
		_screens.Add(screen);
	}

	internal void PreloadScreen(GameScreen screen, PlayerIndex? controllingPlayer)
	{
		Console.WriteLine($"[ScreenManager] PreloadScreen: {screen.GetType().Name}");
		screen.ControllingPlayer = controllingPlayer;
		screen.ScreenManager = this;
		screen.IsExiting = false;
		if (_isInitialized)
		{
			screen.LoadContent();
		}
	}

	internal void AddPreloadedScreen(GameScreen screen)
	{
		Console.WriteLine($"[ScreenManager] +AddPreloadedScreen: {screen.GetType().Name} (Total: {_screens.Count + 1})");
		_screens.Add(screen);
	}

	public void RemoveScreen(GameScreen screen)
	{
		Console.WriteLine($"[ScreenManager] -RemoveScreen: {screen.GetType().Name} (Remaining: {_screens.Count - 1})");
		if (_isInitialized)
		{
			screen.UnloadContent();
		}
		_screens.Remove(screen);
		_screensToUpdate.Remove(screen);
	}

	public void FadeBackBufferToBlack(int alpha)
	{
		Viewport viewport = base.GraphicsDevice.Viewport;
		_spriteBatch.Begin();
		_spriteBatch.Draw(BlankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, (byte)alpha));
		_spriteBatch.End();
	}

	internal void OnScreenSizeChanged()
	{
		if (!_isInitialized)
		{
			return;
		}
		Rectangle bounds = base.GraphicsDevice.Viewport.Bounds;
		Point size = new Point(bounds.Width, bounds.Height);
		EGameResolutionType nearestResolutionTypeFromSize = GraphicsEx.GetNearestResolutionTypeFromSize(size);
		if (nearestResolutionTypeFromSize != CurrentResolution)
		{
			CurrentResolution = nearestResolutionTypeFromSize;
			Constants.InGameZoom = GraphicsEx.GetGameZoomFromType(nearestResolutionTypeFromSize);
		}
		_titleSafeArea = base.GraphicsDevice.Viewport.TitleSafeArea;
		_screenSize = new Point(base.GraphicsDevice.Viewport.Width, base.GraphicsDevice.Viewport.Height);
		_viewPortArea = new Rectangle(0, 0, _screenSize.X, _screenSize.Y);
		_screenCenter = new Vector2((float)_viewPortArea.Width / 2f, (float)_viewPortArea.Height / 2f);
		Point point = new Point(400 * Constants.InGameZoom, 240 * Constants.InGameZoom);
		_smallScreenRect = new Rectangle((_screenSize.X - point.X) / 2, (_screenSize.Y - point.Y) / 2, point.X, point.Y);
		if (_titleSafeArea.Left == 0 && _titleSafeArea.Top == 0)
		{
			int num = (int)((float)_titleSafeArea.Width * 0.05f);
			int num2 = (int)((float)_titleSafeArea.Height * 0.05f);
			int width = _titleSafeArea.Width - num * 2;
			int height = _titleSafeArea.Height - num2 * 2;
			_titleSafeArea = new Rectangle(num, num2, width, height);
		}
		int num3 = 240 * Constants.InGameZoom;
		int num4 = _viewPortArea.Height - num3;
		if (num4 > 0)
		{
			int num5 = num4 / 2;
			int num6 = (int)((float)num3 * 0.025f);
			int num7 = 400 * Constants.InGameZoom;
			int num8 = (_viewPortArea.Width - num7) / 2;
			int num9 = (int)((float)num7 * 0.025f);
			int num10 = Math.Max(num8 + num9, _titleSafeArea.Left);
			int num11 = Math.Max(num5 + num6, _titleSafeArea.Top);
			int width2 = _viewPortArea.Width - num10 * 2;
			int height2 = _viewPortArea.Height - num11 * 2;
			_titleSafeArea = new Rectangle(num10, num11, width2, height2);
		}
		GCM.SetScreenSizeAndTitleSafeArea(new Point(_viewPortArea.Width, _viewPortArea.Height), _titleSafeArea);
		int count = _screens.Count;
		for (int num12 = count - 1; num12 >= 0; num12--)
		{
			try
			{
				GameScreen gameScreen = _screens[num12];
				gameScreen.OnScreenResize();
			}
			catch
			{
			}
		}
	}

	internal void ChangeResolution(EGameResolutionType resolution, bool isForcingWindowed)
	{
		if (_game.Graphics.IsFullScreen && isForcingWindowed)
		{
			_game.Graphics.IsFullScreen = false;
		}
		CurrentResolution = resolution;
		Constants.InGameZoom = GraphicsEx.GetGameZoomFromType(resolution);
		Point gameResolutionFromType = GraphicsEx.GetGameResolutionFromType(resolution);
		_game.Graphics.PreferredBackBufferWidth = gameResolutionFromType.X;
		_game.Graphics.PreferredBackBufferHeight = gameResolutionFromType.Y;
		_game.Graphics.ApplyChanges();
	}

	internal void ToggleFullscreen()
	{
		bool isFullScreen = !_game.Graphics.IsFullScreen;
		DisplayMode currentDisplayMode = _game.Graphics.GraphicsDevice.Adapter.CurrentDisplayMode;
		EGameResolutionType nearestResolutionTypeFromSize = GraphicsEx.GetNearestResolutionTypeFromSize(new Point(currentDisplayMode.Width, currentDisplayMode.Height));
		_game.Graphics.IsFullScreen = isFullScreen;
		ChangeResolution(nearestResolutionTypeFromSize, isForcingWindowed: false);
	}

	internal void ToggleBorderDraw()
	{
		SaveFileManager.ConfigSave.DoesDrawBorderFrame = !SaveFileManager.ConfigSave.DoesDrawBorderFrame;
	}

	internal void UpdateRichPresence(int levelIndex)
	{
		_platformHelper.UpdateRichPresence(levelIndex);
	}
}
