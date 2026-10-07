using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions.StatusParticleEffects;

public class DisintegrateAshParticleSystem : ParticleSystem
{
	public DisintegrateAshParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 20f;
		_maxAcceleration = 30f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.4f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(60f), MathHelper.ToRadians(120f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, where);
	}

	internal void AddParticles(int left, int right, int bottom)
	{
		Vector2 where = new Vector2(ParticleSystem.RandomBetween(left, right), bottom);
		AddParticles(where);
	}
}
