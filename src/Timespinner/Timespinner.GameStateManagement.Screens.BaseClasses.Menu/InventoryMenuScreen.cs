using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.StatusEffects;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal abstract class InventoryMenuScreen : MenuScreen
{
	public const float TransitionToBlackTime = 0.05f;

	public const float TransitionToMenuTime = 0.1f;

	public const float TotalTransitionTime = 0.15f;

	public const float MenuShowThresholdPercentage = 1f / 3f;

	public const float ScreenBaseWidth = 320f;

	public const float ScreenBaseHeight = 240f;

	public const float WidthDivider = 320f;

	public const float HeightDivider = 192f;

	private const float VerticalRatio = 51f / 64f;

	private const float TitlePositionRatioY = 11f / 192f;

	private const float PrimaryMenuPositionRatioX = 3f / 32f;

	private const float PrimaryMenuWidePositionRatioX = 11f / 160f;

	private const float PrimaryMenuPositionRatioY = 5f / 24f;

	internal const float StatsBorderRatioX = 0.5f;

	internal const float StatsLargeBorderRatioX = 0.009375f;

	internal const float StatsBorderRatioY = 23f / 192f;

	internal const float StatsWidthRatio = 0.490625f;

	internal const float StatsLargeWidthRatio = 157f / 160f;

	internal const float StatsHeightRatio = 27f / 64f;

	private const int NarrowListBaseColumnWidth = 112;

	private const float ListBorderRatioY = 13f / 24f;

	private const float ListColumnOffsetX = 4f;

	private const float ListWideColumnOffsetX = 6.66f;

	private const float ListBackgroundOffsetY = 7f / 12f;

	private const float ListTextPositionXRatio = 3f / 32f;

	private const float ListTextPositionYRatio = 2f / 3f;

	private const float DescriptionPositionXRatio = 0.075f;

	private const float DescriptionPositionYRatio = 5f / 6f;

	private const float HealthBarOffsetXMultiplier = 3f;

	private const float HealthBarOffsetYMultiplier = 1f;

	private const float ManaBarOffsetY = 8f;

	private const float ScrollbarPositionRatioX = 31f / 32f;

	private const float ScrollbarPositionRatioY = 29f / 48f;

	private const float ScrollbarHeightRatio = 25f / 96f;

	private readonly ScrollbarWidget _scrollbarWidget;

	private readonly GameSave _saveFile;

	private readonly GCM _gcm;

	private readonly SpriteSheet _sprite;

	private readonly SpriteFont _font;

	private readonly List<StatCollection> _statCollections = new List<StatCollection>();

	private readonly Action _fullExitAction;

	protected int _topSectionHeight;

	protected int _bottomSectionHeight;

	protected int _screenWidth;

	protected int _screenLeft;

	protected int _screenHeight;

	protected int _screenTop;

	protected EStatusEffectType _playerStatus;

	protected int _playerHealth;

	protected int _playerMaxHealth;

	protected int _playerSand;

	protected int _playerMaxSand;

	protected int _playerAura;

	protected int _playerMaxAura;

	private bool _isNotInFocus;

	private float _healthBarOffsetX;

	private float _healthBarOffsetY;

	private float _manaBarOffsetY;

	protected Vector2 _listBorderDrawPosition;

	private Vector2 _healthBarDrawPosition;

	private Vector2 _hpBarDrawPosition;

	private Vector2 _mpBarDrawPosition;

	protected Rectangle _listBackgroundDrawRectangle;

	private Rectangle _statsBorderDrawRectangle;

	private Rectangle _statsBracketDrawRectangle;

	protected int[] _topLowerFrameIndices = new int[9] { 37, 38, 37, 39, 40, 39, 37, 38, 37 };

	public bool DoesDrawBrackets { get; set; }

	public bool DoesDrawBracketsOverAll { get; set; }

	public bool DoesHaveWideColumns { get; set; }

	public bool DoesDrawHealthbar { get; set; }

	public bool DoesDrawTopLowerFrame { get; set; }

	internal bool DoesDrawScrollbarWidget { get; set; }

	internal int Zoom { get; set; }

	internal int ScrollBarHeight { get; set; }

	internal Vector2 ScrollBarDrawPosition { get; set; }

	public int ListColumnWidth { get; private set; }

	internal int NarrowListColumnWidth { get; private set; }

	public Vector2 ListTextDrawPosition { get; set; }

	public Vector2 HealthBarDrawPosition
	{
		get
		{
			return _healthBarDrawPosition;
		}
		set
		{
			_healthBarDrawPosition = value;
			RefreshHealthBarPosition();
		}
	}

	internal ScrollbarWidget ScrollbarWidget => _scrollbarWidget;

	internal ControllerMapping DescriptionControllerMapping { get; set; }

	public GameSave SaveFile => _saveFile;

	public GCM GCM => _gcm;

	public SpriteSheet Sprite => _sprite;

	public SpriteFont Font => _font;

	public List<StatCollection> StatCollections => _statCollections;

	protected InventoryMenuScreen(string title, GameSave inSave, GCM gcm, Action fullExitAction)
		: base(title)
	{
		Zoom = Constants.InGameZoom;
		_saveFile = inSave;
		_gcm = gcm;
		_sprite = _gcm.SpPauseMenu;
		_font = _gcm.ActiveFont;
		_fullExitAction = fullExitAction;
		_scrollbarWidget = new ScrollbarWidget();
		_screenTransitionType = EScreenTransitionMovementType.None;
		_screenFadeType = EScreenFadeType.BlackInBlackOut;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.15000000223517418);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.15000000223517418);
		base.IsPopupScreen = false;
		base.DoesLeaveIfNotInFocus = false;
		base.DoesOverridePrimaryMenuPosition = true;
		_doesUseBlackGradientBox = false;
		_doesUseCursor = true;
	}

	public override void LoadContent()
	{
		DescriptionControllerMapping = base.ScreenManager.SaveFileManager.ConfigSave.PlayerControllerMapping;
		base.LoadContent();
		RefreshSizes();
		_primaryMenuCollection.Font = Font;
		foreach (MenuEntryCollection subMenuCollection in _subMenuCollections)
		{
			subMenuCollection.Font = Font;
		}
		base.DescriptionFont = Font;
		OnSelectedEntryChanged(0);
	}

	public override void HandleInput(InputState input)
	{
		bool flag = true;
		if (_fullExitAction != null && input.IsNewPressExit(base.ControllingPlayer))
		{
			flag = false;
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
			_fullExitAction();
		}
		if (flag)
		{
			base.HandleInput(input);
		}
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		Zoom = Constants.InGameZoom;
		_healthBarOffsetX = 3f * (float)Zoom;
		_healthBarOffsetY = 1f * (float)Zoom;
		_manaBarOffsetY = 8f * (float)Zoom;
		_screenWidth = (int)Math.Min(base.ScreenManager.ViewPortArea.Width, 320f * (float)Zoom);
		_screenLeft = (base.ScreenManager.ViewPortArea.Width - _screenWidth) / 2;
		_screenHeight = (int)Math.Min(base.ScreenManager.ViewPortArea.Height, 240f * (float)Zoom);
		_screenTop = (base.ScreenManager.ViewPortArea.Height - _screenHeight) / 2;
		_topSectionHeight = (int)((float)_screenHeight * (51f / 64f));
		_bottomSectionHeight = _screenHeight - _topSectionHeight;
		_isTitlePositionOverridden = true;
		_titlePosition = new Vector2((float)_screenLeft + (float)_screenWidth / 2f, (float)_screenTop + 11f / 192f * (float)_topSectionHeight);
		float num = (DoesHaveWideColumns ? (11f / 160f) : (3f / 32f));
		_primaryMenuCollection.DrawPosition = new Vector2(num * (float)_screenWidth + (float)_screenLeft, 5f / 24f * (float)_topSectionHeight + (float)_screenTop);
		if (DoesDrawBracketsOverAll)
		{
			_statsBorderDrawRectangle = new Rectangle(_screenLeft + (int)(0.009375f * (float)_screenWidth), _screenTop + (int)(23f / 192f * (float)_topSectionHeight), (int)(157f / 160f * (float)_screenWidth), (int)(27f / 64f * (float)_topSectionHeight));
			_statsBracketDrawRectangle = _statsBorderDrawRectangle;
		}
		else
		{
			_statsBorderDrawRectangle = new Rectangle(_screenLeft + (int)(0.5f * (float)_screenWidth), _screenTop + (int)(23f / 192f * (float)_topSectionHeight), (int)(0.490625f * (float)_screenWidth), (int)(27f / 64f * (float)_topSectionHeight));
			_statsBracketDrawRectangle = new Rectangle(_statsBorderDrawRectangle.Left + Zoom, _statsBorderDrawRectangle.Top, _statsBorderDrawRectangle.Width, _statsBorderDrawRectangle.Height);
		}
		_listBorderDrawPosition = new Vector2(_screenLeft, (int)(13f / 24f * (float)_topSectionHeight));
		_listBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * Zoom, _screenTop + (int)(7f / 12f * (float)_topSectionHeight), _screenWidth - 11 * Zoom, (int)((float)_topSectionHeight - _listBorderDrawPosition.Y - (float)(16 * Zoom)));
		float num2 = 0.075f * (float)_screenWidth;
		base.DescriptionDrawPosition = new Vector2(_screenLeft + (int)num2, _screenTop + (int)(5f / 6f * (float)(_topSectionHeight + _bottomSectionHeight)));
		float num3 = (DoesHaveWideColumns ? 6.66f : 4f) * (float)Zoom;
		float num4 = (DoesHaveWideColumns ? (11f / 160f * (float)_screenWidth) : num2);
		ListColumnWidth = (int)((float)_screenWidth / 2f - num4 + num3);
		ListTextDrawPosition = new Vector2((int)(3f / 32f * (float)_screenWidth + (float)_screenLeft), (int)(2f / 3f * (float)_topSectionHeight) + _screenTop);
		NarrowListColumnWidth = 112 * Zoom;
		ScrollBarDrawPosition = new Vector2((int)(31f / 32f * (float)_screenWidth + (float)_screenLeft), (int)(29f / 48f * (float)_topSectionHeight) + _screenTop);
		ScrollBarHeight = (int)(25f / 96f * (float)_topSectionHeight);
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		int num = 0;
		foreach (MenuEntryCollection subMenuCollection in _subMenuCollections)
		{
			subMenuCollection.IsVisible = _selectedMenuCollection == subMenuCollection || _primaryMenuCollection.SelectedIndex == num;
			num++;
		}
		if (DoesDrawScrollbarWidget)
		{
			if (_primaryMenuCollection != _selectedMenuCollection)
			{
				_selectedMenuCollection.RefreshScrollWindow();
			}
			_scrollbarWidget.Percentage = _selectedMenuCollection.ScrollPercentage;
		}
	}

	internal override void ChangeDescription(string description, EInventoryItemIcon icon)
	{
		base.CurrentDescription = new MenuDescription(description, base.DescriptionFont, icon, GCM.SpMenuIcons, GCM.SpUIButtons, base.IsDescriptionCentered, DescriptionControllerMapping);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		_isNotInFocus = otherScreenHasFocus;
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		bool flag = 1f - base.TransitionOffPercentage >= 1f / 3f;
		bool flag2 = flag;
		if (!_isNotInFocus && (base.ScreenState != 0 || _isFirstTransitionOn))
		{
			int alpha = (int)Math.Min(255f * ((1f - base.TransitionOffPercentage) * 2f), 255f);
			base.ScreenManager.FadeBackBufferToBlack(alpha);
		}
		else
		{
			flag = true;
			flag2 = false;
			base.ScreenManager.FadeBackBufferToBlack(255);
		}
		if (!flag)
		{
			return;
		}
		Color white = Color.White;
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		DrawFrames(spriteBatch, white);
		DrawCharacterFrames(spriteBatch, white);
		DrawStats(spriteBatch, white);
		DrawMisc(spriteBatch, white);
		if (DoesDrawHealthbar)
		{
			DrawHealthBar(spriteBatch, white);
		}
		spriteBatch.End();
		base.Draw(gameTime);
		if (flag2)
		{
			int num = (int)MathHelper.Clamp(255f * (base.TransitionOffPercentage + 1f / 3f), 0f, 255f);
			if (base.ScreenState == EScreenState.TransitionOn || (base.ScreenState == EScreenState.TransitionOff && num != 255))
			{
				base.ScreenManager.FadeBackBufferToBlack(num);
			}
		}
		else
		{
			int alpha2 = (int)MathHelper.Clamp(255f * (base.TransitionOffPercentage + 1f / 3f), 0f, 255f);
			base.ScreenManager.FadeBackBufferToBlack(alpha2);
		}
	}

	public virtual void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteEffects[] array = new SpriteEffects[9];
		if (DoesDrawTopLowerFrame)
		{
			array[2] = SpriteEffects.FlipHorizontally;
			array[5] = SpriteEffects.FlipHorizontally;
			array[6] = SpriteEffects.FlipVertically;
			array[7] = SpriteEffects.FlipVertically;
			array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
			DrawingEx.DrawIrregularBox(spriteBatch, _listBackgroundDrawRectangle, drawColor, Sprite, Zoom, _topLowerFrameIndices, array, shouldTile: true);
		}
		DrawHeader(spriteBatch, drawColor);
		if (DoesDrawBrackets)
		{
			array[2] = SpriteEffects.FlipHorizontally;
			array[6] = SpriteEffects.FlipVertically;
			array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
			DrawingEx.DrawIrregularBox(spriteBatch, _statsBracketDrawRectangle, drawColor, Sprite, Zoom, new int[9] { 34, -1, 34, -1, -1, -1, 34, -1, 34 }, array);
		}
		if (DoesDrawTopLowerFrame)
		{
			array[2] = SpriteEffects.FlipHorizontally;
			DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(_screenLeft, _screenTop + (int)_listBorderDrawPosition.Y, _screenWidth, 24), drawColor, Sprite, Zoom, new int[9] { 35, 36, 35, -1, -1, -1, -1, -1, -1 }, array, shouldTile: true);
		}
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[7] = SpriteEffects.None;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(_screenLeft, _screenTop + _topSectionHeight, _screenWidth, _bottomSectionHeight), drawColor, Sprite, Zoom, new int[9] { 27, 20, 27, -1, 0, -1, 28, 23, 28 }, array);
		if (base.CurrentDescription != null && base.CurrentDescription.HasIcon)
		{
			Rectangle frameSource = _sprite.GetFrameSource(110);
			spriteBatch.Draw(_sprite.Texture, new Vector2(_screenLeft, _screenTop + _topSectionHeight), frameSource, drawColor, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
		}
		if (DoesDrawScrollbarWidget)
		{
			_scrollbarWidget.Draw(spriteBatch, ScrollBarDrawPosition, Sprite, GCM.EfBrighten, drawColor, Zoom, ScrollBarHeight);
		}
	}

	public virtual void DrawHeader(SpriteBatch spriteBatch, Color drawColor)
	{
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
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
		}, spriteBatch: spriteBatch, backgroundRectangle: new Rectangle(_screenLeft, _screenTop, _screenWidth, _topSectionHeight), color: drawColor, sprite: Sprite, zoom: Zoom, frames: new int[9] { 32, 33, 32, 25, -1, 26, 35, 36, 35 }, shouldTile: true);
	}

	public virtual void DrawStats(SpriteBatch spriteBatch, Color drawColor)
	{
		foreach (StatCollection statCollection in StatCollections)
		{
			statCollection.Draw(spriteBatch, _gcm, 1f, Zoom);
		}
	}

	public virtual void DrawCharacterFrames(SpriteBatch spriteBatch, Color drawColor)
	{
	}

	private void DrawHealthBar(SpriteBatch spriteBatch, Color drawColor)
	{
		Rectangle frameSource = _sprite.GetFrameSource(5);
		spriteBatch.Draw(_sprite.Texture, _hpBarDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(_sprite.Texture, _mpBarDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
		frameSource = _sprite.GetFrameSource(2);
		float num = (float)_playerHealth / (float)_playerMaxHealth;
		spriteBatch.Draw(_sprite.Texture, _hpBarDrawPosition, new Rectangle(frameSource.Left, frameSource.Top, (int)Math.Floor((float)frameSource.Width * num), frameSource.Height), drawColor, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
		frameSource = _sprite.GetFrameSource(3);
		float num2 = (float)_playerAura / (float)_playerMaxAura;
		spriteBatch.Draw(_sprite.Texture, _mpBarDrawPosition, new Rectangle(frameSource.Left, frameSource.Top, (int)Math.Floor((float)frameSource.Width * num2), frameSource.Height), drawColor, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
		frameSource = _sprite.GetFrameSource(6);
		spriteBatch.Draw(_sprite.Texture, HealthBarDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, Zoom, SpriteEffects.None, 0f);
	}

	public virtual void DrawMisc(SpriteBatch spriteBatch, Color drawColor)
	{
	}

	private void RefreshHealthBarPosition()
	{
		_hpBarDrawPosition = new Vector2(HealthBarDrawPosition.X + _healthBarOffsetX, HealthBarDrawPosition.Y + _healthBarOffsetY);
		_mpBarDrawPosition = new Vector2(_hpBarDrawPosition.X, _hpBarDrawPosition.Y + _manaBarOffsetY);
	}
}
