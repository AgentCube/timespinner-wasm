using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class TimespinnerShrapnelEvent : GameEvent
{
	private const int MaxShrapnelUnits = 64;

	private const int TimespinnerRadius = 100;

	private const float TrigFrequency = 0.35f;

	private const float PulseFrequency = 2f;

	private const float PulseRadius = 0.05f;

	private const float TimeBetweenAbsorbingUnits = 0.025f;

	private const float UnitRadialMin = 0.1f;

	private const float UnitRadialMax = 1.4f;

	private const float UnitTrigMin = 0f;

	private const float UnitTrigMax = (float)Math.PI * 2f;

	private const float TimeForInitialGrowth = 0.35f;

	private const float GrowthStart = 0.5f;

	private const float GrowthEnd = 1f;

	private readonly Random _random;

	private readonly TimespinnerShrapnelUnit[] _units = new TimespinnerShrapnelUnit[64];

	private int _absorbIndex;

	private float _initialGrowthTimer;

	private float _globalRadiusMultiplier;

	private float _globalTrigValue;

	private float _pulseTimer;

	private float _absorbTimer;

	internal bool IsAbsorbingUnits { get; set; }

	public TimespinnerShrapnelEvent(Level inLevel, Point inPosition, SpriteSheet sprite)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_sprite = sprite;
		ChangeAnimation(-1);
		Bbox = new Rectangle(0, 0, 1, 1);
		SnapBboxToPosition();
		base.IsAffectedByTime = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_random = new Random(_level.NextRandomInt(0, 256));
		for (int i = 0; i < 64; i++)
		{
			float radius = RandomBetween(0.1f, 1.4f, _random);
			float trig = RandomBetween(0f, (float)Math.PI * 2f, _random);
			TimespinnerShrapnelUnit timespinnerShrapnelUnit = new TimespinnerShrapnelUnit(this, _level, _sprite, radius, trig);
			_units[i] = timespinnerShrapnelUnit;
			_appendages.Add(timespinnerShrapnelUnit);
		}
	}

	private static float RandomBetween(float min, float max, Random random)
	{
		return min + (float)random.NextDouble() * (max - min);
	}

	public override void Update(float delta)
	{
		UpdateAbsorbing(delta);
		UpdateCircling(delta);
		base.Update(delta);
	}

	private void UpdateAbsorbing(float delta)
	{
		if (IsAbsorbingUnits && _absorbIndex < 64)
		{
			_absorbTimer -= delta;
			if (_absorbTimer <= 0f)
			{
				_units[_absorbIndex].IsAbsorbing = true;
				_absorbTimer += 0.025f;
				_absorbIndex++;
			}
		}
	}

	private void UpdateCircling(float delta)
	{
		float num = 1f;
		if (_initialGrowthTimer < 0.35f)
		{
			_initialGrowthTimer += delta;
			if (_initialGrowthTimer < 0.35f)
			{
				num = MathEx.SineInterpolate(0.5f, 1f, _initialGrowthTimer / 0.35f);
			}
		}
		_globalTrigValue += 0.35f * delta;
		if (_globalTrigValue >= (float)Math.PI * 2f)
		{
			_globalTrigValue -= (float)Math.PI * 2f;
		}
		_pulseTimer += 2f * delta;
		if (_pulseTimer >= (float)Math.PI * 2f)
		{
			_pulseTimer -= (float)Math.PI * 2f;
		}
		float num2 = (float)Math.Sin(_pulseTimer) * 0.05f;
		_globalRadiusMultiplier = 1f;
		float num3 = (_globalRadiusMultiplier + num2) * num;
		TimespinnerShrapnelUnit[] units = _units;
		foreach (TimespinnerShrapnelUnit timespinnerShrapnelUnit in units)
		{
			float num4 = timespinnerShrapnelUnit.RadiusMultiplier * num3;
			float num5 = timespinnerShrapnelUnit.TrigValue + _globalTrigValue;
			float num6 = 100f * num4;
			int x = (int)Math.Ceiling(Math.Cos(num5) * (double)num6);
			int y = (int)Math.Ceiling(Math.Sin(num5) * (double)num6);
			timespinnerShrapnelUnit.AnchorOffset = new Point(x, y);
		}
	}
}
