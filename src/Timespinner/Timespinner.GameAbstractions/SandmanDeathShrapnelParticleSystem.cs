using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class SandmanDeathShrapnelParticleSystem : AnimatedParticleSystem
{
	private const int Anim_Start = 37;

	private const int Anim_Length = 6;

	public SandmanDeathShrapnelParticleSystem(SpriteSheet sprite, int howManyEffects)
		: base(sprite, howManyEffects, 37, 6, 1f, EAnimationType.None)
	{
		_isStartIndexRandom = true;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 500f;
		_maxAcceleration = 500f;
		_minLifetime = 2f;
		_maxLifetime = 3f;
		_minScale = 0.3f;
		_maxScale = 1f;
		_minNumParticles = 32;
		_maxNumParticles = 32;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
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
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
		p.Acceleration.Y = _maxAcceleration;
	}
}
