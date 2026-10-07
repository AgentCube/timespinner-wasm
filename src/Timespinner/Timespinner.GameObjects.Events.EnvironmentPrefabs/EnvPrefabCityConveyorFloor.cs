using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabCityConveyorFloor : EnvironmentPrefabBase
{
	public EnvPrefabCityConveyorFloor(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		SnapBboxToPosition();
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(0, 3, 0.1f, EAnimationType.Cycle);
		base.DrawPlane = EDrawPlane.Front;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
	}
}
