using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Animations;

internal class AuraPaintBrush : Animate
{
	private const int KillThreshold = 64;

	private const float TimeToCourseChange = 1f;

	private const float TimeToColorChange = 2f;

	private static readonly Color StartColor = new Color(1f, 1f, 1f, 0.2f);

	private static readonly Color EndColor = new Color(0.15f, 0.15f, 0.9f, 0.3f);

	private readonly Point _destination;

	private readonly Vector2 _initialVector;

	private float _currentFollowTime;

	private float _courseChangeDelta;

	public bool IsDead { get; set; }

	public AuraPaintBrush(Point inPosition, Vector2 randomOffset, Point destination, Level inLevel, int inID)
		: base(inPosition, inLevel, inID)
	{
		_initialVector = Vector2.Add(randomOffset, new Vector2(150f, -200f));
		_destination = destination.Add(16, -12);
		_doesDrawBaseSprite = false;
		_sprite = _level.GCM.SpItems;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_trailColor = StartColor;
		_brushTrailSize = 10;
		_trailLength = 75;
		_trailShrinkRate = 0.00033f;
		_trailInterpolationAmount = 10;
		_trailFadeRate = 1f;
	}

	public override void Update(float delta)
	{
		if (_courseChangeDelta < 1f)
		{
			_courseChangeDelta += delta;
		}
		GoToPoint(_destination, delta);
		base.Update(delta);
	}

	private void GoToPoint(Point target, float delta)
	{
		_currentFollowTime += delta;
		if (_currentFollowTime > 314f)
		{
			_currentFollowTime -= 314f;
		}
		float scaleFactor = (float)Math.Sin(_currentFollowTime * 2f) * 200f;
		Vector2 value = new Vector2(target.X - _position.X, target.Y - _position.Y);
		if (value.LengthSquared() < 64f)
		{
			IsDead = true;
			return;
		}
		Vector2 value2 = Vector2.Normalize(value);
		Vector2 value3 = Vector2.Multiply(value2, 300f);
		value3 = Vector2.Add(value3, Vector2.Multiply(new Vector2(0f - value2.Y, value2.X), scaleFactor));
		_velocity = Vector2.Lerp(_initialVector, value3, _courseChangeDelta / 1f);
		_trailColor = Color.Lerp(StartColor, EndColor, Math.Min(_currentFollowTime / 2f, 1f));
	}
}
