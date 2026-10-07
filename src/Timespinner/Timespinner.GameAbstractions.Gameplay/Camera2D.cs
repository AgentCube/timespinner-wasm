using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Gameplay;

public class Camera2D
{
	private const int FocusOffsetY = 0;

	private const float FloorRepulsionDecayRate = 30f;

	private const float FloorRepulsionGrowthRate = 90f;

	private const float FloorRepulsionDecayTime = 1f;

	private const float FloorRepulsionGrowthTime = 0.5f;

	private const float BufferDistanceSquared = 500f;

	private const float FocusPanRateX = 24f;

	private const float ChaseSpeed = 10f;

	private const int MaxDistanceFocusX = 0;

	private readonly Point _cameraBlockerDist = new Point(200, 120);

	private readonly Vector2 _levelRenderCenter;

	private readonly Level _level;

	private bool _hasCameraChanged;

	private int _focusOffsetX;

	private int _floorRepulsionTarget;

	private float _currentFloorRepulsion;

	private float _floorRepulsionTimer;

	private float _zoomValue;

	private Point _positionValue;

	private Point _bubbleBuffer = new Point(8, 4);

	private Point _lastPosition;

	private Point _targetPoint;

	private bool _isScreenShaking;

	private bool _isScreenShakeInfinite;

	private bool _isScreenShakeAffectedByTime;

	private float _screenShakeTime;

	private float _screenShakeTimeTotal;

	private float _screenShakeOscillation;

	private float _screenShakeFrequency;

	private Point _currentScreenShake;

	private Vector2 _screenShakeAmplitude;

	public bool DoesUseFollowBubble { get; set; }

	public bool HasChanged => _hasCameraChanged;

	public CameraFollowType FollowType { get; set; }

	public Vector2 LevelRenderCenter => _levelRenderCenter;

	public Point CameraBlockerDist => _cameraBlockerDist;

	internal Point TargetPoint
	{
		get
		{
			return _targetPoint;
		}
		set
		{
			_targetPoint = value;
		}
	}

	public Point Position
	{
		get
		{
			return _positionValue;
		}
		set
		{
			if (_positionValue != value)
			{
				_hasCameraChanged = true;
				_positionValue = value;
			}
		}
	}

	public float Zoom
	{
		get
		{
			return _zoomValue;
		}
		set
		{
			if (_zoomValue != value)
			{
				_hasCameraChanged = true;
				_zoomValue = value;
			}
		}
	}

	public Camera2D(Vector2 levelRenderCenter, Level level)
	{
		_level = level;
		_zoomValue = 1f;
		_positionValue = Point.Zero;
		FollowType = CameraFollowType.RectangleFollow;
		_levelRenderCenter = levelRenderCenter;
		DoesUseFollowBubble = true;
	}

	public void ResetChanged()
	{
		_hasCameraChanged = false;
	}

	public void Update(float delta, Rectangle inBounds)
	{
		switch (FollowType)
		{
		case CameraFollowType.RectangleChase:
			ChaseRectangleFollowPoint(_targetPoint);
			break;
		case CameraFollowType.RectangleFollow:
			RectangleFollowPoint(_targetPoint);
			break;
		case CameraFollowType.LinearFollow:
			TravelToPoint(_targetPoint, delta, 10f);
			break;
		}
		if (HasChanged)
		{
			DetectCameraBlockers();
		}
		UpdateScreenshake(delta);
		EnforceBoundary(inBounds);
		Position = new Point(_positionValue.X + _currentScreenShake.X, _positionValue.Y + _currentScreenShake.Y);
	}

	private void UpdateScreenshake(float delta)
	{
		if (_isScreenShaking)
		{
			if (!_level.IsTimeFrozen || !_isScreenShakeAffectedByTime)
			{
				_screenShakeOscillation += delta;
			}
			float num = 1f;
			if (!_isScreenShakeInfinite)
			{
				num = 1f - _screenShakeTime / _screenShakeTimeTotal;
			}
			double num2 = Math.Sin(Math.PI * 2.0 * (double)_screenShakeOscillation * (double)_screenShakeFrequency) * (double)num;
			_currentScreenShake = new Point((int)Math.Round(num2 * (double)_screenShakeAmplitude.X), (int)Math.Round(num2 * (double)_screenShakeAmplitude.Y));
			if (!_level.IsTimeFrozen || !_isScreenShakeAffectedByTime)
			{
				_screenShakeTime += delta;
			}
			if (!_isScreenShakeInfinite && (_screenShakeTime >= _screenShakeTimeTotal || _screenShakeTimeTotal <= 0f))
			{
				_isScreenShaking = false;
				_screenShakeOscillation = 0f;
				_currentScreenShake = Point.Zero;
			}
		}
	}

	public void RequestScreenShake(Vector2 inDimensions, float inShakeTime, float inFrequency, bool isAffectedByTime)
	{
		_isScreenShaking = true;
		_isScreenShakeAffectedByTime = isAffectedByTime;
		_screenShakeTime = 0f;
		_screenShakeTimeTotal = inShakeTime;
		_screenShakeFrequency = inFrequency;
		_screenShakeAmplitude = inDimensions;
		_isScreenShakeInfinite = inShakeTime < 0f;
	}

	public void UpdateFloorRepulsion(float delta, Rectangle inBounds, Rectangle visibleArea16, Dictionary<Point, Tile> solidTiles)
	{
		int num = 0;
		int num2 = Math.Max(visibleArea16.Left, 0);
		int num3 = Math.Min(visibleArea16.Right, inBounds.Right / 16 - 1);
		int num4 = (int)(_currentFloorRepulsion / 16f);
		int num5 = visibleArea16.Center.Y - num4;
		int num6 = visibleArea16.Bottom + num4;
		for (int i = num5; i <= num6; i++)
		{
			bool flag = true;
			for (int j = num2; j <= num3; j++)
			{
				if (!solidTiles.ContainsKey(new Point(j, i)))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				num += 16;
			}
		}
		num -= 32;
		int num7 = ((num > 0) ? num : 0);
		if (_floorRepulsionTarget > num7 + 32 || _floorRepulsionTarget < num7 - 32)
		{
			_floorRepulsionTarget = num7;
			_floorRepulsionTimer = ((_currentFloorRepulsion > (float)_floorRepulsionTarget) ? 1f : 0.5f);
		}
		if (_floorRepulsionTimer != 0f)
		{
			_floorRepulsionTimer -= delta;
			if (_floorRepulsionTimer < 0f)
			{
				_floorRepulsionTimer = 0f;
			}
		}
		if (_currentFloorRepulsion != (float)_floorRepulsionTarget)
		{
			bool flag2 = _currentFloorRepulsion > (float)_floorRepulsionTarget;
			_currentFloorRepulsion += delta * (flag2 ? (-30f) : 90f);
			if (flag2 != _currentFloorRepulsion > (float)_floorRepulsionTarget)
			{
				_currentFloorRepulsion = _floorRepulsionTarget;
			}
		}
		Position = new Point(Position.X, Position.Y - (int)_currentFloorRepulsion);
	}

	public void RectangleFollowPoint(Point targetPoint)
	{
		int num = Position.X;
		int num2 = Position.Y;
		int num3 = Position.X - targetPoint.X;
		int num4 = Position.Y - targetPoint.Y;
		Point point = (DoesUseFollowBubble ? _bubbleBuffer : Point.Zero);
		if (Math.Abs(num3) > point.X)
		{
			num = ((num3 >= 0) ? (targetPoint.X + point.X) : (targetPoint.X - point.X));
		}
		if (Math.Abs(num4) > point.Y)
		{
			num2 = ((num4 >= 0) ? (targetPoint.Y + point.Y) : (targetPoint.Y - point.Y));
		}
		if (num != Position.X || num2 != Position.Y)
		{
			Position = new Point(num, num2);
		}
	}

	public void ChaseRectangleFollowPoint(Point targetPoint)
	{
		Point point = targetPoint;
		point.X += _focusOffsetX;
		point.Y = point.Y;
		int num = Position.X;
		int num2 = Position.Y;
		int num3 = Position.X - point.X;
		int num4 = Position.Y - point.Y;
		if (Math.Abs(num3) > _bubbleBuffer.X)
		{
			num = ((num3 >= 0) ? (point.X + _bubbleBuffer.X) : (point.X - _bubbleBuffer.X));
			if (_lastPosition != _targetPoint)
			{
				_focusOffsetX = (int)MathHelper.Clamp((float)_focusOffsetX - (float)num3 / 24f, 0f, 0f);
			}
		}
		if (Math.Abs(num4) > _bubbleBuffer.Y)
		{
			num2 = ((num4 >= 0) ? (point.Y + _bubbleBuffer.Y) : (point.Y - _bubbleBuffer.Y));
		}
		if (num != Position.X || num2 != Position.Y)
		{
			Position = new Point(num, num2);
		}
		_lastPosition = _targetPoint;
	}

	public void TravelToPoint(Point target, float delta, float chaseSpeed)
	{
		float num = chaseSpeed;
		if (Position != target)
		{
			float num2 = new Vector2(_positionValue.X - target.X, _positionValue.Y - target.Y).LengthSquared();
			if (num2 < 500f)
			{
				num *= num2 / 500f;
			}
			Position = new Point((int)MathHelper.Lerp(Position.X, target.X, delta * num), (int)MathHelper.Lerp(Position.Y, target.Y, delta * num));
		}
	}

	public void EnforceBoundary(Rectangle bounds)
	{
		if (_positionValue.X < bounds.X)
		{
			_positionValue.X = bounds.X;
			_focusOffsetX = 0;
		}
		if (_positionValue.X > bounds.Width)
		{
			_positionValue.X = bounds.Width;
			_focusOffsetX = 0;
		}
		if (_positionValue.Y < bounds.Y)
		{
			_positionValue.Y = bounds.Y;
		}
		if (_positionValue.Y > bounds.Height)
		{
			_positionValue.Y = bounds.Height;
		}
	}

	public void EnforceCameraBlockers(int direction, int barrier)
	{
		switch (direction)
		{
		case 0:
			_positionValue.X = barrier + CameraBlockerDist.X;
			_focusOffsetX = 0;
			break;
		case 1:
			_positionValue.Y = barrier + CameraBlockerDist.Y;
			break;
		case 2:
			_positionValue.X = barrier - CameraBlockerDist.X;
			_focusOffsetX = 0;
			break;
		case 3:
			_positionValue.Y = barrier - CameraBlockerDist.Y;
			_focusOffsetX = 0;
			break;
		}
	}

	public void DetectCameraBlockers()
	{
		Dictionary<Point, Tile> cameraBlockerTiles = _level.CameraBlockerTiles;
		if (cameraBlockerTiles.Count <= 0)
		{
			return;
		}
		Point zero = Point.Zero;
		Vector2 visibleSize = _level.VisibleSize;
		Rectangle rectangle = new Rectangle((int)(((float)Position.X - visibleSize.X / 2f) / 16f), (int)(((float)Position.Y - visibleSize.Y / 2f) / 16f), (int)visibleSize.X / 16, (int)visibleSize.Y / 16);
		bool flag = false;
		for (int i = rectangle.Center.X; i <= rectangle.Right + 1; i++)
		{
			zero.X = i;
			for (int j = rectangle.Top; j < rectangle.Bottom; j++)
			{
				zero.Y = j;
				if (!cameraBlockerTiles.ContainsKey(zero))
				{
					continue;
				}
				Tile tile = cameraBlockerTiles[zero];
				if (tile.Special == ETileSpecialType.CamBlock && (!cameraBlockerTiles.ContainsKey(new Point(zero.X + 1, zero.Y)) || cameraBlockerTiles[new Point(zero.X + 1, zero.Y)].Special != ETileSpecialType.CamBlock) && (!cameraBlockerTiles.ContainsKey(new Point(zero.X - 1, zero.Y)) || cameraBlockerTiles[new Point(zero.X - 1, zero.Y)].Special != ETileSpecialType.CamBlock))
				{
					if (tile.Bbox.Left - Position.X < CameraBlockerDist.X)
					{
						EnforceCameraBlockers(2, tile.Bbox.Left);
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		flag = false;
		for (int num = rectangle.Center.X; num >= rectangle.Left; num--)
		{
			zero.X = num;
			for (int k = rectangle.Top - 1; k < rectangle.Bottom; k++)
			{
				zero.Y = k;
				if (!cameraBlockerTiles.ContainsKey(zero))
				{
					continue;
				}
				Tile tile2 = cameraBlockerTiles[zero];
				if (tile2.Special == ETileSpecialType.CamBlock && (!cameraBlockerTiles.ContainsKey(new Point(zero.X + 1, zero.Y)) || cameraBlockerTiles[new Point(zero.X + 1, zero.Y)].Special != ETileSpecialType.CamBlock) && (!cameraBlockerTiles.ContainsKey(new Point(zero.X - 1, zero.Y)) || cameraBlockerTiles[new Point(zero.X - 1, zero.Y)].Special != ETileSpecialType.CamBlock))
				{
					if (Position.X - tile2.Bbox.Right < CameraBlockerDist.X)
					{
						EnforceCameraBlockers(0, tile2.Bbox.Right);
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		rectangle.X = (int)(((float)Position.X - visibleSize.X / 2f) / 16f);
		rectangle.Y = (int)(((float)Position.Y - visibleSize.Y / 2f) / 16f);
		flag = false;
		for (int num2 = rectangle.Center.Y; num2 >= rectangle.Top; num2--)
		{
			zero.Y = num2;
			for (int l = rectangle.Left; l < rectangle.Right; l++)
			{
				zero.X = l;
				if (!cameraBlockerTiles.ContainsKey(zero))
				{
					continue;
				}
				Tile tile3 = cameraBlockerTiles[zero];
				if (tile3.Special == ETileSpecialType.CamBlock && (!cameraBlockerTiles.ContainsKey(new Point(zero.X, zero.Y - 1)) || cameraBlockerTiles[new Point(zero.X, zero.Y - 1)].Special != ETileSpecialType.CamBlock) && (!cameraBlockerTiles.ContainsKey(new Point(zero.X, zero.Y + 1)) || cameraBlockerTiles[new Point(zero.X, zero.Y + 1)].Special != ETileSpecialType.CamBlock))
				{
					if (Position.Y - tile3.Bbox.Bottom < CameraBlockerDist.Y)
					{
						EnforceCameraBlockers(1, tile3.Bbox.Bottom);
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		for (int m = rectangle.Center.Y; m <= rectangle.Bottom; m++)
		{
			zero.Y = m;
			for (int n = rectangle.Left; n < rectangle.Right; n++)
			{
				zero.X = n;
				if (!cameraBlockerTiles.ContainsKey(zero))
				{
					continue;
				}
				Tile tile4 = cameraBlockerTiles[zero];
				if (tile4.Special == ETileSpecialType.CamBlock && (!cameraBlockerTiles.ContainsKey(new Point(zero.X, zero.Y - 1)) || cameraBlockerTiles[new Point(zero.X, zero.Y - 1)].Special != ETileSpecialType.CamBlock) && (!cameraBlockerTiles.ContainsKey(new Point(zero.X, zero.Y + 1)) || cameraBlockerTiles[new Point(zero.X, zero.Y + 1)].Special != ETileSpecialType.CamBlock))
				{
					if (tile4.Bbox.Top - Position.Y < CameraBlockerDist.Y)
					{
						EnforceCameraBlockers(3, tile4.Bbox.Top);
					}
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
}
