using Microsoft.Xna.Framework;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class SaveLoadWaitScreen : MenuScreen
{
	private readonly bool _isLoadingSaveSelect;

	private readonly string _messageText;

	private bool _hasFinishedLoading;

	private int _zoom;

	public SaveLoadWaitScreen(bool isLoadingSaveSelect)
		: base("")
	{
		_doesUseBlackGradientBox = false;
		_doesUseCursor = false;
		_isLoadingSaveSelect = isLoadingSaveSelect;
		_messageText = Loc.Get(_isLoadingSaveSelect ? "SaveLoadWaitPlural" : "SaveLoadWaitSingle");
		MenuEntry item = new MenuEntry(_messageText)
		{
			DoesDrawLargeShadow = true,
			DoesConfirmationPlaySound = false
		};
		base.MenuEntries.Add(item);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
	}

	internal override void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		base.RefreshSizes();
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_messageText) * _zoom;
		base.MenuOffset = new Point(-(int)(vector.X / 2f), (int)vector.Y);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (_hasFinishedLoading)
		{
			return;
		}
		SaveFileManager saveFileManager = base.ScreenManager.SaveFileManager;
		if (!saveFileManager.IsFinishedLoading)
		{
			return;
		}
		_hasFinishedLoading = true;
		if (_isLoadingSaveSelect)
		{
			GameScreen[] screens = base.ScreenManager.GetScreens();
			foreach (GameScreen gameScreen in screens)
			{
				gameScreen.ExitScreen();
			}
			base.ScreenManager.AddScreen(new SaveSelectScreen(saveFileManager, base.ScreenManager.GCM), base.ControllingPlayer);
		}
		else
		{
			GameSave newestSave = saveFileManager.GetNewestSave();
			if (newestSave != null && !newestSave.IsCorrupt)
			{
				LoadGame(base.ControllingPlayer, newestSave);
				return;
			}
			PlayErrorSound();
			base.ScreenManager.AddScreen(new MainMenuScreen(base.ScreenManager.SaveFileManager, isSaveAvailable: true), base.ControllingPlayer);
			base.ScreenManager.RemoveScreen(this);
		}
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
	}

	private void LoadGame(PlayerIndex? playerIndex, GameSave gameSave)
	{
		LoadingScreen.Load(base.ScreenManager, true, playerIndex, new GameplayScreen(gameSave, base.ScreenManager.SaveFileManager.ConfigSave));
	}
}
