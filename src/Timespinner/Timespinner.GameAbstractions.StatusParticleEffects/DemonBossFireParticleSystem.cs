using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.StatusParticleEffects;

public class DemonBossFireParticleSystem : AnimatedParticleSystem
{
	public DemonBossFireParticleSystem(SpriteSheet inSprite, int start, int length, int howManyEffects)
		: base(inSprite, howManyEffects, start, length, 0.065f, EAnimationType.Cycle)
	{
		BaseColor = Vector4.One * 0.95f;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 5f;
		_maxAcceleration = 10f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.4f;
		_minScale = 1f;
		_maxScale = 1f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween((float)Math.PI / 4f, (float)Math.PI * 3f / 4f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, where);
	}

	internal void AddParticles(Rectangle area)
	{
		Vector2 where = new Vector2(ParticleSystem.RandomBetween(area.Left, area.Right), ParticleSystem.RandomBetween(area.Top, area.Bottom));
		AddParticles(where);
	}
}
