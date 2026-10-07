using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

public class CursedMothSporeParticleSystem : AnimatedParticleSystem
{
	private const int EmissionWidth = 16;

	private const int EmissionHeight = 8;

	private const float MaxEmissionMultiplier = 8f;

	private float _emissionMultiplier = 1f;

	public CursedMothSporeParticleSystem(SpriteSheet inSprite, int howManyEffects, Vector4 sporeColor)
		: base(inSprite, howManyEffects, 9, 0, 1f, EAnimationType.None)
	{
		BaseColor = sporeColor;
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
		_maxNumParticles = 15;
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
		int y = (int)ParticleSystem.RandomBetween(-8f, 8f);
		base.InitializeParticle(p, where.Add(new Point(x, y)));
		p.BaseColor = BaseColor;
		p.Velocity.X *= _emissionMultiplier;
	}
}
