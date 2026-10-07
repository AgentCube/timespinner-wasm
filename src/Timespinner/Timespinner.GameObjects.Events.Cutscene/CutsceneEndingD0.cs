using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingD0 : CutsceneBase
{
	public CutsceneEndingD0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		FadeOut(0f, 0.5f, 0.5f);
		AddHidePlayer();
		AddLockCamera();
		AddCameraPan(new Point(232, 120), 0f, doesBlockQueue: false);
	}

	internal override void DoCutscene()
	{
		InstantLevelFade();
		AddAutoplayGhostDialogue("cs_endc_0_lun_00");
		AddWaitScript(0.25f);
		AddDelegateScript(base.StartLevelFadeIn);
		AddCameraPan(new Point(600, 120), 8f, doesBlockQueue: false);
		AddAutoplayGhostDialogue("cs_endc_0_lun_01");
		AddAutoplayGhostDialogue("cs_endc_0_lun_02");
		AddDelegateScript(base.StartLevelFadeOut);
		AddAutoplayGhostDialogue("cs_endc_0_lun_03");
		AddAutoplayGhostDialogue("cs_endc_0_lun_04");
		if (NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Quartermaster, _level.GameSave) && NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.SickSoldier, _level.GameSave))
		{
			TeleportToLevelAndRoom(17, 9, ECutsceneType.EndingD1_Past1);
		}
		else
		{
			TeleportToLevelAndRoom(17, 1, ECutsceneType.EndingD3_Past3);
		}
	}
}
