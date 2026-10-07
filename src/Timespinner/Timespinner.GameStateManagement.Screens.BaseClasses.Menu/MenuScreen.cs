using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal abstract class MenuScreen : GameScreen
{
	internal enum EScreenFadeType
	{
		FadeOutFadeIn,
		BlackInBlackOut
	}

	internal enum EScreenTransitionMovementType
	{
		None,
		SlideInSlideOut
	}

	private const int CursorOscillateRadius = 2;

	private const int BaseBorderWidth = 76;

	private const int BaseBorderMarginX = 9;

	private const int MaxTitleWidth = 120;

	private const int HalfTitleWidth = 60;

	private const int HalfTitleHeight = 8;

	private const float CursorOscillateFrequency = 5f;

	private const float CursorFollowSpeed = 0.35f;

	private const float CursorAnimationSpeed = 0.16666f;

	private const float TitleScale = 1f;

	internal static readonly Color TitleBaseColor = new Color(240, 240, 208);

	internal static readonly Color TitleShadowColor = new Color(60, 60, 24);

	private readonly string _menuTitle;

	private readonly ScrollThrottle _scrollThrottle = new ScrollThrottle();

	private readonly Stack<MenuEntryCollection> _collectionSelectionHistory = new Stack<MenuEntryCollection>();

	protected readonly List<MenuEntryCollection> _subMenuCollections = new List<MenuEntryCollection>();

	private bool _isCursorAnimatingForward;

	private EGameResolutionType _lastScreenResolution;

	private int _rowHeight;

	private int _totalHeight;

	private int _cursorAnimationIndex;

	private int _zoom = Constants.InGameZoom;

	private float _cursorAnimationCounter;

	private float _alpha;

	private Point _baseCursorOffset;

	private Vector2 _cursorPosition;

	private Vector2 _titleOrigin;

	private Vector2 _startPosition;

	private Vector2 _position;

	private Vector2 _borderPosition;

	private Rectangle _titleSafeArea;

	private Color _titleColor;

	private SpriteFont _font;

	private ScrollableTextBlock _titleTextBlock;

	private bool _wasScrollingHorizontally;

	private bool _wasScrollingVertically;

	protected bool _doesUseBlackGradientBox = true;

	protected bool _doesUseCursor = true;

	protected bool _isTitlePositionOverridden;

	protected EScreenFadeType _screenFadeType;

	protected EScreenTransitionMovementType _screenTransitionType = EScreenTransitionMovementType.SlideInSlideOut;

	protected int _descriptionWidth = 800;

	protected Vector2 _titlePosition;

	protected Rectangle _cursorFrameSource;

	protected MenuEntryCollection _selectedMenuCollection;

	protected MenuEntryCollection _primaryMenuCollection = new MenuEntryCollection
	{
		IsVisible = true
	};

	internal bool DoesDrawTitle { get; set; }

	internal bool DoesOverridePrimaryMenuPosition { get; set; }

	internal bool DoesTitleDrawLargeShadow { get; set; }

	internal bool IsDescriptionCentered { get; set; }

	internal bool IsMenuDisabled { get; set; }

	internal int SelectedIndex => _selectedMenuCollection.SelectedIndex;

	internal int BorderMarginX { get; set; }

	internal int BorderWidth { get; set; }

	internal float CursorOscillation { get; private set; }

	internal float TransitionReverse { get; private set; }

	internal Point MenuOffset { get; set; }

	internal Point CursorOffset { get; set; }

	internal Vector2 CursorPosition => _cursorPosition;

	internal Vector2 TitleOffset { get; set; }

	internal Vector2 DescriptionDrawPosition { get; set; }

	internal Rectangle TitleSafeArea => _titleSafeArea;

	internal MenuDescription CurrentDescription { get; set; }

	internal ScrollThrottle ScrollThrottle => _scrollThrottle;

	internal SpriteFont DescriptionFont { get; set; }

	internal IList<MenuEntry> MenuEntries => _primaryMenuCollection.Entries;

	protected MenuScreen(string menuTitle)
	{
		_menuTitle = menuTitle;
		DoesDrawTitle = !string.IsNullOrEmpty(menuTitle);
		_selectedMenuCollection = _primaryMenuCollection;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		_font = base.ScreenManager.MenuFont;
		_lastScreenResolution = base.ScreenManager.CurrentResolution;
		Vector2 vector = _font.MeasureString(_menuTitle) / 2f;
		_titleOrigin = new Vector2((int)vector.X, (int)vector.Y);
		DescriptionFont = _font;
		_primaryMenuCollection.Font = _font;
		RefreshSizes();
		OnSelectedEntryChanged(0);
	}

	internal override void OnScreenResize()
	{
		_zoom = Constants.InGameZoom;
		_lastScreenResolution = base.ScreenManager.CurrentResolution;
		RefreshSizes();
		if (_selectedMenuCollection.Font != null)
		{
			_rowHeight = _selectedMenuCollection.EntryHeight;
		}
		UpdateMenuPosition();
		UpdateMenuCollections(0f, base.IsActive);
		_cursorPosition = GetTargetCursorPosition();
		UpdateMenuCursor(0f);
	}

	internal virtual void RefreshSizes()
	{
		_titleSafeArea = base.ScreenManager.TitleSafeArea;
		TitleOffset = new Vector2(12 * _zoom, -(_zoom * (base.ScreenManager.MenuFont.LineSpacing + 10)));
		_baseCursorOffset = new Point(-18 * _zoom, -5 * _zoom);
		BorderMarginX = 9 * _zoom;
		BorderWidth = 76 * _zoom;
		if (_primaryMenuCollection != null && _primaryMenuCollection.Font != null)
		{
			_primaryMenuCollection.GetMenuDimensions();
		}
		foreach (MenuEntryCollection subMenuCollection in _subMenuCollections)
		{
			if (subMenuCollection != null && subMenuCollection.Font != null)
			{
				subMenuCollection.GetMenuDimensions();
			}
		}
	}

	public override void HandleInput(InputState input)
	{
		if (!_wasScrollingHorizontally && !_wasScrollingVertically)
		{
			ScrollThrottle.Reset();
		}
		if (IsMenuDisabled)
		{
			return;
		}
		bool flag = false;
		bool flag2 = input.IsPressMenuRight(base.ControllingPlayer);
		bool flag3 = input.IsPressMenuLeft(base.ControllingPlayer);
		bool flag4 = input.IsPressMenuUp(base.ControllingPlayer);
		bool flag5 = input.IsPressMenuDown(base.ControllingPlayer);
		if (_wasScrollingVertically && (flag4 || flag5))
		{
			if (ScrollThrottle.IsScrollReady())
			{
				flag = MoveSelectedInDirection((!flag5) ? EMenuMoveDirection.Up : EMenuMoveDirection.Down, base.ControllingPlayer);
			}
		}
		else if (_wasScrollingHorizontally && (flag3 || flag2))
		{
			if (ScrollThrottle.IsScrollReady())
			{
				flag = MoveSelectedInDirection(flag2 ? EMenuMoveDirection.Right : EMenuMoveDirection.Left, base.ControllingPlayer);
			}
		}
		else if (input.IsNewPressMenuUp(base.ControllingPlayer))
		{
			flag = MoveSelectedInDirection(EMenuMoveDirection.Up, base.ControllingPlayer);
			_wasScrollingVertically = true;
			_wasScrollingHorizontally = false;
		}
		else if (input.IsNewPressMenuDown(base.ControllingPlayer))
		{
			flag = MoveSelectedInDirection(EMenuMoveDirection.Down, base.ControllingPlayer);
			_wasScrollingVertically = true;
			_wasScrollingHorizontally = false;
		}
		else if (flag3 || flag2)
		{
			flag = MoveSelectedInDirection(flag2 ? EMenuMoveDirection.Right : EMenuMoveDirection.Left, base.ControllingPlayer);
			_wasScrollingVertically = false;
			_wasScrollingHorizontally = true;
		}
		else
		{
			_wasScrollingVertically = false;
			_wasScrollingHorizontally = false;
		}
		if (input.IsNewPressConfirm(base.ControllingPlayer, out var pressedPlayer))
		{
			Console.WriteLine($"[MenuScreen] Confirm pressed on {GetType().Name}, SelectedIndex: {_selectedMenuCollection.SelectedIndex}");
			if (_selectedMenuCollection.SelectEntry(pressedPlayer))
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
			}
		}
		else if (input.IsNewPressCancel(base.ControllingPlayer, out pressedPlayer))
		{
			OnCancel(pressedPlayer);
		}
		if (flag)
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuMove);
			OnSelectedEntryChanged(_selectedMenuCollection.SelectedIndex);
		}
	}

	internal virtual bool MoveSelectedInDirection(EMenuMoveDirection direction, PlayerIndex? playerIndex)
	{
		return _selectedMenuCollection.MoveSelection(direction, playerIndex);
	}

	internal void SetMenuSelectedIndex(int index)
	{
		if (_selectedMenuCollection.SetSelectedIndex(index))
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuMove);
			OnSelectedEntryChanged(_selectedMenuCollection.SelectedIndex);
		}
	}

	protected virtual void OnSelectedEntryChanged(int entryIndex)
	{
		int count = _selectedMenuCollection.Entries.Count;
		if (count > 0 && count > entryIndex)
		{
			EInventoryItemIcon selectedIcon = _selectedMenuCollection.GetSelectedIcon();
			ChangeDescription(_selectedMenuCollection.Entries[entryIndex].Description, selectedIcon);
		}
	}

	internal virtual void ChangeDescription(string description, EInventoryItemIcon icon)
	{
		CurrentDescription = new MenuDescription(description, DescriptionFont, icon, null, base.ScreenManager.UIControllerButtons, IsDescriptionCentered, base.ScreenManager.MenuControllerMapping);
	}

	protected virtual void ChangeMenuCollection(MenuEntryCollection newCollection, bool shouldPush)
	{
		if (shouldPush)
		{
			_collectionSelectionHistory.Push(_selectedMenuCollection);
		}
		_selectedMenuCollection = newCollection;
		OnSelectedEntryChanged(_selectedMenuCollection.SelectedIndex);
	}

	protected virtual void GoToPreviousMenuCollection()
	{
		ChangeMenuCollection(_collectionSelectionHistory.Pop(), shouldPush: false);
	}

	protected virtual void OnCancel(PlayerIndex playerIndex)
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
		if (_selectedMenuCollection != _primaryMenuCollection && _collectionSelectionHistory.Count > 0)
		{
			GoToPreviousMenuCollection();
		}
		else
		{
			ExitScreen();
		}
	}

	protected void OnCancel(object sender, PlayerIndexEventArgs e)
	{
		OnCancel(e.PlayerIndex);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!otherScreenHasFocus)
		{
			float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (_zoom != Constants.InGameZoom || _lastScreenResolution != base.ScreenManager.CurrentResolution)
			{
				_zoom = Constants.InGameZoom;
				_lastScreenResolution = base.ScreenManager.CurrentResolution;
				RefreshSizes();
			}
			if (_selectedMenuCollection.Font != null)
			{
				_rowHeight = _selectedMenuCollection.EntryHeight;
			}
			UpdateMenuPosition();
			UpdateMenuCollections(delta, base.IsActive);
			UpdateMenuCursor(delta);
		}
	}

	private void UpdateMenuCollections(float delta, bool isActive)
	{
		_primaryMenuCollection.Update(delta, isActive, base.TransitionOffPercentage);
		foreach (MenuEntryCollection subMenuCollection in _subMenuCollections)
		{
			bool isScreenActive = _selectedMenuCollection == subMenuCollection;
			subMenuCollection.Update(delta, isScreenActive, base.TransitionOffPercentage);
		}
	}

	private void UpdateMenuCursor(float delta)
	{
		CursorOscillation += delta * 5f;
		if (CursorOscillation >= 6.14f)
		{
			CursorOscillation -= 6.14f;
		}
		_cursorAnimationCounter += delta;
		if (_cursorAnimationCounter >= 0.16666f || _cursorFrameSource == Rectangle.Empty)
		{
			_cursorAnimationCounter -= 0.16666f;
			_cursorAnimationIndex += (_isCursorAnimatingForward ? 1 : (-1));
			if (_cursorAnimationIndex >= 4)
			{
				_cursorAnimationIndex = 3;
				_isCursorAnimatingForward = !_isCursorAnimatingForward;
			}
			else if (_cursorAnimationIndex < 0)
			{
				_cursorAnimationIndex = 0;
				_isCursorAnimatingForward = !_isCursorAnimatingForward;
			}
			_cursorFrameSource = base.ScreenManager.UIMenuArrow.GetFrameSource(_cursorAnimationIndex);
		}
		Vector2 targetCursorPosition = GetTargetCursorPosition();
		if (_cursorPosition == Vector2.Zero)
		{
			_cursorPosition = targetCursorPosition;
		}
		else if (_cursorPosition != targetCursorPosition)
		{
			_cursorPosition = MathEx.PercentageFollowPoint(targetCursorPosition.ToPoint(), _cursorPosition, 0.35f, 4, delta);
		}
	}

	private Vector2 GetTargetCursorPosition()
	{
		int num = (int)(Math.Sin(CursorOscillation) * 2.0 * (double)_zoom);
		Point point = CursorOffset.Add(_baseCursorOffset);
		return Vector2.Add(_selectedMenuCollection.GetCursorPosition(), new Vector2(point.X + num, point.Y));
	}

	private void UpdateMenuPosition()
	{
		_totalHeight = 0;
		int num = _titleSafeArea.Center.X + MenuOffset.X;
		int num2 = _titleSafeArea.Center.Y;
		int count = MenuEntries.Count;
		if (count > 0)
		{
			_totalHeight = _rowHeight * count;
			num2 = _titleSafeArea.Center.Y - (count - 1) / 2 * _rowHeight + MenuOffset.Y;
		}
		_startPosition = new Vector2(num, num2);
		_position = new Vector2(_startPosition.X, _startPosition.Y);
		_borderPosition = new Vector2(_startPosition.X, _startPosition.Y);
		TransitionReverse = (float)Math.Pow(base.TransitionOffPercentage, 2.0);
		_alpha = 1f - TransitionReverse;
		_titleColor = TitleBaseColor * _alpha;
		if (!_isTitlePositionOverridden)
		{
			_titlePosition = Vector2.Add(new Vector2(_titleSafeArea.Center.X, (int)_startPosition.Y), TitleOffset);
		}
		if (_screenTransitionType == EScreenTransitionMovementType.SlideInSlideOut)
		{
			int num3 = ((base.ScreenState == EScreenState.TransitionOn) ? 1 : (-2));
			_borderPosition.X += TransitionReverse * ((float)BorderWidth * 0.5f) * (float)num3;
		}
		if (!DoesOverridePrimaryMenuPosition)
		{
			_primaryMenuCollection.DrawPosition = _position;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		if (base.ScreenState != EScreenState.TransitionOff || _screenTransitionType == EScreenTransitionMovementType.None)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
			DrawBackgroundAndGoldBars(spriteBatch);
			DrawMenus(spriteBatch);
			DrawCursor(spriteBatch);
			DrawTitle(spriteBatch);
			DrawDescription(spriteBatch);
			spriteBatch.End();
		}
	}

	protected virtual void DrawBackgroundAndGoldBars(SpriteBatch spriteBatch)
	{
		if (_doesUseBlackGradientBox)
		{
			int num = (int)((float)BorderWidth * (1f - TransitionReverse));
			Color color = Color.White * (1f - base.TransitionOffPercentage);
			if (_doesUseBlackGradientBox)
			{
				Rectangle destinationRectangle = new Rectangle((int)_borderPosition.X - 16, (int)_borderPosition.Y - _rowHeight / 2, num / 2, _totalHeight);
				spriteBatch.Draw(base.ScreenManager.UIBlackGradientBox, destinationRectangle, null, color, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
				destinationRectangle.Location = destinationRectangle.Location.Add(num / 2, 0);
				spriteBatch.Draw(base.ScreenManager.UIBlackGradientBox, destinationRectangle, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			}
		}
	}

	protected virtual void DrawCursor(SpriteBatch spriteBatch)
	{
		if (_doesUseCursor)
		{
			spriteBatch.Draw(base.ScreenManager.UIMenuArrow.Texture, _cursorPosition, _cursorFrameSource, Color.White * (1f - base.TransitionOffPercentage), 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		}
	}

	protected virtual void DrawMenus(SpriteBatch spriteBatch)
	{
		_primaryMenuCollection.Draw(spriteBatch, _zoom);
		foreach (MenuEntryCollection subMenuCollection in _subMenuCollections)
		{
			subMenuCollection.Draw(spriteBatch, _zoom);
		}
	}

	protected void DrawTitle(SpriteBatch spriteBatch)
	{
		if (!DoesDrawTitle || !(_titlePosition != Vector2.Zero))
		{
			return;
		}
		Vector2 vector = new Vector2((int)_titlePosition.X, (int)_titlePosition.Y);
		if (DoesTitleDrawLargeShadow)
		{
			DrawingEx.DrawLargeTextShadow(spriteBatch, _font, _menuTitle, vector, _zoom, _titleOrigin, _alpha);
			DrawingEx.DrawString(spriteBatch, _font, _menuTitle, Vector2.Add(vector, new Vector2(0f, _zoom)), TitleShadowColor, _titleOrigin, 1f * (float)_zoom);
			DrawingEx.DrawString(spriteBatch, _font, _menuTitle, vector, _titleColor, _titleOrigin, 1f * (float)_zoom);
			return;
		}
		Vector2 vector2 = new Vector2(vector.X - (float)(60 * _zoom), vector.Y - (float)(8 * _zoom));
		if (_titleTextBlock == null)
		{
			_titleTextBlock = new ScrollableTextBlock(_font, 120, vector2, isTextCentered: true);
			_titleTextBlock.SetText(_menuTitle);
		}
		else if (_titleTextBlock.TopLeft != vector2)
		{
			_titleTextBlock.SetTopLeft(vector2);
		}
		_titleTextBlock.Draw(spriteBatch, _titleColor, TitleShadowColor, base.ScreenManager.GCM.TxBlankSquare);
	}

	protected virtual void DrawDescription(SpriteBatch spriteBatch)
	{
		if (CurrentDescription != null)
		{
			CurrentDescription.Draw(spriteBatch, DescriptionDrawPosition, _zoom);
		}
	}

	protected void PlayErrorSound()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuError);
	}
}
