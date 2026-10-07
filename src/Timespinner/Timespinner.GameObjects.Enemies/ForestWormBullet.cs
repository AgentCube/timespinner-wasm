using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestWormBullet : Projectile
{
	private readonly Func<Point, bool> _sproutAction;

	private bool _hasSproutedYet;

	public ForestWormBullet(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, Func<Point, bool> sproutAction, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sproutAction = sproutAction;
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 8);
		_bboxOffset = new Point(0, 2);
		DrawOrigin = new Vector2(4f, 5f);
		_power = baseDamage;
		_force = 0;
		_life = 1f;
		_isAffectedByGravity = true;
		_isAffectedByFriction = true;
		_doesOverrideMaxSpeed = false;
		_isFlying = true;
		_airDragFactor = 0f;
		base.DoesCollideWithTiles = true;
		_doesCollideWithCeilings = false;
		_doesCollideWithWalls = true;
		_doesCollideWithFloors = true;
		_doesDieOnTiles = true;
		_doesCollideWithSlopes = true;
		_rotationSpeed = -10f;
		_doesRotateBasedOnVelocity = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		base.DoesDieOnImpact = false;
		ChangeAnimation(24, 3, 0.1f, EAnimationType.PingPong);
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		bool flag = false;
		if (!_hasSproutedYet)
		{
			_hasSproutedYet = true;
			if (isVerticalCollision)
			{
				flag = _sproutAction(contactPoint);
			}
			contactPoint = contactPoint.Add(0, -3);
			if (!flag)
			{
				_level.AddAnimation(EBattleAnimationType.SmallBoom, _bbox.Center, base.DefaultTeam);
				_level.PlayCue(ESFX.EnemyWormFlowerSeedBoom, contactPoint);
			}
			else
			{
				_level.PlayCue(ESFX.EnemyWormFlowerSeedHitSprout, contactPoint);
			}
		}
		Kill(useAnimation: false);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collisionRectangle)
	{
		_level.PlayCue(ESFX.EnemyWormFlowerSeedBoom, Position);
		_level.AddAnimation(EBattleAnimationType.SmallBoom, collisionRectangle.Center, base.DefaultTeam);
		base.AddImpactAnimation(target, collisionRectangle);
	}
}
