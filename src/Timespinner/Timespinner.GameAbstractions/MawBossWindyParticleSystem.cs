using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class MawBossWindyParticleSystem : AnimatedParticleSystem
{
	private readonly bool _isBlowingLeft;

	private readonly int _height;

	public MawBossWindyParticleSystem(SpriteSheet inSprite, int howManyEffects, int height, bool isBlowingLeft)
		: base(inSprite, howManyEffects, 0, 3, 1f, EAnimationType.None)
	{
		_isStartIndexRandom = true;
		_isBlowingLeft = isBlowingLeft;
		_height = height;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 300f;
		_maxInitialSpeed = 500f;
		_minAcceleration = 500f;
		_maxAcceleration = 750f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = -40f;
		_maxRotationSpeed = 40f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= -1f;
		_velocityBias.Y *= 0.01f;
		_accelerationBias.Y *= 0.05f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween((float)Math.PI / 2f, 4.712389f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = (float)Math.Sin(num);
		if (_isBlowingLeft)
		{
			zero.X = 0f - zero.X;
		}
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = ParticleSystem.RandomBetween(0f, 256f);
		float num2 = (float)_height / 2f;
		Vector2 where2 = new Vector2(where.X + num, where.Y + ParticleSystem.RandomBetween(0f - num2, num2));
		base.InitializeParticle(p, where2);
		if (_isBlowingLeft != p.Acceleration.X < 0f)
		{
			p.Acceleration.X = 0f - p.Acceleration.X;
		}
	}
}
