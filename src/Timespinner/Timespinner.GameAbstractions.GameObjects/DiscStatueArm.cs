using Microsoft.Xna.Framework;
using Timespinner.Core;

namespace Timespinner.GameAbstractions.GameObjects;

internal class DiscStatueArm
{
	private readonly Appendage _baseArm;

	private float _timeToRotateTo;

	private float _rotationTimer;

	internal float TimeToRotateTo
	{
		get
		{
			return _timeToRotateTo;
		}
		set
		{
			_timeToRotateTo = value;
			_rotationTimer = 0f;
		}
	}

	internal float TargetRotation { get; set; }

	internal float StartingRotation { get; set; }

	internal float Rotation { get; set; }

	internal DiscStatueArm(Appendage baseArm)
	{
		_baseArm = baseArm;
	}

	internal void Update(float delta)
	{
		if (_rotationTimer < _timeToRotateTo)
		{
			_rotationTimer += delta;
			float num = _rotationTimer / _timeToRotateTo;
			if (num > 1f)
			{
				num = 1f;
			}
			Rotation = MathEx.SineInterpolate(StartingRotation, TargetRotation, num);
		}
		else
		{
			Rotation = TargetRotation;
		}
		_baseArm.Rotation = Rotation;
	}

	internal void Reset(float timeToReset)
	{
		SetTargetRotation(timeToReset, Rotation, 0f);
	}

	internal void SetTargetRotation(float duration, float starting, float target)
	{
		TimeToRotateTo = duration;
		StartingRotation = starting;
		TargetRotation = target;
	}

	internal void SetDrawOrigin(Vector2 newDrawOrigin)
	{
		_baseArm.DrawOrigin = newDrawOrigin;
	}
}
