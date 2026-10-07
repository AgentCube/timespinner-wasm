using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class ForcedWaterBubbleParticleSystem : ParticleSystem
{
	private const int MinOffsetX = 0;

	private const int MaxOffsetX = 12;

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

	public ForcedWaterBubbleParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		base.DoParticleUseLifeForAlpha = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 65f;
		_minAcceleration = 100f;
		_maxAcceleration = 150f;
		_minLifetime = 0.5f;
		_maxLifetime = 5f;
		_minScale = 0.1f;
		_maxScale = 0.25f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		float num = ParticleSystem.RandomBetween(0f, 12f);
		base.InitializeParticle(p, new Vector2(where.X + num, where.Y));
		float x = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed) * 0.35f;
		float y = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed);
		p.Velocity = new Vector2(x, y);
		float num2 = ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration);
		p.Acceleration = new Vector2(0f, 0f - num2);
		p.BaseColor = BaseColor;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
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
}
