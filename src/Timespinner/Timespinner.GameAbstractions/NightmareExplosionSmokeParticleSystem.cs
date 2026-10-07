using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

internal class NightmareExplosionSmokeParticleSystem : ParticleSystem
{
	public NightmareExplosionSmokeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
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
		float num = ParticleSystem.RandomBetween((float)Math.PI * 9f / 40f, 3.2986722f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector4 baseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.3f);
		float num = ParticleSystem.RandomBetween(-32f, 32f);
		float num2 = ParticleSystem.RandomBetween(-32f, 32f);
		base.InitializeParticle(p, new Vector2(where.X + num, where.Y + num2));
		p.BaseColor = baseColor;
	}
}
