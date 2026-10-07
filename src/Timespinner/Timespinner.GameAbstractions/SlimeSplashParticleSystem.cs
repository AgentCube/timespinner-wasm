using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class SlimeSplashParticleSystem : ParticleSystem
{
	private Vector2 _lastWhere = Vector2.Zero;

	public SlimeSplashParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 80f;
		_minAcceleration = 300f;
		_maxAcceleration = 500f;
		_minLifetime = 0.5f;
		_maxLifetime = 0.75f;
		_minScale = 0.1f;
		_maxScale = 0.25f;
		_minNumParticles = 5;
		_maxNumParticles = 5;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.75f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -0.8f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(45f), MathHelper.ToRadians(135f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, new Vector2(where.X + ParticleSystem.RandomBetween(-4f, 4f), where.Y));
		p.BaseColor = BaseColor;
		_lastWhere = where;
	}

	public override void Update(float delta)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive && particle.Position.Y > _lastWhere.Y - 6f && particle.Velocity.Y > 0f)
			{
				KillParticle(particle);
			}
		}
		base.Update(delta);
	}
}
