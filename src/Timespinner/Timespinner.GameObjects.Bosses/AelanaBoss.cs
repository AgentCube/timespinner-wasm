using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Bosses;

internal sealed class AelanaBoss : BossClass
{
	private enum EAelanaSate
	{
		None,
		PlasmaGeyser,
		PlasmaStorm,
		OrbRevolution,
		OrbToss
	}

	private const float CriticalModeHealthThreshold = 0.5f;

	private const float StandingAnimationSpeed = 0.1f;

	private const float FloatingAnimationSpeed = 0.08f;

	private const int PlasmaGeyserBoltCount = 8;

	private const float TimeBeforeGeyserChargeFlash = 0.75f;

	private const float TimeForGeyserToCharge = 1f;

	private const float TotalPlasmaGeyserTime = 2.25f;

	private const int PlasmaStormBoltCount = 6;

	private const float TimeForPlasmaStormToCharge = 1f;

	private const float PlasmaStormIntervalLength = 0.66f;

	private const float PlasmaStormLength = 4.6200004f;

	private const float TotalPlasmaStormTime = 5.6200004f;

	private const int ScepterRevolutionWidth = 160;

	private const int ScepterRevolutionHeight = 112;

	private const int ScepterRevolutionWindupWidth = 24;

	private const int ScepterRevolutionWindupHeight = 24;

	private const float TimeForScepterRevolutionToCharge = 0.5f;

	private const float TimeForScepterRevolutionToTransitionFromChargeToCircuit = 0.1f;

	private const float TimeForScepterRevolutionCircuit = 1.1f;

	private const float TotalScepterRevolutionTime = 1.6f;

	private const int ScepterTossWidth = 196;

	private const int ScepterTossHeight = 0;

	private const int ScepterTossWindupWidth = 16;

	private const int ScepterTossWindupHeight = 16;

	private const float ScepterTossWindupAnimationSpeed = 0.1f;

	private const float ScepterTossThrowAnimationSpeed = 0.07f;

	private const float TimeForScepterTossToCharge = 0.5f;

	private const float TimeForScepterTossToTransitionFromChargeToThrow = 0.1f;

	private const float TimeForScepterTossSend = 0.33f;

	private const float TimeForScepterTossReturn = 0.66f;

	private const float TimeForScepterTossThrow = 0.99f;

	private const float TotalScepterTossTime = 1.49f;

	private const int StartOffsetX = -20;

	private const int StartOffsetY = -23;

	private const int ElectricEffectOffsetY = 4;

	private const float DormantIncrement = 0.15f;

	private const float TimeForElectricToFadeInFadeOut = 0.1f;

	private const int CutsceneFloatingWidth = 4;

	private const int CutsceneFloatingHeight = 6;

	private const float CutsceneFloatingFrequency = 2f;

	private static readonly Point IdleOrbLocation = new Point(18, -32);

	private static readonly Point StormOrbOffset = new Point(0, -48);

	private static readonly Point LazerOrbOffset = new Point(IdleOrbLocation.X, -24);

	private readonly bool _hasAelanaEmpathy;

	private readonly bool _hasDefeatedMaw;

	private readonly bool _hasDoneNelQ2;

	private readonly int _boltDamage;

	private readonly int _beamDamage;

	private readonly int _levelCenterX;

	private readonly int _floorHeight;

	private readonly Appendage _electricAppendage;

	private readonly BattleAnimation _electricAnimation;

	private readonly BattleAnimation _chargeReadyAnimation;

	private readonly AelanaOrb _mainOrb;

	private readonly AelanaOrb _subOrb;

	private bool _lastFacing;

	private bool _isDrawingOrb = true;

	private bool _isOverridingOrbPosition;

	private bool _isTargetToOurLeft;

	private bool _isFacingDirectionLocked;

	private bool _isElectricAppendageActive;

	private bool _isElectricAppendageFadingIn;

	private bool _isElectricAppendageFadingOut;

	private bool _isElectricAnimationActive;

	private bool _isInCriticalMode;

	private bool _isSubAttacking;

	private bool _isDeathFalling;

	private bool _hasTakenOff;

	private bool _hasDoneAnyAttacks;

	private bool _isCutsceneMoving;

	private EAelanaSate _aelanaState;

	private int _lastNonFollowAction;

	private int _boltCount;

	private int _nonStormMovesUsedSinceStorm;

	private float _emissionTimer;

	private float _electricAppendageFadeTimer;

	private float _cutsceneFloatingTimer;

	private float _cutsceneMovingTimer;

	private float _timeForCutsceneMoving;

	private Point _closestHeroPosition;

	private Point _baseOrbPosition;

	private Point _targetMainOrbPosition;

	private Point _targetSubOrbPosition;

	private Point _cutsceneFloatingCenter;

	private Point _cutsceneMovingTarget;

	private Point _cutsceneMovingStart;

	private AelanaOrb _electricAnimationHostOrb;

	internal bool IsSpawnedForCutscene { get; set; }

	public AelanaBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_levelCenterX = _level.RoomSize.X / 2;
		_floorHeight = _level.RoomSize.Y - 32;
		_boltDamage = (int)Math.Ceiling((float)base.Damage * 1.1f);
		_beamDamage = (int)Math.Ceiling((float)base.Damage * 1.25f);
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 2f;
		_maxMoveSpeed = 225f;
		_bboxOffset = new Point(8, 6);
		Bbox = new Rectangle(_position.X, _position.Y, 25, 38);
		_isAffectedByGravity = false;
		base.CannotBeGrabbed = true;
		_doesUseAppendageCollision = false;
		_oscillationMultiplierX = 0f;
		_oscillationMultiplierY = 0.5f;
		base.DoesDrawAura = true;
		base.AuraColor = new Color(0.75f, 0.25f, 0.5f, 0.25f);
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.1f;
		_auraCount = 5f;
		_deathParticlesColor = new Color(0.75f, 0.25f, 0.5f, 1f);
		IsFacingLeft = false;
		_lastFacing = IsFacingLeft;
		_destinationNodes = new Point[5]
		{
			new Point(72, 208),
			new Point(328, 208),
			new Point(200, 80),
			new Point(200, 128),
			new Point(200, 200)
		};
		_gunOffset = IdleOrbLocation;
		_mainOrb = new AelanaOrb(_level, _position, _sprite, this, isMainOrb: true, base.Damage);
		_mainOrb.SnapBboxToPosition();
		_mainOrb.SnapFrameToBbox();
		_subOrb = new AelanaOrb(_level, _position, _sprite, this, isMainOrb: false, base.Damage);
		_subOrb.SnapBboxToPosition();
		_subOrb.SnapFrameToBbox();
		ChangeAnimation(0, 4, 0.1f, EAnimationType.PingPong);
		_electricAppendage = new Appendage(this, new Point(8, 8), new Point(17, 17), _level, _sprite)
		{
			DoesDrawTrail = true,
			DoesDrawBrushTrail = true,
			BrushTrailSize = 40,
			TrailColor = new Color(0.8f, 0.3f, 0.6f, 0.5f),
			TrailLength = 8,
			TrailFadeRate = 2f,
			DrawColor = Color.White * 0.8f
		};
		_electricAppendage.ChangeAnimation(58, 4, 0.05f, EAnimationType.Cycle);
		_electricAnimation = new BattleAnimation(_sprite, Point.Zero, _level)
		{
			AnimationStart = 62,
			AnimationLength = 4,
			AnimationSpeed = 0.07f,
			DrawColor = Color.White * 0.9f,
			DoesRepeat = true
		};
		_chargeReadyAnimation = new BattleAnimation(_level.GCM.SpEffectsMedium, Point.Zero, _level)
		{
			TeamSide = ETeamSide.Enemies,
			AnchorObject = _mainOrb,
			AnimationSpeed = 0.04f,
			AnimationStart = 6,
			AnimationLength = 5,
			DrawColor = new Color(0.75f, 0.25f, 0.5f, 0.25f)
		};
		GameSave gameSave = _level.GameSave;
		_hasAelanaEmpathy = gameSave.GetSaveInt("AelEmpath") > 1;
		_hasDefeatedMaw = gameSave.GetSaveBool($"IsBossDead_{EBossType.Maw}");
		_hasDoneNelQ2 = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Astrologer, gameSave) >= 2;
	}

	public override void InitializeMob()
	{
		base.IsBossIntroInProgress = true;
		if (!IsSpawnedForCutscene)
		{
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.CutsceneStart,
				DoesBlockQueue = false
			});
			bool flag = _level.GetNearestProtagonistPosition(Position).X < Position.X;
			_level.ToggleExits(isEnabled: false);
			_level.OpenAllBossDoors(-1f);
			_level.LockAllBossDoors(0.5f);
			_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
			_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
			Point point = new Point(Position.X + 128, Position.Y);
			AddLevelScriptAction(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.GoToPoint,
				ActionTimer = 1f,
				DoesBlockQueue = true,
				Arguments = new Vector4(point.X, point.Y, 0f, 0f)
			});
			if (flag)
			{
				AddLevelScriptAction(new ScriptAction
				{
					TargetType = EScriptTargetType.Player1,
					ActionType = EScriptActionType.LookDirection,
					ActionTimer = 0.25f,
					DoesBlockQueue = true,
					Arguments = new Vector4(IsFacingLeft ? 1 : (-1), 0f, 0f, 0f)
				});
			}
			_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
			StartBossIntroCutscene();
		}
		else
		{
			ChangeAnimation(66, 4, 0.125f, EAnimationType.PingPong);
			_isDrawingOrb = false;
		}
	}

	protected override void StartBossIntroCutscene()
	{
		_level.JukeBox.FadeOutSong(1f);
		AddDialogue("cs_ael_0_ael_00");
		AddDialogue("cs_ael_0_lun_01");
		AddDialogue("cs_ael_0_ael_02");
		AddDialogue("cs_ael_0_ael_03");
		AddDialogue("cs_ael_0_ael_04");
		AddDialogue("cs_ael_0_ael_05");
		if (!_hasAelanaEmpathy)
		{
			AddDialogue("cs_ael_0_lun_06");
			AddDialogue("cs_ael_0_ael_07");
			AddDialogue("cs_ael_0_lun_08");
			AddDialogue("cs_ael_0_ael_09");
			AddDialogue("cs_ael_0_lun_10");
		}
		else
		{
			AddDialogue("cs_ael_0_lun_11");
			if (_hasDefeatedMaw)
			{
				AddDialogue("cs_ael_0_ael_12");
			}
			AddDialogue("cs_ael_0_lun_13");
			AddDialogue("cs_ael_0_ael_14");
			AddDialogue("cs_ael_0_ael_15");
			AddDialogue("cs_ael_0_ael_16");
			AddDialogue("cs_ael_0_lun_17");
			AddDialogue("cs_ael_0_ael_18");
		}
		AddLiftOffSequence();
		AddWaitScript(0.25f);
		AddDelegateScript(EndBossIntroCutscene);
	}

	protected override void EndBossIntroCutscene()
	{
		base.IsBossIntroInProgress = false;
		base.IsDormant = false;
		_level.HasPlayerBeenDamagedInThisRoom = false;
		_level.HasPlayerFrozenTimeInThisRoom = false;
	}

	internal void AddLiftOffSequence()
	{
		AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
		animationSpecCollection.Collection.Add(new AnimationSpec
		{
			Start = 14,
			Length = 1,
			Speed = 0.08f,
			Type = EAnimationType.Once
		});
		animationSpecCollection.Collection.Add(new AnimationSpec
		{
			Start = 4,
			Length = 4,
			Speed = 0.08f,
			Type = EAnimationType.Cycle
		});
		if (!IsSpawnedForCutscene)
		{
			AddLevelScriptAction(new ScriptAction(EBGM.Boss06));
		}
		AddLevelScriptAction(new ScriptAction(animationSpecCollection, this));
		AddWaitScript(0.08f);
		AddDelegateScript(DoLiftOffVelocityChange);
	}

	private void DoLiftOffVelocityChange()
	{
		base.Velocity = new Vector2(0f, -200f);
		_isDrawingOrb = true;
		_hasTakenOff = true;
		if (IsSpawnedForCutscene)
		{
			_cutsceneFloatingCenter = new Point(Position.X, Position.Y - 8);
		}
	}

	public override void Update(float delta)
	{
		if (IsSpawnedForCutscene && _hasTakenOff)
		{
			UpdateCutsceneFloating(delta);
		}
		base.Update(delta);
		if (!_isFrozen && !base.IsBossIntroInProgress)
		{
			if (_isAtTargetLocation && !_isCarryingOutAbility)
			{
				switch (_aelanaState)
				{
				case EAelanaSate.PlasmaGeyser:
					StartAbility(0);
					break;
				case EAelanaSate.PlasmaStorm:
					StartAbility(1);
					break;
				case EAelanaSate.OrbRevolution:
					StartAbility(2);
					break;
				case EAelanaSate.OrbToss:
					StartAbility(3);
					break;
				}
			}
			else if (!_isCarryingOutAbility && !_isFacingDirectionLocked && !_isRunningDeathScript)
			{
				_updateTargetTimer -= delta;
				if (_updateTargetTimer <= 0f)
				{
					_closestHeroPosition = _level.GetNearestProtagonistPosition(_position);
					IsFacingLeft = _closestHeroPosition.X < _position.X;
					_updateTargetTimer = _maxUpdateTargetTime;
				}
				TurnAround();
				_lastFacing = IsFacingLeft;
			}
		}
		_baseOrbPosition = FindBulletOffset();
		if (!_isOverridingOrbPosition)
		{
			_targetMainOrbPosition = _baseOrbPosition;
			_targetSubOrbPosition = _baseOrbPosition;
		}
		_mainOrb.Update(delta, _targetMainOrbPosition, IsFacingLeft, base.Velocity);
		_subOrb.Update(delta, _targetSubOrbPosition, IsFacingLeft, base.Velocity);
		if (!base.IsFrozen)
		{
			UpdateElectricAppendage(delta);
			if (_isElectricAnimationActive)
			{
				_electricAnimation.Position = _mainOrb.Position;
				_electricAnimation.Update(delta);
			}
		}
	}

	public override void Freeze()
	{
		if (_mainOrb != null)
		{
			_mainOrb.Freeze();
		}
		if (_subOrb != null)
		{
			_subOrb.Freeze();
		}
		base.Freeze();
	}

	public override void Unfreeze()
	{
		if (_mainOrb != null)
		{
			_mainOrb.Unfreeze();
		}
		if (_subOrb != null)
		{
			_subOrb.Unfreeze();
		}
		base.Unfreeze();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isDrawingOrb && _isRunningDeathScript)
		{
			_mainOrb.Draw(spriteBatch);
			_subOrb.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
		if (_isDrawingOrb && !_isRunningDeathScript)
		{
			_mainOrb.Draw(spriteBatch);
			_subOrb.Draw(spriteBatch);
		}
		if (_isElectricAnimationActive)
		{
			_electricAnimation.Draw(spriteBatch);
		}
		if (_isElectricAppendageActive)
		{
			_electricAppendage.Draw(spriteBatch);
		}
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_currentAction == EAIAction.FloatInPlace)
		{
			_isAtTargetLocation = false;
			_isSubAttacking = false;
			_startPosition = _position;
			_followTimer = 0f;
			bool isInCriticalMode = _isInCriticalMode;
			if (!_isInCriticalMode && base.HPPercentage < 0.5f)
			{
				_isInCriticalMode = true;
			}
			int num = (_isInCriticalMode ? 4 : 3);
			int num2 = _random.Next(0, num);
			if (num2 == _lastNonFollowAction)
			{
				num2 = (num2 + 1) % num;
			}
			if (num2 == 2)
			{
				if (_nonStormMovesUsedSinceStorm < 2)
				{
					num2 = 0;
					_nonStormMovesUsedSinceStorm++;
				}
				else
				{
					_nonStormMovesUsedSinceStorm = 0;
				}
			}
			else
			{
				_nonStormMovesUsedSinceStorm++;
			}
			if (!_hasDoneAnyAttacks)
			{
				_hasDoneAnyAttacks = true;
				num2 = 1;
			}
			if (_isInCriticalMode && !isInCriticalMode)
			{
				num2 = num - 1;
			}
			_lastNonFollowAction = num2;
			_currentAction = EAIAction.GoTowards;
			_agility = 0.6f;
			switch (num2)
			{
			case 0:
				_aelanaState = EAelanaSate.OrbToss;
				_targetPosition = _destinationNodes[4];
				_nextActionTimer = 3f;
				break;
			case 1:
				_aelanaState = EAelanaSate.OrbRevolution;
				_targetPosition = _destinationNodes[3];
				_nextActionTimer = 100f;
				break;
			case 2:
				_aelanaState = EAelanaSate.PlasmaStorm;
				_targetPosition = _destinationNodes[2];
				_nextActionTimer = 100f;
				break;
			case 3:
				_aelanaState = EAelanaSate.PlasmaGeyser;
				_isFacingDirectionLocked = true;
				_isTargetToOurLeft = _closestHeroPosition.X > _levelCenterX;
				_targetPosition = (_isTargetToOurLeft ? _destinationNodes[0] : _destinationNodes[1]);
				_nextActionTimer = 100f;
				break;
			}
			_currentDestinationNode = num2;
			_currentCustomAction = num2;
			_totalActionTimer = _nextActionTimer;
		}
		else
		{
			_currentAction = EAIAction.FloatInPlace;
			_nextActionTimer = (float)_random.Next(5, 10) * 0.2f;
		}
		base.PickNextCustomScriptAIAction(delta);
	}

	public override void UpdateAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			UpdateLazerAbility();
			break;
		case 1:
			UpdatePlasmaStormAbility(delta);
			break;
		case 2:
			UpdateRevolutionAbility();
			break;
		case 3:
			UpdateThrowAbility();
			break;
		}
	}

	private void UpdateLazerAbility()
	{
		if (_abilityTimer <= 0f)
		{
			_mainOrb.State = EOrbState.Charge;
			_subOrb.State = EOrbState.Charge;
			_mainOrb.ShouldAddChargeParticles = true;
			PlayCue(ESFX.LunaisChargeStart, _mainOrb.Position);
			ChangeAnimation(new AnimationSpec[5]
			{
				new AnimationSpec
				{
					Start = 21,
					Length = 3,
					Speed = 0.07f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 23,
					Length = 1,
					Speed = 0.58f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 24,
					Length = 1,
					Speed = 0.07f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 28,
					Length = 1,
					Speed = 0.07f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 29,
					Length = 4,
					Speed = 0.07f,
					Type = EAnimationType.Cycle
				}
			});
			_gunOffset = LazerOrbOffset;
			IsFacingLeft = !_isTargetToOurLeft;
		}
		if (_abilityTimer >= 0.75f && _lastAbilityTimer < 0.75f)
		{
			_chargeReadyAnimation.Reset(_mainOrb.Bbox.Center, isFacingLeft: true);
			_level.AddAnimation(_chargeReadyAnimation);
			PlayCue(ESFX.LunaisChargeFlashLarge, _mainOrb.Position);
		}
		if (_abilityTimer >= 1f && _lastAbilityTimer < 1f)
		{
			_mainOrb.ShouldAddChargeParticles = false;
			Point point = Position.Add(new Point(-20 * ((!_isTargetToOurLeft) ? 1 : (-1)), -23));
			_level.AddProjectile(new PinkOrbPlasmaLazer(_level, point, new Vector2(_isTargetToOurLeft ? 1 : (-1), 0f), ETeamSide.Enemies, null, _beamDamage, null, isWide: false));
			Point end = new Point(point.X + 400 * (_isTargetToOurLeft ? 1 : (-1)), point.Y);
			float num = 0f;
			for (int i = 0; i < 8; i++)
			{
				List<Vector4> intervalsBetween = ThunderBoltDamageArea.GetIntervalsBetween(point, end, _level);
				ThunderBoltDamageArea thunderBoltDamageArea = new ThunderBoltDamageArea(_level, point, base.DefaultTeam, _boltDamage, intervalsBetween, !_isTargetToOurLeft, null, EThunderBoltType.Aelana);
				thunderBoltDamageArea.DormantTimer = num;
				ThunderBoltDamageArea newProjectile = thunderBoltDamageArea;
				_level.AddProjectile(newProjectile);
				num += 0.15f;
			}
			_level.PlayCue(ESFX.LunaisLargeLazerShoot, point);
			PlayCue2D(ESFX.LunaisLargeLazerShoot2D);
		}
		else if (_abilityTimer >= 2.25f)
		{
			ChangeAnimation(4, 4, 0.08f, EAnimationType.Cycle, 33, 1, 0.07f);
			EndAbility();
			_gunOffset = IdleOrbLocation;
			_isFacingDirectionLocked = false;
			_mainOrb.State = EOrbState.Idle;
			_subOrb.State = EOrbState.Idle;
		}
	}

	private void UpdatePlasmaStormAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			_boltCount = 0;
			TargetFloor();
			ChangeAnimation(17, 4, 0.08f, EAnimationType.Cycle, 15, 2, 0.08f);
			_gunOffset = StormOrbOffset;
			_mainOrb.State = EOrbState.Charge;
			_subOrb.State = EOrbState.Charge;
			_mainOrb.ShouldAddChargeParticles = true;
			PlayCue(ESFX.LunaisChargeStart, _mainOrb.Position);
		}
		if (_abilityTimer < 1f)
		{
			return;
		}
		if (_abilityTimer < 5.6200004f)
		{
			if (_lastAbilityTimer <= 1f)
			{
				_isElectricAnimationActive = true;
				_electricAnimation.Position = _mainOrb.Bbox.Center;
			}
			_emissionTimer -= delta;
			if (_emissionTimer <= 0f && _boltCount < 6)
			{
				_emissionTimer = 0.66f;
				Point point = Position.Add(_gunOffset);
				List<Vector4> intervalsBetween = ThunderBoltDamageArea.GetIntervalsBetween(point, _targetPosition, _level);
				ThunderBoltDamageArea thunderBoltDamageArea = new ThunderBoltDamageArea(_level, point, base.DefaultTeam, _boltDamage, intervalsBetween, IsFacingLeft, null, EThunderBoltType.Aelana);
				thunderBoltDamageArea.MakePreBolt();
				_level.AddProjectile(thunderBoltDamageArea);
				PlayCue(ESFX.BossSorceressLightningStorm, Position);
				TargetFloor();
				_boltCount++;
			}
			if (_boltCount == 6)
			{
				_mainOrb.ShouldAddChargeParticles = false;
			}
		}
		else
		{
			_gunOffset = IdleOrbLocation;
			ChangeAnimation(new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 15,
					Length = 2,
					Speed = 0.08f,
					Type = EAnimationType.Once,
					IsInReverse = true
				},
				new AnimationSpec
				{
					Start = 4,
					Length = 4,
					Speed = 0.08f,
					Type = EAnimationType.Cycle
				}
			});
			_mainOrb.State = EOrbState.Idle;
			_subOrb.State = EOrbState.Idle;
			_isElectricAnimationActive = false;
			EndAbility();
		}
	}

	private void UpdateRevolutionAbility()
	{
		_isOverridingOrbPosition = true;
		AelanaOrb aelanaOrb = (_isSubAttacking ? _subOrb : _mainOrb);
		Point point = _baseOrbPosition;
		if (_abilityTimer <= 0f)
		{
			_isTargetToOurLeft = _level.GetNearestProtagonistPosition(Position).X < Position.X;
			if (_isTargetToOurLeft != IsFacingLeft)
			{
				IsFacingLeft = _isTargetToOurLeft;
				TurnAround();
			}
			ChangeAnimation(21, 3, 0.1f, EAnimationType.Once);
			point = _baseOrbPosition;
			aelanaOrb.State = EOrbState.Charge;
		}
		if (_abilityTimer < 0.5f)
		{
			float num = _abilityTimer / 0.5f;
			Point b = new Point((int)((Math.Cos(num * (float)Math.PI) - 1.0) * 24.0) * ((!_isTargetToOurLeft) ? 1 : (-1)), (int)((0.0 - Math.Sin(num * ((float)Math.PI / 2f))) * 24.0));
			point = _baseOrbPosition.Add(b);
		}
		else if (_abilityTimer < 1.6f)
		{
			if (_lastAbilityTimer < 0.5f)
			{
				aelanaOrb.StartSpinning();
				StartShowingElectricAppendage(aelanaOrb);
				ChangeAnimation(new AnimationSpec[4]
				{
					new AnimationSpec
					{
						Start = 24,
						Length = 3,
						Speed = 0.07f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 26,
						Length = 1,
						Speed = 0.14f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 27,
						Length = 1,
						Speed = 0.07f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 4,
						Length = 4,
						Speed = 0.08f,
						Type = EAnimationType.Cycle
					}
				});
			}
			float num2 = _abilityTimer - 0.5f;
			float num3 = num2 / 1.1f;
			float num4 = (num3 - 0.05f) * 1.16f;
			Point point2 = new Point((int)(Math.Sin(num4 * (float)Math.PI) * 160.0) * ((!_isTargetToOurLeft) ? 1 : (-1)), (int)(Math.Cos(num4 * ((float)Math.PI / 2f)) * 112.0));
			Point b2 = point2;
			if (num2 < 0.1f)
			{
				float amount = num2 / 0.1f;
				Point start = new Point(-48 * ((!_isTargetToOurLeft) ? 1 : (-1)), -24);
				b2 = start.SineInterpolate(point2, amount);
			}
			else if (num4 > 1f)
			{
				float amount2 = (num4 - 1f) * 10f;
				b2 = point2.SineInterpolate(Point.Zero, amount2);
			}
			point = _baseOrbPosition.Add(b2);
		}
		else if (_abilityTimer >= 1.6f)
		{
			_mainOrb.StopSpinning();
			_mainOrb.State = EOrbState.Idle;
			StopShowingElectricAppendage();
			if (_isInCriticalMode && !_isSubAttacking)
			{
				_isSubAttacking = true;
				_abilityTimer = 0f;
				UpdateRevolutionAbility();
			}
			else
			{
				EndAbility();
				_subOrb.StopSpinning();
				_subOrb.State = EOrbState.Idle;
			}
		}
		if (!_isSubAttacking)
		{
			_targetMainOrbPosition = point;
		}
		else
		{
			_targetSubOrbPosition = point;
		}
	}

	private void UpdateThrowAbility()
	{
		_isOverridingOrbPosition = true;
		AelanaOrb aelanaOrb = (_isSubAttacking ? _subOrb : _mainOrb);
		Point point = _baseOrbPosition;
		Point point2 = new Point(Position.X, Position.Y - 12);
		if (_abilityTimer <= 0f)
		{
			_isTargetToOurLeft = _level.GetNearestProtagonistPosition(Position).X < Position.X;
			if (_isTargetToOurLeft != IsFacingLeft)
			{
				IsFacingLeft = _isTargetToOurLeft;
				TurnAround();
			}
			ChangeAnimation(21, 3, 0.1f, EAnimationType.Once);
			point = point2;
			aelanaOrb.State = EOrbState.Charge;
		}
		if (_abilityTimer < 0.5f)
		{
			float num = _abilityTimer / 0.5f;
			Point b = new Point((int)((Math.Cos(num * (float)Math.PI) - 1.0) * 16.0) * ((!_isTargetToOurLeft) ? 1 : (-1)), (int)((0.0 - Math.Sin(num * ((float)Math.PI / 2f))) * 16.0));
			point = point2.Add(b);
		}
		else if (_abilityTimer < 1.49f)
		{
			if (_lastAbilityTimer < 0.5f)
			{
				aelanaOrb.StartSpinning();
				StartShowingElectricAppendage(aelanaOrb);
				ChangeAnimation(new AnimationSpec[4]
				{
					new AnimationSpec
					{
						Start = 24,
						Length = 3,
						Speed = 0.07f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 26,
						Length = 1,
						Speed = 0.14f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 27,
						Length = 1,
						Speed = 0.07f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 4,
						Length = 4,
						Speed = 0.08f,
						Type = EAnimationType.Cycle
					}
				});
			}
			float num2 = _abilityTimer - 0.5f;
			float num3 = ((num2 < 0.33f) ? (num2 / 0.33f * 0.5f) : (0.5f + (num2 - 0.33f) / 0.66f * 0.5f));
			float num4 = (num3 - 0.05f) * 1.16f;
			Point point3 = new Point((int)(Math.Sin(num4 * (float)Math.PI) * 196.0) * ((!_isTargetToOurLeft) ? 1 : (-1)), (int)(Math.Cos(num4 * (float)Math.PI) * 0.0));
			Point b2 = point3;
			if (num2 < 0.1f)
			{
				float amount = num2 / 0.1f;
				Point start = new Point(-32 * ((!_isTargetToOurLeft) ? 1 : (-1)), -16);
				b2 = start.SineInterpolate(point3, amount);
			}
			else if (num4 > 1f)
			{
				float amount2 = (num4 - 1f) * 10f;
				b2 = point3.SineInterpolate(Point.Zero, amount2);
			}
			point = point2.Add(b2);
		}
		else if (_abilityTimer >= 1.49f)
		{
			_mainOrb.StopSpinning();
			_mainOrb.State = EOrbState.Idle;
			StopShowingElectricAppendage();
			if (_isInCriticalMode && !_isSubAttacking)
			{
				_isSubAttacking = true;
				_abilityTimer = 0f;
				UpdateThrowAbility();
			}
			else
			{
				EndAbility();
				_subOrb.StopSpinning();
				_subOrb.State = EOrbState.Idle;
			}
		}
		if (!_isSubAttacking)
		{
			_targetMainOrbPosition = point;
		}
		else
		{
			_targetSubOrbPosition = point;
		}
	}

	private void EndAbility()
	{
		_isCarryingOutAbility = false;
		_isOverridingOrbPosition = false;
		_currentAction = EAIAction.FloatInPlace;
		_isAtTargetLocation = false;
		_nextActionTimer = 0.75f;
	}

	private void UpdateElectricAppendage(float delta)
	{
		if (!_isElectricAppendageActive)
		{
			return;
		}
		Color drawColor = Color.White;
		if (_isElectricAppendageFadingIn || _isElectricAppendageFadingOut)
		{
			_electricAppendageFadeTimer += delta;
			if (_electricAppendageFadeTimer >= 0.1f)
			{
				if (_isElectricAppendageFadingIn)
				{
					_isElectricAppendageFadingIn = false;
				}
				else
				{
					_isElectricAppendageActive = false;
					_isElectricAppendageFadingOut = false;
					drawColor = new Color(0, 0, 0, 0);
				}
			}
			else
			{
				float num = (float)Math.Sin((float)Math.PI / 2f * _electricAppendageFadeTimer / 0.1f);
				drawColor = (_isElectricAppendageFadingOut ? (Color.White * (1f - num)) : (Color.White * num));
			}
		}
		Point position = _electricAnimationHostOrb.Position;
		_electricAppendage.DrawColor = drawColor;
		_electricAppendage.Update(delta);
		_electricAppendage.Position = new Point(position.X, position.Y + 4);
		_electricAppendage.SnapBboxToPosition();
	}

	private void StartShowingElectricAppendage(AelanaOrb targetOrb)
	{
		PlayCue(ESFX.BossSorceressLightningBallStart, targetOrb.Position);
		_electricAnimationHostOrb = targetOrb;
		_electricAppendage.Position = targetOrb.BasePosition.ToPoint();
		_electricAppendage.SnapBboxToPosition();
		_isElectricAppendageActive = true;
		_electricAppendageFadeTimer = 0f;
		_isElectricAppendageFadingIn = true;
		_isElectricAppendageFadingOut = false;
		_electricAppendage.ClearTrailHistory();
		UpdateElectricAppendage(0f);
	}

	private void StopShowingElectricAppendage()
	{
		_electricAppendageFadeTimer = 0f;
		_isElectricAppendageFadingIn = false;
		_isElectricAppendageFadingOut = true;
	}

	public override void SetState(EAFSM state, int newFrame)
	{
		EAFSM currentState = _currentState;
		base.SetState(state, newFrame);
		switch (state)
		{
		case EAFSM.Idle:
			if (_lastFacing != IsFacingLeft)
			{
				ChangeAnimation(4, 4, 0.08f, EAnimationType.Cycle, 13, 1, 0.08f);
			}
			else if (currentState == EAFSM.Running)
			{
				ChangeAnimation(4, 4, 0.08f, EAnimationType.Cycle, 8, 1, 0.08f);
			}
			else
			{
				ChangeAnimation(4, 4, 0.08f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Running:
			if (_lastFacing != IsFacingLeft)
			{
				ChangeAnimation(9, 4, 0.08f, EAnimationType.Cycle, 13, 1, 0.08f);
			}
			else
			{
				ChangeAnimation(9, 4, 0.08f, EAnimationType.Cycle, 8, 1, 0.08f);
			}
			break;
		}
	}

	private void TurnAround()
	{
		if (_lastFacing != IsFacingLeft)
		{
			if (_currentState == EAFSM.Idle)
			{
				ChangeAnimation(4, 4, 0.08f, EAnimationType.Cycle, 13, 1, 0.08f);
			}
			else if (_currentState == EAFSM.Running || _currentState == EAFSM.Falling)
			{
				ChangeAnimation(9, 4, 0.08f, EAnimationType.Cycle, 13, 1, 0.08f);
			}
			_lastFacing = IsFacingLeft;
		}
	}

	public Point FindBulletOffset()
	{
		int num = ((!IsFacingLeft) ? 1 : (-1));
		return new Point(_position.X + num * _gunOffset.X, _position.Y + _gunOffset.Y);
	}

	protected override void StartDeathScript()
	{
		IsFacingLeft = Position.X > _closestHeroPosition.X;
		_lastFacing = IsFacingLeft;
		ChangeAnimation(34, 2, 0.07f, EAnimationType.Cycle);
		_mainOrb.DoDeathCleanup(isMainOrb: true);
		_subOrb.DoDeathCleanup(isMainOrb: false);
		_isElectricAnimationActive = false;
		_isElectricAppendageActive = false;
		_isAffectedByGravity = true;
		_isFlying = false;
		_isDeathFalling = true;
		_isGrounded = false;
		base.DoesDrawAura = false;
		_gravityAcceleration = 350f;
		_airDragFactor = 0.015f;
		_velocity = new Vector2(150 * (IsFacingLeft ? 1 : (-1)), -150f);
		Appendage appendage = new Appendage(this, new Point(1, 1), new Point(5, 3), _level, _sprite);
		appendage.Position = new Point(Position.X, Position.Y - 41);
		appendage.DrawOrigin = new Vector2(4f, 4f);
		appendage.IsFacingLeft = IsFacingLeft;
		Appendage appendage2 = appendage;
		appendage2.ChangeAnimation(70);
		appendage2.Update(0f);
		DebrisEvent debrisEvent = new DebrisEvent(appendage2, _sprite, -1, new ObjectTileSpecification());
		debrisEvent.GravityAcceleration = _gravityAcceleration;
		debrisEvent.DeathType = DebrisEvent.EDebrisDeathType.Dust;
		DebrisEvent debrisEvent2 = debrisEvent;
		debrisEvent2.Push(new Vector2(_velocity.X, _velocity.Y - 100f), new Point(Bbox.X, Bbox.Top));
		_level.AddEvent(debrisEvent2);
		_level.PlayCue(ESFX.BossSorceressDeath, Position);
		base.StartDeathScript();
	}

	protected override void UpdateDeathScript(float delta)
	{
		_deathScriptTimer += delta;
		if (!_isDeathFalling || (!_isGrounded && !(_deathScriptTimer > 0.1f)))
		{
			return;
		}
		if (Position.Y >= _floorHeight)
		{
			_isDeathFalling = false;
			ChangeAnimation(36, 3, 0.07f, EAnimationType.Once);
			MovePlayerToTalkingPosition(shouldBeFaceToFace: true, -1);
			AnimationSpec animationSpec = new AnimationSpec();
			animationSpec.Start = 39;
			animationSpec.Length = 3;
			animationSpec.Speed = 0.1f;
			animationSpec.Type = EAnimationType.Once;
			AnimationSpec newAnim = animationSpec;
			AnimationSpec animationSpec2 = new AnimationSpec();
			animationSpec2.Start = 42;
			animationSpec2.Length = 3;
			animationSpec2.Speed = 0.1f;
			animationSpec2.Type = EAnimationType.Once;
			AnimationSpec newAnim2 = animationSpec2;
			AddLevelScriptAction(new ScriptAction(newAnim, this));
			AddDialogue("cs_ael_1_ael_00");
			AddDialogue("cs_ael_1_ael_01");
			if (_hasAelanaEmpathy)
			{
				AddDialogue("cs_ael_1_lun_02");
			}
			AddLevelScriptAction(new ScriptAction(newAnim2, this));
			AddDialogue("cs_ael_1_ael_03");
			AddDialogue("cs_ael_1_ael_04");
			if (!_hasAelanaEmpathy)
			{
				AddDialogue("cs_ael_1_lun_05");
				AddDialogue("cs_ael_1_ael_06");
				AddDialogue("cs_ael_1_ael_07");
			}
			else
			{
				AddDialogue("cs_ael_1_lun_08");
			}
			if (_hasDefeatedMaw)
			{
				AddDialogue("cs_ael_1_ael_09");
			}
			AddDialogue("cs_ael_1_ael_10");
			AddDialogue("cs_ael_1_ael_11");
			if (!_hasAelanaEmpathy)
			{
				AddDialogue("cs_ael_1_lun_12");
				AddDialogue("cs_ael_1_ael_13");
				AddDialogue("cs_ael_1_lun_14");
				AddDialogue("cs_ael_1_lun_15");
				AddDialogue("cs_ael_1_ael_16");
				if (_hasDoneNelQ2)
				{
					AddDialogue("cs_ael_1_lun_17");
				}
				else
				{
					AddDialogue("cs_ael_1_lun_18");
				}
				AddDialogue("cs_ael_1_lun_19");
			}
			else
			{
				AddDialogue("cs_ael_1_lun_20");
				AddDialogue("cs_ael_1_lun_21");
				AddDialogue("cs_ael_1_lun_23");
				AddDialogue("cs_ael_1_ael_24");
				if (!_hasDefeatedMaw)
				{
					AddDialogue("cs_ael_1_ael_25");
					AddDialogue("cs_ael_1_lun_26");
				}
			}
			AddDialogue("cs_ael_1_lun_27");
			AddDialogue("cs_ael_1_ael_28");
			AddDialogue("cs_ael_1_lun_29");
			AddDialogue("cs_ael_1_ael_30");
			AddDialogue("cs_ael_1_ael_31");
			AddDialogue("cs_ael_1_lun_32");
			AddDialogue("cs_ael_1_ael_33");
			AddDelegateScript(EndDeathDialog);
		}
		else if (_isGrounded)
		{
			_isGrounded = false;
			if (Position.X < 192)
			{
				Position = new Point(Position.X + 1, Position.Y + 1);
			}
			else
			{
				Position = new Point(Position.X - 1, Position.Y + 1);
			}
			SnapBboxToPosition();
		}
	}

	private void EndDeathDialog()
	{
		SaveBossDeath();
		base.DeathPosition = Position;
		DropLoot();
		BossDeathOpenDoors(shouldPlaySong: false);
		_level.JukeBox.StopSong();
	}

	private void TargetFloor()
	{
		_targetPosition = new Point(_level.GetNearestProtagonistPosition(Position).X, _floorHeight);
	}

	private void UpdateCutsceneFloating(float delta)
	{
		_cutsceneFloatingTimer += delta * 2f;
		if (_cutsceneFloatingTimer >= (float)Math.PI * 2f)
		{
			_cutsceneFloatingTimer -= (float)Math.PI * 2f;
		}
		int num = (int)Math.Round(Math.Cos(_cutsceneFloatingTimer) * 4.0);
		int num2 = (int)Math.Round(Math.Sin(_cutsceneFloatingTimer) * 6.0);
		if (_isCutsceneMoving)
		{
			_cutsceneMovingTimer += delta;
			float amount;
			if (_cutsceneMovingTimer >= _timeForCutsceneMoving)
			{
				amount = 1f;
				_isCutsceneMoving = false;
			}
			else
			{
				amount = _cutsceneMovingTimer / _timeForCutsceneMoving;
			}
			_cutsceneFloatingCenter = _cutsceneMovingStart.CosInterpolate(_cutsceneMovingTarget, amount);
		}
		Position = new Point(_cutsceneFloatingCenter.X + num, _cutsceneFloatingCenter.Y + num2);
		SnapBboxToPosition();
		_isFlying = true;
		_isAffectedByGravity = false;
	}

	internal void CutsceneMoveToPoint(Point destination, float duration)
	{
		_isCutsceneMoving = true;
		_cutsceneMovingTarget = destination;
		_cutsceneMovingStart = _cutsceneFloatingCenter;
		_cutsceneMovingTimer = 0f;
		_timeForCutsceneMoving = duration;
		_damageCaused = 0;
		_doesCollideWithTiles = false;
		_canBeDamaged = false;
		IsFacingLeft = _cutsceneFloatingCenter.X > destination.X;
	}
}
