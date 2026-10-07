using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneGear2 : CutsceneBase
{
	public CutsceneGear2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
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
		AddDialogue("cs_gear_1_lun_00");
		AddMeyefPurr();
		AddDialogue("cs_gear_1_lun_01");
		AddDialogue("cs_gear_1_mey_02");
		AddDialogue("cs_gear_1_lun_03");
		AddDialogue("cs_gear_1_mey_04");
		AddDialogue("cs_gear_1_lun_05");
		AddDialogue("cs_gear_1_mey_06");
		AddDialogue("cs_gear_1_lun_07");
		AddDialogue("cs_gear_1_mey_08");
		AddDialogue("cs_gear_1_lun_09");
		AddDialogue("cs_gear_1_mey_10");
		AddDialogue("cs_gear_1_mey_11");
		AddDialogue("cs_gear_1_lun_12");
		AddMeyefMew();
		AddDialogue("cs_gear_1_lun_13");
		AddDismissMeyef();
	}
}
