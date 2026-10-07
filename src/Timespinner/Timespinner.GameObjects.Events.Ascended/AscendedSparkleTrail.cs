using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Ascended;

internal class AscendedSparkleTrail : Animate
{
	private const int Anim_SparkleLargeStart = 38;

	private const int Anim_SparkleLargeLength = 4;

	private const float Anim_SparkleSpeed = 0.1f;

	private const int SparkleOffsetX = 6;

	private const int SparkleOffsetY = 12;

	private const int MinRadius = 8;

	private const int DefaultArcRadius = 52;

	private const int DefaultArcFrequency = 12;

	private const int DefaultOriginFrequency = 1;

	private const int DefaultRotationFrequency = 1;

	private const float ScatterVelocity = 250f;

	private bool _isScattering;

	private int _arcRadius;

	private int _index;

	private float _percentageOfCircle;

	private float _arcFrequency;

	private float _rotationFrequency;

	private float _originFrequency;

	private float _arcTimer;

	private float _originTimer;

	private float _rotationTimer;

	private float _zPosition;

	private Point _origin;

	private Vector2 _effectiveIV;

	private Vector2 _arcVector;

	internal bool IsActive { get; private set; }

	internal Point Origin
	{
		get
		{
			return _origin;
		}
		set
		{
			_origin = value;
		}
	}

	internal Color BaseDrawColor { get; set; }

	public AscendedSparkleTrail(Point inPosition, Level inLevel, SpriteSheet sprite)
		: base(inPosition, inLevel, -1)
	{
		_sprite = sprite;
		ChangeAnimation(38, 4, 0.1f, EAnimationType.Cycle);
		ChangeAnimation(-1);
		Bbox = new Rectangle(0, 0, 16, 16);
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 6;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 6.6E-05f;
		_trailLength = 33;
		_isAffectedByGravity = false;
		_isFlying = true;
	}

	internal void Activate(Point position, int index, float percentage)
	{
		_index = index;
		_percentageOfCircle = percentage;
		Position = position;
		_origin = position;
		IsActive = true;
		_zPosition = 0f;
		_arcTimer = 0f;
		_rotationTimer = 0f;
		ClearTrailHistory();
		_arcTimer = (float)_index * _percentageOfCircle * ((float)Math.PI * 2f);
		_rotationTimer = 0f;
		_originTimer = _arcTimer;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isScattering)
			{
				UpdateArc(delta);
			}
			else
			{
				UpdateScatter(delta);
			}
		}
		base.Update(delta);
	}

	private void UpdateArc(float delta)
	{
		_arcRadius = 52;
		_arcFrequency = 12f;
		_originFrequency = 1f;
		_rotationFrequency = 1f;
		_effectiveIV = new Vector2(0.5f, 0.5f);
		Point a = new Point(_origin.X + 6, _origin.Y + 12);
		float num = 1f;
		if (_rotationFrequency > 0f)
		{
			_rotationTimer += delta * _rotationFrequency;
			if (_rotationTimer >= (float)Math.PI * 2f)
			{
				_rotationTimer -= (float)Math.PI * 2f;
			}
			float num2 = (float)Math.Cos(_rotationTimer);
			float num3 = (float)Math.Sin(_rotationTimer);
			float x = _effectiveIV.X * num2 - _effectiveIV.Y * num3;
			float y = _effectiveIV.X * num3 + _effectiveIV.Y * num2;
			_effectiveIV = new Vector2(x, y);
		}
		float num4 = _arcRadius;
		if (_originFrequency > 0f)
		{
			_originTimer += delta * _originFrequency;
			if (_originTimer >= (float)Math.PI * 2f)
			{
				_originTimer -= (float)Math.PI * 2f;
			}
			float num5 = (float)Math.Abs(Math.Cos(_originTimer));
			num4 = MathHelper.Clamp(num5 * (float)_arcRadius, 8f, _arcRadius);
			num = num5;
			float num6 = (float)Math.Sin(_originTimer);
			Vector2 vector = new Vector2(0f - _effectiveIV.Y, _effectiveIV.X) * num6 * _arcRadius;
			Point b = new Point((int)Math.Round(vector.X), (int)Math.Round(vector.Y));
			a = a.Add(b);
		}
		_arcTimer += delta * _arcFrequency;
		if (_arcTimer >= (float)Math.PI * 2f)
		{
			_arcTimer -= (float)Math.PI * 2f;
		}
		float num7 = (float)Math.Sin(_arcTimer);
		_arcVector = _effectiveIV * num7 * num4;
		Position = a.Add(_arcVector);
		_zPosition = (float)Math.Cos(_arcTimer);
		Color color = new Color(BaseDrawColor.R, BaseDrawColor.G, BaseDrawColor.B, (byte)(int)Math.Round((float)(int)BaseDrawColor.A * num));
		base.DrawColor = color * (0.5f + (_zPosition + 1f) / 4f);
		_trailColor = base.DrawColor;
	}

	private void UpdateScatter(float delta)
	{
		Point a = new Point(_origin.X + 6, _origin.Y + 12);
		Point a2 = a.Add(_arcVector);
		_arcTimer += delta;
		Vector2 b = _effectiveIV * (250f * _arcTimer);
		Position = a2.Add(b);
		base.DrawColor = BaseDrawColor * (0.5f + (_zPosition + 1f) / 4f);
		_trailColor = base.DrawColor;
	}

	internal void StartScatter()
	{
		_isScattering = true;
		_arcTimer = 0f;
		float num = (float)_index * _percentageOfCircle * ((float)Math.PI * 2f);
		_effectiveIV = new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
	}

	internal void Draw(SpriteBatch spritebatch, bool isAbove)
	{
		if (_zPosition >= 0f == isAbove)
		{
			Draw(spritebatch);
		}
	}
}
