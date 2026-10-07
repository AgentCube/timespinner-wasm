using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.Items;

namespace Timespinner.GameObjects.BaseClasses;

public class Monster : Alive
{
	private const int RunForeverBoundaryCheckX = 8;

	internal const int DefaultAggroWidth = 300;

	internal const int DefaultAggroHeight = 150;

	internal const int DefaultDeaggroWidth = 800;

	internal const int DefaultDeaggroHeight = 300;

	private const float DefaultAbilityTime = 10f;

	private const float GemDropRarity = 10f;

	public const string DeathCounterKeyPrefix = "KILL_";

	public const string ItemDropKeyPrefix = "DROP_";

	private const float TimeToRecoil = 0.03334f;

	private readonly EEnemyTileType _enemyTileType;

	private readonly int _argument;

	private readonly int _experienceGiven;

	private readonly BestiaryEntrySpecification _bestiaryEntry;

	private bool _wasFacingLeft;

	private bool _hasShownNameYet;

	protected bool _doesIgnoreOutOfBoundsDeath;

	protected bool _canBeDamaged = true;

	protected EAIStrategy _currentAI;

	protected EAIStrategy _nonAggroStrategy;

	protected EAIAction _currentAction;

	protected EAIAction _lastAction;

	protected int _currentCustomAction;

	protected int _selectedAbility;

	protected bool _isCarryingOutAbility;

	protected bool _wasCarryingOutAbility;

	protected float _abilityTimer;

	protected float _lastAbilityTimer;

	protected float _totalAbilityTime;

	protected EAIMovementType _movementType;

	protected EAIAction _nonAggroAction = EAIAction.Idle;

	protected int _attackDistanceThresholdX = 180;

	protected int _attackDistanceThresholdY = -200;

	protected int _retreatDistanceThresholdX;

	protected bool _isRecoiling;

	protected float _recoilTimer;

	protected bool _wasStartingImageFacingLeft;

	protected bool _doesBounceOffWall;

	protected bool _isBoss;

	protected bool _isBossMusicPlaying;

	protected int _damageCaused = 1;

	protected float _paceLength;

	protected float _nextActionTimer;

	protected float _lastActionTimer;

	protected float _totalActionTimer;

	protected float _timeToWaitAfterArriving = 0.6f;

	protected bool _isFinallyDead;

	protected bool _isRunningDeathScript;

	protected bool _doesDeathScriptIgnoreFrozen;

	protected float _deathScriptTimer;

	protected bool _isAlreadyTouchingHero;

	protected bool _doesDropBasicLoot = true;

	protected bool _isAfraidOfFalling;

	protected bool _isAfraidOfMoving;

	protected bool _isAfraidOfAttackingNearCliff;

	protected bool _isAtTargetLocation;

	protected bool _isAfraidOfBeingTooClose;

	protected float _followTimer;

	protected float _oscillationMultiplierX = 1f;

	protected float _oscillationMultiplierY = 1f;

	protected Random _random;

	protected float _timeToIdleAfterAttacking = 1f;

	protected float _timeToIdleAfterMoving = 0.025f;

	protected float _timeToMove = 0.2f;

	protected bool _isAggroed;

	protected bool _wasAggroed;

	protected bool _canLoseAggro = true;

	protected bool _isAlwaysAggroed;

	protected bool _doesDrawAggroBbox;

	protected bool _doesAggroOnTakingDamage = true;

	private Point _aggroBboxDimensions = new Point(300, 150);

	private Point _deaggroBboxDimensions = new Point(800, 300);

	private Rectangle _aggroBbox;

	private Rectangle _deaggroBbox;

	public bool IsDead { get; internal set; }

	public bool HasHeroInAggroBbox { get; private set; }

	public bool HasHeroInDeaggroBbox { get; private set; }

	public bool IsTargetBehindMe
	{
		get
		{
			if (_currentTarget != null)
			{
				return Position.X < _currentTarget.Position.X == IsFacingLeft;
			}
			return false;
		}
	}

	public bool CanBeDamaged
	{
		get
		{
			if (_canBeDamaged && base.HP > 0)
			{
				return !base.IsFrozen;
			}
			return false;
		}
	}

	internal bool IsMinion { get; set; }

	internal bool DoesTouchDamageKnockback { get; set; }

	internal bool IsSolidWhenFrozen { get; set; }

	internal bool IsAggroed => _isAggroed;

	internal bool IsDormant { get; set; }

	internal bool IsImmuneToSpikes { get; set; }

	internal bool DoesNotTurnToFacePlayer { get; set; }

	internal bool DoesRecoil { get; set; }

	public EEnemyTileType EnemyType => _enemyTileType;

	internal float GoToOscillationMultiplier { get; set; }

	internal Point AggroBboxOffset { get; set; }

	internal Point FollowAttackTargetOffset { get; set; }

	public Point AggroBboxDimensions
	{
		get
		{
			return _aggroBboxDimensions;
		}
		protected set
		{
			_aggroBboxDimensions = value;
			_aggroBbox.Width = value.X;
			_aggroBbox.Height = value.Y;
		}
	}

	public Point DeaggroBboxDimensions
	{
		get
		{
			return _deaggroBboxDimensions;
		}
		protected set
		{
			_deaggroBboxDimensions = value;
			_deaggroBbox.Width = value.X;
			_deaggroBbox.Height = value.Y;
		}
	}

	public Rectangle AggroBbox
	{
		get
		{
			return _aggroBbox;
		}
		protected set
		{
			_aggroBbox = value;
		}
	}

	public Rectangle DeaggroBbox
	{
		get
		{
			return _deaggroBbox;
		}
		protected set
		{
			_deaggroBbox = value;
		}
	}

	public Animate HeroWhoAggroedMe { get; private set; }

	public int Damage => _damageCaused;

	public int ExperienceGiven => _experienceGiven;

	public Monster(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID)
	{
		_doesHarmOnTouch = true;
		base.BaseType = EGameObjectBaseType.Monster;
		_random = new Random(inPosition.X * inPosition.Y);
		AggroBbox = new Rectangle(0, 0, _aggroBboxDimensions.X, _aggroBboxDimensions.Y);
		DeaggroBbox = new Rectangle(0, 0, _deaggroBboxDimensions.X, _deaggroBboxDimensions.Y);
		_defaultTeam = ETeamSide.Enemies;
		IsSolidWhenFrozen = true;
		IsDormant = false;
		_agility = 3f;
		_timeToTurnAround = 0.05f;
		GoToOscillationMultiplier = 100f;
		FollowAttackTargetOffset = Point.Zero;
		base.DoesDrawWhenOutsideOfObjectVisibleArea = false;
		DoesRecoil = true;
		if (objectSpec != null)
		{
			_enemyTileType = objectSpec.GetEnemyType();
			_argument = objectSpec.Argument;
			_bestiaryEntry = _level.GCM.Bestiary.GetEntry(_enemyTileType, _argument);
		}
		if (_bestiaryEntry != null)
		{
			_experienceGiven = _bestiaryEntry.Exp;
			_damageCaused = _bestiaryEntry.TouchDamage;
			base.MaxHP = _bestiaryEntry.HP;
			base.HP = base.MaxHP;
		}
		base.CharacterSpecification = _level.GetCharacterSpecification(objectSpec);
		if (base.CharacterSpecification != null && base.CharacterSpecification.CoreAppendage != null)
		{
			CreateAppendagesFromSpecification(base.CharacterSpecification.CoreAppendage);
		}
	}

	internal void SetAgility(float agility)
	{
		_agility = agility;
	}

	public override void SnapBboxToPosition()
	{
		base.SnapBboxToPosition();
		SnapAggroBboxToBbox();
	}

	protected void SnapAggroBboxToBbox()
	{
		_aggroBbox.Location = new Point(_bbox.Center.X - _aggroBboxDimensions.X / 2 + AggroBboxOffset.X, _bbox.Center.Y - _aggroBboxDimensions.Y / 2 + AggroBboxOffset.Y);
		_deaggroBbox.Location = new Point(_bbox.Center.X - _deaggroBboxDimensions.X / 2 + AggroBboxOffset.X, _bbox.Center.Y - _deaggroBboxDimensions.Y / 2 + AggroBboxOffset.Y);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_doesDrawAggroBbox)
		{
			spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)AggroBbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)AggroBbox.Y)), AggroBbox.Width, AggroBbox.Height), null, new Color(150, 50, 50, 50));
			spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)DeaggroBbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)DeaggroBbox.Y)), DeaggroBbox.Width, DeaggroBbox.Height), null, new Color(50, 50, 150, 50));
		}
	}

	public override void Update(float delta)
	{
		if (_isRunningDeathScript && (!_isFrozen || _doesDeathScriptIgnoreFrozen))
		{
			UpdateDeathScript(delta);
			if (_doesDeathScriptIgnoreFrozen)
			{
				base.Update(delta);
			}
		}
		else
		{
			if (_isInvulnerable && !_isAlwaysInvulnerable)
			{
				_invulnerableTimer -= delta;
				if (_invulnerableTimer <= 0f)
				{
					_isInvulnerable = false;
				}
			}
			if (!_isFrozen)
			{
				if (_isRecoiling)
				{
					UpdateRecoil(delta);
					UpdateParticleSystems(delta);
				}
				else
				{
					UpdateMonsterAI(delta);
					base.Update(delta);
					if (IsFacingLeft != _wasFacingLeft)
					{
						DoTurningAroundAnimation(_currentState, _wasFacingLeft);
					}
					_wasJumping = _isJumping;
					_wasCarryingOutAbility = _isCarryingOutAbility;
					_wasFacingLeft = IsFacingLeft;
				}
			}
			else if (_isDeadButFrozen)
			{
				base.DrawColor = Color.Crimson;
			}
			KillIfOutOfBounds();
			HasHeroInAggroBbox = false;
			HasHeroInDeaggroBbox = false;
		}
		UpdateIsWithinObjectVisibleArea();
	}

	internal void UpdateMonsterAI(float delta)
	{
		if (_isCarryingOutAbility)
		{
			_lastAbilityTimer = _abilityTimer;
			_abilityTimer += delta;
			if (_abilityTimer >= _totalAbilityTime)
			{
				_isCarryingOutAbility = false;
			}
			else
			{
				UpdateAbility(delta);
			}
		}
		if (_isCarryingOutAbility)
		{
			return;
		}
		_lastActionTimer = _nextActionTimer;
		_nextActionTimer -= delta;
		if (_nextActionTimer <= 0f || !_isAggroed)
		{
			DetermineAggroStatus(delta);
			if (_nextActionTimer <= 0f)
			{
				_lastAction = _currentAction;
				DetermineAction(delta);
			}
		}
		DoAction(delta);
	}

	protected virtual void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.Boom, _bbox.Center, ETeamSide.Enemies);
		foreach (Appendage appendage in _appendages)
		{
			appendage.Kill();
		}
		DropLootAndRemove();
	}

	protected virtual void DetermineAction(float delta)
	{
		EAIStrategy eAIStrategy = _currentAI;
		if (!_isAggroed)
		{
			eAIStrategy = _nonAggroStrategy;
			_currentAction = _nonAggroAction;
			_nextActionTimer = 0.1f;
		}
		switch (eAIStrategy)
		{
		case EAIStrategy.Wander:
			if (_random.Next(2) == 0)
			{
				if (_movementType != EAIMovementType.Fly)
				{
					IsFacingLeft = _random.Next(2) == 1;
					_isMovingLeft = IsFacingLeft;
					_currentAction = EAIAction.Move;
				}
				else
				{
					int where = _random.Next(1, 8);
					Point pointFromDirection = Level.GetPointFromDirection(Point.Zero, (EDirection)where);
					_currentAction = EAIAction.GoTowards;
					_targetPosition = new Point(Position.X + pointFromDirection.X * 64, Position.Y + pointFromDirection.Y * 64);
					_startPosition = _position;
					_nextActionTimer = _timeToMove;
					_totalActionTimer = _nextActionTimer;
					_followTimer = 0f;
				}
				_nextActionTimer = (float)_random.Next(5, 10) * 0.1f;
			}
			else
			{
				_currentAction = ((_movementType != EAIMovementType.Fly) ? EAIAction.Idle : EAIAction.FloatInPlace);
				_nextActionTimer = (float)_random.Next(30, 40) * 0.1f;
			}
			break;
		case EAIStrategy.Pace:
			if (_velocity.X == 0f)
			{
				_currentAction = EAIAction.Move;
				_nextActionTimer = _paceLength / 2f;
				break;
			}
			_currentAction = EAIAction.Move;
			_nextActionTimer = _paceLength;
			IsFacingLeft = !IsFacingLeft;
			_isMovingLeft = IsFacingLeft;
			break;
		case EAIStrategy.FlyTowards:
			if (_currentAction == _nonAggroAction)
			{
				Point nearestProtagonistPosition3 = _level.GetNearestProtagonistPosition(_position);
				nearestProtagonistPosition3 = OvershootTarget(nearestProtagonistPosition3);
				IsFacingLeft = nearestProtagonistPosition3.X < _position.X;
				_currentAction = EAIAction.GoTowards;
				_targetPosition = nearestProtagonistPosition3;
				_startPosition = _position;
				_nextActionTimer = _timeToMove;
				_totalActionTimer = _nextActionTimer;
				_followTimer = 0f;
			}
			else
			{
				_currentAction = _nonAggroAction;
				_nextActionTimer = (float)_random.Next(5, 10) * 0.1f;
			}
			break;
		case EAIStrategy.ChargeDash:
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(_position);
			int num3 = nearestProtagonistPosition.X - _position.X;
			if ((num3 < 196 && num3 > 0) || (num3 > -196 && num3 < 0))
			{
				IsFacingLeft = nearestProtagonistPosition.X < _position.X;
				_isMovingLeft = IsFacingLeft;
				_currentAction = EAIAction.Move;
				_targetPosition = nearestProtagonistPosition;
				_nextActionTimer = (float)_random.Next(4, 7) * 0.1f;
			}
			else
			{
				_currentAction = EAIAction.Idle;
				_nextActionTimer = 0.25f;
			}
			break;
		}
		case EAIStrategy.LethargicRunTowards:
			if (_random.Next(2) == 0)
			{
				Point nearestProtagonistPosition2 = _level.GetNearestProtagonistPosition(_position);
				IsFacingLeft = nearestProtagonistPosition2.X < _position.X;
				_isMovingLeft = IsFacingLeft;
				_currentAction = EAIAction.Move;
				_targetPosition = nearestProtagonistPosition2;
				_nextActionTimer = (float)_random.Next(5, 15) * 0.1f;
			}
			else
			{
				_currentAction = EAIAction.Idle;
				_nextActionTimer = (float)_random.Next(10, 20) * 0.1f;
			}
			break;
		case EAIStrategy.Chase:
			_currentTarget = _level.GetNearestProtagonist(_position);
			_targetPosition = _currentTarget.Bbox.Center;
			IsFacingLeft = _targetPosition.X < _position.X;
			_isMovingLeft = IsFacingLeft;
			_currentAction = EAIAction.Move;
			_nextActionTimer = (float)_random.Next(5, 15) * 0.1f;
			break;
		case EAIStrategy.RunForever:
			if (Math.Abs(_velocity.X) < 1f)
			{
				bool flag2 = _level.CheckNearby((!IsFacingLeft) ? EDirection.East : EDirection.West, new Point(Position.X / 16, Position.Y / 16));
				bool flag3 = (IsFacingLeft && Bbox.Left - 8 <= 0) || (!IsFacingLeft && Bbox.Right + 8 >= _level.RoomSize.X);
				if (flag2 || flag3)
				{
					Position = new Point(Position.X + (IsFacingLeft ? 1 : (-1)), Position.Y);
					IsFacingLeft = !IsFacingLeft;
				}
			}
			_currentAction = EAIAction.Move;
			_nextActionTimer = _paceLength;
			_isMovingLeft = IsFacingLeft;
			break;
		case EAIStrategy.StandAttack:
		{
			if (_lastAction == EAIAction.DoAbility)
			{
				_currentAction = EAIAction.Idle;
				_nextActionTimer = (float)_random.Next(1, 3) * _timeToIdleAfterAttacking;
				break;
			}
			_targetPosition = _level.GetNearestProtagonistPosition(_position);
			bool flag4 = _targetPosition.X < _position.X;
			if (flag4 == IsFacingLeft || DoesNotTurnToFacePlayer)
			{
				_currentAction = EAIAction.DoAbility;
				_selectedAbility = 1;
				_nextActionTimer = 0f;
			}
			else
			{
				IsFacingLeft = flag4;
				_currentAction = EAIAction.Idle;
				_nextActionTimer = 0.5f;
			}
			break;
		}
		case EAIStrategy.FollowAttack:
		{
			_currentTarget = _level.GetNearestProtagonist(_position);
			_targetPosition = _currentTarget.Bbox.Center.Add(FollowAttackTargetOffset);
			int num = _targetPosition.X - _position.X;
			int num2 = _targetPosition.Y - _position.Y;
			_isJumpReset = !_isJumping && _isGrounded;
			bool flag = false;
			if (((num < _attackDistanceThresholdX && num > 0) || (num > -_attackDistanceThresholdX && num < 0)) && num2 >= _attackDistanceThresholdY && IsFacingLeft == num < 0)
			{
				if (_lastAction == EAIAction.Move || _lastAction == EAIAction.MoveJump || _lastAction == EAIAction.GoTowards || (_isAfraidOfMoving && !_isAfraidOfAttackingNearCliff))
				{
					_isAfraidOfMoving = false;
					_movementX = 0f;
					IsFacingLeft = _targetPosition.X < _position.X;
					_currentAction = EAIAction.DoAbility;
					_selectedAbility = 1;
					_nextActionTimer = 0f;
					_isJumping = false;
				}
				else if (_lastAction == EAIAction.Idle)
				{
					flag = true;
				}
				else
				{
					_currentAction = EAIAction.Idle;
					_nextActionTimer = (float)_random.Next(5, 10) * _timeToIdleAfterMoving;
					_isJumping = false;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				base.IsMoonWalking = false;
				if (_isAfraidOfBeingTooClose && ((num < _retreatDistanceThresholdX && num > 0) || (num > -_retreatDistanceThresholdX && num < 0)))
				{
					base.IsMoonWalking = true;
				}
				IsFacingLeft = _targetPosition.X < _position.X;
				_isMovingLeft = (base.IsMoonWalking ? (!IsFacingLeft) : IsFacingLeft);
				switch (_movementType)
				{
				case EAIMovementType.Walk:
					_currentAction = EAIAction.Move;
					break;
				case EAIMovementType.MoveJump:
					_currentAction = EAIAction.MoveJump;
					break;
				case EAIMovementType.Fly:
					_currentAction = EAIAction.GoTowards;
					break;
				}
				_nextActionTimer = (float)_random.Next(2, 5) * _timeToMove;
			}
			break;
		}
		case EAIStrategy.CustomScriptAI:
			PickNextCustomScriptAIAction(delta);
			break;
		}
	}

	protected virtual void DoAction(float delta)
	{
		if (_currentAction == EAIAction.Idle)
		{
			ManageState(EAFSM.Idle);
			_movementX = 0f;
			_movementY = 0f;
		}
		else if (_currentAction == EAIAction.Move)
		{
			if (_isCollidingWithWall)
			{
				if (_currentAI == EAIStrategy.Pace)
				{
					IsFacingLeft = !IsFacingLeft;
					_isMovingLeft = IsFacingLeft;
					_nextActionTimer = _paceLength;
				}
				else
				{
					_isAfraidOfMoving = true;
					_currentAction = EAIAction.Idle;
					_nextActionTimer = 0.01f;
				}
			}
			if (_isAfraidOfFalling && CheckIfFloorEnds())
			{
				_currentAction = EAIAction.Idle;
				_nextActionTimer = 0.1f;
				_movementX = 0f;
				_isAfraidOfMoving = true;
			}
			else if (!_isMovingLeft)
			{
				DoHorizontalRun(_agility);
			}
			else
			{
				DoHorizontalRun(0f - _agility);
			}
		}
		else if (_currentAction == EAIAction.MoveJump)
		{
			if (!_isGrounded)
			{
				if (!_isMovingLeft)
				{
					DoHorizontalRun(_agility);
				}
				else
				{
					DoHorizontalRun(0f - _agility);
				}
			}
			else
			{
				if (_isJumping && _currentJumpTime == 0f)
				{
					_isJumping = false;
				}
				_movementX = 0f;
				if (!_wasGrounded)
				{
					ManageState(EAFSM.Idle);
				}
			}
			if (_isJumpReset && _isGrounded)
			{
				_isJumping = true;
				_isJumpReset = false;
				ManageState(EAFSM.Jumping);
			}
		}
		else if (_currentAction == EAIAction.GoTowards)
		{
			ManageState(EAFSM.Moving);
			if (GoToPoint(_targetPosition, delta).LengthSquared() < 6f)
			{
				_currentAction = EAIAction.FloatInPlace;
				ManageState(EAFSM.Idle);
				_nextActionTimer = _timeToWaitAfterArriving;
				_isAtTargetLocation = true;
			}
		}
		else if (_currentAction == EAIAction.FloatInPlace)
		{
			ManageState(EAFSM.Idle);
			_movementX = 0f;
			_movementY = 0f;
			IdleInPlace(delta);
		}
		else if (_currentAction == EAIAction.DoAbility)
		{
			StartAbility(_selectedAbility);
		}
		else if (_currentAction == EAIAction.Custom)
		{
			UpdateCustomScriptAIAction(delta);
		}
	}

	protected virtual void DetermineAggroStatus(float delta)
	{
		_wasAggroed = _isAggroed;
		if (_isAlwaysAggroed)
		{
			_isAggroed = true;
		}
		else if (_isAggroed)
		{
			if (_canLoseAggro && !HasHeroInDeaggroBbox)
			{
				_isAggroed = false;
				OnDeAggroed();
			}
		}
		else if (HasHeroInAggroBbox || (_isRecoiling && HasHeroInDeaggroBbox))
		{
			_isAggroed = true;
			_nextActionTimer = 0f;
			OnAggroed();
		}
	}

	protected virtual void OnAggroed()
	{
	}

	protected virtual void OnDeAggroed()
	{
	}

	protected virtual void PickNextCustomScriptAIAction(float delta)
	{
	}

	protected virtual void UpdateCustomScriptAIAction(float delta)
	{
	}

	protected Vector2 GoToPoint(Point target, float delta)
	{
		_followTimer += delta;
		if (_followTimer > 314f)
		{
			_followTimer -= 314f;
		}
		float num = ((_startPosition.Y >= _targetPosition.Y) ? ((float)Math.Sin(_followTimer * 2f) * GoToOscillationMultiplier) : ((float)Math.Cos(_followTimer * 2f) * GoToOscillationMultiplier));
		if (_startPosition.X < _targetPosition.X)
		{
			num = 0f - num;
		}
		Vector2 vector = new Vector2(target.X - _position.X, target.Y - _position.Y);
		if (vector == Vector2.Zero)
		{
			_velocity = Vector2.Zero;
		}
		else
		{
			Vector2 value = Vector2.Normalize(vector);
			_velocity = Vector2.Multiply(value, 275f * _agility);
			_velocity = Vector2.Add(_velocity, Vector2.Multiply(new Vector2(0f - value.Y, value.X), num));
		}
		return vector;
	}

	private void IdleInPlace(float delta)
	{
		_followTimer += delta;
		if (_followTimer > 314f)
		{
			_followTimer -= 314f;
		}
		float x = (float)Math.Cos(_followTimer * 3f) * 18f * _oscillationMultiplierX;
		float y = (float)Math.Sin(_followTimer * 3f) * 18f * _oscillationMultiplierY;
		_velocity = new Vector2(x, y);
	}

	internal Point OvershootTarget(Point target)
	{
		Vector2 vector = new Vector2(Position.X - target.X, Position.Y - target.Y);
		vector *= 1.2f;
		return new Point((int)((float)target.X - vector.X), (int)((float)target.Y - vector.Y));
	}

	protected override float DoJump(float velocityY, float delta)
	{
		if (_isJumping)
		{
			if ((!_wasJumping && _isGrounded) || _currentJumpTime > 0f || (_currentJumpTime == 0f && !_isGrounded && !_wasJumping) || (IsInWater && !_wasJumping))
			{
				if (!_wasJumping)
				{
					_ = _isGrounded;
				}
				_currentJumpTime += delta;
			}
			if (0f < _currentJumpTime && _currentJumpTime <= _maxJumpTime + _currentSproingTime && !_isHittingHeadOnCeiling && _damagedTimer <= 0f)
			{
				if (_currentJumpTime >= _currentSproingTime)
				{
					float num = _currentJumpTime - _currentSproingTime;
					velocityY = _jumpLaunchVelocity * (1f - (float)Math.Pow(num / _maxJumpTime, 0.5));
				}
			}
			else
			{
				_currentJumpTime = 0f;
			}
		}
		else
		{
			_currentJumpTime = 0f;
		}
		return velocityY;
	}

	internal void SetCutsceneAI()
	{
		_currentAI = EAIStrategy.None;
		_currentAction = EAIAction.None;
	}

	public override void StartAbility(int whichAbility)
	{
		_selectedAbility = whichAbility;
		_isCarryingOutAbility = true;
		_abilityTimer = 0f;
		_totalAbilityTime = 10f;
		base.StartAbility(whichAbility);
		UpdateAbility(0f);
	}

	public virtual void UpdateAbility(float delta)
	{
		_isCarryingOutAbility = false;
		_abilityTimer = 0f;
	}

	internal EElementalWeaknessState GetElementWeakness(EDamageElement element)
	{
		EElementalWeaknessState result = EElementalWeaknessState.None;
		if (_bestiaryEntry != null && element != 0)
		{
			int num = (int)(element - 1);
			result = _bestiaryEntry.ElementalWeaknesses[num];
		}
		return result;
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result = false;
		if (type == EDamageType.Spike)
		{
			if (!IsImmuneToSpikes)
			{
				base.HP = 0;
				Kill();
			}
		}
		else
		{
			ENumberColor numberColor = ENumberColor.White;
			if (_level.IsHardMode)
			{
				damage = (int)Math.Ceiling((float)damage * 0.85f);
			}
			switch (GetElementWeakness(element))
			{
			case EElementalWeaknessState.Weak:
				damage = (int)Math.Ceiling((float)damage * 1.5f);
				numberColor = ENumberColor.Orange;
				break;
			case EElementalWeaknessState.Strong:
				damage = (int)Math.Ceiling((float)damage * 0.5f);
				numberColor = ENumberColor.Gray;
				break;
			}
			if (!_hasShownNameYet && _bestiaryEntry != null && !_bestiaryEntry.IsEntryInvisible && !IsMinion && _level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.FoeScanner))
			{
				_hasShownNameYet = true;
				if (_bestiaryEntry.VisibleName == null)
				{
					_bestiaryEntry.RefreshNameAndDescription();
				}
				_level.EnemyHitName = _bestiaryEntry.VisibleName;
			}
			if (!_isInvulnerable)
			{
				result = true;
				_damageFlashFrame = 4;
				base.HP = (short)(base.HP - damage);
				_level.AddNumber(damage, where, numberColor);
				if (base.HP <= 0)
				{
					base.HP = 0;
					Kill();
				}
				_invulnerableTimer += _defaultInvulnerableTime;
				_isInvulnerable = true;
				StartRecoil();
			}
			if (_doesAggroOnTakingDamage && !_isAggroed)
			{
				_isAggroed = true;
				OnAggroed();
			}
		}
		return result;
	}

	private void StartRecoil()
	{
		if (DoesRecoil && !_isRecoiling)
		{
			_isRecoiling = true;
			_recoilTimer = 0.03334f;
		}
	}

	private void UpdateRecoil(float delta)
	{
		_recoilTimer -= delta;
		if (_recoilTimer <= 0f)
		{
			_isRecoiling = false;
			_recoilTimer = 0f;
		}
	}

	public override void Kill()
	{
		if (!_isFrozen)
		{
			StartDeathScript();
			IncrementKillCounter();
			_level.AddKill();
			base.StatusEffects.Clear();
		}
		else
		{
			_isDeadButFrozen = true;
		}
	}

	private void IncrementKillCounter()
	{
		string killKeyFromTypeAndArgument = GetKillKeyFromTypeAndArgument(EnemyType, _argument);
		int saveInt = _level.GameSave.GetSaveInt(killKeyFromTypeAndArgument);
		_level.GameSave.SetValue(killKeyFromTypeAndArgument, saveInt + 1);
	}

	internal static string GetKillKeyFromTypeAndArgument(EEnemyTileType enemyType, int argument)
	{
		bool flag = argument != 0 && ObjectTileSpecification.DoesUseArgumentForKey(enemyType);
		return string.Format("{0}{1}{2}", "KILL_", enemyType, (!flag) ? "" : ("_" + argument));
	}

	internal static string GetItemDropKeyFromTypeArgumentAndIndex(EEnemyTileType enemyType, int argument, int itemIndex)
	{
		bool flag = argument != 0 && ObjectTileSpecification.DoesUseArgumentForKey(enemyType);
		return string.Format("{0}{3}_{1}{2}", "DROP_", enemyType, (!flag) ? "" : ("_" + argument), itemIndex);
	}

	protected virtual void StartDeathScript()
	{
		_isFinallyDead = true;
		_isRunningDeathScript = true;
	}

	public virtual void DropLootAndRemove()
	{
		DropLoot();
		RemoveInstance();
	}

	protected virtual void DropLoot()
	{
		bool flag = false;
		float num = (float)_level.GameSave.CharacterStats.Luck * 0.05f;
		if (_bestiaryEntry != null && !IsMinion)
		{
			bool isSpeedrunAActive = _level.GameSave.IsSpeedrunAActive;
			int count = _bestiaryEntry.LootTable.Count;
			bool flag2 = false;
			if (isSpeedrunAActive && count > 0)
			{
				BestiaryItemDropSpecification bestiaryItemDropSpecification = _bestiaryEntry.LootTable[0];
				flag2 = InventoryItem.IsQuestItem(bestiaryItemDropSpecification.Item, bestiaryItemDropSpecification.Category);
			}
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				BestiaryItemDropSpecification bestiaryItemDropSpecification2 = _bestiaryEntry.LootTable[num2];
				double num3 = _level.NextRandomDouble();
				if (isSpeedrunAActive && (InventoryItem.IsQuestItem(bestiaryItemDropSpecification2.Item, bestiaryItemDropSpecification2.Category) || (count > 1 && num2 == count - 1 && bestiaryItemDropSpecification2.Category > 1 && !flag2)))
				{
					num3 = 0.0;
				}
				if (num3 * 100.0 <= (double)((float)bestiaryItemDropSpecification2.DropRate + num))
				{
					flag = true;
					Point inPosition = _level.FindOpenTilePosition(Bbox.Center);
					ItemDropPickup item = new ItemDropPickup(bestiaryItemDropSpecification2, _level, inPosition, _level.NextObjectTicketID);
					_level.AddItem(item);
					_level.GameSave.SetValue(GetItemDropKeyFromTypeArgumentAndIndex(EnemyType, _argument, num2), value: true);
					break;
				}
			}
		}
		if (!flag && _doesDropBasicLoot && _level.NextRandomDouble() * 100.0 <= (double)(10f + num))
		{
			int gemAmountFromLotteryRoll = GemItem.GetGemAmountFromLotteryRoll((float)_random.NextDouble());
			_level.AddItem(EItemType.Money, gemAmountFromLotteryRoll, Bbox.Center, _level.NextObjectTicketID);
		}
		GiveExperience();
	}

	protected void GiveExperience()
	{
		int amount = ((!IsMinion) ? ExperienceGiven : 0);
		_level.GiveExperience(amount, base.ID, base.OuterBbox.Center, isBossHit: false);
	}

	protected void RemoveInstance()
	{
		IsDead = true;
		base.Kill();
	}

	protected void KillButLeaveCorpse()
	{
		IsDead = true;
		_particleSystems.Clear();
		StopAllSFX();
	}

	public override void Unfreeze()
	{
		base.Unfreeze();
		if (_isDeadButFrozen)
		{
			Kill();
		}
	}

	public virtual void InitializeMob()
	{
		_doesTrackTimeSinceGrounded = _isAffectedByGravity;
		_wasFacingLeft = IsFacingLeft;
		Update(0f);
	}

	protected virtual void KillIfOutOfBounds()
	{
		if (!_doesIgnoreOutOfBoundsDeath && Position.Y > _level.RoomSize.Y + 100)
		{
			SilentKill();
		}
	}

	internal bool CheckObjectCollision(Animate target)
	{
		bool flag = false;
		bool flag2 = false;
		if (!IsDead)
		{
			if (DoesCollideWith(target.Bbox))
			{
				List<Rectangle> list = FindIntersectingBoundingBoxes(target.Bbox, (!base.IsFrozen) ? 1 : (-1));
				if (list.Count > 0)
				{
					flag2 = true;
					if (base.IsFrozen)
					{
						if (!_isAlreadyTouchingHero && IsSolidWhenFrozen)
						{
							foreach (Rectangle item in list)
							{
								Vector2 intersectionDepth = target.Bbox.GetIntersectionDepth(item);
								flag = target.CollideSolidObject(this, ETileType.Monster, intersectionDepth) || flag;
							}
						}
					}
					else
					{
						if (Damage != 0 && base.HP > 0)
						{
							Rectangle rectangle = list[0];
							Point intersectionCenter = RectangleExtensions.GetIntersectionCenter(target.Bbox, rectangle);
							flag = target.ManageDamage(Damage, new Vector2((rectangle.Center.X < target.Position.X) ? 1 : (-1), 0f), intersectionCenter, rectangle, EDamageType.None, EDamageElement.None, DoesTouchDamageKnockback);
							if (flag && target is Alive target2)
							{
								AfterDealingTouchDamage(target2, intersectionCenter);
							}
						}
						_isAlreadyTouchingHero = true;
					}
				}
			}
			if (!flag2)
			{
				_isAlreadyTouchingHero = false;
			}
			HasHeroInAggroBbox = HasHeroInAggroBbox || AggroBbox.Intersects(target.Bbox);
			HasHeroInDeaggroBbox = HasHeroInDeaggroBbox || DeaggroBbox.Intersects(target.Bbox);
			HeroWhoAggroedMe = target;
		}
		return flag;
	}

	protected virtual void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		_level.AddAnimation(EBattleAnimationType.MediumHitYellow, effectPosition, ETeamSide.Heroes, target.IsFacingLeft);
	}

	internal virtual void InitializeForBestiary()
	{
	}
}
