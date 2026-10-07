using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses;

internal sealed class GodBirdGoopBall : Projectile
{
	private readonly BirdBossSpitParticleSystem _trailParticles;

	private readonly GodBirdFloorGoopEvent _goopEvent;

	public GodBirdGoopBall(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, BirdBossSpitParticleSystem spitParticles, int baseDamage)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 8, inPosition.Y - 8, 16, 16);
		DrawOrigin = new Vector2(8f, 8f);
		ChangeAnimation(18);
		_power = (int)Math.Ceiling((float)baseDamage * 0.75f);
		_force = 0;
		_life = 1f;
		_doesRotateBasedOnVelocity = false;
		_isAffectedByGravity = true;
		_gravityAcceleration = 500f;
		_isAffectedByFriction = false;
		_doesCollideWithFloors = true;
		base.DoesCollideWithTiles = true;
		base.DoesDieOnImpact = false;
		base.DoesDieToEnemyProjectiles = false;
		_doesDieOnTiles = true;
		_animationSpeed = 0f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_doesRotateBasedOnVelocity = false;
		_isFlying = true;
		_trailParticles = spitParticles;
		_goopEvent = new GodBirdFloorGoopEvent(_level, Position, new ObjectTileSpecification(), _sprite);
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (!base.IsFrozen && !_isFading)
		{
			_emitTimer -= delta;
			if (_emitTimer < 0f)
			{
				_emitTimer = _trailParticles.MaxEmissionCounter;
				_trailParticles.AddParticles(_particleEmissionOffset.Add(_position), _velocity);
			}
		}
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		bool isActive = _goopEvent.IsActive;
		_goopEvent.Reset(contactPoint);
		if (!isActive)
		{
			_level.AddEvent(_goopEvent);
		}
		_level.PlayCue(ESFX.EnemyCheveuxTowerVomitSplat, contactPoint);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	public override void KillOnProjectileImpact(Projectile proj)
	{
		Kill();
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Position = position;
		base.Velocity = iV;
		_rotationSpeed = iV.X * 0.1f;
		base.ID = -1;
		_isFading = false;
		_fadeTimer = 0f;
		base.CanDamageThings = true;
		_life = 1f;
		_trailParticles.KillOffParticles(0f);
	}
}
