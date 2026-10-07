using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class ExtinguishSmokeParticleSystem : ParticleSystem
{
	public bool IsParticleSystemFacingLeft { get; set; }

	public ExtinguishSmokeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.5f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 30f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 0.75f;
		_maxLifetime = 1.5f;
		_minScale = 0.15f;
		_maxScale = 0.5f;
		_minNumParticles = 8;
		_maxNumParticles = 10;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_minScaleGrowthRate = -0.5f;
		_maxScaleGrowthRate = -0.1f;
		_accelerationBias.X = 0.75f;
		_velocityBias.X = 0.75f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float degrees = ParticleSystem.RandomBetween(75f, 105f);
		float num = MathHelper.ToRadians(degrees);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
