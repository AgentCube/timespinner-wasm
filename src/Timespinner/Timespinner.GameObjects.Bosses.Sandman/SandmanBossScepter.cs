using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal sealed class SandmanBossScepter : DamageArea
{
	private const int MaxLife = 100;

	private const float TimeForElectricToFadeInFadeOut = 0.5f;

	private const float RotationFrequency = 10f;

	private static readonly Color BarrierColor = Color.White * 0.5f;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandDrawHelper _sandDrawHelper;

	private readonly Appendage _tinyOrbAppendage;

	private readonly Appendage _overGlowAppendage;

	private readonly Appendage _underRaysAppendage;

	private readonly Appendage _underRaysAppendage2;

	private bool _isElectricAppendageFadingIn;

	private bool _isElectricAppendageFadingOut;

	private bool _isDrawingSand;

	private float _electricAppendageFadeTimer;

	internal bool IsActive { get; private set; }

	internal SandmanBossScepter(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		base.Power = (int)Math.Ceiling((float)baseDamage * 1.2f);
		base.DamageDimensions = new Point(24, 24);
		base.Life = 100f;
		base.DamageTimeoutTime = 0.1f;
		_canDamageThings = false;
		base.DoesKnockBack = true;
		base.AnchorOffset = new Point(0, 0);
		ChangeAnimation(-1);
		_doAppendagesInheritDrawColor = false;
		_doesUseAppendageCollision = false;
		_sandDrawHelper = new SandDrawHelper(this);
		_tinyOrbAppendage = new Appendage(this, new Point(12, 12), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 6),
			DrawPriority = 1
		};
		_tinyOrbAppendage.ChangeAnimation(21, 2, 0.05f, EAnimationType.Cycle);
		_overGlowAppendage = new Appendage(this, new Point(40, 40), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 20),
			DrawPriority = 1,
			DrawOrigin = new Vector2(20f, 20f),
			DoesDrawTrail = true,
			TrailLength = 8,
			TrailFadeRate = 4f
		};
		_overGlowAppendage.ChangeAnimation(18);
		_underRaysAppendage = new Appendage(this, new Point(32, 33), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 16),
			DrawPriority = -1,
			DrawOrigin = new Vector2(16f, 16.5f)
		};
		_underRaysAppendage.ChangeAnimation(19);
		_underRaysAppendage2 = new Appendage(this, new Point(16, 26), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 13),
			DrawPriority = -1,
			DrawOrigin = new Vector2(8f, 13f)
		};
		_underRaysAppendage2.ChangeAnimation(20);
		_appendages.Add(_tinyOrbAppendage);
		_appendages.Add(_overGlowAppendage);
		_appendages.Add(_underRaysAppendage);
		_appendages.Add(_underRaysAppendage2);
	}

	internal void Hide()
	{
		_electricAppendageFadeTimer = 0f;
		_isElectricAppendageFadingIn = false;
		_isElectricAppendageFadingOut = true;
	}

	internal void Reset(bool shouldResetID)
	{
		base.Life = 100f;
		_isFading = false;
		_fadeTimer = 0f;
		base.DrawColor = Color.Transparent;
		base.CanDamageThings = false;
		IsActive = true;
		_electricAppendageFadeTimer = 0f;
		_isElectricAppendageFadingIn = true;
		_isElectricAppendageFadingOut = false;
		ClearTrailHistory();
		if (shouldResetID)
		{
			base.ID = -1;
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && IsActive)
		{
			_sandDrawHelper.Update(delta);
			float num = 1f;
			if (_isElectricAppendageFadingIn || _isElectricAppendageFadingOut)
			{
				_electricAppendageFadeTimer += delta;
				if (_electricAppendageFadeTimer >= 0.5f)
				{
					if (_isElectricAppendageFadingIn)
					{
						_isElectricAppendageFadingIn = false;
					}
					else
					{
						IsActive = false;
						_isElectricAppendageFadingOut = false;
						num = 0f;
						SilentKill();
					}
				}
				else
				{
					float num2 = (float)Math.Sin((float)Math.PI / 2f * _electricAppendageFadeTimer / 0.5f);
					num = (_isElectricAppendageFadingOut ? (1f - num2) : num2);
				}
			}
			base.DrawColor = Color.White * num;
			_underRaysAppendage.DrawColor = base.DrawColor * 0.25f;
			_underRaysAppendage2.DrawColor = base.DrawColor * 0.25f;
			_overGlowAppendage.DrawColor = BarrierColor * num;
			_tinyOrbAppendage.DrawColor = _overGlowAppendage.DrawColor;
			float num3 = 10f * delta;
			_overGlowAppendage.Rotation += num3;
			if (_overGlowAppendage.Rotation >= (float)Math.PI * 2f)
			{
				_overGlowAppendage.Rotation -= (float)Math.PI * 2f;
			}
			_underRaysAppendage.Rotation -= num3;
			if (_underRaysAppendage.Rotation < 0f)
			{
				_underRaysAppendage.Rotation += (float)Math.PI * 2f;
			}
			_underRaysAppendage2.Rotation += num3;
			if (_underRaysAppendage2.Rotation >= (float)Math.PI * 2f)
			{
				_underRaysAppendage.Rotation -= (float)Math.PI * 2f;
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
