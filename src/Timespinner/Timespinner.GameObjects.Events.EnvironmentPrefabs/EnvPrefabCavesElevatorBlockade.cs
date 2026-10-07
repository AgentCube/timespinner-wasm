using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal class EnvPrefabCavesElevatorBlockade : EnvironmentPrefabBase
{
	public EnvPrefabCavesElevatorBlockade(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpPlatforms;
		Bbox = new Rectangle(0, 0, 48, 26);
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_isSolid = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.CanBeTriggered = true;
		base.DrawPlane = EDrawPlane.Front;
	}

	internal void DoDestroy()
	{
		DebrisEvent.CreateFromAppendages(base.Appendages, new Vector2(0f, -800f), Position, _sprite, DebrisEvent.EDebrisDeathType.Dust, Point.Zero);
		_appendages.Clear();
		Kill();
	}
}
