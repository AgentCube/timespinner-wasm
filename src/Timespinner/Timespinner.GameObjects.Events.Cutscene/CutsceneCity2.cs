using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCity2 : CutsceneBase
{
	public CutsceneCity2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_cit_2_lun_00");
		AddDialogue("cs_cit_2_lun_01");
		AddDialogue("cs_cit_2_lun_02");
		AddDialogue("cs_cit_2_lun_03");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_cit_2_lun_04");
		AddDialogue("cs_cit_2_lun_05");
	}
}
