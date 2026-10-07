using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Bird;

internal sealed class GodBirdAuraDangerZone : GameEvent
{
	private const int ZoneAnimationIndex = 19;

	internal const int BoxWidth = 48;

	private const int BoxHeight = 208;

	private const int BboxOffsetX = 8;

	private const int SlamOffsetX = 48;

	private const int BrightVolumeMaxWidth = 56;

	private const int BaseVolumeWidth = 64;

	private const float TimeForBrightVolumeToAppear = 1f;

	private const float ParticleEmissionTime = 0.15f;

	private const float TimeToFadeIn = 0.5f;

	private const float TimeToFadeOut = 0.25f;

	private static readonly Color BaseVolumeColor = new Color(0.05f, 0.05f, 0.2f, 0.1f);

	private static readonly Color BaseBrightVolumeColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);

	private readonly int _baseDamage;

	private readonly Point _originalPosition;

	private readonly Vector2 _particleEmissionPosition;

	private readonly Rectangle _zoneFrameSource;

	private readonly BirdBossWindyParticleSystem _windyParticles;

	private readonly BirdBossChargeGustParticleSystem _chargeParticles;

	private readonly BirdBossWallPebblesParticleSystem _pebbleParticles;

	private readonly HashSet<Alive> _targets = new HashSet<Alive>();

	private bool _isActive;

	private int _brightVolumeWidth;

	private float _particleEmissionTimer;

	private float _fadeInOutTimer;

	private float _brightVolumeTimer;

	private Color _volumeDrawColor;

	private Color _brightVolumeDrawColor;

	internal bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			IsActiveChanged(value);
		}
	}

	public GodBirdAuraDangerZone(Level inLevel, Point inPosition, ObjectTileSpecification objectSpec, SpriteSheet sprite, int baseDamage, BirdBossWallPebblesParticleSystem pebbleParticles)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_pebbleParticles = pebbleParticles;
		_doesDrawBaseSprite = false;
		_sprite = sprite;
		_zoneFrameSource = _sprite.GetFrameSource(19);
		_originalPosition = inPosition;
		_baseDamage = baseDamage;
		_bboxOffset = new Point(8, 0);
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 48, 208);
		_particleEmissionPosition = Position.ToVector2();
		base.IsLostWhenNotTouching = true;
		base.IsLostWhenNotGrounded = false;
		_isAffectedByTime = true;
		base.CanBeTriggered = false;
		_windyParticles = new BirdBossWindyParticleSystem(_level.GCM.SpAnimatedParticlesSmall, 20, 64);
		_particleSystems.Add(_windyParticles);
		_chargeParticles = new BirdBossChargeGustParticleSystem(_sprite, 3);
		_particleSystems.Add(_chargeParticles);
	}

	private void IsActiveChanged(bool newValue)
	{
		if (_isActive != newValue)
		{
			_isActive = newValue;
			_fadeInOutTimer = (_isActive ? 0.5f : 0.25f);
			Position = _originalPosition;
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && base.CanBeTriggered && IsActive)
		{
			if (!_targets.Contains(who))
			{
				_targets.Add(who);
			}
			return base.TriggerEvent(who, depth);
		}
		return false;
	}

	public override void Update(float delta)
	{
		foreach (Alive target in _targets)
		{
			if (!base.TriggerBbox.Contains(target.Bbox.Center))
			{
				_targets.Remove(target);
				break;
			}
		}
		if (!base.IsFrozen)
		{
			if (IsActive)
			{
				_particleEmissionTimer += delta;
				if (_particleEmissionTimer >= 0.15f)
				{
					_particleEmissionTimer -= 0.15f;
					_windyParticles.AddParticles(_particleEmissionPosition);
					_chargeParticles.AddParticles(_particleEmissionPosition);
				}
			}
			UpdateFadeInOut(delta);
		}
		base.Update(delta);
	}

	private void UpdateFadeInOut(float delta)
	{
		if (_fadeInOutTimer > 0f)
		{
			_brightVolumeWidth = 0;
			_fadeInOutTimer -= delta;
			if (_fadeInOutTimer <= 0f)
			{
				_brightVolumeTimer = 0f;
				_fadeInOutTimer = 0f;
				_volumeDrawColor = (IsActive ? BaseVolumeColor : Color.Transparent);
				_brightVolumeDrawColor = (IsActive ? BaseBrightVolumeColor : Color.Transparent);
			}
			else
			{
				float num = (IsActive ? 0.5f : 0.25f);
				float num2 = _fadeInOutTimer / num;
				if (IsActive)
				{
					num2 = 1f - num2;
				}
				num2 = (float)Math.Sin(num2 * ((float)Math.PI / 2f));
				_volumeDrawColor = BaseVolumeColor * num2;
				_brightVolumeDrawColor = BaseBrightVolumeColor * num2;
				if (!IsActive)
				{
					Position = new Point(_originalPosition.X + (int)(48f * (1f - num2)), _originalPosition.Y);
				}
			}
		}
		if (IsActive && _fadeInOutTimer <= 0f)
		{
			_brightVolumeTimer += delta;
			if (_brightVolumeTimer < 1f)
			{
				double num3 = Math.Sin((float)Math.PI / 2f * _brightVolumeTimer / 1f);
				_brightVolumeWidth = (int)(num3 * 56.0);
			}
			else
			{
				_brightVolumeWidth = 56;
			}
		}
	}

	internal void Slam()
	{
		foreach (Alive target in _targets)
		{
			target.AddScriptAction(new GodBirdGustStunScript(_level, _baseDamage, _pebbleParticles));
		}
	}

	internal bool ContainsHeroPosition(Point playerPosition)
	{
		return Bbox.Contains(playerPosition);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (IsActive || _fadeInOutTimer > 0f)
		{
			if (_brightVolumeWidth > 0)
			{
				int num = Position.X - _brightVolumeWidth / 2;
				spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)num)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y)), _brightVolumeWidth, _bbox.Height), _zoneFrameSource, _brightVolumeDrawColor);
			}
			int num2 = Position.X - 32;
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)num2)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y)), 64, _bbox.Height), _zoneFrameSource, _volumeDrawColor);
		}
		base.Draw(spriteBatch);
	}
}
