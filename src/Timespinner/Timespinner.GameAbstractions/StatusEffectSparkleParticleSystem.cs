using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class StatusEffectSparkleParticleSystem : AnimatedParticleSystem
{
	private const int MaxOffsetX = 12;

	private const int MaxOffsetY = 12;

	public StatusEffectSparkleParticleSystem(SpriteSheet inSprite, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, length, 0.07f, EAnimationType.Cycle)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 30f;
		_maxInitialSpeed = 40f;
		_minAcceleration = 2f;
		_maxAcceleration = 5f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.8f;
		_minScale = 0.1f;
		_maxScale = 0.1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X = 0f;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(-12f, 12f), ParticleSystem.RandomBetween(-12f, 0f));
		base.InitializeParticle(p, Vector2.Add(where, value));
		p.BaseColor = BaseColor;
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
	}

	internal void AddParticles(Rectangle hostBbox)
	{
		Vector2 where = new Vector2(ParticleSystem.RandomBetween(hostBbox.Left, hostBbox.Right), ParticleSystem.RandomBetween(hostBbox.Top, hostBbox.Bottom));
		AddParticles(where);
	}
}
