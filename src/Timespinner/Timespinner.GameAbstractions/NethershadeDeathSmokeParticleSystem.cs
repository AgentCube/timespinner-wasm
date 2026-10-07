using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class NethershadeDeathSmokeParticleSystem : ParticleSystem
{
	private static readonly Vector4 SmokeColor = new Vector4(0.2f, 0.15f, 0.3f, 0.1f);

	public NethershadeDeathSmokeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.1f;
		_maxFriction = 0.1f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 15;
		_maxNumParticles = 15;
		_minRotationSpeed = -(float)Math.PI / 2f;
		_maxRotationSpeed = (float)Math.PI / 2f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = SmokeColor;
		p.Acceleration = new Vector2(0f, -150f);
	}
}
