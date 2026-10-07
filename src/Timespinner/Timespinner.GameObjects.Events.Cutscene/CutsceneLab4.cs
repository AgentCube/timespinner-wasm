using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLab4 : CutsceneBase
{
	public CutsceneLab4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesHideOrbsAutomatically = false;
	}

	internal override void DoCutscene()
	{
	}
}
