using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabCavesPortalRubble : EnvironmentPrefabBase
{
	public EnvPrefabCavesPortalRubble(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_bboxOffset = new Point(32, 0);
		Bbox = new Rectangle(0, 0, 64, 144);
		_isAffectedByGravity = false;
		_isFlying = true;
		IsFacingLeft = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = false;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		_sprite = _level.GCM.BgVileteArchways;
		ChangeAnimation(4);
	}
}
