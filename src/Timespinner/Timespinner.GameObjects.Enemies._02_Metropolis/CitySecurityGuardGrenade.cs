using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._02_Metropolis;

internal sealed class CitySecurityGuardGrenade : Projectile
{
	private const float TimeBeforeExploding = 1f;

	private const float RotationDecayRate = 0.05f;

	private const float BaseRotationSpeed = 10f;

	private const float MinimumTimeBetweenBounceSFX = 0.125f;

	private readonly int _barePower;

	private readonly int _explosionPower;

	private bool _hasTouchedTheGround;

	private bool _isDoneRotating;

	private float _timeSinceHittingGround;

	private float _bounceSFXTimer;

	public CitySecurityGuardGrenade(Level inLevel, SpriteSheet sprite, Point inPosition, Vector2 iV, int barePower, int explosionPower)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_sprite = sprite;
		_barePower = barePower;
		_explosionPower = explosionPower;
		base.Power = _barePower;
		base.Life = 5f;
		BboxOffset = new Point(2, 2);
		Bbox = new Rectangle(0, 0, 12, 12);
		DrawOrigin = new Vector2(4f, 4f);
		ChangeAnimation(11);
		_isAffectedByGravity = true;
		_isAffectedByFriction = true;
		_doesOverrideMaxSpeed = false;
		_doesBounceOnGround = true;
		_isFlying = true;
		_airDragFactor = 0f;
		_groundDragFactor = 0.1f;
		_rotationSpeed = (float)((!(iV.X < 0f)) ? 1 : (-1)) * 10f;
		base.DoesDieToEnemyProjectiles = true;
		base.DoesCollideWithTiles = true;
		_doesCollideWithCeilings = false;
		_doesCollideWithFloors = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithWalls = false;
		_doesBounceOnGround = true;
		_doesDieOnTiles = false;
		_doesRotateBasedOnVelocity = false;
		base.DoesDieOnImpact = false;
		_doesAutomaticallyEmitParticles = false;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_isTrailLengthAffectedByTime = false;
		_brushTrailSize = 8;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 0.0003f;
		_trailColor = Color.Red * 0.5f;
		_trailLength = 50;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _hasTouchedTheGround)
		{
			_timeSinceHittingGround += delta;
			if (_timeSinceHittingGround >= 1f)
			{
				DoExplosion();
			}
			if (!_isDoneRotating)
			{
				if (_rotationSpeed < 0f)
				{
					_rotationSpeed -= delta * 60f * (_rotationSpeed * 0.05f);
					if (_rotationSpeed >= 0f)
					{
						_rotationSpeed = 0f;
						_isDoneRotating = true;
					}
				}
				else if (_rotationSpeed > 0f)
				{
					_rotationSpeed -= delta * 60f * (_rotationSpeed * 0.05f);
					if (_rotationSpeed <= 0f)
					{
						_rotationSpeed = 0f;
						_isDoneRotating = true;
					}
				}
				_bounceSFXTimer -= delta;
			}
		}
		base.Update(delta);
	}

	public override bool DetermineDamage(Alive target, Rectangle collidingBbox)
	{
		bool result = base.DetermineDamage(target, collidingBbox);
		if (base.Power > 0f)
		{
			DoBounceOffObject();
		}
		return result;
	}

	public override void KillOnProjectileImpact(Projectile proj)
	{
		if (base.Power > 0f)
		{
			DoBounceOffObject();
		}
	}

	private void DoBounceOffObject()
	{
		base.Power = 0f;
		_rotationSpeed = 0f - _rotationSpeed;
		_velocity.X = 0f - _velocity.X;
		TryPlayBounceSFX(Bbox.Center);
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		_hasTouchedTheGround = true;
		bool flag = base.CollideSolidTile(tile, depth);
		if (flag)
		{
			TryPlayBounceSFX(new Point(tile.Position.X, tile.Bbox.Top));
		}
		return flag;
	}

	protected override void DoBounce(Point impactPoint)
	{
		_hasTouchedTheGround = true;
		if (!_isDoneRotating)
		{
			_rotationSpeed *= 2f;
		}
		base.DoBounce(impactPoint);
	}

	private void TryPlayBounceSFX(Point impactPoint)
	{
		if (_bounceSFXTimer <= 0f && Math.Abs(_velocity.Y) > 15f)
		{
			_bounceSFXTimer = 0.125f;
			PlayCue(ESFX.EnemySecGuardGrenadeBounce, impactPoint);
		}
	}

	private void DoExplosion()
	{
		Point point = Bbox.Center.Add(-4, -8);
		_level.AddAnimation(EBattleAnimationType.Boom, point);
		DamageArea damageArea = new DamageArea(base.Level, point, ETeamSide.Neutral, -1, null);
		damageArea.Life = 0.05f;
		damageArea.Power = _explosionPower;
		damageArea.DamageDimensions = new Point(32, 32);
		DamageArea newObject = damageArea;
		_level.RequestAddObject(newObject);
		Kill();
	}
}
