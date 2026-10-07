using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class SmokePlumeParticleSystem : ParticleSystem
{
	private static readonly Vector4 ExplosionBaseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.5f);

	public SmokePlumeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 50f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 1f;
		_maxLifetime = 2.5f;
		_minScale = 0.25f;
		_maxScale = 0.75f;
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
		p.BaseColor = ExplosionBaseColor;
		p.Acceleration.X += ParticleSystem.RandomBetween(-30f, -10f);
	}
}
