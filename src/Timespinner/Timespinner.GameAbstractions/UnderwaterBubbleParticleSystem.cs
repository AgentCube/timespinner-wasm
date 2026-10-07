using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions;

public class UnderwaterBubbleParticleSystem : ParticleSystem
{
	private int _waterTopY;

	internal int WaterTopY
	{
		get
		{
			return _waterTopY;
		}
		set
		{
			_waterTopY = value;
		}
	}

	public UnderwaterBubbleParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		base.DoParticleUseLifeForAlpha = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 35f;
		_minAcceleration = 5f;
		_maxAcceleration = 10f;
		_minLifetime = 10f;
		_maxLifetime = 15f;
		_minScale = 0.1f;
		_maxScale = 0.2f;
		_minNumParticles = 1;
		_maxNumParticles = 3;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Velocity = new Vector2(0f, -1f) * ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed);
		p.BaseColor = BaseColor;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				particle.Velocity.X = ParticleSystem.RandomBetween(0f - _maxInitialSpeed, _maxInitialSpeed);
				particle.Update(delta);
				if (particle.Position.Y < (float)_waterTopY)
				{
					particle.Lifetime = 0f;
				}
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
	}

	public int UpdateWaterTop(Point position, Level level)
	{
		int waterTopFromBelowWater = level.GetWaterTopFromBelowWater(position);
		_waterTopY = waterTopFromBelowWater;
		return _waterTopY;
	}
}
