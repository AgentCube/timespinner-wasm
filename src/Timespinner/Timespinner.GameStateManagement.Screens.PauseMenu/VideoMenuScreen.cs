using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class VideoMenuScreen : InventoryMenuScreen
{
	private const float TopBackgroundDrawOffsetY = 7f / 64f;

	private static readonly EGameResolutionType[] AvailableResolution = new EGameResolutionType[6]
	{
		EGameResolutionType.R1280_720,
		EGameResolutionType.R1600_960,
		EGameResolutionType.R1920_1080,
		EGameResolutionType.R960_544,
		EGameResolutionType.R800_480,
		EGameResolutionType.R400_240
	};

	private readonly bool _wasFullscreen;

	private readonly bool _wasDrawingBorder;

	private readonly int _originalResolution;

	private readonly GameConfigSave _configSave;

	private bool _wereScreenSettingsJustChanged;

	private bool _wasFullscreenJustChanged;

	private bool _hasUpdatedSinceScreenResolutionChange;

	private bool _hasDrawnSinceScreenResolutionChange;

	private EGameResolutionType _resolutionBeforeChange;

	private Rectangle _topBackgroundDrawRectangle;

	public VideoMenuScreen(GameSave inSave, GameConfigSave configSave, GCM gcm, Action fullExitAction)
		: base(Loc.Get("VideoMenuTitle"), inSave, gcm, fullExitAction)
	{
		_configSave = configSave;
		_wasFullscreen = _configSave.IsFullScreen;
		_wasDrawingBorder = _configSave.DoesDrawBorderFrame;
		_originalResolution = _configSave.ScreenResolutionType;
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		_doesUseCursor = true;
		string format = Loc.Get("VideoMenuResolutionTitle");
		EGameResolutionType[] availableResolution = AvailableResolution;
		foreach (EGameResolutionType eGameResolutionType in availableResolution)
		{
			string text = string.Format(format, eGameResolutionType.ToString().Replace('_', 'x').Replace("R", ""));
			MenuEntry menuEntry = new MenuEntry(text)
			{
				Description = Loc.Get("VideoMenuResolutionDescription")
			};
			EGameResolutionType resolution = eGameResolutionType;
			menuEntry.Selected += delegate
			{
				ChangeResolution(resolution);
			};
			base.MenuEntries.Add(menuEntry);
		}
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("VideoMenuFullscreenTitle"))
		{
			Description = Loc.Get("VideoMenuFullscreenDescription")
		};
		menuEntry2.Selected += delegate
		{
			ToggleFullscreen();
		};
		base.MenuEntries.Add(menuEntry2);
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("VideoMenuBorderTitle"))
		{
			Description = Loc.Get("VideoMenuBorderDescription")
		};
		menuEntry3.Selected += delegate
		{
			ToggleDrawBorder();
		};
		base.MenuEntries.Add(menuEntry3);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (_wereScreenSettingsJustChanged && !_hasUpdatedSinceScreenResolutionChange)
		{
			_hasUpdatedSinceScreenResolutionChange = true;
		}
		else if (_wereScreenSettingsJustChanged && _hasUpdatedSinceScreenResolutionChange && _hasDrawnSinceScreenResolutionChange)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("VideoMenuConfirmResolution"), base.ScreenManager.MenuControllerMapping);
			messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
			messageBoxScreen.Cancelled += CancelExitMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
			_wereScreenSettingsJustChanged = false;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);
		if (_wereScreenSettingsJustChanged && _hasUpdatedSinceScreenResolutionChange)
		{
			_hasDrawnSinceScreenResolutionChange = true;
		}
	}

	private void ChangeResolution(EGameResolutionType resolution)
	{
		_resolutionBeforeChange = base.ScreenManager.CurrentResolution;
		if (_resolutionBeforeChange != resolution)
		{
			base.ScreenManager.ChangeResolution(resolution, isForcingWindowed: true);
			_hasDrawnSinceScreenResolutionChange = false;
			_hasUpdatedSinceScreenResolutionChange = false;
			_wereScreenSettingsJustChanged = true;
			_wasFullscreenJustChanged = false;
		}
	}

	private void ToggleFullscreen()
	{
		_resolutionBeforeChange = base.ScreenManager.CurrentResolution;
		base.ScreenManager.ToggleFullscreen();
		_hasDrawnSinceScreenResolutionChange = false;
		_hasUpdatedSinceScreenResolutionChange = false;
		_wereScreenSettingsJustChanged = true;
		_wasFullscreenJustChanged = true;
	}

	private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		_configSave.ScreenResolutionType = (int)base.ScreenManager.CurrentResolution;
		_configSave.IsFullScreen = base.ScreenManager.IsFullscreen;
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
	}

	private void CancelExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		if (_wasFullscreenJustChanged)
		{
			base.ScreenManager.ToggleFullscreen();
		}
		base.ScreenManager.ChangeResolution(_resolutionBeforeChange, isForcingWindowed: false);
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
	}

	private void ToggleDrawBorder()
	{
		base.ScreenManager.ToggleBorderDraw();
	}

	public override void ExitScreen()
	{
		if (_originalResolution != _configSave.ScreenResolutionType || _wasDrawingBorder != _configSave.DoesDrawBorderFrame || _wasFullscreen != _configSave.IsFullScreen)
		{
			base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		}
		base.ExitScreen();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_topBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
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
		}, spriteBatch: spriteBatch, backgroundRectangle: _topBackgroundDrawRectangle, color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 45, 46, 45, 47, 48, 47, 45, 46, 45 }, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
	}
}
