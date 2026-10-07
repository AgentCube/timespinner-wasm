using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.FamiliarParticleEffects;

public class FamiliarCrowSporeParticleSystem : AnimatedParticleSystem
{
	private const int EmissionWidth = 16;

	private const int EmissionHeight = 4;

	private const float MaxEmissionMultiplier = 8f;

	private float _emissionMultiplier = 1f;

	public FamiliarCrowSporeParticleSystem(SpriteSheet inSprite, int howManyEffects)
		: base(inSprite, howManyEffects, 32, 0, 1f, EAnimationType.None)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 50f;
		_minAcceleration = 100f;
		_maxAcceleration = 125f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 1f;
		_maxLifetime = 2f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 5;
		_maxNumParticles = 5;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_accelerationBias.X *= 0.5f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float degrees = ParticleSystem.RandomBetween(-45f, -135f);
		float num = MathHelper.ToRadians(degrees);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	public void AddParticles(Vector2 where, float emissionMultiplier)
	{
		_emissionMultiplier = Math.Min(emissionMultiplier, 8f);
		AddParticles(where);
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int x = (int)ParticleSystem.RandomBetween(-16f, 16f);
		int y = (int)ParticleSystem.RandomBetween(-4f, 4f);
		base.InitializeParticle(p, where.Add(new Point(x, y)));
		float num = ParticleSystem.RandomBetween(0.5f, 1f);
		float y2 = 0.75f * num;
		float z = 0.25f * num;
		p.BaseColor = new Vector4(0.9f, y2, z, 1f);
		p.Velocity.X *= _emissionMultiplier;
	}
}
