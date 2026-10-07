using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

internal class NightmareDeathSmokeParticleSystem : ParticleSystem
{
	private const float DirectionMin = (float)Math.PI * 9f / 20f;

	private const float DirectionMax = (float)Math.PI * 11f / 20f;

	private static readonly Vector4 ExplosionBaseColorA = new Vector4(0.125f, 0.05f, 0.15f, 0.35f);

	private static readonly Vector4 ExplosionBaseColorB = new Vector4(0.1f, 0.15f, 0.1f, 0.35f);

	public NightmareDeathSmokeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 300f;
		_maxInitialSpeed = 500f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.4f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minFriction = 0.02f;
		_maxFriction = 0.03f;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween((float)Math.PI * 9f / 20f, (float)Math.PI * 11f / 20f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		float num = ParticleSystem.RandomBetween(0f, 1f);
		Vector4 baseColor = ((num < 0.5f) ? ExplosionBaseColorA : ExplosionBaseColorB);
		p.BaseColor = baseColor;
	}
}
