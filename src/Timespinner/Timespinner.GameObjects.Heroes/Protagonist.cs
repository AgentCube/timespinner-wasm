using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Heroes;

public class Protagonist : Alive
{
	internal const int JumpBufferThreshold = 6;

	private const float TimeToDisableInputAfterPause = 0.33f;

	private const float TimeBetweenUnderwaterBubbleEmission = 1.5f;

	private readonly bool _isPrimaryPlayer;

	private readonly UnderwaterBubbleParticleSystem _underwaterBubbleParticleSystem;

	protected readonly GamePadWrapper _gamePadWrapper;

	protected bool _isPlayable = true;

	protected bool _areControlsLocked;

	protected int _playerNumber;

	protected float _inputDisableTimer;

	private float _underwaterBubbleEmissionTimer;

	private float _nextUnderwaterBubbleEmissionTime;

	protected Point _spawnPoint;

	protected bool _canDoubleJump;

	protected int _currentLevel = 1;

	protected float _currentExperience;

	protected bool _isCarryingOutAbility;

	protected bool _hasUsedAbility;

	protected bool _didStartAbilityGrounded;

	protected bool _canCurrentAbilityCanBeCanceled;

	protected bool _isCurrentAbilityGivingFlight;

	protected bool _wasFacingLeftWhenStartedAbility;

	protected int _selectedAbility;

	protected byte _selectedAbilityVariation;

	protected float _abilityTimer;

	protected float _lastAbilityTimer;

	protected float _totalAbilityTime;

	protected bool _isHoldingCharge;

	public bool IsBlocked { get; set; }

	public bool IsPlayable => _isPlayable;

	public bool IsCharging => _isCharging;

	public bool WasCharging { get; set; }

	public bool IsPrimaryPlayer => _isPrimaryPlayer;

	internal bool IsLevelingUp { get; set; }

	internal bool IsAffectedByLevelBounds
	{
		get
		{
			return _isAffectedByLevelBounds;
		}
		set
		{
			_isAffectedByLevelBounds = value;
		}
	}

	internal bool HasCastASpell { get; set; }

	public float CurrentExperience => _currentExperience;

	internal Point UnderwaterBubbleEmissionPointOffset { get; set; }

	public virtual bool IsCurrentlyMagnetizing => false;

	internal bool IsOOM { get; set; }

	internal int OOMSpellAmount { get; set; }

	internal int Defense { get; set; }

	public virtual int Aura { get; set; }

	public virtual int MaxAura => 0;

	public virtual int ChargeSelect => 0;

	internal float GravityAcceleration => _gravityAcceleration;

	public virtual Point CurrentMagnetCenter => _bbox.Center;

	public virtual Rectangle SecondaryItemCollect => _bbox;

	public virtual List<int> ChargeIntervals => new List<int>();

	public Protagonist(Point inPosition, Level inLevel, SpriteSheet inSprite, int inPlayerNumber, int inID, bool isPrimaryPlayer)
		: base(inPosition, inLevel, inSprite, inID)
	{
		_doesUse16X16TileCollisionBbox = false;
		_spawnPoint = inPosition;
		_playerNumber = inPlayerNumber;
		_isPrimaryPlayer = isPrimaryPlayer;
		ControllerMapping controllerMapping = (isPrimaryPlayer ? _level.ConfigSave.PlayerControllerMapping : _level.ConfigSave.FamiliarControllerMapping);
		_gamePadWrapper = new GamePadWrapper(_playerNumber, controllerMapping, canVibrate: true);
		base.BaseType = EGameObjectBaseType.Hero;
		_defaultTeam = ETeamSide.Heroes;
		_isAffectedByWater = true;
		_underwaterBubbleParticleSystem = new UnderwaterBubbleParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_particleSystems.Add(_underwaterBubbleParticleSystem);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && IsInWater)
		{
			_underwaterBubbleEmissionTimer += delta;
			if (_underwaterBubbleEmissionTimer >= _nextUnderwaterBubbleEmissionTime)
			{
				Point b = (IsImageFacingLeft ? UnderwaterBubbleEmissionPointOffset : new Point(-UnderwaterBubbleEmissionPointOffset.X, UnderwaterBubbleEmissionPointOffset.Y));
				Point point = new Point(Bbox.Center.X, Bbox.Top).Add(b);
				if (point.Y > _underwaterBubbleParticleSystem.UpdateWaterTop(point, _level))
				{
					_underwaterBubbleParticleSystem.AddParticles(point.ToVector2());
				}
				_underwaterBubbleEmissionTimer = 0f;
				_nextUnderwaterBubbleEmissionTime = 1.5f * (1f + (float)_level.NextRandomDouble());
			}
		}
		if (_currentlyGrabbedEvent != null)
		{
			if (IsGrabbing)
			{
				bool flag = false;
				if (_currentlyGrabbedEvent.CannotBeGrabbed)
				{
					flag = true;
				}
				else if (Math.Abs(base.StandingOnVector.X) > 0f || Math.Abs(base.StandingOnVector.Y) > 0f)
				{
					flag = true;
				}
				if (flag)
				{
					IsGrabbing = false;
					ManageState(EAFSM.Jumping);
					_currentlyGrabbedEvent = null;
				}
			}
			else
			{
				_currentlyGrabbedEvent = null;
			}
		}
		if (!IsBlocked && !_isBlockingPlayerInput && !_level.IsPlayerInputBlocked)
		{
			ProcessKeys(delta);
		}
		else
		{
			if (!_areActiveScriptsGoing)
			{
				ScriptIdleSelf();
			}
			_gamePadWrapper.Reset();
			_gamePadWrapper.UpdateVibration();
		}
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
				CarryOutAbility(delta);
			}
		}
		if (!IsBlocked)
		{
			base.Update(delta);
		}
		else
		{
			UpdateScriptActions(delta);
		}
	}

	protected virtual void ScriptIdleSelf()
	{
		_isCharging = false;
		if (_isGrounded)
		{
			_isJumpReset = true;
			_isDoubleJumping = false;
		}
		_isJumping = false;
		_isIgnoringPlatform = false;
		if (_wasJumping && _velocity.Y < 0f)
		{
			_velocity.Y /= 2f;
		}
		if (IsPrimaryPlayer)
		{
			ManageState(EAFSM.Idle);
		}
		_movementX = 0f;
		_isDashing = false;
		_isSkydashing = false;
		_overrideFallSpeedAmount = 0f;
	}

	protected override float DoJump(float velocityY, float delta)
	{
		if (_isJumping)
		{
			if ((_isGrounded && _gamePadWrapper.JumpBufferValue <= 6) || (_isGrounded && _wasGrabbing && !_wasJumping) || (!_wasJumping && _isMiracleJumping) || _currentJumpTime > 0f || (_currentJumpTime <= 0f && !_isDoubleJumping && _canDoubleJump && !_isGrounded && !_isMiracleJumping && !_wasJumping) || (IsInWater && !_wasJumping))
			{
				if (IsInWater)
				{
					_isDoubleJumping = false;
				}
				if (_currentJumpTime <= 0f && !_isDoubleJumping && _canDoubleJump && !_isGrounded && !_isMiracleJumping && !IsInWater)
				{
					_isDoubleJumping = true;
					ManageState(EAFSM.Jumping);
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

	protected override bool HandleHorizontalCollision(GameObject target, Point tileKey, ETileType tileType, Vector2 depth)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		_isGrabbingFrozenObject = false;
		_currentlyGrabbedEvent = null;
		GameObject source = ((tileType == ETileType.Event) ? target : null);
		if (depth.X > 0f)
		{
			if (!_level.CheckIfHorizontallyAdjacentTilesBlock(EDirection.East, tileKey, target, _position) || tileType == ETileType.Monster || tileType == ETileType.Event)
			{
				if (tileType != ETileType.Monster && tileType != ETileType.Event)
				{
					if ((!_level.CheckNearby(EDirection.North, tileKey) || tileType == ETileType.Slope) && !_level.CheckNearbySolid(EDirection.East, tileKey) && !_level.CheckNearby(EDirection.NorthEast, tileKey) && !_level.CheckNearby(EDirection.SouthEast, tileKey) && IsFacingLeft && _grabNextTimer >= 0.2f && base.CurrentState != EAFSM.Damaged && !_wasGrounded)
					{
						flag3 = true;
					}
					else
					{
						flag2 = true;
					}
				}
				else
				{
					Rectangle rectangle = new Rectangle(target.Bbox.Right, target.Bbox.Top, Bbox.Width, Bbox.Height);
					int num = (rectangle.Left - 1) / 16;
					int num2 = (rectangle.Top - 1) / 16;
					int num3 = (int)Math.Ceiling((float)(rectangle.Right + 1) / 16f);
					int num4 = (int)Math.Ceiling((float)rectangle.Bottom / 16f);
					bool flag4 = base.CurrentState != EAFSM.Damaged;
					for (int i = num2; i < num4; i++)
					{
						for (int j = num; j < num3; j++)
						{
							if (_level.SolidTiles.ContainsKey(new Point(j, i)))
							{
								flag4 = false;
							}
						}
					}
					if (flag4)
					{
						flag3 = true;
					}
					else
					{
						flag2 = true;
					}
				}
			}
		}
		else if (depth.X < 0f && (!_level.CheckIfHorizontallyAdjacentTilesBlock(EDirection.West, tileKey, target, _position) || tileType == ETileType.Monster || tileType == ETileType.Event))
		{
			if (tileType != ETileType.Monster && tileType != ETileType.Event)
			{
				if ((!_level.CheckNearby(EDirection.North, tileKey) || tileType == ETileType.Slope) && !_level.CheckNearbySolid(EDirection.West, tileKey) && !_level.CheckNearby(EDirection.NorthWest, tileKey) && !_level.CheckNearby(EDirection.SouthWest, tileKey) && !IsFacingLeft && _grabNextTimer >= 0.2f && !_wasGrounded)
				{
					flag3 = true;
				}
				else
				{
					flag2 = true;
				}
			}
			else
			{
				Rectangle rectangle2 = new Rectangle(target.Bbox.Left - Bbox.Width, target.Bbox.Top, Bbox.Width, Bbox.Height);
				int num5 = rectangle2.Left / 16;
				int num6 = (rectangle2.Top - 1) / 16;
				int num7 = (int)Math.Ceiling((float)(rectangle2.Right + 1) / 16f);
				int num8 = (int)Math.Ceiling((float)rectangle2.Bottom / 16f);
				bool flag5 = true;
				for (int k = num6; k < num8; k++)
				{
					for (int l = num5; l < num7; l++)
					{
						if (_level.SolidTiles.ContainsKey(new Point(l, k)))
						{
							flag5 = false;
						}
					}
				}
				if (flag5)
				{
					flag3 = true;
				}
				else
				{
					flag2 = true;
				}
			}
		}
		int num9 = target.Bbox.Top;
		if (flag3)
		{
			bool flag6 = Bbox.Height < target.Bbox.Height;
			bool flag7 = (flag6 ? (Bbox.Top > target.Bbox.Top) : (depth.Y >= 0f));
			bool flag8 = (flag6 ? (_velocity.Y >= 0f) : (_velocity.Y >= -100f));
			bool flag9 = Bbox.Top - target.Bbox.Top < 16;
			bool flag10 = base.LastPosition.Y - Bbox.Height - 12 <= target.Bbox.Top;
			if (target.CannotBeGrabbed || IsInWater || base.WasInWater)
			{
				flag2 = true;
				flag3 = false;
			}
			else if (!flag8 || !flag7 || !flag9 || !flag10 || num9 <= 0 || _currentState == EAFSM.Running)
			{
				flag2 = true;
				flag3 = false;
			}
			else if (tileType == ETileType.Monster || tileType == ETileType.Event)
			{
				if (Bbox.Top - target.Bbox.Top > Bbox.Height)
				{
					flag2 = true;
					flag3 = false;
				}
				if (Math.Abs(base.StandingOnVector.X) > 0f || Math.Abs(base.StandingOnVector.Y) > 0f)
				{
					flag2 = true;
					flag3 = false;
				}
			}
			switch (tileType)
			{
			case ETileType.Solid:
				if (target is Tile tile2 && (tile2.Special == ETileSpecialType.VerticalSpike || tile2.Special == ETileSpecialType.HorizontalSpike))
				{
					flag2 = true;
					flag3 = false;
				}
				break;
			case ETileType.Slope:
				if (target is Tile tile)
				{
					int num10 = _position.X + (int)depth.X;
					bool flag11 = tile.Position.X < num10;
					int num11 = (flag11 ? tile.LookupTileHeight(tile.Bbox.Right) : tile.LookupTileHeight(tile.Bbox.Left));
					num9 = tile.Bbox.Top + ((!tile.IsFlippedVertically) ? num11 : 0);
					if (num9 < _bbox.Top)
					{
						flag3 = false;
						flag2 = true;
					}
					if (num11 == 16 && _level.CheckNearby(flag11 ? EDirection.SouthEast : EDirection.SouthWest, new Point(tile.DictKey.X, tile.DictKey.Y + 1)))
					{
						flag3 = false;
						flag2 = true;
					}
				}
				break;
			}
			if (!_isAffectedByGravity)
			{
				flag2 = true;
				flag3 = false;
			}
			if ((IsFacingLeft && depth.X < 0f) || (!IsFacingLeft && depth.X > 0f))
			{
				flag2 = true;
				flag3 = false;
			}
		}
		if (flag3)
		{
			flag = true;
			IsGrabbing = true;
			_isGrounded = true;
			ManageState(EAFSM.Grabbing);
			_velocity.X = 0f;
			_targetGrabPosition = new Point(_position.X + (int)depth.X, num9 + Bbox.Height);
			if (Position.Y > _targetGrabPosition.Y || _velocity.Y < 0f)
			{
				_velocity.Y = 0f;
				CollisionSetPosition(_targetGrabPosition);
				_targetGrabPosition = new Point(-1, -1);
			}
			else
			{
				CollisionSetPosition(new Point(_targetGrabPosition.X, Position.Y), source);
			}
			if (tileType == ETileType.Monster)
			{
				_isGrabbingFrozenObject = true;
			}
			if (tileType == ETileType.Event)
			{
				GameEvent gameEvent = target as GameEvent;
				AddMovingPlatform(gameEvent);
				_currentlyGrabbedEvent = gameEvent;
			}
		}
		else if (flag2)
		{
			if (_isDashing && !base.IsGrounded && _timeSinceGrounded > 0.25f && depth.X > 0f == IsFacingLeft)
			{
				flag = true;
				_isDashing = false;
				ManageState(EAFSM.Idle);
				ManageState(EAFSM.Idle);
			}
			flag = base.HandleHorizontalCollision(target, tileKey, tileType, depth) || flag;
		}
		return flag;
	}

	protected virtual void ProcessKeys(float delta)
	{
		_gamePadWrapper.UpdateState(_areControlsLocked);
		if (!_level.WasLastActive)
		{
			if (IsLevelingUp)
			{
				IsLevelingUp = false;
			}
			else
			{
				_inputDisableTimer = 0.33f;
			}
		}
		if (_inputDisableTimer > 0f)
		{
			_inputDisableTimer -= delta;
			_gamePadWrapper.PostMenuDisableInput();
		}
	}

	public override void Kill()
	{
	}

	public virtual void CarryOutAbility(float delta)
	{
	}

	public virtual void CancelSpells()
	{
	}

	public virtual void HideAndBlockInput(bool shouldBlockAndHide, bool shouldHideFamiliar)
	{
	}

	public virtual bool DetectFallDeath(int levelHeight)
	{
		return false;
	}

	protected virtual void CreateBullet(int whichBullet)
	{
	}

	public virtual void GetPowerup(EItemType item, float amount)
	{
		if (item == EItemType.Money)
		{
			_level.GameSave.Money += (int)amount;
		}
	}

	public virtual void ChangeRoom()
	{
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			particleSystem.KillOffParticles(0f);
		}
		foreach (BaseStatusEffect statusEffect in base.StatusEffects)
		{
			statusEffect.ChangeRoom();
		}
	}

	public virtual void RefreshStats(GameSave inSave)
	{
	}

	public void RefreshControls(GameConfigSave inSave)
	{
		_gamePadWrapper.ControllerMapping = (_isPrimaryPlayer ? inSave.PlayerControllerMapping : inSave.FamiliarControllerMapping);
	}

	public virtual void EndAbility()
	{
		_isCarryingOutAbility = false;
		_hasUsedAbility = false;
		_abilityTimer = 0f;
		_lastAbilityTimer = -1f;
		_canCurrentAbilityCanBeCanceled = false;
		if (_isCurrentAbilityGivingFlight)
		{
			_isAffectedByGravity = true;
			_isCurrentAbilityGivingFlight = false;
		}
	}

	public virtual void CancelAbility(bool overrideCanBeCanceled)
	{
		if (overrideCanBeCanceled || _canCurrentAbilityCanBeCanceled)
		{
			EndAbility();
		}
	}

	public virtual void DoneCasting()
	{
		if (_isGrabbingFrozenObject)
		{
			_isGrabbingFrozenObject = false;
			IsGrabbing = false;
			ManageState(EAFSM.Jumping);
		}
	}

	public void HealHP(int amount)
	{
		if (base.HP < base.MaxHP)
		{
			base.HP = (short)(base.HP + amount);
		}
	}

	public virtual void Revive()
	{
		base.HP = base.MaxHP;
		_currentState = EAFSM.Idle;
		SetState(EAFSM.Idle);
	}

	public virtual void HealAura(int amount)
	{
	}

	public bool CheckButton(int whichButton)
	{
		return CheckButton(whichButton, isNewPressOnly: false);
	}

	public bool CheckButton(int whichButton, bool isNewPressOnly)
	{
		if (!_isPrimaryPlayer)
		{
			return false;
		}
		switch (whichButton)
		{
		case 0:
			if (_gamePadWrapper.IsJumpDown)
			{
				if (isNewPressOnly)
				{
					return !_gamePadWrapper.WasJumpDown;
				}
				return true;
			}
			return false;
		case 4:
			if (_gamePadWrapper.IsUpDown)
			{
				if (isNewPressOnly)
				{
					return !_gamePadWrapper.WasUpDown;
				}
				return true;
			}
			return false;
		case 5:
			if (_gamePadWrapper.IsRightDown)
			{
				if (isNewPressOnly)
				{
					return !_gamePadWrapper.WasRightDown;
				}
				return true;
			}
			return false;
		case 6:
			if (_gamePadWrapper.IsDownDown)
			{
				if (isNewPressOnly)
				{
					return !_gamePadWrapper.WasDownDown;
				}
				return true;
			}
			return false;
		case 7:
			if (_gamePadWrapper.IsLeftDown)
			{
				if (isNewPressOnly)
				{
					return !_gamePadWrapper.WasLeftDown;
				}
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public void StopMovement()
	{
		base.Velocity = Vector2.Zero;
		_movementX = 0f;
		_isJumping = false;
		_currentJumpTime = 0f;
	}

	public void GiveLoot(EInventoryUseItemType type, int amount)
	{
		_level.GameSave.Inventory.AddItem(type, amount);
	}

	public void GiveLoot(EInventoryEquipmentType type)
	{
		_level.GameSave.Inventory.AddItem(type);
	}

	internal void GiveLoot(EInventoryOrbType type, EOrbSlot slot)
	{
		_level.GameSave.GiveOrb(type, slot);
	}

	public void GiveLoot(EInventoryRelicType type)
	{
		_level.GameSave.Inventory.AddItem(type);
	}

	public void GiveLoot(EInventoryFamiliarType type)
	{
		_level.GameSave.GiveFamiliar(type);
	}

	public virtual void GiveExperience(int amount, int enemyID, Point position, bool isBossHit)
	{
	}

	internal void FullyHeal()
	{
		base.HP = base.MaxHP;
		base.MP = base.MaxHP;
		Aura = MaxAura;
		_healGlowTimer = 1f;
		_healGlowColor = Alive._sandHealGlowColorPeak;
	}

	internal void ToggleInvulnerability(bool isInvulnerable)
	{
		if (_isBlinking)
		{
			base.DrawColor = Color.White;
			_isBlinking = false;
		}
		if (isInvulnerable)
		{
			_invulnerableTimer = 100f;
			_isInvulnerable = true;
		}
		else
		{
			_isInvulnerable = false;
			_invulnerableTimer = 0f;
		}
	}

	internal virtual void ReduceAura(float reductionAmount)
	{
	}
}
