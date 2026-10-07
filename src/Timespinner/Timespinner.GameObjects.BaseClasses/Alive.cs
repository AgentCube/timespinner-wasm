using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.BaseClasses;

public class Alive : Animate
{
	protected const float MaxDashingCooldown = 0.3f;

	protected const float MaxSkydashingCooldown = 0.3f;

	protected const float HealGlowTime = 1.5f;

	protected const float SandHealGlowTime = 1f;

	protected static readonly Color _flashFrame1Color = new Color(1f, 1f, 0.8f, 0.65f);

	protected static readonly Color _flashFrame2Color = new Color(1f, 0.3f, 0.1f, 0.8f);

	protected static readonly Color _healGlowColorPeak = new Color(0.5f, 1f, 0.5f, 0.8f);

	protected static readonly Color _sandHealGlowColorPeak = new Color(250, 200, 150, 150);

	private readonly List<int> _abilitiesUsedList = new List<int>();

	private readonly List<BaseStatusEffect> _statusEffects = new List<BaseStatusEffect>();

	protected bool _isMovingLeft;

	protected bool _wasCharging;

	protected bool _isCharging;

	protected bool _isDoubleJumping;

	protected bool _wasDoubleJumping;

	protected bool _doesHaveTimeStopped;

	protected bool _isFallCrouching;

	protected bool _isDashing;

	protected bool _wasDashing;

	protected bool _isSkydashing;

	protected bool _wasSkydashing;

	protected bool _isAlwaysInvulnerable;

	protected bool _isInvulnerable;

	protected bool _isJumpReset = true;

	protected bool _isDeadButFrozen;

	protected bool _hasStartedChargingAnimation;

	protected bool _isTreadingWater;

	private int _hp;

	private int _maxHp;

	private int _maxMp;

	protected int _damageFlashFrame = -1;

	protected float _mp;

	protected float _mpRegen = 1f;

	protected float _invulnerableTimer;

	protected float _defaultInvulnerableTime = 0.01f;

	protected float _damagedTimer;

	protected float _dashCooldownTimer;

	protected float _skydashCooldownTimer;

	protected float _healGlowTimer;

	protected float _agility = 1f;

	protected EAFSM _currentState = EAFSM.Idle;

	protected EAFSM _previousState = EAFSM.Idle;

	protected Color _healGlowColor = _healGlowColorPeak;

	protected Point _gunOffset;

	internal bool HasCurrentStateChanged { get; set; }

	internal bool IsMPBeingRestored { get; set; }

	internal bool IsInvulnerable => _isInvulnerable;

	internal bool IsJumping
	{
		get
		{
			return _isJumping;
		}
		set
		{
			_isJumping = value;
		}
	}

	internal bool IsDoubleJumping
	{
		get
		{
			return _isDoubleJumping;
		}
		set
		{
			_isDoubleJumping = value;
		}
	}

	internal bool IsDashing
	{
		get
		{
			return _isDashing;
		}
		set
		{
			_isDashing = value;
		}
	}

	internal bool IsSkydashing
	{
		get
		{
			return _isSkydashing;
		}
		set
		{
			_isSkydashing = value;
		}
	}

	internal bool IsCurrentlyGrabbing
	{
		get
		{
			return IsGrabbing;
		}
		set
		{
			IsGrabbing = value;
		}
	}

	internal bool IsMovingLeft
	{
		get
		{
			if (!(_velocity.X < -0.1f))
			{
				return _movementX < 0f;
			}
			return true;
		}
	}

	internal bool IsMovingRight
	{
		get
		{
			if (!(_velocity.X > 0.1f))
			{
				return _movementX > 0f;
			}
			return true;
		}
	}

	internal bool IsCrouchingDisabled { get; set; }

	internal bool WasCrouchingDisabled { get; private set; }

	public EAFSM CurrentState => _currentState;

	public ETeamSide DefaultTime => _defaultTeam;

	public short ChangeInHP { get; set; }

	public int MaxHP
	{
		get
		{
			return _maxHp;
		}
		set
		{
			_maxHp = value;
			if (_maxHp < _hp)
			{
				_hp = _maxHp;
			}
		}
	}

	public int MaxMP
	{
		get
		{
			return _maxMp;
		}
		set
		{
			_maxMp = value;
			if ((float)_maxMp < _mp)
			{
				_mp = _maxMp;
			}
		}
	}

	public float HPPercentage => (float)HP / (float)MaxHP;

	public List<int> AbilitiesUsedList => _abilitiesUsedList;

	internal List<BaseStatusEffect> StatusEffects => _statusEffects;

	public int HP
	{
		get
		{
			return _hp;
		}
		set
		{
			int hp = _hp;
			_hp = value;
			if (_hp > MaxHP)
			{
				_hp = MaxHP;
			}
			ChangeInHP -= (short)(hp - _hp);
		}
	}

	public int MP
	{
		get
		{
			return (int)Math.Ceiling(_mp);
		}
		set
		{
			_mp = value;
			if (_mp > (float)MaxMP)
			{
				_mp = MaxMP;
			}
		}
	}

	public float MPFloat
	{
		get
		{
			return _mp;
		}
		set
		{
			_mp = value;
			if (_mp > (float)MaxMP)
			{
				_mp = MaxMP;
			}
		}
	}

	internal virtual bool IsABoss => false;

	public Alive(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID)
		: base(inPosition, inLevel, inID)
	{
		_sprite = inSprite;
		_bbox = new Rectangle(inPosition.X, inPosition.Y, inSprite.FrameSize.X, inSprite.FrameSize.Y);
		SetState(EAFSM.Idle);
	}

	public override void Update(float delta)
	{
		if (_previousState != _currentState)
		{
			HasCurrentStateChanged = true;
		}
		_previousState = _currentState;
		if (!_isAlwaysInvulnerable)
		{
			if (_isInvulnerable)
			{
				_invulnerableTimer -= delta;
				if (_invulnerableTimer <= 0f)
				{
					_isInvulnerable = false;
					_isBlinking = false;
					_isBeingSquished = false;
					_objectThatSquishedUs = null;
					base.DrawColor = Color.White;
				}
			}
		}
		else
		{
			_isInvulnerable = true;
		}
		if (_healGlowTimer > 0f)
		{
			_healGlowTimer -= delta;
			if (_healGlowTimer <= 0f)
			{
				_isGlowing = false;
				_healGlowTimer = 0f;
			}
			else
			{
				_isGlowing = true;
				float num = 1f - _healGlowTimer / 1.5f;
				float num2 = (float)Math.Sin((double)(num * 2f) * Math.PI);
				if (num < 0.5f)
				{
					num2 *= 2f;
					if (num2 > 1f)
					{
						num2 = 1f;
					}
				}
				_glowColor = new Color((int)MathHelper.Lerp(255f, (int)_healGlowColor.R, num2), (int)MathHelper.Lerp(255f, (int)_healGlowColor.G, num2), (int)MathHelper.Lerp(255f, (int)_healGlowColor.B, num2), (int)MathHelper.Lerp(255f, (int)_healGlowColor.A, num2));
			}
		}
		if (!_isFrozen)
		{
			if (_damagedTimer > 0f)
			{
				_damagedTimer -= delta;
				if (_damagedTimer < 0f)
				{
					_damagedTimer = 0f;
					ManageState(EAFSM.Idle);
				}
			}
			else if (_currentState == EAFSM.Damaged)
			{
				ManageState(EAFSM.Idle);
			}
			UpdateStatusEffects(delta);
			base.Update(delta);
			if (_mp != (float)_maxMp)
			{
				_mp += _mpRegen * delta;
			}
			if (_mp > (float)_maxMp)
			{
				_mp = _maxMp;
			}
			if (_dashCooldownTimer > 0f)
			{
				_dashCooldownTimer -= delta;
			}
			if (_skydashCooldownTimer > 0f)
			{
				_skydashCooldownTimer -= delta;
			}
		}
		else
		{
			base.Update(0f);
		}
		UpdateChargingAnimation();
		_wasDashing = _isDashing;
		_wasSkydashing = _isSkydashing;
		WasCrouchingDisabled = IsCrouchingDisabled;
		IsCrouchingDisabled = false;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_damageFlashFrame > -1)
		{
			_isGlowing = true;
			_glowBase = 1f;
			_glowColor = ((_damageFlashFrame % 3 == 0) ? _flashFrame1Color : _flashFrame2Color);
			_damageFlashFrame--;
			if (_damageFlashFrame <= -1)
			{
				_isGlowing = false;
				base.DrawColor = Color.White;
			}
		}
		base.Draw(spriteBatch);
		DrawStatusEffects(spriteBatch);
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result = false;
		if (!_isInvulnerable)
		{
			result = true;
			_damageFlashFrame = 4;
			HP = (short)(HP - damage);
			_level.AddNumber(damage, where, (base.DefaultTeam == ETeamSide.Heroes) ? ENumberColor.Red : ENumberColor.White);
			if (_hp <= 0)
			{
				_hp = 0;
				Kill();
			}
		}
		return result;
	}

	public void ManageSubtleDamage(int damage, bool isLethal, EElementalWeaknessState weaknessState)
	{
		if (HP <= 0)
		{
			return;
		}
		ENumberColor numberColor = ENumberColor.Red;
		if (base.DefaultTeam != ETeamSide.Heroes)
		{
			switch (weaknessState)
			{
			case EElementalWeaknessState.None:
				numberColor = ENumberColor.White;
				break;
			case EElementalWeaknessState.Strong:
				numberColor = ENumberColor.Gray;
				damage = (int)Math.Ceiling((float)damage * 0.5f);
				break;
			case EElementalWeaknessState.Weak:
				numberColor = ENumberColor.Orange;
				damage *= 2;
				break;
			}
		}
		short num = (short)(HP - damage);
		if (!isLethal && num <= 0)
		{
			num = 1;
		}
		HP = num;
		_level.AddNumber(damage, new Point(Position.X + -8, Bbox.Center.Y), numberColor);
		if (_hp <= 0)
		{
			_hp = 0;
			Kill();
		}
	}

	public virtual bool ManageHeal(float amount, bool shouldShowAnimation)
	{
		bool result = false;
		if (amount > 0f && HP < MaxHP)
		{
			result = true;
			HP += (short)amount;
			if (shouldShowAnimation)
			{
				_healGlowTimer = 1.5f;
			}
			_level.AddNumber((int)amount, Bbox.Center, ENumberColor.Green);
		}
		return result;
	}

	public virtual bool ManageManaRestore(float amount)
	{
		bool result = false;
		if (amount > 0f && MP < MaxMP)
		{
			result = true;
			MP += (int)amount;
			IsMPBeingRestored = true;
		}
		return result;
	}

	public override void ManageState(EAFSM action)
	{
		switch (action)
		{
		case EAFSM.Idle:
			if (_currentState == EAFSM.Idle && !_isGrounded && _isAffectedByGravity)
			{
				if (_currentState == EAFSM.Jumping)
				{
					if (_velocity.Y >= 0f)
					{
						SetState(EAFSM.Falling);
					}
				}
				else
				{
					SetState(EAFSM.Falling);
				}
			}
			if (_currentState == EAFSM.Running || _currentState == EAFSM.Damaged || _currentState == EAFSM.Dashing || _currentState == EAFSM.Backdashing || _currentState == EAFSM.Special)
			{
				SetState(EAFSM.Idle);
			}
			else if (_currentState == EAFSM.Jumping)
			{
				if (_wasJumping || !_isGrounded)
				{
					if (_velocity.Y >= 0f)
					{
						SetState(EAFSM.Falling);
					}
				}
				else if (_isGrounded && !_isJumping)
				{
					SetState((!_isDashing) ? EAFSM.Idle : EAFSM.Dashing);
				}
			}
			else if (_currentState == EAFSM.Skydashing)
			{
				SetState(_isGrounded ? EAFSM.Idle : EAFSM.Falling);
			}
			else if (_currentState == EAFSM.Ducking)
			{
				SetState(EAFSM.Idle);
			}
			else if ((_currentState == EAFSM.Falling || _currentState == EAFSM.Jumping) && _isGrounded)
			{
				SetState((!_isDashing) ? EAFSM.Idle : EAFSM.Dashing);
			}
			else if (IsInWater && _currentState != EAFSM.Dying)
			{
				SetWaterState(EAFSM.Idle);
			}
			break;
		case EAFSM.Moving:
			if (_currentState == EAFSM.Idle || _currentState == EAFSM.Ducking)
			{
				SetState(EAFSM.Running);
			}
			else if (_currentState == EAFSM.Dashing)
			{
				SetState(EAFSM.Running);
			}
			else if ((_currentState == EAFSM.Jumping || _currentState == EAFSM.Falling || _currentState == EAFSM.Skydashing) && _isGrounded)
			{
				SetState(EAFSM.Running);
			}
			else if ((_currentState == EAFSM.Jumping || _currentState == EAFSM.Skydashing) && _velocity.Y >= 0f)
			{
				SetState(EAFSM.Falling);
			}
			else if (!_isGrounded && _isAffectedByGravity && (_currentState == EAFSM.Running || _currentState == EAFSM.Idle || _currentState == EAFSM.Ducking))
			{
				SetState(EAFSM.Falling);
			}
			break;
		case EAFSM.Ducking:
			if (_currentState == EAFSM.Idle || _currentState == EAFSM.Backdashing || _currentState == EAFSM.Running)
			{
				SetState(EAFSM.Ducking);
			}
			else if ((_currentState == EAFSM.Jumping || _currentState == EAFSM.Falling) && _isGrounded && (_wasGrounded || _isFallCrouching))
			{
				SetState(EAFSM.Ducking);
			}
			break;
		case EAFSM.Jumping:
			if (_currentState == EAFSM.Idle || _currentState == EAFSM.Running || _currentState == EAFSM.Ducking || _currentState == EAFSM.Backdashing || _currentState == EAFSM.Grabbing || _currentState == EAFSM.Dashing)
			{
				if (_currentState != EAFSM.Falling)
				{
					SetState(EAFSM.Jumping);
				}
				else if (_currentState == EAFSM.Jumping)
				{
					if (_velocity.Y >= 0f)
					{
						SetState(EAFSM.Falling);
					}
				}
				else
				{
					SetState(EAFSM.Falling);
				}
			}
			else if (_currentState == EAFSM.Jumping || (_currentState == EAFSM.Skydashing && !_isSkydashing))
			{
				if (_isDoubleJumping && !_wasDoubleJumping)
				{
					SetState(EAFSM.Jumping);
				}
				if (_velocity.Y >= 0f && (_wasJumping || !_isGrounded))
				{
					SetState(EAFSM.Falling);
				}
			}
			if (_currentState == EAFSM.Falling && ((_velocity.Y < 0f && !_isTreadingWater) || _isMiracleJumping))
			{
				SetState(EAFSM.Jumping);
			}
			break;
		case EAFSM.Dashing:
			if (_currentState == EAFSM.Idle || _currentState == EAFSM.Running || _currentState == EAFSM.Backdashing || _currentState == EAFSM.Ducking)
			{
				SetState(EAFSM.Dashing);
			}
			break;
		case EAFSM.Skydashing:
			if (_currentState == EAFSM.Idle || _currentState == EAFSM.Running || _currentState == EAFSM.Dashing || _currentState == EAFSM.Backdashing || _currentState == EAFSM.Ducking || _currentState == EAFSM.Falling || _currentState == EAFSM.Jumping)
			{
				SetState(EAFSM.Skydashing);
			}
			break;
		case EAFSM.Grabbing:
			if (_currentState == EAFSM.Falling || _currentState == EAFSM.Jumping || _currentState == EAFSM.Backdashing || _currentState == EAFSM.Dashing || _currentState == EAFSM.Special)
			{
				SetState(EAFSM.Grabbing);
			}
			else if (_currentState != EAFSM.Grabbing)
			{
				IsGrabbing = false;
			}
			break;
		case EAFSM.Backdashing:
			if (_currentState == EAFSM.Idle || _currentState == EAFSM.Moving || _currentState == EAFSM.Ducking || _currentState == EAFSM.Running || _currentState == EAFSM.Dashing || _currentState == EAFSM.Special)
			{
				SetState(EAFSM.Backdashing);
			}
			break;
		case EAFSM.Damaged:
			if (_currentState != EAFSM.Dying)
			{
				_isDashing = false;
				_isSkydashing = false;
				SetState(EAFSM.Damaged);
			}
			break;
		case EAFSM.Dying:
			_isDashing = false;
			_isSkydashing = false;
			SetState(EAFSM.Dying);
			break;
		case EAFSM.Special:
			if (_currentState != EAFSM.Dying)
			{
				_isDashing = false;
				if (_isSkydashing)
				{
					_overrideFallSpeedAmount = 0f;
					_velocity.Y *= 0.5f;
					_isSkydashing = false;
				}
				SetState(EAFSM.Special);
			}
			break;
		}
	}

	public virtual void SetState(EAFSM state)
	{
		SetState(state, 0);
	}

	public virtual void SetState(EAFSM state, int newFrame)
	{
		if (_currentState != state)
		{
			_currentState = state;
			_animationIndex = newFrame;
			_animationState = 0;
			_isAnimationDone = false;
		}
	}

	internal virtual void SetWaterState(EAFSM state)
	{
	}

	protected virtual void UpdateChargingAnimation()
	{
		_wasCharging = _isCharging;
	}

	protected virtual void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
	}

	public virtual void StartAbility(int whichAbility)
	{
		AbilitiesUsedList.Add(whichAbility);
		if (AbilitiesUsedList.Count > 20)
		{
			AbilitiesUsedList.RemoveAt(0);
		}
	}

	public virtual void TeleportToPoint(Point moveTo)
	{
		Position = moveTo;
		SnapBboxToPosition();
	}

	protected override void CarryOutScriptAction(ScriptAction inAction, float delta)
	{
		if (inAction.ActionType == EScriptActionType.GoToPoint)
		{
			int num = (int)inAction.Arguments.X;
			int num2 = Position.X - num;
			float num3 = _agility * (float)((num2 < 0) ? 1 : (-1));
			int num4 = Position.X + (int)Math.Ceiling(delta * delta * _moveAcceleration * num3);
			int num5 = num4 - num;
			if (num2 == 0 || num5 == 0 || num2 < 0 != num5 < 0 || inAction.ActionTimer <= 0f || inAction.IsBeingSkipped)
			{
				TeleportToPoint(new Point(num, Position.Y));
				_velocity = Vector2.Zero;
				_movementX = 0f;
				inAction.ActionTimer = 0f;
				ManageState(EAFSM.Idle);
			}
			else if (IsGrabbing)
			{
				TeleportToPoint(new Point(num, (int)inAction.Arguments.Y));
				_velocity = Vector2.Zero;
				_movementX = 0f;
				inAction.ActionTimer = 0f;
				StopGrabbing();
				SetState(EAFSM.Idle);
			}
			else
			{
				DoHorizontalRun(num3);
				inAction.ActionTimer += delta;
			}
		}
		else if (inAction.ActionType == EScriptActionType.LookDirection)
		{
			if (IsFacingLeft != inAction.Arguments.X < 0f)
			{
				IsFacingLeft = !IsFacingLeft;
				DoTurningAroundAnimation(_currentState, !IsFacingLeft);
			}
		}
		else if (inAction.ActionType == EScriptActionType.WarpToPoint)
		{
			Position = new Point((int)inAction.Arguments.X, (int)inAction.Arguments.Y);
			if (_isAffectedByGravity)
			{
				_isGrounded = true;
				_wasGrounded = true;
				_velocity = Vector2.Zero;
			}
		}
		base.CarryOutScriptAction(inAction, delta);
	}

	protected virtual bool CheckIfFloorEnds()
	{
		bool result = false;
		Point point = new Point(Position.X / 16, (Position.Y - 1) / 16);
		if (!_level.SolidTiles.ContainsKey(point))
		{
			point = point.Add(0, 1);
		}
		bool flag = false;
		if (_level.SolidTiles.ContainsKey(point))
		{
			flag = _level.SolidTiles[point].Type == ETileType.Platform;
		}
		EDirection where = ((!_isMovingLeft) ? (flag ? EDirection.East : EDirection.SouthEast) : (flag ? EDirection.West : EDirection.SouthWest));
		if (!_level.CheckNearby(where, point))
		{
			result = true;
		}
		return result;
	}

	protected virtual bool CheckForStraightShot(Point target)
	{
		int num = Position.X / 16;
		int num2 = target.X / 16;
		int y = (Position.Y - 1) / 16;
		if (num > num2)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		for (int i = num; i < num2; i++)
		{
			if (_level.SolidTiles.ContainsKey(new Point(i, y)))
			{
				return false;
			}
		}
		return true;
	}

	protected bool IsStandingOnPlatform()
	{
		bool flag = false;
		int y = _bbox.Bottom / 16;
		int num = _bbox.Left / 16;
		int num2 = (_bbox.Left + _bbox.Width - 1) / 16;
		for (int i = num; i <= num2; i++)
		{
			Point start = new Point(i, y);
			switch (_level.CheckNearbyType(EDirection.Center, start))
			{
			case ETileType.Platform:
				flag = true;
				continue;
			case ETileType.Passable:
				continue;
			}
			flag = false;
			break;
		}
		if (!flag)
		{
			foreach (GameEvent currentMovingPlatform in base.CurrentMovingPlatforms)
			{
				if (currentMovingPlatform != null && currentMovingPlatform.IsConsideredPassablePlatform)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public virtual Rectangle GetFirstInsectingBbox(Rectangle targetBbox)
	{
		Rectangle result = Rectangle.Empty;
		if (_outerBbox.IsEmpty)
		{
			result = Bbox;
		}
		else
		{
			List<Rectangle> list = FindIntersectingBoundingBoxes(targetBbox, 1);
			if (list.Count > 0)
			{
				result = list[0];
			}
		}
		return result;
	}

	internal virtual void StopGrabbing()
	{
		IsGrabbing = false;
	}

	internal void UpdateStatusEffects(float delta)
	{
		for (int num = _statusEffects.Count - 1; num >= 0; num--)
		{
			BaseStatusEffect baseStatusEffect = _statusEffects[num];
			baseStatusEffect.Update(delta);
			if (baseStatusEffect.IsFinished)
			{
				_statusEffects.RemoveAt(num);
			}
		}
	}

	internal void DrawStatusEffects(SpriteBatch spriteBatch)
	{
		foreach (BaseStatusEffect statusEffect in _statusEffects)
		{
			statusEffect.Draw(spriteBatch);
		}
	}

	internal virtual bool GiveStatusEffect(EStatusEffectType statusType, int power)
	{
		bool flag = !HasStatusEffect(statusType, checkForLife: false);
		if (flag)
		{
			BaseStatusEffect baseStatusEffect = BaseStatusEffect.FromType(statusType, this, _level, base.BaseType == EGameObjectBaseType.Hero, power);
			if (baseStatusEffect != null)
			{
				_statusEffects.Add(baseStatusEffect);
			}
			else
			{
				flag = false;
			}
		}
		else if (statusType == EStatusEffectType.Suffocate || statusType == EStatusEffectType.Burn)
		{
			foreach (BaseStatusEffect statusEffect in _statusEffects)
			{
				if (statusEffect != null && statusEffect.StatusEffectType == statusType)
				{
					statusEffect.Refresh();
				}
			}
			flag = true;
		}
		return flag;
	}

	internal bool HasStatusEffect(EStatusEffectType statusType, bool checkForLife)
	{
		bool result = false;
		foreach (BaseStatusEffect statusEffect in _statusEffects)
		{
			if (statusEffect != null && statusEffect.StatusEffectType == statusType)
			{
				if (!checkForLife || !statusEffect.IsFadingOut)
				{
					result = true;
				}
				break;
			}
		}
		return result;
	}

	internal EStatusEffectType GetFirstActiveStatusEffect()
	{
		EStatusEffectType result = EStatusEffectType.None;
		foreach (BaseStatusEffect statusEffect in StatusEffects)
		{
			if (!statusEffect.IsFadingOut && !statusEffect.IsFinished)
			{
				result = statusEffect.StatusEffectType;
				break;
			}
		}
		return result;
	}

	internal void HealStatus(EStatusEffectType statusType)
	{
		foreach (BaseStatusEffect statusEffect in _statusEffects)
		{
			if (statusType == EStatusEffectType.All || statusEffect.StatusEffectType == statusType)
			{
				statusEffect.Kill();
			}
		}
	}
}
