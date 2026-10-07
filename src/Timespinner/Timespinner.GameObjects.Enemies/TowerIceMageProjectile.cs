using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._06_Tower;

namespace Timespinner.GameObjects.Enemies;

internal sealed class TowerIceMageProjectile : Projectile
{
	private const int BboxWidth = 8;

	private const int BboxHeight = 8;

	private const float MaxLife = 1.5f;

	private const float InitialSpeed = 400f;

	private readonly float _projectileAngle;

	private readonly TowerIceMageTrailEvent _trailEvent;

	public TowerIceMageProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_velocity = iV * 400f;
		if (Math.Abs(_velocity.Y) > 0f)
		{
			_projectileAngle = (float)Math.Atan2(_velocity.X, 0f - _velocity.Y);
		}
		if (_velocity.X > 0f)
		{
			_projectileAngle -= 1.57f;
			DrawOrigin = new Vector2(32f, 3.5f);
		}
		else if (_velocity.X < 0f)
		{
			_projectileAngle += 1.57f;
			DrawOrigin = new Vector2(9.5f, 3.5f);
		}
		_sprite = sprite;
		_bboxOffset = new Point(5, 0);
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 8);
		IsFacingLeft = iV.X < 0f;
		base.Rotation = _projectileAngle;
		SnapBboxToPosition();
		_power = (int)Math.Ceiling((float)baseDamage * 1.5f);
		_force = 0;
		_life = 1.5f;
		_doesRotateBasedOnVelocity = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_damageElement = EDamageElement.Ice;
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_trailLength = 4;
		_trailFadeRate = 1f;
		base.DoesDieOnImpact = false;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithFloors = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithCeilings = true;
		_doesCollideWithWalls = true;
		base.DoesDieToEnemyProjectiles = true;
		_doesDieOutsideOfVisibleArea = false;
		_isPlatformColliding = false;
		_isIgnoringPlatform = true;
		ChangeAnimation(18);
		_level.AddAnimation(new BattleAnimation(_sprite, inPosition, _level)
		{
			AnimationStart = 20,
			AnimationLength = 5
		});
		_trailEvent = new TowerIceMageTrailEvent(_level, inPosition, iV, new ObjectTileSpecification
		{
			Category = EObjectTileCategory.Event
		}, _sprite);
		_level.RequestAddObject(_trailEvent);
		PlayCue(ESFX.EnemyIceMageIceCast);
	}

	public override void Update(float delta)
	{
		_trailEvent.UpdateParentProjectile(this);
		base.Update(delta);
	}

	public override void SilentKill()
	{
		base.SilentKill();
		_trailEvent.ParentIsDying();
	}

	public override void FadeKill()
	{
		if (!_isFading)
		{
			DoDeathAnimation(Bbox.Center);
		}
		base.FadeKill();
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		DoDeathAnimation(contactPoint);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	private void DoDeathAnimation(Point position)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, position, _level)
		{
			TeamSide = _defaultTeam,
			AnimationStart = 14,
			AnimationLength = 4
		});
		_level.PlayCue(ESFX.LunaisOrbImpactIce, position);
	}
}
