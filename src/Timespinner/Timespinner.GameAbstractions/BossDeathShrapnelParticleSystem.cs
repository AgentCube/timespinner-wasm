using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class BossDeathShrapnelParticleSystem : ParticleSystem
{
	public BossDeathShrapnelParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minAngle = 0f;
		_maxAngle = (float)Math.PI * 2f;
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 400f;
		_minAcceleration = 800f;
		_maxAcceleration = 900f;
		_minLifetime = 2f;
		_maxLifetime = 3f;
		_minScale = 0.3f;
		_maxScale = 0.8f;
		_minNumParticles = 10;
		_maxNumParticles = 25;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -0.75f;
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
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
		p.BaseColor = BaseColor;
	}
}
