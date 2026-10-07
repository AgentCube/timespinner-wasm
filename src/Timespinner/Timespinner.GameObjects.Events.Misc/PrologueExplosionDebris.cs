using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class PrologueExplosionDebris : GameEvent
{
	private const int Anim_DebrisStart = 56;

	private const float TimeBetweenParticles = 0.015f;

	private readonly DebrisSmokeParticleSystem _smokeParticles;

	private float _particlesTimer;

	private float _rotationSpeed;

	public PrologueExplosionDebris(Level inLevel, Point inPosition, int index)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_sprite = _level.GCM.SpPlatforms;
		ChangeAnimation(56 + index % 4);
		int num = _frameSource.Width / 4;
		int num2 = _frameSource.Height / 4;
		_bboxOffset = new Point(num, num2);
		Bbox = new Rectangle(0, 0, num * 2, num2 * 2);
		DrawOrigin = new Vector2((float)_frameSource.Width / 2f, (float)_frameSource.Height / 2f);
		_isAffectedByGravity = true;
		_isAffectedByLevelBounds = false;
		base.DoesCollideWithTiles = true;
		_airDragFactor = 0.015f;
		_isAffectedByTime = true;
		base.CanBeTriggered = false;
		_doesBounceOnGround = true;
		_airDragFactor = 0.0015f;
		_maxMoveSpeed = 1000f;
		_gravityAcceleration = 1000f;
		_smokeParticles = new DebrisSmokeParticleSystem(_level.GCM.TxParticleSmoke, 24);
		_particleSystems.Add(_smokeParticles);
		_doesDrawParticleSystemsUnder = true;
	}

	internal void Reset(Point position, Vector2 velocity, float rotationSpeed)
	{
		base.Rotation = 0f;
		Position = position;
		SnapBboxToPosition();
		_velocity = velocity;
		_rotationSpeed = rotationSpeed;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && Math.Abs(_rotationSpeed) > 0.1f && Math.Abs(_velocity.X) > 10f)
		{
			float num = base.Rotation + delta * _rotationSpeed;
			if (num >= 6.28f)
			{
				num -= 6.28f;
			}
			base.Rotation = num;
			_particlesTimer -= delta;
			if (_particlesTimer <= 0f)
			{
				_particlesTimer += 0.015f;
				_smokeParticles.AddParticles(Bbox.Center.ToVector2(), _velocity);
			}
		}
		base.Update(delta);
	}
}
