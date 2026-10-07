using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;
using Timespinner.GameObjects.NPCs.City;
using Timespinner.GameObjects.NPCs.Misc;
using Timespinner.GameObjects.StatusEffects;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.Shop;

namespace Timespinner.GameObjects.BaseClasses;

public class NPCBase : Alive
{
	public enum ENPCTriggerType
	{
		None,
		Touch,
		Talk
	}

	public enum ENPCAIType
	{
		None,
		Follow,
		EscortFollow
	}

	public enum ENPCType
	{
		Astrologer = 0,
		Medic = 1,
		Quartermaster = 2,
		Captain = 3,
		SickSoldier = 4,
		MerchantCrow = 5,
		Selen = 6,
		Librarian = 7,
		CultistPriest = 8,
		CultistWorshipper = 9,
		Yorne = 10,
		Marella = 11,
		Faron = 12,
		Jiana = 13,
		Messenger = 14,
		Elder = 15,
		Aelana = 16,
		Philia = 17,
		ScientistA = 18,
		PastAdvisor = 19,
		PastKnight = 20,
		Child = 21,
		PastSickArcher = 22,
		ScientistB = 23,
		FutureAdvisor = 24,
		FutureKnight = 25,
		FindMedic = 32,
		FindQuartermaster = 33
	}

	internal enum EQuestStateType
	{
		Unknown,
		Started,
		InProgress,
		ReadyToTurnIn,
		Closed,
		QuestsCapped
	}

	private const int DefaultTalkingStandingOffsetX = 32;

	private const int FollowMinimumThresholdX = 32;

	private const int FollowBubbleThresholdX = 24;

	private const int EscortEnemyThresholdX = 48;

	private const int FirstFloorY = 368;

	private const int SecondFloorY = 160;

	private const int NearEntranceX = 976;

	private const int NearEschemX = 1032;

	private const int NearEschemX2 = 984;

	private const int NearHaristelX = 616;

	private const int NearSeykisX = 120;

	private const int NearSeykisX2 = 136;

	internal const int Astrologer_Quest_1_Required = 1;

	internal const int Astrologer_Quest_4_Required = 3;

	internal const int Astrologer_Quest_5_Required = 1;

	internal const int Captain_Quest_1_Required = 15;

	internal const int Captain_Quest_2_Required = 10;

	internal const int Captain_Quest_3_Required = 20;

	internal const int Captain_Quest_4_Required = 8;

	internal const int Captain_Quest_5_Required = 1;

	internal const int Medic_Quest_1_Required = 4;

	internal const int Medic_Quest_2_Required = 2;

	internal const int Medic_Quest_3_Required = 1;

	internal const int Medic_Quest_4_Required = 1;

	internal const int Medic_Quest_5_Required = 1;

	internal const int Quartermaster_Quest_1_Required = 5;

	internal const int Quartermaster_Quest_2_Required = 3;

	internal const int Quartermaster_Quest_3_Required = 1;

	internal const int Quartermaster_Quest_4_Required = 1;

	internal const int Quartermaster_Quest_5_Required = 1;

	internal const int SickSoldier_Quest_1_Required = 1;

	internal const int SickSoldier_Quest_2_Required = 1;

	private const float EscortScaredToMoveTime = 0.25f;

	internal const string PrimaryProgressFormat = "NPC_Progress_{0}";

	internal const string SubProgressFormat = "NPC_SubProgress_{0}";

	internal const string StartingProgressFormat = "NPC_StartingProgress_{0}";

	internal const string IsUnlockedFormat = "NPC_Unlocked_{0}";

	private readonly NPCQuestIndicator _questIndicator;

	protected bool _hasTriggerBbox;

	protected bool _canTriggerRepeatedly;

	protected bool _isSolid;

	protected bool _wasFollowTargetToLeft;

	private bool _isWaitingToBeRemoved;

	protected ENPCTriggerType _npcTriggerType;

	protected ENPCAIType _npcAIType;

	protected ENPCType _npcType;

	private int _primaryProgress;

	private int _subProgress;

	private float _removalTimer;

	private float _scaredToMoveTimer;

	protected float _triggerCooldownTimer;

	protected float _timeForTriggerToCooldown = 0.5f;

	private Rectangle _triggerBbox;

	private ScriptAction _mostRecentScript;

	internal Point TriggerBboxOffset { get; set; }

	protected Rectangle TriggerBbox
	{
		get
		{
			return _triggerBbox;
		}
		set
		{
			_triggerBbox = value;
			_hasTriggerBbox = true;
		}
	}

	internal bool IsTalking { get; private set; }

	internal bool IsSpawnedForCutscene { get; set; }

	internal bool DoesDrawTriggerBbox { get; set; }

	internal bool CannotBeTalkedTo { get; set; }

	internal bool DoesNeedZoneBeforeNextQuest { get; set; }

	internal bool IsTalkingOffsetXFlipped { get; set; }

	internal bool IsAtAltSpawnPoint { get; private set; }

	internal ENPCType NPCType => _npcType;

	internal int PrimaryProgress => _primaryProgress;

	internal int SubProgress => _subProgress;

	internal float Agility
	{
		get
		{
			return _agility;
		}
		set
		{
			_agility = value;
		}
	}

	internal string PrimaryProgressKeyword => $"NPC_Progress_{_npcType}";

	internal string SubProgressKeyword => $"NPC_SubProgress_{_npcType}";

	internal NPCQuestIndicator QuestIndicator => _questIndicator;

	internal int TalkingStandingOffsetX { get; set; }

	internal Point TalkStandingLocation
	{
		get
		{
			bool flag = IsFacingLeft;
			if (IsTalkingOffsetXFlipped)
			{
				flag = !flag;
			}
			if (!flag)
			{
				return new Point(Position.X + TalkingStandingOffsetX, Position.Y);
			}
			return new Point(Position.X - TalkingStandingOffsetX, Position.Y);
		}
	}

	public bool CanBeTriggerered
	{
		get
		{
			if (_mostRecentScript != null && _mostRecentScript.IsFinished)
			{
				_mostRecentScript = null;
				_triggerCooldownTimer = _timeForTriggerToCooldown;
			}
			bool result = _triggerCooldownTimer <= 0f;
			if (!_canTriggerRepeatedly && _mostRecentScript != null)
			{
				result = false;
			}
			if (_level.IsInBossRoom || CannotBeTalkedTo)
			{
				result = false;
			}
			return result;
		}
	}

	public NPCBase(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet, int inID)
		: base(inPosition, inLevel, inSpriteSheet, inID)
	{
		TalkingStandingOffsetX = 32;
		base.BaseType = EGameObjectBaseType.NPC;
		_questIndicator = new NPCQuestIndicator(this, new Point(13, 13), new Point(2, 0), _level, _level.GCM.SpLunaisHUD);
		base.Appendages.Add(_questIndicator);
	}

	public void ChangeNPCAIType(ENPCAIType newType)
	{
		_npcAIType = newType;
	}

	public void SetPrimaryProgress(int value)
	{
		bool flag = NPCType != ENPCType.Librarian && NPCType != ENPCType.MerchantCrow && NPCType != ENPCType.CultistPriest && NPCType != ENPCType.CultistWorshipper;
		if (flag)
		{
			_level.AddScript(new ScriptAction(NPCType, value));
		}
		_primaryProgress = value;
		_subProgress = 0;
		SetSaveInt(PrimaryProgressKeyword, _primaryProgress);
		SetSaveInt(SubProgressKeyword, _subProgress);
		if (!flag)
		{
			return;
		}
		_level.GameSave.UnlockFeat(EGameFeatType.FinishQuest);
		EQuestStateType questProgress = GetQuestProgress();
		if (questProgress == EQuestStateType.QuestsCapped)
		{
			_level.GameSave.UnlockFeat(EGameFeatType.FinishQuestLine);
			if (NPCType == ENPCType.Astrologer)
			{
				_level.GameSave.UnlockFeat(EGameFeatType.FinishAllQuests);
			}
		}
	}

	public void SetSubProgress(int value)
	{
		_subProgress = value;
		SetSaveInt(SubProgressKeyword, _subProgress);
	}

	internal void SetInitialKillProgress(EEnemyTileType enemyType)
	{
		int enemyKillCount = GetEnemyKillCount(enemyType, _level.GameSave);
		_level.GameSave.SetValue($"NPC_StartingProgress_{enemyType}", enemyKillCount);
	}

	internal static int GetInitialKillCount(EEnemyTileType enemyType, GameSave saveFile)
	{
		return saveFile.GetSaveInt($"NPC_StartingProgress_{enemyType}");
	}

	public virtual void Initialize()
	{
		if (IsNPCUnlocked(_npcType, _level.GameSave) || IsSpawnedForCutscene)
		{
			LoadSaveData();
			if (IsSpawnedForCutscene)
			{
				return;
			}
			Vector3 altSpawnPointByNPCAndSave = GetAltSpawnPointByNPCAndSave(_npcType, _level.GameSave);
			if (altSpawnPointByNPCAndSave != Vector3.Zero)
			{
				Position = new Point((int)altSpawnPointByNPCAndSave.X, (int)altSpawnPointByNPCAndSave.Y);
				IsAtAltSpawnPoint = true;
				if (Math.Abs(altSpawnPointByNPCAndSave.Z) > 0.1f)
				{
					IsFacingLeft = altSpawnPointByNPCAndSave.Z < 0f;
				}
				ShrinkTriggerBboxToBbox();
			}
		}
		else
		{
			SilentKill();
		}
	}

	public virtual void LoadSaveData()
	{
		_primaryProgress = GetSaveInt(PrimaryProgressKeyword);
		_subProgress = GetSaveInt(SubProgressKeyword);
	}

	public bool CheckForHeroCollision(Protagonist hero)
	{
		bool result = false;
		if (_hasTriggerBbox && !_isFrozen && CanBeTriggerered && Position.X < _level.RoomSize.X && !_areActiveScriptsGoing)
		{
			Vector2 intersectionDepth = hero.Bbox.GetIntersectionDepth(TriggerBbox);
			if (intersectionDepth != Vector2.Zero)
			{
				switch (_npcTriggerType)
				{
				case ENPCTriggerType.Touch:
					TriggerNPC(hero);
					break;
				case ENPCTriggerType.Talk:
					if (!IsTalking)
					{
						_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
						if (hero.CheckButton(4) && !hero.CheckButton(5) && !hero.CheckButton(7))
						{
							TriggerNPC(hero);
						}
					}
					break;
				}
			}
		}
		Vector2 intersectionDepth2 = hero.Bbox.GetIntersectionDepth(Bbox);
		if (intersectionDepth2 != Vector2.Zero)
		{
			result = HeroCollide(hero, intersectionDepth2);
		}
		return result;
	}

	public virtual bool HeroCollide(Protagonist who, Vector2 depth)
	{
		bool result = false;
		if (_isSolid)
		{
			result = who.CollideSolidObject(this, ETileType.Event, depth);
		}
		return result;
	}

	protected virtual void TriggerNPC(Protagonist who)
	{
		_triggerCooldownTimer = _timeForTriggerToCooldown;
	}

	internal void StartNPCDialogue()
	{
		IsTalking = true;
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon
		});
	}

	internal void EndNPCDialogue()
	{
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				IsTalking = false;
			}
		});
	}

	public override void Update(float delta)
	{
		if (_triggerCooldownTimer > 0f)
		{
			_triggerCooldownTimer -= delta;
			if (_triggerCooldownTimer < 0f)
			{
				_triggerCooldownTimer = 0f;
			}
		}
		if (_isWaitingToBeRemoved && _removalTimer >= 0f)
		{
			_removalTimer -= delta;
			if (_removalTimer < 0f || !_areActiveScriptsGoing)
			{
				_level.RequestRemoveObject(this);
			}
		}
		if (delta > 0f)
		{
			UpdateQuestIndicator();
		}
		base.Update(delta);
		if (!_isFrozen)
		{
			UpdateAIState(delta);
		}
	}

	private void UpdateQuestIndicator()
	{
		if (!_level.IsDialoguePlaying && !_areActiveScriptsGoing && !_level.IsPlayerInputBlocked && !_level.IsInBossRoom && !CannotBeTalkedTo)
		{
			if (_questIndicator.IsAwaitingNewIcon)
			{
				_questIndicator.IsAwaitingNewIcon = false;
				_questIndicator.ChangeQuestState(GetQuestIndicatorIcon());
			}
		}
		else
		{
			_questIndicator.ChangeQuestState(NPCQuestIndicator.EQuestIndicatorIcon.None);
			_questIndicator.IsAwaitingNewIcon = true;
		}
	}

	public override void SnapBboxToPosition()
	{
		base.SnapBboxToPosition();
		if (_hasTriggerBbox)
		{
			_triggerBbox.Location = new Point(_bbox.Center.X - _triggerBbox.Width / 2 + (IsFacingLeft ? TriggerBboxOffset.X : (-TriggerBboxOffset.X)), _bbox.Center.Y - _triggerBbox.Height / 2 + TriggerBboxOffset.Y);
		}
	}

	private void UpdateAIState(float delta)
	{
		switch (_npcAIType)
		{
		case ENPCAIType.Follow:
		case ENPCAIType.EscortFollow:
		{
			bool flag = _npcAIType == ENPCAIType.EscortFollow;
			Protagonist mainHero = _level.MainHero;
			if (mainHero == null)
			{
				break;
			}
			EAFSM eAFSM = EAFSM.Idle;
			_movementX = 0f;
			if (mainHero.Position.X + 24 < Position.X && !flag)
			{
				if ((_currentState == EAFSM.Running || mainHero.Position.X + 32 < Position.X) && _wasFollowTargetToLeft)
				{
					_movementX = -1f;
					IsFacingLeft = true;
					eAFSM = EAFSM.Moving;
				}
			}
			else if (mainHero.Position.X - 24 > Position.X && (_currentState == EAFSM.Running || mainHero.Position.X - 32 > Position.X) && !_wasFollowTargetToLeft)
			{
				_movementX = 1f;
				IsFacingLeft = false;
				eAFSM = EAFSM.Moving;
			}
			if (_scaredToMoveTimer > 0f)
			{
				_scaredToMoveTimer -= delta;
			}
			if (eAFSM == EAFSM.Moving && flag)
			{
				Monster nearestEnemy = _level.GetNearestEnemy(Position, shouldBeVisible: true, shouldBeAggroed: true);
				if (nearestEnemy != null)
				{
					int num = Math.Abs(nearestEnemy.Position.X - Position.X);
					if (num < 48)
					{
						_scaredToMoveTimer = 0.25f;
					}
				}
				if (_scaredToMoveTimer > 0f)
				{
					eAFSM = EAFSM.Idle;
					_movementX = 0f;
				}
			}
			ManageState(eAFSM);
			_wasFollowTargetToLeft = mainHero.Position.X < Position.X;
			break;
		}
		}
	}

	protected override void CarryOutScriptAction(ScriptAction inAction, float delta)
	{
		EScriptActionType actionType = inAction.ActionType;
		if (actionType == EScriptActionType.Run)
		{
			if (inAction.ActionTimer > 0f)
			{
				DoHorizontalRun(inAction.Arguments.X);
			}
			else
			{
				ManageState(EAFSM.Idle);
				_movementX = 0f;
			}
		}
		base.CarryOutScriptAction(inAction, delta);
	}

	internal void AddScript(ScriptAction newScript)
	{
		_mostRecentScript = newScript;
		_level.AddScript(newScript);
	}

	internal void AddScript(string key)
	{
		ScriptAction mostRecentScript = _level.ShowDialogueMessage(key);
		_mostRecentScript = mostRecentScript;
	}

	internal void StartCutscene(CutsceneBase.ECutsceneType cutscene)
	{
		CutsceneBase.CreateAndCallCutscene(cutscene, _level, Position);
	}

	internal int GetSaveInt(string keyword)
	{
		return _level.GameSave.GetSaveInt(keyword);
	}

	internal void SetSaveInt(string keyword, int value)
	{
		_level.GameSave.SetValue(keyword, value);
	}

	internal static bool CheckInventoryCount(EInventoryUseItemType itemType, int requiredCount, GameSave saveFile)
	{
		return GetInventoryCount(itemType, saveFile) >= requiredCount;
	}

	internal static int GetInventoryCount(EInventoryUseItemType itemType, GameSave saveFile)
	{
		int result = 0;
		Dictionary<int, InventoryUseItem> inventory = saveFile.Inventory.UseItemInventory.Inventory;
		if (inventory.ContainsKey((int)itemType))
		{
			result = inventory[(int)itemType].Count;
		}
		return result;
	}

	internal static bool GetIsJournalFound(EInventoryJournalType journalType, GameSave saveFile)
	{
		bool result = false;
		Dictionary<int, InventoryJournal> inventory = saveFile.Inventory.JournalCollection.Inventory;
		if (inventory.ContainsKey((int)journalType))
		{
			result = true;
		}
		return result;
	}

	internal static bool GetIsRelicUnlocked(EInventoryRelicType relicType, GameSave saveFile)
	{
		bool result = false;
		Dictionary<int, InventoryRelic> inventory = saveFile.Inventory.RelicInventory.Inventory;
		if (inventory.ContainsKey((int)relicType))
		{
			result = true;
		}
		return result;
	}

	internal void RemoveInventoryItems(EInventoryUseItemType itemType, int howMany)
	{
		_level.GameSave.Inventory.UseItemInventory.RemoveItem((int)itemType, howMany);
	}

	internal void AddGiveItemScript(EInventoryUseItemType itemType, int howMany)
	{
		_level.AddScript(new ScriptAction(itemType, howMany));
	}

	internal void AddGiveItemScript(EInventoryRelicType relicType)
	{
		_level.AddScript(new ScriptAction(relicType, 1));
		_level.AddScript(new ScriptAction(relicType));
	}

	internal void AddGiveItemScript(EInventoryEquipmentType equipmentType)
	{
		_level.AddScript(new ScriptAction(equipmentType, 1));
	}

	internal int GetEnemyKillCount(EEnemyTileType enemyType)
	{
		return GetEnemyKillCount(enemyType, _level.GameSave);
	}

	internal static int GetEnemyKillCount(EEnemyTileType enemyType, GameSave saveFile)
	{
		return saveFile.GetSaveInt("KILL_" + enemyType);
	}

	internal static int GetNewEnemyKillCount(EEnemyTileType enemyType, GameSave saveFile)
	{
		return GetEnemyKillCount(enemyType, saveFile) - GetInitialKillCount(enemyType, saveFile);
	}

	internal virtual void OpenShop(ENPCType npcType, MerchantInventory merchandiseInventory)
	{
		MenuScreen newScreen;
		if (npcType == ENPCType.Astrologer)
		{
			Protagonist mainHero = _level.MainHero;
			if (mainHero != null)
			{
				CharacterStats characterStats = _level.GameSave.CharacterStats;
				characterStats.HP = mainHero.HP;
				characterStats.Sand = mainHero.MP;
				characterStats.Aura = mainHero.Aura;
				characterStats.CurrentStatus = ((mainHero.StatusEffects.Count > 0) ? mainHero.StatusEffects[0].StatusEffectType : EStatusEffectType.None);
			}
			newScreen = new OrbShopMenuScreen(_level.GameSave, _level.GCM, OnExitOrbShopMenu);
		}
		else
		{
			newScreen = new ShopLobbyMenuScreen(_level.GameSave, _level.GCM, npcType, merchandiseInventory);
		}
		_level.AddScreen(newScreen);
	}

	private void OnExitOrbShopMenu()
	{
		if (_level != null && _level.MainHero != null && _level.GameSave != null)
		{
			_level.MainHero.RefreshStats(_level.GameSave);
		}
	}

	internal void MovePlayerToTalkingPosition()
	{
		Point playerPosition = _level.GetPlayerPosition();
		Point talkStandingLocation = TalkStandingLocation;
		bool flag = IsFacingLeft;
		if (IsTalkingOffsetXFlipped)
		{
			flag = !flag;
		}
		bool flag2 = playerPosition.X > talkStandingLocation.X == flag;
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			Arguments = new Vector4(talkStandingLocation.X, talkStandingLocation.Y, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.1f,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		if (flag2)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.LookDirection,
				ActionTimer = 0.25f,
				DoesBlockQueue = true,
				Arguments = new Vector4(flag ? 1 : (-1), 0f, 0f, 0f)
			});
		}
	}

	internal static bool AreAnyQuestsActive(GameSave saveFile)
	{
		bool flag = false;
		foreach (ENPCType value in Enum.GetValues(typeof(ENPCType)))
		{
			flag = saveFile.GetSaveInt($"NPC_Progress_{value}") > 0;
			if (!flag)
			{
				flag = saveFile.GetSaveInt($"NPC_SubProgress_{value}") > 0;
			}
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	internal void GiveAelanaEmpathy()
	{
		int saveInt = _level.GameSave.GetSaveInt("AelEmpath");
		_level.GameSave.SetValue("AelEmpath", saveInt + 1);
	}

	internal bool GetIsBossDead(EBossType boss)
	{
		return _level.GameSave.GetSaveBool($"IsBossDead_{boss}");
	}

	internal static bool GetIsBossDead(EBossType boss, GameSave save)
	{
		return save.GetSaveBool($"IsBossDead_{boss}");
	}

	internal int GetDefaultSpeechNumber()
	{
		bool isBossDead = GetIsBossDead(EBossType.Demon);
		bool isBossDead2 = GetIsBossDead(EBossType.Maw);
		bool isBossDead3 = GetIsBossDead(EBossType.Sorceress);
		int primaryQuestState = GetPrimaryQuestState(ENPCType.Astrologer, _level.GameSave);
		if (primaryQuestState >= 5)
		{
			return 4;
		}
		if (!isBossDead && !isBossDead2 && !isBossDead3)
		{
			return 0;
		}
		if (isBossDead3)
		{
			return 3;
		}
		if (isBossDead2)
		{
			return 2;
		}
		return 1;
	}

	internal static bool GetAreQuestsFinished(ENPCType npcType, GameSave gameSave)
	{
		int primaryQuestState = GetPrimaryQuestState(npcType, gameSave);
		return GetQuestProgress(npcType, primaryQuestState, 0, gameSave) == EQuestStateType.QuestsCapped;
	}

	internal static int GetPrimaryQuestState(ENPCType npcType, GameSave gameSave)
	{
		return gameSave.GetSaveInt($"NPC_Progress_{npcType}");
	}

	internal static int GetSubQuestState(ENPCType npcType, GameSave gameSave)
	{
		return gameSave.GetSaveInt($"NPC_SubProgress_{npcType}");
	}

	internal EQuestStateType GetQuestProgress()
	{
		return GetQuestProgress(_npcType, PrimaryProgress, SubProgress, _level.GameSave);
	}

	internal static Point GetRatioQuestRatio(ENPCType npcType, int questID, GameSave saveFile)
	{
		Point result = new Point(-1, -1);
		switch (npcType)
		{
		case ENPCType.Astrologer:
			if (questID == 3)
			{
				result = new Point(GetInventoryCount(EInventoryUseItemType.PlasmaCore, saveFile), 3);
			}
			break;
		case ENPCType.Medic:
			switch (questID)
			{
			case 0:
				result = new Point(GetInventoryCount(EInventoryUseItemType.Herb, saveFile), 4);
				break;
			case 1:
				result = new Point(GetInventoryCount(EInventoryUseItemType.Mushroom, saveFile), 2);
				break;
			case 2:
				result = new Point(GetInventoryCount(EInventoryUseItemType.RadiationCrystal, saveFile), 1);
				break;
			case 3:
				result = new Point(GetInventoryCount(EInventoryUseItemType.PlasmaIV, saveFile), 1);
				break;
			case 4:
				result = new Point(GetInventoryCount(EInventoryUseItemType.HistoricalDocuments, saveFile), 1);
				break;
			}
			break;
		case ENPCType.Quartermaster:
			switch (questID)
			{
			case 0:
				result = new Point(GetInventoryCount(EInventoryUseItemType.Drumstick, saveFile), 5);
				break;
			case 1:
				result = new Point(GetInventoryCount(EInventoryUseItemType.WyvernTail, saveFile), 3);
				break;
			case 2:
				result = new Point(GetInventoryCount(EInventoryUseItemType.EelMeat, saveFile), 1);
				break;
			case 3:
				result = new Point(GetInventoryCount(EInventoryUseItemType.CheveuxBreast, saveFile), 1);
				break;
			case 4:
				result = new Point(GetInventoryCount(EInventoryUseItemType.FoodSynth, saveFile), 1);
				break;
			}
			break;
		case ENPCType.Captain:
			switch (questID)
			{
			case 0:
				result = new Point(GetNewEnemyKillCount(EEnemyTileType.ForestPlantBat, saveFile), 15);
				break;
			case 1:
				result = new Point(GetNewEnemyKillCount(EEnemyTileType.CavesSiren, saveFile), 10);
				break;
			case 2:
			{
				int newEnemyKillCount = GetNewEnemyKillCount(EEnemyTileType.CastleShieldKnight, saveFile);
				int newEnemyKillCount2 = GetNewEnemyKillCount(EEnemyTileType.CastleArcher, saveFile);
				result = new Point(newEnemyKillCount + newEnemyKillCount2, 20);
				break;
			}
			case 3:
				result = new Point(GetNewEnemyKillCount(EEnemyTileType.TowerRoyalGuard, saveFile), 8);
				break;
			case 4:
				result = new Point(GetNewEnemyKillCount(EEnemyTileType.CantoranBoss, saveFile), 1);
				break;
			}
			break;
		}
		return result;
	}

	internal static EQuestStateType GetQuestProgress(ENPCType npcType, int primaryProgress, int subProgress, GameSave saveFile)
	{
		EQuestStateType eQuestStateType = EQuestStateType.Unknown;
		if (subProgress > 0)
		{
			eQuestStateType = EQuestStateType.Started;
		}
		switch (npcType)
		{
		case ENPCType.Astrologer:
			switch (primaryProgress)
			{
			case 0:
				eQuestStateType = QuestStateByNumberAndRequired(GetInventoryCount(EInventoryUseItemType.AlchemistTools, saveFile), 1);
				break;
			case 1:
				eQuestStateType = QuestStateByBool(GetIsJournalFound(EInventoryJournalType.File4, saveFile));
				break;
			case 2:
				eQuestStateType = QuestStateByBool(GetIsJournalFound(EInventoryJournalType.File6, saveFile));
				break;
			case 3:
				eQuestStateType = QuestStateByNumberAndRequired(GetInventoryCount(EInventoryUseItemType.PlasmaCore, saveFile), 3);
				break;
			case 4:
				eQuestStateType = QuestStateByNumberAndRequired(GetInventoryCount(EInventoryUseItemType.GalaxyStone, saveFile), 1);
				break;
			case 5:
				eQuestStateType = EQuestStateType.QuestsCapped;
				break;
			}
			break;
		case ENPCType.Captain:
			switch (primaryProgress)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			{
				Point ratioQuestRatio3 = GetRatioQuestRatio(ENPCType.Captain, primaryProgress, saveFile);
				eQuestStateType = QuestStateByNumberAndRequired(ratioQuestRatio3.X, ratioQuestRatio3.Y);
				break;
			}
			case 5:
				eQuestStateType = EQuestStateType.QuestsCapped;
				break;
			}
			break;
		case ENPCType.Medic:
			switch (primaryProgress)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			{
				Point ratioQuestRatio2 = GetRatioQuestRatio(ENPCType.Medic, primaryProgress, saveFile);
				eQuestStateType = QuestStateByNumberAndRequired(ratioQuestRatio2.X, ratioQuestRatio2.Y);
				break;
			}
			case 5:
				eQuestStateType = EQuestStateType.QuestsCapped;
				break;
			}
			break;
		case ENPCType.Quartermaster:
			switch (primaryProgress)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			{
				Point ratioQuestRatio = GetRatioQuestRatio(ENPCType.Quartermaster, primaryProgress, saveFile);
				eQuestStateType = QuestStateByNumberAndRequired(ratioQuestRatio.X, ratioQuestRatio.Y);
				break;
			}
			case 5:
				eQuestStateType = EQuestStateType.QuestsCapped;
				break;
			}
			break;
		case ENPCType.SickSoldier:
			switch (primaryProgress)
			{
			case 0:
			{
				int inventoryCount = GetInventoryCount(EInventoryUseItemType.CheveuxFeather, saveFile);
				int inventoryCount2 = GetInventoryCount(EInventoryUseItemType.SirenInk, saveFile);
				eQuestStateType = ((inventoryCount <= 0 && inventoryCount2 <= 0) ? EQuestStateType.Started : ((inventoryCount < 1 || inventoryCount2 < 1) ? EQuestStateType.InProgress : EQuestStateType.ReadyToTurnIn));
				break;
			}
			case 1:
				eQuestStateType = QuestStateByNumberAndRequired(GetInventoryCount(EInventoryUseItemType.SilverOre, saveFile), 1);
				break;
			case 2:
				eQuestStateType = EQuestStateType.ReadyToTurnIn;
				break;
			case 3:
				eQuestStateType = EQuestStateType.QuestsCapped;
				break;
			}
			break;
		}
		if (subProgress <= 0 && eQuestStateType != EQuestStateType.QuestsCapped)
		{
			eQuestStateType = EQuestStateType.Unknown;
		}
		return eQuestStateType;
	}

	private static EQuestStateType QuestStateByNumberAndRequired(int current, int target)
	{
		EQuestStateType result = EQuestStateType.Unknown;
		if (current == 0)
		{
			result = EQuestStateType.Started;
		}
		else if (current < target)
		{
			result = EQuestStateType.InProgress;
		}
		else if (current >= target)
		{
			result = EQuestStateType.ReadyToTurnIn;
		}
		return result;
	}

	private static EQuestStateType QuestStateByBool(bool isFound)
	{
		if (!isFound)
		{
			return EQuestStateType.Started;
		}
		return EQuestStateType.ReadyToTurnIn;
	}

	internal static bool IsNextQuestAvailable(ENPCType npcType, int primaryProgress, GameSave save)
	{
		bool flag = true;
		switch (npcType)
		{
		case ENPCType.Astrologer:
			switch (primaryProgress)
			{
			case 0:
			{
				bool saveBool5 = save.GetSaveBool(GetIsNPCUnlockedKeyFromType(ENPCType.Captain));
				flag = saveBool5;
				break;
			}
			case 1:
				flag = save.Inventory.RelicInventory.Inventory.ContainsKey(6);
				break;
			case 2:
			{
				int primaryQuestState = GetPrimaryQuestState(ENPCType.Medic, save);
				int primaryQuestState6 = GetPrimaryQuestState(ENPCType.Captain, save);
				int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
				flag = primaryQuestState >= 1 && primaryQuestState6 >= 1 && primaryQuestState2 >= 1;
				break;
			}
			case 3:
				if (save.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw)))
				{
					flag = CutsceneBase.GetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.LakeSerene3_VileteSaved, save);
					if (!flag)
					{
						InventoryJournalCollection journalCollection = save.Inventory.JournalCollection;
						flag = journalCollection.IsJournalRead(EInventoryJournalType.File3) || journalCollection.IsJournalRead(EInventoryJournalType.File5);
					}
				}
				else
				{
					flag = save.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon)) && save.GetSaveBool("HasBeenToNewPresent");
				}
				break;
			case 4:
			{
				int primaryQuestState = GetPrimaryQuestState(ENPCType.Medic, save);
				int primaryQuestState6 = GetPrimaryQuestState(ENPCType.Captain, save);
				int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
				int primaryQuestState4 = GetPrimaryQuestState(ENPCType.SickSoldier, save);
				flag = primaryQuestState >= 5 && primaryQuestState6 >= 5 && primaryQuestState2 >= 5 && primaryQuestState4 >= 3;
				break;
			}
			}
			break;
		case ENPCType.Captain:
			switch (primaryProgress)
			{
			case 0:
			{
				bool flag2 = IsNPCUnlocked(ENPCType.Medic, save);
				bool saveBool4 = save.GetSaveBool(GetIsNPCUnlockedKeyFromType(ENPCType.Quartermaster));
				flag = flag2 && saveBool4;
				break;
			}
			case 4:
			{
				bool isBossDead = GetIsBossDead(EBossType.Sorceress, save);
				int primaryQuestState = GetPrimaryQuestState(ENPCType.Medic, save);
				flag = primaryQuestState >= 5 && isBossDead;
				break;
			}
			}
			break;
		case ENPCType.Medic:
		{
			int primaryQuestState5 = GetPrimaryQuestState(ENPCType.Astrologer, save);
			switch (primaryProgress)
			{
			case 1:
			{
				int subQuestState = GetSubQuestState(ENPCType.Astrologer, save);
				bool saveBool3 = save.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon));
				flag = primaryQuestState5 >= 2 || (primaryQuestState5 == 1 && subQuestState > 0) || saveBool3;
				break;
			}
			case 3:
			{
				int primaryQuestState7 = GetPrimaryQuestState(ENPCType.SickSoldier, save);
				flag = primaryQuestState5 >= 3 && primaryQuestState7 >= 1;
				break;
			}
			case 4:
			{
				bool saveBool2 = save.GetSaveBool("IsVileteSaved");
				int primaryQuestState6 = GetPrimaryQuestState(ENPCType.Captain, save);
				int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
				flag = saveBool2 && primaryQuestState6 >= 4 && primaryQuestState2 >= 4;
				break;
			}
			}
			break;
		}
		case ENPCType.Quartermaster:
			switch (primaryProgress)
			{
			case 1:
				flag = IsNPCUnlocked(ENPCType.Medic, save);
				break;
			case 2:
				flag = GetPrimaryQuestState(ENPCType.SickSoldier, save) >= 1;
				break;
			case 3:
			{
				int primaryQuestState = GetPrimaryQuestState(ENPCType.Medic, save);
				int primaryQuestState4 = GetPrimaryQuestState(ENPCType.SickSoldier, save);
				int primaryQuestState5 = GetPrimaryQuestState(ENPCType.Astrologer, save);
				flag = primaryQuestState >= 3 && primaryQuestState4 >= 2 && primaryQuestState5 >= 2;
				break;
			}
			case 4:
			{
				int primaryQuestState3 = GetPrimaryQuestState(ENPCType.Medic, save);
				flag = primaryQuestState3 >= 5;
				break;
			}
			}
			break;
		case ENPCType.SickSoldier:
			switch (primaryProgress)
			{
			case 0:
			{
				bool saveBool = save.GetSaveBool(GetIsNPCUnlockedKeyFromType(ENPCType.Quartermaster));
				flag = saveBool;
				break;
			}
			case 1:
			{
				int primaryQuestState = GetPrimaryQuestState(ENPCType.Medic, save);
				int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
				flag = primaryQuestState >= 3 && primaryQuestState2 >= 3;
				break;
			}
			case 2:
			{
				int primaryQuestState = GetPrimaryQuestState(ENPCType.Medic, save);
				int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
				flag = primaryQuestState >= 4 && primaryQuestState2 >= 4;
				break;
			}
			}
			break;
		}
		return flag;
	}

	private NPCQuestIndicator.EQuestIndicatorIcon GetQuestIndicatorIcon()
	{
		NPCQuestIndicator.EQuestIndicatorIcon result = NPCQuestIndicator.EQuestIndicatorIcon.None;
		switch (NPCType)
		{
		case ENPCType.Astrologer:
			result = NPCQuestIndicator.EQuestIndicatorIcon.OrbShop;
			break;
		case ENPCType.Quartermaster:
			result = NPCQuestIndicator.EQuestIndicatorIcon.ItemShop;
			break;
		case ENPCType.MerchantCrow:
			result = NPCQuestIndicator.EQuestIndicatorIcon.RingShop;
			break;
		case ENPCType.Librarian:
			if (LibrarianNPC.DoesHaveItemToGive(_level.GameSave))
			{
				result = NPCQuestIndicator.EQuestIndicatorIcon.FinishQuest;
			}
			break;
		}
		if (!DoesNeedZoneBeforeNextQuest)
		{
			switch (NPCType)
			{
			case ENPCType.Astrologer:
			case ENPCType.Medic:
			case ENPCType.Quartermaster:
			case ENPCType.Captain:
			case ENPCType.SickSoldier:
				switch (GetQuestProgress(NPCType, PrimaryProgress, SubProgress, _level.GameSave))
				{
				case EQuestStateType.Unknown:
					if (IsNextQuestAvailable(NPCType, PrimaryProgress, _level.GameSave))
					{
						result = NPCQuestIndicator.EQuestIndicatorIcon.NewQuest;
					}
					break;
				case EQuestStateType.ReadyToTurnIn:
					result = NPCQuestIndicator.EQuestIndicatorIcon.FinishQuest;
					break;
				}
				break;
			}
		}
		return result;
	}

	internal static string GetIsNPCUnlockedKeyFromType(ENPCType npcType)
	{
		return $"NPC_Unlocked_{npcType}";
	}

	internal static bool IsNPCUnlocked(ENPCType npcType, GameSave saveFile)
	{
		bool result = true;
		if (npcType == ENPCType.Astrologer || npcType == ENPCType.Captain || npcType == ENPCType.Quartermaster || npcType == ENPCType.Medic || npcType == ENPCType.SickSoldier)
		{
			result = saveFile.GetSaveBool(GetIsNPCUnlockedKeyFromType(npcType));
		}
		return result;
	}

	public static NPCBase FromArgumentAndLevel(Level level, Point tilePosition, int newObjectID, ObjectTileSpecification objectTileSpec)
	{
		NPCBase result = null;
		switch ((ENPCType)objectTileSpec.Argument)
		{
		case ENPCType.Astrologer:
			result = new AstrologerNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Medic:
			result = new MedicNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Captain:
			result = new CaptainNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Quartermaster:
			result = new QuartermasterNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.SickSoldier:
			result = new SickSoldierNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Librarian:
			result = new LibrarianNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.CultistPriest:
			result = new CultistNPC(level, tilePosition, newObjectID, objectTileSpec, isPriest: true);
			break;
		case ENPCType.CultistWorshipper:
			result = new CultistNPC(level, tilePosition, newObjectID, objectTileSpec, isPriest: false);
			break;
		case ENPCType.Yorne:
			result = new YorneNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Marella:
			result = new MarellaNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Faron:
			result = new FaronNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Jiana:
			result = new JianaNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Messenger:
			result = new MessengerNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Elder:
			result = new ElderNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Aelana:
			result = new AelanaNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.Philia:
			result = new PhiliaNPC(level, tilePosition, newObjectID, objectTileSpec);
			break;
		case ENPCType.ScientistA:
			result = new ScientistNPC(level, tilePosition, level.GCM.SpMerchantCrow, isScientist1: true, objectTileSpec.IsFlippedHorizontally);
			break;
		case ENPCType.ScientistB:
			result = new ScientistNPC(level, tilePosition, level.GCM.SpMerchantCrow, isScientist1: false, objectTileSpec.IsFlippedHorizontally);
			break;
		case ENPCType.FutureAdvisor:
			result = new FutureAdvisorNPC(level, tilePosition, level.GCM.SpMerchantCrow, objectTileSpec);
			break;
		case ENPCType.FutureKnight:
			result = new FutureKnightNPC(level, tilePosition, level.GCM.SpMerchantCrow, objectTileSpec);
			break;
		}
		return result;
	}

	internal void AddSummonMeyef()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Summon));
	}

	internal void AddDismissMeyef()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Dismiss));
	}

	internal void AddMeyefMew()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.MeyefMew));
		AddWaitScript(0.1f);
		AddLevelScriptAction(new ScriptAction(ESFX.MeyefMeow, _level.GetFamiliarPosition()));
		AddWaitScript(0.5f);
	}

	internal void ShrinkTriggerBboxToBbox()
	{
		TriggerBbox = Bbox;
		TriggerBboxOffset = Point.Zero;
	}

	internal static Vector3 GetAltSpawnPointByNPCAndSave(ENPCType npcType, GameSave save)
	{
		Vector3 result = Vector3.Zero;
		switch (npcType)
		{
		case ENPCType.Astrologer:
		{
			int primaryQuestState4 = GetPrimaryQuestState(ENPCType.Astrologer, save);
			bool saveBool = save.GetSaveBool(GetIsNPCUnlockedKeyFromType(ENPCType.Captain));
			bool flag2 = save.Inventory.RelicInventory.Inventory.ContainsKey(6);
			if (primaryQuestState4 == 0 && saveBool && !flag2)
			{
				result = new Vector3(976f, 368f, 1f);
			}
			break;
		}
		case ENPCType.Medic:
		{
			int primaryQuestState = GetPrimaryQuestState(ENPCType.SickSoldier, save);
			int subQuestState = GetSubQuestState(ENPCType.SickSoldier, save);
			EQuestStateType questProgress = GetQuestProgress(ENPCType.SickSoldier, primaryQuestState, subQuestState, save);
			if (primaryQuestState == 0 && questProgress == EQuestStateType.ReadyToTurnIn)
			{
				result = new Vector3(1032f, 160f, 0f);
				break;
			}
			int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
			int subQuestState2 = GetSubQuestState(ENPCType.Quartermaster, save);
			EQuestStateType questProgress2 = GetQuestProgress(ENPCType.Quartermaster, primaryQuestState2, subQuestState2, save);
			if (primaryQuestState2 == 3 && questProgress2 == EQuestStateType.ReadyToTurnIn)
			{
				result = new Vector3(984f, 160f, 1f);
				break;
			}
			bool isCutsceneTriggeredByType = CutsceneBase.GetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.CavesPast1_Camp, save);
			int primaryQuestState3 = GetPrimaryQuestState(ENPCType.Medic, save);
			if (primaryQuestState2 == 1 && isCutsceneTriggeredByType)
			{
				result = new Vector3(136f, 368f, 0f);
				break;
			}
			switch (primaryQuestState3)
			{
			case 3:
				result = new Vector3(1032f, 160f, 0f);
				break;
			case 4:
				result = new Vector3(616f, 368f, 0f);
				break;
			}
			break;
		}
		case ENPCType.Quartermaster:
		{
			int primaryQuestState = GetPrimaryQuestState(ENPCType.SickSoldier, save);
			int subQuestState = GetSubQuestState(ENPCType.SickSoldier, save);
			EQuestStateType questProgress = GetQuestProgress(ENPCType.SickSoldier, primaryQuestState, subQuestState, save);
			if (primaryQuestState == 0 && questProgress == EQuestStateType.ReadyToTurnIn)
			{
				result = new Vector3(984f, 160f, 0f);
				break;
			}
			int primaryQuestState2 = GetPrimaryQuestState(ENPCType.Quartermaster, save);
			int subQuestState2 = GetSubQuestState(ENPCType.Quartermaster, save);
			EQuestStateType questProgress2 = GetQuestProgress(ENPCType.Quartermaster, primaryQuestState2, subQuestState2, save);
			if (primaryQuestState2 == 3 && questProgress2 == EQuestStateType.ReadyToTurnIn)
			{
				result = new Vector3(1032f, 160f, 1f);
				break;
			}
			bool isCutsceneTriggeredByType = save.GetSaveBool(GetIsNPCUnlockedKeyFromType(ENPCType.SickSoldier));
			bool flag = !CutsceneBase.GetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.CavesPast1_Camp, save);
			if (primaryQuestState2 == 1 && isCutsceneTriggeredByType && !flag)
			{
				result = new Vector3(1032f, 160f, 0f);
			}
			break;
		}
		case ENPCType.SickSoldier:
		{
			int primaryQuestState = GetPrimaryQuestState(ENPCType.SickSoldier, save);
			if (primaryQuestState >= 3)
			{
				result = new Vector3(120f, 368f, 1f);
			}
			break;
		}
		}
		return result;
	}

	internal void RemoveNPC(float timeBeforeRemoving)
	{
		_isWaitingToBeRemoved = true;
		_removalTimer = timeBeforeRemoving;
		CannotBeTalkedTo = true;
	}

	internal static void TryShowQuestFinishedPopupByUseItem(Level level, EInventoryUseItemType useItemType)
	{
		ENPCType eNPCType = ENPCType.Aelana;
		int num = -1;
		int num2 = -1;
		switch (useItemType)
		{
		case EInventoryUseItemType.PlasmaCore:
			eNPCType = ENPCType.Astrologer;
			num = 3;
			num2 = 4;
			break;
		case EInventoryUseItemType.Herb:
			eNPCType = ENPCType.Medic;
			num = 4;
			num2 = 1;
			break;
		case EInventoryUseItemType.Mushroom:
			eNPCType = ENPCType.Medic;
			num = 2;
			num2 = 2;
			break;
		case EInventoryUseItemType.Drumstick:
			eNPCType = ENPCType.Quartermaster;
			num = 5;
			num2 = 1;
			break;
		case EInventoryUseItemType.WyvernTail:
			eNPCType = ENPCType.Quartermaster;
			num = 3;
			num2 = 2;
			break;
		case EInventoryUseItemType.CheveuxFeather:
		case EInventoryUseItemType.SirenInk:
			eNPCType = ENPCType.SickSoldier;
			num = 1;
			num2 = 1;
			break;
		}
		if (num2 <= -1 || num <= -1 || eNPCType == ENPCType.Aelana)
		{
			return;
		}
		GameSave gameSave = level.GameSave;
		if (!gameSave.Inventory.UseItemInventory.Inventory.ContainsKey((int)useItemType))
		{
			return;
		}
		int count = gameSave.Inventory.UseItemInventory.Inventory[(int)useItemType].Count;
		if (count != num)
		{
			return;
		}
		int primaryQuestState = GetPrimaryQuestState(eNPCType, gameSave);
		if (primaryQuestState + 1 != num2)
		{
			return;
		}
		int subQuestState = GetSubQuestState(eNPCType, gameSave);
		if (subQuestState != 1)
		{
			return;
		}
		bool flag = true;
		if (eNPCType == ENPCType.SickSoldier && num2 == 1)
		{
			flag = false;
			EInventoryUseItemType eInventoryUseItemType = ((useItemType == EInventoryUseItemType.SirenInk) ? EInventoryUseItemType.CheveuxFeather : EInventoryUseItemType.SirenInk);
			int key = (int)eInventoryUseItemType;
			if (gameSave.Inventory.UseItemInventory.Inventory.ContainsKey(key))
			{
				int count2 = gameSave.Inventory.UseItemInventory.Inventory[key].Count;
				if (count2 >= num)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			level.AddScript(new ScriptAction(eNPCType, num2));
		}
	}
}
