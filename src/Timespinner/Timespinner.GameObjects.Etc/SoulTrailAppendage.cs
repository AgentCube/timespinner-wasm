using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Etc;

internal sealed class SoulTrailAppendage : Appendage
{
	internal enum ESoulTrailState
	{
		Random,
		GoToPoint,
		FadeOut,
		Scatter
	}

	private const int EndOffsetY = -16;

	private const float RandomSpeed = 500f;

	private const float TimeBetweenParticleEmissions = 0.033f;

	private const float TimeBetweenVelocityChange = 0.25f;

	private const float GoToSpeed = 500f;

	private const float TimeToGoToPoint = 0.66f;

	private const float TimeToFadeOut = 0.15f;

	private const float TimeToRandom = 0.15f;

	private readonly Point _roomDimensions;

	private readonly Color _baseTrailColor;

	private readonly SoulTrailParticleSystem _trailParticles;

	private bool _isTargetOriginSpecified;

	private ESoulTrailState _soulTrailStreamerState;

	private float _stateTimer;

	private float _particleEmissionTimer;

	private float _velocityChangeTimer;

	private Point _targetOrigin;

	private Point _startingOrigin;

	private Vector2 _currentVector;

	private Vector2 _targetVector;

	internal bool IsFinished { get; private set; }

	internal bool IsDead { get; private set; }

	internal Point TargetOrigin
	{
		get
		{
			return _targetOrigin;
		}
		set
		{
			_targetOrigin = value;
			_isTargetOriginSpecified = true;
		}
	}

	public SoulTrailAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, Color trailColor, Vector4 particlesColor)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_baseTrailColor = trailColor;
		ChangeAnimation(-1);
		base.DoesDrawTrail = true;
		base.DoesInheritDrawColor = false;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 4;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 0f;
		_trailColor = _baseTrailColor;
		_trailLength = 50;
		_trailParticles = new SoulTrailParticleSystem(_level.GCM.TxParticleEnergy, 16, particlesColor);
		_particleSystems.Add(_trailParticles);
		_roomDimensions = _level.RoomSize;
		Position = parent.Bbox.Center;
		_targetOrigin = Point.Zero;
	}

	public override void Update(float delta)
	{
		_particleEmissionTimer += delta;
		if (_particleEmissionTimer >= 0.033f && _soulTrailStreamerState != ESoulTrailState.FadeOut)
		{
			_particleEmissionTimer -= 0.033f;
			_trailParticles.AddParticles(Bbox.Center.ToVector2());
		}
		UpdateStreamer(delta);
		base.Update(delta);
	}

	private void UpdateStreamer(float delta)
	{
		switch (_soulTrailStreamerState)
		{
		case ESoulTrailState.Random:
			UpdateRandom(delta);
			break;
		case ESoulTrailState.GoToPoint:
			UpdateGoToPoint(delta);
			break;
		case ESoulTrailState.FadeOut:
			UpdateFadeOut(delta);
			break;
		}
	}

	private void UpdateGoToPoint(float delta)
	{
		if (_stateTimer <= 0f)
		{
			_startingOrigin = Position;
		}
		_stateTimer += delta;
		if (!_isTargetOriginSpecified)
		{
			_targetOrigin = _level.GetPlayerPosition().Add(0, -16);
		}
		float num = _stateTimer / 0.66f;
		Point point = _startingOrigin.SineInterpolate(_targetOrigin, num);
		Vector2 vector = _currentVector * 500f * delta;
		Point point2 = new Point(Position.X + (int)vector.X, Position.Y + (int)vector.Y);
		Position = new Point((int)MathEx.CosInterpolate(point2.X, point.X, num), (int)MathEx.CosInterpolate(point2.Y, point.Y, num));
		if (_stateTimer > 0.66f)
		{
			_stateTimer = 0f;
			_soulTrailStreamerState = ESoulTrailState.FadeOut;
			IsFinished = true;
		}
	}

	private void UpdateRandom(float delta)
	{
		_velocityChangeTimer -= delta;
		if (_velocityChangeTimer <= 0f)
		{
			_velocityChangeTimer = 0.25f;
			Point point = _level.GetPlayerPosition().Add(0, -16);
			Vector2 vector = new Vector2(Position.X - point.X, Position.Y - point.Y);
			vector.Normalize();
			_targetVector = new Vector2(0f - vector.Y, 0f - Math.Abs(vector.X));
		}
		_currentVector = new Vector2(MathHelper.SmoothStep(_currentVector.X, _targetVector.X, delta * 6f), MathHelper.SmoothStep(_currentVector.Y, _targetVector.Y, delta * 6f));
		_velocity = Vector2.Multiply(_currentVector, 500f);
		if (Position.X < 0)
		{
			_targetVector = new Vector2(1f, _targetVector.Y);
		}
		else if (Position.X > _roomDimensions.X)
		{
			_targetVector = new Vector2(-1f, _targetVector.Y);
		}
		if (Position.Y < 0)
		{
			_targetVector = new Vector2(_targetVector.X, 1f);
		}
		else if (Position.Y > _roomDimensions.Y)
		{
			_targetVector = new Vector2(_targetVector.X, -1f);
		}
		_stateTimer += delta;
		if (_stateTimer > 0.15f)
		{
			_stateTimer = 0f;
			_soulTrailStreamerState = ESoulTrailState.GoToPoint;
			if (!_isTargetOriginSpecified)
			{
				_targetOrigin = _level.GetPlayerPosition().Add(0, -16);
			}
		}
	}

	private void UpdateFadeOut(float delta)
	{
		_stateTimer += delta;
		if (_stateTimer < 0.15f)
		{
			float num = _stateTimer / 0.15f;
			_trailColor = _baseTrailColor * (1f - num);
		}
		else if (_stateTimer > 2f)
		{
			IsDead = true;
		}
		else
		{
			_velocity = Vector2.Zero;
		}
	}

	internal void Reset(Point position)
	{
		_stateTimer = 0f;
		_particleEmissionTimer = 0f;
		_velocityChangeTimer = 0f;
		IsFinished = false;
		IsDead = false;
		_soulTrailStreamerState = ESoulTrailState.Random;
		_trailColor = _baseTrailColor;
		_currentVector = Vector2.Zero;
		_velocity = Vector2.Zero;
		Position = position;
	}

	internal void ForceEnd()
	{
		_soulTrailStreamerState = ESoulTrailState.FadeOut;
		_stateTimer = 0.15f;
		IsFinished = true;
		IsDead = true;
		base.TrailColor = Color.Transparent;
	}
}
