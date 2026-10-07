using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Events.Treasure;

namespace Timespinner.GameObjects.Bosses.RoboKitty;

internal sealed class RoboKittyBoss : BossClass
{
	private enum ERoboKittyGlobalState
	{
		None,
		Growl,
		AboveScreen,
		Falling,
		JumpingOffScreen,
		Crawl,
		PawAttack,
		TriplePawAttack,
		LazerAttack,
		GroundPound,
		Backdash
	}

	private const int OffscreenHeight = -25;

	private const int FloorHeight = 224;

	private const int LeftTooFarThreshold = 142;

	private const int RightTooFarThreshold = 256;

	private const int FallLeftPosition = 90;

	private const int FallRightPosition = 316;

	private const int TooFarRightBackdash = 354;

	private const int TooFarLeftBackdash = 48;

	private const int NearClawIdleOffsetX = -32;

	private const int FarClawIdleOffsetX = 8;

	private const int NearClawJumpWindupOffsetX = -40;

	private const int FarClawJumpWindupOffsetX = -32;

	private const int NearClawJumpingOffsetX = -40;

	private const int FarClawJumpingOffsetX = -32;

	private const int ClawJumpMaxOffsetY = 40;

	private const int ClawCrawlWidth = 48;

	private const int ClawCrawlHeight = 12;

	private const int ClawAttackOriginX = 48;

	private const int ClawAttackWindupRadius = 40;

	private const int ClawAttackWidth = 64;

	private const int ClawAttackRetractHeight = 4;

	private const int BouldersPerGroundPound = 1;

	private const int TotalGroundPounds = 5;

	private const int LeftBoulderFallPoint = 40;

	private const int RightBoulderFallPoint = 360;

	private const int CenterBoulderFallPoint = 216;

	private const float OscillXScale = 1f;

	private const float OscillYScale = 0.75f;

	private const float OscillFrequencyX = 4f;

	private const float OscillFrequencyY = 4f;

	private const float OscillRadius = 4f;

	private const float GroundSkidEffectTimerThreshold = 0.2f;

	private const float FrenzyThreshold = 0.5f;

	private const float DropPebbleThreshold = 0.5f;

	private const float FallSFXThreshold = 0.4f;

	private const float IndefiniteActionTime = 1000f;

	private const float TimeForOneClawToCrawl = 3f;

	private const float TimeForJumpWindup = 1f;

	private const float InitialTimeForKittyToBeOffscreen = 2f;

	private const float TimeForKittyToBeOffscreen = 1f;

	private const float TimeForGrowl = 1f;

	private const float TimeForGrowlSFXToWait = 0.2f;

	private const float TimeForOneClawToAttackWindup = 0.5f;

	private const float TimeForOneClawToAttackWait = 0.1f;

	private const float TimeForOneClawToAttackSwipe = 0.2f;

	private const float TimeForOneClawToAttackRetract = 0.25f;

	private const float TimeForOneClawToAttack = 1.05f;

	private const float TimeForOneClawToStartAttackAnimation = 0.35f;

	private const float TimeForLazerChargeUp = 1.25f;

	private const float TimeForLazerFiring = 1.25f;

	private const float TimeToTransitionIntoGroundPound = 0.5f;

	private const float TimeForOnePoundWindUp = 0.35f;

	private const float TimeForOnePoundSwipe = 0.1f;

	private const float TimeForOneEntirePound = 0.45f;

	private const float TimeForAllPounds = 2.75f;

	private const float TimeForDeathExplosions = 0.05f;

	private const float BaseHeadFollowPercentageSpeed = 0.1f;

	private static readonly Vector2 GroundPoundClawOffset = new Vector2(32f, -40f);

	private readonly Appendage _headAppendage;

	private readonly Appendage _nearClawAppendage;

	private readonly Appendage _farClawAppendage;

	private readonly PebblesParticleSystem _landingPebbleParticles;

	private readonly LandingDustParticleSystem _landingDustParticles;

	private readonly RoboKittyLazerChargeParticleSystem _lazerChargeParticles;

	private readonly BossDeathChargeParticleSystem _bossDeathChargeParticleSystem;

	private readonly BossDeathChargeLazerParticleSystem _bossDeathChargeLazerParticleSystem;

	private readonly BattleAnimation _lazerChargeAnimation;

	private readonly BattleAnimation _nearClawJetpackFlameAnimation;

	private readonly BattleAnimation _farClawJetpackFlameAnimation;

	private bool _wasLeftClawLastToGroundPound;

	private bool _hasGrowledYet;

	private bool _isDying;

	private bool _isReadyToDie;

	private ERoboKittyGlobalState _globalKittyState;

	private float _oscillDelta;

	private float _headOscillationAmount = 1f;

	private float _headFollowPercentageSpeed = 0.1f;

	private float _groundSkidEffectTimer;

	private float _currentGlobalStateTimer;

	private float _lastGlobalStateTimer;

	private float _deathExplosionTimer;

	private Point _headTargetOffset;

	private Point _nearClawTargetOffset;

	private Point _farClawTargetOffset;

	private Vector2 _headCurrentOffset;

	private Vector2 _nearClawCurrentOffset;

	private Vector2 _farClawCurrentOffset;

	private Color _deathColor;

	private SFXCueInstance _ribScrapeLoopSFXInstance;

	private OrbPedestalEvent _droppedBladeOrb;

	private Point HeadBasePosition => new Point(Position.X - (IsFacingLeft ? 68 : (-68)), Position.Y - 24);

	private Point ClawBasePosition => new Point(Position.X - (IsFacingLeft ? 40 : (-40)), Position.Y);

	private Point LazerEffectEmissionPosition => new Point(_headAppendage.Position.X + (IsFacingLeft ? (-4) : 4), _headAppendage.Position.Y - 8);

	private Point LazerChargeAnimationPosition => new Point(_headAppendage.Position.X + (IsFacingLeft ? (-5) : 5), _headAppendage.Position.Y - 9);

	public override Point AnchorPosition => new Point(base.LastPosition.X - Bbox.Width * (IsFacingLeft ? 1 : (-1)) / 2, base.LastPosition.Y - Bbox.Height / 2);

	public RoboKittyBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_level.JukeBox.FadeOutSong(0.5f);
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 1f;
		Bbox = new Rectangle(_position.X, _position.Y, 54, 54);
		Position = new Point(316, -25);
		_isAffectedByLevelBounds = false;
		_isAffectedByGravity = false;
		_maxFallSpeed = 600f;
		_maxJumpTime = 1000f;
		_doAppendagesMatchImageFacing = true;
		_deathParticlesColor = new Color(1f, 0.15f, 0f, 0.1f);
		_deathParticleColorVect = _deathParticlesColor.ToVector4();
		_doesDrawBaseSprite = false;
		_headAppendage = new Appendage(this, new Point(28, 35), new Point(10, 16), _level, _sprite)
		{
			FollowType = EAppendageFollowType.None,
			EndPointOffset = new Point(0, -8),
			Position = HeadBasePosition
		};
		_nearClawAppendage = new Appendage(this, new Point(24, 11), new Point(12, 8), _level, _sprite)
		{
			FollowType = EAppendageFollowType.None,
			Position = ClawBasePosition
		};
		_farClawAppendage = new Appendage(this, new Point(24, 11), new Point(12, 8), _level, _sprite)
		{
			FollowType = EAppendageFollowType.None,
			Position = ClawBasePosition
		};
		CreateAppendages();
		_landingDustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 100);
		_landingPebbleParticles = new PebblesParticleSystem(_level.GCM.TxBlankSquare, 10, _level.ID);
		_lazerChargeParticles = new RoboKittyLazerChargeParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_bossDeathChargeParticleSystem = new BossDeathChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
		_bossDeathChargeLazerParticleSystem = new BossDeathChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 2);
		_particleSystems.AddRange(new ParticleSystem[5] { _landingDustParticles, _landingPebbleParticles, _lazerChargeParticles, _bossDeathChargeParticleSystem, _bossDeathChargeLazerParticleSystem });
		_lazerChargeAnimation = new BattleAnimation(_sprite, _headAppendage.Position, _level)
		{
			TeamSide = ETeamSide.Enemies,
			DrawColor = new Color(0.25f, 1f, 0.4f) * 0.85f,
			DoesRepeat = true,
			AnimationSpeed = 0.033f,
			AnimationStart = 26,
			AnimationLength = 5
		};
		_nearClawJetpackFlameAnimation = new BattleAnimation(_sprite, Point.Zero, _level)
		{
			TeamSide = ETeamSide.Enemies,
			AnchorObject = _nearClawAppendage,
			AnchorOffset = new Point(0, 8),
			DrawColor = Color.White * 0.9f,
			DoesRepeat = true,
			AnimationSpeed = 0.033f,
			AnimationStart = 22,
			AnimationLength = 4
		};
		_farClawJetpackFlameAnimation = new BattleAnimation(_sprite, Point.Zero, _level)
		{
			TeamSide = ETeamSide.Enemies,
			AnchorObject = _farClawAppendage,
			AnchorOffset = new Point(0, 8),
			DrawColor = Color.White * 0.85f,
			DoesRepeat = true,
			AnimationSpeed = 0.033f,
			AnimationStart = 22,
			AnimationLength = 4
		};
	}

	private void CreateAppendages()
	{
		AddClaw(isNearClaw: false);
		Appendage appendage = new Appendage(this, new Point(54, 54), new Point(4, 4), _level, _sprite);
		appendage.FollowType = EAppendageFollowType.ParentObjectLocked;
		appendage.DrawPriority = -1;
		appendage.AnchorOffset = new Point(25, 27);
		Appendage appendage2 = appendage;
		appendage2.ChangeAnimation(8, 0, 1f, EAnimationType.None);
		_appendages.Add(appendage2);
		Appendage appendage3 = new Appendage(this, new Point(58, 56), new Point(3, 2), _level, _sprite);
		appendage3.FollowType = EAppendageFollowType.ParentObjectLocked;
		appendage3.AnchorOffset = new Point(15, 12);
		Appendage appendage4 = appendage3;
		appendage4.ChangeAnimation(7, 0, 1f, EAnimationType.None);
		_appendages.Add(appendage4);
		_headAppendage.ChangeAnimation(15, 0, 1f, EAnimationType.None);
		_headAppendage.AddLinks(6, EAppendageFollowType.TrigoInterpolate, new Point(12, 28), new Point(4, 2), 0, new Point(8, -6));
		_appendages.Add(_headAppendage);
		Appendage appendage5 = new Appendage(this, new Point(52, 41), new Point(2, 4), _level, _sprite);
		appendage5.FollowType = EAppendageFollowType.ParentObjectLocked;
		appendage5.AnchorOffset = new Point(12, 1);
		Appendage appendage6 = appendage5;
		appendage6.ChangeAnimation(1, 0, 1f, EAnimationType.None);
		_appendages.Add(appendage6);
		AddClaw(isNearClaw: true);
		Appendage appendage7 = new Appendage(this, new Point(44, 35), new Point(2, 9), _level, _sprite);
		appendage7.FollowType = EAppendageFollowType.ParentObjectLocked;
		appendage7.AnchorOffset = new Point(7, -4);
		Appendage appendage8 = appendage7;
		appendage8.ChangeAnimation(2, 0, 1f, EAnimationType.None);
		_appendages.Add(appendage8);
	}

	private void AddClaw(bool isNearClaw)
	{
		Appendage appendage = (isNearClaw ? _nearClawAppendage : _farClawAppendage);
		appendage.ChangeAnimation(isNearClaw ? 9 : 12, 0, 1f, EAnimationType.None);
		appendage.AddJointedLinks(new int[2]
		{
			isNearClaw ? 4 : 6,
			isNearClaw ? 3 : 5
		}, new Point[2]
		{
			new Point(1, 1),
			new Point(1, 1)
		}, new Point[2]
		{
			new Point(12, 12),
			new Point(14, 12)
		}, new Point[2]
		{
			new Point(12, -16),
			new Point(4, -3)
		}, new Vector2[2]
		{
			new Vector2(12f, 12f),
			new Vector2(14f, 12f)
		}, doHingesHaveAngularLimits: true);
		appendage.SetChildAppendageDrawPriorities(1);
		_appendages.Add(appendage);
	}

	protected override void StartBossIntroCutscene()
	{
		base.IsBossIntroInProgress = false;
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			_oscillDelta += delta;
			if (_oscillDelta > 6.28f)
			{
				_oscillDelta -= 6.28f;
			}
			UpdateHead(delta);
			UpdateClaws(delta);
			if (_groundSkidEffectTimer > 0f)
			{
				_groundSkidEffectTimer -= delta;
			}
			if (_groundSkidEffectTimer <= 0f && _isGrounded && Math.Abs(_velocity.X) > 1f)
			{
				EmitLandingDust(isFullBody: false);
				EmitPebbles(isFullBody: false, Position.Y);
				_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 0.2f, isAffectedByTime: true);
				_groundSkidEffectTimer = 0.2f;
				if (_ribScrapeLoopSFXInstance == null)
				{
					_ribScrapeLoopSFXInstance = PlayCue(ESFX.BossRoboKittyRibScrapeLoop, isLooped: true);
				}
				else if (_ribScrapeLoopSFXInstance.IsPaused || _ribScrapeLoopSFXInstance.IsFadingOut)
				{
					_ribScrapeLoopSFXInstance.Resume();
				}
			}
			if (Math.Abs(_velocity.X) < 1f && _ribScrapeLoopSFXInstance != null && !_ribScrapeLoopSFXInstance.IsPaused && !_ribScrapeLoopSFXInstance.IsFadingOut)
			{
				_ribScrapeLoopSFXInstance.Pause(0.1f);
			}
		}
		base.Update(delta);
	}

	private void UpdateHead(float delta)
	{
		double num = Math.Cos((0f - _oscillDelta) * 4f) * 4.0 * 1.0 * (double)_headOscillationAmount;
		double num2 = Math.Sin((0f - _oscillDelta) * 4f) * 4.0 * 0.75 * (double)_headOscillationAmount;
		_headCurrentOffset = MathEx.PercentageFollowPoint(_headTargetOffset, _headCurrentOffset, _headFollowPercentageSpeed, 4, delta);
		_headAppendage.Position = new Point((int)Math.Round((double)HeadBasePosition.X + num + (double)((int)_headCurrentOffset.X * ((!IsFacingLeft) ? 1 : (-1)))), (int)Math.Round((double)HeadBasePosition.Y + num2 + (double)(int)_headCurrentOffset.Y));
		if (_headOscillationAmount < 1f)
		{
			_headOscillationAmount += delta;
			if (_headOscillationAmount > 1f)
			{
				_headOscillationAmount = 1f;
			}
		}
	}

	private void UpdateClaws(float delta)
	{
		if (_currentAction == EAIAction.Idle)
		{
			_nearClawTargetOffset = new Point(-32, 0);
			_farClawTargetOffset = new Point(8, 0);
		}
		_nearClawCurrentOffset = MathEx.PercentageFollowPoint(_nearClawTargetOffset, _nearClawCurrentOffset, 0.2f, 4, delta);
		_farClawCurrentOffset = MathEx.PercentageFollowPoint(_farClawTargetOffset, _farClawCurrentOffset, 0.2f, 4, delta);
		_nearClawAppendage.Position = new Point(ClawBasePosition.X + (int)_nearClawCurrentOffset.X * ((!IsFacingLeft) ? 1 : (-1)), ClawBasePosition.Y + (int)_nearClawCurrentOffset.Y);
		_farClawAppendage.Position = new Point(ClawBasePosition.X + (int)_farClawCurrentOffset.X * ((!IsFacingLeft) ? 1 : (-1)), ClawBasePosition.Y + (int)_farClawCurrentOffset.Y);
		if (_nearClawAppendage.Position.Y > 224)
		{
			_nearClawAppendage.Position = new Point(_nearClawAppendage.Position.X, 224);
		}
		if (_farClawAppendage.Position.Y > 224)
		{
			_farClawAppendage.Position = new Point(_farClawAppendage.Position.X, 224);
		}
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_currentGlobalStateTimer = 0f;
		_lastGlobalStateTimer = -1E-07f;
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(_headAppendage.Position);
		bool flag = nearestProtagonistPosition.X < Position.X != IsFacingLeft;
		bool flag2 = !flag && Math.Abs(nearestProtagonistPosition.X - Position.X) < 144;
		if (_globalKittyState == ERoboKittyGlobalState.None)
		{
			_globalKittyState = ERoboKittyGlobalState.AboveScreen;
			_nextActionTimer = 2f;
			return;
		}
		if ((IsFacingLeft && Position.X < 142) || (!IsFacingLeft && Position.X > 256))
		{
			if (_globalKittyState != ERoboKittyGlobalState.JumpingOffScreen)
			{
				_nextActionTimer = 1000f;
				if ((flag || _globalKittyState == ERoboKittyGlobalState.PawAttack || _globalKittyState == ERoboKittyGlobalState.TriplePawAttack) && !_isReadyToDie)
				{
					_globalKittyState = ERoboKittyGlobalState.JumpingOffScreen;
					PlayCue(ESFX.BossRoboKittyPrelaunch, Position);
				}
				else
				{
					_globalKittyState = ((base.HPPercentage > 0.5f) ? ERoboKittyGlobalState.PawAttack : ERoboKittyGlobalState.TriplePawAttack);
				}
				return;
			}
			_globalKittyState = ERoboKittyGlobalState.AboveScreen;
			_nextActionTimer = 1f;
			bool flag3 = _level.NextRandomInt(0, 1) == 0;
			int x = (flag3 ? 90 : 316);
			IsFacingLeft = !flag3;
			Position = new Point(x, -25);
			_nearClawTargetOffset = new Point(-32, 0);
			_farClawTargetOffset = new Point(8, 0);
			_nearClawCurrentOffset = _nearClawTargetOffset.ToVector2();
			_farClawCurrentOffset = _farClawTargetOffset.ToVector2();
			return;
		}
		if (_globalKittyState == ERoboKittyGlobalState.AboveScreen)
		{
			_globalKittyState = ERoboKittyGlobalState.Falling;
			_nextActionTimer = 1000f;
			_lastActionTimer = _nextActionTimer;
			return;
		}
		if (!_hasGrowledYet)
		{
			_nextActionTimer = 1000f;
			_globalKittyState = ERoboKittyGlobalState.Growl;
			_hasGrowledYet = true;
			EndBossIntroCutscene();
			return;
		}
		_nextActionTimer = 1000f;
		if (flag)
		{
			_globalKittyState = ERoboKittyGlobalState.Backdash;
			return;
		}
		int num = _level.NextRandomInt((!flag2) ? 1 : 0, 3);
		if (base.HPPercentage > 0.5f)
		{
			switch (num)
			{
			case 0:
				_globalKittyState = ERoboKittyGlobalState.PawAttack;
				break;
			case 1:
				_globalKittyState = ERoboKittyGlobalState.Crawl;
				break;
			case 2:
				_globalKittyState = ERoboKittyGlobalState.LazerAttack;
				break;
			case 3:
				_globalKittyState = ERoboKittyGlobalState.GroundPound;
				break;
			}
		}
		else
		{
			switch (num)
			{
			case 0:
				_globalKittyState = ERoboKittyGlobalState.TriplePawAttack;
				break;
			case 1:
				_globalKittyState = ERoboKittyGlobalState.Crawl;
				break;
			case 2:
				_globalKittyState = ERoboKittyGlobalState.LazerAttack;
				break;
			case 3:
				_globalKittyState = ERoboKittyGlobalState.GroundPound;
				break;
			}
		}
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		_movementX = 0f;
		_isJumping = false;
		_headFollowPercentageSpeed = 0.1f;
		if (_isReadyToDie && (_globalKittyState == ERoboKittyGlobalState.None || _globalKittyState == ERoboKittyGlobalState.Backdash || _globalKittyState == ERoboKittyGlobalState.Crawl || _globalKittyState == ERoboKittyGlobalState.GroundPound || _globalKittyState == ERoboKittyGlobalState.Growl || _globalKittyState == ERoboKittyGlobalState.LazerAttack || _globalKittyState == ERoboKittyGlobalState.PawAttack || _globalKittyState == ERoboKittyGlobalState.TriplePawAttack))
		{
			_nearClawTargetOffset = new Point(-32, 0);
			_farClawTargetOffset = new Point(8, 0);
			ActuallyStartDeathScript();
		}
		else
		{
			switch (_globalKittyState)
			{
			case ERoboKittyGlobalState.AboveScreen:
				_isAffectedByGravity = false;
				if (_lastActionTimer > 0.5f && _nextActionTimer <= 0.5f)
				{
					EmitPebbles(isFullBody: true, 0);
				}
				break;
			case ERoboKittyGlobalState.Falling:
			{
				_isAffectedByGravity = true;
				float num3 = 1000f - _nextActionTimer;
				float num4 = 1000f - _lastActionTimer;
				if (num4 < 0.4f && num3 >= 0.4f)
				{
					PlayCue(ESFX.BossRoboKittyFall, Position);
				}
				if (_isGrounded)
				{
					PlayCue(ESFX.BossRoboKittyLand, Position);
					FinishAttack(0f);
				}
				else
				{
					_nearClawTargetOffset = new Point(-32, 40);
					_farClawTargetOffset = new Point(8, 40);
				}
				break;
			}
			case ERoboKittyGlobalState.JumpingOffScreen:
				UpdateJumpOffScreen();
				break;
			case ERoboKittyGlobalState.Crawl:
				UpdateCrawl();
				break;
			case ERoboKittyGlobalState.PawAttack:
				if (_currentGlobalStateTimer <= 1.05f)
				{
					_nearClawTargetOffset = UpdatePawAttack(_currentGlobalStateTimer, _lastGlobalStateTimer, isNear: true);
					if (_currentGlobalStateTimer >= 0.35f && _lastGlobalStateTimer < 0.35f)
					{
						_nearClawAppendage.ChangeAnimation(new List<AnimationSpec>
						{
							new AnimationSpec
							{
								Start = 9,
								Length = 3,
								Type = EAnimationType.Once,
								Speed = 0.066f
							},
							new AnimationSpec
							{
								Start = 9,
								Length = 3,
								Type = EAnimationType.Once,
								Speed = 0.066f,
								IsInReverse = true
							}
						});
					}
				}
				else
				{
					FinishAttack();
				}
				break;
			case ERoboKittyGlobalState.TriplePawAttack:
				if (_currentGlobalStateTimer <= 1.05f)
				{
					_nearClawTargetOffset = UpdatePawAttack(_currentGlobalStateTimer, _lastGlobalStateTimer, isNear: true);
					if (_currentGlobalStateTimer >= 0.35f && _lastGlobalStateTimer < 0.35f)
					{
						_nearClawAppendage.ChangeAnimation(new List<AnimationSpec>
						{
							new AnimationSpec
							{
								Start = 9,
								Length = 3,
								Type = EAnimationType.Once,
								Speed = 0.066f
							},
							new AnimationSpec
							{
								Start = 9,
								Length = 3,
								Type = EAnimationType.Once,
								Speed = 0.066f,
								IsInReverse = true
							}
						});
					}
				}
				else if (_currentGlobalStateTimer <= 2.1f)
				{
					float num = _currentGlobalStateTimer - 1.05f;
					float num2 = _lastGlobalStateTimer - 1.05f;
					_farClawTargetOffset = UpdatePawAttack(num, num2, isNear: false);
					if (num >= 0.35f && num2 < 0.35f)
					{
						_farClawAppendage.ChangeAnimation(new List<AnimationSpec>
						{
							new AnimationSpec
							{
								Start = 12,
								Length = 3,
								Type = EAnimationType.Once,
								Speed = 0.066f
							},
							new AnimationSpec
							{
								Start = 12,
								Length = 3,
								Type = EAnimationType.Once,
								Speed = 0.066f,
								IsInReverse = true
							}
						});
					}
				}
				else
				{
					FinishAttack();
				}
				break;
			case ERoboKittyGlobalState.LazerAttack:
				UpdateLazerAttack(delta);
				break;
			case ERoboKittyGlobalState.GroundPound:
				UpdateGroundPound();
				break;
			case ERoboKittyGlobalState.Growl:
				UpdateGrowl(delta);
				break;
			case ERoboKittyGlobalState.Backdash:
				UpdateBackdash();
				break;
			default:
				_nextActionTimer = 1f;
				_currentAction = EAIAction.Idle;
				break;
			}
		}
		_lastGlobalStateTimer = _currentGlobalStateTimer;
		_currentGlobalStateTimer += delta;
	}

	private void UpdateJumpOffScreen()
	{
		if (Position.Y < -100)
		{
			FinishAttack(0f);
			_battleAnimations.Clear();
			return;
		}
		float num = _currentGlobalStateTimer / 1f;
		if (num <= 1f)
		{
			_nearClawTargetOffset = new Point(-40, 0);
			_farClawTargetOffset = new Point(-32, 0);
			return;
		}
		if (_lastGlobalStateTimer <= 1f)
		{
			_battleAnimations.Add(_farClawJetpackFlameAnimation);
			_battleAnimations.Add(_nearClawJetpackFlameAnimation);
			PlayCue(ESFX.BossRoboKittyLaunch, Position);
		}
		_isJumping = true;
		_nearClawTargetOffset = new Point(-40, 40);
		_farClawTargetOffset = new Point(-32, 40);
	}

	private void UpdateCrawl()
	{
		if ((IsFacingLeft && Position.X < 142) || (!IsFacingLeft && Position.X > 256))
		{
			FinishAttack();
			return;
		}
		float value = _currentGlobalStateTimer / 3f + 0.25f;
		float value2 = _lastGlobalStateTimer / 3f + 0.25f;
		float num = value.Mod(1f);
		value2 = value2.Mod(1f);
		if (value2 < 0.23f && num >= 0.23f)
		{
			PlayCue(ESFX.BossRoboKittyLegMoveL, _nearClawAppendage.Position);
		}
		else if (value2 < 0.73f && num >= 0.73f)
		{
			PlayCue(ESFX.BossRoboKittyLegMoveR, _farClawAppendage.Position);
		}
		double num2 = Math.Cos(num * ((float)Math.PI * 2f));
		double num3 = Math.Sin(num * ((float)Math.PI * 2f));
		int x = -(int)(num2 * 48.0);
		int num4 = -(int)(num3 * 12.0);
		_nearClawTargetOffset = new Point(x, (num < 0.5f) ? num4 : 0);
		num2 = Math.Cos(num * ((float)Math.PI * 2f) + (float)Math.PI);
		num3 = Math.Sin(num * ((float)Math.PI * 2f) + (float)Math.PI);
		x = -(int)(num2 * 48.0);
		num4 = -(int)(num3 * 12.0);
		_farClawTargetOffset = new Point(x, (num > 0.5f) ? num4 : 0);
		if ((num > 0.585f && num < 0.95f) || (num > 0.085f && num < 0.45f))
		{
			_movementX = (IsFacingLeft ? (-0.25f) : 0.25f);
		}
		if (num > 0.53499997f && value2 <= 0.53499997f)
		{
			PlayCue(ESFX.BossRoboKittyPawDownL, _nearClawAppendage.Position);
		}
		else if (num > 0.035f && value2 <= 0.035f)
		{
			PlayCue(ESFX.BossRoboKittyPawDownR, _farClawAppendage.Position);
		}
	}

	private void UpdateLazerAttack(float delta)
	{
		if (_headOscillationAmount > 0f)
		{
			_headOscillationAmount -= delta * 3f;
			if (_headOscillationAmount < 0f)
			{
				_headOscillationAmount = 0f;
			}
		}
		if (_currentGlobalStateTimer <= 1.25f)
		{
			if (_currentGlobalStateTimer <= 0f)
			{
				_headAppendage.ChangeAnimation(15, 3, 0.09f, EAnimationType.Once);
				PlayCue(ESFX.BossRoboKittyLazer, _headAppendage.Position);
				_battleAnimations.Add(_lazerChargeAnimation);
			}
			_headTargetOffset = new Point(8, -4);
			_lazerChargeAnimation.Position = LazerChargeAnimationPosition;
			_lazerChargeParticles.AddParticles(LazerEffectEmissionPosition.ToVector2(), IsFacingLeft);
		}
		else if (_lastGlobalStateTimer <= 1.25f && _currentGlobalStateTimer > 1.25f)
		{
			_level.AddProjectile(new RoboKittyHorizontalLazer(_level, _headAppendage.Position, new Vector2((!IsFacingLeft) ? 1 : (-1), 0f), ETeamSide.Enemies, _headAppendage, new Point(-4, -4), base.Damage));
			_battleAnimations.Remove(_lazerChargeAnimation);
		}
		else if (_currentGlobalStateTimer > 2.5f)
		{
			_headTargetOffset = Point.Zero;
			FinishAttack();
			_headAppendage.ChangeAnimation(new List<AnimationSpec>
			{
				new AnimationSpec
				{
					Start = 15,
					Length = 3,
					Speed = 0.066f,
					Type = EAnimationType.Once,
					IsInReverse = true
				}
			});
		}
	}

	private void UpdateGroundPound()
	{
		if (_currentGlobalStateTimer <= 0f)
		{
			PlayCue(ESFX.BossRoboKittyPreGroundPound);
		}
		if (_currentGlobalStateTimer <= 2.75f)
		{
			Vector2 vector = new Vector2(GroundPoundClawOffset.X + 8f, 0f);
			if (_currentGlobalStateTimer <= 0.5f)
			{
				float amount = _currentGlobalStateTimer / 0.5f;
				Point end = vector.ToPoint();
				_farClawTargetOffset = _farClawTargetOffset.Lerp(end, amount);
				_nearClawTargetOffset = _nearClawTargetOffset.Lerp(end, amount);
				return;
			}
			float num = _currentGlobalStateTimer - 0.5f;
			float num2 = num.Mod(0.45f);
			float num3 = (_lastGlobalStateTimer - 0.5f).Mod(0.45f);
			vector = Vector2.Lerp(amount: (num2 < num3) ? 0f : ((!(num2 <= 0.35f)) ? (1f - (num2 - 0.35f) / 0.1f) : (1f - (0.35f - num2) / 0.35f)), value1: vector, value2: GroundPoundClawOffset);
			if (_wasLeftClawLastToGroundPound)
			{
				_farClawTargetOffset = vector.ToPoint();
			}
			else
			{
				_nearClawTargetOffset = vector.ToPoint();
			}
			if (num2 < num3)
			{
				DropRubble(1);
				_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: true);
				Vector2 vector2 = (_wasLeftClawLastToGroundPound ? _farClawAppendage.Position.ToVector2() : _nearClawAppendage.Position.ToVector2());
				vector2.Y = 224f;
				_landingPebbleParticles.AddParticles(vector2);
				_landingDustParticles.AddParticles(vector2);
				PlayCue(_wasLeftClawLastToGroundPound ? ESFX.BossRoboKittyPawDownR : ESFX.BossRoboKittyPawDownL, vector2.ToPoint());
				_wasLeftClawLastToGroundPound = !_wasLeftClawLastToGroundPound;
				if (num < 0.9f)
				{
					PlayCue(ESFX.BossRoboKittyGroundPound, Position);
				}
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdateGrowl(float delta)
	{
		if (_headOscillationAmount > 0f)
		{
			_headOscillationAmount -= delta * 3f;
			if (_headOscillationAmount < 0f)
			{
				_headOscillationAmount = 0f;
			}
		}
		if (_currentGlobalStateTimer <= 1f)
		{
			if (_currentGlobalStateTimer == 0f)
			{
				_headAppendage.ChangeAnimation(15, 3, 0.09f, EAnimationType.Once);
			}
			else if (_currentGlobalStateTimer >= 0.2f && _lastGlobalStateTimer < 0.2f)
			{
				PlayCue(ESFX.BossRoboKittyMeow, _headAppendage.Position);
			}
			bool flag = (int)(_currentGlobalStateTimer * 10f) % 2 == 0;
			_headTargetOffset = new Point(8, -4 + (flag ? (-4) : 4));
		}
		else
		{
			_headTargetOffset = Point.Zero;
			FinishAttack(0.25f);
			_headAppendage.ChangeAnimation(new List<AnimationSpec>
			{
				new AnimationSpec
				{
					Start = 15,
					Length = 3,
					Speed = 0.066f,
					Type = EAnimationType.Once,
					IsInReverse = true
				}
			});
		}
	}

	private void UpdateBackdash()
	{
		if ((IsFacingLeft && Position.X > 354) || (!IsFacingLeft && Position.X < 48))
		{
			FinishAttack();
			return;
		}
		float value = _currentGlobalStateTimer / 3f + 0.25f;
		float value2 = _lastGlobalStateTimer / 3f + 0.25f;
		float num = value.Mod(1f);
		value2 = value2.Mod(1f);
		if (value2 < 0.23f && num >= 0.23f)
		{
			PlayCue(ESFX.BossRoboKittyLegMoveL, _nearClawAppendage.Position);
		}
		double num2 = Math.Cos(num * ((float)Math.PI * 2f));
		double num3 = Math.Sin(num * ((float)Math.PI * 2f));
		int x = (int)(num2 * 48.0);
		int num4 = -(int)(num3 * 12.0);
		_nearClawTargetOffset = new Point(x, (num < 0.5f) ? num4 : 0);
		num2 = Math.Cos(num * ((float)Math.PI * 2f) - 0.2f);
		num3 = Math.Sin(num * ((float)Math.PI * 2f) - 0.2f);
		x = (int)(num2 * 48.0);
		num4 = -(int)(num3 * 12.0);
		_farClawTargetOffset = new Point(x, (num < 0.5f) ? num4 : 0);
		if (num > 0.585f && num < 0.95f)
		{
			_movementX = (IsFacingLeft ? 0.25f : (-0.25f));
		}
		if (num > 0.53499997f && value2 <= 0.53499997f)
		{
			PlayCue(ESFX.BossRoboKittyPawDownL, _nearClawAppendage.Position);
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
		_nearClawTargetOffset = new Point(-32, 0);
		_farClawTargetOffset = new Point(8, 0);
	}

	private Point UpdatePawAttack(float timeIntoAttack, float lastTimeIntoAttack, bool isNear)
	{
		if (timeIntoAttack >= 0f && lastTimeIntoAttack < 0f)
		{
			if (isNear)
			{
				_nearClawAppendage.PlayCue(ESFX.BossRoboKittyClaw);
			}
			else
			{
				_farClawAppendage.PlayCue(ESFX.BossRoboKittyClawFar);
			}
		}
		Point result;
		if (timeIntoAttack <= 0.5f)
		{
			float num = timeIntoAttack / 0.5f;
			double num2 = Math.Cos(num * ((float)Math.PI / 2f));
			double num3 = Math.Sin(num * ((float)Math.PI / 2f));
			int x = (int)(num2 * 40.0);
			int y = -(int)(num3 * 40.0);
			result = new Point(x, y);
		}
		else if (timeIntoAttack <= 0.6f)
		{
			result = new Point(40, -40);
		}
		else if (timeIntoAttack <= 0.8f)
		{
			float num4 = timeIntoAttack - 0.5f;
			float num5 = num4 / 0.2f;
			double num6 = Math.Sin(num5 * ((float)Math.PI / 2f) + 0f);
			double num7 = Math.Cos(num5 * ((float)Math.PI / 2f) + 0f);
			int x2 = (int)(num6 * 64.0) + 48 - 40;
			int y2 = -(int)(num7 * 40.0);
			result = new Point(x2, y2);
		}
		else
		{
			float num8 = timeIntoAttack - 0.7f;
			float num9 = num8 / 0.25f;
			double num10 = Math.Sin(num9 * ((float)Math.PI / 2f) + 0f);
			double num11 = Math.Sin(num9 * (float)Math.PI + 0f);
			int x3 = (int)((1.0 - num10) * 72.0);
			int y3 = -(int)(num11 * 4.0);
			result = new Point(x3, y3);
		}
		return result;
	}

	public override void UpdateAbility(float delta)
	{
	}

	protected override void DoLandingAction()
	{
		_level.RequestScreenShake(new Vector2(0f, 5f), 0.4f, 6f, isAffectedByTime: true);
		EmitLandingDust(isFullBody: true);
		EmitPebbles(isFullBody: true, Position.Y);
		base.DoLandingAction();
	}

	private void EmitPebbles(bool isFullBody, int verticalPosition)
	{
		int num = (IsFacingLeft ? 1 : (-1));
		_landingPebbleParticles.AddParticles(new Vector2(Position.X, verticalPosition));
		if (isFullBody)
		{
			_landingPebbleParticles.AddParticles(new Vector2(Position.X - 32 * num, verticalPosition));
			_landingPebbleParticles.AddParticles(new Vector2(Position.X - 64 * num, verticalPosition));
			_landingPebbleParticles.AddParticles(new Vector2(Position.X - 80 * num, verticalPosition));
		}
		else
		{
			Point point = ((_nearClawCurrentOffset.Y == 0f) ? _nearClawAppendage.Position : _farClawAppendage.Position);
			point.X += 12 * ((!IsFacingLeft) ? 1 : (-1));
			_landingPebbleParticles.AddParticles(new Vector2(point.X, point.Y));
		}
	}

	private void EmitLandingDust(bool isFullBody)
	{
		int num = (IsFacingLeft ? 1 : (-1));
		int num2 = (isFullBody ? 200 : 0);
		_landingDustParticles.AddParticles(new Vector2(Position.X, Position.Y), num2);
		if (isFullBody)
		{
			_landingDustParticles.AddParticles(new Vector2(Position.X - 32 * num, Position.Y), num2);
			_landingDustParticles.AddParticles(new Vector2(Position.X - 64 * num, Position.Y), num2);
			_landingDustParticles.AddParticles(new Vector2(Position.X - 80 * num, Position.Y), num2);
		}
		else
		{
			Point point = ((_nearClawCurrentOffset.Y == 0f) ? _nearClawAppendage.Position : _farClawAppendage.Position);
			point.X += 12 * ((!IsFacingLeft) ? 1 : (-1));
			_landingDustParticles.AddParticles(new Vector2(point.X, point.Y), num2);
		}
	}

	private void DropRubble(int amount)
	{
		bool flag = _level.GetNearestProtagonistPosition(_headAppendage.Position).X < Position.X;
		int start = (flag ? 40 : 184);
		int end = (flag ? 248 : 360);
		for (int i = 0; i < amount; i++)
		{
			_level.AddProjectile(new RoboKittyRubble(new Point(_level.NextRandomInt(start, end), 0), _level, _sprite, -1, base.Damage));
		}
	}

	protected override void StartDeathScript()
	{
		if (!_isReadyToDie)
		{
			_isReadyToDie = true;
			_movementX = 0f;
			_lazerChargeAnimation.Kill();
			_lazerChargeParticles.KillOffParticles(0.1f);
			GiveExperience();
			_level.RequestScreenFlash(new ScreenFlash(0.2f)
			{
				Frequency = 2f
			});
			_level.TogglePlayerIsInvulnerable(isInvulnerable: true);
			_level.PlayCue(ESFX.BossDeathFinalHit);
		}
	}

	private void ActuallyStartDeathScript()
	{
		SaveBossDeath();
		_level.JukeBox.FadeOutSong(4f);
		base.DeathPosition = Bbox.Center;
		_isFinallyDead = true;
		_isRunningDeathScript = true;
	}

	protected override void UpdateDeathScript(float delta)
	{
		_isDying = true;
		if (base.IsFrozen)
		{
			return;
		}
		_nearClawTargetOffset.Y = 224;
		_farClawTargetOffset.Y = 224;
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		Vector2 where = new Vector2(_bbox.Center.X, _bbox.Center.Y);
		if (deathScriptTimer == 0f)
		{
			_deathParticleColorVect = _deathParticlesColor.ToVector4();
			_deathScriptTimer = 1E-06f;
			_bossDeathChargeParticleSystem.BaseColor = _deathParticleColorVect;
			_bossDeathChargeLazerParticleSystem.BaseColor = _deathParticleColorVect;
			_bossDeathChargeParticleSystem.AddParticles(where);
			_bossDeathChargeLazerParticleSystem.AddParticles(where);
		}
		else if (_deathScriptTimer < 2f)
		{
			_bossDeathChargeParticleSystem.AddParticles(where);
			_bossDeathChargeLazerParticleSystem.AddParticles(where);
			_bossDeathChargeParticleSystem.AddParticles(_headAppendage.Position.ToVector2());
			_bossDeathChargeLazerParticleSystem.AddParticles(_headAppendage.Position.ToVector2());
			float num = _deathScriptTimer / 1f;
			if (num > 1f)
			{
				num = 1f;
			}
			Vector4 vector = Color.White.ToVector4().Lerp(_deathParticleColorVect, num);
			_deathColor = new Color(vector.X, vector.Y, vector.Z, 1f - num);
			_deathExplosionTimer += delta;
			if (_deathExplosionTimer > 0.05f)
			{
				_deathExplosionTimer -= 0.05f;
				int num2 = _random.Next(base.OuterBbox.Left, base.OuterBbox.Right);
				int y = _random.Next(base.OuterBbox.Top, base.OuterBbox.Bottom);
				Point position = new Point(num2, y);
				_level.AddAnimation(EBattleAnimationType.Boom, position, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
				if (num2 % 2 == 0)
				{
					PlayCue(ESFX.EnemyWormFlowerSeedBoom, position, isLooped: false, 0.5f);
				}
				else
				{
					PlayCue(ESFX.FoleyExplosionLarge, position, isLooped: false, 0.5f);
				}
			}
			if (_deathScriptTimer + 0.5f >= 2f && deathScriptTimer + 0.5f < 2f)
			{
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
			_deathScriptTimer = 2.000001f;
			_doesDrawSpriteAndAppendages = false;
			_doesDrawTrail = false;
			_doesDrawBrushTrail = false;
			base.DoesDrawAura = false;
			DropKittyLoot();
			_bossDeathChargeLazerParticleSystem.KillOffParticles(0.25f);
			_bossDeathChargeParticleSystem.BaseColor = _deathParticleColorVect;
			_bossDeathChargeParticleSystem.AddParticles(where);
		}
		else if (_deathScriptTimer < 4f)
		{
			if (_appendages.Any())
			{
				DebrisEvent.CreateFromObject(this, Vector2.One, Bbox.Center, _sprite);
				_appendages.Clear();
			}
		}
		else
		{
			if (!(_deathScriptTimer >= 4f))
			{
				return;
			}
			if (deathScriptTimer < 4f)
			{
				SaveBossDeath();
			}
			if (_droppedBladeOrb == null || _droppedBladeOrb.HasBeenPickedUp)
			{
				AddWaitScript(0.1f);
				_level.AddScript(new ScriptAction
				{
					ScriptType = EScriptType.Delegate,
					Delegate = delegate
					{
						BossDeathOpenDoors(shouldPlaySong: true);
					}
				});
				RemoveInstance();
			}
		}
	}

	private void DropKittyLoot()
	{
		DropLoot();
		Point position = _headAppendage.Position.Add(0, 16);
		_droppedBladeOrb = PlaceBladeOrb(position, _level);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isDying)
		{
			_doesDrawParticleSystems = false;
			_level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(3f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
			base.DrawColor = _deathColor;
		}
		base.Draw(spriteBatch);
		if (_isDying)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
			_doesDrawParticleSystems = true;
			DrawParticleSystems(spriteBatch);
		}
	}

	internal static OrbPedestalEvent PlaceBladeOrb(Point position, Level level)
	{
		OrbPedestalEvent orbPedestalEvent = new OrbPedestalEvent(level, position, -1, new ObjectTileSpecification(480)
		{
			Argument = 2
		});
		orbPedestalEvent.Initialize();
		orbPedestalEvent.RemoteSilentKill();
		level.RequestAddObject(orbPedestalEvent);
		return orbPedestalEvent;
	}

	internal override void InitializeForBestiary()
	{
		Update(1f);
	}
}
