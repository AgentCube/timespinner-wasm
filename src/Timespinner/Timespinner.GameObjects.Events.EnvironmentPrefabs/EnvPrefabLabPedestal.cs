using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabLabPedestal : EnvironmentPrefabBase
{
	public EnvPrefabLabPedestal(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpMiscLab;
		IsFacingLeft = objectSpec.IsFlippedHorizontally;
		Bbox = new Rectangle(0, 0, 42, 74);
		SnapBboxToPosition();
		ChangeAnimation(41);
		_isSolid = false;
		base.CanBeTriggered = false;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_doAppendagesMatchImageFacing = false;
	}
}
