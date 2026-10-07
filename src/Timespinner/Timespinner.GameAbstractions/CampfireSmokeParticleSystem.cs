using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class CampfireSmokeParticleSystem : ParticleSystem
{
	private static readonly Vector4 SmokeColor = new Vector4(0.15f, 0.15f, 0.15f, 0.15f);

	public CampfireSmokeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = SmokeColor;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 15f;
		_maxInitialSpeed = 25f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 1.5f;
		_maxLifetime = 2f;
		_minScale = 0.2f;
		_maxScale = 0.5f;
		_minNumParticles = 2;
		_maxNumParticles = 3;
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
		p.Acceleration.X += ParticleSystem.RandomBetween(-4f, 4f);
		p.BaseColor = BaseColor;
	}
}
