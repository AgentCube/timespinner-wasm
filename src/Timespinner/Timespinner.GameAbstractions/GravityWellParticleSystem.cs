using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public abstract class GravityWellParticleSystem : ParticleSystem
{
	protected float _systemMass = 1f;

	protected Vector2 _lastWhere;

	private Point _lastTetherBase;

	public bool GrowsWhenNearCenter { get; set; }

	public bool DoesBrightnessChangeByDistance { get; set; }

	public float LostRadius { get; set; }

	public float MinStartRadius { get; set; }

	public float MaxStartRadius { get; set; }

	public float AbsorbRadius { get; set; }

	public float AbsorbRate { get; set; }

	public float FollowPercentage { get; set; }

	public float DistanceScaleModifier { get; set; }

	public float ProximityAgeRate { get; set; }

	public float ProximityDistance { get; set; }

	protected GravityWellParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		LostRadius = 10000f;
		AbsorbRadius = 20f;
		AbsorbRate = 10f;
		GrowsWhenNearCenter = true;
		DoesBrightnessChangeByDistance = true;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		_lastWhere = where;
		Vector2 vector = PickRandomDirection();
		float num = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed);
		float num2 = ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration);
		float lifetime = ParticleSystem.RandomBetween(_minLifetime, _maxLifetime);
		float scale = ParticleSystem.RandomBetween(_minScale, _maxScale);
		float rotationSpeed = ParticleSystem.RandomBetween(_minRotationSpeed, _maxRotationSpeed);
		float rotation = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		float scaleFactor = ParticleSystem.RandomBetween(MinStartRadius, MaxStartRadius);
		Vector2 value = Vector2.Multiply(vector, scaleFactor);
		p.Initialize(Vector2.Add(where, value), num * new Vector2(vector.Y, 0f - vector.X) * _velocityBias, num2 * vector * _accelerationBias, lifetime, scale, 0f, rotationSpeed, rotation, 0f);
		p.BaseColor = BaseColor;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Vector2 value = new Vector2(_lastTetherBase.X - tetherBase.X, _lastTetherBase.Y - tetherBase.Y);
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				ApplyGravity(particle);
				if (FollowPercentage != 0f)
				{
					particle.Position = Vector2.Subtract(particle.Position, Vector2.Multiply(value, FollowPercentage));
				}
				particle.Update(delta);
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
		_lastTetherBase = tetherBase;
	}

	private void ApplyGravity(Particle p)
	{
		Vector2 value = _lastWhere - p.Position;
		Vector2 vector = Vector2.Normalize(value);
		float num = value.LengthSquared();
		if (num > LostRadius)
		{
			vector *= 100f;
		}
		else if (num > AbsorbRadius)
		{
			vector += 1f * _systemMass / num * vector;
		}
		else
		{
			p.Velocity = Vector2.Multiply(vector, AbsorbRate);
			if (p.Acceleration.LengthSquared() > 2f)
			{
				vector = Vector2.Divide(p.Acceleration, 2f);
			}
		}
		if (num > 10f)
		{
			if (GrowsWhenNearCenter)
			{
				p.Scale = _minScale + DistanceScaleModifier * (1f / num);
			}
			else
			{
				p.Scale = _minScale + DistanceScaleModifier * num;
			}
			if (p.Scale > _maxScale)
			{
				p.Scale = _maxScale;
			}
			if (DoesBrightnessChangeByDistance)
			{
				p.Brightness = num / 100f;
			}
			else
			{
				p.Brightness = 1f;
			}
		}
		if (Math.Abs(ProximityAgeRate) > 0.01f && num < ProximityDistance && num != 0f)
		{
			AgeParticle(p, ProximityAgeRate * (1f / num));
		}
		p.Acceleration = vector;
	}
}
