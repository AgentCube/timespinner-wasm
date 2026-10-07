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
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Maw;
using Timespinner.GameObjects.Enemies;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses;

internal sealed class MawBoss : BossClass
{
	private enum EMawGlobalState
	{
		None,
		HorizontalLazer,
		VerticalLazers,
		AlternatingSpikes,
		FloorSpikes,
		Summon,
		Intro
	}

	private enum EMawPortalColorState
	{
		None,
		Plasma,
		Chaos
	}

	private const int CeilingY = 16;

	private const int FloorY = 192;

	private const int MawBossTileID = 437;

	private const int SpikeStartX = 112;

	private const int AlternatingSpikeBufferX = 32;

	private const int SequenceSpikesBufferX = 24;

	private const int NumberOfSpikesAlternating = 12;

	private const int NumberOfSpikesSequence = 15;

	private const int MaxSpikesCount = 16;

	private const float TimeForSpikesOpenMouth = 1f;

	private const float TimeForSpikesSummon = 1.5f;

	private const float AlternatingSpikeSleepTime = 0.01f;

	private const float SequenceSpikeSleepTime = 0.125f;

	private const float TimeForSpikesAlternatingEntireAttack = 1.62f;

	private const float TimeForSpikesSequenceEntireAttack = 3.375f;

	private const float FrenzyThreshold = 0.5f;

	private const float IndefiniteActionTime = 1000f;

	private const int TotalEnemiesToSummon = 3;

	private const float TimeForSummonOpenMouth = 1f;

	private const float TimeForSummonSummon = 1.2f;

	private const float TimeForEntireSummonAttack = 2.2f;

	private const float TimeToWaitAfterSummoning = 4f;

	private const float TimeForLazerChargeUp = 1f;

	private const float TimeForLazerOpeningMouth = 0.9f;

	private const float TimeForHorizontalLazerFiring = 1.25f;

	private const float TimeForVerticalLazerFiring = 3.75f;

	private const float TimeForRayBackgroundsFade = 0.5f;

	private const int VerticalLazerCeilingStartX = 416;

	private const int VerticalLazerFloorStartX = 128;

	private const float HandFlexAnimationSpeed = 0.066f;

	private const int WindyParticlesOffsetX = 300;

	private const int DeathParticlesOffsetX = 24;

	private const int DeathParticlesOffsetY = -48;

	private const float TimeForDeathChargeUp = 4f;

	private const float TimeForDeathExplosion = 2f;

	private const float TimeForEntireDeathSequence = 6f;

	private const float TimeBeforeStartingSuctionScript = 0.5f;

	private static readonly Vector2 StreamParticlesEmissionPoint = new Vector2(416f, 128f);

	private static readonly Color InvisibleColor = new Color(0, 0, 0, 0);

	private static readonly Color PortalColor1None = new Color(128, 16, 32);

	private static readonly Color PortalColor2None = new Color(200, 80, 64);

	private static readonly Color PortalColor1Plasma = new Color(200, 16, 128);

	private static readonly Color PortalColor2Plasma = new Color(248, 200, 240);

	private static readonly Color PortalColor1Chaos = new Color(128, 8, 24);

	private static readonly Color PortalColor2Chaos = new Color(160, 32, 48);

	private static readonly Color DeathGlowColor = new Color(1f, 1f, 1f, 0.6f);

	private readonly int _baseDamageCaused;

	private readonly Color _rayBackBackgroundDefaultColor;

	private readonly Color _rayTextureBackgroundDefaultColor;

	private readonly Background _rayBackBackground;

	private readonly Background _rayTextureBackground;

	private readonly Appendage _knuckleAppendage;

	private readonly Appendage _fingersAppendage;

	private readonly Appendage _mawPortalAppendage;

	private readonly Appendage _doorAppendage;

	private readonly MawPreLazerStreamParticleSystem _streamParticles;

	private readonly MawBossDeathChargeLazerPS _deathLazerParticles;

	private readonly MawBossWindyParticleSystem _deathWindyParticles;

	private readonly MawBossHorizontalLazer _horizontalLazer;

	private readonly MinionContainer _minionContainer;

	private readonly MawBossSpike[] _spikes = new MawBossSpike[16];

	private bool _areRayBackgroundsFadingOut;

	private bool _isPortalTargetColorNumber1;

	private bool _isReadyToDie;

	private bool _hasDoneHyperMove;

	private EMawGlobalState _globalState;

	private EMawPortalColorState _portalColorState;

	private int _usedSpikeCount;

	private float _globalStateTimer;

	private float _lastGlobalStateTimer;

	private float _rayBackgroundFadeTimer;

	private float _portalColorChangeTimer;

	private SFXCueInstance _lazerLoopCueInstance;

	public MawBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Idle;
		_globalState = EMawGlobalState.None;
		_agility = 1f;
		Bbox = new Rectangle(_position.X, _position.Y, 92, 160);
		_isAffectedByGravity = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_deathParticlesColor = Color.Gold;
		_baseDamageCaused = _damageCaused;
		if (_level.Backgrounds.Count > 2)
		{
			_rayBackBackground = _level.Backgrounds[1];
			_rayTextureBackground = _level.Backgrounds[2];
			_rayBackBackgroundDefaultColor = _rayBackBackground.DrawColor;
			_rayTextureBackgroundDefaultColor = _rayTextureBackground.DrawColor;
		}
		if (base.Appendages.Count > 4)
		{
			_knuckleAppendage = base.Appendages[1];
			_fingersAppendage = base.Appendages[2];
			_mawPortalAppendage = base.Appendages[3];
			_doorAppendage = base.Appendages[4];
			_mawPortalAppendage.DoesInheritDrawColor = false;
			_mawPortalAppendage.DrawColor = new Color(0.8f, 0.9f, 0.95f);
			_doorAppendage.DoesDrawAura = true;
			_doorAppendage.AuraColor = Color.DarkViolet * 0.9f;
			_doorAppendage.AuraOffset = new Vector2(1f, 1f);
			_doorAppendage.AuraSize = 0.015f;
			_doorAppendage.AuraFrequency = 8f;
			_horizontalLazer = new MawBossHorizontalLazer(_level, _mawPortalAppendage.Position, new Vector2(1f, 0f), ETeamSide.Enemies, _mawPortalAppendage, new Point(11, -17), _sprite, base.Damage);
		}
		InitalizeAppendages();
		ChangeAnimation(0);
		_streamParticles = new MawPreLazerStreamParticleSystem(_level.GCM.TxParticleEnergy, 5)
		{
			BaseColor = new Vector4(0.9f, 0.5f, 0.75f, 0.9f)
		};
		_particleSystems.Add(_streamParticles);
		_deathLazerParticles = new MawBossDeathChargeLazerPS(_level.GCM.TxParticleEnergy, 5);
		_deathWindyParticles = new MawBossWindyParticleSystem(_level.GCM.SpAnimatedParticlesSmall, 20, 64, isBlowingLeft: true);
		_minionContainer = new MinionContainer(3, _level);
		_level.IsOnVilete = true;
	}

	private void InitalizeAppendages()
	{
		CloseDoor(isInstant: true);
	}

	public override void InitializeMob()
	{
		_level.ToggleExits(isEnabled: false);
		_minionContainer.Initialize();
		base.IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected override void StartBossIntroCutscene()
	{
		_canBeDamaged = false;
		_globalState = EMawGlobalState.Intro;
		_currentAction = EAIAction.Custom;
		_doorAppendage.ChangeAnimation(5);
		_damageCaused = 0;
		Update(0f);
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.CavesPast4_MawSpit, _level, _doorAppendage.Bbox.Center);
		AddDelegateScript(EndBossIntroCutscene);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdatePortalColor(delta);
			if (_isReadyToDie && _currentAction == EAIAction.Idle && !_isRunningDeathScript)
			{
				ActuallyStartDeathScript();
			}
		}
		base.Update(delta);
		_minionContainer.Update(delta);
		if (!base.IsFrozen)
		{
			UpdateRayBackgrounds(delta);
		}
	}

	private void UpdatePortalColor(float delta)
	{
		_portalColorChangeTimer += delta;
		float num = 0f;
		Color color = Color.Black;
		switch (_portalColorState)
		{
		case EMawPortalColorState.None:
			num = 0.15f;
			color = (_isPortalTargetColorNumber1 ? PortalColor1None : PortalColor2None);
			break;
		case EMawPortalColorState.Plasma:
			num = 0.15f;
			color = (_isPortalTargetColorNumber1 ? PortalColor1Plasma : PortalColor2Plasma);
			break;
		case EMawPortalColorState.Chaos:
			num = 0.25f;
			color = (_isPortalTargetColorNumber1 ? PortalColor1Chaos : PortalColor2Chaos);
			break;
		}
		float num2 = 1f;
		if (_portalColorChangeTimer >= num || num <= 0f)
		{
			_isPortalTargetColorNumber1 = !_isPortalTargetColorNumber1;
			_portalColorChangeTimer = 0f;
		}
		else
		{
			num2 = _portalColorChangeTimer / num;
		}
		Color drawColor = ((!(num2 < 1f)) ? color : _mawPortalAppendage.DrawColor.SineInterpolate(color, num2));
		_mawPortalAppendage.DrawColor = drawColor;
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_globalStateTimer = 0f;
		_lastGlobalStateTimer = -1E-07f;
		if (_globalState == EMawGlobalState.Intro)
		{
			_nextActionTimer = 1f;
			return;
		}
		_nextActionTimer = 1000f;
		if (base.HPPercentage > 0.5f)
		{
			switch (_level.NextRandomInt(0, 2))
			{
			case 0:
				_globalState = EMawGlobalState.AlternatingSpikes;
				break;
			case 1:
				_globalState = ((!_minionContainer.IsFull) ? EMawGlobalState.Summon : EMawGlobalState.AlternatingSpikes);
				break;
			default:
				_globalState = EMawGlobalState.FloorSpikes;
				break;
			}
		}
		else
		{
			switch (_level.NextRandomInt(0, 3))
			{
			case 0:
				_globalState = EMawGlobalState.AlternatingSpikes;
				break;
			case 1:
				_globalState = ((!_minionContainer.IsFull) ? EMawGlobalState.Summon : EMawGlobalState.AlternatingSpikes);
				break;
			case 2:
				_globalState = EMawGlobalState.VerticalLazers;
				break;
			default:
				_globalState = EMawGlobalState.FloorSpikes;
				break;
			}
			if (!_hasDoneHyperMove)
			{
				_hasDoneHyperMove = true;
				_globalState = EMawGlobalState.VerticalLazers;
			}
		}
		if ((_globalState == EMawGlobalState.AlternatingSpikes || _globalState == EMawGlobalState.FloorSpikes) && _level.GetNearestProtagonistPosition(Position).X > 450)
		{
			_globalState = EMawGlobalState.HorizontalLazer;
		}
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_globalState)
		{
		case EMawGlobalState.AlternatingSpikes:
			UpdateSummonSpikes(isAlternating: true);
			break;
		case EMawGlobalState.FloorSpikes:
			UpdateSummonSpikes(isAlternating: false);
			break;
		case EMawGlobalState.Summon:
			UpdateSummonEnemies();
			break;
		case EMawGlobalState.HorizontalLazer:
			UpdateHorizontalLazers();
			break;
		case EMawGlobalState.VerticalLazers:
			UpdateVerticalLazers();
			break;
		}
		_lastGlobalStateTimer = _globalStateTimer;
		_globalStateTimer += delta;
	}

	internal void DoIntroCloseMouth()
	{
		CloseDoor(isInstant: false);
		_damageCaused = _baseDamageCaused;
		_globalState = EMawGlobalState.None;
		_canBeDamaged = true;
	}

	private void UpdateSummonSpikes(bool isAlternating)
	{
		if (_globalStateTimer < (isAlternating ? 1.62f : 3.375f))
		{
			if (_lastGlobalStateTimer < 1f && _globalStateTimer >= 1f)
			{
				FlexHand(isAlternating ? 2 : 4);
			}
			else
			{
				if (!(_lastGlobalStateTimer < 1.5f) || !(_globalStateTimer >= 1.5f))
				{
					return;
				}
				PlayCue2D(ESFX.BossMawSpikeWaveFlame);
				PlayCue2D(ESFX.BossMawSpikeTeethCast);
				float num = 0f;
				int num2 = (isAlternating ? 12 : 15);
				int num3 = 0;
				int num4 = (isAlternating ? 32 : 24);
				bool flag = false;
				if (isAlternating)
				{
					Protagonist mainHero = _level.MainHero;
					if (mainHero != null)
					{
						int num5 = (mainHero.Position.X - 96) % 64;
						flag = num5 > 32;
					}
				}
				for (int i = 0; i < num2; i++)
				{
					bool flag2 = isAlternating && i % 2 != 0;
					if (flag)
					{
						flag2 = !flag2;
					}
					int x = 112 + num3;
					int y = (flag2 ? 16 : 192);
					AddSpike(new Point(x, y), flag2, num);
					num += (isAlternating ? 0.01f : 0.125f);
					num3 += num4;
				}
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void AddSpike(Point startPoint, bool isCeilingSpike, float sleepTime)
	{
		MawBossSpike mawBossSpike = null;
		if (_usedSpikeCount < 16)
		{
			mawBossSpike = new MawBossSpike(_level, startPoint, _sprite, isCeilingSpike, sleepTime, base.Damage);
			_spikes[_usedSpikeCount] = mawBossSpike;
			_usedSpikeCount++;
		}
		else
		{
			for (int i = 0; i < 16; i++)
			{
				if (_spikes[i].IsFinished)
				{
					mawBossSpike = _spikes[i];
					mawBossSpike.Reset(startPoint, isCeilingSpike, sleepTime);
					break;
				}
			}
		}
		if (mawBossSpike != null)
		{
			_level.AddProjectile(mawBossSpike);
		}
	}

	private void UpdateSummonEnemies()
	{
		if (_globalStateTimer < 2.2f)
		{
			if (_globalStateTimer <= 0f)
			{
				FlexHand(1);
			}
			if (_lastGlobalStateTimer < 1f && _globalStateTimer >= 1f)
			{
				OpenDoor(isInstant: false);
				_portalColorState = EMawPortalColorState.Chaos;
			}
			else
			{
				if (!(_lastGlobalStateTimer < 1.2f) || !(_globalStateTimer >= 1.2f))
				{
					return;
				}
				Point location = _mawPortalAppendage.Bbox.Center.Add(8, 8);
				for (int i = 0; i < 3; i++)
				{
					if (!SummonMinion(location))
					{
						break;
					}
				}
			}
		}
		else
		{
			FinishAttack(4f);
			CloseDoor(isInstant: false);
		}
	}

	private void UpdateHorizontalLazers()
	{
		if (_globalStateTimer < 1f)
		{
			if (_lastGlobalStateTimer <= 0f)
			{
				_level.PlayCue(ESFX.BossMawLazerStart, new Point(288, 120));
			}
			_streamParticles.AddParticles(StreamParticlesEmissionPoint, new Vector2(-1f, 0f));
		}
		if (_globalStateTimer >= 0.9f && _lastGlobalStateTimer < 0.9f)
		{
			OpenDoor(isInstant: false);
			ToggleRayBackgroundFade(areBackgroundsFadingOut: true);
			_portalColorState = EMawPortalColorState.Plasma;
			_streamParticles.KillOffParticles(0.1f);
		}
		if (_globalStateTimer >= 1f && _lastGlobalStateTimer < 1f)
		{
			_horizontalLazer.Reset(1.25f);
			_level.AddProjectile(_horizontalLazer);
			_level.RequestScreenShake(new Vector2(1f, 2f), 2.75f, 12f, isAffectedByTime: true);
			PlayLazerLoopInstance();
		}
		if (_horizontalLazer.IsClosing && !_horizontalLazer.WasClosing)
		{
			CloseDoor(isInstant: false);
			EndLazerLoopInstance();
		}
		if (_globalStateTimer > 2.25f)
		{
			FinishAttack();
			ToggleRayBackgroundFade(areBackgroundsFadingOut: false);
		}
	}

	private void UpdateVerticalLazers()
	{
		if (_globalStateTimer < 1f)
		{
			if (_lastGlobalStateTimer <= 0f)
			{
				_level.PlayCue(ESFX.BossMawLazerStart, new Point(288, 120));
			}
			_streamParticles.AddParticles(StreamParticlesEmissionPoint, new Vector2(-1f, 0f));
		}
		if (_globalStateTimer >= 0.9f && _lastGlobalStateTimer < 0.9f)
		{
			OpenDoor(isInstant: false);
			ToggleRayBackgroundFade(areBackgroundsFadingOut: true);
			_portalColorState = EMawPortalColorState.Plasma;
			_streamParticles.KillOffParticles(0.1f);
		}
		if (_globalStateTimer >= 1f && _lastGlobalStateTimer < 1f)
		{
			_horizontalLazer.Reset(3.75f);
			_level.AddProjectile(_horizontalLazer);
			_level.AddProjectile(new MawBossVerticalLazer(_level, new Point(416, 16), new Vector2(0f, 1f), _sprite, base.Damage));
			_level.AddProjectile(new MawBossVerticalLazer(_level, new Point(128, 192), new Vector2(0f, -1f), _sprite, base.Damage));
			_level.RequestScreenShake(new Vector2(1f, 2f), 6f, 12f, isAffectedByTime: true);
			PlayLazerLoopInstance();
		}
		if (_horizontalLazer.IsClosing && !_horizontalLazer.WasClosing)
		{
			CloseDoor(isInstant: false);
			EndLazerLoopInstance();
		}
		if (_globalStateTimer > 4.75f)
		{
			FinishAttack();
			ToggleRayBackgroundFade(areBackgroundsFadingOut: false);
		}
	}

	private void PlayLazerLoopInstance()
	{
		if (_lazerLoopCueInstance == null)
		{
			_lazerLoopCueInstance = PlayCue(ESFX.BossMawLazerLoop, new Point(288, 120), isLooped: true);
		}
		else if (_lazerLoopCueInstance.IsPaused)
		{
			_lazerLoopCueInstance.Resume();
		}
	}

	private void EndLazerLoopInstance()
	{
		if (_lazerLoopCueInstance != null && !_lazerLoopCueInstance.IsFinished)
		{
			_lazerLoopCueInstance.Pause(0.25f);
		}
	}

	private void FinishAttack()
	{
		_portalColorState = EMawPortalColorState.None;
		FinishAttack(3f);
	}

	private void FinishAttack(float waitTime)
	{
		_nextActionTimer = waitTime;
		_currentAction = EAIAction.Idle;
	}

	private bool SummonMinion(Point location)
	{
		bool result = false;
		if (!_minionContainer.IsFull)
		{
			MawBossMinion newMinion = new MawBossMinion(location, _level, _sprite, -1, new ObjectTileSpecification(437)
			{
				Category = EObjectTileCategory.Enemy,
				Argument = 1
			});
			_minionContainer.AddMinion(newMinion);
			result = true;
		}
		return result;
	}

	private void OpenDoor(bool isInstant)
	{
		if (isInstant)
		{
			_doorAppendage.ChangeAnimation(5);
			return;
		}
		PlayCue(ESFX.BossMawMouthOpen, _mawPortalAppendage.Bbox.Center);
		_doorAppendage.ChangeAnimation(new AnimationSpec
		{
			Start = 2,
			Length = 4,
			Speed = 0.1f,
			Type = EAnimationType.Once
		});
	}

	private void CloseDoor(bool isInstant)
	{
		if (isInstant)
		{
			_doorAppendage.ChangeAnimation(2);
			return;
		}
		PlayCue(ESFX.BossMawMouthClose, _mawPortalAppendage.Bbox.Center);
		_doorAppendage.ChangeAnimation(new AnimationSpec
		{
			Start = 2,
			Length = 4,
			Speed = 0.1f,
			Type = EAnimationType.Once,
			IsInReverse = true
		});
	}

	private void FlexHand(int timesToFlex)
	{
		List<AnimationSpec> list = new List<AnimationSpec>();
		List<AnimationSpec> list2 = new List<AnimationSpec>();
		_knuckleAppendage.PlayCue(ESFX.BossMawBoneTwitch);
		for (int i = 0; i < timesToFlex; i++)
		{
			list.Add(new AnimationSpec
			{
				Start = 7,
				Length = 4,
				Speed = 0.066f,
				Type = EAnimationType.Once,
				IsInReverse = (i % 2 == 0)
			});
			list2.Add(new AnimationSpec
			{
				Start = 11,
				Length = 4,
				Speed = 0.066f,
				Type = EAnimationType.Once,
				IsInReverse = (i % 2 == 0)
			});
		}
		list.Add(new AnimationSpec
		{
			Start = 8,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		});
		list2.Add(new AnimationSpec
		{
			Start = 12,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		});
		_knuckleAppendage.ChangeAnimation(list);
		_fingersAppendage.ChangeAnimation(list2);
	}

	private void ToggleRayBackgroundFade(bool areBackgroundsFadingOut)
	{
		_areRayBackgroundsFadingOut = areBackgroundsFadingOut;
		_rayBackgroundFadeTimer = 0f;
	}

	private void UpdateRayBackgrounds(float delta)
	{
		if (_rayBackgroundFadeTimer < 0.5f && _rayBackBackground != null && _rayTextureBackground != null)
		{
			float amount = 1f;
			_rayBackgroundFadeTimer += delta;
			if (_rayBackgroundFadeTimer < 0.5f)
			{
				amount = _rayBackgroundFadeTimer / 0.5f;
			}
			if (_areRayBackgroundsFadingOut)
			{
				_rayBackBackground.DrawColor = _rayBackBackgroundDefaultColor.SineInterpolate(InvisibleColor, amount);
				_rayTextureBackground.DrawColor = _rayTextureBackgroundDefaultColor.SineInterpolate(InvisibleColor, amount);
			}
			else
			{
				_rayBackBackground.DrawColor = InvisibleColor.SineInterpolate(_rayBackBackgroundDefaultColor, amount);
				_rayTextureBackground.DrawColor = InvisibleColor.SineInterpolate(_rayTextureBackgroundDefaultColor, amount);
			}
		}
	}

	protected override void StartDeathScript()
	{
		if (!_isReadyToDie)
		{
			_isReadyToDie = true;
			_minionContainer.KillAll();
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
		_minionContainer.KillAll();
		_isFinallyDead = true;
		_isRunningDeathScript = true;
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 6f)
		{
			Vector2 where = new Vector2(Position.X + 24, Position.Y + -48);
			if (_deathScriptTimer < 4f)
			{
				if (deathScriptTimer <= 0f)
				{
					OpenDoor(isInstant: false);
					_deathParticleColorVect = _deathParticlesColor.ToVector4();
					base.DrawColor = Color.White;
					_level.RequestScreenShake(new Vector2(1f, 2f), 6f, 12f, isAffectedByTime: true);
					_level.JukeBox.PlayCue(ESFX.BossMawDeathRoar);
					_deathParticleSystems[0] = _deathLazerParticles;
					_particleSystems.Add(_deathWindyParticles);
					foreach (Appendage appendage in base.Appendages[0].Appendages)
					{
						appendage.DoesInheritDrawColor = true;
					}
				}
				_deathLazerParticles.AddParticles(where);
				_deathWindyParticles.AddParticles(new Vector2(Position.X + 300, _doorAppendage.Bbox.Center.Y));
				_isGlowing = true;
				_glowColor = DeathGlowColor;
				float num = _deathScriptTimer / 4f;
				float num2 = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
				_glowBase = 1f + num2 * 10f;
			}
			if (_deathScriptTimer >= 0.5f && deathScriptTimer < 0.5f)
			{
				CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.CavesPast5_MawDie, _level, _doorAppendage.Bbox.Center);
			}
			ParticleSystem[] deathParticleSystems = _deathParticleSystems;
			for (int i = 0; i < deathParticleSystems.Length; i++)
			{
				deathParticleSystems[i]?.Update(delta);
			}
		}
		else
		{
			EndBossDeathScript();
		}
	}

	internal override void InitializeForBestiary()
	{
		CloseDoor(isInstant: true);
	}
}
