using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal sealed class RavenBoss : BossClass
{
	private enum ERavenlordPhase
	{
		Phase1,
		Phase2,
		Phase3
	}

	private enum ERavenlordGlobalState
	{
		Idle,
		Teleport,
		RavenBarrage,
		WindGusts,
		GrappleLunge,
		RammingLunge
	}

	private const int MaximumSingleRavens = 16;

	private const int MaxAbilities = 2;

	private const float Phase2Threshold = 0.67f;

	private const float Phase3Threshold = 0.33f;

	private const float IndefiniteActionTime = 1000f;

	private const float MinimumIdleTime = 1f;

	private const float MaximumIdleTime = 2f;

	private const float TimeForEyeFlash = 0.6f;

	private const float SingleRavenSpeed = 150f;

	private const float SingleRavenSpeedHyper = 250f;

	private const float TimeForPassiveSingleRavenAttack = 2f;

	private const float TimeForPassiveRavenFlockAttack = 5f;

	private const float TimeForPassiveSingleRavenAttackPhase3 = 1f;

	private const float TimeForPassiveRavenFlockAttackPhase3 = 3f;

	private const int BarrageDistanceSquaredThreshold = 65536;

	private const int RavenBarrageCount = 12;

	private const float TimeBetweenRavenBarrageRavens = 0.35f;

	private const float TimeForEntireRavenBarrageAction = 4.2f;

	private const int RoomUnitWidth = 400;

	private const int RoomUnitHeight = 320;

	private const int HalfRoomWidth = 200;

	private const int HalfRoomHeight = 160;

	private const float TimeBeforeTeleportReappearing = 1f;

	private const float TimeForEntireTeleportAction = 2f;

	private const int RamWindupWidth = 48;

	private const int RamLungeWidth = 128;

	private const int RamRecoverWidth = 80;

	private const float TimeForRamWindup = 1.25f;

	private const float TimeForRamLunge = 0.35f;

	private const float TimeForRamRecover = 0.5f;

	private const float TimeBeforeRamRecover = 1.6f;

	private const float TimeForEntireRammingLungeAction = 2.1f;

	private const int GrappleWindupRadius = 32;

	private const int GrappleLungeMaxRadius = 160;

	private const int GrappleLungeOvershootRadius = 24;

	private const int GrappleRecoilRadius = 32;

	private const int GrappleGrabRadius = 32;

	private const int GrappleGrabRadiusSquared = 1024;

	private const float TimeForGrappleWindup = 1.35f;

	private const float TimeForGrappleLunge = 0.5f;

	private const float TimeForGrappleFailRecoil = 1f;

	private const float TimeBeforeGrappleInitiate = 1.85f;

	private const float TimeForEntireGrappleSequence = 2.85f;

	private const int PostGrappleWindupWidth = 64;

	private const int PostGrappleThrowWidth = 128;

	private const int PostGrappleRecoverWidth = 64;

	private const float TimeForPostGrappleWindup = 0.5f;

	private const float TimeForPostGrappleThrow = 0.15f;

	private const float TimeForPostGrappleRecover = 0.25f;

	private const float TimeBeforePostGrappleWindup = 0f;

	private const float TimeBeforePostGrappleThrow = 0.5f;

	private const float TimeBeforePostGrappleRecover = 0.65f;

	private const float TimeForEntirePostGrappleSequence = 0.9f;

	private const float TimeForEntireWindGustsAction = 5f;

	private const float TimeToStillEmitNormalRavens = 1.25f;

	private const float TimeForDeathChargeUp = 2f;

	private const float TimeForDeathExplosion = 2f;

	private const float TimeForEntireDeathSequence = 4f;

	private const float MinimumTimeBetweenCroaks = 0.5f;

	private const float CroakChanceInterval = 0.25f;

	private const float CroakPlayProbability = 0.25f;

	private const int BestiaryFrames = 20;

	private const float BestiaryDelta = 0.1f;

	private readonly int _baseDamage;

	private readonly SFXCueInstance _flockLoopCueInstance;

	private readonly Appendage _eyesAppendage;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _disappearSequence;

	private readonly CharacterSequenceSpecification _appearSequence;

	private readonly CharacterSequenceSpecification _turretSequence;

	private readonly CharacterSequenceSpecification _wallSequence;

	private readonly CharacterSequenceSpecification _ramSequence;

	private readonly CharacterSequenceSpecification _wheelSequence;

	private readonly RavenBossRavenParticleSystem _ravenParticles;

	private readonly RavenBossSmokeParticleSystem _smokeParticles;

	private readonly RavenBossFeathersParticleSystem _impactFeatherParticles;

	private readonly RavenBossDeathRavenParticleSystem _deathRavenParticles;

	private readonly RavenBossWindZone _windZone;

	private readonly RavenBossDangerAppendage _ramDangerAppendage;

	private readonly RavenBossSingleProjectile[] _singleRavenProjectiles = new RavenBossSingleProjectile[16];

	private readonly List<BoundingBoxParticleEmitter> _ravenParticleEmitters = new List<BoundingBoxParticleEmitter>();

	private readonly List<BoundingBoxParticleEmitter> _smokeParticleEmitters = new List<BoundingBoxParticleEmitter>();

	private readonly List<BoundingBoxParticleEmitter> _deathRavenParticleEmitters = new List<BoundingBoxParticleEmitter>();

	private bool _isNeedingToTeleport;

	private bool _hasBeenIdleYet;

	private bool _hasAddedWindzone;

	private bool _isDrawingRamDangerArea;

	private bool _hasGrabbedPlayer;

	private bool _isThrowingPlayerToLeft;

	private ERavenlordPhase _currentPhase;

	private ERavenlordGlobalState _currentGlobalState;

	private int _singleRavenCounter;

	private int _lastActionInteger = -1;

	private float _globalStateTimer;

	private float _lastGlobalStateTimer;

	private float _passiveSingleRavenTimer;

	private float _passiveRavenFlockTimer;

	private float _currentMaximumIdleTime;

	private float _ravenBarrageTimer;

	private float _eyeFlashTimer;

	private float _distanceToPlayer;

	private float _croakTimer;

	private float _croakIntervalTimer;

	private Point _lastPlayerPosition;

	private Point _idleOffsetPoint;

	private Point _ramStartPosition;

	private Vector2 _grappleVector;

	private RavenBossFlockProjectile _ravenFlockProjectile;

	private RavenBossThrowStunScript _throwStunScript;

	public RavenBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_agility = 0.1f;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		_baseDamage = base.Damage;
		_damageCaused = 0;
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_doesDrawParticleSystemsUnder = true;
		_doAppendagesMatchImageFacing = true;
		_currentPhase = ERavenlordPhase.Phase1;
		_currentGlobalState = ERavenlordGlobalState.Idle;
		_nextActionTimer = 1f;
		_isFlying = true;
		_isAffectedByGravity = false;
		base.DoesCollideWithTiles = true;
		_isIgnoringPlatform = true;
		base.IsImmuneToSpikes = true;
		base.IsSolidWhenFrozen = false;
		if (base.Appendages.Count > 0)
		{
			Appendage appendage = base.Appendages[0];
			if (appendage.Appendages.Count > 0)
			{
				_eyesAppendage = appendage.Appendages[0];
				Color auraColor = new Color(255, 60, 20);
				_eyesAppendage.DoesDrawBaseSprite = false;
				_eyesAppendage.DoesDrawAura = true;
				_eyesAppendage.AuraColor = auraColor;
				_eyesAppendage.AuraOffset = new Vector2(0f, 0f);
				_eyesAppendage.AuraSize = 0f;
				_eyesAppendage.AuraCount = 1f;
				_eyesAppendage.AuraFrequency = 4f;
			}
		}
		_ramDangerAppendage = new RavenBossDangerAppendage(this, _level, _sprite);
		_windZone = new RavenBossWindZone(_level, Position, new ObjectTileSpecification(), _sprite);
		_smokeParticles = new RavenBossSmokeParticleSystem(_sprite, 128);
		_particleSystems.Add(_smokeParticles);
		_ravenParticles = new RavenBossRavenParticleSystem(_sprite, 256, 2, 4);
		_particleSystems.Add(_ravenParticles);
		_impactFeatherParticles = new RavenBossFeathersParticleSystem(_sprite, 8, 6, 4);
		_particleSystems.Add(_impactFeatherParticles);
		_deathRavenParticles = new RavenBossDeathRavenParticleSystem(_sprite, 128, 2, 4);
		int num = _level.NextRandomInt(0, 256);
		foreach (Appendage appendage2 in _appendages)
		{
			BoundingBoxParticleEmitter item = new BoundingBoxParticleEmitter(appendage2, _ravenParticles, num++)
			{
				InstancesPerEmission = 2,
				TimeBetweenEmissions = 0.033f
			};
			_ravenParticleEmitters.Add(item);
			BoundingBoxParticleEmitter item2 = new BoundingBoxParticleEmitter(appendage2, _smokeParticles, num++)
			{
				InstancesPerEmission = 1,
				TimeBetweenEmissions = 0.05f
			};
			_ravenParticleEmitters.Add(item2);
			BoundingBoxParticleEmitter item3 = new BoundingBoxParticleEmitter(appendage2, _deathRavenParticles, num++)
			{
				InstancesPerEmission = 1,
				TimeBetweenEmissions = 0.075f
			};
			_deathRavenParticleEmitters.Add(item3);
		}
		_idleSequence = GetCharacterSequenceByName("Idle");
		_disappearSequence = GetCharacterSequenceByName("Disappear");
		_appearSequence = GetCharacterSequenceByName("Appear");
		_turretSequence = GetCharacterSequenceByName("Turret");
		_wallSequence = GetCharacterSequenceByName("Wall");
		_ramSequence = GetCharacterSequenceByName("Ram");
		_wheelSequence = GetCharacterSequenceByName("Wheel");
		_flockLoopCueInstance = CreateCue(ESFX.BossRavenFlockLoop, Position, isLooped: true);
		if (_flockLoopCueInstance != null)
		{
			_flockLoopCueInstance.RangeMultiplier = 1.75f;
			_flockLoopCueInstance.PlayWhenInRange();
		}
	}

	public override void InitializeMob()
	{
		base.InitializeMob();
		PlayCue(ESFX.BossRavenAggro);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_globalStateTimer = 0f;
		_lastGlobalStateTimer = -1E-07f;
		_lastPlayerPosition = _level.GetNearestProtagonistPosition(Position);
		_nextActionTimer = 1000f;
		bool flag = false;
		float hPPercentage = base.HPPercentage;
		if (_currentPhase != ERavenlordPhase.Phase2 && hPPercentage <= 0.67f && hPPercentage > 0.33f)
		{
			flag = true;
			_currentPhase = ERavenlordPhase.Phase2;
		}
		else if (_currentPhase != ERavenlordPhase.Phase3 && hPPercentage <= 0.33f)
		{
			flag = true;
			_currentPhase = ERavenlordPhase.Phase3;
		}
		if (!flag && !_isNeedingToTeleport)
		{
			Point center = Bbox.Center;
			Vector2 vector = new Vector2(_lastPlayerPosition.X - center.X, _lastPlayerPosition.Y - center.Y);
			float num = vector.LengthSquared();
			IsFacingLeft = vector.X <= 0f;
			if (num >= 65536f)
			{
				_currentGlobalState = ((_currentGlobalState != ERavenlordGlobalState.RavenBarrage) ? ERavenlordGlobalState.RavenBarrage : ERavenlordGlobalState.Idle);
			}
			else if (_currentGlobalState != 0)
			{
				_currentGlobalState = ERavenlordGlobalState.Idle;
			}
			else
			{
				int num2 = _level.NextRandomInt(0, 2);
				if (num2 == _lastActionInteger)
				{
					num2 = (num2 + 1) % 3;
				}
				switch (num2)
				{
				case 0:
					_currentGlobalState = ERavenlordGlobalState.RammingLunge;
					break;
				case 1:
					_currentGlobalState = ERavenlordGlobalState.WindGusts;
					break;
				case 2:
					_currentGlobalState = ERavenlordGlobalState.GrappleLunge;
					break;
				}
				_lastActionInteger = num2;
			}
			if (_currentGlobalState == ERavenlordGlobalState.Idle)
			{
				vector.Normalize();
				if (num >= 65536f)
				{
					Vector2 vector2 = vector * 32f;
					_idleOffsetPoint = new Point((int)vector2.X, (int)vector2.Y);
				}
				else
				{
					Vector2 vector3 = vector * 128f;
					_idleOffsetPoint = new Point(-(int)vector3.X, -(int)vector3.Y);
				}
				ResetIdleTimer();
			}
		}
		else
		{
			_currentGlobalState = ERavenlordGlobalState.Teleport;
		}
		if (!_hasBeenIdleYet)
		{
			_currentGlobalState = ERavenlordGlobalState.Idle;
			_hasBeenIdleYet = true;
		}
		_isNeedingToTeleport = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isRunningDeathScript)
			{
				UpdatePassives(delta);
				UpdateRandomCroaking(delta);
			}
			UpdateEyeFlash(delta);
			UpdateParticleEmitters(delta);
			if (_isDrawingRamDangerArea)
			{
				_ramDangerAppendage.Update(delta);
			}
		}
		base.Update(delta);
	}

	private void UpdatePassives(float delta)
	{
		if (_currentGlobalState != 0 || _isCarryingOutAbility)
		{
			return;
		}
		_passiveSingleRavenTimer += delta;
		if (_ravenFlockProjectile == null || _ravenFlockProjectile.IsFinished)
		{
			_passiveRavenFlockTimer += delta;
		}
		if (_currentPhase != ERavenlordPhase.Phase3)
		{
			if (_passiveSingleRavenTimer >= 2f)
			{
				_passiveSingleRavenTimer -= 2f;
				EmitSingleRaven(isHyper: false);
			}
			if (_passiveRavenFlockTimer >= 5f)
			{
				_passiveRavenFlockTimer -= 5f;
				EmitRavenFlock(isHyper: false);
			}
		}
		else
		{
			if (_passiveSingleRavenTimer >= 1f)
			{
				_passiveSingleRavenTimer -= 1f;
				EmitSingleRaven(isHyper: true);
			}
			if (_passiveRavenFlockTimer >= 3f)
			{
				_passiveRavenFlockTimer -= 3f;
				EmitRavenFlock(isHyper: true);
			}
		}
	}

	private void UpdateRandomCroaking(float delta)
	{
		_croakTimer += delta;
		if (!(_croakTimer >= 0.5f))
		{
			return;
		}
		_croakIntervalTimer -= delta;
		if (_croakIntervalTimer <= 0f)
		{
			double num = _random.NextDouble();
			if (num <= 0.25)
			{
				PlayCue(ESFX.BossRavenCroak);
				_croakTimer = 0f;
				_croakIntervalTimer = 0f;
			}
			else
			{
				_croakIntervalTimer = 0.25f;
			}
		}
	}

	private void UpdateParticleEmitters(float delta)
	{
		if (!_isRunningDeathScript)
		{
			foreach (BoundingBoxParticleEmitter ravenParticleEmitter in _ravenParticleEmitters)
			{
				ravenParticleEmitter.Update(delta);
			}
		}
		foreach (BoundingBoxParticleEmitter smokeParticleEmitter in _smokeParticleEmitters)
		{
			smokeParticleEmitter.Update(delta);
		}
	}

	private void EmitSingleRaven(bool isHyper)
	{
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
		Point center = base.OuterBbox.Center;
		Vector2 iV = new Vector2(nearestProtagonistPosition.X - center.X, nearestProtagonistPosition.Y - center.Y);
		iV.Normalize();
		iV *= (isHyper ? 250f : 150f);
		if (_singleRavenProjectiles[_singleRavenCounter] == null)
		{
			_singleRavenProjectiles[_singleRavenCounter] = new RavenBossSingleProjectile(_level, center, iV, _sprite, _baseDamage);
		}
		RavenBossSingleProjectile ravenBossSingleProjectile = _singleRavenProjectiles[_singleRavenCounter];
		ravenBossSingleProjectile.Reset(center, iV);
		_level.AddProjectile(ravenBossSingleProjectile);
		_singleRavenCounter++;
		if (_singleRavenCounter >= 16)
		{
			_singleRavenCounter = 0;
		}
	}

	private void EmitRavenFlock(bool isHyper)
	{
		PlayCue(ESFX.BossRavenCroak);
		Point center = Bbox.Center;
		Protagonist nearestProtagonist = _level.GetNearestProtagonist(Position);
		if (_ravenFlockProjectile == null)
		{
			_ravenFlockProjectile = new RavenBossFlockProjectile(_level, center, _sprite, _baseDamage);
		}
		_ravenFlockProjectile.Reset(center, nearestProtagonist, this, isHyper);
		_level.AddProjectile(_ravenFlockProjectile);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_currentGlobalState)
		{
		case ERavenlordGlobalState.Idle:
			UpdateIdleAction(delta);
			break;
		case ERavenlordGlobalState.Teleport:
			UpdateTeleportAction();
			break;
		case ERavenlordGlobalState.RavenBarrage:
			UpdateRavenBarrageAction(delta);
			break;
		case ERavenlordGlobalState.WindGusts:
			UpdateWindGustsAction();
			break;
		case ERavenlordGlobalState.GrappleLunge:
			UpdateGrappleLungeAction();
			break;
		case ERavenlordGlobalState.RammingLunge:
			UpdateRammingLungeAction();
			break;
		}
		_lastGlobalStateTimer = _globalStateTimer;
		_globalStateTimer += delta;
	}

	private void ResetIdleTimer()
	{
		_currentMaximumIdleTime = 1f + (float)(_level.NextRandomDouble() * 1.0);
	}

	private void UpdateIdleAction(float delta)
	{
		if (_globalStateTimer >= _currentMaximumIdleTime)
		{
			StartEyeFlash();
			FinishAttack();
			return;
		}
		if (_globalStateTimer <= 0f)
		{
			SetCharacterSequence(_idleSequence);
		}
		GoToPoint(Position.Add(_idleOffsetPoint), delta);
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool result = base.CollideSolidTile(tile, depth);
		if (tile.Type != ETileType.Platform)
		{
			_isNeedingToTeleport = true;
		}
		return result;
	}

	private void UpdateTeleportAction()
	{
		if (_globalStateTimer <= 2f)
		{
			if (_globalStateTimer <= 0f)
			{
				SetCharacterSequence(_disappearSequence);
				_level.PlayCue(ESFX.BossRavenVanish, Position);
				_damageCaused = 0;
			}
			if (_globalStateTimer >= 1f && _lastGlobalStateTimer < 1f)
			{
				Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
				int num = ((nearestProtagonistPosition.X < 400) ? 1 : 0);
				int num2 = ((nearestProtagonistPosition.Y < 320) ? 1 : 0);
				Point position = new Point(200 + num * 400, 160 + num2 * 320);
				Position = position;
				SetCharacterSequence(_appearSequence);
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdateRavenBarrageAction(float delta)
	{
		if (_globalStateTimer <= 4.2f)
		{
			if (_globalStateTimer <= 0f)
			{
				SetCharacterSequence(_turretSequence);
				_ravenBarrageTimer = 0f;
			}
			_ravenBarrageTimer -= delta;
			if (_ravenBarrageTimer <= 0f)
			{
				_ravenBarrageTimer += 0.35f;
				EmitSingleRaven(isHyper: true);
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdateWindGustsAction()
	{
		if (_globalStateTimer <= 5f)
		{
			if (_globalStateTimer <= 0f)
			{
				SetCharacterSequence(_wallSequence);
				_windZone.Reset(Position, _lastPlayerPosition.X < Position.X);
				if (!_hasAddedWindzone)
				{
					_hasAddedWindzone = true;
					_level.RequestAddObject(_windZone);
				}
			}
		}
		else
		{
			_windZone.Deactivate();
			FinishAttack();
		}
	}

	private void UpdateGrappleLungeAction()
	{
		if (!_hasGrabbedPlayer)
		{
			if (_globalStateTimer <= 2.85f)
			{
				_isDrawingRamDangerArea = true;
				if (_globalStateTimer <= 0f)
				{
					_hasGrabbedPlayer = false;
					_throwStunScript = null;
					SetCharacterSequence(_wheelSequence);
					_ramDangerAppendage.ClearTrailHistory();
					_ramDangerAppendage.IsDrawingCircleTrail = true;
					_ramStartPosition = Position;
					Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
					_grappleVector = new Vector2(nearestProtagonistPosition.X - Position.X, nearestProtagonistPosition.Y - 24 - base.OuterBbox.Center.Y);
					_grappleVector.Normalize();
				}
				if (_globalStateTimer < 1.35f)
				{
					float num = _globalStateTimer / 1.35f;
					float num2 = (float)(Math.Sin(num * ((float)Math.PI / 2f)) * 32.0);
					Point point = (_grappleVector * (0f - num2)).ToPoint();
					Position = new Point(_ramStartPosition.X + point.X, _ramStartPosition.Y + point.Y);
					_ramDangerAppendage.ColorMultiplier = 1f;
				}
				else if (_globalStateTimer < 1.85f)
				{
					if (_lastGlobalStateTimer < 1.35f)
					{
						SetCharacterSequence(_turretSequence);
						_ramDangerAppendage.ColorMultiplier = 1f;
						_ramStartPosition = Position;
						Point nearestProtagonistPosition2 = _level.GetNearestProtagonistPosition(Position);
						_grappleVector = new Vector2(nearestProtagonistPosition2.X - Position.X, nearestProtagonistPosition2.Y - 24 - base.OuterBbox.Center.Y);
						_distanceToPlayer = _grappleVector.Length();
						_grappleVector.Normalize();
						if (_distanceToPlayer > 160f)
						{
							_distanceToPlayer = 160f;
						}
						_distanceToPlayer += 24f;
						_isThrowingPlayerToLeft = _grappleVector.X > 0f;
					}
					float num3 = (_globalStateTimer - 1.35f) / 0.5f;
					float num4 = (float)(Math.Sin(num3 * ((float)Math.PI / 2f)) * (double)_distanceToPlayer);
					Point point2 = (_grappleVector * num4).ToPoint();
					Position = new Point(_ramStartPosition.X + point2.X, _ramStartPosition.Y + point2.Y);
					if (!_hasGrabbedPlayer)
					{
						Protagonist mainHero = _level.MainHero;
						if (mainHero != null && mainHero.Bbox.Intersects(Bbox))
						{
							_hasGrabbedPlayer = true;
							_throwStunScript = new RavenBossThrowStunScript(_level, _baseDamage, this);
							mainHero.AddScriptAction(_throwStunScript);
						}
					}
				}
				else
				{
					if (_lastGlobalStateTimer < 1.85f)
					{
						_ramStartPosition = Position;
						if (!_hasGrabbedPlayer)
						{
							Protagonist mainHero2 = _level.MainHero;
							if (mainHero2 != null)
							{
								Point center = mainHero2.Bbox.Center;
								if (new Vector2(center.X - Position.X, center.Y - Position.Y).LengthSquared() <= 1024f)
								{
									PlayCue(ESFX.BossRavenCroak);
									_hasGrabbedPlayer = true;
									_throwStunScript = new RavenBossThrowStunScript(_level, _baseDamage, this);
									mainHero2.AddScriptAction(_throwStunScript);
								}
							}
						}
						if (!_hasGrabbedPlayer)
						{
							SetCharacterSequence(_idleSequence);
						}
					}
					if (!_hasGrabbedPlayer)
					{
						float num5 = (_globalStateTimer - 1.85f) / 1f;
						float num6 = (float)(Math.Sin(num5 * ((float)Math.PI / 2f)) * 32.0);
						Point point3 = (_grappleVector * (0f - num6)).ToPoint();
						Position = new Point(_ramStartPosition.X + point3.X, _ramStartPosition.Y + point3.Y);
						_ramDangerAppendage.ColorMultiplier = 1f - num5;
					}
				}
			}
			else
			{
				_isDrawingRamDangerArea = false;
				FinishAttack();
			}
			if (_hasGrabbedPlayer)
			{
				_globalStateTimer = 0f;
				_lastGlobalStateTimer = -0.033f;
				_ramStartPosition = Position;
			}
		}
		if (!_hasGrabbedPlayer)
		{
			return;
		}
		if (_globalStateTimer < 0.9f)
		{
			if (_globalStateTimer < 0.5f)
			{
				if (_globalStateTimer <= 0f && _lastGlobalStateTimer < 0f)
				{
					PlayCue2D(_isThrowingPlayerToLeft ? ESFX.BossRavenWindThrowLeft : ESFX.BossRavenWindThrowRight);
				}
				float num7 = _globalStateTimer / 0.5f;
				int num8 = (int)Math.Round(Math.Sin(num7 * ((float)Math.PI / 2f)) * 64.0) * (_isThrowingPlayerToLeft ? 1 : (-1));
				Position = new Point(_ramStartPosition.X + num8, _ramStartPosition.Y);
			}
			else if (_globalStateTimer < 0.65f)
			{
				if (_lastGlobalStateTimer < 0.5f)
				{
					_ramStartPosition = Position;
					if (_throwStunScript != null)
					{
						_throwStunScript.ThrowPlayer(_isThrowingPlayerToLeft);
					}
				}
				float num9 = (_globalStateTimer - 0.5f) / 0.15f;
				int num10 = (int)Math.Round((1.0 - Math.Cos(num9 * ((float)Math.PI / 2f))) * 128.0) * ((!_isThrowingPlayerToLeft) ? 1 : (-1));
				Position = new Point(_ramStartPosition.X + num10, _ramStartPosition.Y);
			}
			else
			{
				if (_lastGlobalStateTimer < 0.65f)
				{
					_ramStartPosition = Position;
					SetCharacterSequence(_idleSequence);
				}
				float num11 = (_globalStateTimer - 0.65f) / 0.25f;
				int num12 = (int)Math.Round(Math.Sin(num11 * ((float)Math.PI / 2f)) * 64.0) * (_isThrowingPlayerToLeft ? 1 : (-1));
				Position = new Point(_ramStartPosition.X + num12, _ramStartPosition.Y);
				_ramDangerAppendage.ColorMultiplier = 1f - num11;
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdateRammingLungeAction()
	{
		if (_globalStateTimer <= 2.1f)
		{
			_isDrawingRamDangerArea = true;
			if (_globalStateTimer <= 0f)
			{
				SetCharacterSequence(_ramSequence);
				_ramStartPosition = Position;
				_ramDangerAppendage.ClearTrailHistory();
				_ramDangerAppendage.IsDrawingCircleTrail = false;
				PlayCue(ESFX.BossRavenFlockBash);
			}
			if (_globalStateTimer < 1.25f)
			{
				float num = _globalStateTimer / 1.25f;
				int num2 = (int)Math.Ceiling(Math.Sin(num * ((float)Math.PI / 2f)) * 48.0);
				_ramDangerAppendage.ColorMultiplier = num;
				if (!IsFacingLeft)
				{
					num2 = -num2;
				}
				Position = new Point(_ramStartPosition.X + num2, _ramStartPosition.Y);
			}
			else if (_globalStateTimer < 1.6f)
			{
				if (_lastGlobalStateTimer < 1.25f)
				{
					_ramStartPosition = Position;
					_ramDangerAppendage.ColorMultiplier = 1f;
					_damageCaused = _baseDamage;
				}
				float num3 = (_globalStateTimer - 1.25f) / 0.35f;
				int num4 = (int)Math.Ceiling(Math.Sin(num3 * ((float)Math.PI / 2f)) * 128.0);
				if (IsFacingLeft)
				{
					num4 = -num4;
				}
				Position = new Point(_ramStartPosition.X + num4, _ramStartPosition.Y);
			}
			else
			{
				if (_lastGlobalStateTimer < 1.6f)
				{
					_ramStartPosition = Position;
					_damageCaused = 0;
				}
				float num5 = (_globalStateTimer - 1.6f) / 0.5f;
				int num6 = (int)Math.Ceiling(Math.Sin(num5 * ((float)Math.PI / 2f)) * 80.0);
				_ramDangerAppendage.ColorMultiplier = 1f - num5;
				if (!IsFacingLeft)
				{
					num6 = -num6;
				}
				Position = new Point(_ramStartPosition.X + num6, _ramStartPosition.Y);
			}
		}
		else
		{
			_isDrawingRamDangerArea = false;
			FinishAttack();
		}
	}

	private void FinishAttack()
	{
		FinishAttack(1f);
	}

	private void FinishAttack(float waitTime)
	{
		_nextActionTimer = waitTime;
		_currentAction = EAIAction.Idle;
		_hasGrabbedPlayer = false;
	}

	private void UpdateEyeFlash(float delta)
	{
		if (_eyeFlashTimer > 0f && _eyesAppendage != null)
		{
			_eyeFlashTimer -= delta;
			if (_eyeFlashTimer <= 0f)
			{
				_eyesAppendage.DoesDrawBaseSprite = false;
				_eyesAppendage.DoesInheritDrawColor = true;
				_eyesAppendage.DrawColor = Color.White;
				return;
			}
			_eyesAppendage.DoesDrawBaseSprite = true;
			_eyesAppendage.DoesInheritDrawColor = false;
			float num = 1f - _eyeFlashTimer / 0.6f;
			num = (float)Math.Sin(num * (float)Math.PI);
			_eyesAppendage.IsGlowing = false;
			_eyesAppendage.DrawColor = Color.Red * num;
		}
	}

	private void StartEyeFlash()
	{
		_eyeFlashTimer = 0.6f;
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (flag)
		{
			_impactFeatherParticles.AddParticles(where.ToVector2());
		}
		return flag;
	}

	protected override void StartDeathScript()
	{
		base.DeathPosition = base.OuterBbox.Center;
		GiveExperience();
		_isDrawingRamDangerArea = false;
		_level.RequestScreenFlash(new ScreenFlash(0.2f)
		{
			Frequency = 2f
		});
		_level.JukeBox.FadeOutSong(4f);
		_isFinallyDead = true;
		_isRunningDeathScript = true;
		_particleSystems.Add(_deathRavenParticles);
		RavenBossSingleProjectile[] singleRavenProjectiles = _singleRavenProjectiles;
		foreach (RavenBossSingleProjectile ravenBossSingleProjectile in singleRavenProjectiles)
		{
			if (ravenBossSingleProjectile != null && !ravenBossSingleProjectile.IsFinished)
			{
				ravenBossSingleProjectile.SilentKill();
			}
		}
		if (_ravenFlockProjectile != null)
		{
			_ravenFlockProjectile.SilentKill();
		}
		if (_windZone != null)
		{
			_windZone.Deactivate();
		}
		if (_throwStunScript != null)
		{
			_throwStunScript.Cancel();
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 4f)
		{
			if (_deathScriptTimer < 2f)
			{
				if (deathScriptTimer <= 0f)
				{
					if (_flockLoopCueInstance != null)
					{
						_flockLoopCueInstance.Stop(2f);
					}
					SetCharacterSequenceByName("Death");
					_level.PlayCue(ESFX.BossRavenDeath, Position);
					_deathParticleColorVect = _deathParticlesColor.ToVector4();
					base.DrawColor = Color.White;
					foreach (Appendage appendage in base.Appendages[0].Appendages)
					{
						appendage.DoesInheritDrawColor = true;
					}
				}
				if (_deathScriptTimer < 1.25f)
				{
					float delta2 = delta * 0.5f;
					foreach (BoundingBoxParticleEmitter ravenParticleEmitter in _ravenParticleEmitters)
					{
						ravenParticleEmitter.Update(delta2);
					}
				}
				foreach (BoundingBoxParticleEmitter deathRavenParticleEmitter in _deathRavenParticleEmitters)
				{
					deathRavenParticleEmitter.Update(delta);
				}
			}
			else if (deathScriptTimer < 2f)
			{
				_doesDrawSpriteAndAppendages = false;
				_doesDrawTrail = false;
				_doesDrawBrushTrail = false;
				base.DoesDrawAura = false;
				DropLoot();
			}
			ParticleSystem[] deathParticleSystems = _deathParticleSystems;
			for (int i = 0; i < deathParticleSystems.Length; i++)
			{
				deathParticleSystems[i]?.Update(delta);
			}
		}
		else
		{
			_level.SetLevelSaveBool("IsGyreBossDead", value: true);
			EndBossDeathScript();
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isDrawingRamDangerArea)
		{
			_ramDangerAppendage.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}

	internal override void InitializeForBestiary()
	{
		for (int i = 0; i < 20; i++)
		{
			Update(0.1f);
		}
		base.InitializeForBestiary();
	}
}
