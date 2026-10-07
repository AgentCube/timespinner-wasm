using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Sandman;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class SandmanBoss : BossClass
{
	private enum ESandmanState
	{
		Idle,
		CeilingRubble,
		BodyDash,
		BallSlam,
		ScepterRevolution,
		FloorSpikes,
		JawCrush,
		StartPhase2,
		StartCutscene,
		Phase2GoTo,
		Phase2Idle
	}

	private const int RoomCenterX = 352;

	private const int RoomQuarterWidth = 96;

	private const int CeilingY = 16;

	private const int FloorY = 224;

	private const int LeftWallX = 147;

	private const int RightWallX = 557;

	private const int EnvPrefabID = 491;

	private const int WallPrefabArgument = 1601;

	private const int CameraCenterX = 352;

	private const int CameraCenterY = 136;

	private const float IntroTimeToFadeIn = 1.5f;

	private const int MaxSpikesCount = 16;

	private const int SpikeStartX = 176;

	private const int NumberOfSpikesSequence = 12;

	private const int SequenceSpikesBufferX = 24;

	private const float TimeForSpikesWindup = 1f;

	private const float SequenceSpikeSleepTime = 0.125f;

	private const float TimeForSpikesCooldown = 0.5f;

	private const float TimeForSpikesSequence = 1.5f;

	private const float TimeBeforeSpikesCooldown = 2.5f;

	private const float TimeForSpikesSequenceEntireAttack = 3f;

	private const int SlamCount = 5;

	private const float TimeForOneBallToSlam = 1.65f;

	private const float SlamTimeOffsetBetweenBalls = 0.4125f;

	private const float TimeForEntireBallSlamSpam = 9.9f;

	private const int TotalRubbleGroundPounds = 5;

	private const int MaxRubbleCount = 8;

	private const int LeftBoulderFallStartX = 184;

	private const int RightBoulderFallEndX = 440;

	private const int BoulderFallWidth = 256;

	private const int BoulderFallHalfWidth = 128;

	private const int BoulderDropY = 12;

	private const int PoundSequentialOffsetX = 48;

	private const float TimeToRubbleWindup = 0.5f;

	private const float TimeForOnePoundWindUp = 0.35f;

	private const float TimeForOnePoundSwipe = 0.1f;

	private const float TimeForOneEntirePound = 0.45f;

	private const float TimeForRubbleSequence = 2.75f;

	private const float TimeForPhase2FadeOut = 0.15f;

	private const float TimeForPhase2Wait = 2.5f;

	private const float TimeForPhase2Rise = 1.5f;

	private const float TimeForPhase2Fall = 0.75f;

	private const float TimeBeforePhase2Rise = 2.65f;

	private const float TimeBeforePhase2Fall = 4.15f;

	private const float TimeForEntirePhase2Sequence = 4.9f;

	private const float TimeForSandmanPhase2CuePlay = 3.15f;

	private const int Phase2RiseStartY = 320;

	private const int Phase2RiseEndY = 128;

	private const int Phase2FallEndY = 144;

	private const int Phase2IdleY = 160;

	private const int SpearDashStartOffsetX = 168;

	private const int SpearDashStartY = 128;

	private const int JawCrushStartPositionY = 192;

	private const int ScepterRevolutionY = 124;

	private const float TimeToPhase2GoTo = 2f;

	private const float Phase2TimeToWait = 0f;

	private const int MaxCrushersCount = 4;

	private const int CrusherXLeft = 232;

	private const int CrusherXRight = 472;

	private const int CrusherXMid = 352;

	private const int CrusherY = 32;

	private const float TimeBeforeSecondCrusher = 0.75f;

	private const float TimeBeforeThirdCrusher = 1.25f;

	private const float TimeForEntireCrusherSequence = 2.25f;

	private const int ScepterRevolutionStartY = 116;

	private const int ScepterRevolutionWidth = 174;

	private const int ScepterRevolutionHeight = 90;

	private const int ScepterRevolutionWindupWidth = 24;

	private const int ScepterRevolutionWindupHeight = 24;

	private const int ScepterShockwaveOffset = 4;

	private const float ScepterHourglassRotationSpeed = 6f;

	private const float TimeForScepterRevolutionWindup = 0.5f;

	private const float TimeForScepterRevolutionToTransitionFromWindupToCircuit = 0.1f;

	private const float TimeForScepterRevolutionCircuit = 1.1f;

	private const float TotalScepterRevolutionTime = 1.6f;

	private const float TimeForSpearFollow = 1.5f;

	private const float TimeForSpearWindup = 0.35f;

	private const float TimeForSpearDash = 0.75f;

	private const float TimeForSpearStuck = 2f;

	private const float SpearDashSpeed = 35000f;

	private const float TimeBeforeSpearDash = 1.85f;

	private const float TimeBeforeSpearStuck = 2.6f;

	private const float TimeForEntireSpearAttack = 4.6f;

	private const int SpearStopBufferX = 12;

	private const float SpearDashMaxSpeed = 1200f;

	private const float SpearDashWindupSpeed = 2000f;

	private const float SpearFollowSpeed = 450f;

	private const float RoomShakeTime = 0.75f;

	private const float RoomShakeFrequency = 20f;

	private const float IndefiniteActionTime = 1000f;

	private const float FrenzyThreshold = 0.5f;

	private const float TimeBetweenAttacks = 1f;

	private const int DeathRoomCenterOffsetY = 8;

	private const int DeathChargeOffsetX = 0;

	private const int DeathChargeOffsetY = -8;

	private const int DeathChargeBaseMinRadius = 24;

	private const int DeathChargeBaseMaxRadius = 48;

	private const int DeathChargeRadiusGrowth = 64;

	private const float DeathTimeToMoveToRoomCenter = 1.5f;

	private const float DeathTimeToCharge = 2f;

	private const float DeathRotationSpeedStart = 2f;

	private const float DeathRotationSpeedEnd = 8f;

	private const float DeathScreenFlashDuration = 0.4f;

	private const float DeathTimeBeforeAddingScreenFlash = 1.8f;

	private static readonly Vector2 SpearRoomShakeDimensions = new Vector2(8f, 0f);

	private static readonly Color DeathLevelDrawColor = new Color(1f, 0.9f, 0.75f, 1f);

	private static readonly Color DeathShockwaveColor = new Color(0.6f, 0.35f, 0.2f, 0.5f);

	private readonly int _baseDamageCaused;

	private readonly Point _baseOrbPosition = new Point(352, 116);

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly SandDrawHelper _sandDrawHelper;

	private readonly SandmanHitParticleSystem _sandHitParticles;

	private readonly SandmanDissolveParticleSystem _dissolveParticles;

	private readonly ShockwaveAnimation _scepterShockwaveAnimation;

	private readonly EnvPrefabTempleWall _leftWall;

	private readonly EnvPrefabTempleWall _rightWall;

	private readonly SandmanBossCeilingSpikes _ceilingSpikes;

	private readonly SandmanBossScepter _scepterDamageAreaA;

	private readonly SandmanBossScepter _scepterDamageAreaB;

	private readonly SandmanBossBallAndChain _nearBallAndChain;

	private readonly SandmanBossBallAndChain _farBallAndChain;

	private readonly SandmanBossHourglassManager _hourglassManager;

	private readonly SandmanBossSpike[] _spikes = new SandmanBossSpike[16];

	private readonly SandmanBossRubble[] _rubble = new SandmanBossRubble[8];

	private readonly SandmanBossCrusherProjectile[] _crushers = new SandmanBossCrusherProjectile[4];

	private bool _isInSecondPhase;

	private bool _isTargetToOurLeft;

	private bool _isDashingToLeft;

	private bool _isSubScepterAttacking;

	private bool _wasLeftClawLastToGroundPound;

	private bool _isUsingSandShader;

	private bool _hasPlayedGroundPoundSfx;

	private bool _isPlayerNear;

	private ESandmanState _sandmanState;

	private ESandmanState _lastPhase2State;

	private ESandmanState _nextSandmanState;

	private int _lastNonIdlePhase2StateID;

	private int _usedSpikeCount;

	private int _usedRubbleCount;

	private int _usedCrusherCount;

	private int _nearBallSlamCount;

	private int _farBallSlamCount;

	private float _farBallSlamTimer;

	private float _nearBallSlamTimer = -0.4125f;

	private float _sandmanTimer;

	private float _lastSandmanTimer;

	private Point _lastPlayerPosition;

	private Point _phase2MoveStart;

	private Point _phase2MoveTarget;

	private SandmanDeathChargeParticleSystem _deathChargeParticles;

	private SandmanDeathLazerParticleSystem _deathLazerParticles;

	public SandmanBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_sandmanState = ESandmanState.StartCutscene;
		_agility = 1f;
		_bboxOffset = new Point(7, 4);
		Bbox = new Rectangle(_position.X, _position.Y, 48, 72);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_baseDamageCaused = _damageCaused;
		ChangeAnimation(-1);
		SetDoesDrawAppendageTrails(value: true, isHost: true, 6, 3f);
		_idleSequence = GetCharacterSequenceByName("Idle");
		SetCharacterSequence(_idleSequence);
		_isUsingSandShader = true;
		_sandDrawHelper = new SandDrawHelper(this);
		_hourglassManager = new SandmanBossHourglassManager(this, _sprite);
		_scepterShockwaveAnimation = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, Point.Zero, _level, Color.SandyBrown * 0.5f);
		_scepterDamageAreaA = new SandmanBossScepter(_level, Position, _sprite, base.Damage);
		_scepterDamageAreaB = new SandmanBossScepter(_level, Position, _sprite, base.Damage);
		_nearBallAndChain = new SandmanBossBallAndChain(_level, Position, _sprite, base.Damage, isFrontPlane: true);
		_farBallAndChain = new SandmanBossBallAndChain(_level, Position, _sprite, base.Damage, isFrontPlane: false);
		_sandHitParticles = new SandmanHitParticleSystem(_level.GCM.TxParticleEnergy, 6);
		_particleSystems.Add(_sandHitParticles);
		_dissolveParticles = new SandmanDissolveParticleSystem(_sprite, 1);
		_particleSystems.Add(_dissolveParticles);
		Point inPosition2 = new Point(147, Position.Y);
		Point inPosition3 = new Point(557, Position.Y);
		_ceilingSpikes = new SandmanBossCeilingSpikes(_level, Position, _sprite, new ObjectTileSpecification());
		_leftWall = new EnvPrefabTempleWall(_level, inPosition2, -1, new ObjectTileSpecification(491)
		{
			Argument = 1601
		}, EEnvironmentPrefabType.L16_Wall);
		_rightWall = new EnvPrefabTempleWall(_level, inPosition3, -1, new ObjectTileSpecification(491)
		{
			Argument = 1601,
			IsFlippedHorizontally = true
		}, EEnvironmentPrefabType.L16_Wall);
		base.DrawColor = Color.Transparent;
		base.IsDormant = true;
	}

	internal void StartBattle()
	{
		_sandmanState = ESandmanState.Idle;
		base.IsBossIntroInProgress = false;
		base.IsDormant = false;
		_level.HasPlayerBeenDamagedInThisRoom = false;
		_level.HasPlayerFrozenTimeInThisRoom = false;
		if (!_level.JukeBox.DoesNotPlaySounds)
		{
			BossClass.PlaySongByBossType(_level.JukeBox, base.BossType);
		}
		_damageCaused = _baseDamageCaused;
		_isInvulnerable = false;
		_isAlwaysInvulnerable = false;
		base.DoesTouchDamageKnockback = true;
		base.DrawColor = Color.White;
		_level.RequestAddObject(_leftWall);
		_level.RequestAddObject(_rightWall);
		_level.RequestAddObject(_ceilingSpikes);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_sandmanTimer = 0f;
		_lastSandmanTimer = -1E-07f;
		_nextActionTimer = 1000f;
		_lastPlayerPosition = _level.GetNearestProtagonistPosition(Position);
		_isTargetToOurLeft = _lastPlayerPosition.X < Position.X;
		if (_sandmanState == ESandmanState.StartCutscene)
		{
			return;
		}
		if (!_isInSecondPhase)
		{
			if (base.HPPercentage <= 0.5f)
			{
				_isInSecondPhase = true;
				_sandmanState = ESandmanState.StartPhase2;
				return;
			}
			switch (_level.NextRandomInt(0, 2))
			{
			case 0:
				_sandmanState = ESandmanState.CeilingRubble;
				break;
			case 1:
				_sandmanState = ESandmanState.BallSlam;
				break;
			default:
				_sandmanState = ESandmanState.FloorSpikes;
				break;
			}
			return;
		}
		_sandmanState = ESandmanState.Phase2GoTo;
		_phase2MoveStart = Position;
		if (_lastPhase2State == ESandmanState.Phase2Idle)
		{
			int num = _level.NextRandomInt(0, 2);
			if (num == _lastNonIdlePhase2StateID)
			{
				num = (num + 1) % 3;
			}
			switch (num)
			{
			case 0:
				_isDashingToLeft = Position.X > 352;
				_nextSandmanState = ESandmanState.BodyDash;
				_phase2MoveTarget = new Point(352 + 168 * (_isDashingToLeft ? 1 : (-1)), 128);
				break;
			case 1:
				_nextSandmanState = ESandmanState.ScepterRevolution;
				_phase2MoveTarget = new Point(352, 124);
				break;
			default:
				_nextSandmanState = ESandmanState.JawCrush;
				_phase2MoveTarget = new Point(352, 192);
				break;
			}
			_lastNonIdlePhase2StateID = num;
		}
		else
		{
			_nextSandmanState = ESandmanState.Phase2Idle;
			_phase2MoveTarget = new Point(352 + 96 * (_isTargetToOurLeft ? 1 : (-1)), 160);
		}
		_lastPhase2State = _nextSandmanState;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (_sandmanState == ESandmanState.Phase2GoTo)
		{
			UpdatePhase2GoTo();
		}
		switch (_sandmanState)
		{
		case ESandmanState.CeilingRubble:
			UpdateCeilingRubbleAttack();
			break;
		case ESandmanState.BodyDash:
			UpdateBodyDashAttack(delta);
			break;
		case ESandmanState.BallSlam:
			UpdateStatueSlamAttack(delta);
			break;
		case ESandmanState.ScepterRevolution:
			UpdateScepterRevolutionAttack();
			break;
		case ESandmanState.FloorSpikes:
			UpdateFloorSpikesAttack();
			break;
		case ESandmanState.JawCrush:
			UpdateJawCrusherAttack();
			break;
		case ESandmanState.StartCutscene:
			UpdateStartCutscene();
			break;
		case ESandmanState.StartPhase2:
			UpdateStartPhase2();
			break;
		case ESandmanState.Phase2Idle:
			UpdatePhase2Idle();
			break;
		default:
			FinishAttack();
			break;
		case ESandmanState.Phase2GoTo:
			break;
		}
		_lastSandmanTimer = _sandmanTimer;
		_sandmanTimer += delta;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isUsingSandShader)
			{
				_sandDrawHelper.Update(delta);
			}
			else
			{
				_hourglassManager.Update(delta);
			}
			if (!_level.IsPlayerInputBlocked)
			{
				_level.SetCameraPosition(new Point(352, 136));
			}
		}
		base.Update(delta);
	}

	private void UpdateCeilingRubbleAttack()
	{
		if (_sandmanTimer <= 2.75f)
		{
			if (_sandmanTimer <= 0.5f)
			{
				_hasPlayedGroundPoundSfx = false;
				return;
			}
			float num = _sandmanTimer - 0.5f;
			bool flag = (int)Math.Floor(num / 0.45f) % 2 == 0;
			float num2 = num.Mod(0.45f);
			float num3 = (_lastSandmanTimer - 0.5f).Mod(0.45f);
			if (num2 < num3)
			{
				if (!flag)
				{
					_lastPlayerPosition = _level.GetNearestProtagonistPosition(Position);
					_isPlayerNear = _lastPlayerPosition.X > 352;
				}
				int num4 = (_isPlayerNear ? (-48) : 48);
				int num5 = _lastPlayerPosition.X + (flag ? num4 : 0);
				int num6 = num5 - 184;
				AddRubble(new Point(num5, 12));
				int x = 184 + (num6 + 128) % 256;
				AddRubble(new Point(x, 12));
				_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: false);
				_wasLeftClawLastToGroundPound = !_wasLeftClawLastToGroundPound;
				if (!_hasPlayedGroundPoundSfx)
				{
					PlayCue(ESFX.BossSandmanGroundPound);
					_hasPlayedGroundPoundSfx = true;
				}
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void AddRubble(Point startPoint)
	{
		SandmanBossRubble sandmanBossRubble = null;
		if (_usedRubbleCount < 8)
		{
			sandmanBossRubble = new SandmanBossRubble(startPoint, _level, _sprite, base.Damage);
			_rubble[_usedRubbleCount] = sandmanBossRubble;
			_usedRubbleCount++;
		}
		else
		{
			for (int i = 0; i < 8; i++)
			{
				if (_rubble[i].IsFinished && _rubble[i].IsParticleSystemFinished)
				{
					sandmanBossRubble = _rubble[i];
					break;
				}
			}
		}
		if (sandmanBossRubble != null)
		{
			sandmanBossRubble.Reset(startPoint);
			_level.AddProjectile(sandmanBossRubble);
		}
	}

	private void UpdateBodyDashAttack(float delta)
	{
		_isFlying = true;
		_isAffectedByGravity = false;
		_maxMoveSpeed = 1200f;
		if (_sandmanTimer < 1.5f)
		{
			if (_sandmanTimer <= 0f)
			{
				_hourglassManager.RotateToAngle((float)Math.PI * 2f, 1.5f);
				PlayCue(ESFX.BossSandmanRamCharge);
			}
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			int num = _position.Y - nearestProtagonistPosition.Y + 16;
			num = ((num > 10) ? 10 : num);
			num = ((num < -10) ? (-10) : num);
			_velocity.X = 0f;
			if (Math.Abs(num) > 4)
			{
				_velocity.Y += (float)num * -450f * delta;
			}
		}
		else if (_sandmanTimer < 1.85f)
		{
			if (_lastAbilityTimer < 1.5f)
			{
				PlayCue(ESFX.BossSandmanRam);
			}
			_velocity.Y = 0f;
			_velocity.X += 2000f * delta * (float)(_isDashingToLeft ? 1 : (-1));
		}
		else if (_sandmanTimer < 2.6f)
		{
			if (!_doesDrawTrail)
			{
				_doesDrawTrail = true;
				_trailFadeRate = 2f;
				_trailLength = 16;
				ClearTrailHistory();
			}
			_velocity.Y = 0f;
			_velocity.X += -35000f * delta * (float)(_isDashingToLeft ? 1 : (-1));
			int num2 = (_isDashingToLeft ? _leftWall.Bbox.Right : _rightWall.Bbox.Left);
			int num3 = (_isDashingToLeft ? (num2 + 12) : (num2 - 12));
			if (_isDashingToLeft ? (Position.X <= num3) : (Position.X >= num3))
			{
				_sandmanTimer = 2.6f;
				_velocity = Vector2.Zero;
				Position = new Point(num3, Position.Y);
				SnapBboxToPosition();
				_level.RequestScreenShake(SpearRoomShakeDimensions, 0.75f, 20f, isAffectedByTime: false);
				Rectangle outerBbox = base.OuterBbox;
				Point position = new Point(num2, outerBbox.Center.Y);
				Point position2 = new Point(num2, outerBbox.Top + 8);
				Point position3 = new Point(num2, outerBbox.Bottom - 8);
				_level.AddAnimation(EBattleAnimationType.CrackingDust, position, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.CrackingDust, position2, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.CrackingDust, position3, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.Pebbles, position, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.Pebbles, position2, ETeamSide.Enemies);
				_level.AddAnimation(EBattleAnimationType.Pebbles, position3, ETeamSide.Enemies);
				PlayCue(ESFX.BossSandmanRamCrash);
			}
		}
		else if (_sandmanTimer > 4.6f)
		{
			_hourglassManager.ResumeNormalRotation();
			FinishAttack();
		}
	}

	private void UpdateStatueSlamAttack(float delta)
	{
		if (_sandmanTimer <= 9.9f)
		{
			if (_sandmanTimer <= 0f)
			{
				_nearBallSlamCount = 0;
				_farBallSlamCount = 0;
				_farBallSlamTimer = 0f;
				_nearBallSlamTimer = -0.4125f;
				PlayCue2D(ESFX.BossSandmanChainCast);
			}
			if (_nearBallSlamCount < 5)
			{
				if (!_nearBallAndChain.IsActive)
				{
					_nearBallAndChain.StartBallSlam();
					_nearBallAndChain.SetTargetX(_lastPlayerPosition.X);
				}
				_nearBallAndChain.UpdateSlam(_nearBallSlamTimer);
				_nearBallSlamTimer += delta;
				if (_nearBallSlamTimer >= 1.65f)
				{
					_nearBallSlamTimer -= 1.65f;
					_lastPlayerPosition = _level.GetNearestProtagonistPosition(Position);
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
			if (!_farBallAndChain.IsActive)
			{
				_farBallAndChain.StartBallSlam();
				_farBallAndChain.SetTargetX(_lastPlayerPosition.X);
			}
			_farBallAndChain.UpdateSlam(_farBallSlamTimer);
			_farBallSlamTimer += delta;
			if (_farBallSlamTimer >= 1.65f)
			{
				_farBallSlamTimer -= 1.65f;
				_lastPlayerPosition = _level.GetNearestProtagonistPosition(Position);
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
			FinishAttack();
		}
	}

	private void UpdateScepterRevolutionAttack()
	{
		Point position = _baseOrbPosition;
		Point position2 = _baseOrbPosition;
		if (_sandmanTimer < 0.5f)
		{
			if (_sandmanTimer <= 0f)
			{
				PlayCue(_isSubScepterAttacking ? ESFX.BossSandmanBallB : ESFX.BossSandmanBallA);
				Point position3 = Bbox.Center.Add(4, 4);
				_scepterShockwaveAnimation.Reset(position3, isFacingLeft: true);
				_level.AddAnimation(_scepterShockwaveAnimation);
				_lastPlayerPosition = _level.GetNearestProtagonistPosition(Position);
				_isTargetToOurLeft = _lastPlayerPosition.X < Position.X;
				_scepterDamageAreaA.Reset(!_isSubScepterAttacking);
				_scepterDamageAreaB.Reset(!_isSubScepterAttacking);
				if (!_isSubScepterAttacking)
				{
					_level.AddProjectile(_scepterDamageAreaA);
					_level.AddProjectile(_scepterDamageAreaB);
				}
			}
			float num = _sandmanTimer / 0.5f;
			Point b = new Point((int)((Math.Cos(num * (float)Math.PI) - 1.0) * 24.0) * ((!_isTargetToOurLeft) ? 1 : (-1)), (int)((0.0 - Math.Sin(num * ((float)Math.PI / 2f))) * 24.0));
			position = _baseOrbPosition.Add(b);
			position2 = _baseOrbPosition.Add(new Point(-b.X, b.Y));
			_hourglassManager.RotationSpeedMultiplier = MathEx.SineInterpolate(1f, 6f, num);
		}
		else if (_sandmanTimer < 1.6f)
		{
			if (_lastSandmanTimer < 0.5f)
			{
				_scepterDamageAreaA.CanDamageThings = true;
				_scepterDamageAreaB.CanDamageThings = true;
			}
			float num2 = _sandmanTimer - 0.5f;
			float num3 = num2 / 1.1f;
			float num4 = (num3 - 0.05f) * 1.16f;
			_hourglassManager.RotationSpeedMultiplier = MathEx.SineInterpolate(6f, 1f, num3);
			Point point = new Point((int)(Math.Sin(num4 * (float)Math.PI) * 174.0) * ((!_isTargetToOurLeft) ? 1 : (-1)), (int)(Math.Cos(num4 * ((float)Math.PI / 2f)) * 90.0));
			Point b2 = point;
			if (num2 < 0.1f)
			{
				float amount = num2 / 0.1f;
				Point start = new Point(-48 * ((!_isTargetToOurLeft) ? 1 : (-1)), -24);
				b2 = start.SineInterpolate(point, amount);
			}
			else if (num4 > 1f)
			{
				b2 = Point.Zero;
			}
			position = _baseOrbPosition.Add(b2);
			position2 = _baseOrbPosition.Add(new Point(-b2.X, b2.Y));
		}
		else if (_sandmanTimer >= 1.6f)
		{
			_hourglassManager.RotationSpeedMultiplier = 1f;
			if (!_isSubScepterAttacking)
			{
				_isSubScepterAttacking = true;
				_sandmanTimer = 0f;
				UpdateScepterRevolutionAttack();
			}
			else
			{
				_isSubScepterAttacking = false;
				_scepterDamageAreaA.Hide();
				_scepterDamageAreaB.Hide();
				FinishAttack();
			}
		}
		_scepterDamageAreaA.Position = position;
		_scepterDamageAreaB.Position = position2;
	}

	private void UpdateFloorSpikesAttack()
	{
		if (_sandmanTimer < 3f)
		{
			if (!(_sandmanTimer < 1f) && _sandmanTimer < 2.5f && _lastSandmanTimer < 1f)
			{
				float num = 0f;
				int num2 = 0;
				for (int i = 0; i < 12; i++)
				{
					int x = 176 + num2;
					AddSpike(new Point(x, 224), num);
					num += 0.125f;
					num2 += 24;
				}
				PlayCue2D(ESFX.BossSandmanSpikeCast);
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void AddSpike(Point startPoint, float sleepTime)
	{
		SandmanBossSpike sandmanBossSpike = null;
		if (_usedSpikeCount < 16)
		{
			sandmanBossSpike = new SandmanBossSpike(_level, startPoint, _sprite, base.Damage);
			_spikes[_usedSpikeCount] = sandmanBossSpike;
			_usedSpikeCount++;
		}
		else
		{
			for (int i = 0; i < 16; i++)
			{
				if (_spikes[i].IsFinished)
				{
					sandmanBossSpike = _spikes[i];
					break;
				}
			}
		}
		if (sandmanBossSpike != null)
		{
			sandmanBossSpike.Reset(startPoint, sleepTime);
			_level.AddProjectile(sandmanBossSpike);
		}
	}

	private void UpdateJawCrusherAttack()
	{
		if (_sandmanTimer >= 0f && _lastSandmanTimer < 0f)
		{
			AddCrusher(SandmanBossCrusherProjectile.ESandmanCrusherType.Left);
			PlayCue(ESFX.BossSandmanTeethCast);
		}
		else if (_sandmanTimer >= 0.75f && _lastSandmanTimer < 0.75f)
		{
			AddCrusher(SandmanBossCrusherProjectile.ESandmanCrusherType.Right);
		}
		else if (_sandmanTimer >= 1.25f && _lastSandmanTimer < 1.25f)
		{
			AddCrusher(SandmanBossCrusherProjectile.ESandmanCrusherType.Center);
		}
		else if (_sandmanTimer >= 2.25f)
		{
			FinishAttack();
		}
	}

	private void AddCrusher(SandmanBossCrusherProjectile.ESandmanCrusherType crusherType)
	{
		SandmanBossCrusherProjectile sandmanBossCrusherProjectile = null;
		Point point = crusherType switch
		{
			SandmanBossCrusherProjectile.ESandmanCrusherType.Left => new Point(232, 32), 
			SandmanBossCrusherProjectile.ESandmanCrusherType.Center => new Point(352, 32), 
			_ => new Point(472, 32), 
		};
		if (_usedCrusherCount < 4)
		{
			sandmanBossCrusherProjectile = new SandmanBossCrusherProjectile(_level, point, _sprite, base.Damage);
			_crushers[_usedCrusherCount] = sandmanBossCrusherProjectile;
			_usedCrusherCount++;
		}
		else
		{
			for (int i = 0; i < 4; i++)
			{
				if (_crushers[i].IsFinished)
				{
					sandmanBossCrusherProjectile = _crushers[i];
					break;
				}
			}
		}
		if (sandmanBossCrusherProjectile != null)
		{
			sandmanBossCrusherProjectile.Reset(point, crusherType);
			_level.AddProjectile(sandmanBossCrusherProjectile);
		}
	}

	private void FinishAttack()
	{
		if (_isInSecondPhase || base.HPPercentage <= 0.5f)
		{
			_nextActionTimer = 0f;
			return;
		}
		_nextActionTimer = 1f;
		_currentAction = EAIAction.Idle;
	}

	private void UpdateStartCutscene()
	{
		_nextActionTimer = 10f;
		if (_sandmanTimer < 1.5f)
		{
			float num = _sandmanTimer / 1.5f;
			base.DrawColor = Color.White * num;
		}
		else if (_lastSandmanTimer < 1.5f)
		{
			base.DrawColor = Color.White;
		}
	}

	private void UpdateStartPhase2()
	{
		if (_sandmanTimer <= 0f)
		{
			PlayCue2D(ESFX.BossSandmanDissolve);
			_dissolveParticles.AddParticles(Position.ToVector2());
			_canBeDamaged = false;
			_damageCaused = 0;
		}
		if (_sandmanTimer < 0.15f)
		{
			float num = 1f - _sandmanTimer / 0.15f;
			base.DrawColor = Color.White * num;
		}
		else if (_sandmanTimer < 2.65f)
		{
			base.DrawColor = Color.Transparent;
		}
		else if (_sandmanTimer < 4.15f)
		{
			if (_lastSandmanTimer < 2.65f)
			{
				SwitchToHourglass();
				base.DrawColor = Color.White;
			}
			if (_sandmanTimer >= 3.15f && _lastSandmanTimer < 3.15f)
			{
				PlayCue2D(ESFX.BossSandmanPhase2Intro);
			}
			float num2 = (_sandmanTimer - 2.65f) / 1.5f;
			num2 *= num2;
			int y = (int)Math.Round(MathEx.SineInterpolate(320f, 128f, num2));
			Position = new Point(352, y);
		}
		else if (_sandmanTimer < 4.9f)
		{
			float percentage = (_sandmanTimer - 4.15f) / 0.75f;
			int y2 = (int)Math.Round(MathEx.WaveInterpolate(128f, 144f, percentage));
			Position = new Point(352, y2);
		}
		else
		{
			_lastPhase2State = ESandmanState.Idle;
			_canBeDamaged = true;
			_damageCaused = _baseDamageCaused;
			FinishAttack();
		}
	}

	private void UpdatePhase2GoTo()
	{
		if (_sandmanTimer <= 0f)
		{
			_hourglassManager.ResumeNormalRotation();
		}
		if (_sandmanTimer <= 2f)
		{
			float amount = _sandmanTimer / 2f;
			Position = _phase2MoveStart.WaveInterpolate(_phase2MoveTarget, amount);
			return;
		}
		Position = _phase2MoveTarget;
		_sandmanState = _nextSandmanState;
		_sandmanTimer = 0f;
		_lastSandmanTimer = -0.033f;
	}

	private void UpdatePhase2Idle()
	{
		if (_sandmanTimer >= 0f)
		{
			FinishAttack();
		}
	}

	private void SwitchToHourglass()
	{
		StopCharacterSequences();
		_isUsingSandShader = false;
		_appendages.Clear();
		_particleSystems.Clear();
		Bbox = new Rectangle(0, 0, 16, 16);
		SnapBboxToPosition();
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByLevelBounds = false;
		_doesCollideWithTiles = false;
		_doesIgnoreOutOfBoundsDeath = true;
		_appendages.Add(_hourglassManager.TopGlass);
		_appendages.Add(_hourglassManager.BottomGlass);
		_appendages.Add(_hourglassManager.PowerSphere);
		_appendages.Add(_hourglassManager.CollisionAppendage);
		_particleSystems.Add(_hourglassManager.ChargeParticles);
		SetDoesDrawAppendageTrails(value: true, isHost: true, 12, 4f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isUsingSandShader)
		{
			if (_damageFlashFrame > -1 && _damageFlashFrame % 3 != 0)
			{
				base.Draw(spriteBatch);
				return;
			}
			foreach (Appendage appendage in _appendages)
			{
				if (appendage.DoesInheritDrawColor)
				{
					appendage.DrawColor = base.DrawColor;
				}
				_sandDrawHelper.Draw(spriteBatch, appendage, _sandTextureRatio);
			}
			DrawParticleSystems(spriteBatch);
		}
		else
		{
			_hourglassManager.SandSwirl.Draw(spriteBatch, isOver: false);
			_hourglassManager.GlowTexture.Draw(spriteBatch);
			base.Draw(spriteBatch);
			_hourglassManager.SandSwirl.Draw(spriteBatch, isOver: true);
		}
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (!_isInSecondPhase && base.HPPercentage < 0.5f)
		{
			base.HP = (int)Math.Floor((float)base.MaxHP * 0.5f) - 1;
		}
		if (flag && _isUsingSandShader)
		{
			bool isFacingRight = ((Math.Abs(velocity.X) > 0.1f) ? (velocity.X > 0f) : (where.X < Position.X));
			_sandHitParticles.IsFacingRight = isFacingRight;
			_sandHitParticles.AddParticles(where.ToVector2());
		}
		return flag;
	}

	private void DismissAttacks()
	{
		if (_scepterDamageAreaA != null && _scepterDamageAreaA.IsActive)
		{
			_scepterDamageAreaA.Hide();
		}
		if (_scepterDamageAreaB != null && _scepterDamageAreaB.IsActive)
		{
			_scepterDamageAreaB.Hide();
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		int x = _level.RoomSize.X / 2;
		int y = _level.RoomSize.Y / 2 + 8;
		_level.IsPreventingPauseMenuUsage = true;
		if (_deathScriptTimer <= 0f)
		{
			_level.JukeBox.PlayCue(ESFX.BossSandmanDeath);
			_deathChargeParticles = new SandmanDeathChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathLazerParticles = new SandmanDeathLazerParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathParticleSystems[0] = _deathChargeParticles;
			_deathParticleSystems[1] = _deathLazerParticles;
			DismissAttacks();
			_hourglassManager.ResumeNormalRotation();
			_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 24f, isAffectedByTime: false);
		}
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 1.5f)
		{
			float amount = _deathScriptTimer / 1.5f;
			Position = base.DeathPosition.SineInterpolate(new Point(x, y), amount);
		}
		else if (deathScriptTimer < 1.5f)
		{
			Position = new Point(x, y);
		}
		if (_deathScriptTimer <= 2f)
		{
			float num = _deathScriptTimer / 2f;
			float rotationSpeedMultiplier = MathHelper.Lerp(2f, 8f, num);
			_hourglassManager.RotationSpeedMultiplier = rotationSpeedMultiplier;
			base.IsGlowing = true;
			_glowBase = 1f + num * 5f;
			if (_deathChargeParticles != null && _deathLazerParticles != null)
			{
				int num2 = (int)(64f * num);
				int num3 = 24 + num2;
				int num4 = 48 + num2;
				_deathChargeParticles.MinStartRadius = num3;
				_deathChargeParticles.MaxStartRadius = num4;
				_deathLazerParticles.MinStartRadius = num3;
				_deathLazerParticles.MaxStartRadius = num4;
				int x2 = Position.X;
				_ = IsFacingLeft;
				Vector2 where = new Vector2(x2, Position.Y + -8);
				_deathChargeParticles.AddParticles(where);
				_deathLazerParticles.AddParticles(where);
			}
			if (_deathScriptTimer >= 1.8f && deathScriptTimer < 1.8f)
			{
				_level.RequestScreenFlash(new ScreenFlash(0.4f)
				{
					Frequency = 1f,
					EffectColor = DeathLevelDrawColor
				});
			}
		}
		else if (deathScriptTimer <= 2f)
		{
			_leftWall.FadeOut();
			_rightWall.FadeOut();
			_ceilingSpikes.FadeOut();
			_level.SetCameraPosition(new Point(352, 136));
			_level.RequestScreenShake(new Vector2(5f, 0f), 1f, 24f, isAffectedByTime: false);
			SandmanDeathShrapnelParticleSystem particleSystem = new SandmanDeathShrapnelParticleSystem(_sprite, 1);
			ShockwaveAnimation shockwaveAnimation = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, Bbox.Center, _level, DeathShockwaveColor);
			shockwaveAnimation.ParticleSystem = particleSystem;
			ShockwaveAnimation newAnimation = shockwaveAnimation;
			_level.AddAnimation(newAnimation);
			SaveBossDeath();
			CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Temple1_BossEnd, _level, new Point(x, _level.RoomSize.Y / 2));
			RemoveInstance();
			_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
			_level.ToggleExits(isEnabled: true);
		}
		ParticleSystem[] deathParticleSystems = _deathParticleSystems;
		for (int i = 0; i < deathParticleSystems.Length; i++)
		{
			deathParticleSystems[i]?.Update(delta);
		}
	}

	internal override void InitializeForBestiary()
	{
		base.DrawColor = Color.White;
	}
}
