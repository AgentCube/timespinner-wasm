using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabForestNightAmbiA : EnvironmentPrefabBase
{
	private static readonly Color NightAmbienceColor = new Color(112, 144, 196);

	public EnvPrefabForestNightAmbiA(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
	}

	public override void Update(float delta)
	{
		_level.SetLevelDrawColor(NightAmbienceColor);
		base.Update(delta);
	}

	public override void SilentKill()
	{
		_level.SetLevelDrawColor(Color.White);
		base.SilentKill();
	}
}
