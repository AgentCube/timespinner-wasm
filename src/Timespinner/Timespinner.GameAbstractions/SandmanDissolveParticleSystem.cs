using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public sealed class SandmanDissolveParticleSystem : AnimatedParticleSystem
{
	private const int Width = 68;

	private const int Height = 128;

	private const int HalfWidth = 34;

	private const int TopRoundingThreshold = 108;

	private const int ThresholdDifference = 20;

	private static readonly Vector4 SandColorA = new Vector4(0.5f, 0.375f, 0.225f, 1f);

	private static readonly Vector4 SandColorB = new Vector4(0.125f, 0.09f, 0.05f, 1f);

	public SandmanDissolveParticleSystem(SpriteSheet sprite, int howManyEffects)
		: base(sprite, howManyEffects, 28, 1, 1f, EAnimationType.None)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 0f;
		_maxInitialSpeed = 25f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.02f;
		_maxFriction = 0.025f;
		_minLifetime = 1f;
		_maxLifetime = 1.5f;
		_minScale = 1f;
		_maxScale = 1f;
		_minNumParticles = 512;
		_maxNumParticles = 512;
		_minRotationSpeed = -5f;
		_maxRotationSpeed = 5f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int num = (int)Math.Round(ParticleSystem.RandomBetween(0f, 128f));
		int x;
		if (num > 108)
		{
			float num2 = (float)(num - 108) / 20f;
			float num3 = (float)Math.Cos(num2 * ((float)Math.PI / 2f)) * 34f;
			x = (int)Math.Round(ParticleSystem.RandomBetween(0f - num3, num3));
		}
		else
		{
			x = (int)Math.Round(ParticleSystem.RandomBetween(-34f, 34f));
		}
		num = -num;
		base.InitializeParticle(p, where.Add(new Point(x, num)));
		float amount = ParticleSystem.RandomBetween(0f, 1f);
		p.BaseColor = SandColorA.Lerp(SandColorB, amount);
		p.Acceleration = new Vector2(ParticleSystem.RandomBetween(-100f, 100f), 400f);
	}
}
