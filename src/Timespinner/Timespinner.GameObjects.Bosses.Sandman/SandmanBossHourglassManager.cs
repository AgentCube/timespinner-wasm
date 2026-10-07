using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal class SandmanBossHourglassManager
{
	private const int Anim_TopGlass = 26;

	private const int Anim_BottomGlass = 27;

	private const int Anim_PowerSphere = 29;

	private const int HourglassMinBBox = 16;

	private const int HourglassRadius = 32;

	private const int MinRadius = 4;

	private const int MaxRadius = 6;

	private const float RadiusSpeed = 2f;

	private const float TimeBetweenChargeParticleEmission = 0.07f;

	private const float BaseRotationSpeed = 3f;

	private readonly SandmanPowerChargeParticleSystem _chargeParticles;

	private readonly GlowTexture _glowTexture;

	private readonly SandSwirlEvent _sandSwirl;

	private readonly Appendage _topGlass;

	private readonly Appendage _bottomGlass;

	private readonly Appendage _powerSphere;

	private readonly Appendage _collisionAppendage;

	private readonly Animate _parent;

	private bool _isRotatingToTargetAngle;

	private float _rotation;

	private float _startingRotation;

	private float _targetRotation;

	private float _rotationTimer;

	private float _timeToRotate;

	private float _radiusTimer;

	private float _radius;

	private float _particleEmissionTimer;

	private Point _parentPosition;

	internal float RotationSpeedMultiplier { get; set; }

	internal ParticleSystem ChargeParticles => _chargeParticles;

	internal GlowTexture GlowTexture => _glowTexture;

	internal SandSwirlEvent SandSwirl => _sandSwirl;

	internal Appendage TopGlass => _topGlass;

	internal Appendage BottomGlass => _bottomGlass;

	internal Appendage PowerSphere => _powerSphere;

	internal Appendage CollisionAppendage => _collisionAppendage;

	internal SandmanBossHourglassManager(Animate parent, SpriteSheet sprite)
	{
		_parent = parent;
		Level level = parent.Level;
		Point bboxDimensions = new Point(1, 1);
		Point inBboxOffset = new Point(14, 32);
		Vector2 drawOrigin = new Vector2(14.5f, 32f);
		RotationSpeedMultiplier = 1f;
		_topGlass = new Appendage(parent, bboxDimensions, inBboxOffset, level, sprite)
		{
			DrawOrigin = drawOrigin,
			FollowType = EAppendageFollowType.None,
			DoesCollideWithAnything = false,
			DrawPriority = -1
		};
		_topGlass.ChangeAnimation(26);
		_bottomGlass = new Appendage(parent, bboxDimensions, inBboxOffset, level, sprite)
		{
			DrawOrigin = drawOrigin,
			FollowType = EAppendageFollowType.None,
			DoesCollideWithAnything = false,
			DrawPriority = -1
		};
		_bottomGlass.ChangeAnimation(27);
		_powerSphere = new Appendage(parent, new Point(16, 16), Point.Zero, level, sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = parent,
			DoesInheritDrawColor = false,
			DrawColor = new Color(0.5f, 0.4f, 0.2f, 0.15f),
			DrawPriority = 1
		};
		_powerSphere.ChangeAnimation(29, 4, 0.066f, EAnimationType.Cycle);
		_collisionAppendage = new Appendage(parent, new Point(32, 32), Point.Zero, level, sprite)
		{
			DoesCollideWithAnything = true,
			DrawPriority = -1
		};
		_collisionAppendage.ChangeAnimation(-1);
		_chargeParticles = new SandmanPowerChargeParticleSystem(level.GCM.TxParticleEnergy, 16);
		_glowTexture = new GlowTexture(level)
		{
			BaseColor = new Color(0.6f, 0.35f, 0.2f),
			GlowCircleRadius = 48,
			GlowColorMultiplier = 0.1f
		};
		_sandSwirl = new SandSwirlEvent(level, parent.Bbox.Center, parent);
	}

	internal void Update(float delta)
	{
		UpdateRotating(delta);
		UpdateRadius(delta);
		UpdatePosition();
		UpdateParticles(delta);
		UpdateGlowTexture(delta);
		_sandSwirl.Update(delta);
	}

	private void UpdateRotating(float delta)
	{
		if (_isRotatingToTargetAngle)
		{
			if (_rotationTimer >= _timeToRotate)
			{
				_rotation = _targetRotation;
				return;
			}
			_rotationTimer += delta;
			if (_rotationTimer < _timeToRotate)
			{
				float percentage = _rotationTimer / _timeToRotate;
				_rotation = MathEx.SineInterpolate(_startingRotation, _targetRotation, percentage);
			}
			else
			{
				_rotation = _targetRotation;
			}
		}
		else
		{
			_rotationTimer += delta * 3f * RotationSpeedMultiplier;
			if (_rotationTimer >= (float)Math.PI * 2f)
			{
				_rotationTimer -= (float)Math.PI * 2f;
			}
			_rotation = _startingRotation + _rotationTimer;
		}
	}

	private void UpdateRadius(float delta)
	{
		_radiusTimer += delta * 2f;
		if (_radiusTimer >= 4f)
		{
			_radiusTimer -= 4f;
		}
		_radius = MathEx.SineInterpolate(4f, 6f, _radiusTimer);
	}

	private void UpdatePosition()
	{
		_parentPosition = _parent.Bbox.Center;
		_topGlass.Rotation = _rotation;
		_bottomGlass.Rotation = _rotation + (float)Math.PI;
		double num = Math.Sin(_rotation);
		double num2 = Math.Cos(_rotation);
		int num3 = (int)Math.Round(num * (double)_radius);
		int num4 = (int)Math.Round(num2 * (double)_radius);
		_topGlass.Position = new Point(_parentPosition.X + num3, _parentPosition.Y - num4);
		_bottomGlass.Position = new Point(_parentPosition.X - num3, _parentPosition.Y + num4);
		int x = 16 + (int)Math.Abs(Math.Round(num * 32.0));
		int num5 = 16 + (int)Math.Abs(Math.Round(num2 * 32.0));
		_collisionAppendage.ChangeBboxDimensions(new Point(x, num5), Point.Zero);
		_collisionAppendage.Position = new Point(_parentPosition.X, _parentPosition.Y + num5 / 2);
	}

	private void UpdateParticles(float delta)
	{
		_particleEmissionTimer -= delta;
		if (_particleEmissionTimer <= 0f)
		{
			_particleEmissionTimer += 0.07f;
			_chargeParticles.AddParticles(_parentPosition.ToVector2());
			_chargeParticles.BaseColor = new Vector4(0.5f, 0.3f, 0.2f, 0.75f);
		}
	}

	private void UpdateGlowTexture(float delta)
	{
		_glowTexture.Center = _parentPosition;
		_glowTexture.Update(delta);
	}

	internal void RotateToAngle(float angle, float timeToRotate)
	{
		_isRotatingToTargetAngle = true;
		_targetRotation = angle;
		_timeToRotate = timeToRotate;
		_rotationTimer = 0f;
		_startingRotation = _rotation;
	}

	internal void ResumeNormalRotation()
	{
		if (_isRotatingToTargetAngle)
		{
			_isRotatingToTargetAngle = false;
			_startingRotation = _rotation;
			_rotationTimer = 0f;
		}
	}
}
