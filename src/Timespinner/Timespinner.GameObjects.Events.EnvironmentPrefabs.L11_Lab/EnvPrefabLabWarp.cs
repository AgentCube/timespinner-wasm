using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal class EnvPrefabLabWarp : TextPromptPrefab
{
	public EnvPrefabLabWarp(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType, string text, Color textColor, Vector4 sparklesColor, Color lightDrawColor)
		: base(inLevel, inPosition, inID, objectSpec, prefabType, text, textColor)
	{
		GlowingFloorEvent newObject = new GlowingFloorEvent(_level, inPosition, lightDrawColor, sparklesColor);
		_level.RequestAddObject(newObject);
	}
}
