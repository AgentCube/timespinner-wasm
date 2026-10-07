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
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorBoss : BossClass
{
	private enum EEmperorState
	{
		None,
		Idle,
		Move,
		Melee,
		Spell
	}

	private const int TileSize = 16;

	private const int ArenaCenterX = 288;

	private const int MovementFloorY = 214;

	private const int MovementIntervalCount = 5;

	private const int SpellSlashThresholdX = 136;

	private const int EmpireOrbSpellOffsetY = -24;

	private const int BlueSpellCastOffsetY = -48;

	private const int PlasmaSpellCastOffsetX = 12;

	private const int PlasmaSpellCastOffsetY = -44;

	private const int FireSpellSpeed = 400;

	private const int CameraPanX = 288;

	private const int CameraPanY = 120;

	private const float BladeChargeVelocityX = 300f;

	private const float IdleFloatFrequency = 2f;

	private const float IdleFloatAmplitudeX = 4f;

	private const float IdleFloatAmplitudeY = 8f;

	private const float IdleFloatOffset = -18f;

	private const float TimeForBlueMelee = 4f;

	private const float TimeForBladeMelee = 5f;

	private const float TimeForEmpireMelee = 5f;

	private const float TimeForBlueSpell = 1.5f;

	private const float TimeForEmpireSpell = 2f;

	private const float MinTimeBetweenHurtCues = 2f;

	private const float HurtCuePlayProbability = 0.15f;

	private const float EmpireChargeRampUpTime = 1f;

	private const float EmpireChargeVelocityX = 400f;

	private const float TimeForTeleportFadeOut = 0.5f;

	private const float TimeForTeleportFadeIn = 0.25f;

	private const float TimeForTeleportMove = 1f;

	private const float TimeBeforeTeleportFadeIn = 1.5f;

	private const float TimeForEntireTeleport = 1.75f;

	private const float TimeToWaitAfterAnyAction = 1.25f;

	private const int PlayerTalkOffsetX = -104;

	private const int CutsceneMoveHeightOffset = 24;

	private const float TimeToTakeOff = 2f;

	private const int DeathExplosionOffsetX = 0;

	private const int DeathExplosionOffsetY = -48;

	private const int DeathChargeOffsetX = 0;

	private const int DeathChargeOffsetY = -48;

	private const int DeathChargeBaseMinRadius = 24;

	private const int DeathChargeBaseMaxRadius = 48;

	private const int DeathChargeRadiusGrowth = 64;

	private const float DeathTimeForChargingUp = 3f;

	private const float DeathTimeForDeathPostExplosion = 2f;

	private const float DeathScreenFlashDuration = 0.4f;

	private const float DeathTimeBeforeAddingScreenFlash = 2.8f;

	private const float DeathTimeBeforeExploding = 3f;

	private const float DeathTimeForEntireSequence = 5f;

	private static readonly Color BaseDeathSandGlowColor = new Color(0.9f, 0.5f, 0.25f, 0.15f);

	private static readonly Color BlueDeathLevelDrawColor = new Color(0.25f, 0.35f, 0.75f, 1f);

	private static readonly Color BlueDeathScreenFlashColor = new Color(0.8f, 0.85f, 1f, 1f);

	private static readonly Color PinkDeathLevelDrawColor = new Color(0.75f, 0.25f, 0.45f, 1f);

	private static readonly Color PinkDeathScreenFlashColor = new Color(1f, 0.8f, 0.9f, 1f);

	private readonly bool _isViletianEmperor;

	private readonly bool _isPrinceEmperor;

	private readonly int _baseTouchDamage;

	private readonly int _standingFloorY;

	private readonly Point _startingPosition;

	private readonly Color _baseAuraColor;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _throwSequence;

	private readonly CharacterSequenceSpecification _channelSequence;

	private readonly CharacterSequenceSpecification _channelThrowSequence;

	private readonly CharacterSequenceSpecification _landSequence;

	private readonly CharacterSequenceSpecification _takeOffSequence;

	private readonly CharacterSequenceSpecification _channelLandSequence;

	private readonly CharacterSequenceSpecification _channelThrowLandSequence;

	private readonly EmperorOrbManager _orbManager;

	private readonly int[] _moveNodePositions = new int[5] { 80, 192, 288, 384, 496 };

	private bool _hasStartedAttack;

	private bool? _isFacingLeftAfterTeleport;

	private bool _isLanding;

	private bool _isTakingOff;

	private bool _shouldShowLandingAnimation;

	private bool _isCutsceneMoveSinusoidal;

	private bool _isUsingALandedAbility;

	private bool _wasUsingALandedAbility;

	private EEmperorOrbType _currentOrbSet;

	private EEmperorState _emperorState;

	private EEmperorState _nextEmperorState;

	private int _lastNonMoveAction;

	private int _moveNodeIndex;

	private float _idleFloatTimer;

	private float _emperorStateTimer;

	private float _lastEmperorStateTimer;

	private float _timeBeforeMeleeThrowSequence;

	private float _landingTakeOffTimer;

	private float _timeToLand;

	private float _timeForCutsceneMove;

	private float _idleRadiusMultiplier = 1f;

	private float _hurtCueTimer;

	private Point _moveTargetPosition;

	private Point _moveStartPosition;

	private Point _landingTargetPosition;

	private Vector2 _floatingBasePosition;

	private EmperorDeathParticleSystem _deathChargeParticles;

	private EmperorDeathLazerParticleSystem _deathLazerParticles;

	private EmpireOrbSpellDamageArea _leftEmpireSpell;

	private EmpireOrbSpellDamageArea _rightEmpireSpell;

	private EmperorFireSpellDamageArea _fireSpell;

	internal bool IsSpawnedForCutscene { get; set; }

	internal bool IsMovingDuringCutscene { get; set; }

	public EmperorBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_startingPosition = inPosition;
		_standingFloorY = _startingPosition.Y + 4;
		_baseTouchDamage = _damageCaused;
		_floatingBasePosition = inPosition.ToVector2();
		_isViletianEmperor = objectSpec.Argument == 1;
		_isPrinceEmperor = objectSpec.Argument == 2;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_agility = 1f;
		_bboxOffset = Point.Zero;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		base.CannotBeGrabbed = true;
		_isAffectedByFriction = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		_isAffectedByLevelBounds = false;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		_baseAuraColor = (_isViletianEmperor ? new Color(0.75f, 0.25f, 0.5f, 0.25f) : new Color(0.25f, 0.25f, 0.75f, 0.25f));
		base.AuraColor = _baseAuraColor;
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		_currentOrbSet = ((!_isViletianEmperor) ? EEmperorOrbType.Blue : EEmperorOrbType.Fire);
		_orbManager = new EmperorOrbManager(_sprite, this, _currentOrbSet, _baseTouchDamage, _isViletianEmperor);
		_deathParticlesColor = new Color(0.75f, 0.75f, 0.75f, 1f);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 0)
		{
			_idleSequence = GetCharacterSequenceByName("Idle");
			_throwSequence = GetCharacterSequenceByName("Throw");
			_channelSequence = GetCharacterSequenceByName("Channel");
			_channelThrowSequence = GetCharacterSequenceByName("ChannelThrow");
			_landSequence = GetCharacterSequenceByName("Land");
			_takeOffSequence = GetCharacterSequenceByName("TakeOff");
			_channelLandSequence = GetCharacterSequenceByName("ChannelLand");
			_channelThrowLandSequence = GetCharacterSequenceByName("ChannelThrowLand");
			SetCharacterSequence(_idleSequence);
		}
		if (_isViletianEmperor)
		{
			_level.IsOnVilete = true;
		}
	}

	public override void InitializeMob()
	{
		HideOrbs(doesShowAnimation: false, isSilent: true);
		if (!IsSpawnedForCutscene)
		{
			bool flag = !_isViletianEmperor && !_isPrinceEmperor;
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.CutsceneStart,
				DoesBlockQueue = false
			});
			LandAtLocation(new Point(_startingPosition.X, _standingFloorY), 0f, shouldDoLandingAnimation: false);
			SetCharacterSequence(_landSequence);
			UpdateCharacterSequences(1f);
			if (flag)
			{
				bool flag2 = _level.GetNearestProtagonistPosition(Position).X < Position.X;
				_level.JukeBox.FadeOutSong(3f);
				_level.ToggleExits(isEnabled: false);
				_level.OpenAllBossDoors(-1f);
				_level.LockAllBossDoors(0.5f);
				_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
				_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
				Point point = new Point(Position.X + -104, Position.Y);
				AddScript(new ScriptAction
				{
					ScriptType = EScriptType.LockUnlockCamera
				});
				AddScript(new ScriptAction(new Vector2(288f, 120f), 1.5f, shouldBlock: false)
				{
					SleepTime = 0.5f
				});
				AddLevelScriptAction(new ScriptAction
				{
					TargetType = EScriptTargetType.Player1,
					ActionType = EScriptActionType.GoToPoint,
					ActionTimer = 1f,
					DoesBlockQueue = true,
					Arguments = new Vector4(point.X, point.Y, 0f, 0f)
				});
				if (!flag2)
				{
					AddLevelScriptAction(new ScriptAction
					{
						TargetType = EScriptTargetType.Player1,
						ActionType = EScriptActionType.LookDirection,
						ActionTimer = 0.25f,
						DoesBlockQueue = true,
						Arguments = new Vector4((!IsFacingLeft) ? 1 : (-1), 0f, 0f, 0f)
					});
				}
				_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
			}
			base.IsBossIntroInProgress = true;
			if (flag)
			{
				StartBossIntroCutscene();
			}
		}
		else
		{
			SetCharacterSequenceByName("AddHelmet");
		}
	}

	protected override void StartBossIntroCutscene()
	{
		AddDialogue("cs_emp_1_nuv_00");
		AddDialogue("cs_emp_1_lun_01");
		AddDialogue("cs_emp_1_nuv_02");
		AddDialogue("cs_emp_1_lun_03");
		AddDialogue("cs_emp_1_nuv_04");
		AddDialogue("cs_emp_1_nuv_05");
		AddDialogue("cs_emp_1_lun_06");
		AddDialogue("cs_emp_1_nuv_07");
		AddDialogue("cs_emp_1_nuv_08");
		AddDialogue("cs_emp_1_lun_09");
		AddDialogue("cs_emp_1_nuv_10");
		AddDialogue("cs_emp_1_lun_11");
		AddDelegateScript(PlayBossSong);
		AddDialogue("cs_emp_1_nuv_12");
		AddDialogue("cs_emp_1_lun_13");
		AddLevelScriptAction(new ScriptAction(ESFX.BossEmperorFightBegin, Position));
		AddDelegateScript(TakeOffFromGround);
		AddScript(new ScriptAction(new Vector2(Position.X + -104, Position.Y), 1f, shouldBlock: false));
		AddWaitScript(1f);
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.LockUnlockCamera,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDelegateScript(EndBossIntroCutscene);
	}

	private void PlayBossSong()
	{
		_level.JukeBox.PlaySong(EBGM.Boss12);
	}

	protected override void EndBossIntroCutscene()
	{
		ShowOrbs(doesShowAnimation: true);
		_emperorState = EEmperorState.Idle;
		_nextActionTimer = 1f;
		base.IsBossIntroInProgress = false;
		base.IsDormant = false;
		_level.HasPlayerBeenDamagedInThisRoom = false;
		_level.HasPlayerFrozenTimeInThisRoom = false;
	}

	internal void StartBattle()
	{
		_level.LockAllBossDoors(0f);
		base.EndBossIntroCutscene();
	}

	internal void ShowOrbs(bool doesShowAnimation)
	{
		_orbManager.SetAreOrbsHidden(areOrbsHidden: false, doesShowAnimation);
		PlayCue(ESFX.BossEmperorOrbsForm);
	}

	internal void HideOrbs(bool doesShowAnimation, bool isSilent)
	{
		_orbManager.SetAreOrbsHidden(areOrbsHidden: true, doesShowAnimation);
		if (!isSilent)
		{
			PlayCue(ESFX.BossEmperorOrbsVanish);
		}
	}

	internal void LandAtLocation(Point landingPoint, float timeToLand, bool shouldDoLandingAnimation)
	{
		_landingTargetPosition = landingPoint;
		_landingTakeOffTimer = timeToLand;
		_timeToLand = timeToLand;
		_isLanding = true;
		_isTakingOff = false;
		_shouldShowLandingAnimation = shouldDoLandingAnimation;
	}

	internal void TakeOffFromGround()
	{
		_isTakingOff = true;
		_isLanding = false;
		_landingTakeOffTimer = 0f;
		if (IsSpawnedForCutscene)
		{
			SetCharacterSequenceByName("ProTakeOff");
		}
		else
		{
			SetCharacterSequence(_takeOffSequence);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (IsMovingDuringCutscene)
			{
				UpdateCutsceneMoving(delta);
			}
			UpdateFloating(delta);
			_orbManager.Update(delta);
			if (_hurtCueTimer > 0f)
			{
				_hurtCueTimer -= delta;
			}
		}
		base.Update(delta);
	}

	private void UpdateFloating(float delta)
	{
		_idleFloatTimer += delta * 2f;
		if (_idleFloatTimer >= (float)Math.PI * 2f)
		{
			_idleFloatTimer -= (float)Math.PI * 2f;
		}
		float num = (float)((0.0 - Math.Cos(_idleFloatTimer)) * 4.0) * _idleRadiusMultiplier;
		float num2 = (float)(Math.Sin(_idleFloatTimer) * 8.0 + -18.0) * _idleRadiusMultiplier;
		Vector2 vector = new Vector2(_floatingBasePosition.X + num, _floatingBasePosition.Y + num2);
		if (_isLanding)
		{
			if (_landingTakeOffTimer > 0f)
			{
				_landingTakeOffTimer -= delta;
				if (_landingTakeOffTimer <= 0f)
				{
					_landingTakeOffTimer = 0f;
					if (_shouldShowLandingAnimation)
					{
						SetCharacterSequence(_landSequence);
					}
				}
			}
			float amount = 1f;
			if (_timeToLand > 0f)
			{
				amount = 1f - _landingTakeOffTimer / _timeToLand;
			}
			vector = vector.CosInterpolate(_landingTargetPosition.ToVector2(), amount);
		}
		else if (_isTakingOff)
		{
			_landingTakeOffTimer += delta;
			if (_landingTakeOffTimer >= 2f)
			{
				_isTakingOff = false;
			}
			else
			{
				float amount2 = _landingTakeOffTimer / 2f;
				vector = _landingTargetPosition.ToVector2().SineInterpolate(vector, amount2);
			}
		}
		_floatPosition = vector;
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_emperorState == EEmperorState.Idle)
		{
			_isAtTargetLocation = false;
			_startPosition = _position;
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			int num = _random.Next(0, 2);
			if (num == _lastNonMoveAction)
			{
				num = (num + 1) % 2;
			}
			_lastNonMoveAction = num;
			_nextActionTimer = 100f;
			_nextEmperorState = EEmperorState.None;
			switch (num)
			{
			case 0:
				switch (_currentOrbSet)
				{
				case EEmperorOrbType.Blue:
				case EEmperorOrbType.Fire:
					if (nearestProtagonistPosition.X > 288)
					{
						_moveNodeIndex = 3;
					}
					else
					{
						_moveNodeIndex = 1;
					}
					_isFacingLeftAfterTeleport = null;
					break;
				case EEmperorOrbType.Blade:
				case EEmperorOrbType.Iron:
					if (nearestProtagonistPosition.X < 288)
					{
						_moveNodeIndex = 4;
						_isFacingLeftAfterTeleport = true;
					}
					else
					{
						_moveNodeIndex = 0;
						_isFacingLeftAfterTeleport = false;
					}
					break;
				case EEmperorOrbType.Empire:
					if (nearestProtagonistPosition.X > 288)
					{
						_moveNodeIndex = 4;
						_isFacingLeftAfterTeleport = true;
					}
					else
					{
						_moveNodeIndex = 0;
						_isFacingLeftAfterTeleport = false;
					}
					break;
				case EEmperorOrbType.Plasma:
					_moveNodeIndex = 2;
					_isFacingLeftAfterTeleport = null;
					break;
				}
				_emperorState = EEmperorState.Move;
				_nextEmperorState = EEmperorState.Melee;
				break;
			case 1:
				_orbManager.StartCharging();
				SetCharacterSequence(_channelSequence);
				switch (_currentOrbSet)
				{
				case EEmperorOrbType.Blade:
				case EEmperorOrbType.Blue:
				case EEmperorOrbType.Fire:
				case EEmperorOrbType.Iron:
					if (nearestProtagonistPosition.X < 288)
					{
						_moveNodeIndex = 4;
						_isFacingLeftAfterTeleport = true;
					}
					else
					{
						_moveNodeIndex = 0;
						_isFacingLeftAfterTeleport = false;
					}
					break;
				case EEmperorOrbType.Empire:
					_moveNodeIndex = 2;
					_isFacingLeftAfterTeleport = null;
					break;
				case EEmperorOrbType.Plasma:
					if (nearestProtagonistPosition.X > 288)
					{
						_moveNodeIndex = 4;
						_isFacingLeftAfterTeleport = true;
					}
					else
					{
						_moveNodeIndex = 0;
						_isFacingLeftAfterTeleport = false;
					}
					break;
				}
				_emperorState = EEmperorState.Move;
				_nextEmperorState = EEmperorState.Spell;
				break;
			}
			_currentDestinationNode = num;
			_currentCustomAction = num;
			_totalActionTimer = _nextActionTimer;
			_emperorStateTimer = 0f;
			_lastEmperorStateTimer = 0f;
			int x = _moveNodePositions[_moveNodeIndex];
			_wasUsingALandedAbility = _isUsingALandedAbility;
			_isUsingALandedAbility = _nextEmperorState == EEmperorState.Spell && (_currentOrbSet == EEmperorOrbType.Empire || _currentOrbSet == EEmperorOrbType.Blue || _currentOrbSet == EEmperorOrbType.Plasma || _currentOrbSet == EEmperorOrbType.Fire);
			int y = (_isUsingALandedAbility ? _standingFloorY : 214);
			_moveTargetPosition = new Point(x, y);
			_moveStartPosition = _floatingBasePosition.ToPoint();
			_currentAction = EAIAction.Custom;
		}
		else
		{
			_emperorState = EEmperorState.Idle;
			_nextActionTimer = 0f;
		}
		base.PickNextCustomScriptAIAction(delta);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_emperorState)
		{
		case EEmperorState.Melee:
			StartAbility(0);
			_nextActionTimer = 0f;
			break;
		case EEmperorState.Spell:
			StartAbility(1);
			_nextActionTimer = 0f;
			break;
		case EEmperorState.Move:
			UpdateMoving();
			break;
		}
		_lastEmperorStateTimer = _emperorStateTimer;
		_emperorStateTimer += delta;
	}

	public override void UpdateAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
		{
			float num2 = 4f;
			switch (_currentOrbSet)
			{
			case EEmperorOrbType.Blade:
			case EEmperorOrbType.Iron:
				num2 = 5f;
				break;
			case EEmperorOrbType.Empire:
			case EEmperorOrbType.Plasma:
				num2 = 5f;
				break;
			}
			if (_abilityTimer <= 0f)
			{
				PlayCue(ESFX.BossEmperorOrbsPreAttack);
				_timeBeforeMeleeThrowSequence = _orbManager.DoMeleeAttack();
				break;
			}
			if (_abilityTimer > num2)
			{
				EndAbility();
				break;
			}
			if (_abilityTimer >= 0.75f && _lastAbilityTimer < 0.75f)
			{
				PlayMeleeCastCue();
			}
			if (_abilityTimer >= _timeBeforeMeleeThrowSequence && _lastAbilityTimer < _timeBeforeMeleeThrowSequence)
			{
				SetCharacterSequence(_throwSequence);
				PlayCue(ESFX.BossEmperorArmOut);
				PlaySmallEffortCue();
			}
			if (_currentOrbSet == EEmperorOrbType.Empire)
			{
				float num3 = 400f;
				if (_abilityTimer < 1f)
				{
					num3 *= (float)(1.0 - Math.Cos((float)Math.PI / 2f * _abilityTimer / 1f));
				}
				float num4 = delta * num3 * (float)((!IsFacingLeft) ? 1 : (-1));
				_floatingBasePosition = new Vector2(_floatingBasePosition.X + num4, _floatingBasePosition.Y);
				if ((IsFacingLeft && _floatingBasePosition.X < (float)_moveNodePositions[0]) || (!IsFacingLeft && _floatingBasePosition.X > (float)_moveNodePositions[4]))
				{
					EndAbility();
				}
			}
			break;
		}
		case 1:
		{
			EEmperorOrbType currentOrbSet = _currentOrbSet;
			if (currentOrbSet == EEmperorOrbType.Blade || currentOrbSet == EEmperorOrbType.Iron)
			{
				UpdateBladeIronSpell(delta);
				break;
			}
			float num = 1.5f;
			switch (_currentOrbSet)
			{
			case EEmperorOrbType.Empire:
			case EEmperorOrbType.Plasma:
				num = 2f;
				break;
			}
			if (_abilityTimer <= 0f)
			{
				CastSpell(_currentOrbSet);
			}
			else if (_abilityTimer > num)
			{
				EndAbility();
			}
			break;
		}
		}
	}

	private void UpdateBladeIronSpell(float delta)
	{
		float num = 300f;
		if (_abilityTimer < 1f)
		{
			num *= (float)(1.0 - Math.Cos((float)Math.PI / 2f * _abilityTimer / 1f));
		}
		float num2 = delta * num * (float)((!IsFacingLeft) ? 1 : (-1));
		_floatingBasePosition = new Vector2(_floatingBasePosition.X + num2, _floatingBasePosition.Y);
		if (_hasStartedAttack)
		{
			return;
		}
		if ((IsFacingLeft && _floatingBasePosition.X < (float)_moveNodePositions[0]) || (!IsFacingLeft && _floatingBasePosition.X > (float)_moveNodePositions[4]))
		{
			_hasStartedAttack = true;
			SetCharacterSequence(_channelThrowSequence);
		}
		else if (_abilityTimer >= 1f)
		{
			_currentTarget = _level.GetNearestProtagonist(Position);
			if (_currentTarget != null && Math.Abs(_currentTarget.Position.X - Position.X) <= 136)
			{
				_hasStartedAttack = true;
				SetCharacterSequence(_channelThrowSequence);
			}
		}
	}

	private void CastSpell(EEmperorOrbType spellType)
	{
		_orbManager.StopCharging();
		bool flag = true;
		PlayBigEffortCue();
		int baseTouchDamage = _baseTouchDamage;
		switch (spellType)
		{
		case EEmperorOrbType.Blue:
			baseTouchDamage = (int)Math.Ceiling((float)baseTouchDamage * 1.5f);
			BlueOrbSpell.CreatePowerfulSpell(_level, ETeamSide.Enemies, new Point(Position.X, Position.Y + -48), baseTouchDamage, null, IsFacingLeft);
			break;
		case EEmperorOrbType.Blade:
		{
			baseTouchDamage = (int)Math.Ceiling((float)baseTouchDamage * 1.5f);
			LunaGiantSwordProjectile newProjectile = new LunaGiantSwordProjectile(_level, Position, IsFacingLeft, ETeamSide.Enemies, this, baseTouchDamage, null);
			PlayCue(ESFX.LunaisGiantSwordWhiff);
			_level.AddProjectile(newProjectile);
			flag = false;
			break;
		}
		case EEmperorOrbType.Empire:
		{
			baseTouchDamage = (int)Math.Ceiling((float)baseTouchDamage * 1.5f);
			Point point2 = new Point(_moveNodePositions[2], 190);
			bool flag2 = _level.GetNearestProtagonistPosition(point2).X < point2.X;
			if (_leftEmpireSpell == null)
			{
				_leftEmpireSpell = new EmpireOrbSpellDamageArea(_level, point2, ETeamSide.Enemies, baseTouchDamage, null, isCastingLeft: true, flag2);
				_rightEmpireSpell = new EmpireOrbSpellDamageArea(_level, point2, ETeamSide.Enemies, baseTouchDamage, null, isCastingLeft: false, !flag2);
			}
			else
			{
				_leftEmpireSpell.Reset(point2, isCastingLeft: true, baseTouchDamage, flag2);
				_rightEmpireSpell.Reset(point2, isCastingLeft: false, baseTouchDamage, !flag2);
			}
			_level.AddProjectile(_leftEmpireSpell);
			_level.AddProjectile(_rightEmpireSpell);
			PlayCue(ESFX.LunaisOrbEmpireSpell);
			PlayCue2D(ESFX.LunaisOrbEmpireSpell2D);
			break;
		}
		case EEmperorOrbType.Iron:
		{
			baseTouchDamage = (int)Math.Ceiling((float)baseTouchDamage * 1.5f);
			LunaGiantHammerProjectile newProjectile2 = new LunaGiantHammerProjectile(_level, Position, IsFacingLeft, ETeamSide.Enemies, this, baseTouchDamage, null);
			PlayCue(ESFX.LunaisGiantHammerWhiff);
			_level.AddProjectile(newProjectile2);
			flag = false;
			break;
		}
		case EEmperorOrbType.Plasma:
		{
			baseTouchDamage = (int)Math.Ceiling((float)baseTouchDamage * 1.25f);
			Point inPosition = new Point((IsFacingLeft ? (-12) : 12) + Position.X, Position.Y + -44);
			_level.AddProjectile(new PinkOrbPlasmaLazer(_level, inPosition, new Vector2((!IsFacingLeft) ? 1 : (-1), 0f), ETeamSide.Enemies, null, baseTouchDamage, null, isWide: true));
			break;
		}
		case EEmperorOrbType.Fire:
		{
			Point point = new Point(Position.X, Position.Y + -48);
			if (_fireSpell == null)
			{
				_fireSpell = new EmperorFireSpellDamageArea(_level, point, base.Damage);
			}
			_fireSpell.Reset(point, new Vector2(IsFacingLeft ? (-400) : 400, 0f));
			_level.AddProjectile(_fireSpell);
			break;
		}
		}
		if (flag)
		{
			CharacterSequenceSpecification characterSequence = (_isUsingALandedAbility ? _channelThrowLandSequence : _channelThrowSequence);
			SetCharacterSequence(characterSequence);
		}
	}

	private void ChangeOrbSet()
	{
		switch (_currentOrbSet)
		{
		case EEmperorOrbType.Blue:
			_currentOrbSet = EEmperorOrbType.Blade;
			break;
		case EEmperorOrbType.Blade:
			_currentOrbSet = (_isPrinceEmperor ? EEmperorOrbType.Blue : EEmperorOrbType.Empire);
			break;
		case EEmperorOrbType.Empire:
			_currentOrbSet = EEmperorOrbType.Blue;
			break;
		case EEmperorOrbType.Fire:
			_currentOrbSet = EEmperorOrbType.Plasma;
			break;
		case EEmperorOrbType.Plasma:
			_currentOrbSet = EEmperorOrbType.Iron;
			break;
		case EEmperorOrbType.Iron:
			_currentOrbSet = EEmperorOrbType.Fire;
			break;
		}
		_orbManager.ChangeOrbSet(_currentOrbSet);
		PlayCue(ESFX.BossEmperorOrbsShift);
	}

	private void UpdateMoving()
	{
		if (_emperorStateTimer < 1.75f)
		{
			if (_emperorStateTimer <= 0.5f)
			{
				if (_emperorStateTimer <= 0f)
				{
					PlayCue(ESFX.BossEmperorDisappear);
				}
				float num = 1f - _emperorStateTimer / 0.5f;
				base.DrawColor = Color.White * num;
				base.AuraColor = _baseAuraColor * num;
				_orbManager.SetDrawColor(base.DrawColor);
				if (_lastEmperorStateTimer < 0.25f && _emperorStateTimer >= 0.25f)
				{
					SetInvisibilityStatus(isInvisible: true);
				}
				if (_wasUsingALandedAbility)
				{
					if (_lastEmperorStateTimer <= 0f)
					{
						SetCharacterSequence(_takeOffSequence);
					}
					_idleRadiusMultiplier = 1f - num;
				}
				return;
			}
			if (_emperorStateTimer > 0.5f && _emperorStateTimer < 1.5f)
			{
				base.DrawColor = Color.White * 0f;
				base.AuraColor = _baseAuraColor * 0f;
				if (_lastEmperorStateTimer <= 0.5f)
				{
					_orbManager.SetDrawColor(base.DrawColor);
				}
				float num2 = (_emperorStateTimer - 0.5f) / 1f;
				_floatingBasePosition = _moveStartPosition.SineInterpolate(_moveTargetPosition, num2).ToVector2();
				if (_isUsingALandedAbility)
				{
					_idleRadiusMultiplier = 1f - num2;
				}
				else
				{
					_idleRadiusMultiplier = 1f;
				}
				return;
			}
			if (_lastEmperorStateTimer < 1.5f)
			{
				PlayCue(ESFX.BossEmperorReappear);
				SetInvisibilityStatus(isInvisible: false);
				if (_isFacingLeftAfterTeleport.HasValue)
				{
					IsFacingLeft = _isFacingLeftAfterTeleport.Value;
				}
				else
				{
					Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
					IsFacingLeft = Position.X > nearestProtagonistPosition.X;
				}
				if (_isUsingALandedAbility)
				{
					SetCharacterSequence(_channelLandSequence);
					_idleRadiusMultiplier = 0f;
				}
			}
			float num3 = (_emperorStateTimer - 1.5f) / 0.25f;
			base.DrawColor = Color.White * num3;
			base.AuraColor = _baseAuraColor * num3;
			_orbManager.SetDrawColor(base.DrawColor);
		}
		else
		{
			_floatingBasePosition = _moveTargetPosition.ToVector2();
			if (_nextEmperorState != 0)
			{
				_emperorState = _nextEmperorState;
				_emperorStateTimer = 0f;
				_nextActionTimer = 100f;
			}
			else
			{
				_nextActionTimer = 0f;
			}
		}
	}

	private void SetInvisibilityStatus(bool isInvisible)
	{
		_damageCaused = ((!isInvisible) ? _baseTouchDamage : 0);
		_canBeDamaged = !isInvisible;
		base.IsSolidWhenFrozen = !isInvisible;
	}

	private void EndAbility()
	{
		_isCarryingOutAbility = false;
		_hasStartedAttack = false;
		_abilityTimer = 0f;
		_lastAbilityTimer = -1f;
		_nextActionTimer = 1.25f;
		_emperorState = EEmperorState.None;
		if (_random.NextDouble() > 0.5)
		{
			ChangeOrbSet();
		}
	}

	private void FinishThrow()
	{
		if (_selectedAbility == 1 && (_currentOrbSet == EEmperorOrbType.Blade || _currentOrbSet == EEmperorOrbType.Iron))
		{
			CastSpell(_currentOrbSet);
			EndAbility();
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification.IntArgument == 0)
		{
			FinishThrow();
		}
	}

	protected override void StartDeathScript()
	{
		if (_isViletianEmperor)
		{
			_level.GameSave.SetValue("IsTerrilisDead", value: true);
		}
		else if (_isPrinceEmperor)
		{
			_level.GameSave.SetValue("IsPrinceDead", value: true);
		}
		MovePlayerToTalkingPosition(shouldBeFaceToFace: false, 64);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 5f, Vector4.Zero)
		{
			IsUnskippable = true
		});
		base.StartDeathScript();
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			_level.PlayCue(ESFX.BossEmperorDeath, Bbox.Center);
			_deathChargeParticles = new EmperorDeathParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathLazerParticles = new EmperorDeathLazerParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathParticleSystems[0] = _deathChargeParticles;
			_deathParticleSystems[1] = _deathLazerParticles;
			_orbManager.Kill();
			_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 24f, isAffectedByTime: true);
			SetCharacterSequence(_idleSequence);
			UpdateCharacterSequences(0f);
		}
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 5f)
		{
			if (_deathScriptTimer < 3f)
			{
				float num = _deathScriptTimer / 3f;
				float num2 = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
				_isGlowing = true;
				_glowBase = 2f + 10f * num2;
				_glowColor = BaseDeathSandGlowColor;
				_orbManager.UpdateDeathGlow(_glowBase, _glowColor);
				Color end = (_isViletianEmperor ? PinkDeathLevelDrawColor : BlueDeathLevelDrawColor);
				Color color = Color.White.Lerp(end, num);
				_level.SetLevelDrawColor(color);
				SetBackgroundColor(color);
				if (_deathChargeParticles != null && _deathLazerParticles != null)
				{
					num = _deathScriptTimer / 3f;
					int num3 = (int)(64f * num);
					int num4 = 24 + num3;
					int num5 = 48 + num3;
					_deathChargeParticles.MinStartRadius = num4;
					_deathChargeParticles.MaxStartRadius = num5;
					_deathLazerParticles.MinStartRadius = num4;
					_deathLazerParticles.MaxStartRadius = num5;
					int x = Position.X;
					_ = IsFacingLeft;
					Vector2 where = new Vector2(x, Position.Y + -48);
					_deathChargeParticles.AddParticles(where);
					_deathLazerParticles.AddParticles(where);
				}
				if (_deathScriptTimer >= 2.8f && deathScriptTimer < 2.8f)
				{
					Color effectColor = (_isViletianEmperor ? PinkDeathScreenFlashColor : BlueDeathScreenFlashColor);
					_level.RequestScreenFlash(new ScreenFlash(0.4f)
					{
						Frequency = 1f,
						EffectColor = effectColor
					});
					EmperorDeathSmokeParticleSystem emperorDeathSmokeParticleSystem = new EmperorDeathSmokeParticleSystem(_level.GCM.TxParticleSmoke, 1, _isViletianEmperor);
					_deathParticleSystems[2] = emperorDeathSmokeParticleSystem;
					emperorDeathSmokeParticleSystem.AddParticles(base.DeathPosition.ToVector2());
				}
			}
			else if (deathScriptTimer < 3f)
			{
				_doesDrawSpriteAndAppendages = false;
				_doesDrawTrail = false;
				_doesDrawBrushTrail = false;
				base.DoesDrawAura = false;
				_orbManager.SetAreOrbsHidden(areOrbsHidden: true, doesShowAnimation: false);
				_level.RequestScreenShake(new Vector2(0f, 5f), 1f, 24f, isAffectedByTime: true);
				ShowDeathShockwave();
				int x2 = Position.X;
				_ = IsFacingLeft;
				base.DeathPosition = new Point(x2, Position.Y + -48);
				Point position = Position;
				_ = IsFacingLeft;
				Point inPosition = position.Add(0, -48);
				BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsLarge, inPosition, _level);
				battleAnimation.TeamSide = ETeamSide.Neutral;
				battleAnimation.AnimationSpeed = 0.04f;
				battleAnimation.AnimationStart = 18;
				battleAnimation.AnimationLength = 7;
				BattleAnimation newAnimation = battleAnimation;
				_level.AddAnimation(newAnimation);
				EmperorDeathSmokeParticleSystem emperorDeathSmokeParticleSystem2 = new EmperorDeathSmokeParticleSystem(_level.GCM.TxParticleSmoke, 1, _isViletianEmperor);
				_deathParticleSystems[2] = emperorDeathSmokeParticleSystem2;
				emperorDeathSmokeParticleSystem2.AddParticles(base.DeathPosition.ToVector2());
				_level.SetLevelDrawColor(Color.White);
				SetBackgroundColor(Color.White);
				_deathChargeParticles.KillOffParticles(0f);
				_deathLazerParticles.KillOffParticles(0f);
				if (!_isViletianEmperor && !_isPrinceEmperor)
				{
					DropLoot();
				}
			}
		}
		else
		{
			EndDeathSequence();
		}
		ParticleSystem[] deathParticleSystems = _deathParticleSystems;
		for (int i = 0; i < deathParticleSystems.Length; i++)
		{
			deathParticleSystems[i]?.Update(delta);
		}
	}

	private void ShowDeathShockwave()
	{
		EEmperorOrbType orbType = ((!_isViletianEmperor) ? EEmperorOrbType.Blue : EEmperorOrbType.Plasma);
		Color shockwaveColor = EmperorOrbManager.GetChargeColorByEmperorOrbType(orbType).ToColor();
		_orbManager.ResetShockwave(shockwaveColor);
	}

	private void EndDeathSequence()
	{
		RemoveInstance();
		DoStartEndingCutscene();
	}

	private void DoStartEndingCutscene()
	{
		if (_isViletianEmperor || _isPrinceEmperor)
		{
			CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Alt2_Win, _level, Position);
			return;
		}
		SaveBossDeath();
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
		_level.OpenAllBossDoors(1f);
		_level.ToggleExits(isEnabled: true);
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.EmpTower1_Win, _level, Position);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_orbManager.DrawOrbs(spriteBatch, isOnFrontPlane: false);
		base.Draw(spriteBatch);
		_orbManager.DrawOrbs(spriteBatch, isOnFrontPlane: true);
	}

	public override void Freeze()
	{
		base.Freeze();
		_orbManager.Freeze();
	}

	public override void Unfreeze()
	{
		base.Unfreeze();
		_orbManager.Unfreeze();
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (flag && base.HP > 0 && _hurtCueTimer <= 0f)
		{
			double num = _level.NextRandomDouble();
			if (num <= 0.15000000596046448)
			{
				PlayHurtCue();
				_hurtCueTimer = 2f;
			}
		}
		return flag;
	}

	private void PlayBigEffortCue()
	{
		PlayCue(_isViletianEmperor ? ESFX.VO_Duke_BigEffort : ESFX.VO_Emp_BigEffort);
	}

	private void PlaySmallEffortCue()
	{
		PlayCue(_isViletianEmperor ? ESFX.VO_Duke_SmallEffort : ESFX.VO_Emp_SmallEffort);
	}

	private void PlayHurtCue()
	{
		PlayCue(_isViletianEmperor ? ESFX.VO_Duke_Hurt : ESFX.VO_Emp_Hurt);
	}

	private void PlayMeleeCastCue()
	{
		switch (_currentOrbSet)
		{
		case EEmperorOrbType.Blue:
			PlayCue(IsFacingLeft ? ESFX.BossEmperorBlueMeleeCast : ESFX.BossEmperorBlueMeleeCastRight);
			break;
		case EEmperorOrbType.Fire:
			PlayCue(IsFacingLeft ? ESFX.BossVolTerrilisFireMeleeCast : ESFX.BossVolTerrilisFireMeleeCastRight);
			break;
		case EEmperorOrbType.Empire:
			break;
		}
	}

	internal void MoveForCutscene(Point targetPoint, float duration, bool isSinusoidal)
	{
		IsMovingDuringCutscene = true;
		_moveTargetPosition = targetPoint;
		_moveStartPosition = _floatingBasePosition.ToPoint();
		_timeForCutsceneMove = duration;
		_isCutsceneMoveSinusoidal = isSinusoidal;
		_emperorStateTimer = 0f;
		_lastEmperorStateTimer = -1f;
	}

	private void UpdateCutsceneMoving(float delta)
	{
		if (_emperorStateTimer < _timeForCutsceneMove && _timeForCutsceneMove > 0f)
		{
			float num = _emperorStateTimer / _timeForCutsceneMove;
			float x = MathEx.SineInterpolate(_moveStartPosition.X, _moveTargetPosition.X, num);
			float num2 = MathEx.SineInterpolate(_moveStartPosition.Y, _moveTargetPosition.Y, num);
			float y = num2;
			if (_isCutsceneMoveSinusoidal)
			{
				y = (float)(Math.Cos(num * ((float)Math.PI * 2f)) * 24.0) + num2;
			}
			_floatingBasePosition = new Vector2(x, y);
		}
		else
		{
			_floatingBasePosition = _moveTargetPosition.ToVector2();
			IsMovingDuringCutscene = false;
		}
		_lastEmperorStateTimer = _emperorStateTimer;
		_emperorStateTimer += delta;
	}

	private void SetBackgroundColor(Color newColor)
	{
		foreach (Background background in _level.Backgrounds)
		{
			background.DrawColor = newColor;
		}
	}
}
