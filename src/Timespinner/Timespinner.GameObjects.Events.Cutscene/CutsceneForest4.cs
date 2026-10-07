using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest4 : CutsceneBase
{
	public CutsceneForest4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_for_2_lun_00");
		AddDialogue("cs_for_2_lun_01");
		AddDialogue("cs_for_2_lun_02");
		AddDialogue("cs_for_2_lun_03");
	}
}
