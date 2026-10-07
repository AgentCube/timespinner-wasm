using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

internal class MawExplosionBurstParticleSystem : ParticleSystem
{
	private const int ExplosionOffsetX = 32;

	private const int ExplosionOffsetY = 40;

	private const float PiOver8 = (float)Math.PI / 8f;

	private const float DirectionMin = (float)Math.PI * 7f / 8f;

	private const float DirectionMax = 3.5342917f;

	private static readonly Vector4 ExplosionBaseColor = new Vector4(1f, 0.66f, 0.33f, 0.8f);

	private readonly bool _isExplodingToTheLeft;

	public MawExplosionBurstParticleSystem(Texture2D inTexture, int howManyEffects, bool isExplodingToTheLeft)
		: base(inTexture, howManyEffects)
	{
		_isExplodingToTheLeft = isExplodingToTheLeft;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.15f;
		_maxLifetime = 0.15f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 32;
		_maxNumParticles = 32;
		_minFriction = 0.035f;
		_maxFriction = 0.05f;
		_minRotationSpeed = -5f;
		_maxRotationSpeed = 5f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween((float)Math.PI * 7f / 8f, 3.5342917f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		if (zero.X <= 0f != _isExplodingToTheLeft)
		{
			zero.X = 0f - zero.X;
		}
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = ParticleSystem.RandomBetween(-32f, 32f);
		float num2 = ParticleSystem.RandomBetween(-40f, 40f);
		base.InitializeParticle(p, new Vector2(where.X + num, where.Y + num2));
		p.BaseColor = ExplosionBaseColor;
	}
}
