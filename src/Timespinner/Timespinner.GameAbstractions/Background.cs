using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions;

public class Background
{
	public enum EBackgroundType
	{
		Default,
		Stretch
	}

	private const float BaseScrollRate = 5f;

	private readonly bool _isInitialized;

	private readonly bool _isHorizontallyFlipped;

	private readonly bool _isVerticallyFlipped;

	private readonly bool _doesAlternateXFlipped;

	private readonly bool _doesAlternateYFlipped;

	private readonly bool _doesTileEast;

	private readonly bool _doesTileWest;

	private readonly bool _doesTileNorth;

	private readonly bool _doesTileSouth;

	private readonly bool _doesScroll;

	private readonly bool _doesPanX;

	private readonly bool _doesPanY;

	private readonly EBackgroundType _type;

	private readonly EBackgroundTextureType _backgroundTextureType;

	private readonly int _width;

	private readonly int _height;

	private readonly int _frameWidth;

	private readonly int _frameHeight;

	private readonly int _frameIndex;

	private readonly float _baseZoom;

	private readonly Point _tileInterval;

	private readonly Point _drawStart;

	private readonly Point _drawEnd;

	private readonly Vector2 _panRatio;

	private readonly Vector2 _scrollSpeed;

	private readonly Color _baseColor;

	private readonly Rectangle _frameSource;

	private readonly Rectangle _stretchRect;

	private readonly Texture2D _drawingTexture;

	private readonly SpriteSheet _sprite;

	private readonly Level _level;

	private int _halfScreenWidth;

	private int _halfScreenHeight;

	private int _horizontalAncestors;

	private int _verticalAncestors;

	private int _xIndexStart;

	private int _yIndexStart;

	private float _lastZoomValue = 2f;

	private float _farCameraPositionX;

	private float _farCameraPositionY;

	private Vector2 _position;

	private Vector2 _scrollOffset;

	private Vector2 _drawPos;

	private Vector2 _startPos;

	private Vector2 _endPos;

	private Vector2 _incrementation;

	private Vector2 _lastCameraPosition;

	public bool IsVisible => true;

	internal EBackgroundTextureType TextureType => _backgroundTextureType;

	public Point TileInterval => _tileInterval;

	public Vector2 Position
	{
		get
		{
			if (!_doesScroll)
			{
				return _position;
			}
			return Vector2.Add(_position, _scrollOffset);
		}
		set
		{
			_position = value;
		}
	}

	public Vector2 PanRatio => _panRatio;

	public Vector2 DrawPosition => _drawPos;

	public Color DrawColor { get; set; }

	public Color BaseColor => _baseColor;

	public Level Level => _level;

	public Background(BackgroundSpecification specification, Level inLevel)
	{
		_level = inLevel;
		_sprite = specification.GetSpriteFromType(_level.GCM);
		_drawingTexture = _sprite.Texture;
		_frameIndex = specification.FrameIndex;
		_frameSource = _sprite.GetFrameSource(_frameIndex);
		_frameWidth = _frameSource.Width;
		_frameHeight = _frameSource.Height;
		_backgroundTextureType = specification.TextureType;
		_scrollSpeed = specification.ScrollSpeed;
		_tileInterval = specification.TileInterval;
		_panRatio = specification.PanRatio;
		_drawStart = specification.StartPoint;
		_drawEnd = specification.EndPoint;
		_baseZoom = specification.Zoom;
		_isHorizontallyFlipped = specification.IsHorizontallyFlipped;
		_isVerticallyFlipped = specification.IsVerticallyFlipped;
		_doesAlternateXFlipped = specification.DoesAlternateXFlip;
		_doesAlternateYFlipped = specification.DoesAlternateYFlip;
		_doesTileEast = specification.DoesTileEast;
		_doesTileNorth = specification.DoesTileNorth;
		_doesTileWest = specification.DoesTileWest;
		_doesTileSouth = specification.DoesTileSouth;
		_stretchRect = specification.StretchRectangle;
		_baseColor = specification.DrawColor;
		DrawColor = _baseColor;
		_doesScroll = _scrollSpeed != Vector2.Zero;
		_doesPanX = Math.Abs(_panRatio.X) > 0.001f;
		_doesPanY = Math.Abs(_panRatio.Y) > 0.001f;
		_width = ((_doesTileEast || _doesTileWest) ? (_frameWidth + _tileInterval.X) : _frameWidth);
		_height = ((_doesTileNorth || _doesTileSouth) ? (_frameHeight + _tileInterval.Y) : _frameHeight);
		_type = ((!_stretchRect.IsEmpty) ? EBackgroundType.Stretch : EBackgroundType.Default);
		Position = _drawStart.ToVector2();
		RefreshCameraValues();
		_isInitialized = true;
	}

	public void Update(float delta)
	{
		if (_doesScroll)
		{
			UpdateScrolling(delta);
		}
		RefreshCameraValues();
	}

	private void UpdateScrolling(float delta)
	{
		_scrollOffset.X = (_scrollOffset.X + delta * _scrollSpeed.X * 5f) % (float)(_doesAlternateXFlipped ? (_frameWidth * 2) : _frameWidth);
		_scrollOffset.Y = (_scrollOffset.Y + delta * _scrollSpeed.Y * 5f) % (float)(_doesAlternateYFlipped ? (_frameHeight * 2) : _frameHeight);
	}

	public void RefreshCameraValues()
	{
		bool flag = _doesScroll;
		if (_lastZoomValue != _level.BackgroundZoom || !_isInitialized)
		{
			flag = true;
			_lastZoomValue = _level.BackgroundZoom;
			_halfScreenWidth = _level.VisibleArea.Width / 2;
			_halfScreenHeight = _level.VisibleArea.Height / 2;
		}
		if (_lastCameraPosition != _level.CameraPosition || !_isInitialized || _doesScroll)
		{
			flag = true;
			_lastCameraPosition = _level.CameraPosition;
			_farCameraPositionX = (_doesPanX ? (_lastCameraPosition.X / _panRatio.X) : 0f);
			_farCameraPositionY = (_doesPanY ? (_lastCameraPosition.Y / _panRatio.Y) : 0f);
			_horizontalAncestors = (int)Math.Floor((_farCameraPositionX - (float)_halfScreenWidth - Position.X) / (float)_width);
			_verticalAncestors = (int)Math.Floor((_farCameraPositionY - (float)_halfScreenHeight - Position.Y) / (float)_height);
		}
		if (!flag)
		{
			return;
		}
		float num = _farCameraPositionX - ((float)(_horizontalAncestors * _width) + Position.X);
		float num2 = _farCameraPositionY - ((float)(_verticalAncestors * _height) + Position.Y);
		float val = num - (float)(Math.Ceiling((float)_halfScreenWidth * 2f / (float)_width) + 1.0) * (float)_width;
		float val2 = num2 - (float)(Math.Ceiling((float)_halfScreenHeight * 2f / (float)_height) + 1.0) * (float)_height;
		_drawPos = Vector2.Subtract(new Vector2(_farCameraPositionX, _farCameraPositionY), new Vector2(Position.X, Position.Y));
		_startPos = Vector2.Zero;
		_endPos = Vector2.Zero;
		_incrementation = Vector2.Zero;
		_xIndexStart = _horizontalAncestors;
		_yIndexStart = _verticalAncestors;
		if (_doesTileEast && _doesTileWest)
		{
			_incrementation.X = -_width;
			_startPos.X = num;
			_endPos.X = -_halfScreenWidth;
		}
		else if (_doesTileEast)
		{
			_startPos.X = Math.Min(_drawPos.X, num);
			_endPos.X = -(_halfScreenWidth + _width);
			_incrementation.X = -_width;
			if (_drawPos.X < num)
			{
				_xIndexStart = 0;
			}
		}
		else if (_doesTileWest)
		{
			_startPos.X = Math.Max(_drawPos.X, val);
			_endPos.X = _halfScreenWidth + _width;
			_incrementation.X = _width;
		}
		else
		{
			_startPos.X = _drawPos.X;
			_endPos.X = _startPos.X;
			_incrementation.X = _width;
			if (_startPos.X > (float)(_halfScreenWidth + _width))
			{
				_endPos.X -= 1f;
			}
			else if (_startPos.X < (float)(-(_halfScreenWidth + _width)))
			{
				_endPos.X -= 1f;
			}
		}
		if (_doesTileNorth && _doesTileSouth)
		{
			_incrementation.Y = -_height;
			_startPos.Y = num2;
			_endPos.Y = -_halfScreenHeight;
			return;
		}
		if (_doesTileSouth)
		{
			_startPos.Y = Math.Min(_drawPos.Y, num2);
			_endPos.Y = -(_halfScreenHeight + _height);
			_incrementation.Y = -_height;
			if (_drawPos.Y < num2)
			{
				_yIndexStart = 0;
			}
			return;
		}
		if (_doesTileNorth)
		{
			_startPos.Y = Math.Max(_drawPos.Y, val2);
			_endPos.Y = _halfScreenHeight + _height;
			_incrementation.Y = _height;
			return;
		}
		_startPos.Y = _drawPos.Y;
		_endPos.Y = _startPos.Y;
		_incrementation.Y = _height;
		if (_startPos.Y > (float)(_halfScreenHeight + _height))
		{
			_endPos.Y -= 1f;
		}
		else if (_startPos.Y < (float)(-(_halfScreenHeight + _height)))
		{
			_endPos.Y -= 1f;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (!IsVisible)
		{
			return;
		}
		int num = _yIndexStart;
		if (_incrementation.Y > 0f)
		{
			for (float num2 = _startPos.Y; num2 <= _endPos.Y; num2 += _incrementation.Y)
			{
				if (_drawEnd.Y > 0 && num > _drawEnd.Y)
				{
					break;
				}
				int num3 = _xIndexStart;
				if (_incrementation.X > 0f)
				{
					for (float num4 = _startPos.X; num4 <= _endPos.X; num4 += _incrementation.X)
					{
						DrawHelper(spriteBatch, new Vector2(num4, num2), num3 % 2 == 0, num % 2 == 0);
						num3++;
						if (_drawEnd.X > 0 && num3 > _drawEnd.X)
						{
							break;
						}
					}
				}
				else
				{
					for (float num5 = _startPos.X; num5 >= _endPos.X; num5 += _incrementation.X)
					{
						DrawHelper(spriteBatch, new Vector2(num5, num2), num3 % 2 == 0, num % 2 == 0);
						num3++;
						if (_drawEnd.X > 0 && num3 > _drawEnd.X)
						{
							break;
						}
					}
				}
				num++;
			}
			return;
		}
		for (float num6 = _startPos.Y; num6 >= _endPos.Y; num6 += _incrementation.Y)
		{
			if (_drawEnd.Y > 0 && num > _drawEnd.Y)
			{
				break;
			}
			int num7 = _xIndexStart;
			if (_incrementation.X > 0f)
			{
				for (float num8 = _startPos.X; num8 <= _endPos.X; num8 += _incrementation.X)
				{
					DrawHelper(spriteBatch, new Vector2(num8, num6), num7 % 2 == 0, num % 2 == 0);
					num7++;
					if (_drawEnd.X > 0 && num7 > _drawEnd.X)
					{
						break;
					}
				}
			}
			else
			{
				for (float num9 = _startPos.X; num9 >= _endPos.X; num9 += _incrementation.X)
				{
					DrawHelper(spriteBatch, new Vector2(num9, num6), num7 % 2 == 0, num % 2 == 0);
					num7++;
					if (_drawEnd.X > 0 && num7 > _drawEnd.X)
					{
						break;
					}
				}
			}
			num++;
		}
	}

	private void DrawHelper(SpriteBatch spriteBatch, Vector2 drawPos, bool isOddX, bool isOddY)
	{
		Vector2 monitorScreenCenter = _level.MonitorScreenCenter;
		if (_type == EBackgroundType.Default)
		{
			bool flag = _isHorizontallyFlipped;
			bool flag2 = _isVerticallyFlipped;
			if (_doesAlternateXFlipped && isOddX)
			{
				flag = !flag;
			}
			if (_doesAlternateYFlipped && isOddY)
			{
				flag2 = !flag2;
			}
			SpriteEffects spriteEffects = (flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			if (flag2)
			{
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			spriteBatch.Draw(_drawingTexture, monitorScreenCenter, _frameSource, DrawColor, 0f, drawPos, _lastZoomValue + _baseZoom, spriteEffects, 0f);
		}
		else
		{
			spriteBatch.Draw(_drawingTexture, new Rectangle((int)(monitorScreenCenter.X - drawPos.X * _lastZoomValue), (int)(monitorScreenCenter.Y - drawPos.Y * _lastZoomValue), (int)((float)_frameWidth * _lastZoomValue), (int)((float)_frameHeight * _lastZoomValue)), _frameSource, DrawColor);
		}
	}
}
