using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class NethershadeSwirlParticleSystem : AnimatedParticleSystem
{
	private static readonly Vector4 DarknessColor = new Vector4(0.1f, 0.05f, 0.15f, 0.35f);

	private Point _lastTetherBase;

	public NethershadeSwirlParticleSystem(SpriteSheet sprite, int howManyEffects, int animationIndex)
		: base(sprite, howManyEffects, animationIndex, 1, 1f, EAnimationType.None)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.01f;
		_maxFriction = 0.015f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 1f;
		_maxScale = 1.5f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = 3f;
		_maxRotationSpeed = 5f;
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

	public override void Update(float delta, Point tetherBase)
	{
		if (_lastTetherBase == Point.Zero)
		{
			_lastTetherBase = tetherBase;
		}
		Vector2 value = new Vector2(_lastTetherBase.X - tetherBase.X, _lastTetherBase.Y - tetherBase.Y);
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				particle.Position = Vector2.Subtract(particle.Position, value);
				particle.Update(delta);
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
		_lastTetherBase = tetherBase;
	}
}
