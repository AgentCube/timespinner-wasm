using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class LabSpiderDustParticleSystem : ParticleSystem
{
	public LabSpiderDustParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.2f;
		_maxFriction = 0.1f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.2f;
		_maxScale = 0.5f;
		_minNumParticles = 3;
		_maxNumParticles = 6;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.Y *= 0.5f;
		_accelerationBias.Y *= 0.5f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, 30f);
		num = ((ParticleSystem.Random.Next(2) == 0) ? (180f - num) : num);
		float num2 = MathHelper.ToRadians(num);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num2);
		zero.Y = 0f - (float)Math.Sin(num2);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
		p.BaseColor = BaseColor;
	}
}
