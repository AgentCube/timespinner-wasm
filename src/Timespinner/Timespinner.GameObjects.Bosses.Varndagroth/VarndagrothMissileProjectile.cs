using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Varndagroth;

internal sealed class VarndagrothMissileProjectile : Projectile
{
	private const int BboxHeight = 8;

	private const int BboxWidth = 8;

	private const float SeekingTimeout = 1f;

	private readonly ParticleSystem _trailParticleSystem;

	private bool _isSeekingTimedOut;

	private Vector2 _lastAcceleration = Vector2.Zero;

	public VarndagrothMissileProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, Point inTarget, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 8);
		_bboxOffset = new Point(4, 4);
		DrawOrigin = new Vector2(8f, 8f);
		_targetPosition = inTarget;
		_power = (int)Math.Ceiling(1.2f * (float)baseDamage);
		_force = 0;
		_life = 3f;
		_doesDieOnTiles = true;
		base.DoesCollideWithTiles = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = false;
		base.DoesDieToEnemyProjectiles = true;
		_isFlying = true;
		_animationSpeed = 0f;
		_maxMoveSpeed = 300f;
		ChangeAnimation(15, 3, 0.1f, EAnimationType.Cycle);
		_trailParticleSystem = new VarndagrothMissileParticleSystem(_level.GCM.TxParticleDust, 10);
		_particleSystems.Add(_trailParticleSystem);
		_doesDrawParticleSystemsUnder = true;
		PlayCue(ESFX.BossEyeProjectileFly, isLooped: true);
	}

	public override void Update(float delta)
	{
		if (_life < 1f)
		{
			_isSeekingTimedOut = true;
		}
		if (!_isSeekingTimedOut)
		{
			Vector2 vector = new Vector2(_targetPosition.X - _position.X, _targetPosition.Y - _position.Y);
			if (vector.X != 0f && vector.Y != 0f && vector.LengthSquared() > 256f)
			{
				vector.Normalize();
				_lastAcceleration = vector * 200f;
				_particleEmissionOffset = -vector * 8f;
				_particleEmissionOffset.Y -= 4f;
			}
			else
			{
				_isSeekingTimedOut = true;
			}
			_velocity = new Vector2(MathHelper.Lerp(_velocity.X, _lastAcceleration.X, 0.05f), MathHelper.Lerp(_velocity.Y, _lastAcceleration.Y, 0.05f));
		}
		if (IsOutsideOfLevel())
		{
			SilentKill();
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	public override void Kill()
	{
		_level.AddAnimation(EBattleAnimationType.SmallBoom, _bbox.Center, _teamSide, base.Rotation > 3f, doesPlaySFX: false);
		_level.PlayCue(ESFX.BossEyeProjectileDestroy, Position);
		base.Kill();
	}
}
