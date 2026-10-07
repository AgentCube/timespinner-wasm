using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabEndMorningAmbi : EnvironmentPrefabBase
{
	private Color NightAmbienceColor;

	public EnvPrefabEndMorningAmbi(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
	}

	public override void Update(float delta)
	{
		NightAmbienceColor = new Color(216, 224, 255);
		_level.SetLevelDrawColor(NightAmbienceColor);
		base.Update(delta);
	}

	public override void SilentKill()
	{
		_level.SetLevelDrawColor(Color.White);
		base.SilentKill();
	}
}
