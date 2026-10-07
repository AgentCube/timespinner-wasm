using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class InsectWingParticleSystem : AnimatedParticleSystem
{
	private readonly bool _isThereATileBelow;

	private readonly int _groundY;

	public InsectWingParticleSystem(Level level, SpriteSheet inSprite, Point startingPoint, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, length, 0.1f, EAnimationType.PingPong)
	{
		_groundY = level.GetFloorY(startingPoint);
		_isThereATileBelow = _groundY > -1;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 250f;
		_maxInitialSpeed = 500f;
		_minAcceleration = 300f;
		_maxAcceleration = 350f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 2.5f;
		_maxLifetime = 3.5f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 4;
		_maxNumParticles = 4;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(15f), MathHelper.ToRadians(165f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
	}

	public override void Update(float delta)
	{
		if (_isThereATileBelow)
		{
			Particle[] particles = _particles;
			foreach (Particle particle in particles)
			{
				if (particle.IsActive && particle.Position.Y >= (float)_groundY)
				{
					particle.Position = new Vector2(particle.Position.X, _groundY);
					particle.Velocity.Y = 0f;
					particle.RotationSpeed = 0f;
					if ((double)(particle.Lifetime - particle.TimeSinceStart) > 0.25)
					{
						particle.TimeSinceStart = particle.Lifetime - 0.25f;
					}
				}
			}
		}
		base.Update(delta);
	}
}
