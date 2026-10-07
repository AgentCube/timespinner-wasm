using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class EmperorDeathSmokeParticleSystem : ParticleSystem
{
	private static readonly Vector4 BlueSmokeColorA = new Vector4(0.15f, 0.15f, 0.3f, 0.25f);

	private static readonly Vector4 BlueSmokeColorB = new Vector4(0.2f, 0.3f, 0.5f, 0.15f);

	private static readonly Vector4 PinkSmokeColorA = new Vector4(0.3f, 0.15f, 0.2f, 0.25f);

	private static readonly Vector4 PinkSmokeColorB = new Vector4(0.5f, 0.2f, 0.4f, 0.15f);

	private readonly bool _isViletianEmperor;

	public EmperorDeathSmokeParticleSystem(Texture2D inTexture, int howManyEffects, bool isViletianEmperor)
		: base(inTexture, howManyEffects)
	{
		_isViletianEmperor = isViletianEmperor;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.15f;
		_maxFriction = 0.15f;
		_minLifetime = 1.5f;
		_maxLifetime = 2.5f;
		_minScale = 0.3f;
		_maxScale = 0.75f;
		_minNumParticles = 15;
		_maxNumParticles = 15;
		_minRotationSpeed = -(float)Math.PI / 2f;
		_maxRotationSpeed = (float)Math.PI / 2f;
		_spriteBlendMode = BlendState.AlphaBlend;
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
		float amount = (float)ParticleSystem.Random.NextDouble();
		if (_isViletianEmperor)
		{
			p.BaseColor = PinkSmokeColorA.Lerp(PinkSmokeColorB, amount);
		}
		else
		{
			p.BaseColor = BlueSmokeColorA.Lerp(BlueSmokeColorB, amount);
		}
		p.Acceleration = new Vector2(0f, -150f);
	}
}
