using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions.LunaisParticleEffects;

public class SmallSnowingParticleSystem : ParticleSystem
{
	public SmallSnowingParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(0.8f, 0.9f, 1f, 0.9f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 20f;
		_minAcceleration = 20f;
		_maxAcceleration = 30f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.4f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= 1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(3.926991f, 5.4977875f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(-4f, 4f), ParticleSystem.RandomBetween(-4f, 4f));
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, Vector2.Add(where, value));
	}
}
