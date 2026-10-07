using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class HalfDustParticleSystem : ParticleSystem
{
	public bool IsParticleSystemFacingLeft { get; set; }

	public HalfDustParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(0.22f, 0.2f, 0.22f, 0.3f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.1f;
		_maxFriction = 0.05f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.7f;
		_minScale = 0.1f;
		_maxScale = 0.5f;
		_minNumParticles = 20;
		_maxNumParticles = 30;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.Y *= 0.4f;
		_accelerationBias.Y *= 0.5f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, 30f);
		num = (IsParticleSystemFacingLeft ? num : (180f - num));
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
