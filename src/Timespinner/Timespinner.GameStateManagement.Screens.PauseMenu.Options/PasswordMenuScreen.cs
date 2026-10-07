using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.PauseMenu.Options.Passwords;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Options;

internal class PasswordMenuScreen : InventoryMenuScreen
{
	private const int MaxPasswordLength = 12;

	private const int ColumnCount = 10;

	private const int ScrollRowHeight = 4;

	private const int PasswordTextDisplayOffsetX = 6;

	private const int PasswordTextDisplayOffsetY = 16;

	private const int UnderscoreOffsetX = 3;

	private const int UnderscoreOffsetY = 4;

	private const int PasswordBackgroundWidth = 160;

	private const int PasswordBackgroundHeight = 16;

	private const int PasswordBackgroundOffsetY = 17;

	private const int CharactersColumnWidth = 24;

	private const int CharactersRowHeightOffset = 12;

	private const int CharactersMenuOffsetY = 32;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const string SpeedrunPasswordA = "SPEEDRUNALPH";

	private const string SpeedrunPasswordB = "SPEEDRUNBETA";

	private static readonly char[] PasswordCharacters = new char[36]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
		'U', 'V', 'W', 'X', 'Y', 'Z'
	};

	private static readonly Color LettersDrawColor = new Color(240, 224, 160);

	private static readonly Color NumbersDrawColor = new Color(160, 224, 240);

	private static readonly Color CurrentEnteredPasswordShadowColor = new Color(64, 32, 16);

	private readonly string _underscoreDisplay = "";

	private readonly string[] _displayCharacters = new string[12];

	private readonly Vector2[] _displayCharacterOrigins = new Vector2[12];

	private int _sizeOfUnderscoreSpace;

	private Vector2 _currentEnterPasswordDrawPosition;

	private Vector2 _underscoreDisplayDrawPosition;

	private Rectangle _backgroundDrawRectangle;

	private Rectangle _passwordBackgroundDrawRectangle;

	private string _currentEnteredPassword = "";

	private PasswordsMeyefSkin _meyefPasswords;

	private PasswordsCrowSkin _crowPasswords;

	private PasswordsUmbraOrb _umbraPasswords;

	private PasswordsNightmareMode _nightmarePasswords;

	public PasswordMenuScreen(GameSave inSave, GCM gcm, Action fullExitAction)
		: base(Loc.Get("PasswordMenuTitle"), inSave, gcm, fullExitAction)
	{
		_primaryMenuCollection.ColumnCount = 10;
		_primaryMenuCollection.ScrollRowHeight = 4;
		_primaryMenuCollection.IsVisible = true;
		int num = 0;
		char[] passwordCharacters = PasswordCharacters;
		foreach (char c in passwordCharacters)
		{
			MenuEntry menuEntry = new MenuEntry(c.ToString(CultureInfo.InvariantCulture));
			menuEntry.Selected += OnCharacterSelected;
			menuEntry.BaseDrawColor = ((num < 10) ? NumbersDrawColor : LettersDrawColor);
			base.MenuEntries.Add(menuEntry);
			num++;
		}
		MenuEntry menuEntry2 = new MenuEntry("DEL");
		menuEntry2.Selected += delegate
		{
			DeleteLetterFromPassword();
		};
		menuEntry2.DoesConfirmationPlaySound = false;
		base.MenuEntries.Add(menuEntry2);
		MenuEntry menuEntry3 = new MenuEntry("OK");
		menuEntry3.Selected += OnOkayEntrySelected;
		base.MenuEntries.Add(menuEntry3);
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		RefreshDisplayPassword();
		for (int j = 0; j < 12; j++)
		{
			_underscoreDisplay += "_ ";
		}
		_primaryMenuCollection.SetIsCenterAligned(isCenterAligned: true);
		_primaryMenuCollection.SetDoesDrawLargeShadow(doesDrawLargeShadow: true);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
		_sizeOfUnderscoreSpace = (int)base.Font.MeasureString("_ ").X;
		int num2 = _sizeOfUnderscoreSpace * 12;
		int num3 = -(num2 / 2);
		int num4 = _screenLeft + _screenWidth / 2;
		_currentEnterPasswordDrawPosition = new Vector2(num4 + num3 * base.Zoom, _backgroundDrawRectangle.Top + 16 * base.Zoom);
		_underscoreDisplayDrawPosition = _currentEnterPasswordDrawPosition.Add(3 * base.Zoom, 4 * base.Zoom);
		int num5 = 160 * base.Zoom;
		_passwordBackgroundDrawRectangle = new Rectangle(_screenLeft + _screenWidth / 2 - num5 / 2, _backgroundDrawRectangle.Top + 17 * base.Zoom, 160 * base.Zoom, 16 * base.Zoom);
		int num6 = 24 * base.Zoom;
		_primaryMenuCollection.SetColumnWidth(num6, base.Zoom);
		_primaryMenuCollection.EntryHeightOffset = 12;
		int num7 = _screenLeft + _screenWidth / 2;
		int num8 = num6 * 9;
		int num9 = num7 - num8 / 2;
		_primaryMenuCollection.DrawPosition = new Vector2(num9, _primaryMenuCollection.DrawPosition.Y + (float)(32 * base.Zoom));
	}

	public override void HandleInput(InputState input)
	{
		base.HandleInput(input);
		if (input.IsNewPressSecondary(base.ControllingPlayer))
		{
			DeleteLetterFromPassword();
		}
		if (input.IsNewKeyPress(Keys.Delete, base.ControllingPlayer, out var _))
		{
			DeleteLetterFromPassword();
		}
	}

	private void OnCharacterSelected(object sender, PlayerIndexEventArgs e)
	{
		int selectedIndex = base.SelectedIndex;
		char character = PasswordCharacters[selectedIndex];
		AddLetterToPassword(character);
	}

	private void OnOkayEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		TryUnlockCode();
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
		DrawingEx.DrawIrregularBox(spriteBatch, _backgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		int[] frames = new int[3] { 143, 144, 143 };
		DrawingEx.DrawShortBox(spriteBatch, _passwordBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, frames, array, shouldTile: true);
		array[1] = SpriteEffects.FlipHorizontally;
		base.DrawFrames(spriteBatch, drawColor);
	}

	public override void DrawMisc(SpriteBatch spriteBatch, Color drawColor)
	{
		DrawingEx.DrawString(spriteBatch, base.Font, _underscoreDisplay, _underscoreDisplayDrawPosition, LettersDrawColor, Vector2.Zero, base.Zoom);
		Vector2 vector = new Vector2(_currentEnterPasswordDrawPosition.X + (float)(6 * base.Zoom), _currentEnterPasswordDrawPosition.Y);
		for (int i = 0; i < 12; i++)
		{
			string text = _displayCharacters[i];
			if (text == null)
			{
				break;
			}
			bool flag = false;
			for (int j = 0; j < 10; j++)
			{
				if (j.ToString() == text)
				{
					flag = true;
				}
			}
			Color color = (flag ? NumbersDrawColor : LettersDrawColor);
			Vector2 origin = _displayCharacterOrigins[i];
			DrawingEx.DrawString(spriteBatch, base.Font, text, vector.Add(0f, base.Zoom), CurrentEnteredPasswordShadowColor, origin, base.Zoom);
			DrawingEx.DrawString(spriteBatch, base.Font, text, vector, color, origin, base.Zoom);
			vector = new Vector2(vector.X + (float)(_sizeOfUnderscoreSpace * base.Zoom), vector.Y);
		}
		base.DrawMisc(spriteBatch, drawColor);
	}

	private void AddLetterToPassword(char character)
	{
		if (_currentEnteredPassword.Length < 12)
		{
			_currentEnteredPassword += character;
			RefreshDisplayPassword();
		}
		else
		{
			PlayErrorSound();
		}
	}

	private void DeleteLetterFromPassword()
	{
		int length = _currentEnteredPassword.Length;
		if (length > 0)
		{
			_currentEnteredPassword = ((length == 1) ? "" : _currentEnteredPassword.SafeSubstring(0, length - 1));
			RefreshDisplayPassword();
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
		}
	}

	private void RefreshDisplayPassword()
	{
		int num = 0;
		string currentEnteredPassword = _currentEnteredPassword;
		for (int i = 0; i < currentEnteredPassword.Length; i++)
		{
			string text = currentEnteredPassword[i].ToString(CultureInfo.InvariantCulture);
			_displayCharacters[num] = text;
			Vector2 vector = base.Font.MeasureString(text);
			ref Vector2 reference = ref _displayCharacterOrigins[num];
			reference = new Vector2(vector.X / 2f, 0f);
			num++;
		}
		for (int j = num; j < 12; j++)
		{
			if (_displayCharacters[j] != " ")
			{
				_displayCharacters[j] = " ";
				break;
			}
		}
	}

	private void TryUnlockCode()
	{
		string currentEnteredPassword = _currentEnteredPassword;
		EPasswordUnlocks ePasswordUnlocks = EPasswordUnlocks.None;
		if (_meyefPasswords == null)
		{
			_meyefPasswords = new PasswordsMeyefSkin();
		}
		if (_meyefPasswords.Passwords.Contains(currentEnteredPassword))
		{
			ePasswordUnlocks = EPasswordUnlocks.MeyefSkin;
		}
		if (_crowPasswords == null)
		{
			_crowPasswords = new PasswordsCrowSkin();
		}
		if (_crowPasswords.Passwords.Contains(currentEnteredPassword))
		{
			ePasswordUnlocks = EPasswordUnlocks.CrowSkin;
		}
		if (_umbraPasswords == null)
		{
			_umbraPasswords = new PasswordsUmbraOrb();
		}
		if (_umbraPasswords.Passwords.Contains(currentEnteredPassword))
		{
			ePasswordUnlocks = EPasswordUnlocks.UmbraOrb;
		}
		if (_nightmarePasswords == null)
		{
			_nightmarePasswords = new PasswordsNightmareMode();
		}
		if (_nightmarePasswords.Passwords.Contains(currentEnteredPassword))
		{
			ePasswordUnlocks = EPasswordUnlocks.HardMode;
		}
		switch (currentEnteredPassword)
		{
		case "SPEEDRUNALPH":
			ePasswordUnlocks = EPasswordUnlocks.SpeedrunA;
			break;
		case "SPEEDRUNBETA":
			ePasswordUnlocks = EPasswordUnlocks.SpeedrunB;
			break;
		}
		if (ePasswordUnlocks != 0)
		{
			UnlockContent(ePasswordUnlocks);
		}
	}

	private void UnlockContent(EPasswordUnlocks unlockType)
	{
		switch (unlockType)
		{
		case EPasswordUnlocks.MeyefSkin:
			UnlockMeyefSkin(isPrimaryUnlock: true);
			break;
		case EPasswordUnlocks.CrowSkin:
			UnlockMeyefSkin(isPrimaryUnlock: false);
			UnlockUmbraOrb(isPrimaryUnlock: false);
			UnlockCrowSkin(isPrimaryUnlock: true);
			break;
		case EPasswordUnlocks.UmbraOrb:
			UnlockMeyefSkin(isPrimaryUnlock: false);
			UnlockUmbraOrb(isPrimaryUnlock: true);
			break;
		case EPasswordUnlocks.HardMode:
			UnlockHardMode();
			break;
		case EPasswordUnlocks.SpeedrunA:
			UnlockSpeedrunA();
			break;
		case EPasswordUnlocks.SpeedrunB:
			UnlockSpeedrunB();
			break;
		}
	}

	private void UnlockMeyefSkin(bool isPrimaryUnlock)
	{
		if (base.SaveFile == null)
		{
			return;
		}
		if (isPrimaryUnlock)
		{
			ChangeDescription(Loc.Get("PasswordMeyef"), EInventoryItemIcon.FamiliarMeyef);
		}
		if (!base.SaveFile.Inventory.RelicInventory.Inventory.ContainsKey(22))
		{
			if (isPrimaryUnlock)
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.CharacterLevelUp);
			}
			base.SaveFile.UnlockRelic(EInventoryRelicType.FamiliarAltMeyef);
		}
	}

	private void UnlockCrowSkin(bool isPrimaryUnlock)
	{
		if (base.SaveFile == null)
		{
			return;
		}
		if (isPrimaryUnlock)
		{
			ChangeDescription(Loc.Get("PasswordCrow"), EInventoryItemIcon.FamiliarMerchantCrow);
		}
		if (!base.SaveFile.Inventory.RelicInventory.Inventory.ContainsKey(23))
		{
			if (isPrimaryUnlock)
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.MenuBuy);
			}
			base.SaveFile.UnlockRelic(EInventoryRelicType.FamiliarAltCrow);
		}
		if (!base.SaveFile.Inventory.FamiliarInventory.Inventory.ContainsKey(3))
		{
			base.SaveFile.GiveFamiliar(EInventoryFamiliarType.MerchantCrow);
		}
	}

	private void UnlockUmbraOrb(bool isPrimaryUnlock)
	{
		if (base.SaveFile == null)
		{
			return;
		}
		if (isPrimaryUnlock)
		{
			ChangeDescription(Loc.Get("PasswordUmbra"), EInventoryItemIcon.UmbraOrb);
		}
		if (!base.SaveFile.Inventory.OrbInventory.Inventory.ContainsKey(9))
		{
			if (isPrimaryUnlock)
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.LunaisChargeShoot);
			}
			base.SaveFile.GiveOrb(EInventoryOrbType.Umbra, EOrbSlot.Melee);
		}
	}

	private void UnlockHardMode()
	{
		ChangeDescription(Loc.Get("PasswordHard"), EInventoryItemIcon.MaxSandUp);
		GameConfigSave configSave = base.ScreenManager.SaveFileManager.ConfigSave;
		if (!configSave.HasGameBeenCleared)
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.EnemyRoyalGuardDeathCry);
			configSave.HasGameBeenCleared = true;
			base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		}
	}

	private void UnlockSpeedrunA()
	{
		ChangeDescription(Loc.Get("PasswordSpeedrunA"), EInventoryItemIcon.PointyHat);
		base.ScreenManager.Jukebox.PlayCue(ESFX.FoleyTreasureOpen);
		base.SaveFile.FlagSpeedrun(1);
	}

	private void UnlockSpeedrunB()
	{
		ChangeDescription(Loc.Get("PasswordSpeedrunB"), EInventoryItemIcon.EternalBrooch);
		base.ScreenManager.Jukebox.PlayCue(ESFX.FoleyWarpCutsceneExit);
		base.SaveFile.FlagSpeedrun(2);
	}
}
