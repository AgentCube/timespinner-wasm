using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingB1 : CutsceneBase
{
	public CutsceneEndingB1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddHidePlayer();
		AddCameraPan(new Point(200, 520), 0f, doesBlockQueue: false);
	}

	internal override void DoCutscene()
	{
		AddCameraPan(new Point(200, 120), 15f, doesBlockQueue: false);
		AddAutoplayGhostDialogue("cs_endb_1_lun_00");
		AddAutoplayGhostDialogue("cs_endb_1_lun_01");
		TeleportToLevelAndRoom(17, 2, ECutsceneType.EndingB2_Past0);
	}
}
