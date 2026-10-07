using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class ControlsMenuScreen : InventoryMenuScreen
{
	private const float PrimarySectionHeightRatio = 23f / 48f;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const int HeroNameBorderDrawOffsetX = 20;

	private const int NameBorderEdgeOffsetX = 20;

	private const int NameTextDrawOffsetX = 20;

	private const float HeroFamiliarNameBorderDrawPositionRatioY = 0.74583334f;

	private const float HeroFamiliarNameTextDrawPositionRatioY = 0.74583334f;

	private const float ScrollbarPositionRatioY = 0.5f;

	private const float ScrollbarHeightRatio = 35f / 96f;

	private static readonly Color ShadowColor = new Color(32, 24, 24);

	private readonly string _heroName;

	private readonly string _familiarName;

	private readonly MenuEntry _togglePlayerMenuEntry;

	private readonly MenuEntry _controllerRumbleMenuEntry;

	private readonly ControllerMapping _originalHeroControllerMapping;

	private readonly ControllerMapping _originalFamiliarControllerMapping;

	private readonly ControlsMenuEntryCollection _heroButtonMappings;

	private readonly ControlsMenuEntryCollection _familiarButtonMappings;

	private readonly GameConfigSave _saveFile;

	private readonly Queue<ControlsMenuEntry> _buttonSetQueue = new Queue<ControlsMenuEntry>();

	private readonly Action _finalOnFullExitAction;

	private bool _isWaitingForInput;

	private bool _didCancelButtonSetting;

	private bool _isHeroDirty;

	private bool _isFamiliarDirty;

	private bool _isPrimaryPlayerButtonMappingActive;

	private bool _isDoingSetAllButtons;

	private int _setAllButtonsIndex;

	private int _primarySectionHeight;

	private int _heroNameBorderDrawPositionX;

	private int _familiarNameBorderDrawPositionX;

	private int _heroFamiliarNameDrawPositionY;

	private int _heroNameBorderDrawWidth;

	private int _familiarNameBorderDrawWidth;

	private Vector2 _heroNameDrawPosition;

	private Vector2 _familiarNameDrawPosition;

	private Rectangle _backgroundDrawRectangle;

	private MenuDescription _lastDescription;

	private ControlsMenuEntry _buttonToChange;

	private ControllerMapping _draftHeroControllerMapping;

	private ControllerMapping _draftFamiliarControllerMapping;

	private ControlsMenuEntryCollection _activeButtonMappings;

	public ControlsMenuScreen(GameConfigSave configFile, GameSave saveFile, GCM gcm, Action fullExitAction)
		: base(Loc.Get("ControlsMenuTitleDesktop"), saveFile, gcm, null)
	{
		_saveFile = configFile;
		_originalHeroControllerMapping = configFile.PlayerControllerMapping;
		_originalFamiliarControllerMapping = configFile.FamiliarControllerMapping;
		_finalOnFullExitAction = fullExitAction;
		_draftHeroControllerMapping = _originalHeroControllerMapping.Duplicate();
		_draftFamiliarControllerMapping = _originalFamiliarControllerMapping.Duplicate();
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		_heroName = Loc.Get("ControlsMainHero");
		_familiarName = Loc.Get("ControlsSubHero");
		MenuEntry menuEntry = new MenuEntry(Loc.Get("ControlsSetIndividual"))
		{
			Description = Loc.Get("ControlsSetIndividualDescription")
		};
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("ControlsSetAll"))
		{
			Description = Loc.Get("ControlsSetAllDescription")
		};
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("ControlsDefault"))
		{
			Description = Loc.Get("ControlsDefaultDescription")
		};
		MenuEntry menuEntry4 = new MenuEntry(Loc.Get("ControlsRevert"))
		{
			Description = Loc.Get("ControlsRevertDescription")
		};
		_togglePlayerMenuEntry = new MenuEntry(Loc.Get("ControlsTogglePlayer"))
		{
			Description = Loc.Get("ControlsTogglePlayerDescription")
		};
		_controllerRumbleMenuEntry = new MenuEntry("")
		{
			Description = Loc.Get("ControlsRumbleDescription")
		};
		RefreshControllerRumbleMenuEntryName();
		menuEntry.Selected += OnSetInviduallyMenuEntrySelected;
		menuEntry2.Selected += OnSetAllMenuEntrySelected;
		menuEntry3.Selected += OnDefaultMenuEntrySelected;
		menuEntry4.Selected += OnRevertMenuEntrySelected;
		_togglePlayerMenuEntry.Selected += OnTogglePlayerEntrySelected;
		_controllerRumbleMenuEntry.Selected += OnControllerRumbleMenuEntrySelected;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(menuEntry3);
		base.MenuEntries.Add(menuEntry4);
		base.MenuEntries.Add(_controllerRumbleMenuEntry);
		base.MenuEntries.Add(_togglePlayerMenuEntry);
		_heroButtonMappings = new ControlsMenuEntryCollection(_draftHeroControllerMapping, base.GCM.SpUIButtons, OnButtonSelected, EControllerMappingType.Hero);
		_subMenuCollections.Add(_heroButtonMappings);
		_familiarButtonMappings = new ControlsMenuEntryCollection(_draftFamiliarControllerMapping, base.GCM.SpUIButtons, OnButtonSelected, EControllerMappingType.Familiar);
		_subMenuCollections.Add(_familiarButtonMappings);
		_isPrimaryPlayerButtonMappingActive = true;
		_activeButtonMappings = _heroButtonMappings;
		RefreshVisibleControls();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		_primarySectionHeight = (int)(23f / 48f * (float)_topSectionHeight);
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _primarySectionHeight - num - 8 * base.Zoom);
		_listBorderDrawPosition = new Vector2(_screenLeft, _backgroundDrawRectangle.Bottom - _screenTop);
		_listBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _backgroundDrawRectangle.Bottom + 8 * base.Zoom, _screenWidth - 4 * base.Zoom, _screenTop + _topSectionHeight - _backgroundDrawRectangle.Bottom - 16 * base.Zoom);
		int num2 = (int)base.Font.MeasureString(_heroName).X;
		int num3 = (int)base.Font.MeasureString(_familiarName).X;
		_heroNameBorderDrawWidth = (20 + num2 + 20) * base.Zoom;
		_familiarNameBorderDrawWidth = (20 + num3 + 20) * base.Zoom;
		_heroNameBorderDrawPositionX = _screenLeft + 20 * base.Zoom;
		_familiarNameBorderDrawPositionX = _screenLeft + _screenWidth - _familiarNameBorderDrawWidth - 20 * base.Zoom;
		_heroFamiliarNameDrawPositionY = _screenTop + (int)(0.74583334f * (float)(_topSectionHeight + _bottomSectionHeight));
		_heroNameDrawPosition = new Vector2(_heroNameBorderDrawPositionX + 20 * base.Zoom, (float)_screenTop + 0.74583334f * (float)(_topSectionHeight + _bottomSectionHeight));
		_familiarNameDrawPosition = new Vector2(_familiarNameBorderDrawPositionX + 20 * base.Zoom, (float)_screenTop + 0.74583334f * (float)(_topSectionHeight + _bottomSectionHeight));
		_primaryMenuCollection.ColumnCount = 2;
		_primaryMenuCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		int y = (int)(-1.5f * (float)base.Zoom * (float)base.Font.LineSpacing);
		_heroButtonMappings.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_heroButtonMappings.DrawPosition = base.ListTextDrawPosition.Add(new Point(0, y));
		_familiarButtonMappings.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_familiarButtonMappings.DrawPosition = _heroButtonMappings.DrawPosition;
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(0.5f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(35f / 96f * (float)_topSectionHeight);
	}

	public override void HandleInput(InputState input)
	{
		if (_isWaitingForInput && _buttonToChange != null)
		{
			if (input.CurrentKeyboardStates.Any() && input.CurrentKeyboardStates[0].IsKeyDown(Keys.Escape))
			{
				_didCancelButtonSetting = true;
				EndWaitingForInput();
			}
			else if (GetNewButtonPress(input))
			{
				MakeDirty();
				EndWaitingForInput();
			}
		}
		else if (input.IsNewPressExit(base.ControllingPlayer))
		{
			if (IsDirty())
			{
				MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("ControlsChangeConfirm"), base.ScreenManager.MenuControllerMapping);
				messageBoxScreen.Accepted += ConfirmFullExitMessageBoxAccepted;
				messageBoxScreen.Cancelled += CancelExitMessageBoxAccepted;
				base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
			}
			else
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
				ExitScreen();
				_finalOnFullExitAction();
			}
		}
		else
		{
			base.HandleInput(input);
		}
	}

	private bool IsDirty()
	{
		if (!_isHeroDirty && !_isFamiliarDirty)
		{
			return _draftHeroControllerMapping.DoesUseControllerRumble != _originalHeroControllerMapping.DoesUseControllerRumble;
		}
		return true;
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		if (IsDirty() && _selectedMenuCollection == _primaryMenuCollection)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("ControlsChangeConfirm"), base.ScreenManager.MenuControllerMapping);
			messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
			messageBoxScreen.Cancelled += CancelExitMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, playerIndex);
		}
		else
		{
			base.OnCancel(playerIndex);
		}
	}

	private void CancelExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		base.OnCancel(e.PlayerIndex);
	}

	private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		_saveFile.PlayerControllerMapping = _draftHeroControllerMapping;
		_saveFile.FamiliarControllerMapping = _draftFamiliarControllerMapping;
		base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
		ExitScreen();
	}

	private void ConfirmFullExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		_saveFile.PlayerControllerMapping = _draftHeroControllerMapping;
		_saveFile.FamiliarControllerMapping = _draftFamiliarControllerMapping;
		base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
		ExitScreen();
		_finalOnFullExitAction();
	}

	private bool GetNewButtonPress(InputState input)
	{
		bool flag = false;
		PlayerIndex playerIndex;
		foreach (Buttons value in Enum.GetValues(typeof(Buttons)))
		{
			if (input.IsNewButtonPress(value, null, out playerIndex))
			{
				flag = true;
				_buttonToChange.Button.SetNewButton(value);
				break;
			}
		}
		if (!flag)
		{
			foreach (Keys value2 in Enum.GetValues(typeof(Keys)))
			{
				if (input.IsNewKeyPress(value2, null, out playerIndex))
				{
					flag = true;
					_buttonToChange.Button.SetNewButton(value2);
					break;
				}
			}
		}
		return flag;
	}

	private void EndWaitingForInput()
	{
		_buttonToChange.IsAwaitingInput = false;
		_isWaitingForInput = false;
		_activeButtonMappings.IsAnEntryBeingHighlighted = false;
		base.CurrentDescription = _lastDescription;
		if (_isDoingSetAllButtons)
		{
			_setAllButtonsIndex++;
			_activeButtonMappings.SetSelectedIndex(_setAllButtonsIndex);
			_activeButtonMappings.RefreshScrollWindow();
		}
		if (!_didCancelButtonSetting && _buttonSetQueue.Count > 0)
		{
			OnButtonSelected(_buttonSetQueue.Dequeue());
		}
	}

	private void OnSetInviduallyMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_isDoingSetAllButtons = false;
		if (_activeButtonMappings.Entries.Count > 0)
		{
			ChangeMenuCollection(_activeButtonMappings, shouldPush: true);
		}
	}

	private void OnSetAllMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_setAllButtonsIndex = 0;
		_isDoingSetAllButtons = true;
		_didCancelButtonSetting = false;
		_buttonSetQueue.Clear();
		_activeButtonMappings.SetSelectedIndex(0);
		_activeButtonMappings.RefreshScrollWindow();
		foreach (ControlsMenuEntry buttonEntry in _activeButtonMappings.ButtonEntries)
		{
			_buttonSetQueue.Enqueue(buttonEntry);
		}
		if (_buttonSetQueue.Count > 0)
		{
			OnButtonSelected(_buttonSetQueue.Dequeue());
		}
	}

	private void OnDefaultMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_isPrimaryPlayerButtonMappingActive)
		{
			_draftHeroControllerMapping = ControllerMapping.DefaultMapping;
			_heroButtonMappings.ChangeMapping(_draftHeroControllerMapping);
		}
		else
		{
			_draftFamiliarControllerMapping = ControllerMapping.DefaultFamiliarMapping;
			_familiarButtonMappings.ChangeMapping(_draftFamiliarControllerMapping);
		}
		MakeDirty();
	}

	private void OnRevertMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_isPrimaryPlayerButtonMappingActive)
		{
			_draftHeroControllerMapping = _originalHeroControllerMapping.Duplicate();
			_heroButtonMappings.ChangeMapping(_draftHeroControllerMapping);
		}
		else
		{
			_draftFamiliarControllerMapping = _originalFamiliarControllerMapping.Duplicate();
			_familiarButtonMappings.ChangeMapping(_draftFamiliarControllerMapping);
		}
		if (_isPrimaryPlayerButtonMappingActive)
		{
			_isHeroDirty = false;
		}
		else
		{
			_isFamiliarDirty = false;
		}
	}

	private void OnTogglePlayerEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_isPrimaryPlayerButtonMappingActive = !_isPrimaryPlayerButtonMappingActive;
		_activeButtonMappings = (_isPrimaryPlayerButtonMappingActive ? _heroButtonMappings : _familiarButtonMappings);
		RefreshVisibleControls();
	}

	private void RefreshVisibleControls()
	{
		_heroButtonMappings.IsActive = _isPrimaryPlayerButtonMappingActive;
		_familiarButtonMappings.IsActive = !_isPrimaryPlayerButtonMappingActive;
		_topLowerFrameIndices = ((!_isPrimaryPlayerButtonMappingActive) ? new int[9] { 45, 46, 45, 47, 48, 47, 45, 46, 45 } : new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 });
	}

	private void OnControllerRumbleMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_draftHeroControllerMapping.DoesUseControllerRumble = !_draftHeroControllerMapping.DoesUseControllerRumble;
		RefreshControllerRumbleMenuEntryName();
	}

	private void MakeDirty()
	{
		if (_isPrimaryPlayerButtonMappingActive)
		{
			_isHeroDirty = true;
		}
		else
		{
			_isFamiliarDirty = true;
		}
	}

	private void RefreshControllerRumbleMenuEntryName()
	{
		_controllerRumbleMenuEntry.SetText(Loc.Get(_draftHeroControllerMapping.DoesUseControllerRumble ? "ControlsRumbleOn" : "ControlsRumbleOff"));
	}

	private void OnButtonSelected(ControlsMenuEntry button)
	{
		_lastDescription = base.CurrentDescription;
		base.CurrentDescription = new MenuDescription(string.Format(Loc.Get("ControlsPressToChangeButtonDescription"), button.Text), base.Font, EInventoryItemIcon.None, null, null, base.IsDescriptionCentered, base.ScreenManager.MenuControllerMapping);
		_buttonToChange = button;
		_isWaitingForInput = true;
		button.IsAwaitingInput = true;
		_activeButtonMappings.IsAnEntryBeingHighlighted = true;
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteEffects[] array = new SpriteEffects[9]
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
		};
		DrawingEx.DrawIrregularBox(spriteBatch, _backgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 37, 38, 37, 39, 40, 39, 37, 38, 37 }, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
		array[2] = SpriteEffects.FlipHorizontally;
		if (_isPrimaryPlayerButtonMappingActive)
		{
			DrawingEx.DrawShortBox(spriteBatch, new Rectangle(_heroNameBorderDrawPositionX, _heroFamiliarNameDrawPositionY, _heroNameBorderDrawWidth, 16), drawColor, base.Sprite, base.Zoom, new int[3] { 54, 55, 54 }, array, shouldTile: true);
			DrawShadowedString(spriteBatch, base.Font, _heroName, _heroNameDrawPosition, drawColor, base.Zoom);
		}
		else
		{
			DrawingEx.DrawShortBox(spriteBatch, new Rectangle(_familiarNameBorderDrawPositionX, _heroFamiliarNameDrawPositionY, _familiarNameBorderDrawWidth, 16), drawColor, base.Sprite, base.Zoom, new int[3] { 54, 55, 54 }, array, shouldTile: true);
			DrawShadowedString(spriteBatch, base.Font, _familiarName, _familiarNameDrawPosition, drawColor, base.Zoom);
		}
	}

	private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPosition, Color drawColor, int zoom)
	{
		DrawingEx.DrawString(spriteBatch, font, text, drawPosition.Add(new Point(0, zoom)), ShadowColor, Vector2.Zero, zoom);
		DrawingEx.DrawString(spriteBatch, font, text, drawPosition, drawColor, Vector2.Zero, zoom);
	}
}
