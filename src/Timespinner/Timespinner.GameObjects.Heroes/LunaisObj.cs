using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.LunaisParticleEffects;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes.Familiars;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Passives;
using Timespinner.GameObjects.Heroes.Spells;
using Timespinner.GameObjects.StatusEffects;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameObjects.Heroes;

internal sealed class LunaisObj : Protagonist
{
	private const int FrozenTrailLength = 16;

	private const float FrozenTrailFadeRate = 2f;

	private const float TimeStopCastCooldownDuration = 0.33f;

	private const float TimeStopCancelCooldownDuration = 0.167f;

	private const float LunaisMaxMoveSpeed = 145f;

	private const float MaxDashSpeed = 275f;

	private const float MaxGrabJumpTimer = 0.15f;

	private const float MiracleJumpTimeThreshold = 0.04f;

	private const float FallCrouchActivateThreshold = 4f;

	private const float FallDustActivateThreshold = 0.1f;

	private const float FallCrouchDuration = 0.5f;

	private const float BackDashVelocityX = 1.5f;

	private const float BackdashVelocityStopThreshold = 0.332f;

	private const float BackdashDuration = 0.54899997f;

	private const float BackdashCooldown = 0f;

	private const float RecoilFallCrouchTime = 0.3f;

	private const float RecoilSkidInvulnerableTime = 0.5f;

	private const float KnockBackSkidVelocity = 3000f;

	private const float KnockBackUpTime = 0.15f;

	private const float KnockBackUpVelocity = 3000f;

	private const float KnockBackOverVelocity = 4500f;

	private const int TreadWaterUpForceThreshold = 23;

	private const float TreadWaterUpForce = 15f;

	internal const float LunaisGravityAcceleration = 1000f;

	private const float LunaisMaxFallSpeed = 450f;

	private const float LunaisInvulnerableDuration = 0.7f;

	private const float TimeBeforeStandingIdle = 7.5f;

	private const float TimeBeforeStandingIdleOnSlope = 1f;

	private const float TimeBeforeFancyStandingIdle = 10f;

	private const int SkyDashTrailLength = 8;

	private const float SkyDashTrailFadeRate = 2f;

	private const float SkydashVelocity = -800f;

	private const int DeathChargeOffsetX = -4;

	private const int DeathChargeOffsetY = -16;

	private const int DeathChargeBaseMinRadius = 5;

	private const int DeathChargeBaseMaxRadius = 10;

	private const int DeathChargeRadiusGrowth = 64;

	private const int DeathSandOffsetX = -4;

	private const int DeathSandOffsetY = -8;

	private const int DeathExplosionOffsetX = -5;

	private const int DeathExplosionOffsetY = -22;

	private const float TimeBeforeDeathFadingOutScreen = 0.25f;

	private const float TimeToDeathFadeOutScreen = 1.75f;

	private const float TimeToMoveWhileDying = 1.25f;

	private const float TimeBeforeDeathExploding = 2f;

	private const float TimeBeforeAddingDeathChargeParticles = 0.1f;

	private const float TimeBeforeShowingGameOverScreen = 4f;

	internal const float Anim_IdleSpeed = 0.11f;

	internal const float Anim_StandingIdleSpeed = 0.15f;

	internal const float Anim_FancySpeed = 0.15f;

	private const float Anim_FallSpeed = 0.05f;

	private const float Anim_RunningSpeed = 0.07f;

	private const float Anim_TreadSpeed = 0.2f;

	private const float Anim_DuckSpeed_Fall = 0.03f;

	private const float Anim_DuckSpeed_Normal = 0.08f;

	internal const byte Anim_IdleLength = 5;

	private const byte Anim_PreStandingIdleLength = 3;

	internal const byte Anim_StandingIdleLength = 5;

	private const byte Anim_PreRunLength = 5;

	private const byte Anim_RunLength = 10;

	private const byte Anim_LandLength = 5;

	private const byte Anim_EndRunLength = 4;

	private const byte Anim_TurnLength = 5;

	internal const byte Anim_PreSummonLength = 1;

	internal const byte Anim_SummonLength = 5;

	internal const byte Anim_DuckLength = 5;

	internal const byte Anim_EndDuckLength = 2;

	private const byte Anim_JumpLength = 2;

	private const byte Anim_PreFallLength = 2;

	private const byte Anim_FallLength = 3;

	private const byte Anim_PreHorJumpLength = 2;

	private const byte Anim_HorJumpLength = 2;

	private const byte Anim_PreHorFallLength = 2;

	private const byte Anim_LedgeLength = 3;

	private const byte Anim_LedgeJumpLength = 1;

	private const byte Anim_PreDoubleJumpLength = 0;

	private const byte Anim_DoubleJumpLength = 10;

	private const byte Anim_PreDashLength = 1;

	private const byte Anim_DashLength = 5;

	private const byte Anim_EndDashLength = 4;

	private const byte Anim_BackDashLength = 3;

	private const byte Anim_BackDashEndLength = 4;

	private const byte Anim_PreChargeLength = 1;

	private const byte Anim_ChargeLength = 5;

	private const byte Anim_EndChargeLength = 1;

	private const byte Anim_PreChannelLength = 1;

	private const byte Anim_ChannelLength = 5;

	private const byte Anim_EndChannelLength = 1;

	internal const byte Anim_Punch1Length = 5;

	private const byte Anim_Punch2Length = 5;

	private const byte Anim_DuckPunch1Length = 5;

	private const byte Anim_DuckPunch2Length = 5;

	private const byte Anim_AirPunch1Length = 5;

	private const byte Anim_AirPunch2Length = 5;

	private const byte Anim_SmashLength = 10;

	private const byte Anim_AirSmashLength = 10;

	private const byte Anim_PreLazerLength = 4;

	public const byte Anim_LazerLength = 5;

	private const byte Anim_EndLazerLength = 1;

	private const byte Anim_PreDamagedFrontLength = 1;

	private const byte Anim_DamagedFrontLength = 3;

	private const byte Anim_EndDamagedFrontLength = 1;

	private const byte Anim_PreDamagedBackLength = 1;

	private const byte Anim_DamagedBackLength = 3;

	private const byte Anim_EndDamagedBackLength = 1;

	private const byte Anim_PreDamagedFrontAirLength = 1;

	private const byte Anim_DamagedFrontAirLength = 3;

	private const byte Anim_EndDamagedFrontAirLength = 1;

	private const byte Anim_PreDamagedBackAirLength = 1;

	private const byte Anim_DamagedBackAirLength = 3;

	private const byte Anim_EndDamagedBackAirLength = 1;

	internal const byte Anim_PreAirLazerLength = 4;

	internal const byte Anim_AirLazerLength = 5;

	private const byte Anim_EndAirLazerLength = 1;

	private const byte Anim_PreDeadLength = 1;

	private const byte Anim_DeadLength = 2;

	internal const byte Anim_EndDeadLength = 3;

	internal const byte Anim_WakeLength = 2;

	private const byte Anim_GateJumpLength = 2;

	private const byte Anim_GateJumpLandLength = 3;

	internal const byte Anim_GateJumpLandStandUpLength = 2;

	internal const byte Anim_WindyLength = 2;

	internal const byte Anim_AirWindyLength = 2;

	internal const byte Anim_SurprisedLength = 3;

	internal const byte Anim_PreFancyLength = 2;

	internal const byte Anim_FancyLength = 5;

	internal const byte Anim_TreadLength = 5;

	internal const byte Anim_YesLength = 4;

	internal const byte Anim_NoLength = 3;

	internal const byte Anim_CrossLength = 3;

	internal const byte Anim_WaterLength = 1;

	internal const byte Anim_SitLength = 1;

	internal const byte Anim_FloatLength = 5;

	internal const byte Anim_PrePalmPunchLength = 2;

	internal const byte Anim_PalmPunchLength = 4;

	internal const byte Anim_EndPalmPunchLength = 1;

	internal const byte Anim_IdleStart = 0;

	private const byte Anim_PreStandingIdleStart = 5;

	internal const byte Anim_StandingIdleStart = 8;

	private const byte Anim_PreRunStart = 13;

	private const byte Anim_RunStart = 18;

	private const byte Anim_LandStart = 28;

	private const byte Anim_EndRunStart = 29;

	private const byte Anim_TurnStart = 33;

	internal const byte Anim_PreSummonStart = 38;

	internal const byte Anim_SummonStart = 39;

	internal const byte Anim_DuckStart = 44;

	internal const byte Anim_EndDuckStart = 49;

	private const byte Anim_JumpStart = 51;

	private const byte Anim_PreFallStart = 53;

	private const byte Anim_FallStart = 55;

	private const byte Anim_PreHorJumpStart = 58;

	private const byte Anim_HorJumpStart = 60;

	private const byte Anim_PreHorFallStart = 62;

	private const byte Anim_LedgeStart = 64;

	private const byte Anim_LedgeJumpStart = 67;

	private const byte Anim_PreDoubleJumpStart = 68;

	private const byte Anim_DoubleJumpStart = 68;

	private const byte Anim_PreDashStart = 78;

	private const byte Anim_DashStart = 79;

	private const byte Anim_EndDashStart = 84;

	private const byte Anim_BackDashStart = 88;

	private const byte Anim_BackDashEndStart = 91;

	private const byte Anim_PreChargeStart = 95;

	private const byte Anim_ChargeStart = 96;

	private const byte Anim_EndChargeStart = 101;

	private const byte Anim_PreChannelStart = 102;

	private const byte Anim_ChannelStart = 103;

	private const byte Anim_EndChannelStart = 108;

	internal const byte Anim_Punch1Start = 109;

	private const byte Anim_Punch2Start = 114;

	private const byte Anim_DuckPunch1Start = 119;

	private const byte Anim_DuckPunch2Start = 124;

	private const byte Anim_AirPunch1Start = 129;

	private const byte Anim_AirPunch2Start = 134;

	private const byte Anim_SmashStart = 139;

	private const byte Anim_AirSmashStart = 149;

	private const byte Anim_PreLazerStart = 159;

	internal const byte Anim_LazerStart = 163;

	private const byte Anim_EndLazerStart = 168;

	internal const byte Anim_PreAirLazerStart = 169;

	internal const byte Anim_AirLazerStart = 173;

	private const byte Anim_EndAirLazerStart = 178;

	private const byte Anim_PreDamagedFrontStart = 179;

	private const byte Anim_DamagedFrontStart = 180;

	private const byte Anim_EndDamagedFrontStart = 183;

	private const byte Anim_PreDamagedBackStart = 184;

	private const byte Anim_DamagedBackStart = 185;

	private const byte Anim_EndDamagedBackStart = 188;

	private const byte Anim_PreDamagedFrontAirStart = 189;

	private const byte Anim_DamagedFrontAirStart = 190;

	private const byte Anim_EndDamagedFrontAirStart = 193;

	private const byte Anim_PreDamagedBackAirStart = 194;

	private const byte Anim_DamagedBackAirStart = 195;

	private const byte Anim_EndDamagedBackAirStart = 198;

	private const byte Anim_PreDeadStart = 199;

	private const byte Anim_DeadStart = 200;

	internal const byte Anim_EndDeadStart = 202;

	internal const byte Anim_WakeStart = 205;

	private const byte Anim_GateJumpStart = 207;

	private const byte Anim_GateJumpLandStart = 209;

	internal const byte Anim_GateJumpLandStandUpStart = 212;

	internal const byte Anim_WindyStart = 214;

	internal const byte Anim_AirWindyStart = 216;

	internal const byte Anim_SurprisedStart = 218;

	internal const byte Anim_PreFancyStart = 221;

	private const byte Anim_FancyStart = 223;

	private const byte Anim_TreadStart = 228;

	internal const byte Anim_YesStart = 233;

	internal const byte Anim_NoStart = 237;

	internal const byte Anim_CrossStart = 240;

	internal const byte Anim_WaterStart = 243;

	internal const byte Anim_SitStart = 244;

	internal const byte Anim_FloatStart = 245;

	internal const byte Anim_PrePalmPunchStart = 250;

	internal const byte Anim_PalmPunchStart = 252;

	internal const int Anim_EndPalmPunchStart = 256;

	private const byte Anim_SkydashStart = 68;

	private const byte Anim_SkydashLength = 9;

	private static readonly Color BaseDeathSandGlowColor = new Color(0.9f, 0.5f, 0.25f, 0.15f);

	private readonly Point _duckingBoundingOffset;

	private readonly Point _duckingGunOffset;

	private readonly Point _duckingChargingGunOffset;

	private readonly Point _standingBoundingOffset;

	private readonly Point _standingGunOffset;

	private readonly Point _standingChargingGunOffset;

	private readonly SpriteSheet _primarySprite;

	private readonly SpriteSheet _secondarySprite;

	private readonly SpriteSheet _thirdSprite;

	private readonly LunaisOrbManager _orbManager;

	private readonly LunaisSpellManager _spellManager;

	private readonly LunaisPassiveManager _passiveManager;

	private readonly FamiliarManager _familiarManager;

	private readonly ChargeGustParticleSystem _chargeGustParticleSystem;

	private readonly LandingDustParticleSystem _landingDustParticleSystem;

	private readonly LunaisBackDashDustParticleSystem _backDashParticleSystem;

	private static readonly Point WaterEmissionOffset = new Point(-4, 8);

	private bool _canDash = true;

	private bool _canStopTime;

	private bool _canSwitchOrbSets;

	private bool _canFly;

	private bool _isDuckingDuringAbility;

	private bool _wasDuckingDuringAbility;

	private bool _hasDuckBeenCanceled;

	private bool _isBackdashingToTheRight;

	private bool _doesDrawOrbs = true;

	private bool _doesDrawSelf = true;

	private bool _wasDamagedFromBehind;

	private bool _canSwim;

	private bool _isBeingKnockedBack;

	private bool _shouldKnockBackGoUp;

	private bool _hasPlayedDoubleJumpCue;

	private bool _hasDeathExploded;

	private int _framesSinceAttacking = -1;

	private int _lunaisSkinIndex;

	private float _timeSinceEmittingChargeGust;

	private float _fallCrouchTimer;

	private float _backdashTimer;

	private float _timeStopCooldown;

	private float _standingIdleTimer;

	private float _fancyStandingIdleTimer;

	private float _knockBackTimer;

	private float _deathCutsceneTimer;

	private float _timeSinceTreadingWater;

	private Point _duckingBoundingSize;

	private Point _standingBoundingSize;

	private Vector2 _damagedVelocity = Vector2.Zero;

	private BattleAnimation _deathExplosionAnimation;

	private LunaisReviveAnimation _dreamReviveAnimation;

	private LunaisDeathChargeParticleSystem _deathChargeParticles;

	private LunaisDeathLazerParticleSystem _deathLazerParticles;

	private SandStreamerEvent _deathStreamers;

	internal bool IsDucking => _currentState == EAFSM.Ducking;

	internal bool CanCastSpell
	{
		get
		{
			if (!_isCarryingOutAbility && !IsGrabbing)
			{
				return _currentState != EAFSM.Dying;
			}
			return false;
		}
	}

	internal bool IsDrawingSelf => _doesDrawSelf;

	internal bool ShouldHideFamiliarToo { get; private set; }

	public override bool IsGrabbing
	{
		get
		{
			return base.IsGrabbing;
		}
		protected set
		{
			_isCarryingOutAbility = false;
			_hasUsedAbility = false;
			_abilityTimer = 0f;
			base.IsGrabbing = value;
		}
	}

	internal bool IsOnVilete { get; private set; }

	internal bool IsWearingViletianCrown { get; private set; }

	internal bool IsWearingGlassPumpkin { get; private set; }

	internal bool IsWearingSelenBangle { get; private set; }

	public override bool IsCurrentlyMagnetizing => _spellManager.IsCurrentlyMagnetizing;

	internal bool IsDoingFancyIdleAnimation { get; private set; }

	internal bool AreWeaponsSheathed { get; private set; }

	public EInventoryOrbType EquippedSpellType => _spellManager.EquippedSpellType;

	public EInventoryOrbType EquipPassiveType => _passiveManager.EquippedPassiveType;

	public override int ChargeSelect => (int)_spellManager.ChargeSelect;

	public override int Aura
	{
		get
		{
			return (int)_spellManager.Aura;
		}
		set
		{
			_spellManager.Aura = value;
		}
	}

	public override int MaxAura => (int)_spellManager.MaxAura;

	internal int PrimaryOrbSelected => (int)_orbManager.PrimaryOrbSelected;

	internal float AuraRegenRate { get; set; }

	internal float AuraCostMultiplier
	{
		get
		{
			if (_spellManager == null)
			{
				return 1f;
			}
			return _spellManager.AuraCostMultiplier;
		}
		set
		{
			_spellManager.AuraCostMultiplier = value;
		}
	}

	internal float ChargeSelectPercentage => _spellManager.ChargeSelectPercentage;

	internal float BarrierPassiveTimer { get; set; }

	public override Point CurrentMagnetCenter => Bbox.Center;

	internal Point PrimaryOrbLocation => _orbManager.PrimaryOrbLocation;

	internal LunaisSpell EquippedSpell => _spellManager.EquippedSpell;

	internal LunaisOrb MainOrb => _orbManager.MainOrb;

	internal LunaisOrb SubOrb => _orbManager.SubOrb;

	internal FamiliarBase EquippedFamiliar => _familiarManager.EquippedFamiliar;

	internal SpriteSheet Sprite => _sprite;

	internal FamiliarManager FamiliarManager => _familiarManager;

	public override List<int> ChargeIntervals => _spellManager.ChargeIntervals;

	public LunaisObj(Point inPosition, Level inLevel, SpriteSheet inSprite, SpriteSheet inSecondSprite, SpriteSheet inThirdSprite, int player1, int inID)
		: base(inPosition, inLevel, inSprite, player1, inID, isPrimaryPlayer: true)
	{
		_isGrounded = true;
		_bboxOffset = new Point(13, 13);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 31);
		_gunOffset = new Point(24, 16);
		_standingGunOffset = _gunOffset;
		_standingBoundingSize = new Point(_bbox.Width, _bbox.Height);
		_standingBoundingOffset = _bboxOffset;
		_duckingBoundingSize = new Point(_bbox.Width, _bbox.Height - 12);
		_duckingGunOffset = new Point(_gunOffset.X, _gunOffset.Y - 8);
		_duckingBoundingOffset = new Point(_bboxOffset.X, _bboxOffset.Y + 12);
		_standingChargingGunOffset = new Point(_standingGunOffset.X + 1, _standingGunOffset.Y);
		_duckingChargingGunOffset = new Point(_duckingGunOffset.X - 3, _duckingGunOffset.Y + 2);
		IsFacingLeft = false;
		_gravityAcceleration = 1000f;
		_maxFallSpeed = 450f;
		_maxMoveSpeed = 145f;
		_doesTrackTimeSinceGrounded = true;
		_doesSlowlyOverrideFallSpeed = true;
		_defaultInvulnerableTime = 0.7f;
		_chargeGustParticleSystem = new ChargeGustParticleSystem(_level.GCM.SpEffectsSmall, 3);
		_landingDustParticleSystem = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 5, _level.ID, 5);
		_backDashParticleSystem = new LunaisBackDashDustParticleSystem(_level.GCM.TxParticleDust, 15);
		_particleSystems.Add(_landingDustParticleSystem);
		_particleSystems.Add(_chargeGustParticleSystem);
		_particleSystems.Add(_backDashParticleSystem);
		base.MaxHP = 128;
		base.MaxMP = 50;
		_mpRegen = 0f;
		AuraRegenRate = 1.25f;
		_primarySprite = inSprite;
		_secondarySprite = inSecondSprite;
		_thirdSprite = inThirdSprite;
		base.AuraFrequency = 9f;
		_auraCount = 5f;
		_spellManager = new LunaisSpellManager(this);
		_orbManager = new LunaisOrbManager(this);
		_orbManager.ShiftMainOrbPosition(GetBulletOffset(26f));
		_passiveManager = new LunaisPassiveManager(this);
		_familiarManager = new FamiliarManager(this);
		PopulateStats(inLevel.GameSave);
		if (_level.ID == 1 && _level.RoomID == 0)
		{
			Aura = MaxAura;
		}
		base.UnderwaterBubbleEmissionPointOffset = WaterEmissionOffset;
	}

	public override void Update(float delta)
	{
		if (_isFallCrouching)
		{
			_fallCrouchTimer -= delta;
			if (_fallCrouchTimer <= 0f)
			{
				_fallCrouchTimer = 0f;
				_isFallCrouching = false;
			}
		}
		UpdateBackdash(delta);
		if (_level.IsTimeFrozen)
		{
			if (!_doesDrawTrail || _isSkydashing)
			{
				_doesDrawTrail = true;
				_trailFadeRate = 2f;
				_trailLength = 16;
				if (!_isSkydashing)
				{
					ClearTrailHistory();
				}
			}
		}
		else if (_isSkydashing)
		{
			if (!_doesDrawTrail)
			{
				_doesDrawTrail = true;
				ClearTrailHistory();
			}
			else if (_trailLength > 8)
			{
				TruncateTrail(8);
			}
			_trailFadeRate = 2f;
			_trailLength = 8;
		}
		else
		{
			_doesDrawTrail = false;
		}
		_orbManager.Update(delta, _spellManager.ChargeSelect);
		_spellManager.Update(delta);
		_passiveManager.Update(delta);
		if (EquippedSpellType != 0 && EquippedSpell != null && _isGrounded && base.IsCharging && !IsGrabbing && ChargeSelect >= ChargeIntervals[0] && _timeSinceEmittingChargeGust >= 0.2f)
		{
			_chargeGustParticleSystem.AddParticles(new Vector2(Position.X, Position.Y));
			_timeSinceEmittingChargeGust = 0f;
		}
		else if (_timeSinceEmittingChargeGust < 10f)
		{
			_timeSinceEmittingChargeGust += delta;
		}
		if (IsGrabbing)
		{
			_grabNextTimer = 0f;
			if (_grabJumpTimer < 0.15f)
			{
				_grabJumpTimer += delta;
			}
		}
		else
		{
			_grabJumpTimer = 0f;
			if (_grabNextTimer < 0.2f)
			{
				_grabNextTimer += delta;
			}
		}
		if (_timeStopCooldown > 0f)
		{
			_timeStopCooldown -= delta;
		}
		if (_doesHaveTimeStopped)
		{
			float num = 10f;
			if (IsWearingGlassPumpkin)
			{
				num *= 0.5f;
			}
			_mp -= num * delta;
			if (_mp <= 0f)
			{
				_doesHaveTimeStopped = false;
				base.MP = 0;
				StartAbility(8);
				_timeStopCooldown = 0.33f;
			}
		}
		IsOnVilete = _level.IsOnVilete;
		UpdateRecoil(delta);
		UpdateStandingIdle(delta);
		UpdateTreadingWater(delta);
		if (_currentState == EAFSM.Dying)
		{
			UpdateDeathCutscene(delta);
		}
		base.Update(delta);

		_wasJumping = _isJumping;
		_wasDoubleJumping = _isDoubleJumping;
		_wasGrabbing = IsGrabbing;
	}

	private void UpdateBackdash(float delta)
	{
		if (_currentState == EAFSM.Backdashing)
		{
			_backdashTimer -= delta;
			if (_backdashTimer <= 0f || !_isGrounded)
			{
				_movementX = 0f;
				ManageState(EAFSM.Idle);
				return;
			}
			float movementX = (_isBackdashingToTheRight ? 1.5f : (-1.5f));
			if (_backdashTimer <= 0.332f)
			{
				_movementX = 0f;
				return;
			}
			_movementX = movementX;
			_backDashParticleSystem.AddParticles(Position.ToVector2());
		}
		else if (_backdashTimer > 0f)
		{
			_backdashTimer -= delta;
		}
	}

	private void UpdateRecoil(float delta)
	{
		if (_currentState == EAFSM.Damaged)
		{
			if (!_isBeingKnockedBack)
			{
				return;
			}
			_knockBackTimer += delta;
			if (!_isGrounded && _knockBackTimer < 0.5f)
			{
				_damagedTimer = 0.25f;
				bool flag = _damagedVelocity.X < 0f;
				float num = (float)((!flag) ? 1 : (-1)) * 4500f * delta;
				float y = 0f;
				if (_knockBackTimer < 0.15f)
				{
					float num2 = 1f - _knockBackTimer / 0.15f;
					if (_shouldKnockBackGoUp)
					{
						y = (0f - num2) * 3000f * delta;
					}
					num += num2 * (float)((!flag) ? 1 : (-1)) * 4500f * delta;
				}
				_velocity = Vector2.Add(_velocity, new Vector2(num, y));
			}
			else if (!_wasGrounded)
			{
				if (_isGrounded)
				{
					SetState(EAFSM.Ducking);
					_isFallCrouching = true;
					_fallCrouchTimer = 0.3f;
				}
				else
				{
					SetState(EAFSM.Idle);
				}
				_damagedTimer = 0f;
				_isInvulnerable = true;
				_invulnerableTimer = 0.5f;
			}
		}
		else if (_isBeingKnockedBack)
		{
			if (_currentState == EAFSM.Ducking && _isFallCrouching && _fallCrouchTimer > 0f)
			{
				bool flag2 = _damagedVelocity.X < 0f;
				float num3 = _fallCrouchTimer / 0.3f;
				float x = (float)((!flag2) ? 1 : (-1)) * 3000f * num3 * delta;
				_velocity = Vector2.Add(_velocity, new Vector2(x, 0f));
				_backDashParticleSystem.AddParticles(Position.ToVector2());
			}
			else
			{
				_isBeingKnockedBack = false;
			}
		}
	}

	private void UpdateStandingIdle(float delta)
	{
		if (_currentState == EAFSM.Idle && !base.IsCharging && !_areActiveScriptsGoing && !_level.IsDialoguePlaying && _animationStart != 8 && _animationStart != 223)
		{
			if (_animationStart == 0)
			{
				_standingIdleTimer += delta;
			}
			if ((base.IsOnSlope && _standingIdleTimer >= 1f) || (!base.IsOnSlope && _standingIdleTimer >= 7.5f))
			{
				ChangeAnimation(8, 5, 0.15f, EAnimationType.Cycle, 5, 3, 0.11f);
				_standingIdleTimer = 0f;
			}
			return;
		}
		_standingIdleTimer = 0f;
		if (_currentState == EAFSM.Idle && _animationStart == 8 && _orbManager.IsMeleeOrbEquipped && !_areActiveScriptsGoing && !_level.IsDialoguePlaying && !_level.IsPlayerInputBlocked)
		{
			_fancyStandingIdleTimer += delta;
			if (_fancyStandingIdleTimer >= 10f)
			{
				ChangeAnimation(223, 5, 0.15f, EAnimationType.Cycle, 221, 2, 0.15f);
				_fancyStandingIdleTimer = 0f;
				IsDoingFancyIdleAnimation = true;
			}
		}
		else if (_animationStart != 223)
		{
			IsDoingFancyIdleAnimation = false;
		}
		else if (!_orbManager.IsMeleeOrbEquipped && _animationStart == 223)
		{
			ChangeAnimation(new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 221,
					Length = 2,
					Speed = 0.15f,
					Type = EAnimationType.Once,
					IsInReverse = true
				},
				new AnimationSpec
				{
					Start = 8,
					Length = 5,
					Speed = 0.15f,
					Type = EAnimationType.Cycle
				}
			});
		}
	}

	private void UpdateTreadingWater(float delta)
	{
		if (IsInWater && !_canSwim && base.CurrentState != EAFSM.Dying)
		{
			_isTreadingWater = true;
			if (!base.WasInWater && _timeSinceTreadingWater > 0.1f)
			{
				ManageState(EAFSM.Idle);
			}
			if (base.CurrentState == EAFSM.Jumping && !base.IsJumping)
			{
				SetState(EAFSM.Idle);
			}
			int num = Position.Y - _level.GetWaterTopFromBelowWater(Position);
			if (num > 23)
			{
				_velocity.Y -= (float)num * 15f * delta;
				_isOnSlope = false;
			}
			else
			{
				_velocity.Y -= _waterGravityAcceleration * delta;
			}
			_timeSinceTreadingWater = 0f;
		}
		else
		{
			_isTreadingWater = false;
		}
		if (!_isTreadingWater && _timeSinceTreadingWater < 1f)
		{
			_timeSinceTreadingWater += delta;
		}
	}

	internal override void SetWaterState(EAFSM state)
	{
		if (!_canSwim && _animationStart != 228)
		{
			ChangeAnimation(228, 5, 0.2f, EAnimationType.Cycle);
			base.SetWaterState(state);
		}
	}

	protected override void ProcessKeys(float delta)
	{
		base.ProcessKeys(delta);
		bool flag = _currentState == EAFSM.Damaged && _isBeingKnockedBack;
		if (_isFallCrouching)
		{
			_movementX = 0f;
		}
		if (_gamePadWrapper.IsLeftDown)
		{
			if (_currentState != EAFSM.Grabbing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Backdashing && !_isSkydashing && _currentState != EAFSM.Dashing && !_isDashing && _currentState != EAFSM.Dying && (!_isFallCrouching || !_isGrounded) && (!_isCarryingOutAbility || !_isGrounded) && (!_isCarryingOutAbility || _selectedAbility != 6))
			{
				DoHorizontalRun(-1f);
			}
		}
		else if (_gamePadWrapper.IsRightDown && _currentState != EAFSM.Grabbing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Backdashing && !_isSkydashing && _currentState != EAFSM.Dashing && !_isDashing && _currentState != EAFSM.Dying && (!_isFallCrouching || !_isGrounded) && (!_isCarryingOutAbility || !_isGrounded) && (!_isCarryingOutAbility || _selectedAbility != 6))
		{
			DoHorizontalRun(1f);
		}
		if (((_gamePadWrapper.IsDownDown && !_gamePadWrapper.IsRightDown && !_gamePadWrapper.IsLeftDown && !_isDashing) || _isFallCrouching) && !_isIgnoringPlatform)
		{
			if (_currentState != EAFSM.Damaged && _currentState != EAFSM.Dying && _isGrounded && (!_hasDuckBeenCanceled || _currentState != EAFSM.Backdashing) && !base.IsCrouchingDisabled && !base.WasCrouchingDisabled)
			{
				ManageState(EAFSM.Ducking);
				_acceleration.X = 0f;
			}
		}
		else
		{
			_hasDuckBeenCanceled = false;
		}
		if (_gamePadWrapper.IsJumpDown)
		{
			if ((!_isCarryingOutAbility || !_isGrounded) && !_isSkydashing)
			{
				DoVerticalJump();
			}
		}
		else
		{
			if (_isGrounded)
			{
				_isJumpReset = true;
				_isDoubleJumping = false;
			}
			_isJumping = false;
			_isIgnoringPlatform = false;
			_hasPlayedDoubleJumpCue = false;
			if (_wasJumping && _velocity.Y < 0f && _currentJumpTime > 0f && _currentJumpTime < _maxJumpTime / 2f)
			{
				_velocity.Y /= 8f;
			}
		}
		if (_orbManager.IsMeleeOrbEquipped && _currentState != EAFSM.Dying && !flag && _gamePadWrapper.IsMeleeDown && !_gamePadWrapper.WasMeleeDown && !base.IsCurrentlyGrabbing)
		{
			if (!_isCarryingOutAbility || _hasUsedAbility)
			{
				if (!_orbManager.AreAllOrbsBusy)
				{
					StartAbility(9);
				}
				else
				{
					_passiveManager.OnAttackWhenAllOrbsAreBusy();
				}
			}
			_spellManager.CancelCharge();
		}
		if (_currentState != EAFSM.Dying && !_gamePadWrapper.IsMeleeDown)
		{
			_spellManager.ProcessInput(_gamePadWrapper.IsSpellDown, _gamePadWrapper.WasSpellDown, flag);
			base.IsOOM = _spellManager.IsOOM;
			base.OOMSpellAmount = _spellManager.GetFirstChargeIntervalAmount();
		}
		else if (_currentState == EAFSM.Dying)
		{
			_spellManager.CancelCharge();
		}
		if (_gamePadWrapper.IsTimeDown && _currentState != EAFSM.Dying)
		{
			ProcessTimeStopUse();
		}
		if (!_isCarryingOutAbility)
		{
			if (_canDash)
			{
				if (_gamePadWrapper.IsDashDown && !_gamePadWrapper.IsBackdashDown)
				{
					if (!_isDashing && _dashCooldownTimer <= 0f)
					{
						ManageState(EAFSM.Dashing);
						if (_currentState == EAFSM.Dashing)
						{
							_isDashing = true;
						}
					}
					if (_isDashing)
					{
						_movementX = (IsFacingLeft ? (-2f) : 2f);
						_dashCooldownTimer = 0.3f;
					}
				}
				else
				{
					if (_currentState == EAFSM.Dashing)
					{
						ManageState(EAFSM.Idle);
						_movementX = 0f;
					}
					_isDashing = false;
				}
			}
			else if (_isDashing)
			{
				if (_currentState == EAFSM.Dashing)
				{
					ManageState(EAFSM.Idle);
					_movementX = 0f;
				}
				_isDashing = false;
			}
			if (!_isDashing && _currentState != EAFSM.Backdashing)
			{
				EndDash();
			}
			else
			{
				_maxMoveSpeed = 275f;
				_shouldIgnoreIntermediateSlopes = true;
			}
		}
		if (_canFly)
		{
			if (!_isSkydashing && !_isCarryingOutAbility && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying && !_isHittingHeadOnCeiling && _gamePadWrapper.IsBackdashDown && !_gamePadWrapper.WasBackdashDown && _gamePadWrapper.IsUpDown && _skydashCooldownTimer <= 0f)
			{
				ManageState(EAFSM.Skydashing);
				if (_currentState == EAFSM.Skydashing)
				{
					PlayCue(ESFX.LunaisSuperJump);
					_isSkydashing = true;
					_isJumpReset = true;
					_isDoubleJumping = false;
					_isJumping = false;
				}
			}
			if (_isSkydashing)
			{
				if (_isHittingHeadOnCeiling || !_gamePadWrapper.IsBackdashDown || _currentState == EAFSM.Damaged || _currentState == EAFSM.Dying)
				{
					_isSkydashing = false;
					_overrideFallSpeedAmount = 0f;
					_velocity.Y *= 0.5f;
					if (_isHittingHeadOnCeiling)
					{
						_level.RequestScreenShake(new Vector2(0f, 2f), 0.25f, 10f, isAffectedByTime: false);
						_level.PlayCue(ESFX.LunaisSuperJumpImpact, Position);
					}
				}
				else
				{
					_velocity.Y = -800f;
					_skydashCooldownTimer = 0.3f;
					_movementX = 0f;
				}
			}
			_isAffectedByWater = !_isSkydashing;
		}
		else if (_isSkydashing)
		{
			_isSkydashing = false;
			_overrideFallSpeedAmount = 0f;
			_isAffectedByWater = true;
		}
		if (_canSwitchOrbSets && (!_isCarryingOutAbility || _hasUsedAbility))
		{
			if (_gamePadWrapper.IsLTriggerDown && !_gamePadWrapper.WasLTriggerDown)
			{
				SwitchOrbSet(isForward: false);
			}
			else if (_gamePadWrapper.IsRTriggerDown && !_gamePadWrapper.WasRTriggerDown)
			{
				SwitchOrbSet(isForward: true);
			}
		}
		if (_gamePadWrapper.IsBackdashDown && !_gamePadWrapper.WasBackdashDown && _backdashTimer <= 0f && _isGrounded && _currentState != EAFSM.Backdashing && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying && (!_isCarryingOutAbility || _canCurrentAbilityCanBeCanceled))
		{
			_backdashTimer = 0.54899997f;
			_hasDuckBeenCanceled = _currentState == EAFSM.Ducking;
			_isFallCrouching = false;
			_isBeingKnockedBack = false;
			ManageState(EAFSM.Backdashing);
		}
		if ((!_gamePadWrapper.IsLeftDown || _isDashing) && (!_gamePadWrapper.IsRightDown || _isDashing) && (_currentState != EAFSM.Backdashing || !_isGrounded) && ((!_gamePadWrapper.IsDownDown && !_isFallCrouching) || _isDashing || _currentState == EAFSM.Skydashing) && (!_isDashing || _currentState != EAFSM.Dashing) && (!_isSkydashing || _currentState != EAFSM.Skydashing) && (!_gamePadWrapper.IsJumpDown || _isGrounded || _isTreadingWater) && (_currentState != EAFSM.Special || !_isCarryingOutAbility) && _currentState != EAFSM.Damaged && _currentState != EAFSM.Dying)
		{
			ManageState(EAFSM.Idle);
		}
		if ((!_gamePadWrapper.IsLeftDown && !_gamePadWrapper.IsRightDown && !_isDashing && _currentState != EAFSM.Backdashing) || _currentState == EAFSM.Damaged || _currentState == EAFSM.Dying || (_isCarryingOutAbility && _isGrounded))
		{
			_movementX = 0f;
		}
	}

	private void ProcessTimeStopUse()
	{
		if (_canStopTime && !_gamePadWrapper.WasTimeDown && _currentState != EAFSM.Dying && !_isFrozen && _timeStopCooldown <= 0f)
		{
			if (_doesHaveTimeStopped)
			{
				_timeStopCooldown = 0.33f;
				_doesHaveTimeStopped = false;
				StartAbility(8);
			}
			else if (base.MP > 0)
			{
				_timeStopCooldown = 0.167f;
				_doesHaveTimeStopped = true;
				StartAbility(7);
			}
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (lastFacingRight != IsFacingLeft && _currentState == EAFSM.Running)
		{
			if (lastState == EAFSM.Running || _velocity.X > 60f || _velocity.X < -60f)
			{
				ChangeAnimation(18, 10, 0.07f, 9, EAnimationType.Cycle, 33, 5, 0.067f);
			}
			else
			{
				ChangeAnimation(18, 10, 0.07f, 9, EAnimationType.Cycle, 35, 3, 0.067f);
			}
		}
	}

	private void SwitchOrbSet(bool isForward)
	{
		PlayerInventory inventory = _level.GameSave.Inventory;
		int num = inventory.EquippedOrbSetIndex + (isForward ? 1 : (-1));
		if (num < 0)
		{
			num = 3 + num;
		}
		else if (num >= 3)
		{
			num = 0;
		}
		inventory.EquippedOrbSetIndex = num;
		GameSave gameSave = _level.GameSave;
		gameSave.CharacterStats.Aura = Aura;
		inventory.RefreshEquippedOrbs();
		RefreshOrbManagers(gameSave);
		_level.JukeBox.PlayCue(ESFX.LunaisOrbSwitch);
	}

	public void CastSpell(EInventoryOrbType spellType, int spellVariation)
	{
		switch (spellType)
		{
		case EInventoryOrbType.Blue:
			switch (spellVariation)
			{
			case 0:
				StartAbility(2);
				break;
			case 1:
				StartAbility(5);
				break;
			}
			break;
		case EInventoryOrbType.Blade:
			StartAbility(10);
			break;
		case EInventoryOrbType.Flame:
		case EInventoryOrbType.Pink:
		case EInventoryOrbType.Gun:
		case EInventoryOrbType.Empire:
			StartAbility(6);
			break;
		case EInventoryOrbType.Iron:
			StartAbility(11);
			break;
		default:
			StartAbility(5);
			break;
		}
		base.HasCastASpell = true;
	}

	public Point GetBulletOffset(float startRadius)
	{
		bool isCharging = _spellManager.IsCharging;
		bool flag = _isFallCrouching || base.CurrentState == EAFSM.Ducking || _isDuckingDuringAbility;
		_gunOffset = ((!flag) ? (isCharging ? _standingChargingGunOffset : _standingGunOffset) : (isCharging ? _duckingChargingGunOffset : _duckingGunOffset));
		int num = (IsFacingLeft ? 1 : (-1));
		int num2 = (_isDashing ? (-6) : 0);
		int num3 = (_isDashing ? 1 : 0);
		int num4 = ((_currentState == EAFSM.Running) ? (-1) : 0);
		return new Point((int)((float)num * ((float)(_gunOffset.X + num4 + num2) - startRadius)), -_gunOffset.Y + num3);
	}

	private void ThrowOrb(int whichOrb)
	{
		_orbManager.ThrowOrb(whichOrb, IsFacingLeft);
	}

	internal void ReduceAura(int amount)
	{
		Aura -= amount;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_doesDrawSelf)
		{
			return;
		}
		if (_deathChargeParticles != null && _deathLazerParticles != null)
		{
			_deathChargeParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			_deathLazerParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
		if (!_hasDeathExploded)
		{
			if (_doesDrawOrbs)
			{
				if (_currentState != EAFSM.Dying)
				{
					_passiveManager.Draw(spriteBatch);
				}
				_orbManager.Draw(spriteBatch, isTopOrb: false);
			}
			base.Draw(spriteBatch);
			if (_doesDrawOrbs)
			{
				_orbManager.Draw(spriteBatch, isTopOrb: true);
				if (_currentState != EAFSM.Dying)
				{
					_spellManager.Draw(spriteBatch);
				}
			}
		}
		if (_hasDeathExploded)
		{
			if (_deathStreamers != null)
			{
				_deathStreamers.Draw(spriteBatch);
			}
			if (_deathExplosionAnimation != null)
			{
				_deathExplosionAnimation.Draw(spriteBatch);
			}
		}
	}

	public void DrawOrbPassive(SpriteBatch spriteBatch, bool isMainOrb)
	{
		if (_doesDrawOrbs && _currentState != EAFSM.Dying)
		{
			_passiveManager.DrawUnderOrb(spriteBatch, isMainOrb);
		}
	}

	public override void SetState(EAFSM state, int newFrame)
	{
		int animationIndex = base.AnimationIndex;
		EAFSM currentState = _currentState;
		if (currentState != state)
		{
			_hasStartedChargingAnimation = false;
		}
		base.SetState(state, newFrame);
		switch (state)
		{
		case EAFSM.Running:
			switch (currentState)
			{
			case EAFSM.Falling:
				_animationIndex = 0;
				ChangeAnimation(18, 10, 0.07f, 2, EAnimationType.Cycle, 28, 1, 0f);
				break;
			case EAFSM.Dashing:
				ChangeAnimation(18, 10, 0.07f, 3, EAnimationType.Cycle, 84, 1, 0.07f);
				break;
			default:
				_animationIndex = 0;
				ChangeAnimation(18, 10, 0.07f, EAnimationType.Cycle, 13, 5, 0.07f);
				break;
			}
			break;
		case EAFSM.Idle:
		{
			bool flag = false;
			AnimationSpec animationSpec = null;
			if (currentState == EAFSM.Running)
			{
				AnimationSpec animationSpec2 = new AnimationSpec();
				animationSpec2.Start = 29;
				animationSpec2.Length = 4;
				animationSpec2.Speed = 0.1f;
				animationSpec = animationSpec2;
			}
			else if (currentState == EAFSM.Falling)
			{
				AnimationSpec animationSpec3 = new AnimationSpec();
				animationSpec3.Start = 28;
				animationSpec3.Length = 5;
				animationSpec3.Speed = 0.1f;
				animationSpec = animationSpec3;
			}
			else if (currentState == EAFSM.Ducking)
			{
				PlayCue(ESFX.LunaisStandFromCrouch);
				AnimationSpec animationSpec4 = new AnimationSpec();
				animationSpec4.Start = 49;
				animationSpec4.Length = 2;
				animationSpec4.Speed = 0.06f;
				animationSpec = animationSpec4;
			}
			else if (currentState == EAFSM.Dashing)
			{
				AnimationSpec animationSpec5 = new AnimationSpec();
				animationSpec5.Start = 84;
				animationSpec5.Length = 4;
				animationSpec5.Speed = 0.06f;
				animationSpec = animationSpec5;
			}
			else if (currentState == EAFSM.Damaged)
			{
				if (_wasDamagedFromBehind)
				{
					AnimationSpec animationSpec6 = new AnimationSpec();
					animationSpec6.Start = 188;
					animationSpec6.Length = 1;
					animationSpec6.Speed = 0.06f;
					animationSpec = animationSpec6;
				}
				else
				{
					AnimationSpec animationSpec7 = new AnimationSpec();
					animationSpec7.Start = 183;
					animationSpec7.Length = 1;
					animationSpec7.Speed = 0.06f;
					animationSpec = animationSpec7;
				}
			}
			else if (currentState == EAFSM.Special && !_isGrounded)
			{
				_currentState = EAFSM.Falling;
				if (_selectedAbility == 6)
				{
					ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle, 178, 1, 0.07f);
				}
				else
				{
					ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle);
				}
			}
			else
			{
				flag = true;
			}
			if (_isTreadingWater)
			{
				ChangeAnimation(228, 5, 0.2f, EAnimationType.Cycle);
			}
			else if (flag || animationSpec != null)
			{
				List<AnimationSpec> list = new List<AnimationSpec>();
				if (animationSpec != null)
				{
					list.Add(animationSpec);
				}
				list.Add(new AnimationSpec
				{
					Start = 0,
					Length = 5,
					Speed = 0.11f,
					Type = EAnimationType.Cycle
				});
				ChangeAnimation(list);
			}
			break;
		}
		case EAFSM.Jumping:
			_animationIndex = 0;
			if (_isDoubleJumping)
			{
				ChangeAnimation(68, 10, 0.0475f, EAnimationType.Once);
			}
			else if (IsGrabbing)
			{
				ChangeAnimation(51, 2, 0.06f, EAnimationType.Cycle, 67, 1, 0.06f);
			}
			else if (_movementX != 0f)
			{
				ChangeAnimation(60, 2, 0.06f, EAnimationType.Cycle, 58, 2, 0.06f);
			}
			else
			{
				ChangeAnimation(51, 2, 0.06f, EAnimationType.Cycle);
			}
			if (_level.IsMufflingPlayerSFX)
			{
				break;
			}
			if (_isDoubleJumping)
			{
				if (currentState != EAFSM.Idle && !_hasPlayedDoubleJumpCue)
				{
					PlayCue(ESFX.LunaisDoubleJump, Position);
					_hasPlayedDoubleJumpCue = true;
				}
			}
			else if ((_isGrounded || _isMiracleJumping) && (_isGrabbing || !_wasGrabbing || _currentlyGrabbedEvent == null) && (!IsInWater || _isTreadingWater))
			{
				PlayCue(ESFX.LunaisJump, Position);
				PlayCue(ESFX.VO_Lun_Jump, Position, isLooped: false, 0.5f);
			}
			else if (IsInWater && !_isTreadingWater && !_isHittingHeadOnCeiling && !_wasHittingHeadOnCeiling)
			{
				PlayCue(ESFX.LunaisJump);
			}
			break;
		case EAFSM.Ducking:
			PlayCue(ESFX.LunaisCrouch);
			ChangeAnimation(44, 5, _isFallCrouching ? 0.03f : 0.08f, EAnimationType.Once);
			break;
		case EAFSM.Backdashing:
			_isCarryingOutAbility = false;
			_abilityTimer = 0f;
			_isBackdashingToTheRight = IsFacingLeft;
			ChangeAnimation(new AnimationSpec[3]
			{
				new AnimationSpec
				{
					Start = 88,
					Length = 3,
					Speed = 0.066f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 90,
					Length = 1,
					Speed = 0.033f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 91,
					Length = 4,
					Speed = 0.066f,
					Type = EAnimationType.Once
				}
			});
			PlayCue(ESFX.LunaisBackdash, Position);
			break;
		case EAFSM.Falling:
			if (currentState != EAFSM.Falling)
			{
				_animationIndex = 0;
			}
			if (_isTreadingWater)
			{
				ChangeAnimation(228, 5, 0.2f, EAnimationType.Cycle);
			}
			else if (currentState == EAFSM.Damaged)
			{
				if (_wasDamagedFromBehind)
				{
					ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle, 198, 1, 0.06f);
				}
				else
				{
					ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle, 193, 1, 0.06f);
				}
			}
			else if (currentState == EAFSM.Skydashing && _velocity.Y < 0f)
			{
				int num = animationIndex + 1;
				ChangeAnimation(new AnimationSpec[3]
				{
					new AnimationSpec
					{
						Start = 68 + num,
						Length = 10 - num,
						Speed = 0.0475f
					},
					new AnimationSpec
					{
						Start = 53,
						Length = 2,
						Speed = 0.07f
					},
					new AnimationSpec
					{
						Start = 55,
						Length = 3,
						Speed = 0.05f,
						Type = EAnimationType.Cycle
					}
				});
			}
			else if (currentState == EAFSM.Special)
			{
				ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle);
			}
			else if (_animationStart != 51 && !_wasDoubleJumping)
			{
				ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle, 62, 2, 0.07f);
			}
			else
			{
				ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle, 53, 2, 0.07f);
			}
			break;
		case EAFSM.Dashing:
			ChangeAnimation(79, 5, 0.06f, EAnimationType.Cycle, 78, 1, 0.06f);
			break;
		case EAFSM.Skydashing:
			ChangeAnimation(68, 9, 0.0475f, EAnimationType.Cycle);
			break;
		case EAFSM.Grabbing:
			if (currentState != EAFSM.Grabbing)
			{
				PlayCue(ESFX.LunaisLedgeGrab);
				ChangeAnimation(64, 3, 0.05f, EAnimationType.Once);
				_isDashing = false;
			}
			break;
		case EAFSM.Damaged:
			if ((_isGrounded && !IsGrabbing) || base.CurrentMovingPlatforms.Count > 0)
			{
				if (_wasDamagedFromBehind)
				{
					ChangeAnimation(185, 3, 0.045f, EAnimationType.Once, 184, 1, 0.03f);
				}
				else
				{
					ChangeAnimation(180, 3, 0.045f, EAnimationType.Once, 179, 1, 0.03f);
				}
			}
			else if (_wasDamagedFromBehind)
			{
				ChangeAnimation(195, 3, 0.045f, EAnimationType.Once, 194, 1, 0.03f);
			}
			else
			{
				ChangeAnimation(190, 3, 0.045f, EAnimationType.Once, 189, 1, 0.03f);
			}
			break;
		case EAFSM.Dying:
			ChangeAnimation(190, 3, 0.1f, EAnimationType.Once);
			break;
		}
		if ((state == EAFSM.Ducking || _isDuckingDuringAbility) && (currentState != EAFSM.Ducking || !_wasDuckingDuringAbility))
		{
			_bbox.Width = _duckingBoundingSize.X;
			_bbox.Height = _duckingBoundingSize.Y;
			_bboxOffset = _duckingBoundingOffset;
			SnapBboxToPosition();
			SnapFrameToBbox();
			_gunOffset = (base.IsCharging ? _duckingChargingGunOffset : _duckingGunOffset);
		}
		if ((currentState == EAFSM.Ducking || _wasDuckingDuringAbility) && ((state != EAFSM.Ducking && !_isDuckingDuringAbility) || _isJumping))
		{
			_bbox.Width = _standingBoundingSize.X;
			_bbox.Height = _standingBoundingSize.Y;
			_bboxOffset = _standingBoundingOffset;
			SnapBboxToPosition();
			SnapFrameToBbox();
			_gunOffset = (base.IsCharging ? _standingChargingGunOffset : _standingGunOffset);
		}
		_wasDuckingDuringAbility = _isDuckingDuringAbility;
	}

	protected override void UpdateChargingAnimation()
	{
		_isCharging = _spellManager.IsCharging;
		if (_currentState == EAFSM.Idle || _currentState == EAFSM.Ducking)
		{
			int num = 0;
			int num2 = 0;
			if (_currentState == EAFSM.Idle)
			{
				num = 95;
				num2 = 101;
			}
			else if (_currentState == EAFSM.Ducking)
			{
				num = 102;
				num2 = 108;
			}
			if (_isCharging)
			{
				_framesSinceAttacking = 0;
			}
			else if (_framesSinceAttacking != -1 && _framesSinceAttacking < 10)
			{
				_framesSinceAttacking++;
			}
			if (_isCharging && !_hasStartedChargingAnimation)
			{
				if (!_isTreadingWater)
				{
					if (base.IsAnimationQueueEmpty && (_animationStart < num || _animationStart > num2))
					{
						if (_currentState == EAFSM.Idle)
						{
							ChangeAnimation(96, 5, 0.06f, EAnimationType.Cycle, 95, 1, 0.06f);
						}
						else if (_currentState == EAFSM.Ducking && (_animationIndex >= 3 || _animationStart == 48))
						{
							ChangeAnimation(103, 5, 0.06f, EAnimationType.Cycle, 102, 1, 0.06f);
						}
					}
					else if (base.IsAnimationQueueEmpty)
					{
						_hasStartedChargingAnimation = true;
					}
					else if (_animationSpeed > 0.04f)
					{
						_animationSpeed -= 0.01f;
					}
				}
				else
				{
					_hasStartedChargingAnimation = true;
				}
			}
			else if (_framesSinceAttacking >= 10 && _animationStart >= num && _animationStart <= num2)
			{
				if (_currentState == EAFSM.Idle)
				{
					ChangeAnimation(0, 5, 0.11f, EAnimationType.Cycle, 101, 1, 0.06f);
					_hasStartedChargingAnimation = false;
					_framesSinceAttacking = -1;
				}
				else if (_currentState == EAFSM.Ducking)
				{
					ChangeAnimation(48, 0, 0.02f, EAnimationType.None, 108, 1, 0.06f);
					_hasStartedChargingAnimation = false;
					_framesSinceAttacking = -1;
				}
			}
		}
		base.UpdateChargingAnimation();
	}

	internal override void ShiftTrailHistory(Point offset)
	{
		base.ShiftTrailHistory(offset);
		_orbManager.ShiftTrailHistory(offset);
		_passiveManager.ShiftTrailHistory(offset);
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result = false;
		if (!_isInvulnerable && _currentState != EAFSM.Dying && damage > 0)
		{
			int num = damage;
			bool flag = type != EDamageType.Spike && type != EDamageType.Squished;
			if (flag)
			{
				damage -= base.Defense;
			}
			else
			{
				float num2 = (_level.IsHardMode ? 0.2f : 0.05f);
				damage = (int)Math.Floor((float)base.MaxHP * num2);
			}
			if (damage < 1)
			{
				damage = 1;
			}
			if (flag && _level.IsHardMode)
			{
				if (damage > 1)
				{
					damage = (int)Math.Ceiling((float)num * 1.5f) - base.Defense;
				}
				damage += 65;
			}
			damage = _passiveManager.ManageDamage(damage, type);
			bool flag2 = element == EDamageElement.None && doesKnockBack;
			if (damage > 0 || flag2)
			{
				_isAffectedByGravity = true;
				if (IsFacingLeft)
				{
					_wasDamagedFromBehind = velocity.X <= 0f;
				}
				else
				{
					_wasDamagedFromBehind = velocity.X > 0f;
				}
				_knockBackTimer = 0f;
				_isBeingKnockedBack = doesKnockBack || (!_isGrounded && base.CurrentMovingPlatforms.Count == 0) || _wasGrabbing;
				if (_isBeingKnockedBack)
				{
					_shouldKnockBackGoUp = _isGrounded;
					_isGrounded = false;
					_velocity = Vector2.Zero;
					_damagedVelocity = ((Math.Abs(velocity.X) < 0.1f) ? new Vector2((where.X < Position.X) ? 1 : (-1), 0f) : velocity);
				}
				else
				{
					_damagedVelocity = Vector2.Zero;
				}
				result = true;
				ManageState(EAFSM.Damaged);
				_damagedTimer = 0.3f;
				IsGrabbing = false;
				CancelAbility(overrideCanBeCanceled: true);
				_spellManager.CancelSpellOnHit();
				_orbManager.UnhideOrbsOnHit();
				_gamePadWrapper.AddVibration(8, 1f, 1f);
				_level.HasPlayerBeenDamagedInThisRoom = true;
				base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
				_isInvulnerable = true;
				_invulnerableTimer = _damagedTimer + _defaultInvulnerableTime;
				_level.PlayCue(ESFX.LunaisTakeDamage, Position);
				if (type == EDamageType.Spike || type == EDamageType.Squished)
				{
					_invulnerableTimer += _defaultInvulnerableTime - 0.2f;
				}
				if (base.HP > 0)
				{
					_isBlinking = true;
					_level.PlayCue(ESFX.VO_Lun_TakeDamage, Position);
				}
			}
			else
			{
				_isInvulnerable = true;
				_invulnerableTimer = 0.3f + _defaultInvulnerableTime;
				_isBlinking = true;
			}
		}
		return result;
	}

	public override bool ManageHeal(float amount, bool shouldShowAnimation)
	{
		bool flag = base.ManageHeal(amount, shouldShowAnimation);
		if (flag && shouldShowAnimation && IsDrawingSelf)
		{
			int num = _level.NextRandomInt(0, 9);
			Point anchorOffset = new Point(0, -8);
			_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, Bbox.Center, _level)
			{
				TeamSide = ETeamSide.Heroes,
				IsFacingLeft = (num >= 5),
				AnchorObject = this,
				AnchorOffset = anchorOffset,
				AnimationSpeed = 0.05f,
				AnimationStart = 19,
				AnimationLength = 6
			});
			anchorOffset.Y -= 12;
			_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, Bbox.Center, _level)
			{
				TeamSide = ETeamSide.Heroes,
				IsFacingLeft = (num >= 5),
				AnchorObject = this,
				AnchorOffset = anchorOffset,
				AnimationSpeed = 0.05f,
				AnimationStart = 19,
				AnimationLength = 6,
				InitialDelay = 0.25f
			});
		}
		return flag;
	}

	public override void HealAura(int amount)
	{
		_spellManager.HealAura(amount);
	}

	public override void GiveExperience(int amount, int enemyID, Point position, bool isBossHit)
	{
		RestoreManaOnHitOrKill(isBossHit);
		if (!isBossHit)
		{
			if (IsWearingSelenBangle)
			{
				amount = (int)Math.Ceiling((float)amount * 2f);
			}
			CharacterStats characterStats = _level.GameSave.CharacterStats;
			bool flag = characterStats.GiveExperience(amount);
			if (flag)
			{
				int num = characterStats.MaxAura - MaxAura;
				characterStats.Aura = Aura + num;
				int maxHP = base.MaxHP;
				RefreshCharacterStats(shouldRecalculate: false);
				base.HP += base.MaxHP - maxHP;
			}
			bool flag2 = _orbManager.GiveExperience(enemyID);
			bool flag3 = _spellManager.GiveExperience(enemyID);
			if (flag || flag2)
			{
				_spellManager.RefreshDamage();
			}
			if (flag || flag3)
			{
				_orbManager.RefreshDamage();
			}
			if (flag || flag3 || flag2)
			{
				_passiveManager.RefreshStats(_level.GameSave);
			}
			if (flag)
			{
				base.IsLevelingUp = true;
				_level.RequestToastPopup(EToastType.LevelUp, 0);
			}
			_familiarManager.GiveExperience(enemyID);
			if (amount > 0 || _level.IsInBossRoom)
			{
				_spellManager.OnEnemyDeath(position);
				_passiveManager.OnEnemyDeath();
			}
		}
	}

	private void RestoreManaOnHitOrKill(bool isBossHit)
	{
		ManageManaRestore(isBossHit ? 1 : 5);
	}

	private void PopulateStats(GameSave inSave)
	{
		CharacterStats characterStats = inSave.CharacterStats;
		base.MaxHP = characterStats.MaxHP;
		base.MaxMP = characterStats.MaxSand;
		RefreshStats(inSave);
	}

	public override void RefreshStats(GameSave inSave)
	{
		CharacterStats characterStats = inSave.CharacterStats;
		if (characterStats.HP != 0)
		{
			base.MP = characterStats.Sand;
			base.HP = characterStats.HP;
			Aura = characterStats.Aura;
			base.Defense = characterStats.Defense;
		}
		_mpRegen = 0f;
		EInventoryEquipmentType equippedHelmet = _level.GameSave.Inventory.EquippedHelmet;
		if (equippedHelmet == EInventoryEquipmentType.EternalTiara)
		{
			_mpRegen += 0.5f;
		}
		IsWearingViletianCrown = equippedHelmet == EInventoryEquipmentType.VileteCrown;
		EInventoryEquipmentType equippedTrinketA = _level.GameSave.Inventory.EquippedTrinketA;
		EInventoryEquipmentType equippedTrinketB = _level.GameSave.Inventory.EquippedTrinketB;
		IsWearingSelenBangle = equippedTrinketA == EInventoryEquipmentType.SelenBangle || equippedTrinketB == EInventoryEquipmentType.SelenBangle;
		IsWearingGlassPumpkin = equippedTrinketA == EInventoryEquipmentType.GlassPumpkin || equippedTrinketB == EInventoryEquipmentType.GlassPumpkin;
		RefreshOrbManagers(inSave);
		_familiarManager.RefreshStats(inSave);
		InventoryRelicCollection relicInventory = inSave.Inventory.RelicInventory;
		_canDash = inSave.IsDashingUnlocked;
		_canDoubleJump = inSave.IsDoubleJumpUnlocked;
		_canStopTime = inSave.IsTimeStopUnlocked;
		_canSwim = inSave.IsWaterMaskUnlocked;
		_canSwitchOrbSets = inSave.IsOrbSwitchingUnlocked;
		_canFly = relicInventory.IsRelicActive(EInventoryRelicType.EssenceOfSpace);
		if (relicInventory.IsRelicActive(EInventoryRelicType.EternalBrooch))
		{
			if (_lunaisSkinIndex != 2)
			{
				_sprite = _thirdSprite;
				_lunaisSkinIndex = 2;
			}
		}
		else if (relicInventory.IsRelicActive(EInventoryRelicType.EmpireBrooch))
		{
			if (_lunaisSkinIndex != 1)
			{
				_sprite = _secondarySprite;
				_lunaisSkinIndex = 1;
			}
		}
		else if (_lunaisSkinIndex != 0)
		{
			_sprite = _primarySprite;
			_lunaisSkinIndex = 0;
		}
		base.RefreshStats(inSave);
	}

	private void RefreshCharacterStats(bool shouldRecalculate)
	{
		CharacterStats characterStats = _level.GameSave.CharacterStats;
		if (shouldRecalculate)
		{
			characterStats.RefreshBaseStats();
		}
		base.MaxHP = characterStats.MaxHP;
		base.MaxMP = characterStats.MaxSand;
		_spellManager.RefreshStats(_level.GameSave);
		base.Defense = characterStats.Defense;
	}

	private void RefreshOrbManagers(GameSave inSave)
	{
		_orbManager.RefreshStats(inSave);
		_spellManager.RefreshStats(inSave);
		_passiveManager.RefreshStats(inSave);
	}

	public override void GetPowerup(EItemType item, float amount)
	{
		CharacterStats characterStats = _level.GameSave.CharacterStats;
		characterStats.HP = base.HP;
		characterStats.Aura = Aura;
		characterStats.Sand = base.MP;
		switch (item)
		{
		case EItemType.MaxHP:
			characterStats.MaxHPFound++;
			RefreshCharacterStats(shouldRecalculate: true);
			base.HP = base.MaxHP;
			break;
		case EItemType.MaxSand:
			characterStats.MaxSandFound++;
			RefreshCharacterStats(shouldRecalculate: true);
			base.MP = base.MaxMP;
			break;
		case EItemType.MaxAura:
			characterStats.MaxAuraFound++;
			RefreshCharacterStats(shouldRecalculate: true);
			_spellManager.Aura = MaxAura;
			break;
		default:
			base.GetPowerup(item, amount);
			break;
		}
	}

	public override bool DetectFallDeath(int levelHeight)
	{
		bool result = false;
		if (_isAffectedByLevelBounds && _bbox.Top > levelHeight + 100)
		{
			Kill(isFullKill: false);
			result = true;
		}
		return result;
	}

	public override void Kill()
	{
		Kill(isFullKill: true);
	}

	public void Kill(bool isFullKill)
	{
		_isTreadingWater = false;
		if (_currentState == EAFSM.Dying)
		{
			return;
		}
		CancelSpells();
		if (isFullKill)
		{
			if (_level.IsEasyMode)
			{
				base.HP = base.MaxHP;
				bool flag = false;
				if (_dreamReviveAnimation == null)
				{
					flag = true;
					_dreamReviveAnimation = new LunaisReviveAnimation(_level.GCM.SpOrbMeleeBarrier, Bbox.Center, _level);
				}
				else if (_dreamReviveAnimation.IsFinished)
				{
					flag = true;
					_dreamReviveAnimation.Reset(Bbox.Center, isFacingLeft: true);
				}
				if (flag)
				{
					AddBattleAnimation(_dreamReviveAnimation);
					PlayCue(ESFX.FamiliarHealSpell, Position);
					_level.AddNumber(base.MaxHP, Bbox.Center, ENumberColor.Green);
				}
			}
			else
			{
				CancelSpells();
				_level.StartPlayerDeathCutscene();
				CancelAbility(overrideCanBeCanceled: true);
				_velocity = Vector2.Zero;
				base.StatusEffects.Clear();
				_gamePadWrapper.AddVibration(30, 1f, 1f);
				_deathChargeParticles = new LunaisDeathChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
				_deathLazerParticles = new LunaisDeathLazerParticleSystem(_level.GCM.TxParticleEnergy, 2);
				_orbManager.Kill();
				ManageState(EAFSM.Dying);
				PlayDeathCutsceneSFX();
			}
		}
		else
		{
			CancelSpells();
			if (_level.ReloadRoom(1.5f))
			{
				_areControlsLocked = true;
			}
		}
	}

	private void PlayDeathCutsceneSFX()
	{
		_level.PlayCue(ESFX.LunaisDeathCutscene, Position);
	}

	private void UpdateDeathCutscene(float delta)
	{
		_isAffectedByGravity = false;
		_isFlying = true;
		_isBlockingPlayerInput = true;
		_gamePadWrapper.UpdateState(areControlsLocked: true);
		_deathCutsceneTimer += delta;
		if (_deathCutsceneTimer < 4f)
		{
			if (_deathCutsceneTimer < 1.25f)
			{
				float num = _deathCutsceneTimer / 1.25f;
				_velocity.X += (float)(IsFacingLeft ? 1 : (-1)) * 5f * (1f - num);
				_velocity.Y += -10f * (float)Math.Cos(num * ((float)Math.PI * 5f / 8f));
			}
			if (_deathCutsceneTimer > 0.25f)
			{
				float num2 = _deathCutsceneTimer - 0.25f;
				float num3 = num2 / 1.75f;
				if (num3 <= 1f)
				{
					_level.GrayscaleFadePercentage = (float)(1.0 - Math.Cos((float)Math.PI / 2f * num3));
				}
			}
			if (!_hasDeathExploded)
			{
				float num4 = (float)(1.0 - Math.Cos((float)Math.PI / 2f * _deathCutsceneTimer / 2f));
				if (num4 <= 1f)
				{
					_isGlowing = true;
					_glowBase = 2f + num4 * 10f;
					_glowColor = BaseDeathSandGlowColor;
					_orbManager.UpdateDeathGlow(_glowBase, _glowColor);
				}
				if (_deathChargeParticles != null && _deathLazerParticles != null && _deathCutsceneTimer > 0.1f)
				{
					num4 = (_deathCutsceneTimer - 0.1f) / 2f;
					int num5 = (int)(64f * num4);
					int num6 = 5 + num5;
					int num7 = 10 + num5;
					_deathChargeParticles.MinStartRadius = num6;
					_deathChargeParticles.MaxStartRadius = num7;
					_deathLazerParticles.MinStartRadius = num6;
					_deathLazerParticles.MaxStartRadius = num7;
					Vector2 where = new Vector2(Position.X + -4 * ((!IsFacingLeft) ? 1 : (-1)), Position.Y + -16);
					_deathChargeParticles.AddParticles(where);
					_deathLazerParticles.AddParticles(where);
				}
			}
			if (_deathChargeParticles != null && _deathLazerParticles != null)
			{
				_deathChargeParticles.Update(delta);
				_deathLazerParticles.Update(delta);
			}
			if (_deathExplosionAnimation != null)
			{
				_deathExplosionAnimation.Update(delta);
			}
			if (_deathStreamers != null)
			{
				_deathStreamers.Update(delta);
			}
			else if (_deathCutsceneTimer >= 2f)
			{
				_hasDeathExploded = true;
				_doesDrawOrbs = false;
				Point inPosition = Position.Add(-4 * ((!IsFacingLeft) ? 1 : (-1)), -8);
				_deathStreamers = new SandStreamerEvent(_level, inPosition, ESandStreamerType.HeroDeath);
				_deathChargeParticles = null;
				_deathLazerParticles = null;
				Point inPosition2 = Position.Add(-5 * ((!IsFacingLeft) ? 1 : (-1)), -22);
				_deathExplosionAnimation = new BattleAnimation(_level.GCM.SpEffectsLarge, inPosition2, _level)
				{
					TeamSide = ETeamSide.Heroes,
					AnimationSpeed = 0.04f,
					AnimationStart = 18,
					AnimationLength = 7
				};
			}
		}
		else
		{
			_level.ShowGameOverScreen();
		}
	}

	protected override void DoLandingAction()
	{
		if (!base.WasInWater && _lastOverrideFallSpeedAmount > 4f)
		{
			_fallCrouchTimer = 0.5f;
			_isFallCrouching = true;
			ManageState(EAFSM.Ducking);
		}
		if (!_isStandingOnMonster && _lastTimeSinceGrounded > 0.1f && !base.WasInWater)
		{
			if (!_level.IsMufflingPlayerSFX)
			{
				_level.PlayCue(ESFX.LunaisLand, Position);
			}
			_landingDustParticleSystem.AddParticles(new Vector2(Bbox.Center.X, Bbox.Bottom), _lastOverrideFallSpeedAmount);
		}
		base.DoLandingAction();
	}

	public override void DoneCasting()
	{
		_orbManager.UnStopTime();
		base.DoneCasting();
	}

	public override void TeleportToPoint(Point moveTo)
	{
		base.TeleportToPoint(moveTo);
		_orbManager.ShiftMainOrbPosition(GetBulletOffset(26f));
		_orbManager.Update(0f, _spellManager.ChargeSelect);
		_passiveManager.OnTeleportToPoint(moveTo);
		_isBeingKnockedBack = false;
	}

	protected override void DoHorizontalRun(float moveAmount)
	{
		EAFSM currentState = _currentState;
		bool isFacingLeft = IsFacingLeft;
		bool flag = _currentState != EAFSM.Ducking || !_gamePadWrapper.IsDownDown;
		if (flag)
		{
			ManageState(EAFSM.Moving);
		}
		IsFacingLeft = moveAmount < 0f;
		if (flag)
		{
			DoTurningAroundAnimation(currentState, isFacingLeft);
			_movementX = MathHelper.Clamp(moveAmount, -1f, 1f);
		}
	}

	protected override void DoVerticalJump()
	{
		if (_currentState == EAFSM.Damaged || _currentState == EAFSM.Dying)
		{
			return;
		}
		if (_currentState == EAFSM.Grabbing)
		{
			if (_grabJumpTimer >= 0.15f)
			{
				if (_isJumpReset)
				{
					if (_gamePadWrapper.IsDownDown)
					{
						IsGrabbing = false;
						_isGrounded = false;
						_isJumping = true;
						_isJumpReset = false;
						_currentJumpTime = _maxJumpTime;
						SetState(EAFSM.Falling);
					}
					else
					{
						ManageState(EAFSM.Jumping);
						_isJumping = true;
						IsGrabbing = false;
						_isJumpReset = false;
						_isDoubleJumping = false;
					}
				}
			}
			else
			{
				_isJumping = false;
			}
		}
		else if (_isGrounded || _timeSinceGrounded < 0.04f)
		{
			if (_isGrounded && _isDoubleJumping)
			{
				_isJumpReset = true;
				_isDoubleJumping = false;
			}
			if (_isJumpReset)
			{
				if (_gamePadWrapper.IsDownDown && !_gamePadWrapper.IsRightDown && !_gamePadWrapper.IsLeftDown && !_isDashing && IsStandingOnPlatform())
				{
					_isIgnoringPlatform = true;
					_isJumpReset = false;
					SetState(EAFSM.Falling);
					PlayCue(ESFX.LunaisJumpDown);
				}
				else
				{
					if (_wasJumping)
					{
						return;
					}
					if (_timeSinceGrounded < 0.04f)
					{
						_isMiracleJumping = true;
					}
					ManageState(EAFSM.Jumping);
					_isJumping = true;
					_isJumpReset = false;
					_isDoubleJumping = false;
					_isFallCrouching = false;
					_isIgnoringPlatform = false;
					for (int num = base.CurrentMovingPlatforms.Count - 1; num >= 0; num--)
					{
						GameEvent gameEvent = base.CurrentMovingPlatforms[num];
						if (gameEvent.IsLostWhenNotGrounded)
						{
							base.CurrentMovingPlatforms.RemoveAt(num);
						}
					}
				}
			}
			else if (_isIgnoringPlatform && _isGrounded && !IsStandingOnPlatform())
			{
				_isIgnoringPlatform = false;
			}
		}
		else if (!_isGrounded && !_isIgnoringPlatform)
		{
			ManageState(EAFSM.Jumping);
			_isJumping = true;
		}
	}

	private void EndDash()
	{
		if (_maxMoveSpeed == 275f)
		{
			_maxMoveSpeed = 145f;
			_shouldIgnoreIntermediateSlopes = false;
		}
	}

	protected override void CarryOutScriptAction(ScriptAction inAction, float delta)
	{
		base.CarryOutScriptAction(inAction, delta);
		switch (inAction.ActionType)
		{
		case EScriptActionType.Idle:
			ScriptIdleSelf();
			_spellManager.CancelCharge();
			_isBlockingPlayerInput = true;
			EndDash();
			break;
		case EScriptActionType.FancyIdle:
			if (_animationStart != 8)
			{
				ChangeAnimation(8, 5, 0.15f, EAnimationType.Cycle, 5, 3, 0.11f);
			}
			break;
		case EScriptActionType.SheatheWeapon:
			SheatheWeapon(inAction.Arguments.X <= 0f, inAction.Arguments.Y <= 0f);
			break;
		case EScriptActionType.Run:
			DoHorizontalRun(inAction.Arguments.X);
			_isBlockingPlayerInput = true;
			_isDashing = false;
			break;
		case EScriptActionType.GoToPoint:
			_isBlockingPlayerInput = true;
			_isDashing = false;
			break;
		case EScriptActionType.Jump:
			DoVerticalJump();
			_isBlockingPlayerInput = true;
			break;
		case EScriptActionType.Duck:
			ManageState(EAFSM.Ducking);
			_isBlockingPlayerInput = true;
			break;
		case EScriptActionType.StopCast:
			if (_doesHaveTimeStopped)
			{
				_doesHaveTimeStopped = false;
				StartAbility(8);
			}
			CancelAbility(overrideCanBeCanceled: true);
			_spellManager.CancelSpellOnHit();
			break;
		case EScriptActionType.GateJump:
		{
			_isBlockingPlayerInput = true;
			_doesDrawSelf = true;
			base.IsBlocked = false;
			_isAffectedByGravity = false;
			_movementX = 0f;
			if (inAction.ActionTimer >= inAction.Duration)
			{
				Position = new Point((int)inAction.Arguments.X, (int)inAction.Arguments.Y + Bbox.Height);
				SnapBboxToPosition();
				SnapFrameToBbox();
				_isGrounded = false;
				if (inAction.Arguments.W <= 0f)
				{
					ChangeAnimation(207, 2, 0.066f, EAnimationType.Cycle);
					_velocity.Y = -150f;
				}
				else
				{
					ChangeAnimation(200, 2, 0.066f, EAnimationType.Cycle);
				}
				if (inAction.Arguments.Z <= 0f)
				{
					_level.SetCameraUpdateDisable(isDisabled: true);
				}
				_isGlowing = true;
				_glowBase = 8f;
				_glowColor = new Color(0.9f, 0.9f, 1f, 0f);
				break;
			}
			if (inAction.ActionTimer <= 0f)
			{
				_isAffectedByGravity = true;
				_doesDrawOrbs = true;
				_isGlowing = false;
				if (inAction.Arguments.Z <= 0f)
				{
					_level.SetCameraUpdateDisable(isDisabled: false);
				}
				if (inAction.Arguments.W <= 0f)
				{
					SetState(EAFSM.Idle);
				}
				break;
			}
			float num5 = inAction.Duration - inAction.ActionTimer;
			float num6 = num5 * 2f;
			if (num6 > 1f)
			{
				_isGlowing = false;
			}
			else
			{
				_glowColor.A = (byte)(num6 * 255f);
			}
			_isAffectedByGravity = true;
			if (_isGrounded && !_wasGrounded)
			{
				if (inAction.Arguments.W <= 0f)
				{
					ChangeAnimation(new List<AnimationSpec>
					{
						new AnimationSpec
						{
							Start = 209,
							Length = 3,
							Speed = 0.066f
						},
						new AnimationSpec
						{
							Start = 211,
							Length = 3,
							Speed = 0.066f
						},
						new AnimationSpec
						{
							Start = 0,
							Length = 5,
							Speed = 0.11f
						}
					});
				}
				else
				{
					ChangeAnimation(new AnimationSpec
					{
						Start = 202,
						Length = 3,
						Speed = 0.066f,
						Type = EAnimationType.Once
					});
				}
				inAction.ActionTimer = 0.396f;
			}
			break;
		}
		case EScriptActionType.PortalSuction:
		{
			_isAffectedByGravity = false;
			Vector2 vector = new Vector2(inAction.Arguments.X, inAction.Arguments.Y);
			Vector2 vector2 = new Vector2(inAction.Arguments.Z, inAction.Arguments.W);
			Vector2 vector3 = new Vector2(vector2.X - vector.X, vector2.Y - vector.Y);
			float num2 = vector3.Length();
			float num3 = (float)Math.Atan2(vector3.Y, vector3.X);
			float amount = (float)Math.Cos((float)Math.PI / 2f * inAction.ActionTimer / inAction.Duration);
			float num4 = num2 * (1f - amount);
			Vector2 vector4 = new Vector2((float)Math.Cos((double)amount * Math.PI * 3.0 + (double)num3) * num4, (float)Math.Sin((double)amount * Math.PI * 3.0 + (double)num3) * num4);
			base.Rotation = (float)((double)amount * Math.PI * 8.0);
			DrawOrigin = new Vector2((float)_frameSource.Width / 2f, (float)_frameSource.Height / 2f);
			Position = new Point((int)(vector2.X - vector4.X), (int)(vector2.Y - vector4.Y));
			break;
		}
		case EScriptActionType.MawDoorSuck:
		{
			if (_isAffectedByGravity)
			{
				inAction.Arguments = new Vector4(Position.X, Position.Y, inAction.Arguments.Z, inAction.Arguments.W);
			}
			_isAffectedByGravity = false;
			Point start2 = new Point((int)inAction.Arguments.X, (int)inAction.Arguments.Y);
			Point end = new Point((int)inAction.Arguments.Z, (int)inAction.Arguments.W);
			float num = 1f - inAction.ActionTimer / inAction.Duration;
			num = num * num * num;
			Position = start2.Lerp(end, num);
			break;
		}
		case EScriptActionType.MawDoorSpit:
		{
			_isBlockingPlayerInput = true;
			_isAffectedByGravity = false;
			_isGrounded = false;
			_wasGrounded = false;
			if (inAction.ActionTimer >= inAction.Duration)
			{
				IsFacingLeft = inAction.Arguments.X < inAction.Arguments.Z;
				ChangeAnimation(200, 2, 0.1f, EAnimationType.Cycle);
			}
			Point point2 = new Point((int)inAction.Arguments.X, (int)inAction.Arguments.Y);
			Point point3 = new Point((int)inAction.Arguments.Z, (int)inAction.Arguments.W);
			float num10 = 1f - inAction.ActionTimer / inAction.Duration;
			Position = new Point((int)Math.Ceiling(MathHelper.Lerp(point2.X, point3.X, num10)), (int)Math.Ceiling(MathEx.SineInterpolate(point2.Y, point3.Y, num10 * num10 * num10)));
			if (inAction.ActionTimer <= 0f)
			{
				_isAffectedByGravity = true;
				_isGrounded = true;
				_wasGrounded = true;
				ChangeAnimation(202, 3, 0.1f, EAnimationType.Once);
			}
			break;
		}
		case EScriptActionType.GrowAura:
			if (_spellManager != null)
			{
				_spellManager.GrowAura(delta * 2f);
				if (_spellManager.EquippedSpell != null)
				{
					base.AuraColor = _spellManager.EquippedSpell.GetAuraColor();
				}
			}
			break;
		case EScriptActionType.Stun:
			if (!(inAction is StunScript stunScript))
			{
				break;
			}
			if (!stunScript.HasStarted)
			{
				stunScript.HasStarted = true;
				_movementX = 0f;
				if (stunScript.StunAnimationType == StunScript.EStunAnimationType.Wind)
				{
					if (_isGrounded)
					{
						ChangeAnimation(214, 2, 0.1f, EAnimationType.Cycle);
					}
					else
					{
						ChangeAnimation(216, 2, 0.1f, EAnimationType.Cycle);
					}
				}
				else
				{
					ChangeAnimation(216, 2, 0.1f, EAnimationType.Cycle);
				}
			}
			stunScript.UpdateStun(delta, this, _gamePadWrapper.IsTimeDown);
			if (!stunScript.IsFinished)
			{
				_isBlockingPlayerInput = true;
			}
			break;
		case EScriptActionType.Backdash:
			if (base.CurrentState != EAFSM.Backdashing)
			{
				_backdashTimer = 0.54899997f;
				_hasDuckBeenCanceled = _currentState == EAFSM.Ducking;
				_isFallCrouching = false;
				_isBeingKnockedBack = false;
				ManageState(EAFSM.Backdashing);
			}
			break;
		case EScriptActionType.Drowning:
			if (inAction.Arguments.W <= 0f)
			{
				if (_isAffectedByWater)
				{
					_isAffectedByGravity = false;
					_isAffectedByWater = false;
					base.DoesNotMakeSplashesInWater = true;
					_isFlying = true;
					IsFacingLeft = true;
					ChangeAnimation(243, 1, 0.1f, EAnimationType.Cycle);
				}
				float num7 = inAction.Duration - inAction.ActionTimer;
				float num8 = (float)Math.Round(Math.Sin(num7 * 2.5f) * 1.0);
				int num9 = (int)inAction.Arguments.X;
				int x = num9;
				if (inAction.Arguments.Z > 0f && inAction.Duration > 0f)
				{
					float amount2 = num7 / inAction.Duration;
					x = (int)Math.Ceiling(MathHelper.Lerp(num9, inAction.Arguments.Z, amount2));
				}
				Position = new Point(x, (int)(inAction.Arguments.Y + num8));
			}
			else
			{
				_isAffectedByGravity = true;
				_isAffectedByWater = true;
				_isFlying = false;
				base.DoesNotMakeSplashesInWater = false;
			}
			break;
		case EScriptActionType.StartFloating:
		{
			_isAffectedByGravity = false;
			_isFlying = true;
			_velocity = Vector2.Zero;
			Vector4 arguments = inAction.Arguments;
			if (inAction.Arguments.Z <= 0f)
			{
				inAction.Arguments = new Vector4(arguments.X, arguments.Y, Position.X, Position.Y);
				arguments = inAction.Arguments;
				SetState(EAFSM.Falling);
				ChangeAnimation(245, 5, 0.07f, EAnimationType.Cycle);
			}
			Point point = new Point((int)arguments.X, (int)arguments.Y);
			if (inAction.Duration > 0f && inAction.ActionTimer >= 0f)
			{
				Point start = new Point((int)arguments.Z, (int)arguments.W);
				float amount = 1f - inAction.ActionTimer / inAction.Duration;
				Position = start.SineInterpolate(point, amount);
			}
			else
			{
				Position = point;
			}
			break;
		}
		case EScriptActionType.StopFloating:
			_isAffectedByGravity = true;
			_isFlying = false;
			break;
		case EScriptActionType.ChargeSpell:
		{
			bool flag = inAction.ActionTimer >= inAction.Duration;
			_spellManager.ProcessInput(isButtonDown: true, !flag, isParentStunned: false);
			break;
		}
		case EScriptActionType.CastSpell:
			_spellManager.ProcessInput(isButtonDown: false, wasButtonDown: true, isParentStunned: false);
			break;
		case EScriptActionType.Special:
		case EScriptActionType.ChangeAnimation:
		case EScriptActionType.LookDirection:
		case EScriptActionType.ChangeColor:
		case EScriptActionType.WarpToPoint:
		case EScriptActionType.StartGlowing:
			break;
		}
	}

	public override void StartAbility(int whichAbility)
	{
		_canCurrentAbilityCanBeCanceled = false;
		_isCurrentAbilityGivingFlight = false;
		_selectedAbilityVariation = 0;
		_wasFacingLeftWhenStartedAbility = IsFacingLeft;
		_isDuckingDuringAbility = _gamePadWrapper.IsDownDown && _isGrounded && !_isJumping;
		switch (whichAbility)
		{
		case 1:
		case 2:
		case 3:
			PlayCue(ESFX.VO_Lun_ChargeReleaseMedium, Position);
			_selectedAbility = whichAbility;
			ManageState(EAFSM.Special);
			_isCarryingOutAbility = true;
			_hasUsedAbility = false;
			_totalAbilityTime = 0.8f;
			_canCurrentAbilityCanBeCanceled = true;
			CarryOutAbility(0f);
			break;
		case 4:
		case 5:
			PlayCue(ESFX.VO_Lun_ChargeReleaseLarge, Position);
			_selectedAbility = whichAbility;
			ManageState(EAFSM.Special);
			_isCarryingOutAbility = true;
			_hasUsedAbility = false;
			_totalAbilityTime = 0.8f;
			_canCurrentAbilityCanBeCanceled = true;
			CarryOutAbility(0f);
			break;
		case 6:
			_isDuckingDuringAbility = false;
			PlayCue(ESFX.VO_Lun_ChargeReleaseLarge, Position);
			_selectedAbility = whichAbility;
			ManageState(EAFSM.Special);
			_isCarryingOutAbility = true;
			_hasUsedAbility = false;
			_totalAbilityTime = 4f;
			_canCurrentAbilityCanBeCanceled = true;
			CarryOutAbility(0f);
			break;
		case 7:
			if (!_level.IsTimePermanentlyFrozen)
			{
				_level.PlayCue(ESFX.VO_Lun_TimeStop, Position);
			}
			_level.FreezeTime(ETeamSide.Heroes, reFreeze: false);
			_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsLarge, Position, _level)
			{
				TeamSide = ETeamSide.Enemies,
				AnchorObject = this,
				AnchorOffset = new Point(IsFacingLeft ? 1 : (-1), -Bbox.Height / 2 - 2),
				AnimationSpeed = 0.06f,
				AnimationStart = 0,
				AnimationLength = 4,
				DrawColor = Color.White * 0.8f
			});
			_orbManager.StopTime();
			_isDuckingDuringAbility = false;
			break;
		case 8:
			_level.UnfreezeTime(ETeamSide.Heroes);
			_orbManager.UnStopTime();
			_isDuckingDuringAbility = false;
			break;
		case 9:
			PlayCue(ESFX.VO_Lun_OrbThrow, Position, isLooped: false, 0.5f);
			_selectedAbility = 9;
			_selectedAbilityVariation = ((!_orbManager.IsPrimaryOrbNext) ? ((byte)1) : ((byte)0));
			ManageState(EAFSM.Special);
			_isCarryingOutAbility = true;
			_hasUsedAbility = false;
			_abilityTimer = 0f;
			_totalAbilityTime = 0.8f;
			_canCurrentAbilityCanBeCanceled = true;
			CarryOutAbility(0f);
			break;
		case 10:
		case 11:
			_isDuckingDuringAbility = false;
			PlayCue(ESFX.VO_Lun_ChargeReleaseMedium, Position, isLooped: false, 0.66f);
			_canCurrentAbilityCanBeCanceled = false;
			_selectedAbility = whichAbility;
			ManageState(EAFSM.Special);
			_isCarryingOutAbility = true;
			_hasUsedAbility = false;
			_totalAbilityTime = 4f;
			_isDuckingDuringAbility = false;
			CarryOutAbility(0f);
			break;
		}
		base.StartAbility(whichAbility);
	}

	public override void CarryOutAbility(float delta)
	{
		if (_isGrounded && !_wasGrounded && _canCurrentAbilityCanBeCanceled && _hasUsedAbility)
		{
			CancelAbility(overrideCanBeCanceled: false);
			_currentState = EAFSM.Falling;
			ManageState(EAFSM.Idle);
			return;
		}
		switch (_selectedAbility)
		{
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
			if (UpdateAbilityThrowAnimation(0.07f, isVariationOne: false) && _spellManager.CreateSpellProjectiles())
			{
				_hasUsedAbility = true;
				_spellManager.FinishCastingSpell();
			}
			break;
		case 6:
		{
			float num = 0.05f;
			float num2 = 25f * num + 0.28f;
			if (_abilityTimer <= 0f)
			{
				_orbManager.HideOrbs(num2);
				_didStartAbilityGrounded = ShouldStartAbilityGrounded();
				if (_didStartAbilityGrounded)
				{
					_movementX = 0f;
					ChangeAnimation(159, 4, 0.07f, EAnimationType.Once);
				}
				else
				{
					ChangeAnimation(169, 4, 0.07f, EAnimationType.Once);
				}
			}
			if (_abilityTimer >= 0.155f && _lastAbilityTimer < 0.155f)
			{
				_spellManager.CreateSpellProjectiles();
				_spellManager.FinishCastingSpell();
				_isAffectedByGravity = false;
				_movementX = 0f;
				_velocity = Vector2.Zero;
				_canCurrentAbilityCanBeCanceled = false;
				_isCurrentAbilityGivingFlight = true;
				_invulnerableTimer = num2 - 0.462f;
				if (_isGrounded)
				{
					ChangeAnimation(163, 5, num, EAnimationType.Cycle);
				}
				else
				{
					ChangeAnimation(173, 5, num, EAnimationType.Cycle);
				}
			}
			else if (_abilityTimer >= num2)
			{
				_hasUsedAbility = true;
				EndAbility();
				ManageState(EAFSM.Idle);
				if (_isGrounded)
				{
					ChangeAnimation(0, 5, 0.11f, EAnimationType.Cycle, 168, 1, 0.07f);
				}
				_isAffectedByGravity = true;
			}
			if (_isGrounded && !_didStartAbilityGrounded && _isCarryingOutAbility)
			{
				_didStartAbilityGrounded = true;
				ChangeAnimation(163 + _animationIndex, 5, num, EAnimationType.Cycle);
			}
			break;
		}
		case 9:
			if (UpdateAbilityThrowAnimation(_orbManager.GetMeleeAnimationSpeed(), _selectedAbilityVariation == 0))
			{
				ThrowOrb(1);
			}
			break;
		case 10:
		{
			float num = 0.08f;
			if (_abilityTimer == 0f)
			{
				_didStartAbilityGrounded = ShouldStartAbilityGrounded();
				if (_didStartAbilityGrounded)
				{
					ChangeAnimation(139, 10, num, EAnimationType.Once);
				}
				else
				{
					ChangeAnimation(149, 10, num, EAnimationType.Once);
				}
			}
			if (_abilityTimer >= 1f * num && !_hasUsedAbility)
			{
				_spellManager.CreateSpellProjectiles();
				_orbManager.HideOrbs(0.6f);
				_spellManager.FinishCastingSpell();
				_hasUsedAbility = true;
			}
			else if (_abilityTimer > 10f * num)
			{
				ChangeAnimation(0, 5, 0.11f, EAnimationType.Cycle);
				ManageState(EAFSM.Idle);
				EndAbility();
			}
			break;
		}
		case 11:
		{
			float num = 0.08f;
			if (_abilityTimer == 0f)
			{
				_didStartAbilityGrounded = ShouldStartAbilityGrounded();
				if (_didStartAbilityGrounded)
				{
					ChangeAnimation(139, 10, num, EAnimationType.Once);
				}
				else
				{
					ChangeAnimation(149, 10, num, EAnimationType.Once);
				}
			}
			if (_abilityTimer >= 1f * num && !_hasUsedAbility)
			{
				_spellManager.CreateSpellProjectiles();
				_orbManager.HideOrbs(0.6f);
				_spellManager.FinishCastingSpell();
				_hasUsedAbility = true;
			}
			else if (_abilityTimer > 10f * num)
			{
				ChangeAnimation(0, 5, 0.11f, EAnimationType.Cycle);
				ManageState(EAFSM.Idle);
				EndAbility();
			}
			break;
		}
		}
		base.CarryOutAbility(delta);
	}

	private bool UpdateAbilityThrowAnimation(float increment, bool isVariationOne)
	{
		bool result = false;
		if (_abilityTimer <= 0f)
		{
			_didStartAbilityGrounded = ShouldStartAbilityGrounded();
			if (_didStartAbilityGrounded)
			{
				if (isVariationOne)
				{
					if (_isDuckingDuringAbility)
					{
						ChangeAnimation(119, 5, increment, EAnimationType.Once);
					}
					else
					{
						ChangeAnimation(109, 5, increment, EAnimationType.Once);
					}
				}
				else if (_isDuckingDuringAbility)
				{
					ChangeAnimation(124, 5, increment, EAnimationType.Once);
				}
				else
				{
					ChangeAnimation(114, 5, increment, EAnimationType.Once);
				}
			}
			else if (isVariationOne)
			{
				ChangeAnimation(129, 5, increment, EAnimationType.Once);
			}
			else
			{
				ChangeAnimation(134, 5, increment, EAnimationType.Once);
			}
		}
		if (_abilityTimer >= 1f * increment && !_hasUsedAbility)
		{
			result = true;
			_hasUsedAbility = true;
		}
		else if (_abilityTimer > 5f * increment)
		{
			if (_isGrounded)
			{
				if (_isDuckingDuringAbility)
				{
					_currentState = EAFSM.Ducking;
					ChangeAnimation(48, 1, 0.1f, EAnimationType.None);
				}
				else
				{
					ChangeAnimation(0, 5, 0.11f, EAnimationType.Cycle);
					ManageState(EAFSM.Idle);
				}
			}
			else
			{
				ChangeAnimation(55, 3, 0.05f, EAnimationType.Cycle, 53, 2, 0.05f);
				ManageState(EAFSM.Idle);
			}
			EndAbility();
		}
		if (_isCarryingOutAbility)
		{
			if (_isGrounded && !_wasGrounded)
			{
				_didStartAbilityGrounded = true;
				if (isVariationOne)
				{
					ChangeAnimation(109 + _animationIndex, 5 - _animationIndex, increment, EAnimationType.Once);
				}
				else
				{
					ChangeAnimation(114 + _animationIndex, 5 - _animationIndex, increment, EAnimationType.Once);
				}
			}
			else if (!_isGrounded && _wasGrounded)
			{
				if (isVariationOne)
				{
					ChangeAnimation(129 + _animationIndex, 5 - _animationIndex, increment, EAnimationType.Once);
				}
				else
				{
					ChangeAnimation(134 + _animationIndex, 5 - _animationIndex, increment, EAnimationType.Once);
				}
			}
		}
		return result;
	}

	public override void EndAbility()
	{
		_isDuckingDuringAbility = false;
		base.EndAbility();
	}

	public override void ChangeRoom()
	{
		_orbManager.ChangeRoom();
		_spellManager.ChangeRoom();
		_passiveManager.ChangeRoom();
		_familiarManager.ChangeRoom();
		_areControlsLocked = false;
		ClearTrailHistory();
		base.ChangeRoom();
	}

	public override void CancelSpells()
	{
		if (_level.IsTimeFrozen)
		{
			_doesHaveTimeStopped = false;
			_level.UnfreezeTime(ETeamSide.Heroes);
			_orbManager.UnStopTime();
		}
		_spellManager.CancelCharge();
		base.CancelSpells();
	}

	internal void OnOrbEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBBox)
	{
		if (_passiveManager != null)
		{
			_passiveManager.OnMeleeEnemyContact(enemy, damageArea, contactBBox);
		}
	}

	internal void OnSuccessfulEnemyHit(Alive enemy, EOrbSlot orbType)
	{
		if (orbType == EOrbSlot.Melee && _passiveManager != null)
		{
			_passiveManager.OnSuccessfulMeleeEnemyHit(enemy);
		}
	}

	private bool ShouldStartAbilityGrounded()
	{
		if (_isGrounded)
		{
			if (_isJumping)
			{
				if (!(_currentJumpTime > 0f))
				{
					return _wasJumping;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override void HideAndBlockInput(bool shouldBlockAndHide, bool shouldHideFamiliar)
	{
		_doesDrawOrbs = !shouldBlockAndHide;
		_doesDrawSelf = !shouldBlockAndHide;
		base.IsBlocked = shouldBlockAndHide;
		ShouldHideFamiliarToo = shouldHideFamiliar;
	}

	public void SheatheWeapon(bool shouldHide, bool shouldShowAnimation)
	{
		AreWeaponsSheathed = shouldHide;
		if (_orbManager != null)
		{
			_orbManager.SetOrbHiddenStatus(shouldHide, shouldShowAnimation);
		}
		if (_passiveManager != null)
		{
			_passiveManager.SetHiddenStatus(shouldHide);
		}
	}

	internal override void StopGrabbing()
	{
		IsGrabbing = false;
		_isGrounded = false;
		SetState(EAFSM.Falling);
	}

	internal override bool GiveStatusEffect(EStatusEffectType statusType, int power)
	{
		bool flag = true;
		if (statusType == EStatusEffectType.Poison || statusType == EStatusEffectType.Chaos)
		{
			EInventoryEquipmentType equippedTrinketA = _level.GameSave.Inventory.EquippedTrinketA;
			EInventoryEquipmentType equippedTrinketB = _level.GameSave.Inventory.EquippedTrinketB;
			switch (statusType)
			{
			case EStatusEffectType.Poison:
				if (equippedTrinketA == EInventoryEquipmentType.Pendulum || equippedTrinketB == EInventoryEquipmentType.Pendulum)
				{
					flag = false;
				}
				break;
			case EStatusEffectType.Chaos:
				if (equippedTrinketA == EInventoryEquipmentType.BirdStatue || equippedTrinketB == EInventoryEquipmentType.BirdStatue)
				{
					flag = false;
				}
				break;
			}
		}
		if (flag)
		{
			return base.GiveStatusEffect(statusType, power);
		}
		return false;
	}

	internal override void ReduceAura(float reductionAmount)
	{
		_spellManager.ReduceAura(reductionAmount);
	}

	internal void ChangeEquippedSpell(EInventoryOrbType spell)
	{
		_spellManager.ChangeSpell(spell);
	}

	internal void ChangeEquippedPassive(EInventoryOrbType passive)
	{
		_passiveManager.ChangePassive(passive);
	}
}
