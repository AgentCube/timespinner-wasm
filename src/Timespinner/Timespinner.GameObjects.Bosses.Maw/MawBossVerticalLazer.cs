using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Maw;

public sealed class MawBossVerticalLazer : DamageArea
{
	public const float MaxLife = 3.75f;

	private const int DamageWidth = 66;

	private const int DamageHeight = 176;

	private const int TotalTravelDistanceX = 88;

	private const float TimeBeforeLazerAppear = 1.25f;

	private const float TimeBeforeLazerTravels = 0.25f;

	private const float TimeForLazerToTravel = 2f;

	private const float TimeForLazerToLinger = 0.25f;

	private const float LazerLifeTime = 2.5f;

	private const float LazerAppearDisappearPercentage = 0.1f;

	private readonly bool _isOnCeiling;

	private readonly int _originalPower;

	private readonly int _travelDistanceX;

	private readonly float _lazerRotation;

	private readonly Vector2 _streamVector;

	private readonly ParticleSystem _streamParticles;

	private readonly MawPortalWispParticleSystem _ceilingPortalParticleSystem;

	private readonly MawPortalWispParticleSystem _floorPortalParticleSystem;

	private float _lastLifePercentage;

	private float _sleepTimer;

	private float _lazerTimer;

	public MawBossVerticalLazer(Level inLevel, Point inPosition, Vector2 inIV, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_startPosition = inPosition;
		_isOnCeiling = inIV.Y > 0f;
		_travelDistanceX = 88 * ((!_isOnCeiling) ? 1 : (-1));
		_lazerRotation = (float)Math.PI / 2f;
		_streamVector = inIV;
		_sprite = sprite;
		_doesDrawSpriteAndAppendages = false;
		base.DamageDimensions = new Point(66, 176);
		SnapBboxToPosition();
		base.DoesDrawAura = true;
		base.AuraColor = Color.White * 0.9f;
		_auraCount = 4f;
		base.AuraSize = 1.25f;
		base.AuraFrequency = 9f;
		base.DamageTimeoutTime = 0.2f;
		_originalPower = (int)Math.Ceiling((float)baseDamage * 1.2f);
		_power = _originalPower;
		_force = 4;
		_life = 3.75f;
		_canDamageThings = false;
		_sleepTimer = 1.25f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_streamParticles = new LunaisLazerStreamParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = new Vector4(0.5f, 0.9f, 0.75f, 0.9f)
		};
		_particleSystems.Add(_streamParticles);
		_doesAutomaticallyEmitParticles = false;
		_floorPortalParticleSystem = new MawPortalWispParticleSystem(_sprite, 5, 48, EDirection.North);
		_ceilingPortalParticleSystem = new MawPortalWispParticleSystem(_sprite, 5, 48, EDirection.South);
		_particleSystems.Add(_floorPortalParticleSystem);
		_particleSystems.Add(_ceilingPortalParticleSystem);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			bool flag = true;
			if (_sleepTimer > 0f)
			{
				_sleepTimer -= delta;
				base.DoesDrawAura = false;
			}
			else
			{
				_lazerTimer += delta;
				base.DoesDrawAura = true;
				if (_lazerTimer > 0.25f)
				{
					float num = (_lazerTimer - 0.25f) / 2f;
					if (num > 1f)
					{
						num = 1f;
					}
					Position = new Point(_startPosition.X + (int)(num * (float)_travelDistanceX), _startPosition.Y);
				}
				float num2 = _lazerTimer / 2.5f;
				Vector2 where = Position.ToVector2();
				_power = _originalPower;
				if (num2 < 0.1f)
				{
					flag = false;
					_power = 0;
					_bbox.Width = (int)(MathHelper.SmoothStep(0f, 1f, num2 / 0.1f) * (float)_damageDimensions.X);
					_streamParticles.KillOffParticles(0.1f);
					_ceilingPortalParticleSystem.KillOffParticles(0.1f);
					_floorPortalParticleSystem.KillOffParticles(0.1f);
					if (_lastLifePercentage > 0f && num2 < 0f)
					{
						_level.PlayCue(ESFX.BossRoboKittyLazerEnd, Position);
					}
				}
				else if (num2 > 0.9f)
				{
					float num3 = (1f - num2) / 0.1f;
					_bbox.Width = (int)(MathHelper.SmoothStep(0f, 1f, num3) * (float)_damageDimensions.X);
					if (num3 > 0.5f)
					{
						_streamParticles.AddParticles(where, _streamVector);
					}
				}
				else
				{
					_streamParticles.AddParticles(where, _streamVector);
					_bbox.Height = _damageDimensions.Y;
				}
				if (num2 <= 0.95f && !_canDamageThings)
				{
					_canDamageThings = true;
				}
				_lastLifePercentage = num2;
			}
			if (flag)
			{
				_ceilingPortalParticleSystem.AddParticles(new Vector2(Position.X, Bbox.Top));
				_floorPortalParticleSystem.AddParticles(new Vector2(Position.X, Bbox.Bottom));
			}
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - ((!_isOnCeiling) ? _bbox.Height : 0));
	}

	public override void DrawAura(SpriteBatch spriteBatch, SpriteEffects toFlip)
	{
		Color auraColor = base.AuraColor;
		Vector2 vector = new Vector2(_bbox.X + _bbox.Width / 2, _bbox.Y);
		float num = 1f;
		auraColor *= 0.5f;
		for (int i = 0; (float)i < _auraCount; i++)
		{
			num += (float)((Math.Cos((double)base.AuraFrequency * ((double)_auraTimer + 0.4 * (double)i)) + 1.0) / 20.0);
			if (num > base.AuraSize)
			{
				num = 0.9f;
			}
			auraColor *= 0.9f;
			float num2 = (float)_bbox.Width * num - (float)_bbox.Width;
			DrawLazer(spriteBatch, auraColor, new Rectangle((int)vector.X, (int)vector.Y, (int)((float)_bbox.Width + num2), _bbox.Height));
		}
	}

	private void DrawLazer(SpriteBatch spriteBatch, Color lazerColor, Rectangle drawRect)
	{
		Rectangle frameSource = _sprite.GetFrameSource(26);
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)drawRect.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)drawRect.Y)), drawRect.Height, drawRect.Width), frameSource, lazerColor, _lazerRotation, new Vector2(0f, 33f), SpriteEffects.None, 0f);
	}
}
