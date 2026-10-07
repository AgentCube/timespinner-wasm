using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class SaveFileMenuEntry : MenuEntry
{
	private const int BackingWidth = 286;

	private const int BackingHeight = 42;

	private const int BackingOffsetX = -8;

	private const int BackingOffsetY = -14;

	private const int TextOffsetX = 44;

	private const int ColumnsMarginX = 0;

	private const int EntryTotalDisplayWidth = 229;

	private const int OrbRowColumnOffset = 11;

	private const int OrbDrawOffsetX = -2;

	private const int OrbDrawOffsetY = 3;

	private const int NewGameOffsetX = 135;

	private const int LatinSaveIndexOffsetX = 3;

	private const int AsianSaveIndexOffsetX = 2;

	private const int SaveIndexOffsetY = 6;

	private const int ClearedIconOffsetX = 7;

	private const int ClearedIconOffsetY = 1;

	private const int DifficultyModeOffsetX = 12;

	private const int SpeedrunCornerFrameIndex = 153;

	private const int SpeedrunAlphaFrameIndex = 154;

	private const int SpeedrunBetaFrameIndex = 155;

	private const int SpeedrunCornerDrawOffsetX = 16;

	private const int SpeedrunCornerDrawOffsetY = -14;

	private const int SpeedrunLetterDrawOffsetX = 26;

	private const int SpeedrunLetterDrawOffsetY = -13;

	private const int MaxMapPercentage = 100;

	private const int MaxLevel = 255;

	private static readonly Color CorruptedColor = new Color(248, 64, 16);

	private readonly bool _isEmptySaveSlot;

	private readonly bool _isCleared;

	private readonly bool _isEasyMode;

	private readonly bool _isHardMode;

	private readonly bool _isCorrupt;

	private readonly bool _isAnySpeedrun;

	private readonly bool _isSpeedrunA;

	private readonly bool _isSpeedrunB;

	private readonly int _equippedMeleeOrbAIndex;

	private readonly int _equippedMeleeOrbBIndex;

	private readonly int _equippedSpellOrbIndex;

	private readonly int _equippedPassiveOrbIndex;

	private readonly int _playerLevel;

	private readonly int _leftColumnWidth;

	private readonly int _difficultyOffsetX;

	private readonly int _centerTextWidth;

	private readonly string _nonSaveString;

	private readonly string _saveIndexString;

	private readonly string _mapString;

	private readonly string _playerLevelString;

	private readonly string _difficultyModeString;

	private readonly ScrollableTextBlock _areaNameTextBlock;

	private readonly ScrollableTextBlock _timeTextBlock;

	private readonly SpriteFont _font;

	private readonly GameSave _saveFile;

	private int _backingWidthZoomed;

	private int _backingHeightZoomed;

	private int _backingOffsetX;

	private int _backingOffsetY;

	private int _textOffsetX;

	private int _saveColumnOffsetX;

	private int _orbRowColumnOffset;

	private int _orbDrawOffsetX;

	private int _orbDrawOffsetY;

	private int _saveIndexDrawOffsetX;

	private int _saveIndexDrawOffsetY;

	private int _clearedIconDrawOffsetX;

	private int _clearedIconDrawOffsetY;

	private int _difficultyModeDrawOffsetX;

	private int _speedrunCornerDrawOffsetX;

	private int _speedrunCornerDrawOffsetY;

	private int _speedrunLetterDrawOffsetX;

	private int _speedrunLetterDrawOffsetY;

	private int _zoom;

	internal bool IsEmptySaveSlot => _isEmptySaveSlot;

	internal bool IsCorrupt => _isCorrupt;

	internal GameSave SaveFile => _saveFile;

	internal SaveFileMenuEntry(GameSave save, SpriteFont font, int zoom, string mapString, string playtimeString, string levelString)
		: base("")
	{
		_saveFile = save;
		_font = font;
		_isEmptySaveSlot = _saveFile == null;
		if (_saveFile != null)
		{
			_isCorrupt = _saveFile.IsCorrupt;
			if (!_isCorrupt)
			{
				string levelNameFromID = Level.GetLevelNameFromID(save.CurrentLevel);
				_mapString = ((save.MinimapSave != null) ? string.Format("{0} {1}", mapString, save.MinimapSave.CompletionRate + "%") : "0%");
				TimeSpan timeSpan = TimeSpan.FromSeconds(save.ElapsedGameSeconds);
				string text = $"{playtimeString} {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
				_playerLevel = save.CharacterStats.Level + 1;
				_playerLevelString = $"{levelString} {_playerLevel}";
				_equippedMeleeOrbAIndex = GetIndexFromOrb(save.Inventory.EquippedMeleeOrbA);
				_equippedMeleeOrbBIndex = GetIndexFromOrb(save.Inventory.EquippedMeleeOrbB);
				_equippedSpellOrbIndex = GetIndexFromOrb(save.Inventory.EquippedSpellOrb);
				_equippedPassiveOrbIndex = GetIndexFromOrb(save.Inventory.EquippedPassiveOrb);
				_isCleared = save.IsGameCleared;
				_saveIndexString = (save.SaveFileIndex + 1).ToString(CultureInfo.InvariantCulture);
				base.Description = (_isCleared ? Loc.Get("SaveSelectContClearedDescription") : Loc.Get("SaveSelectContinueDescription"));
				_isHardMode = save.IsHardMode;
				_isEasyMode = save.IsEasyMode && !_isHardMode;
				_difficultyOffsetX = 0;
				if (_isHardMode)
				{
					_difficultyModeString = Loc.Get("SaveSelectHardMode");
					_difficultyOffsetX = (int)(_font.MeasureString(_difficultyModeString).X + 12f);
				}
				else if (_isEasyMode)
				{
					_difficultyModeString = Loc.Get("SaveSelectEasyMode");
					_difficultyOffsetX = (int)(_font.MeasureString(_difficultyModeString).X + 12f);
				}
				_isSpeedrunA = _saveFile.GetSaveBool("IsFlaggedSpeedrunA");
				_isSpeedrunB = _saveFile.GetSaveBool("IsFlaggedSpeedrunB");
				_isAnySpeedrun = _isSpeedrunA || _isSpeedrunB;
				string text2 = $"{levelString} {255}";
				float x = _font.MeasureString(text2).X;
				string text3 = string.Format("{0} {1}", mapString, 100 + "%");
				float x2 = _font.MeasureString(text3).X;
				float num = MathEx.Max(x, x2);
				_leftColumnWidth = (int)(229f - num);
				_areaNameTextBlock = new ScrollableTextBlock(_font, _leftColumnWidth, Vector2.Zero, isTextCentered: false);
				_areaNameTextBlock.SetText(levelNameFromID);
				int baseWidth = _leftColumnWidth - _difficultyOffsetX;
				_timeTextBlock = new ScrollableTextBlock(_font, baseWidth, Vector2.Zero, isTextCentered: false);
				_timeTextBlock.SetText(text);
			}
			else
			{
				_nonSaveString = Loc.Get("SaveSelectCorruptFile");
				base.Description = Loc.Get("SaveSelectCorruptFileDescription");
				_centerTextWidth = (int)_font.MeasureString(_nonSaveString).X;
			}
		}
		else
		{
			_nonSaveString = levelString;
			base.Description = Loc.Get("SaveSelectNewGameDescription");
			_centerTextWidth = (int)_font.MeasureString(_nonSaveString).X;
		}
		RefreshSizes(zoom);
	}

	internal void RefreshSizes(int zoom)
	{
		_zoom = zoom;
		_backingWidthZoomed = 286 * _zoom;
		_backingHeightZoomed = 42 * _zoom;
		_backingOffsetX = -8 * _zoom;
		_backingOffsetY = -14 * _zoom;
		_orbRowColumnOffset = 11 * _zoom;
		_orbDrawOffsetX = -2 * _zoom;
		_orbDrawOffsetY = 3 * _zoom;
		_clearedIconDrawOffsetX = 7 * _zoom;
		_clearedIconDrawOffsetY = _zoom;
		_saveIndexDrawOffsetX = (Loc.IsAsianLocale ? 2 : 3) * _zoom;
		_saveIndexDrawOffsetY = 6 * _zoom;
		_speedrunCornerDrawOffsetX = 16 * _zoom;
		_speedrunCornerDrawOffsetY = -14 * _zoom;
		_speedrunLetterDrawOffsetX = 26 * _zoom;
		_speedrunLetterDrawOffsetY = -13 * _zoom;
		_saveColumnOffsetX = _leftColumnWidth * _zoom;
		_difficultyModeDrawOffsetX = _difficultyOffsetX * zoom;
		if (_saveFile != null && !_isCorrupt)
		{
			_textOffsetX = 44 * zoom;
		}
		else
		{
			_textOffsetX = (int)(135f - (float)_centerTextWidth * 0.5f) * zoom;
		}
	}

	private static int GetIndexFromOrb(EInventoryOrbType orbType)
	{
		int num = -1;
		if (orbType != 0)
		{
			num = (int)(orbType + 8);
			if (num > 18)
			{
				num += 77;
			}
		}
		return num;
	}

	internal void Draw(SpriteBatch spriteBatch, SpriteSheet sprite, float drawAlpha, bool isSelected, float deletePercentage)
	{
		Color color = Color.White * drawAlpha;
		Vector2 drawPosition = base.DrawPosition;
		Vector2 origin = new Vector2(0f, (float)_font.LineSpacing / 2f);
		Point point = new Point((int)(drawPosition.X + (float)_backingOffsetX), (int)(drawPosition.Y + (float)_backingOffsetY));
		SpriteEffects[] array = new SpriteEffects[9];
		if (_isEmptySaveSlot || _isCorrupt)
		{
			array[0] = SpriteEffects.FlipHorizontally;
			DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(point.X, point.Y, _backingWidthZoomed, _backingHeightZoomed), color, sprite, _zoom, new int[9] { 77, 76, 77, -1, -1, -1, -1, -1, -1 }, array, shouldTile: true);
			DrawingEx.DrawString(drawPos: new Vector2(drawPosition.X + (float)_textOffsetX, drawPosition.Y + (float)(_font.LineSpacing * _zoom) * 0.5f), color: (!_isEmptySaveSlot) ? (CorruptedColor * drawAlpha) : ((isSelected ? MenuEntry.SelectedColor : MenuEntry.UnselectedColor) * drawAlpha), spriteBatch: spriteBatch, font: _font, text: _nonSaveString, origin: origin, zoom: _zoom);
		}
		else
		{
			DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(point.X, point.Y, _backingWidthZoomed, _backingHeightZoomed), color, sprite, _zoom, new int[9] { 75, 76, 77, -1, -1, -1, -1, -1, -1 }, array, shouldTile: true);
			if (_isAnySpeedrun)
			{
				Rectangle frameSource = sprite.GetFrameSource(153);
				spriteBatch.Draw(sprite.Texture, new Vector2(base.DrawPosition.X + (float)_speedrunCornerDrawOffsetX, base.DrawPosition.Y + (float)_speedrunCornerDrawOffsetY), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
				if (_isSpeedrunA)
				{
					frameSource = sprite.GetFrameSource(154);
					spriteBatch.Draw(sprite.Texture, new Vector2(base.DrawPosition.X + (float)_speedrunLetterDrawOffsetX, base.DrawPosition.Y + (float)_speedrunLetterDrawOffsetY), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
				}
				else if (_isSpeedrunB)
				{
					frameSource = sprite.GetFrameSource(155);
					spriteBatch.Draw(sprite.Texture, new Vector2(base.DrawPosition.X + (float)_speedrunLetterDrawOffsetX, base.DrawPosition.Y + (float)_speedrunLetterDrawOffsetY), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
				}
			}
			if (_isCleared)
			{
				Rectangle frameSource = sprite.GetFrameSource(145);
				spriteBatch.Draw(sprite.Texture, new Vector2(base.DrawPosition.X + (float)_clearedIconDrawOffsetX, base.DrawPosition.Y + (float)_clearedIconDrawOffsetY), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			Vector2 position = new Vector2(base.DrawPosition.X + (float)_orbDrawOffsetX, base.DrawPosition.Y + (float)_orbDrawOffsetY);
			if (_equippedMeleeOrbAIndex != -1)
			{
				Rectangle frameSource = sprite.GetFrameSource(_equippedMeleeOrbAIndex);
				spriteBatch.Draw(sprite.Texture, position, frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			if (_equippedMeleeOrbBIndex != -1)
			{
				Rectangle frameSource = sprite.GetFrameSource(_equippedMeleeOrbBIndex);
				spriteBatch.Draw(sprite.Texture, new Vector2(position.X + (float)(2 * _orbRowColumnOffset), position.Y), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			if (_equippedSpellOrbIndex != -1)
			{
				Rectangle frameSource = sprite.GetFrameSource(_equippedSpellOrbIndex);
				spriteBatch.Draw(sprite.Texture, new Vector2(position.X + (float)_orbRowColumnOffset, position.Y - (float)_orbRowColumnOffset), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			if (_equippedPassiveOrbIndex != -1)
			{
				Rectangle frameSource = sprite.GetFrameSource(_equippedPassiveOrbIndex);
				spriteBatch.Draw(sprite.Texture, new Vector2(position.X + (float)_orbRowColumnOffset, position.Y + (float)_orbRowColumnOffset), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			color = (isSelected ? MenuEntry.SelectedColor : MenuEntry.UnselectedColor) * drawAlpha;
			DrawingEx.DrawString(drawPos: new Vector2(point.X + _saveIndexDrawOffsetX, point.Y + _saveIndexDrawOffsetY), spriteBatch: spriteBatch, font: _font, text: _saveIndexString, color: color, origin: origin, zoom: _zoom);
			drawPosition = new Vector2(base.DrawPosition.X + (float)_textOffsetX, base.DrawPosition.Y);
			_areaNameTextBlock.SetTopLeft(new Vector2(drawPosition.X, drawPosition.Y - origin.Y * (float)_zoom));
			_areaNameTextBlock.Draw(spriteBatch, color, Color.Transparent, null);
			drawPosition = drawPosition.Add(new Point(0, _font.LineSpacing * _zoom));
			if (_isHardMode || _isEasyMode)
			{
				DrawingEx.DrawString(spriteBatch, _font, _difficultyModeString, drawPosition, color, origin, _zoom);
				drawPosition = drawPosition.Add(new Point(_difficultyModeDrawOffsetX, 0));
			}
			_timeTextBlock.SetTopLeft(new Vector2(drawPosition.X, drawPosition.Y - origin.Y * (float)_zoom));
			_timeTextBlock.Draw(spriteBatch, color, Color.Transparent, null);
			drawPosition = new Vector2(base.DrawPosition.X + (float)_textOffsetX + (float)_saveColumnOffsetX, base.DrawPosition.Y);
			DrawingEx.DrawString(spriteBatch, _font, _playerLevelString, drawPosition, color, origin, _zoom);
			DrawingEx.DrawString(drawPos: new Vector2(drawPosition.X, drawPosition.Y + (float)(_font.LineSpacing * _zoom)), spriteBatch: spriteBatch, font: _font, text: _mapString, color: color, origin: origin, zoom: _zoom);
		}
		if (isSelected && deletePercentage > 0f)
		{
			Rectangle destinationRectangle = new Rectangle(point.X, point.Y, (int)Math.Ceiling((float)_backingWidthZoomed * deletePercentage), _backingHeightZoomed);
			Rectangle frameSource2 = sprite.GetFrameSource(1);
			Color color3 = Color.Red * (0.35f + 0.35f * deletePercentage);
			spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource2, color3);
		}
	}
}
