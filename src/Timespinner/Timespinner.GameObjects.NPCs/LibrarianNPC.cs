using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class LibrarianNPC : NPCBase
{
	private const int AnimationStartIndex = 8;

	private const int RelaxedAnimationLength = 5;

	private const int GreenCoatAnimationOffset = 7;

	private const int ToStandingFrame = 13;

	private const float StandingUpAnimationSpeed = 0.07f;

	private const float RelaxedAnimationSpeed = 0.2f;

	private bool _isWearingGreenOutfit;

	private bool _isStandingUp;

	public LibrarianNPC(Level inLevel, Point inPosition, int inID, TileSpecification tileSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpMerchantCrow, inID)
	{
		Position = inPosition.Add(-4, 6);
		_npcType = ENPCType.Librarian;
		_bbox = new Rectangle(0, 0, 16, 16);
		_bboxOffset = new Point(7, 7);
		base.TriggerBbox = new Rectangle(0, 0, 64, 64);
		IsFacingLeft = !tileSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_npcTriggerType = ENPCTriggerType.Talk;
	}

	public override void Initialize()
	{
		base.Initialize();
		_isWearingGreenOutfit = base.PrimaryProgress >= 3;
		Relax();
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		StandUp();
		switch (base.PrimaryProgress)
		{
		case 0:
			DoTabletCutscene();
			SetPrimaryProgress(1);
			break;
		case 1:
			if (DoesWantToGiveVileteKeycard(_level.GameSave))
			{
				DoVileteKeycardCutscene();
				SetPrimaryProgress(2);
			}
			else
			{
				AddDialogue("q_lib2_lib_00");
			}
			break;
		case 2:
			if (IsPlayerWearingFullAdvisorSet())
			{
				DoAdvisorOufitCutscene();
				SetPrimaryProgress(3);
			}
			else
			{
				AddDialogue("q_lib2_lib_00");
			}
			break;
		case 3:
			AddDialogue("q_lib3_lib_00");
			break;
		}
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = Relax
		});
		EndNPCDialogue();
	}

	private bool IsPlayerWearingFullAdvisorSet()
	{
		PlayerInventory inventory = _level.GameSave.Inventory;
		if (inventory.EquippedArmor == EInventoryEquipmentType.AdvisorRobe)
		{
			return inventory.EquippedHelmet == EInventoryEquipmentType.AdvisorHat;
		}
		return false;
	}

	private void SwitchOutfit()
	{
		PlayerInventory inventory = _level.GameSave.Inventory;
		inventory.EquipmentInventory.RemoveItem(9);
		inventory.EquipmentInventory.RemoveItem(41);
		inventory.EquipmentInventory.AddItem(10);
		AddGiveItemScript(EInventoryEquipmentType.LibrarianRobe);
		inventory.EquippedHelmet = EInventoryEquipmentType.LibrarianHat;
		inventory.EquippedArmor = EInventoryEquipmentType.LibrarianRobe;
		_isWearingGreenOutfit = true;
	}

	private void Relax()
	{
		int num = (_isWearingGreenOutfit ? 7 : 0);
		if (_isStandingUp)
		{
			ChangeAnimation(8 + num, 5, 0.2f, EAnimationType.Cycle, 13 + num, 1, 0.2f);
		}
		else
		{
			ChangeAnimation(8 + num, 5, 0.2f, EAnimationType.Cycle);
		}
		_isStandingUp = false;
	}

	private void StandUp()
	{
		int num = (_isWearingGreenOutfit ? 7 : 0);
		if (!_isStandingUp)
		{
			ChangeAnimation(13 + num, 2, 0.07f, EAnimationType.Once);
		}
		else
		{
			ChangeAnimation(13 + num + 1, 1, 0.07f, EAnimationType.Once);
		}
		_isStandingUp = true;
	}

	private void DoTabletCutscene()
	{
		AddDialogue("q_lib_lib_00");
		AddDialogue("q_lib_lun_01");
		AddDialogue("q_lib_lib_02");
		AddDialogue("q_lib_lun_03");
		AddDialogue("q_lib_lib_04");
		AddDialogue("q_lib_lib_05");
		AddDialogue("q_lib_lun_06");
		AddDialogue("q_lib_lib_07");
		AddDialogue("q_lib_lib_08");
		AddDialogue("q_lib_lun_09");
		AddDialogue("q_lib_lib_10");
		AddDialogue("q_lib_lib_11");
		AddGiveItemScript(EInventoryRelicType.Tablet);
		AddWaitScript(0.05f);
		AddDialogue("q_lib_lib_12");
		AddDialogue("q_lib_lun_13");
	}

	private void DoVileteKeycardCutscene()
	{
		AddDialogue("q_lib_v_lib_00");
		AddDialogue("q_lib_v_lun_01");
		AddDialogue("q_lib_v_lib_02");
		AddDialogue("q_lib_v_lib_03");
		AddDialogue("q_lib_v_lun_04");
		AddDialogue("q_lib_v_lib_05");
		AddDialogue("q_lib_v_lun_06");
		AddDialogue("q_lib_v_lun_07");
		AddDialogue("q_lib_v_lib_08");
		AddDialogue("q_lib_v_lib_09");
		AddDialogue("q_lib_v_lib_10");
		AddDialogue("q_lib_v_lib_11");
		AddDialogue("q_lib_v_lun_12");
		AddDialogue("q_lib_v_lib_13");
		AddGiveItemScript(EInventoryRelicType.ScienceKeycardV);
	}

	private void DoAdvisorOufitCutscene()
	{
		AddDialogue("q_lib2_lib_01");
		AddDialogue("q_lib2_lib_02");
		AddDialogue("q_lib2_lib_03");
		AddDialogue("q_lib2_lun_04");
		AddDialogue("q_lib2_lib_05");
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = SwitchOutfit
		});
	}

	internal static bool DoesHaveItemToGive(GameSave saveFile)
	{
		if (!DoesWantToGiveTablet(saveFile))
		{
			return DoesWantToGiveVileteKeycard(saveFile);
		}
		return true;
	}

	private static bool DoesWantToGiveTablet(GameSave saveFile)
	{
		return !saveFile.Inventory.RelicInventory.Inventory.ContainsKey(18);
	}

	private static bool DoesWantToGiveVileteKeycard(GameSave saveFile)
	{
		bool flag = saveFile.Inventory.RelicInventory.Inventory.ContainsKey(17);
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Astrologer, saveFile);
		int subQuestState = NPCBase.GetSubQuestState(ENPCType.Astrologer, saveFile);
		if ((primaryQuestState == 1 && subQuestState > 0) || primaryQuestState > 1)
		{
			return !flag;
		}
		return false;
	}
}
