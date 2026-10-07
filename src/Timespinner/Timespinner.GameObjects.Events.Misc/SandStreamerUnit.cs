using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class SandStreamerUnit : Appendage
{
	private enum ESandStreamerState
	{
		None,
		Random,
		CirclePoint,
		GoToPoint,
		FadeOut,
		Scatter,
		Sleep
	}

	private const int EndOffsetY = -16;

	private const int OutsideOfScreenBuffer = 32;

	private const int ScatterOutsideRoomBuffer = 128;

	private const int AscendAbsorbCenterX = 202;

	private const int AscendAbsorbCenterY = 150;

	private const int AscendCircleRadiusMin = 92;

	private const int AscendCircleRadiusMax = 116;

	private const int AscendCircleFrequency = 2;

	private const int AscendScatterSpeed = 100;

	private const int AscendRandomSpeed = 100;

	private const float TimeForAscendAbsorb = 2f;

	private const float TimerForAscendCircleShrink = 1f;

	private const float AscendCircleShrinkMultiplier = 0.25f;

	private const float RandomSpeed = 500f;

	private const float TimeBetweenParticleEmissions = 0.033f;

	private const float TimeBetweenVelocityChange = 0.25f;

	private const float TimeToRandom = 0.75f;

	private const float GoToSpeed = 500f;

	private const float TimeToGoToPoint = 0.66f;

	private const float TimeToGoToPointSandman = 0.33f;

	private const float TimeToCircle = 2.5f;

	private const float StartingRadius = 160f;

	private const float EndingRadius = 8f;

	private const float CircleFrequency = 8f;

	private const float CircleStartingSpeed = 400f;

	private const float TimeToFadeOut = 0.25f;

	private const float TimeToScatterAccelerate = 0.5f;

	private const float MaxSleepTimeWander = 3f;

	private const float MaxSleepTimeSlowAbsorb = 3f;

	private static readonly Color SandTrailColor = new Color(0.9f, 0.5f, 0.25f, 0.5f);

	private static readonly Vector4 SandParticlesColor = new Vector4(0.8f, 0.5f, 0.25f, 0.8f);

	private static readonly Color NightmareTrailColor = new Color(0.6f, 0.8f, 0.25f, 0.5f);

	private static readonly Vector4 NightmareParticlesColor = new Vector4(0.6f, 0.8f, 0.25f, 0.8f);

	private static readonly Color AscendedTrailColor = new Color(0.9f, 0.8f, 0.5f, 0.5f);

	private static readonly Vector4 AscendedParticlesColor = new Vector4(0.8f, 0.7f, 0.5f, 0.8f);

	private static readonly Color SelenTrailColor = new Color(0.8f, 0.6f, 0.9f, 0.5f);

	private static readonly Vector4 SelenParticlesColor = new Vector4(0.7f, 0.5f, 0.8f, 0.8f);

	private readonly ESandStreamerType _streamerType;

	private readonly int _index;

	private readonly Point _roomDimensions;

	private readonly SandParticleSystem _sandParticles;

	private ESandStreamerState _sandStreamerState;

	private ESandStreamerState _duringSleepState;

	private ESandStreamerState _postSleepState;

	private EAscendStreamerPhase _ascendPhase;

	private float _stateTimer;

	private float _particleEmissionTimer;

	private float _velocityChangeTimer;

	private float _circleValue;

	private float _sleepTimer;

	private float _ascendCircleShrinkTimer;

	private Point _targetOrigin;

	private Point _startingOrigin;

	private Vector2 _currentVector;

	private Vector2 _targetVector;

	internal bool IsFinished { get; private set; }

	internal bool IsDead { get; private set; }

	public SandStreamerUnit(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, int index, ESandStreamerType streamerType)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_index = index;
		_streamerType = streamerType;
		ChangeAnimation(-1);
		base.DoesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 4;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 0f;
		_trailLength = 50;
		_roomDimensions = _level.RoomSize;
		Position = parent.Bbox.Center;
		_targetOrigin = Point.Zero;
		_sandStreamerState = ESandStreamerState.Random;
		Vector4 baseColor;
		switch (_streamerType)
		{
		case ESandStreamerType.Ascended:
			_trailColor = AscendedTrailColor;
			baseColor = AscendedParticlesColor;
			_sandStreamerState = ESandStreamerState.None;
			ResetOutsideOfRoom();
			break;
		case ESandStreamerType.NightmareDeath:
			_trailColor = NightmareTrailColor;
			baseColor = NightmareParticlesColor;
			break;
		case ESandStreamerType.SelenDeath:
			_trailColor = SelenTrailColor;
			baseColor = SelenParticlesColor;
			break;
		default:
			_trailColor = SandTrailColor;
			baseColor = SandParticlesColor;
			break;
		}
		_sandParticles = new SandParticleSystem(_level.GCM.TxParticleEnergy, 100)
		{
			BaseColor = baseColor
		};
		_particleSystems.Add(_sandParticles);
	}

	public override void Update(float delta)
	{
		_particleEmissionTimer += delta;
		if (_particleEmissionTimer >= 0.033f && _sandStreamerState != ESandStreamerState.FadeOut)
		{
			_particleEmissionTimer -= 0.033f;
			Vector2 where = new Vector2((float)(Position.X + base.LastPosition.X) / 2f, (float)(Position.Y + base.LastPosition.Y) / 2f - 4f);
			_sandParticles.AddParticles(where);
			_sandParticles.AddParticles(Bbox.Center.ToVector2());
		}
		UpdateStreamer(delta);
		base.Update(delta);
	}

	private void UpdateStreamer(float delta)
	{
		switch (_sandStreamerState)
		{
		case ESandStreamerState.Random:
			UpdateRandom(delta);
			break;
		case ESandStreamerState.CirclePoint:
			UpdateCirclePoint(delta);
			break;
		case ESandStreamerState.GoToPoint:
			UpdateGoToPoint(delta);
			break;
		case ESandStreamerState.FadeOut:
			UpdateFadeOut(delta);
			break;
		case ESandStreamerState.Scatter:
			UpdateScatter(delta);
			break;
		case ESandStreamerState.Sleep:
			UpdateSleep(delta);
			break;
		}
	}

	private void UpdateCirclePoint(float delta)
	{
		if (_stateTimer <= 0f)
		{
			float num = ((_ascendPhase == EAscendStreamerPhase.None) ? 12 : 16);
			_circleValue = (float)_index / num * ((float)Math.PI * 2f);
		}
		_stateTimer += delta;
		float num2 = _stateTimer / 2.5f;
		float num3;
		if (_ascendPhase == EAscendStreamerPhase.None)
		{
			num3 = MathEx.SineInterpolate(160f, 8f, num2);
		}
		else
		{
			num3 = ((_index % 2 == 0) ? MathEx.SineInterpolate(92f, 116f, num2) : MathEx.SineInterpolate(116f, 92f, num2));
			if (_ascendPhase == EAscendStreamerPhase.CenterCircleAbsorb)
			{
				float num4 = 0.25f;
				if (_ascendCircleShrinkTimer < 1f)
				{
					_ascendCircleShrinkTimer += delta;
					if (_ascendCircleShrinkTimer < 1f)
					{
						num4 = MathEx.SineInterpolate(1f, 0.25f, _ascendCircleShrinkTimer / 1f);
					}
				}
				num3 *= num4;
			}
		}
		if (num2 > 1f)
		{
			num2 = 1f;
		}
		_circleValue += delta * ((_ascendPhase == EAscendStreamerPhase.None) ? 8f : 2f);
		if (_circleValue > (float)Math.PI * 2f)
		{
			_circleValue -= (float)Math.PI * 2f;
		}
		double num5 = Math.Cos(_circleValue) * (double)num3;
		double num6 = Math.Sin(_circleValue) * (double)num3;
		Point end = new Point((int)((double)_targetOrigin.X + num5), (int)((double)_targetOrigin.Y + num6));
		float num7 = _targetVector.X * 400f * delta;
		float num8 = _targetVector.Y * 400f * delta;
		Point start = new Point((int)((float)Position.X + num7), (int)((float)Position.Y + num8));
		Position = start.CosInterpolate(end, num2);
		if (_stateTimer > 2.5f && _ascendPhase == EAscendStreamerPhase.None)
		{
			_stateTimer = 0f;
			_sandStreamerState = ESandStreamerState.GoToPoint;
		}
	}

	private void UpdateGoToPoint(float delta)
	{
		if (_stateTimer <= 0f)
		{
			_startingOrigin = Position;
		}
		_stateTimer += delta;
		if (_streamerType == ESandStreamerType.BossDeath || _ascendPhase != 0)
		{
			_targetOrigin = _level.GetPlayerPosition().Add(0, -16);
		}
		float num;
		switch (_ascendPhase)
		{
		case EAscendStreamerPhase.SlowAbsorb:
		case EAscendStreamerPhase.FastAbsorb:
			num = 2f;
			break;
		default:
			num = ((_streamerType == ESandStreamerType.BossDeath) ? 0.66f : 0.33f);
			break;
		}
		float num2 = _stateTimer / num;
		Point point = _startingOrigin.SineInterpolate(_targetOrigin, num2);
		Vector2 vector = _currentVector * 500f * delta;
		Point point2 = new Point(Position.X + (int)vector.X, Position.Y + (int)vector.Y);
		Position = new Point((int)MathEx.CosInterpolate(point2.X, point.X, num2), (int)MathEx.CosInterpolate(point2.Y, point.Y, num2));
		if (_streamerType == ESandStreamerType.SandmanDeath)
		{
			base.TrailColor = SandTrailColor.Lerp(NightmareTrailColor, num2);
			_sandParticles.BaseColor = SandParticlesColor.Lerp(NightmareParticlesColor, num2);
		}
		if (!(_stateTimer > num))
		{
			return;
		}
		_stateTimer = 0f;
		if (_ascendPhase == EAscendStreamerPhase.None)
		{
			_sandStreamerState = ESandStreamerState.FadeOut;
			IsFinished = true;
			return;
		}
		ResetOutsideOfRoom();
		if (_ascendPhase == EAscendStreamerPhase.SlowAbsorb)
		{
			_sandStreamerState = ESandStreamerState.Sleep;
			_postSleepState = ESandStreamerState.GoToPoint;
			_duringSleepState = ESandStreamerState.None;
			_sleepTimer = 1f;
			_velocity = Vector2.Zero;
		}
	}

	private void UpdateRandom(float delta)
	{
		_velocityChangeTimer -= delta;
		if (_velocityChangeTimer <= 0f)
		{
			_velocityChangeTimer = 0.25f;
			_targetVector = new Vector2((float)(_level.NextRandomDouble() * 2.0 - 1.0), (float)(_level.NextRandomDouble() * 2.0 - 1.0));
		}
		_currentVector = new Vector2(MathHelper.SmoothStep(_currentVector.X, _targetVector.X, delta * 6f), MathHelper.SmoothStep(_currentVector.Y, _targetVector.Y, delta * 6f));
		float scaleFactor = 500f;
		if (_ascendPhase != 0)
		{
			scaleFactor = 100f;
		}
		_velocity = Vector2.Multiply(_currentVector, scaleFactor);
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
		if (_stateTimer > 0.75f && _ascendPhase == EAscendStreamerPhase.None)
		{
			_stateTimer = 0f;
			Point roomSize = _level.RoomSize;
			switch (_streamerType)
			{
			case ESandStreamerType.HeroDeath:
			case ESandStreamerType.NightmareDeath:
			case ESandStreamerType.SelenDeath:
				_sandStreamerState = ESandStreamerState.Scatter;
				break;
			case ESandStreamerType.SandmanDeath:
				_sandStreamerState = ESandStreamerState.CirclePoint;
				_targetOrigin = new Point(roomSize.X / 2 + 1, roomSize.Y / 2);
				break;
			default:
				_sandStreamerState = ESandStreamerState.CirclePoint;
				_targetOrigin = new Point(roomSize.X / 2, roomSize.Y / 2);
				break;
			}
		}
	}

	private void UpdateScatter(float delta)
	{
		_stateTimer += delta;
		float num = ((_ascendPhase == EAscendStreamerPhase.None) ? 500f : 100f);
		_velocity = Vector2.Multiply(_currentVector, num * (1f + _stateTimer / 0.5f * 10f));
		if (_ascendPhase != 0 && IsOutsideOfRoom())
		{
			_stateTimer = 0f;
			ResetAtCenterOfScreen();
			_velocity = Vector2.Zero;
			double num2 = _level.NextRandomDouble() * 6.2831854820251465;
			_currentVector = new Vector2((float)Math.Cos(num2), (float)Math.Sin(num2));
		}
	}

	private void UpdateFadeOut(float delta)
	{
		_stateTimer += delta;
		if (_stateTimer < 0.25f)
		{
			float num = _stateTimer / 0.25f;
			Color color = ((_streamerType == ESandStreamerType.SandmanDeath) ? NightmareTrailColor : SandTrailColor);
			_trailColor = color * (1f - num);
		}
		else if (_stateTimer > 2f)
		{
			IsDead = true;
		}
	}

	private void UpdateSleep(float delta)
	{
		_sleepTimer -= delta;
		if (_sleepTimer <= 0f)
		{
			_stateTimer = 0f;
			_sandStreamerState = _postSleepState;
			return;
		}
		switch (_duringSleepState)
		{
		case ESandStreamerState.Random:
			UpdateRandom(delta);
			break;
		case ESandStreamerState.GoToPoint:
			UpdateGoToPoint(delta);
			break;
		case ESandStreamerState.CirclePoint:
			break;
		}
	}

	internal void SetPhase(EAscendStreamerPhase phase)
	{
		_ascendPhase = phase;
		switch (phase)
		{
		case EAscendStreamerPhase.EdgeWander:
			_stateTimer = 0f;
			_sandStreamerState = ESandStreamerState.Sleep;
			_postSleepState = ESandStreamerState.Random;
			_duringSleepState = ESandStreamerState.None;
			_sleepTimer = (float)(_level.NextRandomDouble() * 3.0);
			break;
		case EAscendStreamerPhase.SlowAbsorb:
			_sandStreamerState = ESandStreamerState.Sleep;
			_postSleepState = ESandStreamerState.GoToPoint;
			_duringSleepState = ESandStreamerState.Random;
			_sleepTimer = (float)(_level.NextRandomDouble() * 3.0);
			break;
		case EAscendStreamerPhase.FastAbsorb:
			_sandStreamerState = ESandStreamerState.GoToPoint;
			break;
		case EAscendStreamerPhase.CenterCircle:
			_stateTimer = 0f;
			_sandStreamerState = ESandStreamerState.CirclePoint;
			ResetAtCenterOfScreen();
			_targetOrigin = new Point(202, 150);
			break;
		case EAscendStreamerPhase.CenterCircleAbsorb:
			_ascendCircleShrinkTimer = 0f;
			_sandStreamerState = ESandStreamerState.CirclePoint;
			_targetOrigin = new Point(202, 150);
			break;
		case EAscendStreamerPhase.CenterLeave:
			_stateTimer = 0f;
			_sandStreamerState = ESandStreamerState.Scatter;
			break;
		}
	}

	private void ResetOutsideOfRoom()
	{
		bool flag = _level.NextRandomDouble() >= 0.5;
		bool flag2 = _level.NextRandomDouble() >= 0.5;
		int x;
		int y;
		if (flag)
		{
			x = (flag2 ? (-32) : (_roomDimensions.X + 32));
			y = _level.NextRandomInt(-32, _roomDimensions.Y + 32);
		}
		else
		{
			x = _level.NextRandomInt(-32, _roomDimensions.X + 32);
			y = (flag2 ? (-32) : (_roomDimensions.Y + 32));
		}
		Position = new Point(x, y);
		ClearTrailHistory();
	}

	private void ResetAtCenterOfScreen()
	{
		Position = new Point(202, 150);
		ClearTrailHistory();
	}

	private bool IsOutsideOfRoom()
	{
		if (Position.X >= -128 && Position.X <= _roomDimensions.X + 128 && Position.Y >= -128)
		{
			return Position.Y > _roomDimensions.Y + 128;
		}
		return true;
	}
}
