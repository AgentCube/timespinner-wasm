using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions.Base;

public abstract class ParticleSystem
{
	public const int AlphaBlendDrawOrder = 100;

	public const int AdditiveDrawOrder = 200;

	private static readonly Random RandomGenerator = new Random();

	private readonly int _howManyEffects;

	protected bool _isAnimatedParticleSystem;

	protected bool _isAccelerationDirectional = true;

	protected int _minNumParticles;

	protected int _maxNumParticles;

	protected float _minInitialSpeed;

	protected float _maxInitialSpeed;

	protected float _minAcceleration;

	protected float _maxAcceleration;

	protected float _minFriction;

	protected float _maxFriction;

	protected float _minRotationSpeed;

	protected float _maxRotationSpeed;

	protected float _minLifetime;

	protected float _maxLifetime;

	protected float _minScale;

	protected float _maxScale;

	protected float _minScaleGrowthRate;

	protected float _maxScaleGrowthRate;

	protected float _minAngle;

	protected float _maxAngle = (float)Math.PI * 2f;

	protected Vector2 _origin;

	protected Vector2 _velocityBias = Vector2.One;

	protected Vector2 _accelerationBias = Vector2.One;

	protected BlendState _spriteBlendMode;

	protected Texture2D _texture;

	protected Particle[] _particles;

	protected Queue<Particle> _freeParticles;

	public float MaxEmissionCounter = 0.1f;

	public Vector4 BaseColor = new Vector4(1f, 1f, 1f, 1f);

	internal bool DoParticleUseLifeForAlpha { get; set; }

	public bool AreParticlesDone => _freeParticles.Count >= _howManyEffects * _maxNumParticles;

	public int FreeParticleCount => _freeParticles.Count;

	public static Random Random => RandomGenerator;

	protected ParticleSystem(Texture2D inTexture, int howManyEffects)
	{
		_texture = inTexture;
		_howManyEffects = howManyEffects;
		_origin.X = _texture.Width / 2;
		_origin.Y = _texture.Height / 2;
		DoParticleUseLifeForAlpha = true;
		Initialize();
	}

	protected virtual void Initialize()
	{
		InitializeConstants();
		_particles = new Particle[_howManyEffects * _maxNumParticles];
		_freeParticles = new Queue<Particle>(_howManyEffects * _maxNumParticles);
		for (int i = 0; i < _particles.Length; i++)
		{
			_particles[i] = (_isAnimatedParticleSystem ? new AnimatedParticle() : new Particle());
			_freeParticles.Enqueue(_particles[i]);
		}
	}

	protected abstract void InitializeConstants();

	public virtual void AddParticles(Vector2 where)
	{
		int num = Random.Next(_minNumParticles, _maxNumParticles);
		for (int i = 0; i < num; i++)
		{
			if (_freeParticles.Count <= 0)
			{
				break;
			}
			Particle p = _freeParticles.Dequeue();
			InitializeParticle(p, where);
		}
	}

	public virtual void AddParticles(Vector2 where, Vector2 velocity)
	{
		int num = Random.Next(_minNumParticles, _maxNumParticles);
		for (int i = 0; i < num; i++)
		{
			if (_freeParticles.Count <= 0)
			{
				break;
			}
			Particle p = _freeParticles.Dequeue();
			InitializeParticle(p, where, velocity);
		}
	}

	protected virtual void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 vector = PickRandomDirection();
		float num = RandomBetween(_minInitialSpeed, _maxInitialSpeed);
		float num2 = RandomBetween(_minAcceleration, _maxAcceleration);
		float lifetime = RandomBetween(_minLifetime, _maxLifetime);
		float scale = RandomBetween(_minScale, _maxScale);
		float scaleGrowthRate = RandomBetween(_minScaleGrowthRate, _maxScaleGrowthRate);
		float rotationSpeed = RandomBetween(_minRotationSpeed, _maxRotationSpeed);
		float rotation = RandomBetween(_minAngle, _maxAngle);
		float friction = RandomBetween(_minFriction, _maxFriction);
		Vector2 vector2 = (_isAccelerationDirectional ? (num2 * vector) : (num2 * Vector2.One));
		p.Initialize(where, num * vector * _velocityBias, vector2 * _accelerationBias, lifetime, scale, scaleGrowthRate, rotationSpeed, rotation, friction);
	}

	protected virtual void InitializeParticle(Particle p, Vector2 where, Vector2 velocity)
	{
		InitializeParticle(p, where);
	}

	protected virtual Vector2 PickRandomDirection()
	{
		float num = RandomBetween(_minAngle, _maxAngle);
		return new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
	}

	protected virtual void AgeParticle(Particle p, float rate)
	{
		p.Lifetime -= rate;
	}

	public virtual void Update(float delta)
	{
		Update(delta, Point.Zero);
	}

	public virtual void Update(float delta, Point tetherBase)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				particle.Update(delta);
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, Vector2 cameraPosition, float cameraZoom)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				float num = particle.TimeSinceStart / particle.Lifetime;
				float num2 = (DoParticleUseLifeForAlpha ? (4f * num * (1f - num)) : 1f);
				Vector4 baseColor = particle.BaseColor;
				baseColor.W *= particle.Brightness;
				Color color = new Color(baseColor) * num2 * baseColor.W;
				Vector2 value = Vector2.Subtract(cameraPosition, new Vector2(particle.Position.X, particle.Position.Y));
				value = Vector2.Multiply(value, cameraZoom);
				spriteBatch.Draw(_texture, Vector2.Subtract(screenCenter, value), null, color, particle.Rotation, _origin, cameraZoom * particle.Scale, SpriteEffects.None, 0f);
			}
		}
	}

	public void KillOffParticles(float timeToDie)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				if (particle.Lifetime > timeToDie)
				{
					particle.Lifetime = timeToDie;
				}
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
	}

	protected void KillParticle(Particle p)
	{
		if (p.IsActive)
		{
			p.TimeSinceStart = 1000000f;
			_freeParticles.Enqueue(p);
		}
	}

	internal static double NextRandomDouble()
	{
		return RandomGenerator.NextDouble();
	}

	public static float RandomBetween(float min, float max)
	{
		return min + (float)RandomGenerator.NextDouble() * (max - min);
	}
}
