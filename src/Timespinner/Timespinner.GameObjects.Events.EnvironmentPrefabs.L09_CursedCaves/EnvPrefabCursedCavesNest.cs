using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L09_CursedCaves;

internal sealed class EnvPrefabCursedCavesNest : EnvironmentPrefabBase
{
	private const int Anim_Start = 23;

	public EnvPrefabCursedCavesNest(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpXarionBoss;
		ChangeAnimation(23);
		base.DrawPlane = EDrawPlane.Front;
		_bboxOffset = new Point(24, 0);
		Bbox = new Rectangle(0, 0, 80, 16);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.CanBeTriggered = true;
		base.CanBeTriggeredByFamiliar = false;
		base.IsTriggerableByMonsters = false;
		base.DrawColor = Color.White * 0.8f;
	}
}
