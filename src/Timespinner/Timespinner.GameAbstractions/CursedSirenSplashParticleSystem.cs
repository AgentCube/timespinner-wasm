using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class CursedSirenSplashParticleSystem : ParticleSystem
{
	private Vector2 _lastWhere;

	private Vector2 _splashDirection;

	public CursedSirenSplashParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 125f;
		_maxInitialSpeed = 180f;
		_minAcceleration = 400f;
		_maxAcceleration = 500f;
		_minLifetime = 0.5f;
		_maxLifetime = 0.75f;
		_minScale = 0.1f;
		_maxScale = 0.35f;
		_minNumParticles = 35;
		_maxNumParticles = 35;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -0.8f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(-20f), MathHelper.ToRadians(60f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num) * _splashDirection.X;
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, new Vector2(where.X + ParticleSystem.RandomBetween(-4f, 4f), where.Y));
		p.BaseColor = BaseColor;
		_lastWhere = where;
	}

	public override void AddParticles(Vector2 where, Vector2 velocity)
	{
		_splashDirection = velocity;
		base.AddParticles(where);
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
