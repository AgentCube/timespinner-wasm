using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._10_Fortress;

internal sealed class FortressLargeSoldierSpikeDamageArea : DamageArea
{
	private const int SpikeAnimationStart = 45;

	private const int DamageWidth = 16;

	private const int DamageHeight = 36;

	private const float TimeBeforeAbleToHitPlayer = 0.05f;

	private const float TimeToRise = 0.15f;

	private const float TimeToFall = 0.15f;

	private const float SpikeRiseAnimationSpeed = 0.0375f;

	private const float SpikeFallAnimationSpeed = 0.0375f;

	private const float TimeToLinger = 0.0375f;

	private const float TimeBeforeFalling = 0.1875f;

	private const float TimeForEntireSequence = 0.3375f;

	private readonly int _spikeDamage;

	private float _riseFallTimer;

	public FortressLargeSoldierSpikeDamageArea(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_bboxOffset = new Point(4, 0);
		Bbox = new Rectangle(0, 0, 16, 36);
		_spikeDamage = (int)Math.Ceiling((float)baseDamage * 1.1f);
		base.DrawColor = Color.White * 0.85f;
		_doesDrawTrail = true;
		_trailLength = 4;
		_trailFadeRate = 2f;
		Reset(inPosition, 0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isDormant)
		{
			float riseFallTimer = _riseFallTimer;
			_riseFallTimer += delta;
			if (_riseFallTimer < 0.3375f)
			{
				base.DamageDimensions = new Point(16, 36);
				if (_riseFallTimer < 0.15f)
				{
					if (riseFallTimer <= 0f)
					{
						ChangeAnimation(45, 4, 0.0375f, EAnimationType.Once);
					}
					base.CanDamageThings = !(_riseFallTimer < 0.05f);
				}
				else if (_riseFallTimer < 0.1875f)
				{
					base.CanDamageThings = true;
					if (riseFallTimer < 0.3375f)
					{
						base.DamageDimensions = new Point(16, 36);
					}
				}
				else
				{
					if (riseFallTimer < 0.1875f)
					{
						ChangeAnimation(49, 4, 0.0375f, EAnimationType.Once);
					}
					base.CanDamageThings = false;
				}
			}
			else
			{
				ChangeAnimation(-1);
				base.CanDamageThings = false;
				Kill();
			}
		}
		base.Update(delta);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - 36);
	}

	internal void Reset(Point position, float dormantTime)
	{
		ChangeAnimation(-1);
		base.Power = _spikeDamage;
		_isFading = false;
		_fadeTimer = 0f;
		_riseFallTimer = 0f;
		base.CanDamageThings = true;
		_life = 10f;
		base.ID = -1;
		if (dormantTime > 0f)
		{
			_isDormant = true;
			_dormantTimer = dormantTime;
		}
		base.DamageDimensions = new Point(16, 0);
		Position = position;
	}
}
