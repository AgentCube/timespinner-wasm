using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class BirdBossWindyParticleSystem : AnimatedParticleSystem
{
	private readonly int _width;

	public BirdBossWindyParticleSystem(SpriteSheet inSprite, int howManyEffects, int width)
		: base(inSprite, howManyEffects, 3, 3, 1f, EAnimationType.None)
	{
		_isStartIndexRandom = true;
		_width = width;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 100f;
		_maxAcceleration = 200f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.8f;
		_maxScale = 1f;
		_minNumParticles = 2;
		_maxNumParticles = 3;
		_minRotationSpeed = -40f;
		_maxRotationSpeed = 40f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.1f;
		_velocityBias.Y *= -1f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = (float)_width / 2f;
		Vector2 where2 = new Vector2(where.X + ParticleSystem.RandomBetween(0f - num, num), where.Y);
		base.InitializeParticle(p, where2);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
	}
}
