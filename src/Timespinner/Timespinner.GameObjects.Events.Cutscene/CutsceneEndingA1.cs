using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingA1 : CutsceneBase
{
	public CutsceneEndingA1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesHideOrbsShowAnimation = false;
		AddLockCamera();
		AddHidePlayer();
	}

	internal override void DoCutscene()
	{
		AddCameraPan(new Point(200, 520), 20f, doesBlockQueue: false);
		AddAutoplayGhostDialogue("cs_enda_1_lun_00");
		AddAutoplayGhostDialogue("cs_enda_1_lun_01");
		TeleportToLevelAndRoom(17, 1, ECutsceneType.EndingA2_Past0);
	}
}
