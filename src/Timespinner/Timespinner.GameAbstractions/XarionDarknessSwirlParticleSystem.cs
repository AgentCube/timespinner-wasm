using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class XarionDarknessSwirlParticleSystem : AnimatedParticleSystem
{
	private static readonly Vector4 DarknessColor = new Vector4(0.1f, 0.125f, 0.15f, 0.5f);

	public XarionDarknessSwirlParticleSystem(SpriteSheet sprite, int howManyEffects, int animationIndex)
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
		_minLifetime = 2f;
		_maxLifetime = 2f;
		_minScale = 0.7f;
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
		p.BaseColor = DarknessColor;
	}
}
