using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class FeatherReviveParticleSystem : AnimatedParticleSystem
{
	public FeatherReviveParticleSystem(SpriteSheet inSprite, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, length, 0.065f, EAnimationType.PingPong)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 0.5f;
		_maxLifetime = 0.8f;
		_minScale = 0.35f;
		_maxScale = 0.75f;
		_minNumParticles = 3;
		_maxNumParticles = 5;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(-1f, 4.141593f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}
}
