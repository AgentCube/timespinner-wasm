using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneHangar0 : CutsceneBase
{
	public CutsceneHangar0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override bool AreTriggerConditionsMet()
	{
		bool flag = _level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(6);
		bool saveBool = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		bool saveBool2 = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw));
		if (!_level.GameSave.GetSaveBool("IsPastCleared") && flag)
		{
			if (saveBool)
			{
				return !saveBool2;
			}
			return true;
		}
		return false;
	}

	internal override void DoCutscene()
	{
		AddSummonMeyef();
		AddMeyefFlyInFrontOfPlayer(0.5f, doesBlock: false);
		AddDialogue("cs_han_0_lun_00");
		AddDialogue("cs_han_0_lun_01");
		AddDialogue("cs_han_0_lun_02");
		AddDialogue("cs_han_0_mey_03");
		AddDialogue("cs_han_0_lun_04");
		AddMeyefPurr();
		AddDialogue("cs_han_0_lun_05");
		AddDismissMeyef();
	}
}
