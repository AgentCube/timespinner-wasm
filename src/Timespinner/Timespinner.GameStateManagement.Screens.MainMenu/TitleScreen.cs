using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class TitleScreen : MenuScreen
{
	private readonly string _pressStartText;

	private bool _areControllersConnected;

	private bool? _doesSaveSaveFileExist;

	private int _zoom;

	private PlayerIndex _personWhoPressed;

	internal bool DoesNeedToStartLoadCheck { get; set; }

	public TitleScreen(bool? doesSaveFileExist)
		: base(string.Empty)
	{
		_doesSaveSaveFileExist = doesSaveFileExist;
		_doesUseBlackGradientBox = false;
		_doesUseCursor = false;
		_pressStartText = Loc.Get("PressStart");
		MenuEntry menuEntry = new MenuEntry(_pressStartText)
		{
			DoesDrawLargeShadow = true,
			DoesConfirmationPlaySound = true
		};
		menuEntry.Selected += PressStartEntrySelected;
		base.MenuEntries.Add(menuEntry);
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
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_pressStartText) * _zoom;
		base.MenuOffset = new Point(-(int)(vector.X / 2f), (int)vector.Y);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (Constants.InGameZoom != _zoom)
		{
			RefreshSizes();
		}
	}

	public override void HandleInput(InputState input)
	{
		_areControllersConnected = false;
		bool[] gamePadWasConnected = input.GamePadWasConnected;
		bool[] array = gamePadWasConnected;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i])
			{
				_areControllersConnected = true;
				break;
			}
		}
		if (input.IsNewButtonPress(Buttons.Start, null, out var playerIndex))
		{
			_personWhoPressed = playerIndex;
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
			AddMainMenu();
		}
		else
		{
			base.HandleInput(input);
		}
	}

	private void PressStartEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_personWhoPressed = e.PlayerIndex;
		AddMainMenu();
	}

	private void AddMainMenu()
	{
		GameConfigSave configSave = base.ScreenManager.SaveFileManager.ConfigSave;
		if (configSave == null || configSave.HasPickedLocale)
		{
			if (_doesSaveSaveFileExist.HasValue)
			{
				bool value = _doesSaveSaveFileExist.Value;
				base.ScreenManager.AddScreen(new MainMenuScreen(base.ScreenManager.SaveFileManager, value), _personWhoPressed);
			}
			else
			{
				if (DoesNeedToStartLoadCheck)
				{
					if (OperatingSystem.IsBrowser())
					{
						base.ScreenManager.SaveFileManager.PassiveCheckForGameSaveFile();
					}
					else
					{
						Task task = new Task(base.ScreenManager.SaveFileManager.PassiveCheckForGameSaveFile);
						task.Start();
					}
				}
				base.ScreenManager.AddScreen(new SaveValidationScreen(_personWhoPressed), _personWhoPressed);
			}
		}
		else
		{
			if (!_areControllersConnected)
			{
				configSave.IsKeyboardPreferred = true;
				Constants.IsKeyboardPreferred = true;
			}
			LocLoadingScreen screen = new LocLoadingScreen(configSave, base.ScreenManager.GCM, isFirstTimeShowing: true);
			base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
		}
		base.ScreenManager.RemoveScreen(this);
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("ExitGameConfirm"), base.ScreenManager.MenuControllerMapping);
		messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, playerIndex);
	}

	private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.Game.Exit();
	}
}
