using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal class EnvPrefabTowerRailingCollider : EnvironmentPrefabBase
{
	public EnvPrefabTowerRailingCollider(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 10, 21);
		_doesDrawBaseSprite = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = false;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		base.CannotBeGrabbed = true;
	}
}
