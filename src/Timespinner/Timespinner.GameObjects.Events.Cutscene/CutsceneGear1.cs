using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneGear1 : CutsceneBase
{
	public CutsceneGear1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
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
		AddDialogue("cs_gear_0_lun_00");
		AddMeyefMew();
		AddDialogue("cs_gear_0_lun_01");
		AddDialogue("cs_gear_0_lun_02");
		AddDialogue("cs_gear_0_lun_03");
		AddMeyefMew();
		AddDialogue("cs_gear_0_mey_04");
		AddDialogue("cs_gear_0_lun_05");
		AddDialogue("cs_gear_0_mey_06");
		AddDialogue("cs_gear_0_lun_07");
		AddDialogue("cs_gear_0_lun_08");
		AddMeyefPurr();
		AddDialogue("cs_gear_0_lun_09");
		AddDialogue("cs_gear_0_lun_10");
		AddDialogue("cs_gear_0_lun_11");
		AddDismissMeyef();
	}
}
