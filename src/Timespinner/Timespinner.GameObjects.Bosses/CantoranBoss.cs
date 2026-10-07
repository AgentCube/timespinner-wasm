using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Cantoran;
using Timespinner.GameObjects.Events.Treasure;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Bosses;

internal sealed class CantoranBoss : BossClass
{
	private enum ECantoranSate
	{
		None,
		PlasmaGeyser,
		PlasmaStorm,
		OrbRevolution,
		OrbToss
	}

	private const int PlayerTalkOffset = 128;

	private const int BarrierCount = 12;

	private const int BarrierCreationOffsetY = -16;

	private const int BarrierStartOffsetX = 32;

	private const int BarrierWallOffsetX = 28;

	private const int BarrierWallCount = 10;

	private const float TimeBetweenBarrierWalls = 0.1f;

	private const float CriticalModeHealthThreshold = 0.5f;

	private const float StandingAnimationSpeed = 0.1f;

	private const float FloatingAnimationSpeed = 0.08f;

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

	private const int DeathChargeBaseMinRadius = 12;

	private const int DeathChargeBaseMaxRadius = 32;

	private const int DeathChargeRadiusGrowth = 24;

	private const float DeathChargeTime = 1f;

	private static readonly Point IdleOrbLocation = new Point(18, -32);

	private static readonly Point StormOrbOffset = new Point(0, -48);

	private static readonly Point LazerOrbOffset = new Point(IdleOrbLocation.X, -24);

	private static readonly Color DeathGlowColor = new Color(0.5f, 0.7f, 0.9f, 0.5f);

	private readonly int _barrierDamage;

	private readonly int _levelCenterX;

	private readonly int _floorHeight;

	private readonly CantoranBarrierAppendage _barrierAppendage;

	private readonly BattleAnimation _electricAnimation;

	private readonly BattleAnimation _chargeReadyAnimation;

	private readonly CantoranLightPillarEvent _lightPillarA;

	private readonly CantoranLightPillarEvent _lightPillarB;

	private readonly CantoranOrb _mainOrb;

	private readonly CantoranOrb _subOrb;

	private readonly CantoranBarrierEvent[] _barriers = new CantoranBarrierEvent[12];

	private bool _lastFacing;

	private bool _isDrawingOrb = true;

	private bool _isOverridingOrbPosition;

	private bool _isTargetToOurLeft;

	private bool _isFacingDirectionLocked;

	private bool _isElectricAnimationActive;

	private bool _isInCriticalMode;

	private bool _isSubAttacking;

	private bool _isLightPillarBNext;

	private bool _hasDoneAnyAttacks;

	private ECantoranSate _cantoranState;

	private int _lastNonFollowAction;

	private int _boltCount;

	private int _nonStormMovesUsedSinceStorm;

	private int _nextBarrierIndex;

	private float _emissionTimer;

	private Point _closestHeroPosition;

	private Point _baseOrbPosition;

	private Point _targetMainOrbPosition;

	private Point _targetSubOrbPosition;

	private EmperorDeathParticleSystem _deathChargeParticles;

	private EmperorDeathLazerParticleSystem _deathLazerParticles;

	public CantoranBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_levelCenterX = _level.RoomSize.X / 2;
		_floorHeight = _level.RoomSize.Y - 32;
		_barrierDamage = (int)Math.Ceiling((float)base.Damage * 1.25f);
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
		base.AuraColor = new Color(0.66f, 0.6f, 0.35f, 0.25f);
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.1f;
		_auraCount = 5f;
		_deathParticlesColor = new Color(0.66f, 0.6f, 0.35f, 0.25f);
		IsFacingLeft = false;
		_lastFacing = IsFacingLeft;
		_destinationNodes = new Point[5]
		{
			new Point(72, 216),
			new Point(328, 216),
			new Point(200, 96),
			new Point(200, 144),
			new Point(200, 216)
		};
		_gunOffset = IdleOrbLocation;
		_mainOrb = new CantoranOrb(_level, _position, _sprite, this, isMainOrb: true, base.Damage);
		_mainOrb.SnapBboxToPosition();
		_mainOrb.SnapFrameToBbox();
		_subOrb = new CantoranOrb(_level, _position, _sprite, this, isMainOrb: false, base.Damage);
		_subOrb.SnapBboxToPosition();
		_subOrb.SnapFrameToBbox();
		ChangeAnimation(0, 4, 0.1f, EAnimationType.PingPong);
		_barrierAppendage = new CantoranBarrierAppendage(this, new Point(8, 8), new Point(17, 17), _level, _sprite);
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
			DrawColor = new Color(0.66f, 0.6f, 0.35f, 0.25f)
		};
		_lightPillarA = new CantoranLightPillarEvent(_level, Point.Zero, new ObjectTileSpecification
		{
			Category = EObjectTileCategory.Event
		}, _sprite);
		_lightPillarB = new CantoranLightPillarEvent(_level, Point.Zero, new ObjectTileSpecification
		{
			Category = EObjectTileCategory.Event
		}, _sprite);
	}

	public override void InitializeMob()
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		bool flag = _level.GetNearestProtagonistPosition(Position).X < Position.X;
		int num = (flag ? (-128) : 128);
		IsFacingLeft = flag;
		_level.ToggleExits(isEnabled: false);
		_level.OpenAllBossDoors(-1f);
		_level.LockAllBossDoors(0.5f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		Point point = new Point(Position.X + num, Position.Y);
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			Arguments = new Vector4(point.X, point.Y, 0f, 0f)
		});
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
		base.IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected override void StartBossIntroCutscene()
	{
		_level.JukeBox.FadeOutSong(1f);
		AddDialogue("q_har_4_can_0");
		AddDialogue("q_har_4_can_1");
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
		AddLevelScriptAction(new ScriptAction(EBGM.Boss06));
		AddLevelScriptAction(new ScriptAction(animationSpecCollection, this));
		AddWaitScript(0.08f);
		AddDelegateScript(delegate
		{
			base.Velocity = new Vector2(0f, -200f);
		});
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

	public override void Update(float delta)
	{
		base.Update(delta);
		if (!_isFrozen && !base.IsBossIntroInProgress)
		{
			if (_isAtTargetLocation && !_isCarryingOutAbility)
			{
				switch (_cantoranState)
				{
				case ECantoranSate.PlasmaGeyser:
					StartAbility(0);
					break;
				case ECantoranSate.PlasmaStorm:
					StartAbility(1);
					break;
				case ECantoranSate.OrbRevolution:
					StartAbility(2);
					break;
				case ECantoranSate.OrbToss:
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
		_barrierAppendage.UpdateBarrier(delta, base.IsFrozen);
		if (!base.IsFrozen && _isElectricAnimationActive)
		{
			_electricAnimation.Position = _mainOrb.Position;
			_electricAnimation.Update(delta);
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
		if (_barrierAppendage.IsActive)
		{
			_barrierAppendage.Draw(spriteBatch);
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
				_cantoranState = ECantoranSate.OrbToss;
				_targetPosition = _destinationNodes[4];
				_nextActionTimer = 3f;
				break;
			case 1:
				_cantoranState = ECantoranSate.OrbRevolution;
				_targetPosition = _destinationNodes[3];
				_nextActionTimer = 100f;
				break;
			case 2:
				_cantoranState = ECantoranSate.PlasmaStorm;
				_targetPosition = _destinationNodes[2];
				_nextActionTimer = 100f;
				break;
			case 3:
				_cantoranState = ECantoranSate.PlasmaGeyser;
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
		if (_abilityTimer >= 1f && _abilityTimer < 2.25f)
		{
			if (_lastAbilityTimer < 1f)
			{
				_mainOrb.ShouldAddChargeParticles = false;
			}
			float num = _abilityTimer - 1f;
			float num2 = _lastAbilityTimer - 1f;
			float num3 = 0f;
			int num4 = -1;
			for (int i = 0; i < 10; i++)
			{
				if (num >= num3 && num2 < num3)
				{
					num4 = i;
					break;
				}
				num3 += 0.1f;
			}
			if (num4 > -1)
			{
				int x = Position.X + (32 + num4 * 28) * (_isTargetToOurLeft ? 1 : (-1));
				int floorHeight = _floorHeight;
				CreateBarrier(new Point(x, floorHeight));
			}
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
				CreateBarrier(_targetPosition);
				_boltCount++;
				if (_boltCount == 6)
				{
					_mainOrb.ShouldAddChargeParticles = false;
				}
				if (_boltCount < 6)
				{
					TargetFloor();
				}
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
		CantoranOrb cantoranOrb = (_isSubAttacking ? _subOrb : _mainOrb);
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
			cantoranOrb.State = EOrbState.Charge;
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
				cantoranOrb.StartSpinning();
				StartShowingElectricAppendage(cantoranOrb);
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
		CantoranOrb cantoranOrb = (_isSubAttacking ? _subOrb : _mainOrb);
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
			cantoranOrb.State = EOrbState.Charge;
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
				cantoranOrb.StartSpinning();
				StartShowingElectricAppendage(cantoranOrb);
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
		_nextActionTimer = 1f;
	}

	private void StartShowingElectricAppendage(CantoranOrb targetOrb)
	{
		PlayCue(ESFX.BossSorceressLightningBallStart, targetOrb.Position);
		_barrierAppendage.Reset(targetOrb.BasePosition.ToPoint(), targetOrb);
		_barrierAppendage.Update(0f);
	}

	private void StopShowingElectricAppendage()
	{
		_barrierAppendage.Hide();
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

	protected override void UpdateDeathScript(float delta)
	{
		if (base.IsFrozen)
		{
			return;
		}
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		Vector2 where = new Vector2(_bbox.Center.X, _bbox.Center.Y);
		_deathParticlesColor = new Color(80, 128, 160, 64);
		if (deathScriptTimer <= 0f)
		{
			_deathParticleColorVect = _deathParticlesColor.ToVector4();
			_deathScriptTimer = 1E-06f;
			_deathChargeParticles = new EmperorDeathParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathLazerParticles = new EmperorDeathLazerParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathParticleSystems[0] = _deathChargeParticles;
			_deathParticleSystems[1] = _deathLazerParticles;
			base.DrawColor = Color.White;
			_isElectricAnimationActive = false;
			_barrierAppendage.IsActive = false;
			ChangeAnimation(34, 2, 0.07f, EAnimationType.Cycle);
			_level.PlayCue(ESFX.BossCantoranDeath, Position);
		}
		else if (_deathScriptTimer < 1f)
		{
			float num = _deathScriptTimer / 1f;
			num = ((!(num > 1f)) ? (1f - (float)Math.Cos(num * ((float)Math.PI / 2f))) : 1f);
			if (_deathChargeParticles != null && _deathLazerParticles != null)
			{
				int num2 = (int)(24f * num);
				int num3 = 12 + num2;
				int num4 = 32 + num2;
				_deathChargeParticles.MinStartRadius = num3;
				_deathChargeParticles.MaxStartRadius = num4;
				_deathLazerParticles.MinStartRadius = num3;
				_deathLazerParticles.MaxStartRadius = num4;
				_deathChargeParticles.AddParticles(where);
				_deathLazerParticles.AddParticles(where);
			}
			_isGlowing = true;
			_glowBase = 2f + 10f * num;
			_glowColor = DeathGlowColor;
		}
		else
		{
			_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: false);
			_deathScriptTimer = 2.000001f;
			_doesDrawSpriteAndAppendages = false;
			_doesDrawTrail = false;
			_doesDrawBrushTrail = false;
			base.DoesDrawAura = false;
			base.DeathPosition = Position;
			DropLoot();
			Point center = _mainOrb.Bbox.Center;
			PlaceRadiantOrb(new Point(center.X, center.Y + 40), _level);
			_deathParticleSystems[0] = null;
			_deathParticleSystems[1] = null;
			_mainOrb.DoDeathCleanup(isMainOrb: true);
			_subOrb.DoDeathCleanup(isMainOrb: false);
			SaveBossDeath();
			BossDeathOpenDoors(shouldPlaySong: false);
			RemoveInstance();
		}
		ParticleSystem[] deathParticleSystems = _deathParticleSystems;
		for (int i = 0; i < deathParticleSystems.Length; i++)
		{
			deathParticleSystems[i]?.Update(delta);
		}
	}

	private void TargetFloor()
	{
		_targetPosition = new Point(_level.GetNearestProtagonistPosition(Position).X, _floorHeight);
		CantoranLightPillarEvent cantoranLightPillarEvent = (_isLightPillarBNext ? _lightPillarB : _lightPillarA);
		_isLightPillarBNext = !_isLightPillarBNext;
		if (cantoranLightPillarEvent.Reset(_targetPosition))
		{
			_level.RequestAddObject(cantoranLightPillarEvent);
		}
	}

	internal static OrbPedestalEvent PlaceRadiantOrb(Point position, Level level)
	{
		OrbPedestalEvent orbPedestalEvent = new OrbPedestalEvent(level, position, -1, new ObjectTileSpecification(480)
		{
			Argument = 16
		});
		orbPedestalEvent.Initialize();
		orbPedestalEvent.RemoteSilentKill();
		level.RequestAddObject(orbPedestalEvent);
		return orbPedestalEvent;
	}

	internal void CreateBarrier(Point position)
	{
		position = new Point(position.X, position.Y + -16);
		if (_barriers[_nextBarrierIndex] == null)
		{
			CantoranBarrierEvent cantoranBarrierEvent = new CantoranBarrierEvent(_level, position, -1, new ObjectTileSpecification
			{
				Category = EObjectTileCategory.Event
			}, _sprite, _barrierDamage);
			_barriers[_nextBarrierIndex] = cantoranBarrierEvent;
		}
		CantoranBarrierEvent cantoranBarrierEvent2 = _barriers[_nextBarrierIndex];
		if (cantoranBarrierEvent2.CreateReset(position))
		{
			_level.AddEvent(cantoranBarrierEvent2);
		}
		_nextBarrierIndex++;
		if (_nextBarrierIndex >= 12)
		{
			_nextBarrierIndex = 0;
		}
		PlayCue(ESFX.LunaisOrbRadiantSpell, position);
	}
}
