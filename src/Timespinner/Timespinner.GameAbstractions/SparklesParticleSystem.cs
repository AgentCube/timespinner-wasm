using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class SparklesParticleSystem : AnimatedParticleSystem
{
	private Vector2 _minStartingOffset = Vector2.Zero;

	private Vector2 _maxStartingOffset = Vector2.Zero;

	public SparklesParticleSystem(SpriteSheet inSprite, int howManyEffects, int animationStart, int animationLength)
		: base(inSprite, howManyEffects, animationStart, animationLength, 0.065f, EAnimationType.Cycle)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 50f;
		_minAcceleration = 10f;
		_maxAcceleration = 15f;
		_minLifetime = 0.5f;
		_maxLifetime = 0.75f;
		_minScale = 1f;
		_maxScale = 1f;
		_minNumParticles = 5;
		_maxNumParticles = 12;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
		_minStartingOffset = new Vector2(-8f, -8f);
		_maxStartingOffset = new Vector2(8f, 8f);
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(60f), MathHelper.ToRadians(120f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(_minStartingOffset.X, _maxStartingOffset.X), ParticleSystem.RandomBetween(_minStartingOffset.Y, _maxStartingOffset.Y));
		base.InitializeParticle(p, Vector2.Add(where, value));
	}
}
