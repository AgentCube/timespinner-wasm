using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.InGame;
using Timespinner.GameStateManagement.Screens.PauseMenu;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class MainMenuScreen : MenuScreen
{
	private const int LocBubbleFrameIndex = 156;

	private const int LocBubbleOffsetX = 2;

	private const int LocBubbleOffsetY = -20;

	private const int LocButtonOffsetX = 20;

	private const int LocButtonOffsetY = 2;

	private const string VersionStringFormat = "v{0}";

	private readonly string _versionNumber;

	private readonly bool _isSaveAvailable;

	private readonly int _entryCount;

	private readonly MenuEntry _continueGameMenuEntry;

	private readonly SaveFileManager _saveFileManager;

	private bool _wasOutOfFocus;

	private int _newGameTargetIndex;

	private int _zoom;

	private PlayerIndex? _newGamePlayerIndex;

	private Vector2 _versionDrawPosition;

	private Vector2 _locBubbleDrawPosition;

	private Vector2 _locButtonDrawPosition;

	private UIButton _locButton;

	private Rectangle _locBubbleFrameSource;

	private SpriteSheet _pauseSprite;

	private SpriteSheet _buttonsSprite;

	public MainMenuScreen(SaveFileManager saveFileManager, bool isSaveAvailable)
		: base("")
	{
		_isSaveAvailable = isSaveAvailable;
		_doesUseBlackGradientBox = false;
		_saveFileManager = saveFileManager;
		_saveFileManager.StartLoadAllSaves();
		if (isSaveAvailable)
		{
			_continueGameMenuEntry = new MenuEntry(Loc.Get("ContinueGame"));
			_continueGameMenuEntry.Selected += ContinueGameMenuEntrySelected;
			base.MenuEntries.Add(_continueGameMenuEntry);
		}
		MenuEntry menuEntry = new MenuEntry(Loc.Get("NewGame"));
		menuEntry.Selected += NewGameMenuEntrySelected;
		base.MenuEntries.Add(menuEntry);
		if (isSaveAvailable)
		{
			MenuEntry menuEntry2 = new MenuEntry(Loc.Get("LoadGame"));
			menuEntry2.Selected += LoadGameMenuEntrySelected;
			base.MenuEntries.Add(menuEntry2);
		}
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("OptionsMenuTitle"));
		menuEntry3.Selected += OptionsMenuEntrySelected;
		base.MenuEntries.Add(menuEntry3);
		MenuEntry menuEntry4 = new MenuEntry(Loc.Get("Exit"));
		menuEntry4.Selected += ExitEntrySelected;
		base.MenuEntries.Add(menuEntry4);
		_entryCount = (isSaveAvailable ? 4 : 2);
		_primaryMenuCollection.SetIsCenterAligned(isCenterAligned: true);
		_primaryMenuCollection.SetDoesDrawLargeShadow(doesDrawLargeShadow: true);
		_versionNumber = string.Format("v{0}", "1.033");
		base.TransitionOffTime = TimeSpan.FromSeconds(0.25);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshLocButton();
		_buttonsSprite = base.ScreenManager.GCM.SpUIButtons;
		_pauseSprite = base.ScreenManager.GCM.SpPauseMenu;
		_locBubbleFrameSource = _pauseSprite.GetFrameSource(156);
		base.ScreenManager.UpdateRichPresence(-1);
		RefreshSizes();
	}

	internal override void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		base.RefreshSizes();
		base.MenuOffset = new Point(0, (int)((base.ScreenManager.MenuFont.MeasureString("N") * _zoom).Y * (float)_entryCount / 2f));
		Rectangle titleSafeArea = base.ScreenManager.TitleSafeArea;
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_versionNumber);
		_versionDrawPosition = new Vector2((float)titleSafeArea.Right - vector.X * (float)_zoom, (float)titleSafeArea.Bottom - vector.Y * (float)_zoom);
		_locBubbleDrawPosition = new Vector2(titleSafeArea.Left + 2 * _zoom, titleSafeArea.Bottom + -20 * _zoom);
		_locButtonDrawPosition = new Vector2(_locBubbleDrawPosition.X + (float)(20 * _zoom), _locBubbleDrawPosition.Y + (float)(2 * _zoom));
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (Constants.InGameZoom != _zoom)
		{
			RefreshSizes();
		}
		if (otherScreenHasFocus != _wasOutOfFocus)
		{
			RefreshLocButton();
		}
		_wasOutOfFocus = otherScreenHasFocus;
	}

	private void RefreshLocButton()
	{
		GameConfigSave configSave = _saveFileManager.ConfigSave;
		if (configSave != null && configSave.MenuControllerMapping != null)
		{
			_locButton = configSave.MenuControllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.Secondary);
		}
	}

	public override void HandleInput(InputState input)
	{
		base.HandleInput(input);
		if (input.IsNewPressSecondary(base.ControllingPlayer))
		{
			ExitScreen();
			base.ScreenManager.AddScreen(new LocLoadingScreen(_saveFileManager.ConfigSave, base.ScreenManager.GCM, isFirstTimeShowing: false), base.ControllingPlayer);
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
		}
	}

	private void NewGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (base.ScreenManager.SaveFileManager.AreSaveFilesFull())
		{
			MessageBoxScreen screen = new MessageBoxScreen(Loc.Get("NewGameNeedDelete"), shouldIncludeUsageText: false, base.ScreenManager.MenuControllerMapping);
			base.ScreenManager.AddScreen(screen, e.PlayerIndex);
		}
		else
		{
			NewGamePickDifficulty(e, -1);
		}
	}

	private void ContinueGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_saveFileManager.IsFinishedLoading)
		{
			GameSave newestSave = _saveFileManager.GetNewestSave();
			if (newestSave != null && !newestSave.IsCorrupt)
			{
				LoadGame(e.PlayerIndex, newestSave);
				return;
			}
			PlayErrorSound();
			_continueGameMenuEntry.DoesConfirmationPlaySound = false;
		}
		else
		{
			base.ScreenManager.AddScreen(new SaveLoadWaitScreen(isLoadingSaveSelect: false), e.PlayerIndex);
			base.ScreenManager.RemoveScreen(this);
		}
	}

	private void LoadGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_saveFileManager.IsFinishedLoading)
		{
			GameScreen[] screens = base.ScreenManager.GetScreens();
			foreach (GameScreen gameScreen in screens)
			{
				gameScreen.ExitScreen();
			}
			base.ScreenManager.AddScreen(new SaveSelectScreen(_saveFileManager, base.ScreenManager.GCM), base.ControllingPlayer);
		}
		else
		{
			base.ScreenManager.AddScreen(new SaveLoadWaitScreen(isLoadingSaveSelect: true), e.PlayerIndex);
			base.ScreenManager.RemoveScreen(this);
		}
	}

	private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		GameConfigSave configSave = base.ScreenManager.SaveFileManager.ConfigSave;
		base.ScreenManager.AddScreen(new OptionsMenuScreen(null, configSave, base.ScreenManager.GCM, null), e.PlayerIndex);
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
		base.ScreenManager.AddScreen(new TitleScreen(_isSaveAvailable), null);
		base.ScreenManager.RemoveScreen(this);
	}

	private void ExitEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("ExitGameConfirm"), base.ScreenManager.MenuControllerMapping);
		messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, e.PlayerIndex);
	}

	private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.Game.Exit();
	}

	private void NewGamePickDifficulty(PlayerIndexEventArgs e, int saveIndex)
	{
		Console.WriteLine($"[MainMenuScreen] NewGamePickDifficulty: saveIndex={saveIndex}");
		_newGameTargetIndex = saveIndex;
		_newGamePlayerIndex = e.PlayerIndex;
		GameDifficultyMenu screen = new GameDifficultyMenu(_saveFileManager, OnDifficultySelected);
		base.ScreenManager.AddScreen(screen, _newGamePlayerIndex);
	}

	private void OnDifficultySelected(GameSave.EGameDifficultyType difficulty)
	{
		Console.WriteLine($"[MainMenuScreen] OnDifficultySelected: {difficulty}");
		LoadGame(_newGamePlayerIndex, CreateNewSave(_newGameTargetIndex, _saveFileManager, difficulty));
	}

	private void LoadGame(PlayerIndex? playerIndex, GameSave gameSave)
	{
		Console.WriteLine($"[MainMenuScreen] LoadGame: starting LoadingScreen for GameplayScreen...");
		LoadingScreen.Load(base.ScreenManager, true, playerIndex, new GameplayScreen(gameSave, _saveFileManager.ConfigSave));
	}

	internal static GameSave CreateNewSave(int newIndex, SaveFileManager saveFileManager, GameSave.EGameDifficultyType gameDifficulty)
	{
		if (newIndex == -1)
		{
			newIndex = saveFileManager.GetNextSaveIndex();
		}
		return GameSave.CreateNewSave(newIndex, gameDifficulty);
	}

	public override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);
		SpriteFont menuFont = base.ScreenManager.MenuFont;
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		byte transitionAlpha = base.TransitionAlpha;
		float num = (float)(int)transitionAlpha / 255f;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
		Color color = new Color(128, 128, 128) * num;
		DrawingEx.DrawString(spriteBatch, menuFont, _versionNumber, _versionDrawPosition, color, Vector2.Zero, _zoom);
		Color color2 = new Color(200, 200, 200) * num;
		spriteBatch.Draw(_pauseSprite.Texture, _locBubbleDrawPosition, _locBubbleFrameSource, color2, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		_locButton.Draw(spriteBatch, _buttonsSprite, menuFont, _locButtonDrawPosition, color2, _zoom, -1);
		spriteBatch.End();
	}
}
