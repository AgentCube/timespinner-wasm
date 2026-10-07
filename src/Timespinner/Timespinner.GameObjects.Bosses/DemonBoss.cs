using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Demon;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.Items;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Bosses;

internal sealed class DemonBoss : BossClass
{
	private enum EDemonBossState
	{
		None,
		Appear,
		Drop,
		ExtendPlatforms,
		RetractPlatforms,
		RedRover,
		Spotlight
	}

	private enum EDemonBossEyeAnimationType
	{
		Normal,
		Surprised,
		Happy,
		Angry
	}

	private const int FallHeight = 64;

	private const int PrefallRiseHeight = 8;

	private const int EyeNormalAnimationStart = 7;

	private const int EyeSurprisedAnimationStart = 8;

	private const int EyeHappyAnimationStart = 10;

	private const int EyeTrackingOffsetY = -24;

	private const int EyeTrackingRatioWidth = 160;

	private const int EyeTrackingRatioHeight = 128;

	private const int EyeMovementWidth = 3;

	private const int EyeMovementHeight = 3;

	private const float TimeBeforeUpdatingEyeTracking = 0.15f;

	private const float EyeAnimatonSpeed = 0.15f;

	private const float TimeToFadeToBlack = 1f;

	private const float TimeToWaitInBlack = 0.5f;

	private const float TimeToFadeIn = 1f;

	private const float TimeBeforeFadingIn = 1.5f;

	private const float TotalTimeToAppear = 2.5f;

	private const float TimeToRiseBeforeDropping = 0.35f;

	private const float TimeToDrop = 0.45f;

	private const float TimeToStayOnGround = 1.5f;

	private const float TimeToRiseBackUp = 1f;

	private const float TimeBeforeStayingOnGround = 0.79999995f;

	private const float TimeBeforeRisingBackUp = 2.3f;

	private const float TimeForEntireDropAttack = 3.3f;

	private const float TimeForRedRoverAttack = 3.5f;

	private const float PlayerUnderneathCrushTimerThreshold = 0.5f;

	private const float PlayerOnPlatformRetractTimerThreshold = 0.5f;

	private const float TimeBeforeRetractingPlatformsAfterExtendingSpikes = 0.2f;

	private const float TimeForEntireRetractPlatformSequence = 2.2f;

	private const int SpotlightPositionY = 224;

	private const float SpotlightFollowMultiplier = 4f;

	private const float SpotlightMaxFollowSpeed = 2f;

	private const float TimeForSpotlightPreFade = 1f;

	private const float TimeForSpotlightToAppear = 0.5f;

	private const float TimeForSpotlightToWait = 1f;

	private const float TimeForSpotlightKnives = 1.5f;

	private const float TimeForSpotlightToFade = 0.75f;

	private const float TimeForSpotlightEndingFadeIn = 0.75f;

	private const float TimeBeforeSpotlightKnives = 2.5f;

	private const float TimeBeforeSpotlightFadingAfterKnives = 4f;

	private const float TimeBeforeSpotlightEndingFadeIn = 4.75f;

	private const float TimeForEntireSpotlightAttack = 5.5f;

	private const int DeathParticlesOffsetY = -64;

	private const float TimeForDeathChargeUp = 2.5f;

	private const float TimeForDeathExplosion = 2f;

	private const float TimeForEntireDeathSequence = 4.5f;

	private const float FrenzyHPThreshold = 0.5f;

	private const float DefaultWaitTime = 1f;

	private const float IndefiniteActionTime = 1000f;

	private static readonly Point BaseEyeAnchorOffset = new Point(2, -1);

	private static readonly Vector2 DustEmissionPosition = new Vector2(200f, 224f);

	private static readonly Rectangle PlayerCrushTriggerArea = new Rectangle(160, 176, 80, 48);

	private static readonly Rectangle PlayerCrushTriggerAreaMid = new Rectangle(128, 80, 144, 80);

	private static readonly Color SpotlightDisabledColor = new Color(64, 64, 64);

	private static readonly Color SpotlightFinalColor = Color.White * 0.85f;

	private static readonly Color BaseGlowColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);

	private readonly int _baseTouchDamage;

	private readonly Point _startingPosition;

	private readonly LandingDustParticleSystem _dustParticles;

	private readonly DemonBossDeathChargeLazerPS _deathLazerParticles;

	private readonly Appendage _rightEye;

	private readonly Appendage _leftEye;

	private readonly DemonPuppet _leftPuppet;

	private readonly DemonPuppet _rightPuppet;

	private readonly DemonBossSawblade _sawblade;

	private readonly DemonBossWallSpikes _spikesLeftWall;

	private readonly DemonBossWallSpikes _spikesRightWall;

	private readonly DemonBossPlatform _topLeftPlatform;

	private readonly DemonBossPlatform _bottomLeftPlatform;

	private readonly DemonBossPlatform _topRightPlatform;

	private readonly DemonBossPlatform _bottomRightPlatform;

	private readonly DemonBossPlatform[] _platforms = new DemonBossPlatform[4];

	private readonly Appendage _spotlightAppendage;

	private readonly Appendage[] _eyes = new Appendage[2];

	private bool _isPlayerStandingOnLeftPlatform;

	private bool _areEyesTrackingPlayer;

	private bool _wasFrenzied;

	private EDemonBossState _demonBossState;

	private EDemonBossState _lastDemonBossState;

	private EDemonBossEyeAnimationType _rightEyeAnimationState;

	private EDemonBossEyeAnimationType _leftEyeAnimationState;

	private float _demonStateTimer;

	private float _lastDemonStateTimer;

	private float _playerStandingUnderneathTimer;

	private float _playerStandingOnPlatformTimer;

	private float _eyeTrackingTimer;

	private Point _targetEyeAnchorOffset;

	private Vector2 _currentEyeAnchorOffset;

	private SFXCueInstance _spotlightLoopCueInstance;

	private Mobile _eyeTrackingTarget;

	private GameEvent _bossDoor;

	public DemonBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_baseTouchDamage = _damageCaused;
		_startingPosition = Position;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_demonBossState = EDemonBossState.Appear;
		_lastDemonBossState = EDemonBossState.None;
		_nextActionTimer = 100f;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		base.DoesDrawBaseSprite = false;
		_doesUseAppendageCollision = true;
		_doAppendagesInheritDrawColor = true;
		base.DoesCollideWithTiles = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_doAppendagesMatchImageFacing = true;
		base.CannotBeGrabbed = true;
		_dustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 100);
		_particleSystems.Add(_dustParticles);
		_deathLazerParticles = new DemonBossDeathChargeLazerPS(_level.GCM.TxParticleEnergy, 5);
		_areEyesTrackingPlayer = true;
		_rightEye = base.Appendages[0].Appendages[0];
		_leftEye = base.Appendages[1].Appendages[0];
		_eyes[0] = _rightEye;
		_eyes[1] = _leftEye;
		Color auraColor = new Color(255, 60, 20);
		Appendage[] eyes = _eyes;
		foreach (Appendage appendage in eyes)
		{
			appendage.DoesDrawBaseSprite = false;
			appendage.DoesDrawAura = true;
			appendage.AuraColor = auraColor;
			appendage.AuraOffset = new Vector2(0f, 0f);
			appendage.AuraSize = 0f;
			appendage.AuraFrequency = 4f;
		}
		ObjectTileSpecification objectSpec2 = new ObjectTileSpecification(objectSpec.ID)
		{
			Argument = 1,
			ObjectID = objectSpec.ObjectID,
			Category = EObjectTileCategory.Enemy
		};
		_leftPuppet = new DemonPuppet(Position, _level, _sprite, -1, objectSpec2);
		ObjectTileSpecification objectSpec3 = new ObjectTileSpecification
		{
			Argument = 2,
			ObjectID = objectSpec.ObjectID,
			IsFlippedHorizontally = true,
			Category = EObjectTileCategory.Enemy
		};
		_rightPuppet = new DemonPuppet(Position, _level, _sprite, -1, objectSpec3);
		_spikesLeftWall = new DemonBossWallSpikes(_level, Position, isFacingLeft: false, _sprite, new ObjectTileSpecification());
		_spikesRightWall = new DemonBossWallSpikes(_level, Position, isFacingLeft: true, _sprite, new ObjectTileSpecification());
		_topLeftPlatform = new DemonBossPlatform(_level, Position, isOnBottomRow: false, isFacingLeft: false, _sprite, new ObjectTileSpecification());
		_bottomLeftPlatform = new DemonBossPlatform(_level, Position, isOnBottomRow: true, isFacingLeft: false, _sprite, new ObjectTileSpecification());
		_topRightPlatform = new DemonBossPlatform(_level, Position, isOnBottomRow: false, isFacingLeft: true, _sprite, new ObjectTileSpecification());
		_bottomRightPlatform = new DemonBossPlatform(_level, Position, isOnBottomRow: true, isFacingLeft: true, _sprite, new ObjectTileSpecification());
		_platforms[0] = _topLeftPlatform;
		_platforms[1] = _bottomLeftPlatform;
		_platforms[2] = _topRightPlatform;
		_platforms[3] = _bottomRightPlatform;
		_spotlightAppendage = new Appendage(this, new Point(46, 228), Point.Zero, _level, _sprite)
		{
			DoesDrawBaseSprite = false,
			DoesDrawAura = true,
			AuraOffset = new Vector2(0f, 0f),
			AuraSize = 0.015f,
			AuraFrequency = 1f,
			DoesInheritDrawColor = false,
			DoesCollideWithAnything = false,
			DrawPriority = 1
		};
		_spotlightAppendage.ChangeAnimation(27);
		_sawblade = new DemonBossSawblade(_level, _spotlightAppendage, _sprite, base.Damage);
		UpdateAppearSequence();
	}

	public override void InitializeMob()
	{
		_level.RequestAddObject(_leftPuppet);
		_level.RequestAddObject(_rightPuppet);
		_level.RequestAddObject(_spikesLeftWall);
		_level.RequestAddObject(_spikesRightWall);
		_level.RequestAddObject(_topLeftPlatform);
		_level.RequestAddObject(_bottomLeftPlatform);
		_level.RequestAddObject(_topRightPlatform);
		_level.RequestAddObject(_bottomRightPlatform);
		base.IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected override void EndBossIntroCutscene()
	{
		base.IsBossIntroInProgress = false;
		base.IsDormant = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdatePlayerStandingTimers(delta);
		}
		UpdateEyes(delta);
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_demonStateTimer = 0f;
		_lastDemonStateTimer = -1E-07f;
		EDemonBossState demonBossState = _demonBossState;
		_nextActionTimer = 1000f;
		bool flag = base.HPPercentage <= 0.5f;
		if (flag && !_wasFrenzied)
		{
			_demonBossState = EDemonBossState.Spotlight;
		}
		else if (_playerStandingUnderneathTimer >= 0.5f && _lastDemonBossState != EDemonBossState.Drop)
		{
			_demonBossState = EDemonBossState.Drop;
		}
		else if (_playerStandingOnPlatformTimer >= 0.5f && IsPlayerStillOnPlatformSideOfRoom())
		{
			_demonBossState = EDemonBossState.RetractPlatforms;
		}
		else
		{
			bool flag2 = false;
			DemonBossPlatform[] platforms = _platforms;
			foreach (DemonBossPlatform demonBossPlatform in platforms)
			{
				if (demonBossPlatform.IsRetracted)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				_demonBossState = EDemonBossState.ExtendPlatforms;
			}
			else
			{
				bool flag3 = !_leftPuppet.IsPlayingDead || !_rightPuppet.IsPlayingDead;
				if (!flag)
				{
					_demonBossState = (flag3 ? EDemonBossState.RedRover : EDemonBossState.Drop);
				}
				else
				{
					_demonBossState = ((_lastDemonBossState != EDemonBossState.Spotlight) ? EDemonBossState.Spotlight : EDemonBossState.RedRover);
				}
			}
		}
		_lastDemonBossState = demonBossState;
		_wasFrenzied = flag;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_demonBossState)
		{
		case EDemonBossState.Appear:
			UpdateAppearSequence();
			break;
		case EDemonBossState.Drop:
			UpdateDropAttack();
			break;
		case EDemonBossState.RetractPlatforms:
			UpdateRetractPlatforms();
			break;
		case EDemonBossState.ExtendPlatforms:
			UpdateExtendPlatforms();
			break;
		case EDemonBossState.RedRover:
			UpdateRedRoverAttack();
			break;
		case EDemonBossState.Spotlight:
			UpdateSpotlightAttack(delta);
			break;
		default:
			FinishAttack();
			break;
		}
		_lastDemonStateTimer = _demonStateTimer;
		_demonStateTimer += delta;
	}

	private void UpdateAppearSequence()
	{
		if (_demonStateTimer < 2.5f)
		{
			if (_demonStateTimer < 1f)
			{
				if (_demonStateTimer <= 0f)
				{
					SetDemonSetDrawColor(Color.Transparent, shouldSetSpikes: true);
					SetDemonSetIsActive(isActive: false);
				}
				float amount = _demonStateTimer / 1f;
				Color color = Color.White.SineInterpolate(Color.Black, amount);
				SetBackgroundTilesDrawColor(color);
				SetCollisionTilesDrawColor(color);
			}
			else if (_demonStateTimer >= 1.5f)
			{
				if (_lastDemonStateTimer < 1.5f && !_level.JukeBox.DoesNotPlaySounds)
				{
					BossClass.PlaySongByBossType(_level.JukeBox, base.BossType);
				}
				float num = (_demonStateTimer - 1.5f) / 1f;
				Color backgroundTilesDrawColor = Color.Black * (1f - num);
				Color newColor = Color.Black.SineInterpolate(Color.White, num);
				SetBackgroundTilesDrawColor(backgroundTilesDrawColor);
				SetDemonSetDrawColor(newColor, shouldSetSpikes: false);
			}
		}
		else
		{
			FinishAttack();
			SetDemonSetDrawColor(Color.White, shouldSetSpikes: true);
			SetDemonSetIsActive(isActive: true);
		}
	}

	private void SetDemonSetIsActive(bool isActive)
	{
		_damageCaused = (isActive ? _baseTouchDamage : 0);
		_leftPuppet.SetIsActive(isActive);
		_rightPuppet.SetIsActive(isActive);
		DemonBossPlatform[] platforms = _platforms;
		foreach (DemonBossPlatform demonBossPlatform in platforms)
		{
			demonBossPlatform.SetIsActive(isActive);
		}
	}

	private void SetDemonSetDrawColor(Color newColor, bool shouldSetSpikes)
	{
		if (shouldSetSpikes)
		{
			_spikesLeftWall.DrawColor = newColor;
			_spikesRightWall.DrawColor = newColor;
		}
		base.DrawColor = newColor;
		_leftPuppet.SetDrawColor(newColor);
		_rightPuppet.SetDrawColor(newColor);
		DemonBossPlatform[] platforms = _platforms;
		foreach (DemonBossPlatform demonBossPlatform in platforms)
		{
			demonBossPlatform.DrawColor = newColor;
		}
		SetDemonEnvironmentDrawColor(newColor);
	}

	private void SetDemonEnvironmentDrawColor(Color newColor)
	{
		foreach (Background foreground in _level.Foregrounds)
		{
			foreground.DrawColor = newColor;
		}
		foreach (Background background in _level.Backgrounds)
		{
			background.DrawColor = newColor;
		}
	}

	private void SetCollisionTilesDrawColor(Color newColor)
	{
		foreach (Tile value in _level.SolidTiles.Values)
		{
			value.SetDrawColor(newColor);
		}
		if (_bossDoor == null)
		{
			using IEnumerator<GameEvent> enumerator2 = _level.GetEventAllEventsOfType(EEventTileType.BossDoor).GetEnumerator();
			if (enumerator2.MoveNext())
			{
				GameEvent current2 = enumerator2.Current;
				_bossDoor = current2;
			}
		}
		if (_bossDoor != null)
		{
			_bossDoor.DrawColor = newColor;
		}
	}

	private void SetBackgroundTilesDrawColor(Color newColor)
	{
		foreach (List<Tile> value in _level.BackgroundTiles.Values)
		{
			if (value == null)
			{
				continue;
			}
			foreach (Tile item in value)
			{
				item.SetDrawColor(newColor);
			}
		}
	}

	private void UpdateDropAttack()
	{
		if (_demonStateTimer < 0.35f)
		{
			if (_demonStateTimer <= 0f)
			{
				SetEyeAnimation(EDemonBossEyeAnimationType.Happy);
				_eyeTrackingTarget = _level.MainHero;
				_areEyesTrackingPlayer = false;
			}
			float num = (float)Math.Sin((float)Math.PI / 2f * (_demonStateTimer / 0.35f));
			num *= num;
			Position = new Point(_startingPosition.X, _startingPosition.Y - (int)(num * 8f));
		}
		else if (_demonStateTimer < 0.79999995f)
		{
			if (_lastDemonStateTimer < 0.35f)
			{
				PlayCue(ESFX.BossDemonCrush);
			}
			float num2 = 1f - (float)Math.Cos((float)Math.PI / 2f * ((_demonStateTimer - 0.35f) / 0.45f));
			num2 *= num2;
			Position = new Point(_startingPosition.X, _startingPosition.Y + (int)(num2 * 72f) - 8);
		}
		else if (_demonStateTimer < 2.3f)
		{
			if (_lastDemonStateTimer < 0.79999995f)
			{
				Position = new Point(_startingPosition.X, _startingPosition.Y + 64);
				_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: true);
				_dustParticles.AddParticles(DustEmissionPosition, 100f);
			}
		}
		else if (_demonStateTimer < 3.3f)
		{
			if (_lastDemonStateTimer < 2.3f)
			{
				SetEyeAnimation(EDemonBossEyeAnimationType.Normal);
			}
			float num3 = (float)Math.Sin((float)Math.PI / 2f * ((_demonStateTimer - 2.3f) / 1f));
			Position = new Point(_startingPosition.X, _startingPosition.Y + (int)((1f - num3) * 64f));
		}
		else
		{
			Position = _startingPosition;
			FinishAttack();
		}
	}

	private void UpdateRetractPlatforms()
	{
		if (_demonStateTimer <= 0f)
		{
			_eyeTrackingTarget = _level.MainHero;
			_areEyesTrackingPlayer = false;
			SetEyeAnimation(!_isPlayerStandingOnLeftPlatform, EDemonBossEyeAnimationType.Angry);
			DemonBossWallSpikes demonBossWallSpikes = (_isPlayerStandingOnLeftPlatform ? _spikesLeftWall : _spikesRightWall);
			if (!demonBossWallSpikes.IsExtended)
			{
				demonBossWallSpikes.Extend();
			}
		}
		else if (_demonStateTimer >= 0.2f && _lastDemonStateTimer < 0.2f)
		{
			int num = ((!_isPlayerStandingOnLeftPlatform) ? 2 : 0);
			for (int i = 0; i < 2; i++)
			{
				_platforms[i + num].Retract();
			}
		}
		else if (_demonStateTimer >= 2.2f)
		{
			FinishAttack();
		}
	}

	private void UpdateExtendPlatforms()
	{
		if (_spikesLeftWall.IsExtended)
		{
			_spikesLeftWall.Retract();
		}
		if (_spikesRightWall.IsExtended)
		{
			_spikesRightWall.Retract();
		}
		DemonBossPlatform[] platforms = _platforms;
		foreach (DemonBossPlatform demonBossPlatform in platforms)
		{
			if (demonBossPlatform.IsRetracted)
			{
				demonBossPlatform.Extend();
			}
		}
		FinishAttack();
	}

	private void UpdateRedRoverAttack()
	{
		if (_demonStateTimer < 3.5f)
		{
			if (_demonStateTimer <= 0f)
			{
				if (_level.GetNearestProtagonistPosition(Position).X >= Position.X && !_leftPuppet.IsPlayingDead)
				{
					_leftPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.RedRover);
					_rightPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.Idle);
					_eyeTrackingTarget = _leftPuppet;
				}
				else
				{
					_rightPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.RedRover);
					_leftPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.Idle);
					_eyeTrackingTarget = _rightPuppet;
				}
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdateSpotlightAttack(float delta)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		_areEyesTrackingPlayer = true;
		if (_demonStateTimer < 5.5f)
		{
			if (_demonStateTimer < 1f)
			{
				float amount = _demonStateTimer / 1f;
				Color newColor = Color.White.SineInterpolate(SpotlightDisabledColor, amount);
				SetDemonSetDrawColor(newColor, shouldSetSpikes: false);
				if (_demonStateTimer <= 0f)
				{
					_leftPuppet.SetIsActive(isActive: false);
					_rightPuppet.SetIsActive(isActive: false);
					_leftPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.Resting);
					_rightPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.Resting);
					_spikesLeftWall.Extend();
					_spikesRightWall.Extend();
					DemonBossPlatform[] platforms = _platforms;
					foreach (DemonBossPlatform demonBossPlatform in platforms)
					{
						demonBossPlatform.Retract();
						demonBossPlatform.SetIsSolid(isSolid: false);
					}
				}
			}
			else if (_demonStateTimer < 1.5f)
			{
				if (_lastDemonStateTimer < 1f)
				{
					_canBeDamaged = false;
					_damageCaused = 0;
					base.IsSolidWhenFrozen = false;
					flag2 = true;
					base.Appendages.Add(_spotlightAppendage);
					PlayCue(ESFX.BossDemonSpotlight);
					if (_spotlightLoopCueInstance == null)
					{
						_spotlightLoopCueInstance = _spotlightAppendage.PlayCue(ESFX.BossDemonSpotlightLoop, isLooped: true);
					}
					else
					{
						_spotlightLoopCueInstance.Resume();
					}
				}
				flag = true;
				SetDemonSetDrawColor(SpotlightDisabledColor, shouldSetSpikes: false);
				float amount2 = (_demonStateTimer - 1f) / 0.5f;
				_spotlightAppendage.AuraColor = Color.Transparent.SineInterpolate(SpotlightFinalColor, amount2);
			}
			else if (_demonStateTimer < 2.5f)
			{
				_spotlightAppendage.AuraColor = SpotlightFinalColor;
				flag = true;
			}
			else if (_demonStateTimer < 4f)
			{
				flag = true;
				if (_lastDemonStateTimer < 2.5f)
				{
					flag3 = true;
				}
			}
			else if (_demonStateTimer < 4.75f)
			{
				float num = _demonStateTimer - 4f;
				if (num < 0.75f)
				{
					float amount3 = num / 0.75f;
					_spotlightAppendage.AuraColor = SpotlightFinalColor.SineInterpolate(Color.Transparent, amount3);
				}
				if (_lastDemonStateTimer < 4f && _spotlightLoopCueInstance != null)
				{
					_spotlightLoopCueInstance.Pause(0.15f);
				}
			}
			else
			{
				float amount4 = (_demonStateTimer - 4.75f) / 0.75f;
				Color newColor2 = SpotlightDisabledColor.SineInterpolate(Color.White, amount4);
				SetDemonSetDrawColor(newColor2, shouldSetSpikes: false);
				if (_lastDemonStateTimer < 4.75f)
				{
					base.Appendages.Remove(_spotlightAppendage);
					_leftPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.StopResting);
					_rightPuppet.SetNextPuppetAction(DemonPuppet.EDemonPuppetAction.StopResting);
					_spikesLeftWall.Retract();
					_spikesRightWall.Retract();
					DemonBossPlatform[] platforms2 = _platforms;
					foreach (DemonBossPlatform demonBossPlatform2 in platforms2)
					{
						demonBossPlatform2.Extend();
						demonBossPlatform2.SetIsSolid(isSolid: true);
					}
				}
			}
			if (!flag)
			{
				return;
			}
			Mobile mobile = (Mobile)(((object)_level.MainHero) ?? ((object)this));
			Point position = new Point(mobile.Position.X, 224);
			int num2 = _spotlightAppendage.Position.X - position.X;
			if (flag2 || Math.Abs(num2) <= 1)
			{
				_spotlightAppendage.Position = position;
			}
			else
			{
				float num3 = delta * 4f * (float)num2;
				if (Math.Abs(num3) > 2f)
				{
					num3 = ((!(num3 < 0f)) ? 2f : (-2f));
				}
				_spotlightAppendage.Position = new Point((int)((float)_spotlightAppendage.Position.X - num3), position.Y);
			}
			if (flag3)
			{
				_sawblade.Reset(_spotlightAppendage.Position);
				_level.AddProjectile(_sawblade);
			}
		}
		else
		{
			_leftPuppet.SetIsActive(isActive: true);
			_rightPuppet.SetIsActive(isActive: true);
			_damageCaused = _baseTouchDamage;
			_canBeDamaged = true;
			base.IsSolidWhenFrozen = true;
			_sawblade.CleanUp();
			FinishAttack();
		}
	}

	private void FinishAttack()
	{
		_nextActionTimer = 1f;
		_currentAction = EAIAction.Idle;
		_areEyesTrackingPlayer = true;
		SetEyeAnimation(EDemonBossEyeAnimationType.Normal);
	}

	private void UpdatePlayerStandingTimers(float delta)
	{
		if (_level.GetIsProtagonistInArea(PlayerCrushTriggerArea) || _level.GetIsProtagonistInArea(PlayerCrushTriggerAreaMid))
		{
			_playerStandingUnderneathTimer += delta;
		}
		else
		{
			_playerStandingUnderneathTimer = 0f;
		}
		_playerStandingOnPlatformTimer = 0f;
		int num = 0;
		DemonBossPlatform[] platforms = _platforms;
		foreach (DemonBossPlatform demonBossPlatform in platforms)
		{
			if (demonBossPlatform.TimePlayerSpentStandingOnMe > _playerStandingOnPlatformTimer)
			{
				_playerStandingOnPlatformTimer = demonBossPlatform.TimePlayerSpentStandingOnMe;
				_isPlayerStandingOnLeftPlatform = num < 2;
			}
			num++;
		}
	}

	private bool IsPlayerStillOnPlatformSideOfRoom()
	{
		bool result = false;
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			if (_isPlayerStandingOnLeftPlatform && mainHero.Position.X < Position.X)
			{
				result = true;
			}
			else if (!_isPlayerStandingOnLeftPlatform && mainHero.Position.X > Position.X)
			{
				result = true;
			}
		}
		return result;
	}

	private void UpdateEyes(float delta)
	{
		if (!_areEyesTrackingPlayer)
		{
			return;
		}
		_eyeTrackingTimer -= delta;
		if (_eyeTrackingTimer <= 0f)
		{
			_eyeTrackingTimer = 0.15f;
			if (_eyeTrackingTarget == null)
			{
				_eyeTrackingTarget = _level.MainHero;
			}
			if (_eyeTrackingTarget != null)
			{
				Point point = new Point(Position.X - _eyeTrackingTarget.Position.X, Position.Y + -24 - _eyeTrackingTarget.Position.Y);
				Vector2 vector = new Vector2((float)point.X / 160f, (float)(-point.Y) / 128f);
				_targetEyeAnchorOffset = new Point((int)(vector.X * 3f), (int)(vector.Y * 3f));
			}
		}
		_currentEyeAnchorOffset = MathEx.PercentageFollowPoint(_targetEyeAnchorOffset, _currentEyeAnchorOffset, 0.1f, 1, delta);
		_rightEye.AnchorOffset = new Point((int)(_currentEyeAnchorOffset.X + (float)BaseEyeAnchorOffset.X), (int)(_currentEyeAnchorOffset.Y + (float)BaseEyeAnchorOffset.Y));
		_leftEye.AnchorOffset = new Point((int)(0f - _currentEyeAnchorOffset.X + (float)BaseEyeAnchorOffset.X), (int)(_currentEyeAnchorOffset.Y + (float)BaseEyeAnchorOffset.Y));
		if (base.IsFrozen)
		{
			_rightEye.Update(delta);
			_leftEye.Update(delta);
		}
	}

	private void SetEyeAnimation(EDemonBossEyeAnimationType newAnimation)
	{
		SetEyeAnimation(isRightEye: true, newAnimation);
		SetEyeAnimation(isRightEye: false, newAnimation);
	}

	private void SetEyeAnimation(bool isRightEye, EDemonBossEyeAnimationType newAnimation)
	{
		Appendage appendage = (isRightEye ? _rightEye : _leftEye);
		EDemonBossEyeAnimationType eDemonBossEyeAnimationType = (isRightEye ? _rightEyeAnimationState : _leftEyeAnimationState);
		if (newAnimation != eDemonBossEyeAnimationType)
		{
			if (newAnimation == EDemonBossEyeAnimationType.Normal)
			{
				int eyeAnimationStartByType = GetEyeAnimationStartByType(_rightEyeAnimationState);
				appendage.ChangeAnimation(7, 1, 0.15f, EAnimationType.Once, eyeAnimationStartByType, 1, 0.15f);
			}
			else
			{
				int eyeAnimationStartByType2 = GetEyeAnimationStartByType(newAnimation);
				appendage.ChangeAnimation(eyeAnimationStartByType2, 2, 0.15f, EAnimationType.Once);
			}
			if (isRightEye)
			{
				_rightEyeAnimationState = newAnimation;
			}
			else
			{
				_leftEyeAnimationState = newAnimation;
			}
		}
	}

	private static int GetEyeAnimationStartByType(EDemonBossEyeAnimationType animationType)
	{
		int result = 7;
		switch (animationType)
		{
		case EDemonBossEyeAnimationType.Surprised:
			result = 8;
			break;
		case EDemonBossEyeAnimationType.Happy:
			result = 8;
			break;
		case EDemonBossEyeAnimationType.Angry:
			result = 10;
			break;
		}
		return result;
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		target.GiveStatusEffect(EStatusEffectType.Chaos, 0);
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	protected override void StartDeathScript()
	{
		DemonBossPlatform[] platforms = _platforms;
		foreach (DemonBossPlatform demonBossPlatform in platforms)
		{
			demonBossPlatform.Retract();
			demonBossPlatform.SetIsSolid(isSolid: false);
		}
		_spikesLeftWall.Retract();
		_spikesRightWall.Retract();
		_leftPuppet.StartBossDeathSequence();
		_rightPuppet.StartBossDeathSequence();
		_appendages.Remove(_spotlightAppendage);
		_spotlightAppendage.StopAllSFX();
		_sawblade.StopAllSFX();
		base.StartDeathScript();
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 4.5f)
		{
			Vector2 where = new Vector2(Position.X, Position.Y + -64);
			if (_deathScriptTimer < 2.5f)
			{
				if (deathScriptTimer <= 0f)
				{
					_deathParticleColorVect = _deathParticlesColor.ToVector4();
					SetDemonSetDrawColor(Color.White, shouldSetSpikes: false);
					_deathParticleSystems[0] = _deathLazerParticles;
					_level.PlayCue(ESFX.BossDemonDeath);
				}
				_deathLazerParticles.AddParticles(where);
				_isGlowing = true;
				_leftPuppet.IsGlowing = true;
				_rightPuppet.IsGlowing = true;
				_glowColor = BaseGlowColor;
				_leftPuppet.GlowColor = BaseGlowColor;
				_rightPuppet.GlowColor = BaseGlowColor;
				float num = _deathScriptTimer / 2.5f;
				float num2 = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
				_glowBase = 1f + num2 * 49f;
				_leftPuppet.GlowBase = _glowBase;
				_rightPuppet.GlowBase = _glowBase;
				SetDemonEnvironmentDrawColor(Color.White.SineInterpolate(Color.Black, num));
			}
			else if (deathScriptTimer < 2.5f)
			{
				_level.RequestScreenFlash(0.2f, 1f, 1f);
				SetBackgroundTilesDrawColor(Color.White);
				SetCollisionTilesDrawColor(Color.White);
				SetDemonSetDrawColor(Color.Transparent, shouldSetSpikes: true);
				_level.AddAnimation(EBattleAnimationType.Boom, Bbox.Center, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
				_doesDrawSpriteAndAppendages = false;
				_doesDrawTrail = false;
				_doesDrawBrushTrail = false;
				base.DoesDrawAura = false;
				_deathLazerParticles.KillOffParticles(0f);
				DemonBossPlatform[] platforms = _platforms;
				foreach (DemonBossPlatform demonBossPlatform in platforms)
				{
					demonBossPlatform.SilentKill();
				}
				_leftPuppet.SilentKill();
				_rightPuppet.SilentKill();
				_spikesLeftWall.SilentKill();
				_spikesRightWall.SilentKill();
				_deathParticleSystems[0] = new BossDeathShrapnelParticleSystem(_level.GCM.TxParticleEnergy, 2)
				{
					BaseColor = _deathParticleColorVect
				};
				_deathParticleSystems[0].AddParticles(where);
			}
			ParticleSystem[] deathParticleSystems = _deathParticleSystems;
			for (int j = 0; j < deathParticleSystems.Length; j++)
			{
				deathParticleSystems[j]?.Update(delta);
			}
		}
		else
		{
			SaveBossDeath();
			BossDeathOpenDoors(shouldPlaySong: false);
			RemoveInstance();
			CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Keep1_Demons1, _level, Position);
		}
	}

	internal static void PlaceHairpin(Point position, Level level)
	{
		if (!level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(8))
		{
			BestiaryItemDropSpecification bestiaryItemDropSpecification = new BestiaryItemDropSpecification();
			bestiaryItemDropSpecification.Category = 4;
			bestiaryItemDropSpecification.Item = 8;
			BestiaryItemDropSpecification itemDropData = bestiaryItemDropSpecification;
			ItemDropPickup item = new ItemDropPickup(itemDropData, level, position, -1);
			level.AddItem(item);
		}
	}
}
