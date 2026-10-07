using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class SmallSmokePlumeParticleSystem : ParticleSystem
{
	public SmallSmokePlumeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 5f;
		_maxInitialSpeed = 30f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.75f;
		_maxLifetime = 2f;
		_minScale = 0.15f;
		_maxScale = 0.5f;
		_minNumParticles = 3;
		_maxNumParticles = 5;
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

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		Vector4 baseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.5f);
		p.BaseColor = baseColor;
		p.Acceleration.X += ParticleSystem.RandomBetween(-30f, -10f);
	}
}
