using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabConveyorAmbient : EnvironmentPrefabBase
{
	public EnvPrefabLabConveyorAmbient(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_isAffectedByTime = true;
		_doesDrawBaseSprite = false;
		_sprite = null;
		Bbox = new Rectangle(0, 0, 16, 16);
		if (_level.IsPowerOff)
		{
			return;
		}
		SFXCueInstance sFXCueInstance = CreateCue(ESFX.AmbientConveyorBeltLoop, Position, isLooped: true);
		if (sFXCueInstance != null)
		{
			sFXCueInstance.RangeMultiplier = 2f;
			if (prefabType == EEnvironmentPrefabType.L11_ConveyorAmbientOffscreen)
			{
				sFXCueInstance.PositionOffset = new Point((!objectSpec.IsFlippedHorizontally) ? (-400) : 400, 0);
			}
			sFXCueInstance.PlayWhenInRange();
			sFXCueInstance.FadeIn(0.1f);
		}
	}
}
