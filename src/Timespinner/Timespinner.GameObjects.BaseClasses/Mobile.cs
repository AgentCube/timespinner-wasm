using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;

namespace Timespinner.GameObjects.BaseClasses;

public abstract class Mobile : GameObject
{
	public const float DefaultDeltaInterval = 0.0166666f;

	public const float DefaultFramerate = 60f;

	protected const float MaxGrabNextTimer = 0.2f;

	protected const float JumpControlPower = 0.5f;

	protected const float MoveAccelerationY = 5000f;

	protected readonly List<Point> _intermediatePositions = new List<Point>();

	protected readonly List<Appendage> _appendages = new List<Appendage>();

	private readonly List<GameEvent> _currentMovingPlatforms = new List<GameEvent>();

	protected readonly List<SFXCueInstance> _sfxCueInstances = new List<SFXCueInstance>();

	protected readonly List<ParticleSystem> _particleSystems = new List<ParticleSystem>();

	private bool _isFlippedVertically;

	private bool _isFacingLeft = true;

	protected bool _isGrounded;

	protected bool _wasGrounded;

	protected bool _isJumping;

	protected bool _wasJumping;

	protected bool _isFlying;

	protected bool _isMiracleJumping;

	protected bool _isGrabbing;

	protected bool _wasGrabbing;

	protected bool _isGrabbingFrozenObject;

	protected bool _isStandingOnMonster;

	protected bool _shouldIgnoreIntermediateSlopes;

	protected bool _doesTrackTimeSinceGrounded;

	protected bool _isGroundColliding;

	protected bool _isPlatformColliding = true;

	protected bool _doesCollideWithTiles = true;

	protected bool _isIgnoringPlatform;

	protected bool _isFrozen;

	protected bool _isOnSlope;

	protected bool _isNearSlope;

	protected bool _isHittingHeadOnCeiling;

	protected bool _wasHittingHeadOnCeiling;

	protected bool _isCollidingWithWall;

	protected bool _hasSkippedASlopeThisTick;

	protected bool _isAffectedByWater;

	protected bool _isAffectedByGravity = true;

	protected bool _isAffectedByFriction = true;

	protected bool _isAffectedByLevelBounds = true;

	protected bool _isAffectedByTime = true;

	protected bool _isBeingSquished;

	protected bool _doesUseAppendageCollision = true;

	protected bool _doesUse16X16TileCollisionBbox = true;

	protected bool _doAppendagesMatchImageFacing;

	protected ETeamSide _defaultTeam = ETeamSide.Neutral;

	protected float _movementX;

	protected float _movementY;

	protected float _timeSinceGrounded;

	protected float _lastTimeSinceGrounded;

	protected float _currentJumpTime;

	protected float _currentSproingTime;

	protected float _currentSlopeAngle;

	protected float _currentSlopeMovementXMultiplier = 0.85f;

	protected float _grabNextTimer;

	protected float _grabJumpTimer;

	protected float _timeToTurnAround;

	protected Point _targetGrabPosition;

	protected Point _targetPosition;

	protected Point _startPosition;

	protected Point _previousPosition;

	protected Point _previousPreviousPosition;

	protected Point _collisionPositionOffsetThisFrame;

	protected Vector2 _floatPosition = Vector2.Zero;

	protected Vector2 _velocity;

	protected Vector2 _acceleration;

	protected Rectangle _outerBbox = Rectangle.Empty;

	protected Rectangle _tileCollisionBbox;

	protected GameObject _currentTarget;

	protected GameObject _objectThatSquishedUs;

	protected GameEvent _currentlyGrabbedEvent;

	protected float _moveAcceleration = 5000f;

	protected float _maxMoveSpeed = 200f;

	protected bool _doesOverrideVelocity;

	protected bool _doesOverrideMaxSpeed;

	protected float _groundDragFactor = 0.3f;

	protected float _airDragFactor = 0.3f;

	protected float _waterDragFactor = 0.5f;

	protected float _maxWaterMoveSpeed = 200f;

	protected bool _doesSlowlyOverrideFallSpeed;

	protected float _overrideFallSpeedAmount;

	protected float _lastOverrideFallSpeedAmount;

	protected float _maxJumpTime = 0.45f;

	protected float _jumpLaunchVelocity = -500f;

	protected float _gravityAcceleration = 1500f;

	protected float _waterGravityAcceleration = 100f;

	protected float _maxFallSpeed = 300f;

	protected float _maxWaterFallSpeed = 120f;

	protected float _waterVerticalDampener = 0.2f;

	protected bool _doesBounceOnGround;

	protected float _bounceDecay = 2f;

	protected bool _doesDrawParticleSystemsUnder;

	protected bool _doesDrawParticleSystems = true;

	protected bool _doesAutomaticallyEmitParticles = true;

	public bool HasPositionChanged { get; set; }

	public bool DoesNotMakeSplashesInWater { get; set; }

	internal bool IsAffectedByTime
	{
		get
		{
			return _isAffectedByTime;
		}
		set
		{
			_isAffectedByTime = value;
		}
	}

	internal bool IsIgnoringPlatforms => _isIgnoringPlatform;

	public bool IsFrozen => _isFrozen;

	public bool DoesCollideWithTiles
	{
		get
		{
			return _doesCollideWithTiles;
		}
		protected set
		{
			_doesCollideWithTiles = value;
		}
	}

	public bool HasAppendageCollision
	{
		get
		{
			if (_appendages.Count > 0)
			{
				return _doesUseAppendageCollision;
			}
			return false;
		}
	}

	internal bool DoesCollideWithSolidEvents { get; set; }

	internal bool IsGrounded
	{
		get
		{
			return _isGrounded;
		}
		set
		{
			_isGrounded = value;
		}
	}

	internal bool WasGrounded => _wasGrounded;

	internal bool IsOnSlope
	{
		get
		{
			return _isOnSlope;
		}
		set
		{
			_isOnSlope = value;
		}
	}

	internal bool IsHittingOnHeadOnCeiling
	{
		get
		{
			return _isHittingHeadOnCeiling;
		}
		set
		{
			_isHittingHeadOnCeiling = value;
		}
	}

	internal bool IsVelocityAffectedBySlopes { get; set; }

	internal bool IsAffectedByGravity => _isAffectedByGravity;

	public virtual bool IsInWater { get; set; }

	internal bool WasInWater { get; set; }

	public virtual bool IsFacingLeft
	{
		get
		{
			return _isFacingLeft;
		}
		set
		{
			_isFacingLeft = value;
		}
	}

	internal bool IsMoonWalking { get; set; }

	public virtual bool IsFlippedVertically
	{
		get
		{
			return _isFlippedVertically;
		}
		set
		{
			_isFlippedVertically = value;
			if (!_doAppendagesMatchImageFacing)
			{
				return;
			}
			foreach (Appendage appendage in _appendages)
			{
				appendage.IsFlippedVertically = value;
			}
		}
	}

	public EGameObjectBaseType BaseType { get; protected set; }

	public int ID { get; set; }

	internal float CurrentSlopeAngle
	{
		get
		{
			return _currentSlopeAngle;
		}
		set
		{
			_currentSlopeAngle = value;
		}
	}

	public virtual Vector2 FloatPosition => _floatPosition;

	public Point LastPosition => _previousPosition;

	public virtual Point AnchorPosition => LastPosition;

	public Vector2 Velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
		}
	}

	public Vector2 StandingOnVector { get; protected set; }

	public Rectangle OuterBbox
	{
		get
		{
			if (!_outerBbox.IsEmpty)
			{
				return _outerBbox;
			}
			return Bbox;
		}
	}

	public ETeamSide DefaultTeam => _defaultTeam;

	public List<GameEvent> CurrentMovingPlatforms => _currentMovingPlatforms;

	public IEnumerable<Point> IntermediatePositions => _intermediatePositions;

	public override Point Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
			_floatPosition = new Vector2(_floatPosition.X - (float)(int)_floatPosition.X + (float)_position.X, _floatPosition.Y - (float)(int)_floatPosition.Y + (float)_position.Y);
		}
	}

	public sealed override Rectangle Bbox
	{
		get
		{
			return _bbox;
		}
		protected set
		{
			_bbox = value;
			SnapBboxToPosition();
			SnapFrameToBbox();
		}
	}

	public virtual bool IsGrabbing
	{
		get
		{
			return _isGrabbing;
		}
		protected set
		{
			if (_isGrabbing && !value)
			{
				for (int num = CurrentMovingPlatforms.Count - 1; num >= 0; num--)
				{
					GameEvent gameEvent = CurrentMovingPlatforms[num];
					if (gameEvent.IsLostWhenNotGrounded)
					{
						CurrentMovingPlatforms.RemoveAt(num);
					}
				}
			}
			_isGrabbing = value;
		}
	}

	internal List<Appendage> Appendages => _appendages;

	protected Mobile(Point inPosition, Level inLevel, int inID)
		: base(inPosition, inLevel)
	{
		_floatPosition = new Vector2(inPosition.X, inPosition.Y);
		_previousPosition = _position;
		ID = inID;
		IsVelocityAffectedBySlopes = true;
		DoesCollideWithSolidEvents = true;
	}

	public virtual void Update(float delta)
	{
		_hasSkippedASlopeThisTick = false;
		_isCollidingWithWall = false;
		_collisionPositionOffsetThisFrame = Point.Zero;
		if (CurrentMovingPlatforms.Count > 0 || StandingOnVector != Vector2.Zero)
		{
			UpdateStandingOnMovingPlatform();
		}
		UpdatePhysics(delta);
		GenerateIntermediatePositions(delta);
		if (_doesTrackTimeSinceGrounded)
		{
			_lastTimeSinceGrounded = _timeSinceGrounded;
			_timeSinceGrounded = (_isGrounded ? 0f : (_timeSinceGrounded + delta));
			_isMiracleJumping = false;
		}
		UpdateParticleSystems(delta);
		SnapBboxToPosition();
		SnapFrameToBbox();
		_isGroundColliding = false;
		WasInWater = IsInWater;
		IsInWater = false;
		UpdateAppendages(delta);
		UpdateSfxCueInstances();
	}

	protected void UpdateParticleSystems(float delta)
	{
		if (_isFrozen)
		{
			return;
		}
		for (int num = _particleSystems.Count - 1; num >= 0; num--)
		{
			if (_particleSystems[num] != null)
			{
				_particleSystems[num].Update(delta, _bbox.Center);
			}
		}
	}

	protected virtual void UpdateAppendages(float delta)
	{
		bool flag = false;
		foreach (Appendage appendage in _appendages)
		{
			if (_doAppendagesMatchImageFacing && !appendage.IsFacingLocked)
			{
				appendage.IsFacingLeft = (appendage.IsFacingOppositeParent ? (!IsFacingLeft) : IsFacingLeft);
				appendage.TimeToTurnAround = _timeToTurnAround - delta;
			}
			appendage.Update(delta);
			flag = true;
		}
		if (flag && _doesUseAppendageCollision)
		{
			UpdateOuterBbox();
		}
	}

	protected void UpdateOuterBbox()
	{
		int left = Bbox.Left;
		int top = Bbox.Top;
		int right = Bbox.Right;
		int bottom = Bbox.Bottom;
		foreach (Appendage appendage in _appendages)
		{
			if (appendage.DoesCollideWithAnything)
			{
				if (appendage.OuterBbox.Left < left)
				{
					left = appendage.OuterBbox.Left;
				}
				if (appendage.OuterBbox.Top < top)
				{
					top = appendage.OuterBbox.Top;
				}
				if (appendage.OuterBbox.Right > right)
				{
					right = appendage.OuterBbox.Right;
				}
				if (appendage.OuterBbox.Bottom > bottom)
				{
					bottom = appendage.OuterBbox.Bottom;
				}
			}
		}
		Rectangle outerBbox = new Rectangle(left, top, right - left, bottom - top);
		if (!outerBbox.Equals(Bbox))
		{
			if (!outerBbox.Equals(_outerBbox))
			{
				_outerBbox = outerBbox;
			}
		}
		else
		{
			_outerBbox = Rectangle.Empty;
		}
	}

	private void UpdateSfxCueInstances()
	{
		for (int num = _sfxCueInstances.Count - 1; num >= 0; num--)
		{
			SFXCueInstance sFXCueInstance = _sfxCueInstances[num];
			if (sFXCueInstance.IsFinished)
			{
				_sfxCueInstances.RemoveAt(num);
			}
		}
	}

	public virtual void PostCollisionUpdate()
	{
		SnapFrameToBbox();
		UpdateAppendages(0f);
	}

	private static float ComputeDragMultiplier(float dragFactor, float num)
	{
		if (dragFactor <= 0f) return 1f;
		return (float)Math.Pow(Math.Max(0.0, 1.0 - (double)dragFactor), num);
	}

	private void UpdatePhysics(float delta)
	{
		float num = delta * 60f;
		float num2 = 0f;
		if (_previousPosition != _position)
		{
			HasPositionChanged = true;
		}
		_previousPosition = new Point(_position.X, _position.Y);
		if (!_doesOverrideVelocity)
		{
			if (!_isFlying)
			{
				if (!_isOnSlope || Math.Abs(_currentSlopeAngle) <= 0f || !IsVelocityAffectedBySlopes)
				{
					_velocity.X += _movementX * _moveAcceleration * delta;
				}
				else
				{
					float num3 = _currentSlopeAngle;
					if (_currentSlopeAngle < 0f)
					{
						num3 = 0f - num3;
					}
					_velocity.X += _movementX * _moveAcceleration * delta * (_currentSlopeMovementXMultiplier / num3) * ((IsInWater && (double)num3 == 0.5) ? ComputeDragMultiplier(_waterDragFactor, num) : 1f);
				}
			}
			if (_isAffectedByGravity)
			{
				_velocity.Y += ((IsInWater && _isAffectedByWater) ? _waterGravityAcceleration : _gravityAcceleration) * delta;
				if (!_doesOverrideMaxSpeed)
				{
					if (_isAffectedByWater && IsInWater)
					{
						_velocity.Y = MathHelper.Clamp(_velocity.Y, 0f - _maxWaterFallSpeed, _maxWaterFallSpeed);
					}
					else
					{
						_velocity.Y = MathHelper.Clamp(_velocity.Y, 0f - _maxFallSpeed, _maxFallSpeed);
						if (_doesSlowlyOverrideFallSpeed)
						{
							_lastOverrideFallSpeedAmount = _overrideFallSpeedAmount;
							if (Math.Abs(_velocity.Y) >= _maxFallSpeed)
							{
								if (_velocity.Y > 0f)
								{
									_overrideFallSpeedAmount += delta * 10f;
									_velocity.Y += _overrideFallSpeedAmount * num;
								}
								else
								{
									_overrideFallSpeedAmount += delta * 100f;
									_velocity.Y -= _overrideFallSpeedAmount * num;
								}
							}
							else
							{
								_overrideFallSpeedAmount = 0f;
							}
						}
					}
				}
				_velocity.Y = DoJump(_velocity.Y, delta);
				if (_isAffectedByWater && IsInWater && _velocity.Y < 0f)
				{
					_velocity.Y *= ComputeDragMultiplier(_waterVerticalDampener, num);
				}
			}
			else
			{
				_velocity.Y += _movementY * 5000f * delta;
				if (_isAffectedByFriction)
				{
					_velocity.Y *= ComputeDragMultiplier(_airDragFactor, num);
				}
				if (!_doesOverrideMaxSpeed)
				{
					_velocity.Y = MathHelper.Clamp(_velocity.Y, 0f - _maxMoveSpeed, _maxMoveSpeed);
				}
			}
			if (_isAffectedByFriction)
			{
				if (IsInWater && _isAffectedByWater)
				{
					_velocity.X *= ComputeDragMultiplier(_waterDragFactor, num);
				}
				else if (_isGrounded && _isAffectedByGravity)
				{
					_velocity.X *= ComputeDragMultiplier(_groundDragFactor, num);
				}
				else
				{
					_velocity.X *= ComputeDragMultiplier(_airDragFactor, num);
				}
			}
			if (!_doesOverrideMaxSpeed)
			{
				if (IsInWater && _isAffectedByWater)
				{
					_velocity.X = MathHelper.Clamp(_velocity.X, 0f - _maxWaterMoveSpeed, _maxWaterMoveSpeed);
				}
				else
				{
					_velocity.X = MathHelper.Clamp(_velocity.X, 0f - _maxMoveSpeed, _maxMoveSpeed);
				}
			}
			if (_isOnSlope || _isNearSlope)
			{
				float num4 = _velocity.X * _currentSlopeAngle * 2f;
				if (num4 > 0f)
				{
					if (num4 > 300f)
					{
						num4 = 300f;
					}
					num2 = num4;
				}
			}
			if (IsGrabbing)
			{
				_isGrounded = true;
				if (_targetGrabPosition.Y != -1)
				{
					_velocity.X = 0f;
				}
				else
				{
					_velocity = Vector2.Zero;
				}
			}
		}
		_floatPosition.X = _velocity.X * delta + _floatPosition.X;
		_floatPosition.Y = (_velocity.Y + num2) * delta + _floatPosition.Y;
		if (IsGrabbing && _targetGrabPosition.Y != -1 && (float)_targetGrabPosition.Y <= _floatPosition.Y)
		{
			_velocity.Y = 0f;
			_floatPosition.Y = _targetGrabPosition.Y;
			_targetGrabPosition = new Point(-1, -1);
		}
		_position = new Point((int)Math.Floor(_floatPosition.X), (int)Math.Floor(_floatPosition.Y));
	}

	private void UpdateStandingOnMovingPlatform()
	{
		Vector2 zero = Vector2.Zero;
		int count = CurrentMovingPlatforms.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			GameEvent gameEvent = CurrentMovingPlatforms[num];
			if (!gameEvent.IsFrozen || gameEvent.CanBeUsedWhenFrozen)
			{
				zero += new Vector2(gameEvent.AmountMovedLastStep.X, gameEvent.AmountMovedLastStep.Y);
			}
			if (gameEvent.IsLostWhenNotTouching)
			{
				CurrentMovingPlatforms.RemoveAt(num);
			}
			else if (!IsGrabbing && !IsGrounded && gameEvent.IsLostWhenNotGrounded)
			{
				CurrentMovingPlatforms.RemoveAt(num);
			}
		}
		StandingOnVector = zero;
		_floatPosition.X += StandingOnVector.X;
		_floatPosition.Y += StandingOnVector.Y;
	}

	public void UngroundMe()
	{
		_wasHittingHeadOnCeiling = _isHittingHeadOnCeiling;
		if (!_wasGrounded && _isGrounded && !IsGrabbing && (!_doesTrackTimeSinceGrounded || _lastTimeSinceGrounded > 0.1f))
		{
			DoLandingAction();
		}
		_wasGrounded = _isGrounded;
		if (_previousPosition.Y == _position.Y || _previousPreviousPosition.Y == _position.Y)
		{
			return;
		}
		bool flag = false;
		if (StandingOnVector != Vector2.Zero && ((float)_previousPosition.Y + StandingOnVector.Y == (float)_position.Y || (float)_previousPosition.Y - StandingOnVector.Y == (float)_position.Y) && StandingOnVector.Y != 0f)
		{
			flag = true;
		}
		if (!flag)
		{
			_isGrounded = false;
			_isOnSlope = false;
			_isNearSlope = false;
			_isHittingHeadOnCeiling = false;
			_isStandingOnMonster = false;
			if (_wasGrounded)
			{
				_timeSinceGrounded = 0f;
			}
		}
	}

	protected virtual float DoJump(float velocityY, float delta)
	{
		return velocityY;
	}

	public void UpdatePreviousPosition()
	{
		if (_previousPosition != _position)
		{
			HasPositionChanged = true;
		}
		_previousPosition = _position;
		_previousPreviousPosition = _previousPosition;
	}

	protected void GenerateIntermediatePositions(float delta)
	{
		bool flag = false;
		int count = _intermediatePositions.Count;
		if (count > 1)
		{
			_intermediatePositions.Clear();
			flag = true;
		}
		else if (count == 0)
		{
			flag = true;
		}
		bool flag2 = false;
		if (_previousPosition != _position)
		{
			Point point = new Point(_position.X - _previousPosition.X, _position.Y - _previousPosition.Y);
			Point point2 = point;
			if (point2.X < 0)
			{
				point2.X = -point2.X;
			}
			if (point2.Y < 0)
			{
				point2.Y = -point2.Y;
			}
			int num = _bbox.Width / 2 - 1;
			int num2 = _bbox.Height / 2 - 1;
			if (num <= 0)
			{
				num = 1;
			}
			else if (num > 7)
			{
				num = 7;
			}
			if (num2 <= 0)
			{
				num2 = 1;
			}
			else if (num2 > 7)
			{
				num2 = 7;
			}
			if (point2.X >= num || point2.Y >= num2)
			{
				int num3 = ((point2.X <= point2.Y) ? ((int)Math.Floor((float)point2.Y / (float)num2)) : ((int)Math.Floor((float)point2.X / (float)num)));
				if (num3 > 30)
				{
					num3 = 30;
				}
				_intermediatePositions.Clear();
				for (int i = 0; i < num3; i++)
				{
					float num4 = (float)(i + 1) / (float)(num3 + 1);
					Point item = new Point(_previousPosition.X + (int)((float)point.X * num4), _previousPosition.Y + (int)((float)point.Y * num4));
					_intermediatePositions.Add(item);
				}
				_intermediatePositions.Add(_position);
			}
			else
			{
				flag2 = true;
			}
		}
		else
		{
			flag2 = true;
		}
		if (flag2)
		{
			if (flag)
			{
				_intermediatePositions.Add(_position);
			}
			else
			{
				_intermediatePositions[0] = _position;
			}
		}
	}

	public virtual bool DetectTileCollisions()
	{
		bool flag = false;
		UngroundMe();
		_tileCollisionBbox = Bbox;
		if (_doesUse16X16TileCollisionBbox)
		{
			int num = Math.Max(Bbox.Height, 16);
			_tileCollisionBbox = new Rectangle(Position.X - 8, Position.Y - num, 16, num);
		}
		int num2 = (int)Math.Floor((float)_tileCollisionBbox.Left / 16f);
		int num3 = (int)Math.Ceiling((float)_tileCollisionBbox.Right / 16f) - 1;
		int num4 = (int)Math.Floor((float)_tileCollisionBbox.Top / 16f);
		int num5 = (int)Math.Ceiling((float)_tileCollisionBbox.Bottom / 16f) - 1;
		Dictionary<Point, Tile> solidTiles = _level.SolidTiles;
		Dictionary<Point, WaterTile> waterTiles = _level.WaterTiles;
		for (int i = num4; i <= num5; i++)
		{
			for (int j = num2; j <= num3; j++)
			{
				Tile tile = _level.GetSolidTileFast(j, i);
				if (tile == null && solidTiles != null && solidTiles.TryGetValue(new Point(j, i), out Tile fallbackTile))
				{
					tile = fallbackTile;
				}
				if (tile != null)
				{
					Vector2 intersectionDepth = _tileCollisionBbox.GetIntersectionDepth(tile.Bbox);
					bool flag2 = tile.TileIndex != -1 || _isAffectedByLevelBounds;
					if (intersectionDepth != Vector2.Zero && flag2)
					{
						flag = CollideSolidTile(tile, intersectionDepth) || flag;
					}
				}
				WaterTile waterTile = _level.GetWaterTileFast(j, i);
				if (waterTile == null && waterTiles != null && waterTiles.TryGetValue(new Point(j, i), out WaterTile fallbackWater))
				{
					waterTile = fallbackWater;
				}
				if (waterTile != null)
				{
					Vector2 intersectionDepth2 = _tileCollisionBbox.GetIntersectionDepth(waterTile.Bbox);
					if (intersectionDepth2 != Vector2.Zero)
					{
						waterTile.MobileCollided(this, intersectionDepth2);
					}
				}
				if (flag && _doesUse16X16TileCollisionBbox)
				{
					int num6 = Math.Max(Bbox.Height, 16);
					_tileCollisionBbox = new Rectangle(Position.X - 8, Position.Y - num6, 16, num6);
				}
				else if (flag)
				{
					_tileCollisionBbox = Bbox;
				}
			}
		}
		if (_isAffectedByLevelBounds)
		{
			if (Bbox.Left < 0)
			{
				_velocity.X = 0f;
				Position = new Point(_bbox.Width / 2, _position.Y);
				flag = true;
				_isCollidingWithWall = true;
			}
			else if (Bbox.Right > _level.RoomSize.X)
			{
				_velocity.X = 0f;
				Position = new Point(_level.RoomSize.X - _bbox.Width / 2, _position.Y);
				flag = true;
				_isCollidingWithWall = true;
			}
			if (Bbox.Top < -24)
			{
				if (_velocity.Y < 0f)
				{
					_velocity.Y = 0f;
				}
				Position = new Point(_position.X, _bbox.Height - 24);
				_isHittingHeadOnCeiling = true;
				flag = true;
			}
		}
		return flag;
	}

	public virtual bool CollideSolidObject(Animate target, ETileType tileType, Vector2 depth)
	{
		bool result = false;
		if (!_isBeingSquished && _objectThatSquishedUs != target)
		{
			result = true;
			Rectangle bbox = target.Bbox;
			bool flag = Math.Abs(depth.Y) < Math.Abs(depth.X);
			if (flag)
			{
				Point newPosition = new Point(_position.X, _position.Y + (int)depth.Y);
				if (depth.Y < 0f)
				{
					_isGrounded = true;
					switch (tileType)
					{
					case ETileType.Event:
						if (target is GameEvent)
						{
							AddMovingPlatform(target as GameEvent);
						}
						break;
					case ETileType.Monster:
						_isStandingOnMonster = true;
						break;
					}
					_velocity.Y = 0f;
				}
				else if (depth.Y > 0f)
				{
					Point key = new Point(newPosition.X / 16, newPosition.Y / 16);
					if (_level.SolidTiles.ContainsKey(key))
					{
						flag = false;
					}
					else
					{
						_isHittingHeadOnCeiling = true;
						if (_velocity.Y < 0f)
						{
							_velocity.Y = 0f;
						}
					}
				}
				if (flag)
				{
					CollisionSetPosition(newPosition, target);
				}
			}
			if (!flag)
			{
				float num = (target.CannotBeGrabbed ? ((float)target.Bbox.Y + depth.Y) : ((float)bbox.Top));
				Point tileKey = ((!(depth.X < 0f)) ? new Point((int)Math.Ceiling((float)bbox.Right / 16f), (int)Math.Floor(num / 16f)) : new Point((int)Math.Floor((float)bbox.Left / 16f), (int)Math.Floor(num / 16f)));
				HandleHorizontalCollision(target, tileKey, tileType, depth);
			}
			SnapBboxToPosition();
			SnapFrameToBbox();
		}
		return result;
	}

	public virtual bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool flag = Math.Abs(depth.Y) < Math.Abs(depth.X);
		bool result = true;
		if (flag || tile.Type == ETileType.Slope || tile.Type == ETileType.Platform)
		{
			if (tile.Type == ETileType.Solid)
			{
				result = HandleSquareTileCollision(tile, depth);
			}
			else if (tile.Type == ETileType.Slope)
			{
				result = HandleSlopeCollision(tile, depth, flag);
			}
			else if (tile.Type == ETileType.Platform)
			{
				result = HandlePlatformTileCollision(tile, depth);
			}
		}
		else if (tile.Type == ETileType.Solid)
		{
			result = HandleHorizontalCollision(tile, tile.DictKey, tile.Type, depth);
			if (tile.Special == ETileSpecialType.HorizontalSpike && ManageDamage(5, new Vector2(3f * depth.X, 0f), _tileCollisionBbox.Center, tile.Bbox, EDamageType.Spike, EDamageElement.None, doesKnockBack: true))
			{
				result = true;
				int x = ((depth.X > 0f) ? _tileCollisionBbox.Left : _tileCollisionBbox.Right);
				_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, new Point(x, tile.Bbox.Center.Y), _level)
				{
					TeamSide = DefaultTeam,
					IsFacingLeft = IsFacingLeft,
					AnimationSpeed = 0.03f,
					AnimationStart = 15,
					AnimationLength = 4
				});
			}
		}
		SnapBboxToPosition();
		SnapFrameToBbox();
		return result;
	}

	private bool HandlePlatformTileCollision(Tile tile, Vector2 depth)
	{
		bool result = false;
		if (depth.Y < 0f && _previousPosition.Y <= tile.Bbox.Top && !_isIgnoringPlatform)
		{
			result = true;
			_isGrounded = true;
			CollisionSetPosition(new Point(_position.X, _position.Y + (int)depth.Y));
			_velocity.Y = 0f;
		}
		return result;
	}

	private bool HandleSquareTileCollision(Tile tile, Vector2 depth)
	{
		if ((!(depth.Y < 0f) || !_level.CheckNearbyIgnoreSlopes(EDirection.North, tile.DictKey)) && (!(depth.Y > 0f) || !_level.CheckNearbyIgnoreSlopes(EDirection.South, tile.DictKey)))
		{
			if (depth.Y < 0f)
			{
				_isGrounded = true;
				if (_doesBounceOnGround && !_wasGrounded)
				{
					_velocity.Y = (0f - _velocity.Y) / _bounceDecay;
					if ((_velocity.Y < 0f && _velocity.Y > -1f) || (_velocity.Y > 0f && _velocity.Y < 1f))
					{
						_velocity.Y = 0f;
					}
					DoBounce(new Point(Position.X, tile.Bbox.Top));
				}
				else
				{
					_velocity.Y = 0f;
				}
				if (_level.CheckNearbyType(EDirection.East, tile.DictKey) == ETileType.Slope && depth.X < 8f && depth.X > 0f)
				{
					Tile tile2 = _level.SolidTiles[new Point(tile.DictKey.X + 1, tile.DictKey.Y)];
					int num = tile2.LookupTileHeight(_position.X);
					num += tile.Bbox.Top;
					CollisionSetPosition(new Point(_position.X, num));
					_isOnSlope = true;
					_currentSlopeAngle = tile2.GetSlopeAngle();
				}
				else if (_level.CheckNearbyType(EDirection.West, tile.DictKey) == ETileType.Slope && depth.X > -8f && depth.X < 0f)
				{
					Tile tile3 = _level.SolidTiles[new Point(tile.DictKey.X - 1, tile.DictKey.Y)];
					int num2 = tile3.LookupTileHeight(_position.X);
					num2 += tile.Bbox.Top;
					CollisionSetPosition(new Point(_position.X, num2));
					_isOnSlope = true;
					_currentSlopeAngle = tile3.GetSlopeAngle();
				}
				else
				{
					CollisionSetPosition(new Point(_position.X, _position.Y + (int)depth.Y));
				}
			}
			else if (depth.Y > 0f)
			{
				_isHittingHeadOnCeiling = true;
				CollisionSetPosition(new Point(_position.X, _position.Y + (int)depth.Y));
				if (_velocity.Y < 0f)
				{
					_velocity.Y = 0f;
				}
			}
			if (tile.Special == ETileSpecialType.VerticalSpike)
			{
				int num3 = ((depth.Y > 0f) ? 1 : (-1));
				int num4 = (IsFacingLeft ? 1 : (-1));
				bool flag = _level.ID == 11;
				if (ManageDamage(5, new Vector2(0.5f * (float)num4, 90 * num3), new Point(Bbox.Center.X, (depth.Y > 0f) ? tile.Bbox.Bottom : tile.Bbox.Top), tile.Bbox, EDamageType.Spike, EDamageElement.None, !flag))
				{
					Point point = new Point(Position.X, tile.Bbox.Top);
					_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, point, _level)
					{
						TeamSide = DefaultTeam,
						IsFacingLeft = IsFacingLeft,
						AnimationSpeed = 0.03f,
						AnimationStart = 15,
						AnimationLength = 4
					});
					if (!flag)
					{
						PlayCue(ESFX.FoleySpikeDamage, point);
					}
				}
			}
		}
		return true;
	}

	private bool HandleSlopeCollision(Tile tile, Vector2 depth, bool isYShallowAxis)
	{
		bool result = false;
		bool isFlippedVertically = tile.IsFlippedVertically;
		int num = tile.LookupTileHeight(_position.X);
		int num2 = num - tile.Bbox.Bottom;
		if (!isFlippedVertically)
		{
			if (num >= 0 && _position.Y >= num2)
			{
				if (_tileCollisionBbox.Height < 16 && depth.Y >= (float)_tileCollisionBbox.Height)
				{
					depth.Y = -25f + depth.Y;
				}
				if ((!(depth.Y < 0f) || !_level.CheckNearby(EDirection.North, tile.DictKey)) && (!(depth.Y > 0f) || !_level.CheckNearby(EDirection.South, tile.DictKey)))
				{
					if (depth.Y + (float)num <= 0f || (_hasSkippedASlopeThisTick && depth.Y <= 0f && _velocity.Y > 0f))
					{
						_isGrounded = true;
						_isOnSlope = true;
						result = true;
						_currentSlopeAngle = tile.GetSlopeAngle();
						CollisionSetPosition(new Point(_position.X, _position.Y + (int)(depth.Y + (float)num)));
						if (_doesBounceOnGround)
						{
							if (_velocity.Y > 0f)
							{
								_velocity.X = _currentSlopeAngle * (100f + _velocity.Y);
								_velocity.Y = (0f - _velocity.Y) / _bounceDecay * 1.25f;
								if ((_velocity.Y < 0f && _velocity.Y > -1f) || (_velocity.Y > 0f && _velocity.Y < 1f))
								{
									_velocity.Y = 0f;
								}
							}
						}
						else
						{
							_velocity.Y = 0f;
						}
					}
					else if (depth.Y > 0f)
					{
						if (isYShallowAxis)
						{
							_velocity.Y = 0f;
							_isHittingHeadOnCeiling = true;
							result = true;
							CollisionSetPosition(new Point(_position.X, _position.Y + (int)depth.Y));
						}
						else
						{
							result = HandleHorizontalCollision(tile, tile.DictKey, tile.Type, depth);
						}
					}
					else
					{
						_hasSkippedASlopeThisTick = true;
					}
					if (_intermediatePositions.Count <= 2 && _shouldIgnoreIntermediateSlopes && Math.Abs(Velocity.X) < 200f)
					{
						result = false;
					}
				}
			}
			else
			{
				int num3 = ((_position.X > tile.Bbox.Center.X) ? tile.LookupTileHeight(tile.Bbox.Right) : tile.LookupTileHeight(tile.Bbox.Left)) - 16;
				if (_position.Y - 16 >= tile.Bbox.Bottom + num3)
				{
					result = HandleHorizontalCollision(tile, tile.DictKey, tile.Type, depth);
				}
			}
		}
		else
		{
			int num4 = _position.Y - Bbox.Height;
			int num5 = tile.Bbox.Bottom - num;
			bool flag = num > 0 && num4 < num5;
			if (depth.Y < 0f)
			{
				if (isYShallowAxis)
				{
					_isGrounded = true;
					_isOnSlope = false;
					result = true;
					_velocity.Y = 0f;
					CollisionSetPosition(new Point(_position.X, _position.Y + (int)depth.Y));
				}
				else
				{
					result = HandleHorizontalCollision(tile, tile.DictKey, tile.Type, depth);
				}
			}
			else if (flag && (!(depth.Y < 0f) || !_level.CheckNearby(EDirection.North, tile.DictKey)) && (!(depth.Y > 0f) || !_level.CheckNearby(EDirection.South, tile.DictKey)) && depth.Y - (float)num >= 0f)
			{
				_isHittingHeadOnCeiling = true;
				result = true;
				CollisionSetPosition(new Point(_position.X, _position.Y + (int)(depth.Y - (float)num)));
				if (_velocity.Y < 0f)
				{
					_velocity.Y = 0f;
				}
			}
		}
		return result;
	}

	protected virtual bool HandleHorizontalCollision(GameObject target, Point tileKey, ETileType tileType, Vector2 depth)
	{
		bool result = false;
		GameObject source = ((tileType == ETileType.Event) ? target : null);
		if ((!_level.CheckIfHorizontallyAdjacentTilesBlock((depth.X > 0f) ? EDirection.East : EDirection.West, tileKey, target, _position) || tileType == ETileType.Event) && (_level.CheckNearbyType(EDirection.North, tileKey) != ETileType.Slope || !_isOnSlope || !(depth.Y >= -3f)))
		{
			result = true;
			CollisionSetPosition(new Point(_position.X + (int)depth.X, _position.Y), source);
			if (_doesBounceOnGround)
			{
				_velocity.X = (0f - _velocity.X) / _bounceDecay;
				if ((_velocity.X < 0f && _velocity.X > -1f) || (_velocity.X > 0f && _velocity.X < 1f))
				{
					_velocity.X = 0f;
				}
			}
			else
			{
				_velocity.X = 0f;
			}
			_isCollidingWithWall = true;
		}
		return result;
	}

	protected virtual void DoBounce(Point impactPoint)
	{
	}

	public bool DoesCollideWith(Rectangle target)
	{
		return DoesCollideWith(target, checkAppendages: false);
	}

	public bool DoesCollideWith(Rectangle target, bool checkAppendages)
	{
		bool result = false;
		if (_outerBbox.IsEmpty)
		{
			result = target.Intersects(Bbox);
		}
		else if (target.Intersects(OuterBbox))
		{
			result = !checkAppendages || FindIntersectingBoundingBoxes(target, 1).Count > 0;
		}
		return result;
	}

	protected List<Rectangle> FindIntersectingBoundingBoxes(Rectangle target, int findAtLeast)
	{
		List<Rectangle> list = new List<Rectangle>();
		if (_outerBbox.IsEmpty && target.Intersects(Bbox))
		{
			list.Add(Bbox);
		}
		else
		{
			int num = 0;
			if (target.Intersects(Bbox))
			{
				list.Add(Bbox);
				num++;
			}
			foreach (Appendage appendage in _appendages)
			{
				if (appendage.DoesCollideWithAnything && (num < findAtLeast || findAtLeast == -1))
				{
					List<Rectangle> list2 = appendage.FindIntersectingBoundingBoxes(target, findAtLeast - num);
					list.AddRange(list2);
					num += list2.Count;
				}
			}
		}
		return list;
	}

	public Rectangle GetCollidingRectangle(Projectile target)
	{
		Rectangle result = Rectangle.Empty;
		if (target.HasAppendageCollision)
		{
			if (DoesCollideWith(target.OuterBbox, checkAppendages: true))
			{
				foreach (Appendage appendage in target.Appendages)
				{
					if (DoesCollideWith(appendage.Bbox, checkAppendages: true))
					{
						result = appendage.Bbox;
						break;
					}
				}
			}
		}
		else if (DoesCollideWith(target.DamageBbox, checkAppendages: true))
		{
			result = target.DamageBbox;
		}
		return result;
	}

	protected void CollisionSetPosition(Point newPosition)
	{
		CollisionSetPosition(newPosition, null);
	}

	internal void CollisionSetPosition(Point newPosition, GameObject source)
	{
		bool flag = false;
		Point point = new Point(newPosition.X - Position.X, newPosition.Y - Position.Y);
		if (source != null && ((point.X != 0 && _collisionPositionOffsetThisFrame.X != 0 && point.X > 0 != _collisionPositionOffsetThisFrame.X > 0) || (point.Y != 0 && _collisionPositionOffsetThisFrame.Y != 0 && point.Y > 0 != _collisionPositionOffsetThisFrame.Y > 0)))
		{
			if (point.Y > 1 || point.Y < -1)
			{
				ManageSquishedDamage(source);
			}
			flag = true;
		}
		if (!flag)
		{
			_collisionPositionOffsetThisFrame = new Point(_collisionPositionOffsetThisFrame.X + point.X, _collisionPositionOffsetThisFrame.Y + point.Y);
			Position = newPosition;
		}
	}

	internal virtual void ManageSquishedDamage(GameObject whoDunIt)
	{
		if (whoDunIt.SquishDamage > 0)
		{
			ManageDamage(whoDunIt.SquishDamage, Vector2.Zero, Bbox.Center, whoDunIt.Bbox, EDamageType.Squished, EDamageElement.None, doesKnockBack: false);
			_isBeingSquished = true;
			_objectThatSquishedUs = whoDunIt;
			_level.AddAnimation(EBattleAnimationType.MediumHitYellow, Bbox.Center, DefaultTeam);
		}
	}

	public virtual bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		return false;
	}

	public void SnapFrameToBbox()
	{
		_drawPosition = new Point(Bbox.Left - BboxOffset.X, Bbox.Top - BboxOffset.Y);
	}

	public virtual void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	public virtual void ManageState(EAFSM action)
	{
	}

	public virtual void Freeze()
	{
		if (!_isAffectedByTime)
		{
			return;
		}
		_isFrozen = true;
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			if (!sfxCueInstance.IsFinished)
			{
				sfxCueInstance.Freeze();
			}
		}
	}

	public virtual void Unfreeze()
	{
		_isFrozen = false;
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			if (!sfxCueInstance.IsFinished)
			{
				sfxCueInstance.Unfreeze();
			}
		}
	}

	public virtual void Kill()
	{
		SilentKill();
	}

	public virtual void SilentKill()
	{
		_level.RequestRemoveObject(this);
		StopAllSFX();
	}

	internal void StopAllSFX()
	{
		foreach (SFXCueInstance sfxCueInstance in _sfxCueInstances)
		{
			if (sfxCueInstance != null && !sfxCueInstance.IsFinished)
			{
				sfxCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.None;
				sfxCueInstance.Stop(0.25f);
			}
		}
	}

	public SFXCueInstance PlayCue(ESFX cue, Point position)
	{
		return PlayCue(cue, position, isLooped: false, 1f);
	}

	public SFXCueInstance PlayCue(ESFX cue, Point position, bool isLooped)
	{
		return PlayCue(cue, position, isLooped, 1f);
	}

	public SFXCueInstance PlayCue(ESFX cue, Point position, bool isLooped, float chance)
	{
		SFXCueInstance sFXCueInstance = null;
		if ((chance >= 1f || _level.NextRandomDouble() < (double)chance) && (isLooped || _level.IsWithinCameraDistance(position)))
		{
			sFXCueInstance = _level.PlayCue(cue, position, isLooped);
			if (sFXCueInstance != null)
			{
				sFXCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Stationary;
				_sfxCueInstances.Add(sFXCueInstance);
				if (IsFrozen)
				{
					sFXCueInstance.Freeze();
				}
			}
		}
		return sFXCueInstance;
	}

	public SFXCueInstance PlayCue2D(ESFX cue)
	{
		SFXCueInstance sFXCueInstance = _level.PlayCue(cue);
		if (sFXCueInstance != null)
		{
			_sfxCueInstances.Add(sFXCueInstance);
			if (IsFrozen)
			{
				sFXCueInstance.Freeze();
			}
		}
		return sFXCueInstance;
	}

	public SFXCueInstance PlayCue(ESFX cue)
	{
		return PlayCue(cue, isLooped: false, 1f);
	}

	public SFXCueInstance PlayCue(ESFX cue, bool isLooped)
	{
		return PlayCue(cue, isLooped, 1f);
	}

	public SFXCueInstance PlayCue(ESFX cue, bool isLooped, float chance)
	{
		SFXCueInstance sFXCueInstance = PlayCue(cue, Position, isLooped, chance);
		if (sFXCueInstance != null)
		{
			sFXCueInstance.Anchor = this;
			sFXCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
		}
		return sFXCueInstance;
	}

	public SFXCueInstance CreateCue(ESFX cue, Point position, bool isLooped)
	{
		SFXCueInstance sFXCueInstance = _level.CreateCue(cue, position, isLooped);
		if (sFXCueInstance != null)
		{
			sFXCueInstance.Anchor = this;
			sFXCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
			_sfxCueInstances.Add(sFXCueInstance);
		}
		return sFXCueInstance;
	}

	public SFXCueInstance CreateCue2D(ESFX cue, bool isLooped)
	{
		SFXCueInstance sFXCueInstance = _level.CreateCue2D(cue, isLooped);
		if (sFXCueInstance != null)
		{
			_sfxCueInstances.Add(sFXCueInstance);
		}
		return sFXCueInstance;
	}

	public bool IsOutsideOfLevel()
	{
		if (Position.X >= 0 && Position.Y >= 0 && Position.X <= _level.RoomSize.X)
		{
			return Position.Y > _level.RoomSize.Y;
		}
		return true;
	}

	protected virtual void DoLandingAction()
	{
	}

	protected virtual void DoHorizontalRun(float moveAmount)
	{
		ManageState(EAFSM.Moving);
		bool flag = moveAmount < 0f;
		if (IsMoonWalking)
		{
			flag = !flag;
		}
		IsFacingLeft = flag;
		_movementX = moveAmount;
	}

	protected virtual void DoVerticalJump()
	{
	}

	public void AddMovingPlatform(GameEvent platform)
	{
		bool flag = true;
		foreach (GameEvent currentMovingPlatform in CurrentMovingPlatforms)
		{
			if (currentMovingPlatform.EventType == platform.EventType)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			CurrentMovingPlatforms.Add(platform);
		}
	}
}
