using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CastleArcherArrow : Projectile
{
	private bool _isLodgedInWall;

	public CastleArcherArrow(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 11, inPosition.Y - 11, 22, 2);
		_bboxOffset = new Point(0, 2);
		DrawOrigin = new Vector2(20f, 4f);
		_power = (int)Math.Ceiling(1.5f * (float)baseDamage);
		_force = 0;
		_life = 3f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesCollideWithWalls = true;
		_doesDieOnTiles = true;
		base.DoesCollideWithTiles = true;
		ChangeAnimation(12);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		_level.PlayCue(ESFX.EnemyArcherArrowImpact, target.Position);
		base.AddImpactAnimation(target, collidingRectangle);
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		_power = 0;
		_velocity = Vector2.Zero;
		_canDamageThings = false;
		if (!_isLodgedInWall)
		{
			_isLodgedInWall = true;
			PlayCue(ESFX.EnemyArcherArrowImpact, contactPoint);
			_life = Math.Min(_life, 1f);
			Position = contactPoint.Add(9 * (IsFacingLeft ? 1 : (-1)), 0);
		}
	}

	public override void KillOnProjectileImpact(Projectile proj)
	{
		SilentKill();
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}
}
