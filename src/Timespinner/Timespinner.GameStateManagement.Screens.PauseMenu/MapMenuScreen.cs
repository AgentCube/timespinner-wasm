using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class MapMenuScreen : InventoryMenuScreen
{
	private const int EraWindowWidth = 64;

	private const int EraWindowHeight = 32;

	private const int MaxEraViewWidth = 100;

	private const int MaxEraViewHeight = 46;

	private const int MaxEraViewOffsetX = 36;

	private const int MaxEraViewOffsetY = 14;

	private const int MapMenuMaxEraViewHeight = 42;

	private const int AddChangeMarkerBaseTextWidth = 124;

	private const float MapBackgroundDrawOffsetY = 7f / 64f;

	private const float BumperDrawPositionRatioY = 23f / 192f;

	private const float LeftBumperDrawPositionRatioX = 0.0125f;

	private const float RightBumperDrawPositionRatioX = 0.9375f;

	private const float EraTitleDrawPositionRatioX = 0.5f;

	private const float EraTitleDrawPositionRatioY = 7f / 64f;

	private const int WideButtonOffsetX = 8;

	private const int MarkerButtonMarginX = 4;

	private const float RemoveMarkerButtonDrawPositionRatioX = 7f / 160f;

	private const float AddMarkerButtonDrawPositionRatioX = 29f / 32f;

	private const float MarkerButtonDrawPositionRatioY = 0.865f;

	private const string MapPercentageStringFormat = "{0}%";

	private readonly string _addMarkerText = Loc.Get("map_add_marker");

	private readonly string _toggleMarkerText = Loc.Get("map_toggle_marker");

	private readonly string _removeMarkerText = Loc.Get("map_remove_marker");

	private readonly string _tooManyMarkers = Loc.Get("map_too_many_markers");

	private static readonly Color ShadowColor = new Color(24, 20, 16);

	private readonly ScrollThrottle _scrollThrottle = new ScrollThrottle
	{
		MinScrollRate = 1,
		MaxScrollRate = 10,
		ScrollGrowthRate = 2
	};

	private readonly UIButton _leftBumperButton;

	private readonly UIButton _rightBumperButton;

	private readonly UIButton _removeMarkerButton;

	private readonly UIButton _addMarkerButton;

	private readonly UIButton _changeMarkerButton;

	private readonly Level _level;

	private readonly MinimapSpecification _minimap;

	private readonly int[] _eraMapCompletions = new int[4];

	private readonly List<EMinimapEraType> _availableEras = new List<EMinimapEraType>();

	private bool _didMoveCursor;

	private EMinimapEraType _selectedEra;

	private MinimapMarker.EMinimapMarkerColor _currentMarkerColor;

	private int _bumperDrawPositionY;

	private int _leftBumperDrawPositionX;

	private int _rightBumperDrawPositionX;

	private float _eraCompletionStringWidth;

	private string _eraName = string.Empty;

	private string _eraCompletionString = string.Empty;

	private Point _cursorGlobalLocation;

	private Point _cursorLocalLocation;

	private Point _selectedEraViewOffset;

	private Vector2 _eraTitleDrawPosition;

	private Vector2 _eraTitleBaseDrawPosition;

	private Vector2 _exampleMarkerDrawPosition;

	private Vector2 _addMarkerButtonDrawPosition;

	private Vector2 _addMarkerTextDrawPosition;

	private Vector2 _toggleMarkerButtonDrawPosition;

	private Vector2 _toggleMarkerTextDrawPosition;

	private Vector2 _removeMarkerButtonDrawPosition;

	private Vector2 _removeMarkerTextDrawPosition;

	private Vector2 _eraCompletionBaseDrawPosition;

	private Vector2 _eraCompletionDrawPosition;

	private Rectangle _mapBackgroundDrawRectangle;

	private ScrollableTextBlock _toggleMarkersTextBlock;

	private ScrollableTextBlock _addMarkersTextBlock;

	private HudMinimap _minimapHud;

	public MapMenuScreen(GameSave inSave, GCM gcm, Level inLevel, ControllerMapping controllerMapping, Action fullExitAction)
		: base(Loc.Get("MapMenuTitle"), inSave, gcm, fullExitAction)
	{
		_level = inLevel;
		_minimap = _level.Minimap;
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		_doesUseCursor = false;
		_leftBumperButton = controllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.PageLeft);
		_rightBumperButton = controllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.PageRight);
		_addMarkerButton = controllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.Confirm);
		_removeMarkerButton = controllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.Secondary);
		_changeMarkerButton = controllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.Tertiary);
		_selectedEra = EMinimapEraType.Present;
		HashSet<EMinimapEraType> hashSet = new HashSet<EMinimapEraType>();
		foreach (MinimapArea area in _minimap.Areas)
		{
			EMinimapEraType eraFromMinimapColor = MinimapSpecification.GetEraFromMinimapColor(area.DefaultColor);
			if (!hashSet.Contains(eraFromMinimapColor))
			{
				bool flag = false;
				foreach (MinimapRoom room in area.Rooms)
				{
					foreach (KeyValuePair<Point, MinimapBlock> block in room.Blocks)
					{
						if (block.Value.IsKnown && !room.IsDebug)
						{
							hashSet.Add(eraFromMinimapColor);
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (inLevel.ID == area.LevelID)
			{
				_selectedEra = eraFromMinimapColor;
			}
		}
		_availableEras.AddRange(hashSet);
		int num = 0;
		foreach (EMinimapEraType availableEra in _availableEras)
		{
			_eraMapCompletions[num] = (int)_minimap.GetCompletionPercentageByEra(availableEra);
			num++;
		}
		ToggleEra(0);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		_minimapHud = new HudMinimap(_minimap, base.GCM, Point.Zero, isMenuMap: true);
		RefreshMinimapSizes();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		base.Zoom = Constants.InGameZoom;
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_mapBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 7 * base.Zoom);
		int num2 = 8 * base.Zoom;
		_bumperDrawPositionY = _screenTop + (int)((float)_topSectionHeight * (23f / 192f));
		_leftBumperDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.0125f);
		_rightBumperDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.9375f) - (_rightBumperButton.IsWide ? num2 : 0);
		int num3 = _screenTop + (int)Math.Ceiling((float)_topSectionHeight * 0.865f);
		_removeMarkerButtonDrawPosition = new Vector2((float)_screenLeft + (float)_screenWidth * (7f / 160f), num3 - base.Zoom * 14);
		_addMarkerButtonDrawPosition = new Vector2((float)_screenLeft + (float)_screenWidth * (29f / 32f) - (float)(_addMarkerButton.IsWide ? num2 : 0), num3);
		_toggleMarkerButtonDrawPosition = new Vector2(_removeMarkerButtonDrawPosition.X, num3);
		int num4 = 4 * base.Zoom;
		_removeMarkerTextDrawPosition = _removeMarkerButtonDrawPosition.Add(new Point(16 * base.Zoom + num4 + (_removeMarkerButton.IsWide ? num2 : 0), 0));
		_toggleMarkerTextDrawPosition = _toggleMarkerButtonDrawPosition.Add(new Point(16 * base.Zoom + num4 + (_changeMarkerButton.IsWide ? num2 : 0), 0));
		int num5 = (int)Math.Min(base.Font.MeasureString(_addMarkerText).X, 124f) * base.Zoom;
		int num6 = 12 * base.Zoom;
		_addMarkerTextDrawPosition = _addMarkerButtonDrawPosition.Add(new Point(-(num5 + num4 + num6), 0));
		_exampleMarkerDrawPosition = _addMarkerButtonDrawPosition.Add(new Point(-num6, 0));
		_addMarkersTextBlock = new ScrollableTextBlock(base.Font, 124, _addMarkerTextDrawPosition, isTextCentered: false);
		_toggleMarkersTextBlock = new ScrollableTextBlock(base.Font, 124, _toggleMarkerTextDrawPosition, isTextCentered: false);
		_addMarkersTextBlock.SetText(_addMarkerText);
		_toggleMarkersTextBlock.SetText(_toggleMarkerText);
		_eraCompletionBaseDrawPosition = new Vector2(_addMarkerButtonDrawPosition.X + (float)((_addMarkerButton.IsWide ? 24 : 16) * base.Zoom), _removeMarkerButtonDrawPosition.Y);
		_eraCompletionDrawPosition = new Vector2(_eraCompletionBaseDrawPosition.X - _eraCompletionStringWidth * (float)base.Zoom, _eraCompletionBaseDrawPosition.Y);
		_eraTitleBaseDrawPosition = new Vector2((float)_screenLeft + (float)_screenWidth * 0.5f, (float)_screenTop + (float)_topSectionHeight * (7f / 64f));
		RefreshEraName();
		if (_minimapHud != null)
		{
			RefreshMinimapSizes();
		}
	}

	private void RefreshMinimapSizes()
	{
		Point newDrawPosition = new Point(_mapBackgroundDrawRectangle.Center.X + 128 * base.Zoom, _mapBackgroundDrawRectangle.Top + 16 * base.Zoom);
		_minimapHud.Zoom = base.Zoom;
		_minimapHud.RefreshZoom(newDrawPosition);
		_minimapHud.Update(0f, _level);
		Point cursorPosition = _minimapHud.CursorPosition;
		_cursorLocalLocation = cursorPosition.Add(-_selectedEraViewOffset.X, -_selectedEraViewOffset.Y);
		RefreshCursor();
		_minimapHud.RefreshCameraCenter();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_minimapHud.Update(delta, _level);
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsNewPressPageRight(base.ControllingPlayer))
		{
			ToggleEra(-1);
		}
		else if (input.IsNewPressPageLeft(base.ControllingPlayer))
		{
			ToggleEra(1);
		}
		if (_minimapHud != null)
		{
			if (input.IsNewPressConfirm(base.ControllingPlayer))
			{
				if (_minimap.AddMarker(_cursorGlobalLocation, _currentMarkerColor))
				{
					_minimapHud.UpdateMarkers();
				}
				else
				{
					ChangeDescription(_tooManyMarkers, EInventoryItemIcon.None);
				}
			}
			if (input.IsPressSecondary(base.ControllingPlayer) && (!input.WasPressSecondary(base.ControllingPlayer) || _minimapHud.HasCameraMoved) && _minimap.TryRemoveMarker(_cursorGlobalLocation))
			{
				_minimapHud.UpdateMarkers();
			}
			if (input.IsNewPressTertiary(base.ControllingPlayer))
			{
				_currentMarkerColor = (MinimapMarker.EMinimapMarkerColor)((int)(_currentMarkerColor + 1) % 5);
			}
		}
		bool flag = false;
		if (input.IsPressMenuRight(base.ControllingPlayer))
		{
			flag = true;
			if (!_didMoveCursor || _scrollThrottle.IsScrollReady())
			{
				MoveCursor(EDirection.East);
			}
		}
		else if (input.IsPressMenuLeft(base.ControllingPlayer))
		{
			flag = true;
			if (!_didMoveCursor || _scrollThrottle.IsScrollReady())
			{
				MoveCursor(EDirection.West);
			}
		}
		if (input.IsPressMenuDown(base.ControllingPlayer))
		{
			flag = true;
			if (!_didMoveCursor || _scrollThrottle.IsScrollReady())
			{
				MoveCursor(EDirection.South);
			}
		}
		else if (input.IsPressMenuUp(base.ControllingPlayer))
		{
			flag = true;
			if (!_didMoveCursor || _scrollThrottle.IsScrollReady())
			{
				MoveCursor(EDirection.North);
			}
		}
		if (!flag)
		{
			_scrollThrottle.Reset();
		}
		_didMoveCursor = flag;
		base.HandleInput(input);
	}

	private void MoveCursor(EDirection direction)
	{
		Point pointFromDirection = Level.GetPointFromDirection(_cursorLocalLocation, direction);
		_cursorLocalLocation = new Point((int)MathHelper.Clamp(pointFromDirection.X, 0f, 100f), (int)MathHelper.Clamp(pointFromDirection.Y, 0f, 42f));
		RefreshCursor();
	}

	private void RefreshCursor()
	{
		_cursorGlobalLocation = _cursorLocalLocation.Add(_selectedEraViewOffset);
		Point a = new Point((int)MathHelper.Clamp(_cursorLocalLocation.X, 32f, 68f), (int)MathHelper.Clamp(_cursorLocalLocation.Y, 16f, 30f));
		Point location = a.Add(_selectedEraViewOffset);
		if (_minimapHud != null)
		{
			_minimapHud.MoveRenderCenter(location);
			_minimapHud.MoveCursor(_cursorGlobalLocation);
		}
	}

	private void ToggleEra(int indexOffset)
	{
		int num = _availableEras.IndexOf(_selectedEra);
		num += indexOffset;
		int count = _availableEras.Count;
		if (num < 0)
		{
			num = count - 1;
		}
		if (num >= count)
		{
			num = 0;
		}
		_selectedEra = _availableEras[num];
		int num2 = _eraMapCompletions[num];
		_eraCompletionString = $"{num2}%";
		_eraCompletionStringWidth = base.Font.MeasureString(_eraCompletionString).X;
		_eraCompletionDrawPosition = new Vector2(_eraCompletionBaseDrawPosition.X - _eraCompletionStringWidth * (float)base.Zoom, _eraCompletionBaseDrawPosition.Y);
		RefreshEraName();
		_selectedEraViewOffset = MinimapSpecification.GetViewOffsetFromEra(_selectedEra);
		RefreshCursor();
	}

	private void RefreshEraName()
	{
		_eraName = EraNameFromEra(_selectedEra);
		_eraTitleDrawPosition = Vector2.Add(_eraTitleBaseDrawPosition, new Vector2((int)((0f - base.Font.MeasureString(_eraName).X) * (float)base.Zoom / 2f), 0f));
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
		}, spriteBatch: spriteBatch, backgroundRectangle: _mapBackgroundDrawRectangle, color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 45, 46, 45, 47, 48, 47, 45, 46, 45 }, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
	}

	public override void DrawMisc(SpriteBatch spriteBatch, Color drawColor)
	{
		_minimapHud.Draw(spriteBatch, 1f);
		SpriteSheet uIControllerButtons = base.ScreenManager.UIControllerButtons;
		_leftBumperButton.Draw(spriteBatch, uIControllerButtons, base.Font, new Vector2(_leftBumperDrawPositionX, _bumperDrawPositionY), drawColor, base.Zoom, -1);
		_rightBumperButton.Draw(spriteBatch, uIControllerButtons, base.Font, new Vector2(_rightBumperDrawPositionX, _bumperDrawPositionY), drawColor, base.Zoom, -1);
		_addMarkerButton.Draw(spriteBatch, uIControllerButtons, base.Font, _addMarkerButtonDrawPosition, drawColor, base.Zoom, -1);
		_removeMarkerButton.Draw(spriteBatch, uIControllerButtons, base.Font, _removeMarkerButtonDrawPosition, drawColor, base.Zoom, -1);
		_changeMarkerButton.Draw(spriteBatch, uIControllerButtons, base.Font, _toggleMarkerButtonDrawPosition, drawColor, base.Zoom, -1);
		DrawShadowedString(spriteBatch, base.Font, _removeMarkerText, _removeMarkerTextDrawPosition, drawColor, base.Zoom);
		_addMarkersTextBlock.DrawShadowed(spriteBatch, drawColor, ShadowColor);
		_toggleMarkersTextBlock.DrawShadowed(spriteBatch, drawColor, ShadowColor);
		Rectangle frameSource = base.Sprite.GetFrameSource((int)(_currentMarkerColor + 60));
		spriteBatch.Draw(base.Sprite.Texture, _exampleMarkerDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		DrawShadowedString(spriteBatch, base.Font, _eraName, _eraTitleDrawPosition, drawColor, base.Zoom);
		DrawShadowedString(spriteBatch, base.Font, _eraCompletionString, _eraCompletionDrawPosition, drawColor, base.Zoom);
	}

	private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPosition, Color drawColor, int zoom)
	{
		Color color = ShadowColor * 0.25f;
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				DrawingEx.DrawString(spriteBatch, font, text, drawPosition.Add(new Point(i * zoom, j * zoom)), color, Vector2.Zero, zoom);
			}
		}
		Color color2 = ShadowColor * 0.5f;
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 1; l++)
			{
				DrawingEx.DrawString(spriteBatch, font, text, drawPosition.Add(new Point(k * zoom, l * zoom)), color2, Vector2.Zero, zoom);
			}
		}
		DrawingEx.DrawString(spriteBatch, font, text, drawPosition, drawColor, Vector2.Zero, zoom);
	}

	internal static string EraNameFromEra(EMinimapEraType era)
	{
		string result = string.Empty;
		switch (era)
		{
		case EMinimapEraType.Present:
			result = Loc.Get("map_name_present");
			break;
		case EMinimapEraType.Past:
			result = Loc.Get("map_name_past");
			break;
		case EMinimapEraType.NearPast:
			result = Loc.Get("map_name_nearpast");
			break;
		case EMinimapEraType.Other:
			result = Loc.Get("map_name_other");
			break;
		}
		return result;
	}

	internal void GoToLevel(int levelID)
	{
		MinimapArea minimapArea = null;
		foreach (MinimapArea area in _minimap.Areas)
		{
			if (area.LevelID == levelID)
			{
				minimapArea = area;
				break;
			}
		}
		if (minimapArea == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (MinimapRoom room in minimapArea.Rooms)
		{
			if (!room.IsDebug)
			{
				num += room.Position.X + room.Width / 2;
				num2 += room.Position.Y + room.Height / 2;
				num3++;
			}
		}
		Point cursorLocalLocation = new Point(num / num3, num2 / num3);
		_cursorLocalLocation = cursorLocalLocation;
		RefreshCursor();
	}
}
