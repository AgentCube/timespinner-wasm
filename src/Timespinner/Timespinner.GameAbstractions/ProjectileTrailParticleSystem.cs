using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public abstract class ProjectileTrailParticleSystem : ParticleSystem
{
	protected float _fanAmount;

	protected float _decelerationRate;

	protected ProjectileTrailParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeParticle(Particle p, Vector2 where, Vector2 inVelocity)
	{
		Vector2 vector = Vector2.Normalize(inVelocity);
		if (_fanAmount != 0f)
		{
			float radians = ParticleSystem.RandomBetween(0f - _fanAmount, _fanAmount);
			vector = Vector2.TransformNormal(vector, Matrix.CreateRotationZ(radians));
		}
		Vector2 vector2 = Vector2.Multiply(Vector2.Negate(vector), ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration) + _decelerationRate);
		float lifetime = ParticleSystem.RandomBetween(_minLifetime, _maxLifetime);
		float scale = ParticleSystem.RandomBetween(_minScale, _maxScale);
		float rotationSpeed = ParticleSystem.RandomBetween(_minRotationSpeed, _maxRotationSpeed);
		float rotation = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		p.Initialize(where, inVelocity * _velocityBias, vector2 * _accelerationBias, lifetime, scale, 0f, rotationSpeed, rotation, 0f);
	}

	public override void Update(float delta, Point tetherBase)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				particle.Update(delta);
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
	}
}
