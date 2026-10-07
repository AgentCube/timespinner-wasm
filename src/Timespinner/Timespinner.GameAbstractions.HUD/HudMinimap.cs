using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameAbstractions.HUD;

internal class HudMinimap : HudElement
{
	private const int SmallViewBlockWidth = 12;

	private const int SmallViewBlockHeight = 6;

	private const int TileSize = 16;

	private const int MapCursorVisibilityThresholdX = 32;

	private const int MapCursorVisibilityThresholdY = 16;

	public const int MediumViewBlockWidth = 64;

	public const int MediumViewBlockHeight = 32;

	private const int LargeViewBlockWidth = 100;

	private const int LargeViewBlockHeight = 46;

	private const int ViewMarginRight = 0;

	private const int ViewMarginBottom = 0;

	private const int CursorAnimationFrameCount = 3;

	private const float PlayerLocationMinScale = 0.02f;

	private const float PlayerLocationMaxScale = 0.13f;

	private const float PlayerLocationGrowthRate = 0.15f;

	private const float CursorAnimationSpeed = 0.15f;

	private int _blockMultiplier;

	private int _smallViewWidth;

	private int _smallViewHeight;

	private int _mediumViewWidth;

	private int _mediumViewHeight;

	private int _largeViewWidth;

	private int _largeViewHeight;

	private readonly bool _doesDrawBoundingBox;

	private readonly bool _isMenuMap;

	private readonly Color _playerLocationColor = Color.White * 0.3f;

	private readonly MinimapSpecification _minimap;

	private readonly SpriteSheet _minimapSprite;

	private readonly SpriteSheet _playerLocationSprite;

	private bool _isMapMenu;

	private bool _isPlayerLocationVisible;

	private bool _isPlayerInDebugRoom;

	private bool _isPlayerInBossRoom;

	private EMinimapToggleState _minimapToggleState;

	private EMinimapRoomColor _currentEraColor;

	private int _lastLevelIndex;

	private int _lastRoomID;

	private int _cursorAnimationIndex;

	private float _basePlayerLocationScale;

	private float _playerLocationScale;

	private float _cursorAnimationCounter;

	private float _markerColorTimer;

	private Point _roomDimensions;

	private Point _cursorBlockKey;

	private Point _renderCenterBlockKey;

	private Point _lastBlockKey;

	private Vector2 _smallDrawTopLeft;

	private Vector2 _mediumDrawTopLeft;

	private Vector2 _largeDrawTopLeft;

	private Vector2 _playerLocationPosition;

	private Vector2 _playerLocationDrawTopLeft;

	private Vector2 _cursorDrawLocationPosition;

	private Rectangle _smallViewRectangle;

	private Rectangle _mediumViewRectangle;

	private Rectangle _largeViewRectangle;

	private Rectangle _viewBlockRectangle;

	private MinimapBlock _lastMinimapBlock;

	private MinimapRoom _currentMinimapRoom;

	public bool HasCameraMoved { get; private set; }

	internal bool DoesDrawDebugRooms { get; set; }

	public Point CursorPosition => _cursorBlockKey;

	public HudMinimap(MinimapSpecification minimap, GCM inGCM, Point drawPosition, bool isMenuMap)
		: base(inGCM, drawPosition)
	{
		_isMenuMap = isMenuMap;
		_minimap = minimap;
		_minimapSprite = _gcm.SpMiniMap;
		_playerLocationSprite = _gcm.SpSmoothCircles;
		RefreshZoom(_drawPosition);
		if (!_isMenuMap)
		{
			minimap.SetAllVisitedAndKnown(value: false, onlySetVisited: true);
		}
		ToggleSize((!_isMenuMap) ? EMinimapToggleState.Small : EMinimapToggleState.Medium);
		_doesDrawBoundingBox = false;
		DoesDrawDebugRooms = false;
	}

	internal void RefreshZoom(Point newDrawPosition)
	{
		_drawPosition = newDrawPosition;
		_blockMultiplier = 4 * base.Zoom;
		_smallViewWidth = 12 * _blockMultiplier;
		_smallViewHeight = 6 * _blockMultiplier;
		_mediumViewWidth = 64 * _blockMultiplier;
		_mediumViewHeight = 32 * _blockMultiplier;
		_largeViewWidth = 100 * _blockMultiplier;
		_largeViewHeight = 46 * _blockMultiplier;
		_smallViewRectangle = new Rectangle(_drawPosition.X - _smallViewWidth, _drawPosition.Y, _smallViewWidth, _smallViewHeight);
		_mediumViewRectangle = new Rectangle(_drawPosition.X - _mediumViewWidth, _drawPosition.Y, _mediumViewWidth, _mediumViewHeight);
		_largeViewRectangle = new Rectangle(_drawPosition.X - _largeViewWidth, _drawPosition.Y, _largeViewWidth, _largeViewHeight);
		_smallDrawTopLeft = _smallViewRectangle.Location.ToVector2();
		_mediumDrawTopLeft = _mediumViewRectangle.Location.ToVector2();
		_largeDrawTopLeft = _largeViewRectangle.Location.ToVector2();
		_playerLocationDrawTopLeft = _smallDrawTopLeft;
		HasCameraMoved = true;
		_lastMinimapBlock = null;
		RefreshCameraCenter();
	}

	public void Update(float delta, Level currentLevel)
	{
		if (currentLevel == null)
		{
			return;
		}
		HasCameraMoved = false;
		if (currentLevel.ID != _lastLevelIndex || currentLevel.RoomID != _lastRoomID)
		{
			_roomDimensions = currentLevel.RoomSize;
			_lastLevelIndex = currentLevel.ID;
			_lastRoomID = currentLevel.RoomID;
			_currentMinimapRoom = _minimap.GetRoomFromLevelAndRoom(_lastLevelIndex, _lastRoomID);
			_isPlayerInDebugRoom = _currentMinimapRoom.IsDebug;
			_isPlayerInBossRoom = currentLevel.IsInBossRoom;
			RefreshCameraCenter();
		}
		UpdatePlayerPosition(currentLevel);
		if (_lastBlockKey != _cursorBlockKey)
		{
			RefreshCameraCenter();
			HasCameraMoved = true;
		}
		_basePlayerLocationScale -= delta * 0.15f;
		if (_basePlayerLocationScale < 0.02f)
		{
			_basePlayerLocationScale = 0.13f;
		}
		_playerLocationScale = _basePlayerLocationScale * ((float)base.Zoom / 3f);
		if (_isMapMenu)
		{
			_cursorAnimationCounter += delta;
			if (_cursorAnimationCounter >= 0.15f)
			{
				_cursorAnimationCounter = 0f;
				_cursorAnimationIndex++;
				if (_cursorAnimationIndex >= 3)
				{
					_cursorAnimationIndex = 0;
				}
			}
		}
		_lastBlockKey = _cursorBlockKey;
	}

	public Vector2 Update(float delta, MinimapBlock currentBlock)
	{
		if (currentBlock != null && _lastMinimapBlock != currentBlock)
		{
			_lastMinimapBlock = currentBlock;
			EMinimapEraType eraFromMinimapColor = MinimapSpecification.GetEraFromMinimapColor(currentBlock.RoomColor);
			Point viewOffsetFromEra = MinimapSpecification.GetViewOffsetFromEra(eraFromMinimapColor);
			_viewBlockRectangle.Location = viewOffsetFromEra;
			Point point = currentBlock.ParentRoom.Position.Subtract(viewOffsetFromEra);
			_playerLocationDrawTopLeft = new Vector2(((float)point.X + 0.6f) * (float)_blockMultiplier + (float)_largeViewRectangle.Left, ((float)point.Y + 0.6f) * (float)_blockMultiplier + (float)_drawPosition.Y);
			_playerLocationPosition = Vector2.Zero;
		}
		_isPlayerLocationVisible = true;
		_basePlayerLocationScale -= delta * 0.15f;
		if (_basePlayerLocationScale < 0.02f)
		{
			_basePlayerLocationScale = 0.13f;
		}
		_playerLocationScale = _basePlayerLocationScale * ((float)base.Zoom / 3f);
		return _playerLocationDrawTopLeft;
	}

	private void UpdatePlayerPosition(Level currentLevel)
	{
		if (_currentMinimapRoom == null || _roomDimensions.X <= 0 || _roomDimensions.Y <= 0)
		{
			return;
		}
		Point playerPosition = currentLevel.GetPlayerPosition();
		Point point = new Point(_currentMinimapRoom.Width * 25 * 16, _currentMinimapRoom.Height * 20 * 16);
		Vector2 vector = new Vector2((float)playerPosition.X / (float)point.X, (float)playerPosition.Y / (float)point.Y);
		Vector2 value = new Vector2((float)_currentMinimapRoom.Width * vector.X, (float)_currentMinimapRoom.Height * vector.Y);
		Point point2 = new Point((int)value.X, (int)value.Y);
		if (!_isMenuMap && _currentMinimapRoom.Blocks.ContainsKey(point2))
		{
			MinimapBlock minimapBlock = _currentMinimapRoom.Blocks[point2];
			if (!minimapBlock.IsVisited)
			{
				minimapBlock.IsVisited = true;
				minimapBlock.IsKnown = true;
				CheckMapCompletionAchievement(currentLevel);
			}
		}
		Vector2 vector2 = new Vector2((float)playerPosition.X / (float)_roomDimensions.X, (float)playerPosition.Y / (float)_roomDimensions.Y);
		Vector2 value2 = new Vector2((float)_currentMinimapRoom.Width * vector2.X, (float)_currentMinimapRoom.Height * vector2.Y);
		Vector2 value3 = Vector2.Add(value2, value) * 0.5f;
		_playerLocationPosition = Vector2.Multiply(value3, _blockMultiplier);
		if (!_isMapMenu)
		{
			_cursorBlockKey = point2.Add(_currentMinimapRoom.Position);
			_isPlayerLocationVisible = true;
			return;
		}
		Point location = _currentMinimapRoom.Position.Add(point2);
		_isPlayerLocationVisible = _currentEraColor == MinimapSpecification.GetEraColorFromLocation(location);
		if (_isPlayerLocationVisible)
		{
			int num = Math.Abs(location.X - _renderCenterBlockKey.X);
			int num2 = Math.Abs(location.Y - _renderCenterBlockKey.Y);
			if (num > 32)
			{
				_isPlayerLocationVisible = false;
			}
			else if (num2 > 16)
			{
				_isPlayerLocationVisible = false;
			}
		}
	}

	private void CheckMapCompletionAchievement(Level level)
	{
		bool flag = true;
		foreach (MinimapBlock value in _currentMinimapRoom.Blocks.Values)
		{
			if (!value.IsSolidWall && !value.IsVisited)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = true;
		MinimapArea minimapArea = _minimap.Areas[level.ID];
		foreach (MinimapRoom room in minimapArea.Rooms)
		{
			if (!room.IsDebug)
			{
				foreach (MinimapBlock value2 in room.Blocks.Values)
				{
					if (!value2.IsSolidWall && !value2.IsVisited)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		if (flag2)
		{
			float completionPercentage = _minimap.GetCompletionPercentage();
			if (completionPercentage >= 100f)
			{
				level.GameSave.UnlockFeat(EGameFeatType.MapCompletion);
			}
		}
	}

	public void RefreshCameraCenter()
	{
		if (_currentMinimapRoom != null)
		{
			if (_minimapToggleState == EMinimapToggleState.Small)
			{
				_viewBlockRectangle.Location = new Point(_cursorBlockKey.X - 6, _cursorBlockKey.Y - 3);
				_playerLocationDrawTopLeft = new Vector2(_smallDrawTopLeft.X + (float)((_currentMinimapRoom.Position.X - _viewBlockRectangle.Location.X) * _blockMultiplier), _smallDrawTopLeft.Y + (float)((_currentMinimapRoom.Position.Y - _viewBlockRectangle.Location.Y) * _blockMultiplier));
			}
			else if (_minimapToggleState == EMinimapToggleState.Medium)
			{
				_viewBlockRectangle.Location = new Point(_renderCenterBlockKey.X - 32, _renderCenterBlockKey.Y - 16);
				_playerLocationDrawTopLeft = new Vector2(_mediumDrawTopLeft.X + (float)((_currentMinimapRoom.Position.X - _viewBlockRectangle.Location.X) * _blockMultiplier), _mediumDrawTopLeft.Y + (float)((_currentMinimapRoom.Position.Y - _viewBlockRectangle.Location.Y) * _blockMultiplier));
				_currentEraColor = MinimapSpecification.GetEraColorFromLocation(_renderCenterBlockKey);
				UpdateMarkers();
			}
			else if (_minimapToggleState == EMinimapToggleState.Large)
			{
				_viewBlockRectangle.Location = new Point(_renderCenterBlockKey.X - 50, _renderCenterBlockKey.Y - 23);
				_playerLocationDrawTopLeft = new Vector2(_largeDrawTopLeft.X + (float)((_currentMinimapRoom.Position.X - _viewBlockRectangle.Location.X) * _blockMultiplier), _largeDrawTopLeft.Y + (float)((_currentMinimapRoom.Position.Y - _viewBlockRectangle.Location.Y) * _blockMultiplier));
				_currentEraColor = MinimapSpecification.GetEraColorFromLocation(_renderCenterBlockKey);
				UpdateMarkers();
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, float alphaAmount)
	{
		if (_minimapToggleState == EMinimapToggleState.Small)
		{
			if (!_isPlayerInDebugRoom && !_isPlayerInBossRoom)
			{
				if (_doesDrawBoundingBox)
				{
					spriteBatch.DrawRectangleBorder(_gcm.TxBlankSquare, _smallViewRectangle, 2, Color.Blue * 0.5f * alphaAmount);
				}
				_minimap.DrawView(spriteBatch, _minimapSprite, _viewBlockRectangle, _smallDrawTopLeft, base.Zoom, alphaAmount, DoesDrawDebugRooms);
			}
		}
		else if (_minimapToggleState == EMinimapToggleState.Medium)
		{
			if (_doesDrawBoundingBox)
			{
				spriteBatch.DrawRectangleBorder(_gcm.TxBlankSquare, _mediumViewRectangle, 2, Color.Blue * 0.5f * alphaAmount);
			}
			_minimap.DrawView(spriteBatch, _minimapSprite, _viewBlockRectangle, _mediumDrawTopLeft, base.Zoom, alphaAmount, DoesDrawDebugRooms);
		}
		else if (_minimapToggleState == EMinimapToggleState.Large)
		{
			_minimap.DrawView(spriteBatch, _minimapSprite, _viewBlockRectangle, _largeDrawTopLeft, base.Zoom, alphaAmount, DoesDrawDebugRooms);
		}
		if ((_minimapToggleState != EMinimapToggleState.Small || (!_isPlayerInDebugRoom && !_isPlayerInBossRoom)) && _minimapToggleState != 0 && _isPlayerLocationVisible)
		{
			SmoothCircle.Draw(spriteBatch, _playerLocationSprite, Vector2.Add(_playerLocationDrawTopLeft, _playerLocationPosition), _playerLocationColor * alphaAmount, 0f, _playerLocationScale);
		}
		if (!_isMapMenu)
		{
			return;
		}
		_markerColorTimer += 1f / 60f;
		if (_markerColorTimer >= (float)Math.PI * 2f)
		{
			_markerColorTimer -= (float)Math.PI * 2f;
		}
		float num = MathHelper.Lerp(0.1f, 1f, (float)Math.Abs(Math.Sin(_markerColorTimer)));
		Color color = Color.White * num;
		Rectangle frameSource;
		foreach (MinimapMarker value in _minimap.Markers.Values)
		{
			if (value.IsVisible)
			{
				frameSource = _gcm.SpPauseMenu.GetFrameSource((int)(60 + value.MarkerColor));
				spriteBatch.Draw(_gcm.SpPauseMenu.Texture, value.DrawLocationPosition, frameSource, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			}
		}
		frameSource = _gcm.SpPauseMenu.GetFrameSource(57 + _cursorAnimationIndex);
		spriteBatch.Draw(_gcm.SpPauseMenu.Texture, _cursorDrawLocationPosition, frameSource, Color.White, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
	}

	public void Toggle()
	{
		switch (_minimapToggleState)
		{
		case EMinimapToggleState.Off:
			ToggleSize(EMinimapToggleState.Small);
			break;
		case EMinimapToggleState.Small:
			ToggleSize(EMinimapToggleState.Medium);
			break;
		case EMinimapToggleState.Medium:
			ToggleSize(EMinimapToggleState.Off);
			break;
		}
		RefreshCameraCenter();
	}

	public void ToggleSize(EMinimapToggleState newState)
	{
		if (newState != _minimapToggleState)
		{
			_minimapToggleState = newState;
			if (_minimapToggleState == EMinimapToggleState.Small)
			{
				_viewBlockRectangle = new Rectangle(0, 0, 12, 6);
			}
			else if (_minimapToggleState == EMinimapToggleState.Medium)
			{
				_viewBlockRectangle = new Rectangle(0, 0, 64, 32);
			}
			else if (_minimapToggleState == EMinimapToggleState.Large)
			{
				_viewBlockRectangle = new Rectangle(0, 0, 100, 46);
			}
		}
	}

	public void MoveCursor(Point location)
	{
		_isMapMenu = true;
		_cursorBlockKey = location;
		Point b = _cursorBlockKey.Subtract(_renderCenterBlockKey).Multiply(4 * base.Zoom);
		_cursorDrawLocationPosition = _mediumViewRectangle.Center.Add(b).Add(-base.Zoom, -base.Zoom * 2).ToVector2();
	}

	public void MoveRenderCenter(Point location)
	{
		_isMapMenu = true;
		_renderCenterBlockKey = location;
	}

	public void UpdateMarkers()
	{
		Point b = new Point(base.Zoom * 2, base.Zoom * -8);
		foreach (MinimapMarker value in _minimap.Markers.Values)
		{
			Point b2 = value.Location.Subtract(_renderCenterBlockKey).Multiply(4 * base.Zoom);
			value.DrawLocationPosition = _mediumViewRectangle.Center.Add(b2).ToVector2().Add(b);
			if (value.EraColor == _currentEraColor)
			{
				value.IsVisible = _viewBlockRectangle.Contains(value.Location);
			}
			else
			{
				value.IsVisible = false;
			}
		}
	}
}
