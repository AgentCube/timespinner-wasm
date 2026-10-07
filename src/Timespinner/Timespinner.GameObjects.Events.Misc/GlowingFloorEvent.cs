using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal class GlowingFloorEvent : GameEvent
{
	private const int Anim_LightPillarIndex = 46;

	private const float TimeBetweenParticles = 0.05f;

	private const float TimeToFadeIn = 0.5f;

	private const float TimeToFadeOut = 0.5f;

	private const float LightFrequency = 3f;

	private const float LightAmplitude = 0.15f;

	private const float LightBaseValue = 0.85f;

	private readonly Vector2 _particleEmissionPoint;

	private readonly Color _lightDrawColor;

	private readonly FloorSparkleParticleSystem _floorSparkles;

	private bool _isFadingOut;

	private float _particleTimer;

	private float _fadeInTimer;

	private float _lightTimer;

	private float _fadeOutTimer;

	public GlowingFloorEvent(Level inLevel, Point inPosition, Color lightDrawColor, Vector4 sparklesColor)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_lightDrawColor = lightDrawColor;
		_floorSparkles = new FloorSparkleParticleSystem(_level.GCM.TxParticleEnergy, 16)
		{
			BaseColor = sparklesColor
		};
		_particleSystems.Add(_floorSparkles);
		_particleEmissionPoint = inPosition.ToVector2();
		Bbox = new Rectangle(0, 0, 48, 48);
		_isSolid = false;
		base.CanBeTriggered = false;
		_isRepeatedTrigger = false;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = false;
		_doAppendagesMatchImageFacing = false;
		_bboxOffset = new Point(-3, 0);
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(46);
		_doesDrawBaseSprite = true;
	}

	internal void FadeOut()
	{
		_isFadingOut = true;
		_fadeOutTimer = 0.5f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isFadingOut)
			{
				_particleTimer -= delta;
				if (_particleTimer <= 0f)
				{
					_particleTimer += 0.05f;
					_floorSparkles.AddParticles(_particleEmissionPoint);
				}
			}
			_lightTimer += delta * 3f;
			if (_lightTimer >= (float)Math.PI * 2f)
			{
				_lightTimer -= (float)Math.PI * 2f;
			}
			float num = 0.85f + (float)Math.Sin(_lightTimer) * 0.15f;
			if (_fadeInTimer < 0.5f)
			{
				_fadeInTimer += delta;
				if (_fadeInTimer < 0.5f)
				{
					num *= _fadeInTimer / 0.5f;
				}
			}
			if (_isFadingOut)
			{
				if (_fadeOutTimer > 0f)
				{
					_fadeOutTimer -= delta;
					num = ((!(_fadeOutTimer > 0f)) ? 0f : (num * (_fadeOutTimer / 0.5f)));
				}
				else
				{
					num = 0f;
				}
			}
			base.DrawColor = _lightDrawColor * num;
		}
		base.Update(delta);
	}
}
