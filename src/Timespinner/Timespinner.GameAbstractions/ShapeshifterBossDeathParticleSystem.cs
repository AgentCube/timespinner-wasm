using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class ShapeshifterBossDeathParticleSystem : ParticleSystem
{
	private const float DirectionChangeLifeThreshold = 3f;

	public ShapeshifterBossDeathParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 17f;
		_maxAcceleration = 17f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.15f;
		_maxScale = 0.25f;
		_minNumParticles = 32;
		_maxNumParticles = 32;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		p.Acceleration.X = 0f - p.Velocity.X;
		p.Acceleration.Y = 0f - _maxAcceleration;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				float timeSinceStart = particle.TimeSinceStart;
				particle.Update(delta);
				if (particle.TimeSinceStart >= 3f && timeSinceStart < 3f)
				{
					particle.Acceleration = new Vector2(0f - particle.Acceleration.X, particle.Acceleration.Y);
				}
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
	}
}
