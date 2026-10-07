using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class AnimatedTilePrefab : EnvironmentPrefabBase
{
	public AnimatedTilePrefab(Level inLevel, SpriteSheet sprite, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType, int animationStart, int animationLength, float animationSpeed, EAnimationType animationType, EDrawPlane drawPlane)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		SnapBboxToPosition();
		_sprite = sprite;
		ChangeAnimation(animationStart, animationLength, animationSpeed, animationType);
		base.DrawPlane = drawPlane;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
	}
}
