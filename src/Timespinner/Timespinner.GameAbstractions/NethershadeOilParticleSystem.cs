using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class NethershadeOilParticleSystem : AnimatedParticleSystem
{
	private const int Anim_OilStart = 1;

	private const int Anim_OilLength = 4;

	public NethershadeOilParticleSystem(SpriteSheet inSprite, int howManyEffects)
		: base(inSprite, howManyEffects, 1, 4, 0.07f, EAnimationType.Cycle)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 75f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.025f;
		_maxFriction = 0.025f;
		_minLifetime = 1f;
		_maxLifetime = 1.5f;
		_minScale = 0.75f;
		_maxScale = 1.25f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minAngle = 0f;
		_maxAngle = 6f;
		_minRotationSpeed = 5f;
		_maxRotationSpeed = 10f;
		_velocityBias = new Vector2(0.25f, 1f);
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		return new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		if (p.Velocity.Y < 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		p.BaseColor = new Vector4(1f, 1f, 1f, 0.75f);
	}
}
