using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class GlassShatterParticleSystem : ParticleSystem
{
	private float _minStartRadius;

	private float _maxStartRadius;

	internal bool IsFacingLeft { get; set; }

	public GlassShatterParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		_isAccelerationDirectional = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 600f;
		_maxAcceleration = 700f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.1f;
		_maxScale = 0.2f;
		_minNumParticles = 5;
		_maxNumParticles = 8;
		_minStartRadius = 1f;
		_maxStartRadius = 8f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_accelerationBias.X = 0f;
		_accelerationBias.Y *= 1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(-(float)Math.PI / 4f, (float)Math.PI / 4f);
		return new Vector2((float)(IsFacingLeft ? (0.0 - Math.Cos(num)) : Math.Cos(num)), 0f - (float)Math.Sin(num));
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = ParticleSystem.RandomBetween(_minStartRadius, _maxStartRadius);
		Vector2 vector = new Vector2(num * ParticleSystem.RandomBetween(-1f, 1f), num * ParticleSystem.RandomBetween(-1f, 1f));
		base.InitializeParticle(p, where + vector);
		p.BaseColor = BaseColor;
	}
}
