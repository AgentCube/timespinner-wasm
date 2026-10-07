using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class SoulStreamSwirlParticleSystem : AnimatedParticleSystem
{
	public SoulStreamSwirlParticleSystem(SpriteSheet sprite, int howManyEffects)
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
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.5f;
		_maxScale = 0.9f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = 1f;
		_maxRotationSpeed = 2f;
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
