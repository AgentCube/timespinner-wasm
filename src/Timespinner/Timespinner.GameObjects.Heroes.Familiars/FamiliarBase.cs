using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars;

public class FamiliarBase : Protagonist
{
	internal enum EFamiliarAIStateType
	{
		Following,
		Attacking,
		Casting,
		Sitting,
		GoTo,
		CutsceneStatic,
		CutsceneFlyAround,
		CutsceneHover
	}

	internal enum EFamiliarGoToType
	{
		Sit,
		Target,
		Leash,
		TargetPoint
	}

	private const int DefaultOrbitRadius = 16;

	private const int SnapPlayableFamiliarOffsetY = -8;

	private const int PlayerSpellMeleeHitThreshold = 5;

	private const int PlayerSpellGlowingFrequency = 3;

	private const int SpellReadyParticlesCount = 16;

	private const float OrbitFrequency = 1.75f;

	private const float OrbitRadiusGrowthSpeed = 10f;

	private const float LeashBaseShrinkRate = 1f;

	private const float LeashCatchUpGrowthRate = 0.5f;

	private const float LeashCatchUpShrinkRate = 3f;

	private const float MaxLeashCatchUpMultiplier = 5f;

	private const float MaxLeashLength = 200f;

	private const float LeashCatchUpThresholdDistance = 32f;

	private const float TimeForParentOffsetToEase = 0.5f;

	private const float TimeBeforeLandingIdle = 8f;

	private const float TimeToWaitBeforeSearchingForTarget = 2f;

	private const float DefaultTimeBeforeCastingSpell = 15f;

	private const float TimeToFadeOutWhenLunaisDies = 0.5f;

	private const float TimeToEaseIntoCutsceneHover = 1f;

	private const float CutsceneFlyAroundFrequency = 2f;

	private const float PlayerStartStopCooldown = 1f;

	private const float PlayerFloatingFrequency = 2f;

	private const float PlayerFloatingAmplitude = 2f;

	private const float PlayerFloatingBiasY = 1.5f;

	private const float PlayerMovementAccelerationMultiplier = 5f;

	private const float PlayerSwitchFamiliarCooldown = 0.5f;

	private const float TimeBetweenPlayerSpellReadyParticles = 0.1f;

	private const float FrameMultiplier = 60f;

	private static readonly Point DefaultParentOffset = new Point(40, -40);

	private readonly EInventoryFamiliarType _familiarType;

	private readonly FamiliarSpellLeakParticleSystem _spellReadyParticles;

	private readonly HaloRingAnimation _haloRingAnimation;

	private readonly LunaisObj _parentObject;

	private readonly HashSet<int> _hitEnemyRegistry = new HashSet<int>();

	private readonly Action<bool> _switchFamiliarAction;

	private bool _wasParentFacingLeft;

	private bool _hasStartedAttack;

	private bool _isDoingLunaisDeathSequence;

	private bool _isShowingHaloAnimation;

	private bool _isGoToUsingCosine;

	private bool _wasInCutscene;

	private EFamiliarGoToType _goToType;

	private EFamiliarAIStateType _goToEndAction;

	private PlayerIndex _lastPlayerIndex = PlayerIndex.Two;

	private int _playerMeleeHitCount;

	private float _playerSpellGlowTimer;

	private float _parentOffsetEaseTimer = 0.5f;

	private float _orbitTimer;

	private float _orbitRadius;

	private float _leashCatchUpMultiplier;

	private float _leashLength;

	private float _rollingAverageChangeInX;

	private float _nextActionTimer;

	private float _timeSinceParentMoved;

	private float _goToTimer;

	private float _goToTotalTime;

	private float _lunaisDeathSequenceTimer;

	private float _cutsceneFlyAroundRadius;

	private float _cutsceneFlyAroundTimer;

	private float _playerStartStopTimer;

	private float _playerFloatingTimer;

	private float _playerSwitchFamiliarTimer;

	private float _playerSpellReadyParticlesTimer;

	private Point _leashOffset;

	private Point _leashPosition;

	private Point _orbitOffset;

	private Point _parentOffsetEased;

	private Point _goToStart;

	private Point _goToTarget;

	private Point _cutsceneFlyAroundCenter;

	private Point _lastParentBasePosition;

	private Vector2 _leashVector;

	private InventoryFamiliar _familiarItem;

	internal bool IsParentFacingLeft { get; set; }

	internal EInventoryFamiliarType FamiliarType => _familiarType;

	internal EFamiliarAIStateType LastFamiliarAIState { get; private set; }

	internal EFamiliarAIStateType FamiliarAIState { get; set; }

	internal PlayerIndex PlayerIndex => _lastPlayerIndex;

	internal int Damage { get; private set; }

	internal int OrbitRadius { get; set; }

	internal float SpellCastTimer { get; set; }

	internal float TimeBeforeCastingSpell { get; set; }

	internal float OrbitTimer => _orbitTimer;

	internal Point ParentOffset { get; set; }

	internal Point ParentBasePosition { get; set; }

	internal Point ParentPosition => ParentBasePosition.Add(_parentOffsetEased);

	internal Point SittingOffset { get; set; }

	internal Point OrbitOffset => _orbitOffset;

	internal Point LeashOffset => _leashOffset;

	internal Point LeashPosition => _leashPosition;

	internal Vector2 OrbitDimensionMultiplier { get; set; }

	internal Color BaseAuraColor { get; set; }

	internal InventoryFamiliar FamiliarItem => _familiarItem;

	internal Mobile ParentObject => _parentObject;

	internal Monster SpellTargetMonster { get; set; }

	internal HashSet<int> HitEnemyRegistry => _hitEnemyRegistry;

	internal FamiliarBase(Point inPosition, Level inLevel, SpriteSheet inSprite, LunaisObj parentObject, EInventoryFamiliarType familiarType, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, 2, -1, isPrimaryPlayer: false)
	{
		_switchFamiliarAction = switchFamiliar;
		_familiarType = familiarType;
		_parentObject = parentObject;
		_leashPosition = inPosition;
		ParentOffset = DefaultParentOffset;
		FamiliarAIState = EFamiliarAIStateType.Following;
		OrbitRadius = 16;
		OrbitDimensionMultiplier = Vector2.One;
		TimeBeforeCastingSpell = 15f;
		SpellCastTimer = TimeBeforeCastingSpell;
		_maxMoveSpeed = 500f;
		_isPlayable = false;
		_isFlying = true;
		_isAffectedByGravity = false;
		_isIgnoringPlatform = true;
		Bbox = new Rectangle(0, 0, 16, 21);
		_bboxOffset = new Point(3, 5);
		SittingOffset = new Point(0, -6);
		RefreshStats();
		_haloRingAnimation = new HaloRingAnimation(_level);
		_spellReadyParticles = new FamiliarSpellLeakParticleSystem(_level.GCM.TxParticleEnergy, 16);
		_particleSystems.Add(_spellReadyParticles);
	}

	internal static FamiliarBase FromFamiliarType(Point inPosition, LunaisObj parentObject, EInventoryFamiliarType familiarType, Action<bool> switchFamiliar)
	{
		FamiliarBase result = null;
		Level level = parentObject.Level;
		switch (familiarType)
		{
		case EInventoryFamiliarType.Meyef:
		{
			SpriteSheet inSprite2 = (level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.FamiliarAltMeyef) ? level.GCM.SpFamiliarAltMeyef : level.GCM.SpFamiliarMeyef);
			result = new FamiliarMeyef(inPosition, level, parentObject, inSprite2, switchFamiliar);
			break;
		}
		case EInventoryFamiliarType.MerchantCrow:
		{
			SpriteSheet inSprite = (level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.FamiliarAltCrow) ? level.GCM.SpFamiliarAltCrow : level.GCM.SpFamiliarCrow);
			result = new FamiliarCrow(inPosition, level, parentObject, inSprite, switchFamiliar);
			break;
		}
		case EInventoryFamiliarType.Griffin:
			result = new FamiliarGriffin(inPosition, level, parentObject, level.GCM.SpFamiliarGriffin, switchFamiliar);
			break;
		case EInventoryFamiliarType.Kobo:
			result = new FamiliarKobo(inPosition, level, parentObject, level.GCM.SpFamiliarKobo, switchFamiliar);
			break;
		case EInventoryFamiliarType.Sprite:
			result = new FamiliarSprite(inPosition, level, parentObject, level.GCM.SpFamiliarSprite, switchFamiliar);
			break;
		case EInventoryFamiliarType.Demon:
			result = new FamiliarDemon(inPosition, level, parentObject, level.GCM.SpFamiliarDemon, switchFamiliar);
			break;
		}
		return result;
	}

	public override void Update(float delta)
	{
		if (_playerStartStopTimer > 0f)
		{
			_playerStartStopTimer -= delta;
		}
		if (_playerSwitchFamiliarTimer < 0.5f)
		{
			_playerSwitchFamiliarTimer += delta;
		}
		_isDoingLunaisDeathSequence = _level.IsDoingPlayerDeathCutscene;
		if (!_isDoingLunaisDeathSequence)
		{
			UpdateParentData(delta);
			if (_level.IsTimeFrozen && !_isFrozen)
			{
				if (!_doesDrawTrail)
				{
					_doesDrawTrail = true;
					_trailFadeRate = 1.35f;
					_trailLength = 5;
					_drawHistories.Clear();
				}
			}
			else
			{
				_doesDrawTrail = false;
			}
			if (!base.IsFrozen)
			{
				if (FamiliarAIState != EFamiliarAIStateType.Casting)
				{
					UpdateOrbit(delta);
				}
				if (FamiliarAIState != EFamiliarAIStateType.CutsceneStatic)
				{
					bool flag = _level.IsPlayerInputBlocked || FamiliarAIState == EFamiliarAIStateType.CutsceneFlyAround || FamiliarAIState == EFamiliarAIStateType.CutsceneHover;
					if (base.IsPlayable && !flag)
					{
						UpdatePlayerFloating(delta);
						UpdatePlayerSpellGlowing(delta);
						SnapFamiliarToVisibleArea();
					}
					else
					{
						if (!_wasInCutscene && base.IsPlayable)
						{
							ResetLeash();
						}
						base.IsGlowing = false;
						_isInvulnerable = false;
						UpdateAI(delta);
					}
					_wasInCutscene = flag;
				}
				if (SpellCastTimer > 0f)
				{
					SpellCastTimer -= delta;
					if (SpellCastTimer < 0f)
					{
						SpellCastTimer = 0f;
					}
				}
				if (_isShowingHaloAnimation)
				{
					_haloRingAnimation.Update(delta);
					if (_haloRingAnimation.IsFinished)
					{
						_isShowingHaloAnimation = false;
					}
				}
			}
			base.Update(delta);
		}
		else
		{
			base.IsGlowing = false;
			if (_lunaisDeathSequenceTimer < 0.5f)
			{
				_lunaisDeathSequenceTimer += delta;
				float num = 1f;
				if (_lunaisDeathSequenceTimer < 0.5f)
				{
					num = _lunaisDeathSequenceTimer / 0.5f;
				}
				base.DrawColor = Color.White * (1f - num);
			}
		}
		if (base.DoesDrawAura)
		{
			float num2 = (float)(int)base.DrawColor.A / 255f;
			base.AuraColor = BaseAuraColor * num2;
		}
		_isInvulnerable = true;
	}

	private void UpdateParentData(float delta)
	{
		bool flag = !_parentObject.IsDrawingSelf;
		bool shouldHideFamiliarToo = _parentObject.ShouldHideFamiliarToo;
		_doesDrawSpriteAndAppendages = !flag || !shouldHideFamiliarToo;
		_lastParentBasePosition = ParentBasePosition;
		IsParentFacingLeft = _parentObject.IsFacingLeft;
		ParentBasePosition = _parentObject.Position;
		Point point = _lastParentBasePosition.Subtract(ParentBasePosition);
		int num = Math.Abs(point.X) + Math.Abs(point.Y);
		if (num > 1)
		{
			_timeSinceParentMoved = 0f;
		}
		else if (_timeSinceParentMoved < 8f)
		{
			_timeSinceParentMoved += delta;
		}
		if (IsParentFacingLeft != _wasParentFacingLeft)
		{
			_parentOffsetEaseTimer = 0.5f;
		}
		if (_parentOffsetEaseTimer > 0f)
		{
			_parentOffsetEaseTimer -= delta;
			if (_parentOffsetEaseTimer < 0f)
			{
				_parentOffsetEaseTimer = 0f;
			}
			float amount = 1f - _parentOffsetEaseTimer / 0.5f;
			int x = (int)MathHelper.Lerp(_parentOffsetEased.X, IsParentFacingLeft ? ParentOffset.X : (-ParentOffset.X), amount);
			_parentOffsetEased = new Point(x, ParentOffset.Y);
		}
		_wasParentFacingLeft = IsParentFacingLeft;
	}

	protected override void ProcessKeys(float delta)
	{
		base.ProcessKeys(delta);
		if (!base.IsPlayable)
		{
			return;
		}
		if (!_isCarryingOutAbility)
		{
			if (_gamePadWrapper.IsLeftDown && _currentState != EAFSM.Grabbing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying)
			{
				ManageState(EAFSM.Moving);
				IsFacingLeft = true;
				if (_movementX > -1f)
				{
					_movementX -= delta * 5f;
					if (_movementX < -1f)
					{
						_movementX = -1f;
					}
				}
			}
			if (_gamePadWrapper.IsRightDown && _currentState != EAFSM.Grabbing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying)
			{
				ManageState(EAFSM.Moving);
				IsFacingLeft = false;
				if (_movementX < 1f)
				{
					_movementX += delta * 5f;
					if (_movementX > 1f)
					{
						_movementX = 1f;
					}
				}
			}
			if (_gamePadWrapper.IsUpDown && _currentState != EAFSM.Grabbing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying && _movementY > -1f)
			{
				_movementY -= delta * 5f;
				if (_movementY < -1f)
				{
					_movementY = -1f;
				}
			}
			if (_gamePadWrapper.IsDownDown && _currentState != EAFSM.Grabbing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying && _movementY < 1f)
			{
				_movementY += delta * 5f;
				if (_movementY > 1f)
				{
					_movementY = 1f;
				}
			}
			if (_gamePadWrapper.IsMeleeDown && !_gamePadWrapper.WasMeleeDown)
			{
				StartAbility(0);
				_movementX = 0f;
				_movementY = 0f;
			}
			if (_gamePadWrapper.IsSpellDown && !_gamePadWrapper.WasSpellDown && _playerMeleeHitCount >= 5)
			{
				_playerMeleeHitCount = 0;
				TargetNearestEnemy();
				StartAbility(1);
				_movementX = 0f;
				_movementY = 0f;
			}
			if (!_isCarryingOutAbility && _playerSwitchFamiliarTimer >= 0.5f)
			{
				if (_gamePadWrapper.IsDashDown && !_gamePadWrapper.WasDashDown)
				{
					_switchFamiliarAction(obj: true);
				}
				else if (_gamePadWrapper.IsBackdashDown && !_gamePadWrapper.WasBackdashDown)
				{
					_switchFamiliarAction(obj: false);
				}
			}
		}
		if (_gamePadWrapper.IsStartDown && !_gamePadWrapper.WasStartDown && _playerStartStopTimer <= 0f)
		{
			SetIsPlayable(isPlayable: false, isSwitching: false);
		}
		if (!_gamePadWrapper.IsLeftDown && !_gamePadWrapper.IsRightDown && !_gamePadWrapper.IsDownDown && !_gamePadWrapper.IsUpDown && (!_gamePadWrapper.IsJumpDown || _isGrounded) && (!_gamePadWrapper.IsSpellDown || base.IsCharging) && !_isCarryingOutAbility && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying)
		{
			ManageState(EAFSM.Idle);
		}
		if ((!_gamePadWrapper.IsLeftDown && !_gamePadWrapper.IsRightDown) || _currentState == EAFSM.Damaged || _currentState == EAFSM.Dying)
		{
			_movementX = 0f;
		}
		if ((!_gamePadWrapper.IsUpDown && !_gamePadWrapper.IsDownDown) || _currentState == EAFSM.Damaged || _currentState == EAFSM.Dying)
		{
			_movementY = 0f;
		}
	}

	private void UpdatePlayerFloating(float delta)
	{
		_playerFloatingTimer += delta * 2f;
		if (_playerFloatingTimer >= (float)Math.PI * 2f)
		{
			_playerFloatingTimer -= (float)Math.PI * 2f;
		}
		float num = delta * 60f;
		float x = (float)Math.Cos(_playerFloatingTimer) * 2f * num;
		float y = (float)Math.Sin(_playerFloatingTimer) * 2f * 1.5f * num;
		_velocity = Vector2.Add(_velocity, new Vector2(x, y));
	}

	private void UpdatePlayerSpellGlowing(float delta)
	{
		if (_playerMeleeHitCount > 0 && !_isBlockingPlayerInput)
		{
			if (_playerMeleeHitCount > 5)
			{
				_playerMeleeHitCount = 5;
			}
			if (_playerMeleeHitCount == 5)
			{
				_playerSpellReadyParticlesTimer += delta;
				if (_playerSpellReadyParticlesTimer >= 0.1f)
				{
					_playerSpellReadyParticlesTimer -= 0.1f;
					_spellReadyParticles.AddParticles(Bbox.Center.ToVector2());
				}
			}
			_playerSpellGlowTimer += delta * (float)(1 + _playerMeleeHitCount * 3);
			if (_playerSpellGlowTimer > (float)Math.PI * 2f)
			{
				_playerSpellGlowTimer -= (float)Math.PI * 2f;
			}
			base.IsGlowing = true;
			base.GlowColor = new Color(1f, 1f, 1f, (float)Math.Cos(_playerSpellGlowTimer) * 0.2f + 0.8f);
			base.GlowBase = 1f + 0.25f * ((float)_playerMeleeHitCount / 5f);
		}
		else
		{
			base.IsGlowing = false;
			base.DrawColor = Color.White;
		}
	}

	private void UpdateAI(float delta)
	{
		if (_isCarryingOutAbility)
		{
			return;
		}
		LastFamiliarAIState = FamiliarAIState;
		if (FamiliarAIState != EFamiliarAIStateType.GoTo && FamiliarAIState != EFamiliarAIStateType.Attacking && FamiliarAIState != EFamiliarAIStateType.Casting && FamiliarAIState != EFamiliarAIStateType.CutsceneFlyAround && FamiliarAIState != EFamiliarAIStateType.CutsceneHover)
		{
			if (_nextActionTimer > 0f)
			{
				_nextActionTimer -= delta;
			}
			if (_nextActionTimer <= 0f)
			{
				if (!_level.IsPlayerInputBlocked)
				{
					DecideNextAction(delta);
				}
				_nextActionTimer = 2f;
			}
			else if (FamiliarAIState == EFamiliarAIStateType.Sitting && _timeSinceParentMoved <= 8f)
			{
				FamiliarAIState = EFamiliarAIStateType.Following;
				SetState(EAFSM.Idle);
				StartGoToPoint(1f, EFamiliarAIStateType.Following, EFamiliarGoToType.Leash);
			}
		}
		switch (FamiliarAIState)
		{
		case EFamiliarAIStateType.Following:
			UpdateFollowPlayer(delta);
			break;
		case EFamiliarAIStateType.Attacking:
			UpdateAttackTarget(delta);
			break;
		case EFamiliarAIStateType.Casting:
			UpdateCasting(delta);
			break;
		case EFamiliarAIStateType.Sitting:
			UpdateSitting();
			break;
		case EFamiliarAIStateType.GoTo:
			UpdateGoToPoint(delta);
			break;
		case EFamiliarAIStateType.CutsceneFlyAround:
			UpdateCutsceneFlyAround(delta);
			break;
		case EFamiliarAIStateType.CutsceneHover:
			UpdateCutsceneHover(delta);
			break;
		case EFamiliarAIStateType.CutsceneStatic:
			break;
		}
	}

	protected virtual void DecideNextAction(float delta)
	{
		Monster nearestVisibleAggroedEnemy = _level.GetNearestVisibleAggroedEnemy(Bbox.Center);
		if (!_level.IsTimeFrozen && nearestVisibleAggroedEnemy != null && nearestVisibleAggroedEnemy.IsAggroed && LastFamiliarAIState != EFamiliarAIStateType.Attacking)
		{
			_currentTarget = nearestVisibleAggroedEnemy;
			StartGoToPoint(0.75f, EFamiliarAIStateType.Attacking, EFamiliarGoToType.Target);
		}
		else if (FamiliarAIState == EFamiliarAIStateType.Following && _timeSinceParentMoved > 8f && !ParentObject.IsInWater)
		{
			_currentTarget = _level.GetNearestSolidTile(Position, EDirection.South, 5);
			if (_currentTarget != null)
			{
				StartGoToPoint(2f, EFamiliarAIStateType.Sitting, EFamiliarGoToType.Sit);
			}
		}
		else if (FamiliarAIState != EFamiliarAIStateType.Sitting)
		{
			FamiliarAIState = EFamiliarAIStateType.Following;
		}
	}

	protected virtual void UpdateAttackTarget(float delta)
	{
		if (!_hasStartedAttack)
		{
			_hasStartedAttack = true;
			StartAbility(0);
			return;
		}
		UpdateLeash(0f);
		IsFacingLeft = _leashPosition.X < Position.X;
		StartGoToPoint(1f, EFamiliarAIStateType.Following, EFamiliarGoToType.Leash);
		_hasStartedAttack = false;
	}

	protected virtual void UpdateCasting(float delta)
	{
		if (!_isCarryingOutAbility)
		{
			FamiliarAIState = EFamiliarAIStateType.Following;
		}
	}

	private void UpdateFollowPlayer(float delta)
	{
		Point leashPosition = _leashPosition;
		UpdateLeash(delta);
		Point end = _leashPosition.Add(_orbitOffset);
		Position = Position.Lerp(end, 0.33f);
		_rollingAverageChangeInX = (_rollingAverageChangeInX + (float)(leashPosition.X - _leashPosition.X)) / 2f;
		float num = Math.Abs(_rollingAverageChangeInX);
		if (_leashOffset.X < 2 && _parentOffsetEaseTimer <= 0f)
		{
			IsFacingLeft = IsParentFacingLeft;
		}
		else
		{
			IsFacingLeft = _rollingAverageChangeInX > 0f;
		}
		ManageState((num < 2f) ? EAFSM.Idle : EAFSM.Moving);
	}

	private void UpdateGoToPoint(float delta)
	{
		switch (_goToType)
		{
		case EFamiliarGoToType.Sit:
			if (_currentTarget != null && _goToTimer <= 0f)
			{
				int x = _currentTarget.Bbox.Center.X;
				if (_currentTarget is Tile { Type: ETileType.Slope } tile)
				{
					int num3 = tile.LookupTileHeight(x);
					_goToTarget = new Point(x, tile.Bbox.Top + num3).Add(SittingOffset);
				}
				else
				{
					_goToTarget = new Point(x, _currentTarget.Bbox.Top).Add(SittingOffset);
				}
			}
			break;
		case EFamiliarGoToType.Target:
			if (_currentTarget != null)
			{
				bool flag = _goToStart.X < _currentTarget.Position.X;
				IsFacingLeft = !flag;
				Rectangle rectangle = ((_currentTarget is Mobile mobile) ? mobile.OuterBbox : _currentTarget.Bbox);
				int num = rectangle.Width / 4;
				int num2 = rectangle.Height / 4;
				_goToTarget = new Point(flag ? (rectangle.Left + num) : (rectangle.Right - num), rectangle.Center.Y + num2);
			}
			break;
		case EFamiliarGoToType.Leash:
			UpdateLeash(delta);
			_goToTarget = _leashPosition.Add(_orbitOffset);
			break;
		case EFamiliarGoToType.TargetPoint:
			ManageState(EAFSM.Moving);
			break;
		}
		_goToTimer += delta;
		if (_goToTimer >= _goToTotalTime || _goToTotalTime <= 0f)
		{
			Position = _goToTarget;
			FamiliarAIState = _goToEndAction;
			_goToTimer = 0f;
			switch (_goToEndAction)
			{
			case EFamiliarAIStateType.Sitting:
				PlayCue(ESFX.FamiliarAfkBegin);
				DoSittingAnimation();
				break;
			case EFamiliarAIStateType.Casting:
				StartAbility(1);
				break;
			case EFamiliarAIStateType.CutsceneStatic:
			case EFamiliarAIStateType.CutsceneFlyAround:
			case EFamiliarAIStateType.CutsceneHover:
				ManageState(EAFSM.Idle);
				break;
			case EFamiliarAIStateType.GoTo:
				break;
			}
		}
		else
		{
			float amount = _goToTimer / _goToTotalTime;
			Position = (_isGoToUsingCosine ? _goToStart.CosInterpolate(_goToTarget, amount) : _goToStart.SineInterpolate(_goToTarget, amount));
		}
	}

	private void UpdateSitting()
	{
		if (LastFamiliarAIState != EFamiliarAIStateType.Sitting)
		{
			PlayCue(ESFX.FamiliarAfkBegin);
			DoSittingAnimation();
			_leashPosition = Position;
			_orbitRadius = 0f;
		}
	}

	private void UpdateCutsceneFlyAround(float delta)
	{
		Point position = Position;
		_cutsceneFlyAroundTimer += delta * 2f;
		if (_cutsceneFlyAroundTimer >= (float)Math.PI * 2f)
		{
			_cutsceneFlyAroundTimer -= (float)Math.PI * 2f;
		}
		int num = (int)Math.Round(Math.Sin(_cutsceneFlyAroundTimer * 2f) * (double)_cutsceneFlyAroundRadius);
		int num2 = (int)Math.Round(Math.Cos(_cutsceneFlyAroundTimer) * (double)_cutsceneFlyAroundRadius);
		Position = MathEx.Lerp(end: new Point(_cutsceneFlyAroundCenter.X + num2, _cutsceneFlyAroundCenter.Y + num), start: Position, amount: 0.33f);
		int num3 = position.X - Position.X;
		if (Math.Abs(num3) > 2 && _cutsceneFlyAroundRadius > 16f)
		{
			IsFacingLeft = num3 > 0;
		}
		_timeSinceParentMoved = 0f;
		_leashCatchUpMultiplier = 0f;
	}

	private void UpdateCutsceneHover(float delta)
	{
		Point end;
		if (_cutsceneFlyAroundTimer < 1f)
		{
			_cutsceneFlyAroundTimer += delta;
			float num = 1f;
			if (_cutsceneFlyAroundTimer < 1f)
			{
				num = _cutsceneFlyAroundTimer / 1f;
			}
			end = MathEx.Add(b: new Point((int)((float)_orbitOffset.X * num), (int)((float)_orbitOffset.Y * num)), a: _goToTarget);
		}
		else
		{
			end = _goToTarget.Add(_orbitOffset);
		}
		Position = Position.Lerp(end, 0.33f);
		_timeSinceParentMoved = 0f;
		_leashCatchUpMultiplier = 0f;
	}

	private void UpdateOrbit(float delta)
	{
		_orbitTimer += delta * 1.75f;
		if (_orbitTimer > (float)Math.PI * 2f)
		{
			_orbitTimer -= (float)Math.PI * 2f;
		}
		if (_orbitRadius < (float)OrbitRadius)
		{
			_orbitRadius += delta * 10f;
			if (_orbitRadius > (float)OrbitRadius)
			{
				_orbitRadius = OrbitRadius;
			}
		}
		int num = -(int)(Math.Cos(_orbitTimer) * (double)_orbitRadius * (double)OrbitDimensionMultiplier.X);
		int y = (int)(Math.Sin(_orbitTimer) * (double)_orbitRadius * (double)OrbitDimensionMultiplier.Y);
		if (_parentOffsetEaseTimer > 0f)
		{
			float amount = 1f - _parentOffsetEaseTimer / 0.5f;
			num = (int)MathHelper.Lerp(_orbitOffset.X, IsParentFacingLeft ? num : (-num), amount);
		}
		else
		{
			num = (IsParentFacingLeft ? num : (-num));
		}
		_orbitOffset = new Point(num, y);
	}

	private void UpdateLeash(float delta)
	{
		Point parentPosition = ParentPosition;
		Vector2 value = new Vector2(_leashPosition.X - parentPosition.X, _leashPosition.Y - parentPosition.Y);
		_leashLength = 0f;
		float num = value.Length();
		if (num > 0f)
		{
			_leashLength = ((num > 200f) ? 200f : num);
			_leashVector = Vector2.Normalize(value);
		}
		if (_leashLength > 32f)
		{
			_leashCatchUpMultiplier += delta * 0.5f;
			if (_leashCatchUpMultiplier > 5f)
			{
				_leashCatchUpMultiplier = 5f;
			}
		}
		else if (_leashCatchUpMultiplier > 0f)
		{
			_leashCatchUpMultiplier -= delta * 3f;
			if (_leashCatchUpMultiplier < 0f)
			{
				_leashCatchUpMultiplier = 0f;
			}
		}
		if (_leashLength > 0f)
		{
			_leashLength -= delta * _leashLength * (1f + _leashCatchUpMultiplier);
			if (_leashLength < 0f)
			{
				_leashLength = 0f;
			}
		}
		_leashOffset = (_leashVector * _leashLength).ToPoint();
		_leashPosition = ParentPosition.Add(_leashOffset);
	}

	private void SnapFamiliarToVisibleArea()
	{
		if (_level.VisibleArea.Contains(Bbox))
		{
			return;
		}
		Point zero = Point.Zero;
		if (Bbox.Left < _level.VisibleArea.Left)
		{
			zero.X = Bbox.Left - _level.VisibleArea.Left;
			IsFacingLeft = false;
		}
		else if (Bbox.Right > _level.VisibleArea.Right)
		{
			zero.X = Bbox.Right - _level.VisibleArea.Right;
			IsFacingLeft = true;
		}
		if (Bbox.Top < _level.VisibleArea.Top)
		{
			zero.Y = Bbox.Top - _level.VisibleArea.Top;
		}
		else if (Bbox.Bottom > _level.VisibleArea.Bottom)
		{
			zero.Y = Bbox.Bottom - _level.VisibleArea.Bottom;
		}
		if (zero != Point.Zero)
		{
			Position = Position.Subtract(zero);
			SnapBboxToPosition();
			if (DetectTileCollisions())
			{
				AddPoofAnimation(_level, Bbox.Center);
				Position = new Point(_lastParentBasePosition.X, _lastParentBasePosition.Y + -8);
				SnapBboxToPosition();
				AddPoofAnimation(_level, Bbox.Center);
			}
		}
	}

	internal void StartGoToPoint(float time, EFamiliarAIStateType endAI, EFamiliarGoToType goToType)
	{
		EFamiliarAIStateType familiarAIState = FamiliarAIState;
		FamiliarAIState = EFamiliarAIStateType.GoTo;
		_goToTotalTime = time;
		_goToTimer = 0f;
		_goToEndAction = endAI;
		_goToType = goToType;
		_goToStart = Position;
		_isGoToUsingCosine = false;
		if (endAI == EFamiliarAIStateType.Attacking || familiarAIState == EFamiliarAIStateType.Attacking)
		{
			ManageState(EAFSM.Moving);
		}
	}

	protected virtual void DoSittingAnimation()
	{
	}

	internal void OnEndSit()
	{
		PlayCue(ESFX.FamiliarAfkFinish);
	}

	internal void CutsceneSitStill()
	{
		FamiliarAIState = EFamiliarAIStateType.CutsceneStatic;
	}

	internal void CutsceneFlyTo(Point targetPoint, float duration, int facingArgument, bool isCosineFlight)
	{
		FamiliarAIState = EFamiliarAIStateType.GoTo;
		_goToType = EFamiliarGoToType.TargetPoint;
		_goToStart = Position;
		_goToTarget = targetPoint;
		_goToTimer = 0f;
		_goToTotalTime = duration;
		_goToEndAction = EFamiliarAIStateType.CutsceneHover;
		_cutsceneFlyAroundTimer = 0f;
		_isGoToUsingCosine = isCosineFlight;
		if (facingArgument == 0)
		{
			IsFacingLeft = targetPoint.X < Position.X;
		}
		else
		{
			IsFacingLeft = facingArgument < 0;
		}
	}

	internal void CutsceneFlyAround(Point targetPoint, float duration, float radius)
	{
		FamiliarAIState = EFamiliarAIStateType.GoTo;
		_goToType = EFamiliarGoToType.TargetPoint;
		_goToStart = Position;
		_goToTarget = new Point(targetPoint.X + (int)radius, targetPoint.Y);
		_goToTimer = 0f;
		_goToTotalTime = duration;
		_goToEndAction = EFamiliarAIStateType.CutsceneFlyAround;
		_isGoToUsingCosine = false;
		_cutsceneFlyAroundRadius = radius;
		_cutsceneFlyAroundTimer = 0f;
		_cutsceneFlyAroundCenter = targetPoint;
		IsFacingLeft = targetPoint.X < Position.X;
	}

	internal void CutsceneResumeAI()
	{
		FamiliarAIState = EFamiliarAIStateType.Following;
		SetState(EAFSM.Idle);
		_timeSinceParentMoved = 0f;
		_leashCatchUpMultiplier = 0f;
		_leashOffset = Point.Zero;
		_leashPosition = Position;
		_parentOffsetEaseTimer = 0f;
		_rollingAverageChangeInX = 0f;
		SnapBboxToPosition();
	}

	public override void StartAbility(int whichAbility)
	{
		_selectedAbility = whichAbility;
		_abilityTimer = 0f;
		_totalAbilityTime = 1f;
		_isCarryingOutAbility = true;
		_hasUsedAbility = false;
		if (whichAbility == 0)
		{
			PlayCue(ESFX.FamiliarSwipe);
		}
		CarryOutAbility(0f);
		base.StartAbility(whichAbility);
	}

	internal virtual void Unequip()
	{
		SilentKill();
	}

	public override void ChangeRoom()
	{
		_isIgnoringPlatform = true;
		_lastParentBasePosition = _parentObject.Position;
		_leashPosition = _lastParentBasePosition.Add(_leashOffset).Add(ParentOffset);
		Position = (base.IsPlayable ? new Point(_lastParentBasePosition.X, _lastParentBasePosition.Y + -8) : _leashPosition.Add(_orbitOffset));
		SnapBboxToPosition();
		FamiliarAIState = EFamiliarAIStateType.Following;
		SetState(EAFSM.Idle);
		_nextActionTimer = 2f;
		if (_isCarryingOutAbility)
		{
			EndAbility();
		}
		base.ChangeRoom();
	}

	internal void RefreshStats()
	{
		_familiarItem = _level.GameSave.GetFamiliarItem(_familiarType);
		base.MaxHP = _familiarItem.MaxHealth;
		Damage = _familiarItem.Damage;
	}

	public bool GiveExperience(int enemyID)
	{
		bool result = false;
		if (HitEnemyRegistry.Contains(enemyID))
		{
			HitEnemyRegistry.Remove(enemyID);
			if (_level.GameSave.GiveFamiliarExperience(FamiliarType))
			{
				RefreshStats();
				result = true;
			}
		}
		return result;
	}

	internal bool TargetNearestEnemy()
	{
		SpellTargetMonster = _level.GetNearestVisibleAggroedEnemy(Bbox.Center);
		return SpellTargetMonster != null;
	}

	private void RememberHitEnemy(int enemyID)
	{
		if (!_hitEnemyRegistry.Contains(enemyID))
		{
			_hitEnemyRegistry.Add(enemyID);
		}
	}

	internal virtual void OnSuccessfulEnemyHit(Alive enemy, bool isMelee)
	{
		if (enemy != null)
		{
			RememberHitEnemy(enemy.ID);
		}
		if (isMelee && base.IsPlayable)
		{
			if (_playerMeleeHitCount == 4)
			{
				EmitSpellReadyEffect();
			}
			_playerMeleeHitCount++;
		}
	}

	private void EmitSpellReadyEffect()
	{
		Vector2 where = Bbox.Center.ToVector2();
		for (int i = 0; i < 16; i++)
		{
			_spellReadyParticles.AddParticles(where);
		}
		_isShowingHaloAnimation = true;
		_haloRingAnimation.Reset();
		_haloRingAnimation.Center = Bbox.Center;
	}

	internal void MatchPreviousFamiliarPosition(FamiliarBase previousFamiliar)
	{
		EFamiliarAIStateType familiarAIState = previousFamiliar.FamiliarAIState;
		if (familiarAIState == EFamiliarAIStateType.CutsceneHover || familiarAIState == EFamiliarAIStateType.CutsceneFlyAround || familiarAIState == EFamiliarAIStateType.CutsceneStatic)
		{
			previousFamiliar.CutsceneResumeAI();
		}
		_parentOffsetEaseTimer = 0f;
		_rollingAverageChangeInX = 0f;
		IsFacingLeft = previousFamiliar.IsFacingLeft;
		Position = previousFamiliar.Position;
		_timeSinceParentMoved = 0f;
		_leashCatchUpMultiplier = 0f;
		_leashPosition = previousFamiliar.LeashPosition;
		_leashOffset = previousFamiliar.LeashOffset;
		_orbitTimer = previousFamiliar.OrbitTimer;
		_orbitOffset = previousFamiliar.OrbitOffset;
		_orbitRadius = previousFamiliar.OrbitRadius;
		SnapBboxToPosition();
	}

	internal static void AddPoofAnimation(Level level, Point center)
	{
		level.PlayCue(ESFX.FamiliarPoof, center);
		level.AddAnimation(EBattleAnimationType.Poof, center, ETeamSide.Heroes, isFacingRight: true, doesPlaySFX: false);
	}

	internal void SetIsPlayable(bool isPlayable, bool isSwitching)
	{
		bool isPlayable2 = _isPlayable;
		if (isPlayable)
		{
			_playerStartStopTimer = 1f;
			_isPlayable = true;
			_isFlying = false;
			FamiliarAIState = EFamiliarAIStateType.Following;
			SetState(EAFSM.Idle);
			LastFamiliarAIState = EFamiliarAIStateType.Following;
			_timeSinceParentMoved = 0f;
			if (!_isShowingHaloAnimation && !isSwitching)
			{
				_level.PlayCue(ESFX.FamiliarPlayer2Start, Position);
				_isShowingHaloAnimation = true;
				_haloRingAnimation.Reset();
				_haloRingAnimation.Center = Bbox.Center;
			}
		}
		else
		{
			_isPlayable = false;
			base.IsGlowing = false;
			if (isPlayable2)
			{
				_level.PlayCue(ESFX.FamiliarPlayer2End, Position);
			}
			ResetLeash();
		}
	}

	private void ResetLeash()
	{
		_playerStartStopTimer = 1f;
		_leashPosition = Position;
		_movementX = 0f;
		_movementY = 0f;
		_timeSinceParentMoved = 0f;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		bool doesDrawAura = base.DoesDrawAura;
		if (!_doesDrawSpriteAndAppendages)
		{
			base.DoesDrawAura = false;
		}
		else if (_isShowingHaloAnimation)
		{
			_haloRingAnimation.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
		base.DoesDrawAura = doesDrawAura;
	}

	internal virtual void ChangeSkin(bool isAltSkin)
	{
	}

	public void Activate(PlayerIndex playerIndex)
	{
		if (!base.IsPlayable && _playerStartStopTimer <= 0f)
		{
			SetIsPlayable(isPlayable: true, isSwitching: false);
			if (_lastPlayerIndex != playerIndex)
			{
				SetPlayerIndex(playerIndex);
			}
		}
	}

	internal void SetPlayerIndex(PlayerIndex playerIndex)
	{
		_lastPlayerIndex = playerIndex;
		_gamePadWrapper.SetPlayerIndex(playerIndex);
	}
}
