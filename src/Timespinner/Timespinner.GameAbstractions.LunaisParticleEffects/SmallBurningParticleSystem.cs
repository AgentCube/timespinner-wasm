using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.LunaisParticleEffects;

public class SmallBurningParticleSystem : AnimatedParticleSystem
{
	private Vector2 _minStartingOffset = Vector2.Zero;

	private Vector2 _maxStartingOffset = Vector2.Zero;

	public SmallBurningParticleSystem(SpriteSheet inSprite, int howManyEffects)
		: base(inSprite, howManyEffects, 4, 4, 0.065f, EAnimationType.Cycle)
	{
		BaseColor = Vector4.One * 0.66f;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 50f;
		_minAcceleration = 5f;
		_maxAcceleration = 10f;
		_minLifetime = 0.1f;
		_maxLifetime = 0.2f;
		_minScale = 0.25f;
		_maxScale = 0.75f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
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
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, Vector2.Add(where, value));
	}
}
