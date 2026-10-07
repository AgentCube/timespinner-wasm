using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeDesolation3 : CutsceneBase
{
	public CutsceneLakeDesolation3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_lde_3_lun_00");
	}
}
