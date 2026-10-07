using System;
using System.Collections.Generic;
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

internal class MenuControlsMenuScreen : InventoryMenuScreen
{
	private enum EMappingSafetyResult
	{
		Safe,
		MatchingConfirmCancelDown,
		MatchingConfirmExitDown,
		MatchingConfirmCancelSecondary
	}

	private const float PrimarySectionHeightRatio = 23f / 48f;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const float ScrollbarPositionRatioY = 0.5f;

	private const float ScrollbarHeightRatio = 35f / 96f;

	private readonly ControlsMenuEntryCollection _activeButtonMappings;

	private readonly ControllerMapping _originalMenuControllerMapping;

	private readonly ControlsMenuEntryCollection _menuButtonMappings;

	private readonly GameConfigSave _saveFile;

	private readonly Queue<ControlsMenuEntry> _buttonSetQueue = new Queue<ControlsMenuEntry>();

	private readonly Action _finalOnFullExitAction;

	private bool _isWaitingForInput;

	private bool _didCancelButtonSetting;

	private bool _isDirty;

	private bool _isDoingSetAllButtons;

	private bool _isTryingFullExit;

	private int _setAllButtonsIndex;

	private int _primarySectionHeight;

	private Rectangle _backgroundDrawRectangle;

	private MenuDescription _lastDescription;

	private ControlsMenuEntry _buttonToChange;

	private ControllerMapping _draftMenuControllerMapping;

	public MenuControlsMenuScreen(GameConfigSave configFile, GameSave saveFile, GCM gcm, Action fullExitAction)
		: base(Loc.Get("MenuControlsMenuTitle"), saveFile, gcm, null)
	{
		_saveFile = configFile;
		_originalMenuControllerMapping = configFile.MenuControllerMapping;
		_finalOnFullExitAction = fullExitAction;
		_draftMenuControllerMapping = _originalMenuControllerMapping.Duplicate();
		_draftMenuControllerMapping.IsMenuMapping = true;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
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
		menuEntry.Selected += OnSetInviduallyMenuEntrySelected;
		menuEntry2.Selected += OnSetAllMenuEntrySelected;
		menuEntry3.Selected += OnDefaultMenuEntrySelected;
		menuEntry4.Selected += OnRevertMenuEntrySelected;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(menuEntry3);
		base.MenuEntries.Add(menuEntry4);
		_menuButtonMappings = new ControlsMenuEntryCollection(_draftMenuControllerMapping, base.GCM.SpUIButtons, OnButtonSelected, EControllerMappingType.Menu);
		_subMenuCollections.Add(_menuButtonMappings);
		_activeButtonMappings = _menuButtonMappings;
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
		_primaryMenuCollection.ColumnCount = 2;
		_primaryMenuCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		int y = (int)(-1.5f * (float)base.Zoom * (float)base.Font.LineSpacing);
		_menuButtonMappings.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_menuButtonMappings.DrawPosition = base.ListTextDrawPosition.Add(new Point(0, y));
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(0.5f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(35f / 96f * (float)_topSectionHeight);
	}

	public override void HandleInput(InputState input)
	{
		if (_isWaitingForInput && _buttonToChange != null)
		{
			if (GetNewButtonPress(input))
			{
				MakeDirty();
				EndWaitingForInput();
			}
		}
		else if (input.IsNewPressExit(base.ControllingPlayer))
		{
			if (_isDirty)
			{
				TryExit(base.ControllingPlayer, isFullExit: true);
				return;
			}
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
			_finalOnFullExitAction();
		}
		else
		{
			base.HandleInput(input);
		}
	}

	private static EMappingSafetyResult CheckIfMappingIsSafe(ControllerMapping mapping)
	{
		EMappingSafetyResult result = EMappingSafetyResult.Safe;
		int[] keys = new int[3] { 32, 33, 12 };
		int[] keys2 = new int[3] { 32, 38, 12 };
		int[] keys3 = new int[3] { 32, 33, 34 };
		if (CheckIfSharingSources(keys, mapping))
		{
			result = EMappingSafetyResult.MatchingConfirmCancelDown;
		}
		else if (CheckIfSharingSources(keys2, mapping))
		{
			result = EMappingSafetyResult.MatchingConfirmExitDown;
		}
		else if (CheckIfSharingSources(keys3, mapping))
		{
			result = EMappingSafetyResult.MatchingConfirmCancelSecondary;
		}
		return result;
	}

	private static bool CheckIfSharingSources(int[] keys, ControllerMapping mapping)
	{
		bool result = false;
		Dictionary<int, ButtonMapping> mappings = mapping.Mappings;
		foreach (int num in keys)
		{
			if (!mappings.ContainsKey(num))
			{
				continue;
			}
			ButtonMapping buttonMapping = mappings[num];
			foreach (int num2 in keys)
			{
				if (num == num2 || !mappings.ContainsKey(num2))
				{
					continue;
				}
				ButtonMapping buttonMapping2 = mappings[num2];
				foreach (ButtonMappingSource source in buttonMapping.Sources)
				{
					foreach (ButtonMappingSource source2 in buttonMapping2.Sources)
					{
						if (ButtonMapping.IsEqual(source, source2))
						{
							result = true;
							break;
						}
					}
				}
			}
		}
		return result;
	}

	private void TryExit(PlayerIndex? playerIndex, bool isFullExit)
	{
		_isTryingFullExit = isFullExit;
		EMappingSafetyResult eMappingSafetyResult = CheckIfMappingIsSafe(_draftMenuControllerMapping);
		if (eMappingSafetyResult == EMappingSafetyResult.Safe)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("ControlsChangeConfirm"), base.ScreenManager.MenuControllerMapping);
			messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
			messageBoxScreen.Cancelled += CancelExitMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, playerIndex);
		}
		else
		{
			EMappingSafetyResult eMappingSafetyResult2 = eMappingSafetyResult;
			string message = ((eMappingSafetyResult2 != EMappingSafetyResult.MatchingConfirmExitDown) ? Loc.Get("ControlsInvalidCancelRevertConfirm") : Loc.Get("ControlsInvalidExitRevertConfirm"));
			MessageBoxScreen messageBoxScreen2 = new MessageBoxScreen(message, base.ScreenManager.MenuControllerMapping);
			messageBoxScreen2.Accepted += CancelExitMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen2, playerIndex);
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuError);
		}
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		if (_isDirty && _selectedMenuCollection == _primaryMenuCollection)
		{
			TryExit(playerIndex, isFullExit: false);
		}
		else
		{
			base.OnCancel(playerIndex);
		}
	}

	private void CancelExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		ExitScreen();
		if (_isTryingFullExit)
		{
			_finalOnFullExitAction();
		}
	}

	private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		_saveFile.MenuControllerMapping = _draftMenuControllerMapping;
		base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		base.ScreenManager.RefreshMenuControllerMapping();
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
		ExitScreen();
		if (_isTryingFullExit)
		{
			_finalOnFullExitAction();
		}
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
		_draftMenuControllerMapping = ControllerMapping.DefaultMenuMapping;
		_menuButtonMappings.ChangeMapping(_draftMenuControllerMapping);
		MakeDirty();
	}

	private void OnRevertMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_draftMenuControllerMapping = _originalMenuControllerMapping.Duplicate();
		_menuButtonMappings.ChangeMapping(_draftMenuControllerMapping);
		_isDirty = false;
	}

	private void RefreshVisibleControls()
	{
		_menuButtonMappings.IsActive = true;
		_topLowerFrameIndices = new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 };
	}

	private void MakeDirty()
	{
		_isDirty = true;
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
}
