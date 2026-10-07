using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingCD3 : CutsceneBase
{
	private readonly bool _isEndingC;

	public CutsceneEndingCD3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, bool isEndingC)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isEndingC = isEndingC;
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddHidePlayer();
	}

	internal override void DoCutscene()
	{
		AddCameraPan(new Point(200, 520), 20f, doesBlockQueue: false);
		if (_isEndingC)
		{
			AddAutoplayGhostDialogue("cs_endc_3_lun_00");
		}
		else
		{
			AddAutoplayGhostDialogue("cs_endd_3_lun_00");
		}
		AddAutoplayGhostDialogue("cs_endc_3_lun_01");
		AddAutoplayGhostDialogue("cs_endc_3_lun_02");
		AddDelegateScript(base.StartLevelFadeOut);
		AddWaitScript(1f);
		AddAutoplayGhostDialogue("cs_endc_3_lun_03");
		AddSongFadeOut(3f, doesBlock: false);
		AddWaitScript(2f);
		if (_isEndingC)
		{
			TeleportToLevelAndRoom(17, 8, ECutsceneType.EndingC4_Winderia0);
		}
		else
		{
			TeleportToLevelAndRoom(17, 8, ECutsceneType.EndingD5_Winderia0);
		}
	}
}
