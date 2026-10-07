using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.LunaisParticleEffects;

public class ChargeGustParticleSystem : AnimatedParticleSystem
{
	private bool _lastDirection;

	public ChargeGustParticleSystem(SpriteSheet inSprite, int howManyEffects)
		: base(inSprite, howManyEffects, 24, 1, 0.06f, EAnimationType.None)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 20f;
		_maxInitialSpeed = 85f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.5f;
		_maxScale = 1.5f;
		_minNumParticles = 5;
		_maxNumParticles = 10;
		_minRotationSpeed = -2f;
		_maxRotationSpeed = 2f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_velocityBias.Y *= 0f;
		_accelerationBias.Y *= 0f;
	}

	public void AddParticles(Vector2 where, float amount)
	{
		base.AddParticles(where);
	}

	protected override Vector2 PickRandomDirection()
	{
		_lastDirection = !_lastDirection;
		return new Vector2((!_lastDirection) ? 1 : (-1), 0f);
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		if (p is AnimatedParticle animatedParticle)
		{
			animatedParticle.IsFacingLeft = animatedParticle.Velocity.X < 0f;
		}
		p.BaseColor = Vector4.One * 0.5f;
	}
}
