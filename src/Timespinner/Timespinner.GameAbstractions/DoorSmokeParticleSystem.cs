using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class DoorSmokeParticleSystem : ParticleSystem
{
	private const int EmissionHeight = 80;

	private static readonly Vector4 SmokeBaseColor = new Vector4(0.15f, 0.15f, 0.15f, 0.5f);

	private readonly bool _isBlowingLeft;

	public DoorSmokeParticleSystem(Texture2D inTexture, int howManyEffects, bool isBlowingLeft)
		: base(inTexture, howManyEffects)
	{
		_isBlowingLeft = isBlowingLeft;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 50f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 1f;
		_maxLifetime = 2.5f;
		_minScale = 0.25f;
		_maxScale = 0.75f;
		_minNumParticles = 3;
		_maxNumParticles = 5;
		_minRotationSpeed = -(float)Math.PI / 8f;
		_maxRotationSpeed = (float)Math.PI / 8f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(80f), MathHelper.ToRadians(100f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = (float)Math.Sin(num);
		if (!_isBlowingLeft)
		{
			zero.X = 0f - zero.X;
		}
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = ParticleSystem.RandomBetween(0f, -80f);
		Vector2 where2 = new Vector2(where.X, where.Y + num);
		base.InitializeParticle(p, where2);
		p.BaseColor = SmokeBaseColor;
		if (_isBlowingLeft)
		{
			p.Acceleration.X += ParticleSystem.RandomBetween(-30f, -10f);
		}
		else
		{
			p.Acceleration.X += ParticleSystem.RandomBetween(30f, 10f);
		}
	}
}
