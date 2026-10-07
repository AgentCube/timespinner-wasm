using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class RavenBossWindyParticleSystem : AnimatedParticleSystem
{
	private readonly int _height;

	internal bool IsBlowingToTheLeft { get; set; }

	public RavenBossWindyParticleSystem(SpriteSheet inSprite, int howManyEffects, int height)
		: base(inSprite, howManyEffects, 21, 3, 1f, EAnimationType.None)
	{
		_isStartIndexRandom = true;
		_height = height;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 400f;
		_maxInitialSpeed = 500f;
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
		_velocityBias.Y *= 0.1f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI);
		Vector2 zero = Vector2.Zero;
		zero.Y = (float)Math.Cos(num);
		zero.X = (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = (float)_height / 2f;
		Vector2 where2 = new Vector2(where.X, where.Y + ParticleSystem.RandomBetween(0f - num, num));
		base.InitializeParticle(p, where2);
		p.Acceleration.Y += ParticleSystem.RandomBetween(-50f, 50f);
		if (IsBlowingToTheLeft != p.Velocity.X < 0f)
		{
			p.Velocity.X = 0f - p.Velocity.X;
		}
	}
}
