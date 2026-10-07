using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.LevelEffects;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneAlt2 : CutsceneBase
{
	private const int LevelEffectID = 490;

	private const int BreakArgument = 1300;

	public CutsceneAlt2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		base.DoesHideOrbsAutomatically = false;
		base.DoesHideOrbsShowAnimation = false;
	}

	internal override void DoCutscene()
	{
		_level.RequestScreenShake(Vector2.Zero, 0f, 0f, isAffectedByTime: false);
		BreakLevelEffect newObject = new BreakLevelEffect(_level, Point.Zero, -1, new ObjectTileSpecification(490)
		{
			Argument = 1300
		});
		_level.RequestAddObject(newObject);
	}
}
