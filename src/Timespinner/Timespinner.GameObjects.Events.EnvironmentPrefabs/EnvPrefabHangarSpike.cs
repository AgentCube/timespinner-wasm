using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabHangarSpike : EnvironmentPrefabBase
{
	public EnvPrefabHangarSpike(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		SnapBboxToPosition();
		_sprite = _level.GCM.SpMiscHangar;
		ChangeAnimation(6, 11, 0.05f, EAnimationType.Cycle);
		base.DrawPlane = EDrawPlane.Front;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
	}
}
