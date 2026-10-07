using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingA0 : CutsceneBase
{
	private readonly bool _shouldPlayAltIntro;

	public CutsceneEndingA0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesHideOrbsShowAnimation = false;
		_shouldPlayAltIntro = _level.GameSave.GetSaveBool("IsPrinceDead") || _level.GameSave.GetSaveBool("IsTerrilisDead");
		FadeOut(0f, 0.5f, 0.5f);
		AddHidePlayer();
		AddLockCamera();
		AddCameraPan(new Point(232, 120), 0f, doesBlockQueue: false);
	}

	internal override void DoCutscene()
	{
		if (_shouldPlayAltIntro)
		{
			InstantLevelFade();
			AddAutoplayGhostDialogue("cs_end_ex_0_lun_00");
			AddWaitScript(0.5f);
			AddDelegateScript(base.StartLevelFadeIn);
		}
		else
		{
			AddWaitScript(0.25f);
		}
		AddCameraPan(new Point(600, 120), 8f, doesBlockQueue: false);
		AddAutoplayGhostDialogue("cs_enda_0_lun_00");
		TeleportToLevelAndRoom(17, 0, ECutsceneType.EndingA1_Present1);
	}
}
