using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneAlt3 : CutsceneBase
{
	public CutsceneAlt3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		base.DoesHideOrbsAutomatically = false;
		base.DoesHideOrbsShowAnimation = false;
	}

	internal override void DoCutscene()
	{
		AddUnskippableWaitScript(8f);
		TeleportToLevelAndRoom(15, 0, ECutsceneType.DarkForest0_Start);
	}
}
