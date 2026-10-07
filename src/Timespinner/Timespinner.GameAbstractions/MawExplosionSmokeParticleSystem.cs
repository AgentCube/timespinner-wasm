using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

internal class MawExplosionSmokeParticleSystem : ParticleSystem
{
	private const int ExplosionOffsetY = 48;

	private const float PiOver8 = (float)Math.PI / 8f;

	private const float DirectionMin = (float)Math.PI * 7f / 8f;

	private const float DirectionMax = 3.5342917f;

	private static readonly Vector4 ExplosionBaseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.5f);

	private readonly bool _isExplodingToTheLeft;

	public MawExplosionSmokeParticleSystem(Texture2D inTexture, int howManyEffects, bool isExplodingToTheLeft)
		: base(inTexture, howManyEffects)
	{
		_isExplodingToTheLeft = isExplodingToTheLeft;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 750f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 1f;
		_maxLifetime = 2f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 128;
		_maxNumParticles = 128;
		_minFriction = 0.035f;
		_maxFriction = 0.05f;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
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
		float num = ParticleSystem.RandomBetween(-48f, 48f);
		base.InitializeParticle(p, new Vector2(where.X, where.Y + num));
		p.BaseColor = ExplosionBaseColor;
	}
}
