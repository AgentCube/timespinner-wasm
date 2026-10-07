using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLab3 : CutsceneBase
{
	public CutsceneLab3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesHideOrbsAutomatically = false;
		base.DoesHideOrbsShowAnimation = false;
		base.DoesMakePlayerIdleAtStart = false;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddScript(new ScriptAction(ESFX.EnvLabGlassTubeBreak, Position));
	}
}
