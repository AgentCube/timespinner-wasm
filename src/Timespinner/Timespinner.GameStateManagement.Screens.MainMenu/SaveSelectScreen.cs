using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class SaveSelectScreen : InventoryMenuScreen
{
	private const int CursorFrameIndex = 138;

	private const int EntryHeightOffset = 31;

	private const int CursorBaseOffsetX = 23;

	private const int CursorBaseOffsetY = -2;

	private const int CursorBottomOffsetY = 30;

	private const int CursorOscillationOffset = 2;

	private const int HeaderOffsetY = 19;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const float PrimaryMenuDisplayRatioX = 0.075f;

	private const float PrimaryMenuDisplayOffsetY = 11f;

	private const float ScrollbarPositionRatioY = 31f / 192f;

	private const float ScrollbarHeightRatio = 17f / 24f;

	private const float TimeToHoldBeforeDeleting = 2.5f;

	private readonly Rectangle _customCursorFrameSource;

	private readonly SpriteSheet _sprite;

	private readonly SaveFileManager _saveFileManager;

	private bool _isDeleting;

	private int _cursorOscillationOffset;

	private int _headerHeight;

	private int _deletingIndex;

	private float _customCursorOffset;

	private float _cursorOffsetY;

	private float _cursorBottomOffsetY;

	private float _baseCursorLeft;

	private float _baseCursorRight;

	private float _cursorLeft;

	private float _cursorRight;

	private float _cursorTop;

	private float _cursorBottom;

	private float _deleteProgressTimer;

	private PlayerIndex? _difficultyPlayerIndex;

	private Rectangle _backgroundDrawRectangle;

	private SaveFileMenuEntryCollection _saveFileCollection;

	public SaveSelectScreen(SaveFileManager saveFileManager, GCM gcm)
		: base(Loc.Get("SaveSelectTitle"), null, gcm, null)
	{
		_saveFileManager = saveFileManager;
		_sprite = gcm.SpPauseMenu;
		_customCursorFrameSource = _sprite.GetFrameSource(138);
		base.IsPopupScreen = false;
		base.IsOverlayScreen = false;
		base.DoesDrawScrollbarWidget = true;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		base.DescriptionControllerMapping = base.ScreenManager.MenuControllerMapping;
		_saveFileCollection = new SaveFileMenuEntryCollection(_saveFileManager.AvailableSaves, OnEntrySelected, base.ScreenManager.MenuFont, _sprite, base.Zoom)
		{
			IsVisible = true,
			EntryHeightOffset = 31,
			ScrollRowHeight = 3,
			DoesMenuAllowScrolling = true
		};
		_primaryMenuCollection = _saveFileCollection;
		_selectedMenuCollection = _saveFileCollection;
		RefreshSizes();
		OnSelectedEntryChanged(0);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
		if (_saveFileCollection != null)
		{
			_saveFileCollection.DrawPosition = new Vector2(0.075f * (float)_screenWidth + (float)_screenLeft, _primaryMenuCollection.DrawPosition.Y + 11f * (float)base.Zoom);
			_saveFileCollection.RefreshSizes(base.Zoom);
		}
		_baseCursorLeft = _screenLeft + 23 * base.Zoom;
		_baseCursorRight = _screenLeft + _screenWidth - 23 * base.Zoom;
		_cursorOffsetY = -2 * base.Zoom;
		_cursorBottomOffsetY = 30 * base.Zoom;
		_cursorOscillationOffset = 2 * base.Zoom;
		_headerHeight = 19 * base.Zoom;
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(31f / 192f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(17f / 24f * (float)_topSectionHeight);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (_isDeleting)
		{
			if (_deleteProgressTimer < 2.5f)
			{
				float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
				_deleteProgressTimer += num;
				if (_deleteProgressTimer >= 2.5f)
				{
					_saveFileCollection.DeletePercentage = 0f;
					StartDelete();
				}
				else
				{
					_saveFileCollection.DeletePercentage = _deleteProgressTimer / 2.5f;
				}
			}
		}
		else
		{
			_deleteProgressTimer = 0f;
			_saveFileCollection.DeletePercentage = 0f;
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		UpdateCustomCursor();
	}

	public override void HandleInput(InputState input)
	{
		base.HandleInput(input);
		if (_saveFileCollection.IsSelectingNonBlankSave)
		{
			if (input.IsPressSecondary(base.ControllingPlayer) || input.IsKeyHold(Keys.Delete, base.ControllingPlayer, out var _))
			{
				if (!_isDeleting)
				{
					_deletingIndex = base.SelectedIndex;
					_isDeleting = true;
				}
				else if (base.SelectedIndex != _deletingIndex)
				{
					_isDeleting = false;
					_deleteProgressTimer = 0f;
				}
			}
			else
			{
				_isDeleting = false;
				if (input.IsNewPressTertiary(base.ControllingPlayer) && _saveFileCollection.IsSelectingClearedSave())
				{
					base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
					MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("SaveSelectConfirmNGPlus"), base.ScreenManager.MenuControllerMapping);
					messageBoxScreen.Accepted += ConfirmRestartAccepted;
					base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
				}
			}
		}
		if (input.IsNewPressExit(base.ControllingPlayer))
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
			ReturnToTitle(base.ControllingPlayer);
		}
	}

	private void StartDelete()
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("SaveSelectConfirmDelete"), base.ScreenManager.MenuControllerMapping);
		messageBoxScreen.Accepted += ConfirmDeleteAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
	}

	private void ConfirmDeleteAccepted(object sender, PlayerIndexEventArgs e)
	{
		if (base.SelectedIndex < base.MenuEntries.Count)
		{
			SaveFileMenuEntry saveFileMenuEntry = _saveFileCollection.SaveFiles[base.SelectedIndex];
			_saveFileManager.RequestGameSaveDelete(saveFileMenuEntry.SaveFile);
			_saveFileCollection.DeleteSelectedFile();
			OnSelectedEntryChanged(base.SelectedIndex);
		}
	}

	private void ConfirmRestartAccepted(object sender, PlayerIndexEventArgs e)
	{
		_difficultyPlayerIndex = e.PlayerIndex;
		GameDifficultyMenu screen = new GameDifficultyMenu(_saveFileManager, OnDifficultySelectedNgp);
		base.ScreenManager.AddScreen(screen, _difficultyPlayerIndex);
	}

	private void OnDifficultySelectedNgp(GameSave.EGameDifficultyType difficulty)
	{
		if (base.SelectedIndex < base.MenuEntries.Count)
		{
			GameSave gameSave = _saveFileCollection.SaveFiles[base.SelectedIndex].SaveFile;
			bool saveBool = gameSave.GetSaveBool("IsFlaggedSpeedrunA");
			bool saveBool2 = gameSave.GetSaveBool("IsFlaggedSpeedrunB");
			if (!saveBool && !saveBool2)
			{
				gameSave.ResetForNewGamePlus(difficulty);
			}
			else if (saveBool)
			{
				gameSave = GameSave.CreateNewSpeedrunASave(base.SelectedIndex, difficulty);
			}
			else
			{
				gameSave.ResetForSpeedrunB(difficulty);
			}
			LoadGame(_difficultyPlayerIndex, gameSave);
		}
	}

	private void ReturnToTitle(PlayerIndex? playerIndex)
	{
		base.ScreenManager.AddScreen(new TitleBackgroundScreen(shouldDoFullIntro: false), playerIndex);
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		base.OnCancel(playerIndex);
		ReturnToTitle(playerIndex);
	}

	private void OnEntrySelected(SaveFileMenuEntry selectedEntry, PlayerIndex playerIndex)
	{
		if (_saveFileCollection.IsSelectingNonBlankSave)
		{
			GameSave saveFile = selectedEntry.SaveFile;
			if (!saveFile.IsCorrupt)
			{
				LoadGame(playerIndex, saveFile);
			}
			else
			{
				PlayErrorSound();
			}
		}
		else
		{
			StartNewGame(playerIndex);
		}
	}

	private void LoadGame(PlayerIndex? playerIndex, GameSave gameSave)
	{
		LoadingScreen.Load(base.ScreenManager, true, playerIndex, new GameplayScreen(gameSave, _saveFileManager.ConfigSave));
		base.ScreenManager.RemoveScreen(this);
	}

	private void StartNewGame(PlayerIndex? playerIndex)
	{
		NewGamePickDifficulty(playerIndex);
	}

	private void NewGamePickDifficulty(PlayerIndex? playerIndex)
	{
		_difficultyPlayerIndex = playerIndex;
		GameDifficultyMenu screen = new GameDifficultyMenu(_saveFileManager, OnDifficultySelected);
		base.ScreenManager.AddScreen(screen, playerIndex);
	}

	private void OnDifficultySelected(GameSave.EGameDifficultyType difficulty)
	{
		GameSave inSave = MainMenuScreen.CreateNewSave(base.SelectedIndex, _saveFileManager, difficulty);
		LoadingScreen.Load(base.ScreenManager, true, _difficultyPlayerIndex, new GameplayScreen(inSave, _saveFileManager.ConfigSave));
		base.ScreenManager.RemoveScreen(this);
	}

	private void UpdateCustomCursor()
	{
		float num = (float)Math.Sin(base.CursorOscillation) * (float)_cursorOscillationOffset;
		_customCursorOffset = ((num < 0f) ? 0f : num);
		if (base.Zoom < 2)
		{
			_customCursorOffset = (float)Math.Ceiling(_customCursorOffset);
		}
		float num2 = base.CursorPosition.Y + _cursorOffsetY;
		_cursorLeft = _baseCursorLeft - _customCursorOffset;
		_cursorRight = _baseCursorRight + _customCursorOffset;
		_cursorTop = num2 - _customCursorOffset;
		_cursorBottom = num2 + _customCursorOffset;
	}

	protected override void DrawCursor(SpriteBatch spriteBatch)
	{
		Color color = Color.White * (1f - base.TransitionOffPercentage);
		Vector2 origin = new Vector2(14.5f, 14.5f);
		spriteBatch.Draw(position: new Vector2(_cursorLeft, _cursorTop), texture: _sprite.Texture, sourceRectangle: _customCursorFrameSource, color: color, rotation: 0f, origin: origin, scale: base.Zoom, effects: SpriteEffects.None, layerDepth: 0f);
		spriteBatch.Draw(position: new Vector2(_cursorLeft, _cursorBottom + _cursorBottomOffsetY), texture: _sprite.Texture, sourceRectangle: _customCursorFrameSource, color: color, rotation: 0f, origin: origin, scale: base.Zoom, effects: SpriteEffects.FlipVertically, layerDepth: 0f);
		spriteBatch.Draw(position: new Vector2(_cursorRight, _cursorTop), texture: _sprite.Texture, sourceRectangle: _customCursorFrameSource, color: color, rotation: 0f, origin: origin, scale: base.Zoom, effects: SpriteEffects.FlipHorizontally, layerDepth: 0f);
		spriteBatch.Draw(position: new Vector2(_cursorRight, _cursorBottom + _cursorBottomOffsetY), texture: _sprite.Texture, sourceRectangle: _customCursorFrameSource, color: color, rotation: 0f, origin: origin, scale: base.Zoom, effects: SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, layerDepth: 0f);
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

	public override void DrawHeader(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteEffects[] flipped = new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		};
		DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(_screenLeft, _screenTop + _headerHeight, _screenWidth, _topSectionHeight - _headerHeight), drawColor, base.Sprite, base.Zoom, new int[9] { 136, 137, 136, 25, -1, 26, 136, 137, 136 }, flipped, shouldTile: true);
		DrawingEx.DrawShortBox(spriteBatch, new Rectangle(_screenLeft, _screenTop, _screenWidth, _headerHeight), drawColor, base.Sprite, base.Zoom, new int[3] { 134, 135, 134 }, flipped, shouldTile: true);
	}
}
