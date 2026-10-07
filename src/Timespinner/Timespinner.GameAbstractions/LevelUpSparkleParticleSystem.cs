using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class LevelUpSparkleParticleSystem : AnimatedParticleSystem
{
	private const int EmissionWidth = 40;

	private const int EmissionHeight = 8;

	private int _rightEmissionCount;

	private int _leftEmissionCount;

	public LevelUpSparkleParticleSystem(SpriteSheet inSprite, int howManyEffects, Color color)
		: base(inSprite, howManyEffects, 9, 8, 0.1f, EAnimationType.Once)
	{
		BaseColor = color.ToVector4();
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 125f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.05f;
		_maxFriction = 0.05f;
		_minLifetime = 0.9f;
		_maxLifetime = 0.9f;
		_minScale = 1f;
		_maxScale = 1f;
		_minNumParticles = 12;
		_maxNumParticles = 12;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_minAngle = 0f;
		_maxAngle = 0f;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int inGameZoom = Constants.InGameZoom;
		float num = ParticleSystem.RandomBetween(-(float)Math.PI / 3f, (float)Math.PI / 3f);
		if (_leftEmissionCount < _rightEmissionCount)
		{
			num += (float)Math.PI;
			_leftEmissionCount++;
		}
		else
		{
			_rightEmissionCount++;
		}
		Vector2 value = new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
		Vector2 value2 = Vector2.Multiply(value, new Vector2(40 * inGameZoom, 8 * inGameZoom));
		base.InitializeParticle(p, Vector2.Add(where, value2));
		float scaleFactor = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed) * (float)inGameZoom;
		p.Velocity = Vector2.Multiply(value, scaleFactor);
		p.Acceleration *= (float)inGameZoom;
		p.Scale *= inGameZoom;
		p.BaseColor = BaseColor;
	}
}
