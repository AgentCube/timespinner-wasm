using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class TimeGateSwirlParticleSystem : AnimatedParticleSystem
{
	public TimeGateSwirlParticleSystem(SpriteSheet sprite, int howManyEffects)
		: base(sprite, howManyEffects, 32, 1, 1f, EAnimationType.None)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 5f;
		_maxInitialSpeed = 10f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.01f;
		_maxFriction = 0.015f;
		_minLifetime = 1f;
		_maxLifetime = 2f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = 0.5f;
		_maxRotationSpeed = 1f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
