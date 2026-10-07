using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneGear3 : CutsceneBase
{
	public CutsceneGear3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddPlayerFaceRoomCenter();
		AddWaitScript(0.1f);
		AddSummonMeyef();
		AddMeyefFlyInFrontOfPlayer(0.5f, doesBlock: false);
		AddDialogue("cs_gear_2_lun_00");
		AddDialogue("cs_gear_2_lun_01");
		AddDialogue("cs_gear_2_lun_02");
		AddDialogue("cs_gear_2_mey_03");
		AddDialogue("cs_gear_2_lun_04");
		AddDialogue("cs_gear_2_mey_05");
		AddDialogue("cs_gear_2_lun_06");
		AddDialogue("cs_gear_2_mey_07");
		AddDialogue("cs_gear_2_lun_08");
		AddDialogue("cs_gear_2_mey_09");
		AddDialogue("cs_gear_2_mey_10");
		AddDialogue("cs_gear_2_mey_11");
		AddDialogue("cs_gear_2_lun_12");
		AddDialogue("cs_gear_2_mey_13");
		AddDialogue("cs_gear_2_mey_14");
		AddDialogue("cs_gear_2_lun_15");
		AddDismissMeyef();
	}
}
