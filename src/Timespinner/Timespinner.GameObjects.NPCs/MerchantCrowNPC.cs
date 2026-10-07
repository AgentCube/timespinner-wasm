using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class MerchantCrowNPC : NPCBase
{
	private const string CrowCrowKey = "CS_CrowCrow";

	private readonly bool _isInPresent;

	private readonly bool _hasKilledFirstBoss;

	private readonly bool _hasKilledSecondBoss;

	private readonly bool _hasPlayerBeenToThePast;

	private readonly MerchantInventory _merchandiseInventory = new MerchantInventory();

	private bool _hasFoundCrowInPresent;

	private bool _hasFoundCrowInPast;

	private bool _hasDoneFirstConversation;

	private bool _hasDoneSecondEraConversation;

	private bool _hasDonePresentAfterPastConversation;

	private bool _hasDoneFirstBossConversation;

	private bool _hasDoneSecondBossConversation;

	private bool _hasHadOneConversation;

	private bool _hasDoneCrowCrowConversation;

	public MerchantCrowNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpMerchantCrow, inID)
	{
		Position = inPosition.Add(-5, 5);
		_npcType = ENPCType.MerchantCrow;
		_bbox = new Rectangle(0, 0, 16, 16);
		_bboxOffset = new Point(3, 8);
		base.TriggerBbox = new Rectangle(0, 0, 96, 160);
		base.TriggerBboxOffset = new Point(-68, 0);
		base.TalkingStandingOffsetX = 52;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = true;
		_npcTriggerType = ENPCTriggerType.Talk;
		ChangeAnimation(0, 2, 0.1f, EAnimationType.None);
		_isInPresent = _level.ID == 2;
		_hasKilledSecondBoss = GetIsBossDead(EBossType.Maw);
		_hasKilledFirstBoss = _hasKilledSecondBoss || GetIsBossDead(EBossType.Demon);
		_hasPlayerBeenToThePast = _level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(6);
		_hasDoneCrowCrowConversation = _level.GameSave.GetSaveBool("CS_CrowCrow");
	}

	public override void Initialize()
	{
		base.Initialize();
		PlayerInventory inventory = _level.GameSave.Inventory;
		bool flag = inventory.RelicInventory.Inventory.ContainsKey(6);
		bool flag2 = inventory.RelicInventory.Inventory.ContainsKey(13);
		LoadDialogueStatusByPrimaryProgress(base.PrimaryProgress);
		if (_isInPresent)
		{
			_merchandiseInventory.AddItem(EInventoryUseItemType.FuturePotion);
			_merchandiseInventory.AddItem(EInventoryUseItemType.FutureEther);
		}
		else
		{
			_merchandiseInventory.AddItem(EInventoryUseItemType.Potion);
			_merchandiseInventory.AddItem(EInventoryUseItemType.Ether);
		}
		_merchandiseInventory.AddItem(EInventoryUseItemType.Biscuit);
		if (flag)
		{
			_merchandiseInventory.AddItem(EInventoryUseItemType.WarpCard);
			_merchandiseInventory.AddItem(EInventoryUseItemType.ChaosHeal);
		}
		_merchandiseInventory.AddItem(EInventoryUseItemType.EssenceCrystal);
		_merchandiseInventory.AddItem(EInventoryUseItemType.GoldRing);
		_merchandiseInventory.AddItem(EInventoryUseItemType.GoldNecklace);
		_merchandiseInventory.AddItem(EInventoryEquipmentType.Sunglasses);
		_merchandiseInventory.AddItem(EInventoryEquipmentType.TrendyJacket);
		_merchandiseInventory.AddItem(EInventoryEquipmentType.LuckyCoin);
		_merchandiseInventory.AddItem(EInventoryEquipmentType.ShinyRock);
		if (flag2)
		{
			_merchandiseInventory.AddItem(EInventoryEquipmentType.FamiliarEgg);
		}
	}

	private void LoadDialogueStatusByPrimaryProgress(int primaryProgress)
	{
		int num = primaryProgress;
		_hasFoundCrowInPresent = (num & 1) == 1;
		num >>= 1;
		_hasFoundCrowInPast = (num & 1) == 1;
		num >>= 1;
		_hasDoneFirstConversation = (num & 1) == 1;
		num >>= 1;
		_hasDoneSecondEraConversation = (num & 1) == 1;
		num >>= 1;
		_hasDonePresentAfterPastConversation = (num & 1) == 1;
		num >>= 1;
		_hasDoneFirstBossConversation = (num & 1) == 1;
		num >>= 1;
		_hasDoneSecondBossConversation = (num & 1) == 1;
	}

	private void SetPrimaryProgressByDialogueStatus()
	{
		int num = 0;
		num |= (_hasDoneSecondBossConversation ? 1 : 0);
		num <<= 1;
		num |= (_hasDoneFirstBossConversation ? 1 : 0);
		num <<= 1;
		num |= (_hasDonePresentAfterPastConversation ? 1 : 0);
		num <<= 1;
		num |= (_hasDoneSecondEraConversation ? 1 : 0);
		num <<= 1;
		num |= (_hasDoneFirstConversation ? 1 : 0);
		num <<= 1;
		num |= (_hasFoundCrowInPast ? 1 : 0);
		num <<= 1;
		num |= (_hasFoundCrowInPresent ? 1 : 0);
		if (num != base.PrimaryProgress)
		{
			SetPrimaryProgress(num);
		}
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddScript(new ScriptAction(new AnimationSpec
		{
			Length = 6,
			Speed = 0.12f,
			Type = EAnimationType.Once
		}, this));
		AddScript(new ScriptAction(ESFX.CrowCaw, Position));
		if (!_hasHadOneConversation)
		{
			_hasHadOneConversation = true;
			if (!_hasDoneFirstConversation)
			{
				DoFirstConversationEver();
			}
			else if (!_hasDoneSecondEraConversation && ((_isInPresent && !_hasFoundCrowInPresent && _hasFoundCrowInPast) || (!_isInPresent && !_hasFoundCrowInPast && _hasFoundCrowInPresent)))
			{
				DoSecondEraTalk();
			}
			else if (_hasDoneSecondEraConversation && _isInPresent && !_hasDonePresentAfterPastConversation)
			{
				DoPresentAfterPastTalk();
			}
			else if (!_hasDoneFirstBossConversation && _hasKilledFirstBoss && _isInPresent)
			{
				DoFirstBossTalk();
			}
			else if (!_hasDoneSecondBossConversation && _hasKilledSecondBoss && _isInPresent)
			{
				DoSecondBossTalk();
			}
			else if (!_hasDoneCrowCrowConversation && _level.GameSave.Inventory.EquippedFamiliar == EInventoryFamiliarType.MerchantCrow && CutsceneBase.GetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.DarkForest0_Start, _level.GameSave))
			{
				DoCrowCrowTalk();
			}
			else
			{
				DoDefaultConversation();
			}
		}
		else
		{
			DoDefaultConversation();
		}
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.15f,
			DoesBlockQueue = true
		});
		if (_isInPresent)
		{
			_hasFoundCrowInPresent = true;
		}
		else
		{
			_hasFoundCrowInPast = true;
		}
		SetPrimaryProgressByDialogueStatus();
		EndNPCDialogue();
	}

	private void DoFirstConversationEver()
	{
		AddDialogue("cs_crow_0_lun_00");
		AddDialogue("cs_crow_0_cro_01");
		AddDialogue("cs_crow_0_lun_02");
		AddDialogue("cs_crow_0_cro_03");
		AddDialogue("cs_crow_0_lun_04");
		AddDialogue("cs_crow_0_cro_05");
		AddDialogue("cs_crow_0_lun_06");
		AddDialogue("cs_crow_0_cro_07");
		AddDialogue("cs_crow_0_lun_08");
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		AddDialogue("cs_crow_0_cro_09");
		AddDialogue("cs_crow_0_lun_10");
		AddDialogue("cs_crow_0_cro_11");
		AddDialogue("cs_crow_0_lun_12");
		_hasDoneFirstConversation = true;
	}

	private void DoDefaultConversation()
	{
		AddDialogue("cs_crow_1_cro_00");
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		if (_hasPlayerBeenToThePast && base.SubProgress <= 0)
		{
			AddDialogue("cs_crow_1_cro_01");
			AddDialogue("cs_crow_1_lun_02");
			SetSubProgress(1);
		}
	}

	private void DoSecondEraTalk()
	{
		AddDialogue("cs_crow_2_cro_00");
		AddDialogue("cs_crow_2_lun_01");
		AddDialogue("cs_crow_2_cro_02");
		AddDialogue("cs_crow_2_lun_03");
		AddDialogue("cs_crow_2_cro_04");
		AddDialogue("cs_crow_2_cro_05");
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		_hasDoneSecondEraConversation = true;
	}

	private void DoPresentAfterPastTalk()
	{
		AddSummonMeyef();
		AddDialogue(_isInPresent ? "cs_crow_3_cro_00" : "cs_crow_3_cro_00b");
		AddMeyefMew();
		AddDialogue("cs_crow_3_lun_01");
		AddDialogue("cs_crow_3_cro_02");
		AddMeyefMew();
		AddDialogue("cs_crow_3_cro_03");
		AddDialogue("cs_crow_3_lun_04");
		AddDialogue("cs_crow_3_cro_05");
		AddDismissMeyef();
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		_hasDonePresentAfterPastConversation = true;
	}

	private void DoFirstBossTalk()
	{
		AddSummonMeyef();
		AddDialogue("cs_crow_4_cro_00");
		AddDialogue("cs_crow_4_lun_01");
		AddDialogue(_hasDoneSecondEraConversation ? "cs_crow_4_cro_02" : "cs_crow_4_cro_02b");
		AddDialogue("cs_crow_4_lun_03");
		AddDialogue("cs_crow_4_cro_04");
		AddDialogue("cs_crow_4_mey_05");
		AddDialogue("cs_crow_4_cro_06");
		AddDialogue("cs_crow_4_mey_07");
		AddDialogue("cs_crow_4_lun_08");
		AddDialogue("cs_crow_4_cro_09");
		AddDismissMeyef();
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		_hasDoneFirstBossConversation = true;
	}

	private void DoSecondBossTalk()
	{
		AddSummonMeyef();
		AddDialogue("cs_crow_5_cro_00");
		AddDialogue("cs_crow_5_lun_01");
		AddDialogue("cs_crow_5_cro_02");
		AddDialogue("cs_crow_5_mey_03");
		AddDialogue("cs_crow_5_cro_04");
		AddDialogue("cs_crow_5_lun_05");
		AddDialogue("cs_crow_5_cro_06");
		AddDialogue("cs_crow_5_lun_07");
		AddDismissMeyef();
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		_hasDoneSecondBossConversation = true;
	}

	private void DoCrowCrowTalk()
	{
		AddDialogue("cs_crow_crow_cro_00");
		AddDialogue("cs_crow_crow_lun_01");
		AddDialogue("cs_crow_crow_cro_02");
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				OpenShop(_npcType, _merchandiseInventory);
			}
		});
		_hasDoneCrowCrowConversation = true;
		_level.GameSave.SetValue("CS_CrowCrow", value: true);
	}
}
