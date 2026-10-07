using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class WormBullet : Projectile
{
	public WormBullet(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int damage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 8, inPosition.Y - 8, 17, 17);
		_bboxOffset = new Point(-4, -4);
		DrawOrigin = new Vector2(4.5f, 4f);
		_power = damage;
		_force = 0;
		_life = 1f;
		_isAffectedByGravity = true;
		_isAffectedByFriction = true;
		_doesOverrideMaxSpeed = false;
		_isFlying = true;
		_airDragFactor = 0f;
		base.DoesCollideWithTiles = true;
		_doesCollideWithCeilings = false;
		_doesCollideWithWalls = false;
		_doesCollideWithFloors = true;
		_doesDieOnTiles = true;
		_doesCollideWithSlopes = true;
		_rotationSpeed = -10f;
		_doesRotateBasedOnVelocity = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		ChangeAnimation(48, 2, 0.1f, EAnimationType.PingPong);
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		contactPoint = contactPoint.Add(0, -3);
		_level.PlayCue(ESFX.EnemyWormFlowerSeedBoom, contactPoint);
		_level.AddAnimation(EBattleAnimationType.SmallBoom, contactPoint, base.DefaultTeam);
		Kill(useAnimation: false);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collisionRectangle)
	{
		_level.PlayCue(ESFX.EnemyWormFlowerSeedBoom, Position);
		_level.AddAnimation(EBattleAnimationType.SmallBoom, collisionRectangle.Center, base.DefaultTeam);
		base.AddImpactAnimation(target, collisionRectangle);
	}
}
