using Microsoft.Xna.Framework;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameStateManagement.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class SaveValidationScreen : MenuScreen
{
	private readonly PlayerIndex? _controllingPlayer;

	private readonly string _checkingText;

	private bool _isAskingPlayerSaveChoices;

	private bool _hasPlayerResponded;

	private bool _isPlayerGoingForward;

	private int _zoom;

	public SaveValidationScreen(PlayerIndex? controllingPlayer)
		: base("")
	{
		_controllingPlayer = controllingPlayer;
		_doesUseBlackGradientBox = false;
		_doesUseCursor = false;
		_checkingText = Loc.Get("SaveValidationChecking");
		MenuEntry item = new MenuEntry(_checkingText)
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
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_checkingText) * _zoom;
		base.MenuOffset = new Point(-(int)(vector.X / 2f), (int)vector.Y);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!_isAskingPlayerSaveChoices)
		{
			SaveFileManager saveFileManager = base.ScreenManager.SaveFileManager;
			if (saveFileManager.IsFinishedCheckingForSaves)
			{
				bool? wasSaveAvailable = saveFileManager.WasSaveAvailable;
				if (wasSaveAvailable.HasValue)
				{
					GoToMainMenu(wasSaveAvailable.Value);
					return;
				}
				_isAskingPlayerSaveChoices = true;
				XnaSaveLoad.XnaNotifyPlayerOfSaveValidationFailure(OnSaveValidationFinished, base.ScreenManager, _controllingPlayer);
			}
		}
		else if (_hasPlayerResponded)
		{
			if (_isPlayerGoingForward)
			{
				GoToMainMenu(isSaveAvailable: false);
			}
			else
			{
				ReturnToTitleScreen();
			}
		}
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
	}

	private void ReturnToTitleScreen()
	{
		base.ScreenManager.AddScreen(new TitleScreen(null)
		{
			DoesNeedToStartLoadCheck = true
		}, null);
		base.ScreenManager.RemoveScreen(this);
	}

	private void GoToMainMenu(bool isSaveAvailable)
	{
		base.ScreenManager.AddScreen(new MainMenuScreen(base.ScreenManager.SaveFileManager, isSaveAvailable), _controllingPlayer);
		base.ScreenManager.RemoveScreen(this);
	}

	private void OnSaveValidationFinished(bool shouldGoForward)
	{
		_hasPlayerResponded = true;
		_isPlayerGoingForward = shouldGoForward;
	}
}
