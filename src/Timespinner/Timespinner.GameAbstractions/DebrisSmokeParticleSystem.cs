using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class DebrisSmokeParticleSystem : ParticleSystem
{
	public DebrisSmokeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 50f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.2f;
		_maxScale = 0.5f;
		_minNumParticles = 2;
		_maxNumParticles = 2;
		_minRotationSpeed = -(float)Math.PI / 8f;
		_maxRotationSpeed = (float)Math.PI / 8f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(80f), MathHelper.ToRadians(100f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where, Vector2 velocity)
	{
		Vector4 baseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.5f);
		base.InitializeParticle(p, where);
		p.BaseColor = baseColor;
		p.Acceleration.X += ParticleSystem.RandomBetween(-30f, -10f);
	}
}
