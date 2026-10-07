using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public sealed class SandmanCrusherDustParticleSystem : ParticleSystem
{
	private const int Radius = 64;

	private static readonly Vector4 SandColorA = new Vector4(0.63f, 0.5f, 0.3f, 1f);

	private static readonly Vector4 SandColorB = new Vector4(0.25f, 0.2f, 0.125f, 1f);

	public SandmanCrusherDustParticleSystem(Texture2D texture, int howManyEffects)
		: base(texture, howManyEffects)
	{
		base.DoParticleUseLifeForAlpha = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.025f;
		_maxFriction = 0.025f;
		_minLifetime = 1f;
		_maxLifetime = 1.5f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 128;
		_maxNumParticles = 128;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int x = (int)Math.Round(ParticleSystem.RandomBetween(-64f, 64f));
		base.InitializeParticle(p, where.Add(new Point(x, 0)));
		float amount = ParticleSystem.RandomBetween(0f, 1f);
		p.BaseColor = SandColorA.Lerp(SandColorB, amount);
		p.Acceleration = new Vector2(0f, 400f);
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
	}
}
