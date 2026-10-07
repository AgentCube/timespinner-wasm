using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest8 : CutsceneBase
{
	private const int PrefabID = 491;

	private const int FloorY = 176;

	private const int LunaisStartX = 368;

	private const int BlockerX = 304;

	public CutsceneForest8(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		EnvPrefabForestPlayerBlocker newObject = new EnvPrefabForestPlayerBlocker(_level, new Point(304, 176), -1, new ObjectTileSpecification(491)
		{
			Argument = 303
		}, EEnvironmentPrefabType.L3_EscortBlocker);
		_level.RequestAddObject(newObject);
		_level.RequestScreenFadeOut(0f, 0.25f, 0.25f, 0f);
	}

	internal override void DoCutscene()
	{
		WarpPlayerToPosition(new Point(368, 176));
		AddDialogue("q_esc_2_esc_escort");
	}
}
