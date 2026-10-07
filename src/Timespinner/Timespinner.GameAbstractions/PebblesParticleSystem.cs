using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class PebblesParticleSystem : ParticleSystem
{
	public PebblesParticleSystem(Texture2D inTexture, int howManyEffects, int levelID)
		: base(inTexture, howManyEffects)
	{
		BaseColor = DustParticleSystem.GetDustColor(levelID) * 3f;
		BaseColor.W = 1f;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 75f;
		_minAcceleration = 400f;
		_maxAcceleration = 500f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.05f;
		_maxScale = 0.1f;
		_minNumParticles = 2;
		_maxNumParticles = 3;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
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
