using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class XarionVoidSwirlParticleSystem : AnimatedParticleSystem
{
	private static readonly Vector4 SwirlColorA = new Vector4(0.4f, 0.125f, 0.5f, 0.5f);

	private static readonly Vector4 SwirlColorB = new Vector4(0.1f, 0.25f, 0.4f, 0.5f);

	public XarionVoidSwirlParticleSystem(SpriteSheet sprite, int howManyEffects, int animationIndex)
		: base(sprite, howManyEffects, animationIndex, 1, 1f, EAnimationType.None)
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
		_maxScale = 0.9f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = 4f;
		_maxRotationSpeed = 6f;
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
		float amount = (float)ParticleSystem.Random.NextDouble();
		p.BaseColor = SwirlColorA.Lerp(SwirlColorB, amount);
	}
}
