using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L8_Caves;

internal class EnvPrefabProCricketsAmbient : EnvironmentPrefabBase
{
	private readonly SFXCueInstance _cricketsCueInstance;

	public EnvPrefabProCricketsAmbient(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_isAffectedByTime = true;
		_doesDrawBaseSprite = false;
		_sprite = null;
		Bbox = new Rectangle(0, 0, 16, 16);
		_cricketsCueInstance = PlayCue(ESFX.AmbientCricketsLoop, isLooped: true);
		if (_cricketsCueInstance != null)
		{
			_cricketsCueInstance.RangeMultiplier = 2f;
			_cricketsCueInstance.PlayWhenInRange();
		}
	}

	internal void StopCrickets()
	{
		if (_cricketsCueInstance != null)
		{
			_cricketsCueInstance.Stop(1.5f);
		}
	}
}
