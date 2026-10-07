using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class AudioMenuScreen : InventoryMenuScreen
{
	private const int HorizontalBarBuffer = 167;

	private const int HorizontalBarWidth = 100;

	private const int TopArrowOffsetY = -2;

	private const int BottomArrowOffsetY = 7;

	private const float MaxSFXRepeatTimer = 0.5f;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private readonly float _initialVolumeMaster;

	private readonly float _initialVolumeMusic;

	private readonly float _initialVolumeSFX;

	private readonly float _initialVolumeVO;

	private readonly ScrollThrottle _scrollThrottle;

	private readonly GameConfigSave _saveFile;

	private readonly Action _finalFullExitAction;

	private bool _isAdjustingVolume;

	private int _selectedVolume = -1;

	private int _horizontalBarBuffer;

	private int _horizontalBarWidth;

	private int _topArrowOffsetY;

	private int _bottomArrowOffsetY;

	private float _sfxRepeatTimer;

	private Point _volumeNumberDrawOffset;

	private Rectangle _backgroundDrawRectangle;

	private List<float> _globalVolumes;

	public AudioMenuScreen(GameConfigSave configFile, GameSave saveFile, GCM gcm, Action fullExitAction)
		: base(Loc.Get("AudioMenuTitle"), saveFile, gcm, fullExitAction)
	{
		_saveFile = configFile;
		_finalFullExitAction = fullExitAction;
		_initialVolumeMaster = _saveFile.AudioVolumeMaster;
		_initialVolumeMusic = _saveFile.AudioVolumeMusic;
		_initialVolumeSFX = _saveFile.AudioVolumeSFX;
		_initialVolumeVO = _saveFile.AudioVolumeVO;
		MenuEntry menuEntry = new MenuEntry(Loc.Get("MasterVolume"))
		{
			Description = Loc.Get("MasterVolumeDescription")
		};
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("MusicVolume"))
		{
			Description = Loc.Get("MusicVolumeDescription")
		};
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("VoiceVolume"))
		{
			Description = Loc.Get("VoiceVolumeDescription")
		};
		MenuEntry menuEntry4 = new MenuEntry(Loc.Get("SFXVolume"))
		{
			Description = Loc.Get("SFXVolumeDescription")
		};
		MenuEntry menuEntry5 = new MenuEntry(Loc.Get("ResetAudioSettings"))
		{
			Description = Loc.Get("ResetAudioSettingsDescription")
		};
		menuEntry.Selected += MasterMenuEntrySelected;
		menuEntry2.Selected += MusicMenuEntrySelected;
		menuEntry3.Selected += VoiceMenuEntrySelected;
		menuEntry4.Selected += SFXMenuEntrySelected;
		menuEntry5.Selected += DefaultMenuEntrySelected;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(menuEntry3);
		base.MenuEntries.Add(menuEntry4);
		base.MenuEntries.Add(menuEntry5);
		_scrollThrottle = new ScrollThrottle
		{
			MinScrollRate = 0,
			MaxScrollRate = 12,
			ScrollGrowthRate = 2
		};
	}

	public override void LoadContent()
	{
		_globalVolumes = base.ScreenManager.Jukebox.GetGlobalVolumes();
		base.LoadContent();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		_volumeNumberDrawOffset = new Point(-24 * Constants.InGameZoom, -8 * Constants.InGameZoom);
		_primaryMenuCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_horizontalBarBuffer = 167 * base.Zoom;
		_horizontalBarWidth = 100 * base.Zoom;
		_topArrowOffsetY = -2 * base.Zoom;
		_bottomArrowOffsetY = 7 * base.Zoom;
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
	}

	public override void HandleInput(InputState input)
	{
		if (!_isAdjustingVolume)
		{
			base.HandleInput(input);
			return;
		}
		int num = 0;
		if (input.IsPressMenuLeft(base.ControllingPlayer))
		{
			num = -1;
		}
		else if (input.IsPressMenuRight(base.ControllingPlayer))
		{
			num = 1;
		}
		if (num != 0)
		{
			if (_scrollThrottle.IsScrollReady())
			{
				float num2 = (float)Math.Round(_globalVolumes[_selectedVolume], 2);
				float value = num2 + (float)num * 0.02f;
				_globalVolumes[_selectedVolume] = MathHelper.Clamp(value, 0f, 2f);
				base.ScreenManager.Jukebox.AdjustGlobalVolume(_globalVolumes[_selectedVolume], CategoryFromIndex(_selectedVolume));
				SaveVolumes();
			}
		}
		else
		{
			_scrollThrottle.Reset();
			_scrollThrottle.Ready();
		}
		if (input.IsNewPressConfirmCancel(base.ControllingPlayer))
		{
			_isAdjustingVolume = false;
			_selectedVolume = -1;
		}
		if (input.IsNewPressExit(base.ControllingPlayer))
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
			_finalFullExitAction();
		}
	}

	private EVolumeCategory CategoryFromIndex(int index)
	{
		EVolumeCategory result = EVolumeCategory.Master;
		switch (index)
		{
		case 0:
			result = EVolumeCategory.Master;
			break;
		case 1:
			result = EVolumeCategory.Music;
			break;
		case 2:
			result = EVolumeCategory.Voice;
			break;
		case 3:
			result = EVolumeCategory.SFX;
			break;
		}
		return result;
	}

	private void MasterMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_isAdjustingVolume = true;
		_selectedVolume = 0;
	}

	private void MusicMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_isAdjustingVolume = true;
		_selectedVolume = 1;
	}

	private void VoiceMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_isAdjustingVolume = true;
		_selectedVolume = 2;
	}

	private void SFXMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_isAdjustingVolume = true;
		_selectedVolume = 3;
	}

	private void DefaultMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		for (int i = 0; i < 4; i++)
		{
			_globalVolumes[i] = 2f;
			base.ScreenManager.Jukebox.AdjustGlobalVolume(2f, CategoryFromIndex(i));
		}
		SaveVolumes();
	}

	private void SaveVolumes()
	{
		_saveFile.AudioVolumeSFX = _globalVolumes[3] - 1f;
		_saveFile.AudioVolumeVO = _globalVolumes[2] - 1f;
		_saveFile.AudioVolumeMusic = _globalVolumes[1] - 1f;
		_saveFile.AudioVolumeMaster = _globalVolumes[0] - 1f;
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (_isAdjustingVolume && (_selectedVolume == 2 || _selectedVolume == 3))
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			_sfxRepeatTimer += num;
			if (_sfxRepeatTimer >= 0.5f)
			{
				_sfxRepeatTimer = 0f;
				base.ScreenManager.Jukebox.PlayCue((_selectedVolume == 2) ? ESFX.VO_Lun_TimeStop : ESFX.FoleyExplosionLarge);
			}
		}
	}

	public override void ExitScreen()
	{
		if (_initialVolumeMaster != _saveFile.AudioVolumeMaster || _initialVolumeMusic != _saveFile.AudioVolumeMusic || _initialVolumeSFX != _saveFile.AudioVolumeSFX || _initialVolumeVO != _saveFile.AudioVolumeVO)
		{
			base.ScreenManager.SaveFileManager.RequestGameConfigSave();
		}
		base.ExitScreen();
	}

	public override void DrawMisc(SpriteBatch spriteBatch, Color drawColor)
	{
		base.DrawMisc(spriteBatch, drawColor);
		float num = 1f - base.TransitionOffPercentage;
		Rectangle frameSource = base.Sprite.GetFrameSource(56);
		Rectangle frameSource2 = base.Sprite.GetFrameSource(87);
		Vector2 origin = new Vector2(2f, 4f);
		for (int i = 0; i < 4; i++)
		{
			Color color = ((_selectedVolume == i) ? Color.White : Color.Gray) * num;
			Vector2 drawPosition = base.MenuEntries[i].DrawPosition;
			drawPosition = new Vector2((int)(drawPosition.X + (float)_horizontalBarBuffer), (int)drawPosition.Y);
			string text = ((int)Math.Round(_globalVolumes[i] * 50f)).ToString(CultureInfo.InvariantCulture);
			Vector2 vector = drawPosition.Add(_volumeNumberDrawOffset);
			DrawingEx.DrawString(spriteBatch, base.GCM.ActiveFont, text, vector.Add(new Point(0, base.Zoom)), Color.Black * num, Vector2.Zero, base.Zoom);
			DrawingEx.DrawString(spriteBatch, base.GCM.ActiveFont, text, vector, color, Vector2.Zero, base.Zoom);
			Texture2D texture = base.Sprite.Texture;
			spriteBatch.Draw(texture, new Rectangle((int)drawPosition.X, (int)drawPosition.Y - base.Zoom, _horizontalBarWidth, Constants.InGameZoom), frameSource2, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			float num2 = (float)_horizontalBarWidth * (_globalVolumes[i] / 2f);
			drawPosition.Y -= Constants.InGameZoom;
			spriteBatch.Draw(texture, new Vector2(drawPosition.X + num2, drawPosition.Y + (float)_topArrowOffsetY), frameSource, color, -1.5708f, origin, Constants.InGameZoom, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(texture, new Vector2(drawPosition.X + num2, drawPosition.Y + (float)_bottomArrowOffsetY), frameSource, color, -1.5708f, origin, Constants.InGameZoom, SpriteEffects.None, 0f);
		}
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
