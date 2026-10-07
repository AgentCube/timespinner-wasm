using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Shapeshifter;
using Timespinner.GameObjects.Enemies;
using Timespinner.GameObjects.Items;

namespace Timespinner.GameObjects.Bosses;

internal sealed class ShapeshifterBoss : BossClass
{
	private enum EShapeshifterState
	{
		None,
		Idle,
		Move,
		Shoot,
		Stab,
		Crush,
		Laugh
	}

	private const int RoomCenterX = 192;

	private const int StabDistanceXThreshold = 96;

	private const int CrushStartY = 48;

	private const int BloodCoughOffsetY = -13;

	private const int CrushOffsetX = 32;

	private const float TimeToMelt = 0.5f;

	private const float TimeToUnmelt = 0.5f;

	private const float TimeToMove = 1f;

	private const float TimeBeforeUnmelting = 1.5f;

	private const float TimeForEntireMove = 2f;

	private const float TimeBeforeThrowingBackstabber = 0.58f;

	private const float TimeBeforeEndingStabAttack = 1.08f;

	private const float TimeToLaughAfterShootingArrows = 1f;

	private const float TimeForDeathFadeSequence = 1f;

	private const float TimeToBrightenRoom = 1f;

	private const float BackgroundFadeColor = 0.25f;

	private readonly int _baseTouchDamage;

	private readonly float _healthyThreshold;

	private static readonly Color MidDeathPurpleFadeColor = new Color(0.75f, 0.3f, 0.7f, 0.4f);

	private static readonly Vector4 DeathParticlesColor = new Vector4(0.75f, 0.5f, 0.7f, 1f);

	private static readonly int[] MoveNodePositions = new int[5] { 72, 136, 200, 264, 328 };

	private readonly CharacterSequenceSpecification _morphSequence;

	private readonly CharacterSequenceSpecification _stabSequence;

	private readonly CharacterSequenceSpecification _meltSequence;

	private readonly CharacterSequenceSpecification _unmeltSequence;

	private readonly CharacterSequenceSpecification _laughSequence;

	private readonly CharacterSequenceSpecification _throwSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly CharacterSequenceSpecification _coughSequence;

	private readonly ShapeshifterBossCeilingGoo _ceilingGoo;

	private readonly ShapeshifterBossDeathParticleSystem _deathSparkles;

	private readonly ShapeshifterBossBloodParticleSystem _bloodCoughParticles;

	private static readonly Color BackgroundShadowColor = new Color(0.25f, 0.25f, 0.25f, 1f);

	private bool _isMelted;

	private bool _isHealthy;

	private bool _isRepeatingAbility;

	private bool _isReadyToFadeDie;

	private bool _hasInitiatedDeathDialog;

	private bool _isWaitingToStartDeathSequence;

	private bool _isReadyToDoDeathCutscene;

	private bool _isBrighteningRoom;

	private EShapeshifterState _shapeshifterState;

	private EShapeshifterState _nextShapeshifterState;

	private int _moveNodeIndex;

	private int _lastNonMoveAction;

	private int _lastStandingLocationX;

	private int _moveTargetPositionX;

	private float _shapeshifterStateTimer;

	private float _lastShapeshifterStateTimer;

	private float _roomBrightenTimer;

	public ShapeshifterBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_baseTouchDamage = _damageCaused;
		_healthyThreshold = (float)base.MaxHP * 0.5f;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_agility = 1f;
		IsFacingLeft = false;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 31);
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		_ceilingGoo = new ShapeshifterBossCeilingGoo(_level, Position, _sprite, new ObjectTileSpecification());
		_level.RequestAddObject(_ceilingGoo);
		if (base.CharacterSpecification != null)
		{
			_morphSequence = GetCharacterSequenceByName("Morph");
			_stabSequence = GetCharacterSequenceByName("Stab");
			_meltSequence = GetCharacterSequenceByName("Melt");
			_unmeltSequence = GetCharacterSequenceByName("Unmelt");
			_laughSequence = GetCharacterSequenceByName("Laugh");
			_throwSequence = GetCharacterSequenceByName("Throw");
			_turnSequence = GetCharacterSequenceByName("Turn");
			_deathSequence = GetCharacterSequenceByName("Death");
			_coughSequence = GetCharacterSequenceByName("Cough");
		}
		_deathSparkles = new ShapeshifterBossDeathParticleSystem(_level.GCM.TxParticleEnergy, 1)
		{
			BaseColor = DeathParticlesColor
		};
		_bloodCoughParticles = new ShapeshifterBossBloodParticleSystem(_level.GCM.TxParticleEnergy, 1)
		{
			BaseColor = DeathParticlesColor
		};
		SetBackgroundColor(BackgroundShadowColor);
	}

	public override void InitializeMob()
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
		Point point = new Point(Position.X + 112 + 8, Position.Y);
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
		base.IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected override void StartBossIntroCutscene()
	{
		if (_level.GameSave.LastWarpLevel == 0)
		{
			_level.GameSave.LastWarpLevel = 11;
			_level.GameSave.LastWarpRoom = 33;
		}
		AddDialogue("cs_gen_0_gen_00");
		InventoryJournalCollection journalCollection = _level.GameSave.Inventory.JournalCollection;
		if (journalCollection.IsJournalRead(EInventoryJournalType.File18) || journalCollection.IsJournalRead(EInventoryJournalType.File19) || journalCollection.IsJournalRead(EInventoryJournalType.File20))
		{
			AddDialogue("cs_gen_0_lun_01");
			AddDialogue("cs_gen_0_gen_02");
		}
		else
		{
			AddDialogue("cs_gen_0_lun_03");
			AddDialogue("cs_gen_0_gen_04");
		}
		AddDialogue("cs_gen_0_gen_05");
		AddDialogue("cs_gen_0_lun_06");
		AddDialogue("cs_gen_0_gen_07");
		AddDialogue("cs_gen_0_gen_08");
		AddDialogue("cs_gen_0_lun_09");
		AddDialogue("cs_gen_0_gen_10");
		AddDialogue("cs_gen_0_gen_11");
		AddDialogue("cs_gen_0_gen_12");
		AddDialogue("cs_gen_0_gen_13");
		AddDelegateScript(delegate
		{
			_isBrighteningRoom = true;
			_level.SetIsOverridingPowerOff(newValue: true);
		});
		AddWaitScript(2.5f);
		AddDialogue("cs_gen_0_gen_14");
		AddDialogue("cs_gen_0_lun_15");
		AddDialogue("cs_gen_0_gen_16");
		AddDialogue("cs_gen_0_lun_17");
		AddDialogue("cs_gen_0_gen_18");
		AddDialogue("cs_gen_0_lun_19");
		AddDialogue("cs_gen_0_gen_20");
		AddDialogue("cs_gen_0_lun_21");
		AddDialogue("cs_gen_0_gen_22");
		AddLevelScriptAction(new ScriptAction(ESFX.BossShapeshifterCutsceneShift, Position));
		AddLevelScriptAction(new ScriptAction(_morphSequence, this));
		AddWaitScript(0.25f);
		AddDialogue("cs_gen_0_lun_23");
		AddDialogue("cs_gen_0_qse_24");
		AddDialogue("cs_gen_0_lun_25");
		AddDialogue("cs_gen_0_qse_26");
		AddDialogue("cs_gen_0_qse_27");
		AddDialogue("cs_gen_0_lun_28");
		AddDialogue("cs_gen_0_qse_29");
		AddDialogue("cs_gen_0_lun_30");
		AddDialogue("cs_gen_0_qse_31");
		AddDialogue("cs_gen_0_lun_32");
		AddDelegateScript(PlayLaughCue);
		AddLevelScriptAction(new ScriptAction(_laughSequence, this));
		AddDelegateScript(EndBossIntroCutscene);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_isWaitingToStartDeathSequence)
		{
			_shapeshifterState = EShapeshifterState.Idle;
			_nextActionTimer = 1f;
			_isReadyToDoDeathCutscene = true;
		}
		else
		{
			_isRepeatingAbility = false;
			if (_shapeshifterState == EShapeshifterState.Idle)
			{
				_isAtTargetLocation = false;
				_startPosition = _position;
				_isHealthy = (float)base.HP >= _healthyThreshold;
				int num = _random.Next(0, 4);
				if (num == _lastNonMoveAction)
				{
					num = (num + 1) % 4;
				}
				_lastNonMoveAction = num;
				_nextActionTimer = 100f;
				_nextShapeshifterState = EShapeshifterState.None;
				switch (num)
				{
				case 0:
				{
					_shapeshifterState = EShapeshifterState.Move;
					int num2 = _level.NextRandomInt(0, 4);
					_moveNodeIndex = ((num2 != _moveNodeIndex) ? num2 : ((num2 + 1) % 5));
					break;
				}
				case 1:
					_shapeshifterState = EShapeshifterState.Stab;
					break;
				case 2:
					if (_moveNodeIndex != 2)
					{
						_nextShapeshifterState = EShapeshifterState.Crush;
						_shapeshifterState = EShapeshifterState.Move;
						_moveNodeIndex = 2;
					}
					else
					{
						_shapeshifterState = EShapeshifterState.Stab;
					}
					break;
				case 3:
					_nextShapeshifterState = EShapeshifterState.Shoot;
					_shapeshifterState = EShapeshifterState.Move;
					_moveNodeIndex = ((_level.GetNearestProtagonistPosition(Position).X < 192) ? 4 : 0);
					break;
				}
				_currentDestinationNode = num;
				_currentCustomAction = num;
				_totalActionTimer = _nextActionTimer;
				_shapeshifterStateTimer = 0f;
				_lastShapeshifterStateTimer = 0f;
				_moveTargetPositionX = MoveNodePositions[_moveNodeIndex];
				_lastStandingLocationX = Position.X;
				_currentAction = EAIAction.Custom;
			}
			else
			{
				_shapeshifterState = EShapeshifterState.Idle;
				_nextActionTimer = 1f;
			}
		}
		base.PickNextCustomScriptAIAction(delta);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_shapeshifterState)
		{
		case EShapeshifterState.Stab:
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			int num = Position.X - nearestProtagonistPosition.X;
			IsFacingLeft = num > 0;
			StartAbility((Math.Abs(num) > 96) ? 1 : 0);
			_nextActionTimer = 0f;
			break;
		}
		case EShapeshifterState.Crush:
			StartAbility(2);
			_nextActionTimer = 0f;
			break;
		case EShapeshifterState.Shoot:
			StartAbility(3);
			_nextActionTimer = 0f;
			break;
		case EShapeshifterState.Move:
			UpdateMoving();
			break;
		case EShapeshifterState.Laugh:
			if (_shapeshifterStateTimer <= 0f)
			{
				DoLaughSequence();
			}
			break;
		}
		_lastShapeshifterStateTimer = _shapeshifterStateTimer;
		_shapeshifterStateTimer += delta;
	}

	public override void UpdateAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			if (_lastAbilityTimer <= 0f && _abilityTimer >= 0f)
			{
				SetCharacterSequence(_stabSequence);
				PlayCue(ESFX.BossShapeshifterStab);
			}
			else if (_abilityTimer >= 1.08f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_lastAbilityTimer <= 0f && _abilityTimer >= 0f)
			{
				SetCharacterSequence(_throwSequence);
			}
			else
			{
				if (!(_abilityTimer >= 0.58f))
				{
					break;
				}
				Vector2 iV = new Vector2(IsFacingLeft ? (-200) : 200, -200f);
				Point inPosition = Position.Add(IsFacingLeft ? (-16) : 16, -20);
				ShapeshifterBackstabber newProjectile2 = new ShapeshifterBackstabber(_level, inPosition, iV, _sprite, _baseTouchDamage);
				_level.AddProjectile(newProjectile2);
				if (_isHealthy || _isRepeatingAbility)
				{
					EndAbility();
					break;
				}
				_isRepeatingAbility = true;
				_abilityTimer = 0f;
				_lastAbilityTimer = -1f;
				if (_level.GetNearestProtagonistPosition(Position).X < Position.X != IsFacingLeft)
				{
					IsFacingLeft = !IsFacingLeft;
				}
			}
			break;
		case 2:
			if (_lastAbilityTimer <= 0f && _abilityTimer >= 0f)
			{
				if (_isHealthy)
				{
					DoLaughSequence();
					break;
				}
				SetCharacterSequence(_stabSequence);
				PlayCue(ESFX.BossShapeshifterStab);
			}
			else if (_abilityTimer >= 0.58f)
			{
				int x = (IsFacingLeft ? (MoveNodePositions[1] - 32) : (MoveNodePositions[3] + 32));
				ShapeshifterCrusherProjectile newProjectile = new ShapeshifterCrusherProjectile(_level, new Point(x, 48), _sprite, _baseTouchDamage);
				_level.AddProjectile(newProjectile);
				EndAbility();
			}
			break;
		case 3:
			if (_lastAbilityTimer <= 0f && _abilityTimer >= 0f)
			{
				if (!_isHealthy)
				{
					DoLaughSequence();
				}
			}
			else if (_abilityTimer >= 1f)
			{
				EndAbility();
			}
			break;
		}
	}

	private void PlayLaughCue()
	{
		PlayCue(ESFX.BossShapeshifterSelenLaugh);
	}

	private void DoLaughSequence()
	{
		PlayCue(ESFX.BossShapeshifterSelenLaugh);
		SetCharacterSequence(_laughSequence);
	}

	public override void Update(float delta)
	{
		if (_isBrighteningRoom && _roomBrightenTimer < 1f)
		{
			_roomBrightenTimer += delta;
			Color backgroundColor;
			if (_roomBrightenTimer >= 1f)
			{
				backgroundColor = Color.White;
				_isBrighteningRoom = false;
			}
			else
			{
				float percentage = _roomBrightenTimer / 1f;
				percentage = MathEx.SineInterpolate(0.25f, 1f, percentage);
				backgroundColor = new Color(percentage, percentage, percentage, 1f);
			}
			SetBackgroundColor(backgroundColor);
		}
		base.Update(delta);
	}

	private void UpdateMoving()
	{
		if (_shapeshifterStateTimer <= 0f)
		{
			SetCharacterSequence(_meltSequence);
			PlayCue(ESFX.BossShapeshifterMelt);
		}
		else if (_shapeshifterStateTimer >= 0.5f && _shapeshifterStateTimer < 1.5f)
		{
			if (_lastShapeshifterStateTimer < 0.5f)
			{
				SetMeltedState(isMelted: true);
			}
			float num = (_shapeshifterStateTimer - 0.5f) / 1f;
			if (num >= 1f)
			{
				num = 1f;
			}
			Position = new Point((int)MathEx.SineInterpolate(_lastStandingLocationX, _moveTargetPositionX, num), Position.Y);
		}
		else
		{
			if (!(_shapeshifterStateTimer > 1.5f))
			{
				return;
			}
			if (_lastShapeshifterStateTimer <= 1.5f)
			{
				SetCharacterSequence(_unmeltSequence);
				PlayCue(ESFX.BossShapeshifterUnmelt);
				IsFacingLeft = _level.GetNearestProtagonistPosition(Position).X < Position.X;
				if (!_isWaitingToStartDeathSequence)
				{
					if (_nextShapeshifterState == EShapeshifterState.Crush)
					{
						ThrowGooIntoCeiling();
					}
					else if (_nextShapeshifterState == EShapeshifterState.Shoot)
					{
						ShootArrow();
					}
				}
			}
			if (_shapeshifterStateTimer >= 2f)
			{
				SetMeltedState(isMelted: false);
				if (_nextShapeshifterState != 0 && !_isWaitingToStartDeathSequence)
				{
					_shapeshifterState = _nextShapeshifterState;
					_shapeshifterStateTimer = 0f;
					_nextActionTimer = 100f;
				}
				else
				{
					_nextActionTimer = 0f;
				}
			}
		}
	}

	private void ThrowGooIntoCeiling()
	{
		_ceilingGoo.ThrowToCeiling(Position, IsFacingLeft);
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (!_isMelted && _turnSequence != null && !base.IsBossIntroInProgress)
		{
			SetCharacterSequence(_turnSequence);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	private void SetMeltedState(bool isMelted)
	{
		_isMelted = isMelted;
		_damageCaused = ((!isMelted) ? _baseTouchDamage : 0);
		_canBeDamaged = !isMelted;
		base.IsSolidWhenFrozen = !isMelted;
	}

	private void EndAbility()
	{
		_isCarryingOutAbility = false;
		_abilityTimer = 0f;
		_lastAbilityTimer = -1f;
		if (_isWaitingToStartDeathSequence)
		{
			_isReadyToDoDeathCutscene = true;
		}
	}

	private void ShootArrow()
	{
		int num = (_isHealthy ? 1 : 3);
		for (int i = 0; i < num; i++)
		{
			ShapeshifterArrow newProjectile = new ShapeshifterArrow(_level, Position, _sprite, _level.MainHero, _moveNodeIndex == 0, i, _baseTouchDamage);
			_level.AddProjectile(newProjectile);
		}
	}

	protected override void StartDeathScript()
	{
		if ((_moveNodeIndex == 0 && IsFacingLeft) || (_moveNodeIndex == 4 && !IsFacingLeft))
		{
			IsFacingLeft = !IsFacingLeft;
		}
		base.StartDeathScript();
		_isWaitingToStartDeathSequence = true;
		_level.PlayCue(ESFX.BossDeathFinalHit);
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_isReadyToDoDeathCutscene)
		{
			if (!_hasInitiatedDeathDialog)
			{
				InitiateDeathDialog();
				_hasInitiatedDeathDialog = true;
			}
			if (!_isReadyToFadeDie)
			{
				return;
			}
			float deathScriptTimer = _deathScriptTimer;
			_deathScriptTimer += delta;
			if (_deathScriptTimer < 1f)
			{
				if (deathScriptTimer <= 0f)
				{
					base.DrawColor = Color.White;
					_particleSystems.Add(_deathSparkles);
					Vector2 where = Position.ToVector2();
					_deathSparkles.AddParticles(where);
					_level.PlayCue(ESFX.BossShapeshifterDissipate, Position);
					base.DeathPosition = Position;
					DropLoot();
					PlaceKeycard(Position, _level);
				}
				float num = _deathScriptTimer / 1f;
				if (num < 0.5f)
				{
					base.DrawColor = Color.White.SineInterpolate(MidDeathPurpleFadeColor, num * 2f);
				}
				else
				{
					base.DrawColor = MidDeathPurpleFadeColor.SineInterpolate(Color.Transparent, (num - 0.5f) * 2f);
				}
			}
			else
			{
				EndBossDeathScript();
			}
		}
		else
		{
			UpdateMonsterAI(delta);
		}
	}

	private void InitiateDeathDialog()
	{
		InventoryJournalCollection journalCollection = _level.GameSave.Inventory.JournalCollection;
		bool flag = (journalCollection.IsJournalRead(EInventoryJournalType.Memory8) && journalCollection.IsJournalRead(EInventoryJournalType.Memory4)) || journalCollection.IsJournalRead(EInventoryJournalType.Memory9) || journalCollection.IsJournalRead(EInventoryJournalType.Memory10);
		_particleSystems.Add(_bloodCoughParticles);
		StopCharacterSequences();
		PlayCue(ESFX.BossShapeshifterDeath);
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction(_deathSequence, this));
		MovePlayerToTalkingPosition(shouldBeFaceToFace: true, -1);
		AddWaitScript(2f);
		AddDialogue("cs_gen_1_gen_00");
		AddDialogue("cs_gen_1_lun_01");
		AddLevelScriptAction(new ScriptAction(_coughSequence, this));
		AddWaitScript(0.5f);
		AddDialogue("cs_gen_1_gen_02");
		AddDialogue((!flag) ? "cs_gen_1_lun_03" : "cs_gen_1_lun_04");
		AddDialogue("cs_gen_1_gen_05");
		AddDialogue("cs_gen_1_gen_06");
		AddLevelScriptAction(new ScriptAction(_coughSequence, this));
		AddDialogue("cs_gen_1_gen_07");
		AddDialogue("cs_gen_1_gen_08");
		if (flag)
		{
			AddDialogue("cs_gen_1_lun_09");
		}
		else
		{
			AddDialogue("cs_gen_1_lun_10");
			AddLevelScriptAction(new ScriptAction(_coughSequence, this));
			AddDialogue("cs_gen_1_gen_11");
			AddDialogue("cs_gen_1_lun_12");
			AddDialogue("cs_gen_1_lun_13");
		}
		AddDialogue("cs_gen_1_lun_14");
		AddDialogue("cs_gen_1_lun_15");
		AddLevelScriptAction(new ScriptAction(_coughSequence, this));
		AddDialogue("cs_gen_1_gen_16");
		AddDialogue("cs_gen_1_gen_17");
		AddDialogue("cs_gen_1_lun_18");
		AddLevelScriptAction(new ScriptAction(_coughSequence, this));
		AddDialogue("cs_gen_1_gen_19");
		AddLevelScriptAction(new ScriptAction(_coughSequence, this));
		AddWaitScript(0.5f);
		AddDelegateScript(EndDeathDialog);
	}

	private void EndDeathDialog()
	{
		_isReadyToFadeDie = true;
	}

	internal static void PlaceKeycard(Point position, Level level)
	{
		if (!level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(13))
		{
			BestiaryItemDropSpecification bestiaryItemDropSpecification = new BestiaryItemDropSpecification();
			bestiaryItemDropSpecification.Category = 4;
			bestiaryItemDropSpecification.Item = 13;
			BestiaryItemDropSpecification itemDropData = bestiaryItemDropSpecification;
			ItemDropPickup item = new ItemDropPickup(itemDropData, level, position, -1);
			level.AddItem(item);
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		int intArgument = specification.IntArgument;
		if (intArgument == 1)
		{
			PlayCue(ESFX.BossShapeshifterCough);
			_bloodCoughParticles.IsFacingLeft = IsFacingLeft;
			_bloodCoughParticles.AddParticles(new Vector2(Position.X, Position.Y + -13));
		}
	}

	private void SetBackgroundColor(Color newColor)
	{
		foreach (Background background in _level.Backgrounds)
		{
			background.DrawColor = newColor;
		}
	}
}
