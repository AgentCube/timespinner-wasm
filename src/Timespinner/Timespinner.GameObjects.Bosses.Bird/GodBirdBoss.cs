using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Bird;

internal sealed class GodBirdBoss : BossClass
{
	private enum EGodBirdGlobalState
	{
		Idle,
		Growl,
		SpitAttack,
		FireballAttack,
		TelekinesisAttack,
		SummonBalls,
		AttackWithBalls,
		WallSlamAttack
	}

	private const int MaxGoopBallCount = 3;

	private const int MaxAuraBallCount = 3;

	private const int AuraDangerZoneCount = 3;

	private const int AuraDangerZoneOffsetX = 105;

	private const int AuraDangerZoneBufferX = 21;

	private const int MinSpitTargetX = 192;

	private const int SpitOriginOffsetX = -3;

	private const int SpitOriginOffsetY = 2;

	private const int AuraBallVelocity = 350;

	private const float FrenzyThreshold = 0.65f;

	private const float IndefiniteActionTime = 1000f;

	private const float TimeForSpitWindup = 1f;

	private const float TimeForSpitWait = 0.2f;

	private const float TimeForSpitSwipe = 0.4f;

	private const float TimeForSpitAttackRetract = 0.25f;

	private const float TimeForEntireSpitAttack = 1.85f;

	private const float TimeForFireballWindup = 1f;

	private const float TimeForFireballWait = 0.2f;

	private const float TimeForFireballSwipe = 0.4f;

	private const float TimeForFireballAttackRetract = 0.25f;

	private const float TimeForEntireFireballAttack = 1.85f;

	private const float TimeBeforeStartingGrowlSequence = 1.75f;

	private const float TimeForGrowl = 4.25f;

	private const float TimeDelayForNearBallSummon = 0.07f;

	private const float TimeForEntireSummonBallSequence = 2f;

	private const int SlamCount = 5;

	private const float TimeForOneBallToSlam = 1.65f;

	private const float SlamTimeOffsetBetweenBalls = 0.4125f;

	private const float TimeForEntireBallSlamSpam = 9.9f;

	private const float TimeBeforeTelekinesisThrowing = 0.5f;

	private const float TimeForEntireTelekinesisAttack = 2.5f;

	private const float TimeForWallSlamAttack = 1f;

	private const int DeathParticlesOffsetX = 24;

	private const int DeathParticlesOffsetY = -64;

	private const float TimeForDeathChargeUp = 2f;

	private const float TimeBeforeDeathExplosionFlash = 1.5f;

	private const float TimeForDeathExplosion = 2f;

	private const float TimeForEntireDeathSequence = 4f;

	private const float GlowFrequency = 5f;

	private const float TimeToTransitionAuraStates = 0.5f;

	private static readonly Color BirdAuraColor = new Color(0.25f, 0.25f, 0.75f, 0.25f);

	private static readonly Color DeathGlowColor = new Color(1f, 1f, 1f, 0.5f);

	private static readonly Vector4 BaseBirdGlowColor = new Vector4(0.9f, 0.95f, 1f, 1f);

	private readonly BirdBossSpitParticleSystem _spitParticleSystem;

	private readonly BirdBossDeathChargeLazerPS _deathLazerParticles;

	private readonly BirdBossLazerChargeParticleSystem _lazerChargeParticles;

	private readonly BirdBossWallPebblesParticleSystem _pebbleParticles;

	private readonly Appendage _headAppendage;

	private readonly GodBirdBossBallAndChain _nearBallAndChain;

	private readonly GodBirdBossBallAndChain _farBallAndChain;

	private readonly GodBirdBossBallAndChain _rearBallAndChain;

	private readonly CharacterSequenceSpecification _auraStartSequence;

	private readonly CharacterSequenceSpecification _auraEndSequence;

	private readonly CharacterSequenceSpecification _spitSequence;

	private readonly CharacterSequenceSpecification _hairIdleSequence;

	private readonly CharacterSequenceSpecification _roarSequence;

	private readonly CharacterSequenceSpecification _sneezeSequence;

	private readonly GodBirdGoopBall[] _goopBalls = new GodBirdGoopBall[3];

	private readonly GodBirdAuraProjectile[] _auraBalls = new GodBirdAuraProjectile[3];

	private readonly GodBirdAuraDangerZone[] _auraDangerZones = new GodBirdAuraDangerZone[3];

	private bool _doesHaveLowHealth;

	private bool _hasGrowled;

	private bool _isAuraGlowing;

	private bool _hasUsedBallAttack;

	private EGodBirdGlobalState _globalState;

	private int _goopBallCounter;

	private int _auraBallCounter;

	private int _nearBallSlamCount;

	private int _farBallSlamCount;

	private float _globalStateTimer;

	private float _lastGlobalStateTimer;

	private float _glowTimer;

	private float _auraTransitionTimer;

	private float _farBallSlamTimer;

	private float _nearBallSlamTimer;

	private Point _lastPlayerPosition;

	public override Point AnchorPosition => new Point(base.LastPosition.X - Bbox.Width * (IsFacingLeft ? 1 : (-1)) / 2, base.LastPosition.Y - Bbox.Height / 2);

	public GodBirdBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_globalState = EGodBirdGlobalState.Idle;
		_agility = 1f;
		Bbox = new Rectangle(_position.X, _position.Y, 54, 54);
		base.CannotBeGrabbed = true;
		IsFacingLeft = objectSpec != null && !objectSpec.IsFlippedHorizontally;
		ChangeAnimation(-1);
		_doAppendagesMatchImageFacing = true;
		_doesDrawBaseSprite = false;
		_timeToTurnAround = 0f;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.DoesDrawAura = false;
		base.AuraColor = BirdAuraColor;
		base.AuraSize = 0.05f;
		base.DoesDrawAppendageAuras = true;
		base.AuraOffset = new Vector2(1f, 2f);
		base.AuraFrequency = 9f;
		_auraCount = 5f;
		if (base.CharacterSpecification != null)
		{
			_headAppendage = _appendages[0].Appendages[1].Appendages[0];
			_headAppendage.DoesDrawAppendagesInReverse = true;
			SetCharacterSequenceByName("Sleep");
			_auraStartSequence = GetCharacterSequenceByName("StartAura");
			_auraEndSequence = GetCharacterSequenceByName("EndAura");
			_spitSequence = GetCharacterSequenceByName("Spit");
			_hairIdleSequence = GetCharacterSequenceByName("HairIdle");
			_roarSequence = GetCharacterSequenceByName("Roar");
			_sneezeSequence = GetCharacterSequenceByName("Sneeze");
		}
		ObjectTileSpecification objectSpec2 = new ObjectTileSpecification();
		Appendage appendage = _appendages[0].Appendages[2];
		int damageCaused = _damageCaused;
		_farBallAndChain = new GodBirdBossBallAndChain(_level, _sprite, appendage.Appendages[0], Position, -1, objectSpec2, isFrontPlane: false, isRearBall: false, damageCaused, IsFacingLeft);
		_nearBallAndChain = new GodBirdBossBallAndChain(_level, _sprite, appendage.Appendages[1], Position, -1, objectSpec2, isFrontPlane: true, isRearBall: false, damageCaused, IsFacingLeft);
		_rearBallAndChain = new GodBirdBossBallAndChain(_level, _sprite, appendage.Appendages[2], Position, -1, objectSpec2, isFrontPlane: true, isRearBall: true, damageCaused, IsFacingLeft);
		_level.RequestAddObject(_farBallAndChain);
		_level.RequestAddObject(_nearBallAndChain);
		_level.RequestAddObject(_rearBallAndChain);
		_spitParticleSystem = new BirdBossSpitParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_particleSystems.Add(_spitParticleSystem);
		_lazerChargeParticles = new BirdBossLazerChargeParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_particleSystems.Add(_lazerChargeParticles);
		_deathLazerParticles = new BirdBossDeathChargeLazerPS(_level.GCM.TxParticleEnergy, 5);
		_pebbleParticles = new BirdBossWallPebblesParticleSystem(_level.GCM.TxParticleEnergy, 1)
		{
			IsFacingRight = false
		};
		int num = Position.X + 105;
		for (int i = 0; i < 3; i++)
		{
			GodBirdAuraDangerZone godBirdAuraDangerZone = new GodBirdAuraDangerZone(_level, new Point(num, Position.Y), objectSpec2, _sprite, base.Damage, _pebbleParticles);
			_auraDangerZones[i] = godBirdAuraDangerZone;
			num += 69;
			_level.RequestAddObject(godBirdAuraDangerZone);
		}
	}

	public override void InitializeMob()
	{
		if (!IsFacingLeft)
		{
			_headAppendage.OscillDelta += (float)Math.PI / 2f;
		}
		base.InitializeMob();
	}

	protected override void EndBossIntroCutscene()
	{
		BossClass.PlaySongByBossType(_level.JukeBox, base.BossType);
		base.IsBossIntroInProgress = false;
		base.IsDormant = false;
		_level.HasPlayerBeenDamagedInThisRoom = false;
		_level.HasPlayerFrozenTimeInThisRoom = false;
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			UpdateGlowing(delta);
		}
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_globalStateTimer = 0f;
		_lastGlobalStateTimer = -1E-07f;
		_lastPlayerPosition = _level.GetNearestProtagonistPosition(_headAppendage.Position);
		bool flag = _lastPlayerPosition.X < Position.X != IsFacingLeft;
		_nextActionTimer = 1000f;
		if (!_hasGrowled)
		{
			_globalState = EGodBirdGlobalState.Growl;
			_hasGrowled = true;
			return;
		}
		if (flag)
		{
			_globalState = EGodBirdGlobalState.WallSlamAttack;
			return;
		}
		bool flag2 = (float)base.HP <= 0.65f * (float)base.MaxHP;
		bool flag3 = flag2 && !_doesHaveLowHealth;
		EGodBirdGlobalState globalState = _globalState;
		_doesHaveLowHealth = flag2;
		EGodBirdGlobalState globalState2 = EGodBirdGlobalState.SpitAttack;
		if (!_doesHaveLowHealth)
		{
			switch (_level.NextRandomInt(0, 2))
			{
			case 0:
				_globalState = EGodBirdGlobalState.SpitAttack;
				globalState2 = EGodBirdGlobalState.FireballAttack;
				break;
			case 1:
				_globalState = EGodBirdGlobalState.FireballAttack;
				globalState2 = EGodBirdGlobalState.TelekinesisAttack;
				break;
			case 2:
				_globalState = EGodBirdGlobalState.TelekinesisAttack;
				globalState2 = EGodBirdGlobalState.SpitAttack;
				break;
			}
		}
		else if (flag3)
		{
			_globalState = EGodBirdGlobalState.SummonBalls;
		}
		else
		{
			int num = _level.NextRandomInt(0, 3);
			if (!_hasUsedBallAttack)
			{
				num = 3;
				_hasUsedBallAttack = true;
			}
			switch (num)
			{
			case 0:
				_globalState = EGodBirdGlobalState.SpitAttack;
				globalState2 = EGodBirdGlobalState.FireballAttack;
				break;
			case 1:
				_globalState = EGodBirdGlobalState.FireballAttack;
				globalState2 = EGodBirdGlobalState.TelekinesisAttack;
				break;
			case 2:
				_globalState = EGodBirdGlobalState.TelekinesisAttack;
				globalState2 = EGodBirdGlobalState.AttackWithBalls;
				break;
			case 3:
				_globalState = EGodBirdGlobalState.AttackWithBalls;
				globalState2 = EGodBirdGlobalState.SpitAttack;
				break;
			}
		}
		if (globalState == _globalState)
		{
			_globalState = globalState2;
		}
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		_movementX = 0f;
		_isJumping = false;
		switch (_globalState)
		{
		case EGodBirdGlobalState.SpitAttack:
			UpdateSpitAttack();
			break;
		case EGodBirdGlobalState.FireballAttack:
			UpdateFireballAttack();
			break;
		case EGodBirdGlobalState.TelekinesisAttack:
			UpdateTelekinesisAttack();
			break;
		case EGodBirdGlobalState.SummonBalls:
			UpdateSummonBalls();
			break;
		case EGodBirdGlobalState.AttackWithBalls:
			UpdateBallAttack(delta);
			break;
		case EGodBirdGlobalState.WallSlamAttack:
			UpdateWallSlamAttack();
			break;
		case EGodBirdGlobalState.Growl:
			UpdateRoar();
			break;
		default:
			_nextActionTimer = 1f;
			_currentAction = EAIAction.Idle;
			break;
		}
		_lastGlobalStateTimer = _globalStateTimer;
		_globalStateTimer += delta;
	}

	private void FinishAttack()
	{
		FinishAttack(1f);
	}

	private void FinishAttack(float waitTime)
	{
		_nextActionTimer = waitTime;
		_currentAction = EAIAction.Idle;
	}

	private void UpdateSpitAttack()
	{
		if (_globalStateTimer <= 1.85f)
		{
			if (_globalStateTimer <= 0f)
			{
				SetCharacterSequence(_sneezeSequence);
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdateFireballAttack()
	{
		if (_globalStateTimer <= 1.85f)
		{
			if (_globalStateTimer <= 0f)
			{
				SetAura(isAuraGlowing: true);
				SetCharacterSequence(_spitSequence);
			}
		}
		else
		{
			if (!_doesHaveLowHealth)
			{
				SetAura(isAuraGlowing: false);
			}
			FinishAttack();
		}
	}

	private void UpdateTelekinesisAttack()
	{
		if (_globalStateTimer <= 2.5f)
		{
			if (_globalStateTimer <= 0f)
			{
				SetAura(isAuraGlowing: true);
				SetCharacterSequence(_auraStartSequence);
				if (_lastPlayerPosition.X <= _auraDangerZones[1].Position.X)
				{
					_auraDangerZones[0].IsActive = true;
					_auraDangerZones[1].IsActive = true;
				}
				else
				{
					_auraDangerZones[1].IsActive = true;
					_auraDangerZones[2].IsActive = true;
				}
				_auraDangerZones[1].PlayCue(ESFX.BossBirdAuraZone);
			}
			else if (_globalStateTimer >= 0.5f && _lastGlobalStateTimer < 0.5f)
			{
				SetCharacterSequence(_roarSequence);
			}
		}
		else
		{
			if (!_doesHaveLowHealth)
			{
				SetAura(isAuraGlowing: false);
			}
			SetCharacterSequence(_auraEndSequence);
			FinishAttack();
		}
	}

	private void UpdateBallAttack(float delta)
	{
		if (_globalStateTimer <= 9.9f)
		{
			if (_globalStateTimer <= 0f)
			{
				_nearBallSlamCount = 0;
				_farBallSlamCount = 0;
				_farBallSlamTimer = 0f;
				_nearBallSlamTimer = -0.4125f;
				SetCharacterSequence(_auraStartSequence);
			}
			if (_nearBallSlamCount < 5)
			{
				if (_nearBallAndChain.IsIdle)
				{
					_nearBallAndChain.SetTargetX(_lastPlayerPosition.X);
				}
				_nearBallAndChain.UpdateBallSlam(_nearBallSlamTimer);
				_nearBallSlamTimer += delta;
				if (_nearBallSlamTimer >= 1.65f)
				{
					_nearBallSlamTimer -= 1.65f;
					_lastPlayerPosition = _level.GetNearestProtagonistPosition(_headAppendage.Position);
					_nearBallAndChain.SetTargetX(_lastPlayerPosition.X);
					_nearBallSlamCount++;
					if (_nearBallSlamCount >= 5)
					{
						_nearBallAndChain.EndBallSlam();
					}
				}
			}
			if (_farBallSlamCount >= 5)
			{
				return;
			}
			if (_farBallAndChain.IsIdle)
			{
				_farBallAndChain.SetTargetX(_lastPlayerPosition.X);
			}
			_farBallAndChain.UpdateBallSlam(_farBallSlamTimer);
			_farBallSlamTimer += delta;
			if (_farBallSlamTimer >= 1.65f)
			{
				_farBallSlamTimer -= 1.65f;
				_lastPlayerPosition = _level.GetNearestProtagonistPosition(_headAppendage.Position);
				_farBallAndChain.SetTargetX(_lastPlayerPosition.X);
				_farBallSlamCount++;
				if (_farBallSlamCount >= 5)
				{
					_farBallAndChain.EndBallSlam();
				}
			}
		}
		else
		{
			SetCharacterSequence(_auraEndSequence);
			FinishAttack();
		}
	}

	private void UpdateSummonBalls()
	{
		if (_globalStateTimer <= 2f)
		{
			if (_globalStateTimer <= 0f)
			{
				_nearBallAndChain.PlayCue(ESFX.BossBirdChainSummon);
				SetAura(isAuraGlowing: true);
				SetCharacterSequence(_auraStartSequence);
				SetCharacterSequence(_hairIdleSequence);
			}
			_nearBallAndChain.UpdateBallSummon(_globalStateTimer - 0.07f);
			_farBallAndChain.UpdateBallSummon(_globalStateTimer);
		}
		else
		{
			SetCharacterSequence(_auraEndSequence);
			FinishAttack();
		}
	}

	private void UpdateWallSlamAttack()
	{
		if (_globalStateTimer <= 1f)
		{
			if (!(_globalStateTimer <= 0f))
			{
				return;
			}
			if (_globalStateTimer <= 0f)
			{
				SetCharacterSequence(_auraStartSequence);
			}
			{
				foreach (KeyValuePair<int, Protagonist> hero in _level.Heroes)
				{
					hero.Value.AddScriptAction(new GodBirdGustStunScript(_level, base.Damage, _pebbleParticles));
				}
				return;
			}
		}
		SetCharacterSequence(_auraEndSequence);
		FinishAttack();
	}

	private void UpdateRoar()
	{
		if (_globalStateTimer <= 4.25f)
		{
			if (_globalStateTimer >= 1.75f && _lastGlobalStateTimer < 1.75f)
			{
				SetCharacterSequenceByName("WakeUp");
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void SpitBall()
	{
		Point point = new Point(_headAppendage.Bbox.Right, _headAppendage.Position.Y);
		Point point2 = _level.GetNearestProtagonistPosition(point);
		if (point2.X < 192)
		{
			point2 = new Point(192, point2.Y);
		}
		int num = point2.X - point.X;
		float num2 = ((!IsFacingLeft) ? 1 : (-1));
		Vector2 iV = new Vector2(num2 * ((float)num * 1.35f), -50f);
		point.X += (int)(-3f * num2);
		point.Y += 2;
		GodBirdGoopBall godBirdGoopBall;
		if (_goopBalls[_goopBallCounter] == null)
		{
			godBirdGoopBall = new GodBirdGoopBall(_level, point, iV, _sprite, _spitParticleSystem, base.Damage);
			_goopBalls[_goopBallCounter] = godBirdGoopBall;
		}
		else
		{
			godBirdGoopBall = _goopBalls[_goopBallCounter];
			godBirdGoopBall.Reset(point, iV);
		}
		_level.AddProjectile(godBirdGoopBall);
		PlayCue(ESFX.EnemyCheveuxTowerVomit, point);
		_goopBallCounter = (_goopBallCounter + 1) % 3;
	}

	private void SpitFireball()
	{
		Point point = new Point(_headAppendage.Bbox.Right, _headAppendage.Position.Y);
		Point point2 = _level.GetNearestProtagonistPosition(point).Add(0, -16);
		if (point2.X < 192)
		{
			point2 = new Point(192, point2.Y);
		}
		Vector2 iV = new Vector2(point2.X - point.X, point2.Y - point.Y);
		iV.Normalize();
		iV *= 350f;
		float num = ((!IsFacingLeft) ? 1 : (-1));
		point.X += (int)(-3f * num);
		point.Y += 2;
		GodBirdAuraProjectile godBirdAuraProjectile;
		if (_auraBalls[_auraBallCounter] == null)
		{
			godBirdAuraProjectile = new GodBirdAuraProjectile(_level, point, iV, _damageCaused, _sprite);
			_auraBalls[_auraBallCounter] = godBirdAuraProjectile;
		}
		else
		{
			godBirdAuraProjectile = _auraBalls[_auraBallCounter];
			godBirdAuraProjectile.Reset(point, iV);
		}
		_level.AddProjectile(godBirdAuraProjectile);
		PlayCue(ESFX.BossBirdAuraBlastCast, point);
		_auraBallCounter = (_auraBallCounter + 1) % 3;
	}

	private void SetAura(bool isAuraGlowing)
	{
		if (isAuraGlowing != _isAuraGlowing)
		{
			_isAuraGlowing = isAuraGlowing;
			_auraTransitionTimer = 0.5f;
		}
	}

	private void UpdateGlowing(float delta)
	{
		if (_isRunningDeathScript)
		{
			return;
		}
		if (_auraTransitionTimer > 0f)
		{
			_auraTransitionTimer -= delta;
			bool flag = _auraTransitionTimer <= 0f;
			if (_isAuraGlowing)
			{
				if (!base.DoesDrawAura)
				{
					base.DoesDrawAura = true;
					base.IsGlowing = true;
					base.GlowBase = 1f;
				}
				if (flag)
				{
					base.AuraColor = BirdAuraColor;
				}
				else
				{
					float num = 1f - _auraTransitionTimer / 0.5f;
					base.AuraColor = BirdAuraColor * num;
				}
			}
			else if (flag)
			{
				base.DoesDrawAura = false;
				base.IsGlowing = false;
				base.DrawColor = Color.White;
			}
			else
			{
				float num2 = _auraTransitionTimer / 0.5f;
				base.AuraColor = BirdAuraColor * num2;
			}
		}
		if (base.IsGlowing)
		{
			_glowTimer += delta * 5f;
			if (_glowTimer >= (float)Math.PI * 2f)
			{
				_glowTimer -= (float)Math.PI * 2f;
			}
			Vector4 baseBirdGlowColor = BaseBirdGlowColor;
			baseBirdGlowColor.W = (float)Math.Cos(_glowTimer) * 0.15f + 0.85f;
			base.GlowColor = new Color(baseBirdGlowColor);
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
			if (_globalState == EGodBirdGlobalState.SpitAttack)
			{
				SpitBall();
			}
			else if (_globalState == EGodBirdGlobalState.FireballAttack)
			{
				SpitFireball();
			}
			break;
		case 1:
			PlayCue(ESFX.EnemyCheveuxTowerSquawk, _headAppendage.Position);
			if (_globalState == EGodBirdGlobalState.TelekinesisAttack)
			{
				GodBirdAuraDangerZone[] auraDangerZones = _auraDangerZones;
				foreach (GodBirdAuraDangerZone godBirdAuraDangerZone in auraDangerZones)
				{
					if (godBirdAuraDangerZone.IsActive)
					{
						godBirdAuraDangerZone.Slam();
						godBirdAuraDangerZone.IsActive = false;
					}
				}
			}
			else if (_globalState == EGodBirdGlobalState.Growl)
			{
				_level.RequestScreenShake(new Vector2(2f, 3f), 0.8f, 10f, isAffectedByTime: true);
			}
			break;
		case 2:
			PlayCue(ESFX.EnemyCheveuxTowerSquawk, _headAppendage.Position);
			break;
		case 3:
		{
			Vector2 where = new Vector2(_headAppendage.Position.X, _headAppendage.Position.Y);
			_lazerChargeParticles.AddParticles(where, IsFacingLeft);
			PlayCue(ESFX.BossBirdAuraBlastCharge, _headAppendage.Position);
			break;
		}
		case 4:
			PlayCue(ESFX.BossBirdVomitPrep, _headAppendage.Position);
			break;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 4f)
		{
			Vector2 where = new Vector2(Position.X + 24, Position.Y + -64);
			if (_deathScriptTimer < 2f)
			{
				if (deathScriptTimer <= 0f)
				{
					SetCharacterSequenceByName("Death");
					_deathParticleColorVect = _deathParticlesColor.ToVector4();
					base.DrawColor = Color.White;
					_deathParticleSystems[0] = _deathLazerParticles;
					_level.PlayCue(ESFX.BossBirdDeath, Position);
					foreach (Appendage appendage in base.Appendages[0].Appendages)
					{
						appendage.DoesInheritDrawColor = true;
					}
				}
				_deathLazerParticles.AddParticles(where);
				_isGlowing = true;
				_nearBallAndChain.IsGlowing = true;
				_farBallAndChain.IsGlowing = true;
				_rearBallAndChain.IsGlowing = true;
				_glowColor = DeathGlowColor;
				_nearBallAndChain.GlowColor = _glowColor;
				_farBallAndChain.GlowColor = _glowColor;
				_rearBallAndChain.GlowColor = _glowColor;
				float num = _deathScriptTimer / 2f;
				float num2 = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
				_glowBase = 1f + num2 * 10f;
				_nearBallAndChain.GlowBase = _glowBase;
				_farBallAndChain.GlowBase = _glowBase;
				_rearBallAndChain.GlowBase = _glowBase;
				_nearBallAndChain.UpdateBallDying(_deathScriptTimer);
				_farBallAndChain.UpdateBallDying(_deathScriptTimer - 0.07f);
				if (_deathScriptTimer >= 1.5f && deathScriptTimer < 1.5f)
				{
					_level.RequestScreenFlash(1.01f, 1f, 1f);
				}
			}
			else if (deathScriptTimer < 2f)
			{
				PlayCue(ESFX.FoleyExplosionFeather, base.OuterBbox.Center);
				EmitFeathers(_appendages[0]);
				_doesDrawSpriteAndAppendages = false;
				_doesDrawTrail = false;
				_doesDrawBrushTrail = false;
				base.DoesDrawAura = false;
				DropLoot();
				_deathLazerParticles.KillOffParticles(0f);
				GodBirdAuraDangerZone[] auraDangerZones = _auraDangerZones;
				foreach (GodBirdAuraDangerZone godBirdAuraDangerZone in auraDangerZones)
				{
					godBirdAuraDangerZone.SilentKill();
				}
				_nearBallAndChain.SilentKill();
				_rearBallAndChain.SilentKill();
				_farBallAndChain.SilentKill();
			}
			ParticleSystem[] deathParticleSystems = _deathParticleSystems;
			for (int j = 0; j < deathParticleSystems.Length; j++)
			{
				deathParticleSystems[j]?.Update(delta);
			}
		}
		else
		{
			EndBossDeathScript();
		}
	}

	private void EmitFeathers(Appendage appendage)
	{
		Point center = appendage.Bbox.Center;
		BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsLarge, center, _level);
		battleAnimation.TeamSide = base.DefaultTeam;
		battleAnimation.AnimationStart = 5;
		battleAnimation.AnimationLength = 6;
		battleAnimation.AnimationSpeed = 0.04f;
		battleAnimation.DrawColor = Color.White * 0.8f;
		battleAnimation.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 37, 4);
		BattleAnimation newAnimation = battleAnimation;
		BattleAnimation battleAnimation2 = new BattleAnimation(null, center, _level);
		battleAnimation2.TeamSide = base.DefaultTeam;
		battleAnimation2.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 41, 4);
		BattleAnimation newAnimation2 = battleAnimation2;
		_level.AddAnimation(newAnimation);
		_level.AddAnimation(newAnimation2);
		foreach (Appendage appendage2 in appendage.Appendages)
		{
			if (appendage2.Bbox.Height > 17)
			{
				EmitFeathers(appendage2);
			}
		}
	}

	internal override void InitializeForBestiary()
	{
		SetCharacterSequenceByName("Blink");
	}
}
