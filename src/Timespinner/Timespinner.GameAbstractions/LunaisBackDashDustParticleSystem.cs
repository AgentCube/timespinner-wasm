using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class LunaisBackDashDustParticleSystem : ParticleSystem
{
	public bool IsParticleSystemFacingLeft { get; set; }

	public LunaisBackDashDustParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(0.22f, 0.2f, 0.22f, 0.3f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 40f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.1f;
		_maxFriction = 0.05f;
		_minLifetime = 0.1f;
		_maxLifetime = 0.25f;
		_minScale = 0.25f;
		_maxScale = 0.5f;
		_minNumParticles = 2;
		_maxNumParticles = 5;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.Y *= 2.5f;
		_accelerationBias.Y *= 2.5f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, 45f);
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
