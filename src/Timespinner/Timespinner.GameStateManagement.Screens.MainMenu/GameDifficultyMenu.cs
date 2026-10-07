using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal sealed class GameDifficultyMenu : MenuScreen
{
	private const int TitleOffsetY = -4;

	private const int DescriptionOffsetX = 132;

	private const int DescriptionBorderWidth = 256;

	private const int DescriptionBorderHeight = 32;

	private readonly bool _isHardModeAvailable;

	private readonly bool _isLevelCap255Available;

	private readonly MenuEntry _hardMenuEntry;

	private readonly SaveFileManager _saveFileManager;

	private readonly Action<GameSave.EGameDifficultyType> _onDifficultyChosen;

	private Rectangle _descriptionBackgroundDrawRectangle;

	public GameDifficultyMenu(SaveFileManager saveFileManager, Action<GameSave.EGameDifficultyType> onDifficultyChosen)
		: base(Loc.Get("DifficultyMenuTitle"))
	{
		_doesUseBlackGradientBox = false;
		_saveFileManager = saveFileManager;
		_onDifficultyChosen = onDifficultyChosen;
		base.DoesTitleDrawLargeShadow = true;
		base.IsDescriptionCentered = true;
		if (_saveFileManager.ConfigSave != null)
		{
			GameConfigSave configSave = _saveFileManager.ConfigSave;
			_isHardModeAvailable = configSave.HasGameBeenCleared;
			_isLevelCap255Available = configSave.HasLevelCap1BeenCleared;
			bool flag = false;
			if (!_isHardModeAvailable && _saveFileManager.AvailableSaves != null)
			{
				foreach (GameSave availableSafe in _saveFileManager.AvailableSaves)
				{
					if (availableSafe.IsGameCleared)
					{
						_isHardModeAvailable = true;
						break;
					}
				}
				if (_isHardModeAvailable)
				{
					configSave.HasGameBeenCleared = true;
					flag = true;
				}
			}
			if (!_isLevelCap255Available && _saveFileManager.AvailableSaves != null)
			{
				foreach (GameSave availableSafe2 in _saveFileManager.AvailableSaves)
				{
					if (availableSafe2.GetSaveBool("IsNightmareNightmare") || availableSafe2.IsLevelCap255)
					{
						_isLevelCap255Available = true;
						break;
					}
				}
				if (_isLevelCap255Available)
				{
					configSave.HasLevelCap1BeenCleared = true;
					flag = true;
				}
			}
			if (flag)
			{
				saveFileManager.RequestGameConfigSave();
			}
		}
		base.IsOverlayScreen = true;
		base.IsPopupScreen = false;
		string description = Loc.Get("DifficultyMenuEasyDescription_Steam");
		MenuEntry menuEntry = new MenuEntry(Loc.Get("DifficultyMenuEasy"))
		{
			Description = description
		};
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("DifficultyMenuNormal"))
		{
			Description = Loc.Get("DifficultyMenuNormalDescription")
		};
		_hardMenuEntry = new MenuEntry(Loc.Get("DifficultyMenuHard"))
		{
			Description = Loc.Get("DifficultyMenuHardDescription")
		};
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("DifficultyMenuHardCap1"))
		{
			Description = Loc.Get("DifficultyMenuHardLevelCap1Description")
		};
		MenuEntry menuEntry4 = new MenuEntry(Loc.Get("DifficultyMenuHardCap255"))
		{
			Description = Loc.Get("DifficultyMenuHardLevelCap255Description")
		};
		menuEntry.Selected += OnEasyEntrySelected;
		menuEntry2.Selected += OnNormalEntrySelected;
		_hardMenuEntry.Selected += OnHardEntrySelected;
		menuEntry3.Selected += OnHardCap1EntrySelected;
		menuEntry4.Selected += OnHardCap255EntrySelected;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(_hardMenuEntry);
		if (_isHardModeAvailable)
		{
			base.MenuEntries.Add(menuEntry3);
		}
		if (_isLevelCap255Available)
		{
			base.MenuEntries.Add(menuEntry4);
		}
		_hardMenuEntry.BaseDrawColor = (_isHardModeAvailable ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
		_primaryMenuCollection.SetIsCenterAligned(isCenterAligned: true);
		_primaryMenuCollection.SetDoesDrawLargeShadow(doesDrawLargeShadow: true);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		_primaryMenuCollection.SelectedIndex = 1;
		OnSelectedEntryChanged(1);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int inGameZoom = Constants.InGameZoom;
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString("N") * inGameZoom;
		base.MenuOffset = new Point(0, (int)(vector.Y * 2f));
		base.TitleOffset = new Vector2(0f, (int)((float)base.MenuOffset.Y - vector.Y * 3f + (float)(-4 * inGameZoom)));
		Rectangle titleSafeArea = base.ScreenManager.TitleSafeArea;
		int num = 132 * inGameZoom;
		base.DescriptionDrawPosition = new Vector2(titleSafeArea.Center.X - num, (float)titleSafeArea.Bottom - vector.Y * 2f);
		_descriptionBackgroundDrawRectangle = new Rectangle(titleSafeArea.Center.X - 256 * inGameZoom, (int)base.DescriptionDrawPosition.Y, 256 * inGameZoom, 32 * inGameZoom);
	}

	private void OnEasyEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		Console.WriteLine("[GameDifficultyMenu] OnEasyEntrySelected triggered!");
		_onDifficultyChosen(GameSave.EGameDifficultyType.Easy);
		ExitScreen();
	}

	private void OnNormalEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		Console.WriteLine("[GameDifficultyMenu] OnNormalEntrySelected triggered!");
		_onDifficultyChosen(GameSave.EGameDifficultyType.Normal);
		ExitScreen();
	}

	private void OnHardEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_hardMenuEntry.DoesConfirmationPlaySound = _isHardModeAvailable;
		if (_isHardModeAvailable)
		{
			_onDifficultyChosen(GameSave.EGameDifficultyType.Hard);
			ExitScreen();
		}
		else
		{
			PlayErrorSound();
			ChangeDescription(Loc.Get("DifficultyMenuHardNotUnlocked"), EInventoryItemIcon.None);
		}
	}

	private void OnHardCap1EntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_onDifficultyChosen(GameSave.EGameDifficultyType.HardCap1);
		ExitScreen();
	}

	private void OnHardCap255EntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_onDifficultyChosen(GameSave.EGameDifficultyType.HardCap255);
	}

	protected override void DrawDescription(SpriteBatch spriteBatch)
	{
		Color color = Color.White * (1f - base.TransitionOffPercentage);
		Rectangle descriptionBackgroundDrawRectangle = _descriptionBackgroundDrawRectangle;
		spriteBatch.Draw(base.ScreenManager.UIBlackGradientBox, descriptionBackgroundDrawRectangle, null, color, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
		descriptionBackgroundDrawRectangle.Location = descriptionBackgroundDrawRectangle.Location.Add(descriptionBackgroundDrawRectangle.Width, 0);
		spriteBatch.Draw(base.ScreenManager.UIBlackGradientBox, descriptionBackgroundDrawRectangle, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		base.DrawDescription(spriteBatch);
	}
}
