using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorOrbOrbitState
{
	private const float ZOrbitTimerOffset = (float)Math.PI;

	private const float BlueTimeToGoOut = 0.75f;

	private const float BlueTimeToArc = 0.15f;

	private const float BlueTimeToReturn = 0.75f;

	internal const float BlueDelay = 0.15f;

	internal const float BlueTransitionTime = 0.25f;

	internal const float BlueDuration = 1.65f;

	internal const float BlueMeleeWindupTime = 0.5f;

	internal const float BlueMeleeWindupTransitionTime = 0.25f;

	internal const float BlueMeleeWindupTotalTime = 0.75f;

	private const int GreenThrowWidth = 432;

	private const int GreenThrowHeight = 128;

	private const float BladeTimeToGoOut = 1.25f;

	private const float BladeTimeToArc = 0.15f;

	private const float BladeTimeToReturn = 1.25f;

	internal const float BladeTransitionTime = 0.25f;

	internal const float BladeDuration = 2.65f;

	private readonly bool _isTilted;

	private readonly bool _isRotatingOnZAxis;

	private readonly EEmperorOrbOrbitType _orbOrbitType;

	private readonly int _radius;

	private readonly float _transitionTime;

	private readonly float _baseDurationTime;

	private readonly float _oscillOffset;

	private readonly Point _baseAnchorOffset;

	private bool _isAtApex;

	private bool _wasAtApex;

	private bool _isStartingNewCycle;

	private bool _wasStartingNewCycle;

	private float _timer;

	private float _transitionPercentage;

	private float _localOrbitTimer;

	private float _frequency;

	private Point _anchorOffset = Point.Zero;

	private Point _firstOrbitPosition;

	internal bool IsFinished { get; private set; }

	internal bool IsTransitioned { get; private set; }

	internal bool IsOnFrontPlane { get; set; }

	internal bool IsAtApex
	{
		get
		{
			if (_isAtApex)
			{
				return !_wasAtApex;
			}
			return false;
		}
	}

	internal bool IsStartingNewCycle
	{
		get
		{
			if (_isStartingNewCycle)
			{
				return !_wasStartingNewCycle;
			}
			return false;
		}
	}

	internal EEmperorOrbOrbitType OrbOrbitType => _orbOrbitType;

	internal float DurationTime { get; set; }

	internal float Timer => _timer;

	internal float OscillOffset => _oscillOffset;

	internal EmperorOrbOrbitState(EmperorOrbOrbitSpecification spec, float oscillOffset, int orbIndex)
	{
		_oscillOffset = oscillOffset;
		_orbOrbitType = spec.OrbOrbitType;
		_transitionTime = spec.TransitionTime;
		_baseDurationTime = spec.DurationTime;
		_radius = spec.Radius;
		_frequency = spec.Frequency;
		_baseAnchorOffset = spec.AnchorOffset;
		_isTilted = spec.IsTilted;
		_isRotatingOnZAxis = spec.IsRotatingOnZAxis;
		DurationTime = _baseDurationTime + (float)orbIndex * spec.IndexDurationMultiplier;
	}

	internal void Update(float delta)
	{
		_timer += delta;
		float localOrbitTimer = _localOrbitTimer;
		_localOrbitTimer += delta * _frequency;
		if (((double)localOrbitTimer < Math.PI && (double)_localOrbitTimer >= Math.PI) || (localOrbitTimer < (float)Math.PI * 2f && _localOrbitTimer >= (float)Math.PI * 2f))
		{
			IsOnFrontPlane = !IsOnFrontPlane;
		}
		if (_localOrbitTimer >= (float)Math.PI * 2f)
		{
			_localOrbitTimer -= (float)Math.PI * 2f;
		}
		if (_timer < _transitionTime && _transitionTime > 0f)
		{
			_transitionPercentage = _timer / _transitionTime;
			return;
		}
		_transitionPercentage = 1f;
		IsTransitioned = true;
		if (_timer - _transitionTime >= DurationTime)
		{
			IsFinished = true;
		}
	}

	internal void ResetTimers()
	{
		IsTransitioned = false;
		IsFinished = false;
		_timer = 0f;
		_isAtApex = false;
		_wasAtApex = false;
		_isStartingNewCycle = false;
		_wasStartingNewCycle = false;
	}

	internal Point GetOrbPosition(Point orbitPosition, Point anchorPosition, bool isAnchorFacingLeft)
	{
		_wasAtApex = _isAtApex;
		float num = _oscillOffset + _localOrbitTimer;
		double num2 = Math.Sin(num);
		double num3 = Math.Cos(num);
		double num4 = 0.0;
		double num5 = 0.0;
		Point point = _anchorOffset.Add(_baseAnchorOffset);
		Point a = new Point(anchorPosition.X + (isAnchorFacingLeft ? (-point.X) : point.X), anchorPosition.Y + point.Y);
		switch (_orbOrbitType)
		{
		case EEmperorOrbOrbitType.Default:
			num4 = num3 * (double)_radius;
			num5 = (_isTilted ? ((num2 * 0.25 + num3 * 0.75) * (double)_radius) : (num2 * (double)_radius));
			if (_isRotatingOnZAxis)
			{
				double num8 = Math.Cos(_oscillOffset + (float)Math.PI);
				double num9 = Math.Sin(_oscillOffset + (float)Math.PI);
				num4 *= num8;
				num5 *= num9;
			}
			break;
		case EEmperorOrbOrbitType.BlueAttack:
		case EEmperorOrbOrbitType.FireAttack:
			num4 = (num3 * (double)_radius + (double)(isAnchorFacingLeft ? (-_radius) : _radius)) * 4.5;
			num5 = num2 * (double)_radius;
			_isAtApex = Math.Abs(num4) > 135.0;
			break;
		case EEmperorOrbOrbitType.BladeAttack:
		case EEmperorOrbOrbitType.IronAttack:
		{
			if (_timer < 1.25f)
			{
				float num10 = _timer / 1.25f;
				num4 = num10 * 432f;
				num5 = _radius;
			}
			else if (_timer < 1.4f)
			{
				float num11 = (_timer - 1.25f) / 0.15f;
				num2 = Math.Sin((double)num11 * Math.PI) * (double)_radius;
				num3 = Math.Cos((double)num11 * Math.PI) * (double)_radius;
				num4 = 432.0 + num2;
				num5 = num3;
			}
			else
			{
				float num12 = (_timer - 1.4f) / 1.25f;
				num4 = (1f - num12) * 432f;
				num5 = -_radius;
			}
			if (isAnchorFacingLeft)
			{
				num4 = 0.0 - num4;
			}
			int num13 = 128 - anchorPosition.Y;
			num5 += (double)num13;
			break;
		}
		case EEmperorOrbOrbitType.EmpireAttack:
			num4 = num3 * (double)_radius * 0.10000000149011612;
			num5 = num2 * (double)_radius;
			break;
		case EEmperorOrbOrbitType.PlasmaAttack:
			if (_firstOrbitPosition == Point.Zero)
			{
				_firstOrbitPosition = orbitPosition;
			}
			num4 = _firstOrbitPosition.X - a.X;
			num5 = _firstOrbitPosition.Y - a.Y;
			break;
		case EEmperorOrbOrbitType.Death:
			_frequency = 4f + _timer * 10f;
			num4 = num3 * (double)_radius;
			num5 = (_isTilted ? ((num2 * 0.25 + num3 * 0.75) * (double)_radius) : (num2 * (double)_radius));
			if (_isRotatingOnZAxis)
			{
				double num6 = Math.Cos(_oscillOffset + (float)Math.PI);
				double num7 = Math.Sin(_oscillOffset + (float)Math.PI);
				num4 *= num6;
				num5 *= num7;
			}
			break;
		}
		Point point2 = a.Add((int)num4, (int)num5);
		if (_transitionPercentage < 1f)
		{
			point2 = orbitPosition.SineInterpolate(point2, _transitionPercentage);
		}
		return point2;
	}

	public void Cancel()
	{
		IsFinished = true;
		IsTransitioned = true;
	}
}
