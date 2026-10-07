using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Relics;

namespace Timespinner.GameObjects.Bosses.Varndagroth;

internal sealed class VarndagrothBoss : BossClass
{
	private enum ERotationState
	{
		None,
		SpinToSpear,
		SpinToGun,
		SpinningSlow,
		SpinningFast,
		SpinToFlames
	}

	private enum EVarndagrothAbility
	{
		FlameThrower = 0,
		MissileBarrage = 1,
		SpearChargeR = 2,
		SpearChargeL = 3,
		Wander = 10,
		Asleep = 11,
		WakingUp = 12,
		Awake = 13
	}

	private enum EEyelidAction
	{
		Close,
		Open,
		Blink
	}

	private const int MissileEmissionRadius = 64;

	private const int FlamethrowerEmissionRadius = 64;

	private const int EyeLeashLength = 6;

	private const int MaxFireProjectiles = 128;

	private const int SpindleOffsetX = 80;

	private const int SpindleOffsetY = 48;

	private const float EyeTransitionSpeed = 10f;

	private const float FlameSmokeTime = 0.75f;

	private const float FlameSpinTime = 3f;

	private const float MissileInterval = 0.75f;

	private const float MissileBarrageDuration = 4f;

	private const float RotationMultiplier = 5f;

	private const float SpearFollowTime = 1.5f;

	private const float SpearWindupTime = 0.35f;

	private const float SpearDashTime = 0.55f;

	private const float SpearSkidTime = 2f;

	private const float SpearDashSpeed = 35000f;

	private const float TurretRotationSetting = 0f;

	private const float SpearRotationSetting = 3.9269907f;

	private const float FlamesRotationSetting = (float)Math.PI / 2f;

	private const float DeathSpinningRotationSpeed = 7f;

	private const float TimeForDeathExplosions = 0.05f;

	private const float ScreenShakeTime = 2.185f;

	private const float RoomShakeTime = 0.75f;

	private const float RoomShakeFrequency = 20f;

	private const float TimeToWakeUp = 1f;

	private static readonly Vector2 RoomShakeDimensions = new Vector2(8f, 0f);

	private static readonly Color SleepingColor = new Color(0.3f, 0.3f, 0.3f, 1f);

	private readonly int _baseDamageCaused;

	private readonly Appendage _eyeAppendage;

	private readonly Appendage _eyelidAppenage;

	private readonly SFXCueInstance _pulseCueInstance;

	private readonly TimespinnerSpindleItem _spindleItem;

	private readonly VarndagrothFireProjectile[] _fireProjectiles = new VarndagrothFireProjectile[128];

	private bool _isAtTargetRotation;

	private bool _hasShakenScreen;

	private bool _isDying;

	private ERotationState _rotateState;

	private EVarndagrothAbility _currentVarndagrothAction;

	private EVarndagrothAbility _lastNonFollowAction;

	private int _flamesEmitted;

	private float _rotationTimer;

	private float _missileTimer;

	private float _deathExplosionTimer;

	private float _deathRotationSpeed;

	private float _wakeUpTimer;

	private Point _eyeTargetPosition;

	private Vector2 _currentEyeOffset;

	private Vector2 _targetEyeOffset;

	private SFXCueInstance _flamethrowerLoopCueInstance;

	public override Point AnchorPosition => Position;

	public VarndagrothBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_agility = 2f;
		_bboxOffset = new Point(20, 20);
		Bbox = new Rectangle(_position.X, _position.Y, 88, 88);
		DrawOrigin = new Vector2(64f, 64f);
		Position = new Point(Position.X, Position.Y - 2);
		_isIgnoringPlatform = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_maxMoveSpeed = 800f;
		base.GoToOscillationMultiplier = 50f;
		ChangeAnimation(0, 3, 0.24f, EAnimationType.Cycle);
		_currentAI = EAIStrategy.CustomScriptAI;
		_maxUpdateTargetTime = 0.1f;
		_timeToWaitAfterArriving = 2f;
		_baseDamageCaused = _damageCaused;
		_deathParticlesColor = new Color(0.2f, 0.75f, 0.2f, 1f);
		_doesUseAppendageCollision = false;
		_eyeAppendage = new Appendage(this, new Rectangle(0, 0, 50, 50), new Point(7, 7), _level, _sprite)
		{
			FollowType = EAppendageFollowType.ParentObjectLocked
		};
		_eyeAppendage.ChangeAnimation(3, 0, 0.1f, EAnimationType.None);
		_appendages.Add(_eyeAppendage);
		_eyelidAppenage = new Appendage(this, new Rectangle(0, 0, 32, 48), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.ParentObjectLocked,
			DrawPriority = 2,
			DrawOrigin = new Vector2(16f, 22f),
			AnchorOffset = new Point(0, -18)
		};
		_eyelidAppenage.ChangeAnimation(6, 0, 0.1f, EAnimationType.None);
		_appendages.Add(_eyelidAppenage);
		_destinationNodes = new Point[5];
		ref Point reference = ref _destinationNodes[0];
		reference = new Point(200, 168);
		ref Point reference2 = ref _destinationNodes[1];
		reference2 = new Point(96, 192);
		ref Point reference3 = ref _destinationNodes[2];
		reference3 = new Point(96, 80);
		ref Point reference4 = ref _destinationNodes[3];
		reference4 = new Point(312, 80);
		ref Point reference5 = ref _destinationNodes[4];
		reference5 = new Point(312, 192);
		_pulseCueInstance = PlayCue(ESFX.BossEyePulse, isLooped: true);
		_spindleItem = new TimespinnerSpindleItem(_level, Position.Add(80, 48), -1, new ObjectTileSpecification(), _sprite, OnSpindlePickedUp);
		_level.RequestAddObject(_spindleItem);
	}

	public override void InitializeMob()
	{
		_level.OpenAllBossDoors(-1f);
		base.IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected override void StartBossIntroCutscene()
	{
		base.IsBossIntroInProgress = false;
		_currentVarndagrothAction = EVarndagrothAbility.Asleep;
		_eyelidAppenage.ChangeAnimation(10);
		_damageCaused = 0;
		base.IsSolidWhenFrozen = false;
		_canBeDamaged = false;
		base.DrawColor = SleepingColor;
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateLookingEye(delta);
			UpdateRotation(delta);
			if (_currentVarndagrothAction == EVarndagrothAbility.WakingUp)
			{
				UpdateWakingUp(delta);
			}
			else if (_isAtTargetLocation && _isAtTargetRotation && !_isCarryingOutAbility)
			{
				switch (_currentVarndagrothAction)
				{
				case EVarndagrothAbility.FlameThrower:
					_rotateState = ERotationState.SpinningFast;
					StartAbility(0);
					break;
				case EVarndagrothAbility.MissileBarrage:
					_rotateState = ERotationState.SpinningFast;
					StartAbility(1);
					break;
				case EVarndagrothAbility.SpearChargeR:
					_rotateState = ERotationState.None;
					StartAbility(2);
					break;
				case EVarndagrothAbility.SpearChargeL:
					_rotateState = ERotationState.None;
					StartAbility(3);
					break;
				}
			}
		}
		base.Update(delta);
		IsFacingLeft = true;
	}

	public override void PostCollisionUpdate()
	{
		base.PostCollisionUpdate();
		Update(0f);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_currentVarndagrothAction == EVarndagrothAbility.Asleep || _currentVarndagrothAction == EVarndagrothAbility.WakingUp)
		{
			_currentAction = EAIAction.Idle;
			_nextActionTimer = 1f;
		}
		else if (_currentAction == EAIAction.FloatInPlace)
		{
			_startPosition = _position;
			_followTimer = 0f;
			_isAtTargetRotation = false;
			_isAtTargetLocation = false;
			EVarndagrothAbility eVarndagrothAbility = EVarndagrothAbility.Wander;
			if (_currentVarndagrothAction == EVarndagrothAbility.Wander || _currentVarndagrothAction == EVarndagrothAbility.SpearChargeL || _currentVarndagrothAction == EVarndagrothAbility.SpearChargeR)
			{
				eVarndagrothAbility = (EVarndagrothAbility)_random.Next(0, 3);
				if (eVarndagrothAbility == _lastNonFollowAction)
				{
					eVarndagrothAbility = (EVarndagrothAbility)((int)(eVarndagrothAbility + 1) % 4);
				}
				_lastNonFollowAction = eVarndagrothAbility;
				ChangeEyelidAnimation(EEyelidAction.Blink);
			}
			_currentAction = EAIAction.GoTowards;
			switch (eVarndagrothAbility)
			{
			case EVarndagrothAbility.FlameThrower:
				_rotateState = ERotationState.SpinToFlames;
				_targetPosition = _destinationNodes[0];
				_agility = 0.4f;
				_nextActionTimer = 100f;
				_rotationTimer = 100f;
				break;
			case EVarndagrothAbility.MissileBarrage:
				_rotateState = ERotationState.SpinToGun;
				_targetPosition = _destinationNodes[0];
				_nextActionTimer = 100f;
				_agility = 0.4f;
				_rotationTimer = 100f;
				PlayCue(ESFX.BossEyeSpin, Position);
				break;
			case EVarndagrothAbility.SpearChargeR:
			{
				bool flag = _position.X < _eyeTargetPosition.X;
				_rotateState = ERotationState.SpinToSpear;
				_targetPosition = (flag ? _destinationNodes[1] : _destinationNodes[4]);
				if (flag)
				{
					eVarndagrothAbility = EVarndagrothAbility.SpearChargeL;
				}
				_agility = 0.4f;
				_nextActionTimer = 210f;
				_rotationTimer = 250f;
				PlayCue(ESFX.BossEyeSpin, Position);
				break;
			}
			case EVarndagrothAbility.Wander:
				_rotateState = ERotationState.SpinningSlow;
				_targetPosition = _eyeTargetPosition;
				_isAtTargetRotation = true;
				_isAtTargetLocation = true;
				_nextActionTimer = 3f;
				_agility = 0.2f;
				_rotationTimer = 100f;
				break;
			}
			_currentVarndagrothAction = eVarndagrothAbility;
			_totalActionTimer = _nextActionTimer;
		}
		else
		{
			_currentAction = EAIAction.FloatInPlace;
			_nextActionTimer = 1f;
		}
		base.PickNextCustomScriptAIAction(delta);
	}

	public override void UpdateAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			if (_abilityTimer < 3f)
			{
				if (_abilityTimer <= 0f)
				{
					PlayCue(ESFX.BossEyeFlamethrowerStart, Position);
					if (_flamethrowerLoopCueInstance == null)
					{
						_flamethrowerLoopCueInstance = PlayCue(ESFX.BossEyeFlamethrower, isLooped: true);
					}
					else
					{
						_flamethrowerLoopCueInstance.Resume();
					}
				}
				bool isSmoke = _abilityTimer < 0.75f;
				float rotation = base.Rotation;
				Point center = _bbox.Center;
				Vector2 value = new Vector2((float)Math.Cos(0f - rotation), 0f - (float)Math.Sin(0f - rotation));
				center.X += (int)(value.X * 64f);
				center.Y += (int)(value.Y * 64f);
				value = Vector2.Multiply(value, 350f);
				EmitFlame(center, value, isSmoke);
				rotation += (float)Math.PI;
				value = new Vector2((float)Math.Cos(0f - rotation), 0f - (float)Math.Sin(0f - rotation));
				center = _bbox.Center;
				center.X += (int)(value.X * 64f);
				center.Y += (int)(value.Y * 64f);
				value = Vector2.Multiply(value, 350f);
				EmitFlame(center, value, isSmoke);
			}
			else
			{
				_isCarryingOutAbility = false;
				_nextActionTimer = 0f;
				_currentAction = EAIAction.FloatInPlace;
				PlayCue(ESFX.BossEyeFlamethrowerEnd, Position);
				if (_flamethrowerLoopCueInstance != null)
				{
					_flamethrowerLoopCueInstance.Pause();
				}
			}
			break;
		case 1:
			_missileTimer += delta;
			if (_missileTimer > 0.75f || _abilityTimer == 0f)
			{
				_missileTimer = 0f;
				ShootMissile((float)Math.PI / 2f);
				if (_level.IsHardMode)
				{
					ShootMissile(4.712389f);
				}
			}
			if (_abilityTimer > 4f)
			{
				_isCarryingOutAbility = false;
				_nextActionTimer = 0f;
				_currentAction = EAIAction.FloatInPlace;
			}
			break;
		case 2:
			UpdateSpearCharge(delta, isOnRight: true);
			break;
		case 3:
			UpdateSpearCharge(delta, isOnRight: false);
			break;
		}
	}

	private void ShootMissile(float extraRotation)
	{
		float rotation = base.Rotation;
		Point center = _bbox.Center;
		rotation += extraRotation;
		Vector2 value = new Vector2((float)Math.Cos(0f - rotation), 0f - (float)Math.Sin(0f - rotation));
		center.X += (int)(value.X * 64f);
		center.Y += (int)(value.Y * 64f);
		value = Vector2.Multiply(value, 200f);
		_level.AddAnimation(new BattleAnimation(_sprite, center, _level)
		{
			TeamSide = ETeamSide.Enemies,
			AnimationSpeed = 0.035f,
			AnimationStart = 21,
			AnimationLength = 4
		});
		_level.AddProjectile(new VarndagrothMissileProjectile(_level, center, value, ETeamSide.Enemies, _eyeTargetPosition, _sprite, base.Damage));
		PlayCue(ESFX.BossEyeShoot, center);
	}

	private void EmitFlame(Point startPoint, Vector2 iV, bool isSmoke)
	{
		VarndagrothFireProjectile varndagrothFireProjectile = null;
		if (_flamesEmitted < 128)
		{
			varndagrothFireProjectile = new VarndagrothFireProjectile(_level, startPoint, iV, base.DefaultTeam, _sprite, isSmoke, base.Damage);
			_fireProjectiles[_flamesEmitted] = varndagrothFireProjectile;
			_flamesEmitted++;
		}
		else
		{
			VarndagrothFireProjectile[] fireProjectiles = _fireProjectiles;
			foreach (VarndagrothFireProjectile varndagrothFireProjectile2 in fireProjectiles)
			{
				if (varndagrothFireProjectile2.IsFinished)
				{
					varndagrothFireProjectile2.Reset(startPoint, iV, isSmoke);
					varndagrothFireProjectile = varndagrothFireProjectile2;
					break;
				}
			}
		}
		if (varndagrothFireProjectile != null)
		{
			_level.AddProjectile(varndagrothFireProjectile);
		}
	}

	private void UpdateSpearCharge(float delta, bool isOnRight)
	{
		if (_abilityTimer <= 0f)
		{
			_eyeAppendage.ChangeAnimation(3, 3, 0.1f, EAnimationType.Once);
			PlayCue(ESFX.BossEyeMatch, Position);
		}
		if (_abilityTimer < 1.5f)
		{
			int num = _position.Y - _eyeTargetPosition.Y - 16;
			num = ((num > 10) ? 10 : num);
			num = ((num < -10) ? (-10) : num);
			_velocity.X = 0f;
			_velocity.Y += (float)(num * -400) * delta;
		}
		else if (_abilityTimer < 1.85f)
		{
			if (_lastAbilityTimer < 1.5f)
			{
				PlayCue(ESFX.BossEyeRam, Position);
			}
			_velocity.Y = 0f;
			_velocity.X += 2000f * delta * (float)(isOnRight ? 1 : (-1));
			_hasShakenScreen = false;
		}
		else if (_abilityTimer < 2.4f)
		{
			if (!_doesDrawTrail)
			{
				_doesDrawTrail = true;
				_trailFadeRate = 2f;
				_trailLength = 16;
				ClearTrailHistory();
				ChangeEyelidAnimation(EEyelidAction.Close);
			}
			_velocity.Y = 0f;
			_velocity.X += -35000f * delta * (float)(isOnRight ? 1 : (-1));
			if (_abilityTimer > 2.185f && !_hasShakenScreen)
			{
				_level.RequestScreenShake(RoomShakeDimensions, 0.75f, 20f, isAffectedByTime: true);
				int x = (isOnRight ? 32 : 368);
				Point position = new Point(x, _bbox.Center.Y);
				Point position2 = new Point(x, _bbox.Top + 8);
				Point position3 = new Point(x, _bbox.Bottom - 8);
				_level.AddAnimation(EBattleAnimationType.CrackingDust, position, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.CrackingDust, position2, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.CrackingDust, position3, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.Pebbles, position, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.Pebbles, position2, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.Pebbles, position3, ETeamSide.Enemies);
				_hasShakenScreen = true;
				PlayCue(ESFX.BossEyeRamCrash, Position);
			}
		}
		else if (_abilityTimer > 4.4f)
		{
			_isCarryingOutAbility = false;
			_nextActionTimer = 0f;
			_currentAction = EAIAction.FloatInPlace;
			_doesDrawTrail = false;
			_eyeAppendage.ChangeAnimation(new AnimationSpec
			{
				Start = 3,
				Length = 3,
				Speed = 0.1f,
				IsInReverse = true
			});
			ChangeEyelidAnimation(EEyelidAction.Open);
		}
	}

	private void UpdateRotation(float delta)
	{
		if (!_isDying)
		{
			if (_rotationTimer > 0f)
			{
				_rotationTimer -= delta;
				if (_rotateState == ERotationState.SpinningFast)
				{
					base.Rotation += delta * 5f / 3f;
				}
				else if (_rotateState == ERotationState.SpinningSlow)
				{
					base.Rotation -= delta * 5f / 2f;
				}
				else if (_rotateState == ERotationState.SpinToGun)
				{
					SpinToTargetRotation(0f, delta);
				}
				else if (_rotateState == ERotationState.SpinToSpear)
				{
					SpinToTargetRotation(3.9269907f, delta);
				}
				else if (_rotateState == ERotationState.SpinToFlames)
				{
					SpinToTargetRotation((float)Math.PI / 2f, delta);
				}
			}
		}
		else
		{
			base.Rotation += delta * _deathRotationSpeed;
		}
		if (base.Rotation > (float)Math.PI * 4f)
		{
			base.Rotation -= (float)Math.PI * 2f;
		}
		else if (base.Rotation < (float)Math.PI * -4f)
		{
			base.Rotation += (float)Math.PI * 2f;
		}
		_eyelidAppenage.Rotation = base.Rotation;
	}

	private void SpinToTargetRotation(float targetRotation, float delta)
	{
		float num = Math.Abs(base.Rotation - targetRotation) / 6.14f * 50f;
		if (num > 10f)
		{
			num = 10f;
		}
		if (base.Rotation > targetRotation)
		{
			base.Rotation -= delta * num;
			if (base.Rotation < targetRotation || MathEx.IsFloatBelowOne(num))
			{
				base.Rotation = targetRotation;
				_isAtTargetRotation = true;
			}
		}
		else if (base.Rotation < targetRotation)
		{
			base.Rotation += delta * num;
			if (base.Rotation > targetRotation || MathEx.IsFloatBelowOne(num))
			{
				base.Rotation = targetRotation;
				_isAtTargetRotation = true;
			}
		}
	}

	private void UpdateLookingEye(float delta)
	{
		_updateTargetTimer -= delta;
		if (_updateTargetTimer <= 0f && !_isDying)
		{
			_updateTargetTimer = _maxUpdateTargetTime;
			_eyeTargetPosition = _level.GetNearestProtagonistPosition(_position);
		}
		Vector2 value = new Vector2(_position.X - _eyeTargetPosition.X, _position.Y - _eyeTargetPosition.Y - 32);
		value.Normalize();
		_targetEyeOffset = Vector2.Multiply(value, 6f);
		_currentEyeOffset = Vector2.SmoothStep(_currentEyeOffset, _targetEyeOffset, delta * 10f);
		_eyeAppendage.AnchorOffset = new Point(-(int)_currentEyeOffset.X, -(int)_currentEyeOffset.Y - 21);
	}

	private void ChangeEyelidAnimation(EEyelidAction type)
	{
		switch (type)
		{
		case EEyelidAction.Close:
			_eyelidAppenage.ChangeAnimation(6, 5, 0.066f, EAnimationType.Once);
			PlayCue(ESFX.BossEyeClose, Position);
			break;
		case EEyelidAction.Open:
			_eyelidAppenage.ChangeAnimation(new AnimationSpec
			{
				Start = 6,
				Length = 5,
				Speed = 0.06f,
				IsInReverse = true
			});
			PlayCue(ESFX.BossEyeOpen, Position);
			break;
		case EEyelidAction.Blink:
			_eyelidAppenage.ChangeAnimation(new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 6,
					Length = 5,
					Speed = 0.03f
				},
				new AnimationSpec
				{
					Start = 6,
					Length = 5,
					Speed = 0.03f,
					IsInReverse = true
				}
			});
			PlayCue(ESFX.BossEyeClose, Position);
			break;
		}
	}

	protected override void StartDeathScript()
	{
		_movementX = 0f;
		if (_pulseCueInstance != null)
		{
			_pulseCueInstance.Stop();
		}
		_level.PlayCue(ESFX.BossDeathFinalHit);
		base.StartDeathScript();
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		Vector2 target = new Vector2(_bbox.Center.X, _bbox.Center.Y);
		if (deathScriptTimer <= 0f)
		{
			_isDying = true;
			_deathRotationSpeed = 7f;
			_eyeTargetPosition = target.ToPoint();
			_eyeAppendage.ChangeAnimation(3, 3, 0.1f, EAnimationType.Once);
			ChangeEyelidAnimation(EEyelidAction.Blink);
			_deathParticleColorVect = _deathParticlesColor.ToVector4();
			_deathScriptTimer = 1E-06f;
		}
		else if (_deathScriptTimer < 2f)
		{
			_deathExplosionTimer += delta;
			if (_deathExplosionTimer > 0.05f)
			{
				_deathExplosionTimer -= 0.05f;
				int num = _random.Next(base.OuterBbox.Left, base.OuterBbox.Right);
				int y = _random.Next(base.OuterBbox.Top, base.OuterBbox.Bottom);
				Point position = new Point(num, y);
				_level.AddAnimation(EBattleAnimationType.Boom, position, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
				PlayCue((num % 2 == 0) ? ESFX.EnemyWormFlowerSeedBoom : ESFX.FoleyExplosionLarge, position, isLooped: false, 0.5f);
			}
			if (_deathScriptTimer + 0.5f >= 2f && deathScriptTimer + 0.5f < 2f)
			{
				ChangeEyelidAnimation(EEyelidAction.Close);
				_level.RequestScreenFlash(1f, 1f, 1f);
			}
		}
		else if (_deathScriptTimer >= 2f && deathScriptTimer <= 2f)
		{
			_level.AddAnimation(EBattleAnimationType.Boom, Bbox.Center, ETeamSide.Enemies);
			foreach (Appendage appendage in _appendages)
			{
				_level.AddAnimation(EBattleAnimationType.Boom, appendage.Position, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
			}
			_appendages.Clear();
			ChangeAnimation(11, 1, 1f, EAnimationType.None);
			_isAffectedByGravity = true;
			base.DoesCollideWithTiles = false;
			base.DoesCollideWithSolidEvents = false;
			_gravityAcceleration = 500f;
			_maxFallSpeed = 1000f;
			_deathRotationSpeed = 0f;
			base.Rotation = 0f;
			_deathScriptTimer = 2.000001f;
			_doesDrawTrail = false;
			_doesDrawBrushTrail = false;
			base.DoesDrawAura = false;
			DropLoot();
		}
		else if (!(_deathScriptTimer < 4f) && _deathScriptTimer >= 4f)
		{
			EndBossDeathScript();
		}
	}

	private void OnSpindlePickedUp()
	{
		_level.ToggleExits(isEnabled: false);
		_level.LockAllBossDoors(0.1f);
		_level.JukeBox.FadeOutSong(1f);
		_level.UnlockRelic(EInventoryRelicType.TimespinnerSpindle);
		ChangeEyelidAnimation(EEyelidAction.Open);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		AddLevelScriptAction(new ScriptAction(EInventoryRelicType.TimespinnerSpindle));
		AddWaitScript(0.05f);
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = StartBattle
		});
	}

	private void StartBattle()
	{
		_currentVarndagrothAction = EVarndagrothAbility.WakingUp;
		base.IsDormant = false;
	}

	private void UpdateWakingUp(float delta)
	{
		if (_wakeUpTimer <= 0f && !_level.JukeBox.DoesNotPlaySounds)
		{
			BossClass.PlaySongByBossType(_level.JukeBox, base.BossType);
		}
		_wakeUpTimer += delta;
		if (_wakeUpTimer >= 1f)
		{
			base.DrawColor = Color.White;
			_level.HasPlayerBeenDamagedInThisRoom = false;
			_level.HasPlayerFrozenTimeInThisRoom = false;
			_canBeDamaged = true;
			base.IsSolidWhenFrozen = true;
			_damageCaused = _baseDamageCaused;
			_currentVarndagrothAction = EVarndagrothAbility.Awake;
			PickNextCustomScriptAIAction(delta);
		}
		else
		{
			float amount = _wakeUpTimer / 1f;
			base.DrawColor = SleepingColor.SineInterpolate(Color.White, amount);
		}
	}

	internal override void InitializeForBestiary()
	{
		base.DrawColor = Color.White;
		_eyelidAppenage.ChangeAnimation(6);
	}
}
