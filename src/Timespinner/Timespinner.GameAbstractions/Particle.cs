using System;
using Microsoft.Xna.Framework;

namespace Timespinner.GameAbstractions;

public class Particle
{
	private const float DefaultFrameRate = 60f;

	private float _brightness = 1f;

	private Vector4 _baseCol = Vector4.One;

	public Vector2 Velocity;

	public Vector2 Acceleration;

	public float Lifetime { get; set; }

	public float TimeSinceStart { get; set; }

	public float Scale { get; set; }

	public float ScaleGrowthRate { get; set; }

	public float Friction { get; set; }

	public float Rotation { get; set; }

	public float RotationSpeed { get; set; }

	public float Brightness
	{
		get
		{
			return _brightness;
		}
		set
		{
			_brightness = value;
		}
	}

	public Vector2 Position { get; set; }

	public Vector2 InitialPosition { get; set; }

	public Vector4 BaseColor
	{
		get
		{
			return _baseCol;
		}
		set
		{
			_baseCol = value;
		}
	}

	public bool IsActive => TimeSinceStart < Lifetime;

	public virtual void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, float lifetime, float scale, float scaleGrowthRate, float rotationSpeed, float rotation, float friction)
	{
		Position = position;
		Velocity = velocity;
		Acceleration = acceleration;
		Lifetime = lifetime;
		Scale = scale;
		ScaleGrowthRate = scaleGrowthRate;
		RotationSpeed = rotationSpeed;
		InitialPosition = position;
		Friction = friction;
		Rotation = rotation;
		TimeSinceStart = 0f;
	}

	public virtual void Update(float delta)
	{
		Velocity += Acceleration * delta;
		if (Math.Abs(Friction) > 0.001f)
		{
			Velocity *= 1f - Friction * (delta * 60f);
		}
		Position += Velocity * delta;
		Rotation += RotationSpeed * delta;
		TimeSinceStart += delta;
		if (Math.Abs(ScaleGrowthRate) > 0.001f)
		{
			Scale += ScaleGrowthRate * delta;
		}
	}
}
