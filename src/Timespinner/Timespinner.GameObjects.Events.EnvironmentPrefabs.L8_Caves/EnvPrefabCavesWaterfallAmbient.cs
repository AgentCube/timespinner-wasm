using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L8_Caves;

internal class EnvPrefabCavesWaterfallAmbient : EnvironmentPrefabBase
{
	public EnvPrefabCavesWaterfallAmbient(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_isAffectedByTime = true;
		_doesDrawBaseSprite = false;
		_sprite = null;
		Bbox = new Rectangle(0, 0, 16, 16);
		SFXCueInstance sFXCueInstance = CreateCue(ESFX.AmbientWaterfallLoop, inPosition, isLooped: true);
		if (sFXCueInstance != null)
		{
			sFXCueInstance.RangeMultiplier = 2f;
			if (prefabType == EEnvironmentPrefabType.L8_WaterfallAmbientOffscreen)
			{
				sFXCueInstance.PositionOffset = new Point((!objectSpec.IsFlippedHorizontally) ? (-400) : 400, 0);
			}
			sFXCueInstance.PlayWhenInRange();
			sFXCueInstance.FadeIn(0.1f);
		}
	}
}
