using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.MainMenu;
using Timespinner.GameStateManagement.Screens.PauseMenu.Options;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class OptionsMenuScreen : InventoryMenuScreen
{
	private const float BackgroundDrawOffsetY = 7f / 64f;

	private readonly bool _wassKeyboardPreferred;

	private readonly int _startingButtonStyle;

	private readonly MenuEntry _buttonStyleMenuEntry;

	private readonly MenuEntry _keyboardPreferredMenuEntry;

	private readonly GameConfigSave _configSave;

	private readonly GameSave _saveFile;

	private readonly Action _finalFullExitAction;

	private Rectangle _backgroundDrawRectangle;

	public OptionsMenuScreen(GameSave saveFile, GameConfigSave configSave, GCM gcm, Action fullExitAction)
		: base(Loc.Get("OptionsMenuTitle"), saveFile, gcm, fullExitAction)
	{
		_saveFile = saveFile;
		_configSave = configSave;
		_finalFullExitAction = fullExitAction;
		_wassKeyboardPreferred = configSave.IsKeyboardPreferred;
		_startingButtonStyle = configSave.ButtonDisplayType;
		MenuEntry menuEntry = new MenuEntry(Loc.Get("ControlsMenuTitleDesktop"))
		{
			Description = Loc.Get("ControlsMenuDescriptionDesktop")
		};
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("MenuControlsMenuTitle"))
		{
			Description = Loc.Get("MenuControlsMenuDescription")
		};
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("AudioMenuTitle"))
		{
			Description = Loc.Get("AudioMenuDescription")
		};
		MenuEntry menuEntry4 = new MenuEntry(Loc.Get("VideoMenuTitle"))
		{
			Description = Loc.Get("VideoMenuDescription")
		};
		MenuEntry menuEntry5 = new MenuEntry(Loc.Get("CreditsMenuTitle"))
		{
			Description = Loc.Get("CreditsMenuDescription")
		};
		MenuEntry menuEntry6 = new MenuEntry(Loc.Get("PasswordMenuTitle"))
		{
			Description = Loc.Get("PasswordMenuDescription")
		};
		_buttonStyleMenuEntry = new MenuEntry(Loc.Get("ControlsButtonStyleTitle"))
		{
			Description = Loc.Get("ControlsButtonStyleDescription")
		};
		_keyboardPreferredMenuEntry = new MenuEntry(Loc.Get("ControlsKeyboardPreferredTitle"))
		{
			Description = Loc.Get("ControlsKeyboardPreferredDescription")
		};
		menuEntry.Selected += ControlsMenuEntrySelected;
		menuEntry2.Selected += MenuControlsMenuEntrySelected;
		menuEntry3.Selected += AudioMenuEntrySelected;
		menuEntry4.Selected += VideoMenuEntrySelected;
		menuEntry5.Selected += CredtisMenuEntrySelected;
		menuEntry6.Selected += PasswordMenuEntrySelected;
		_buttonStyleMenuEntry.Selected += ButtonStyleMenuEntrySelected;
		_keyboardPreferredMenuEntry.Selected += KeyboardPreferredEntrySelected;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(menuEntry3);
		base.MenuEntries.Add(menuEntry4);
		base.MenuEntries.Add(_buttonStyleMenuEntry);
		base.MenuEntries.Add(_keyboardPreferredMenuEntry);
		base.MenuEntries.Add(menuEntry5);
		if (_saveFile != null)
		{
			base.MenuEntries.Add(menuEntry6);
		}
	}

	public override void LoadContent()
	{
		base.LoadContent();
		base.DescriptionControllerMapping = ControllerMapping.OptionsMenuMapping;
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
	}

	private void ControlsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new ControlsMenuScreen(_configSave, _saveFile, base.GCM, FullExit), e.PlayerIndex);
	}

	private void MenuControlsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new MenuControlsMenuScreen(_configSave, _saveFile, base.GCM, FullExit), e.PlayerIndex);
	}

	private void AudioMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new AudioMenuScreen(_configSave, _saveFile, base.GCM, FullExit), e.PlayerIndex);
	}

	private void VideoMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new VideoMenuScreen(_saveFile, _configSave, base.GCM, FullExit), e.PlayerIndex);
	}

	private void CredtisMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		GameConfigSave configSave = base.ScreenManager.SaveFileManager.ConfigSave;
		base.ScreenManager.AddScreen(new CreditsScreen(isAtEndOfGame: false, configSave), e.PlayerIndex);
	}

	private void PasswordMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new PasswordMenuScreen(_saveFile, base.GCM, FullExit), e.PlayerIndex);
	}

	private void ButtonStyleMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		int num = Constants.ButtonDisplayType + 1;
		if (num == 2)
		{
			num++;
		}
		if (num > 4)
		{
			num = 0;
		}
		Constants.ButtonDisplayType = num;
		_configSave.ButtonDisplayType = Constants.ButtonDisplayType;
		ChangeDescription(_buttonStyleMenuEntry.Description, EInventoryItemIcon.None);
	}

	private void KeyboardPreferredEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		Constants.IsKeyboardPreferred = !Constants.IsKeyboardPreferred;
		_configSave.IsKeyboardPreferred = Constants.IsKeyboardPreferred;
		ChangeDescription(_keyboardPreferredMenuEntry.Description, EInventoryItemIcon.None);
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		}, spriteBatch: spriteBatch, backgroundRectangle: _backgroundDrawRectangle, color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 37, 38, 37, 39, 40, 39, 37, 38, 37 }, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
	}

	public override void HandleInput(InputState input)
	{
		bool flag = true;
		if (_finalFullExitAction == null && input.IsNewPressExit(base.ControllingPlayer))
		{
			flag = false;
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
		}
		if (flag)
		{
			base.HandleInput(input);
		}
	}

	private void FullExit()
	{
		ExitScreen();
		if (_finalFullExitAction != null)
		{
			_finalFullExitAction();
		}
	}

	public override void ExitScreen()
	{
		bool flag = false;
		if (_startingButtonStyle != _configSave.ButtonDisplayType || _wassKeyboardPreferred != _configSave.IsKeyboardPreferred)
		{
			flag = true;
		}
		if (flag)
		{
			base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		}
		base.ExitScreen();
	}
}
