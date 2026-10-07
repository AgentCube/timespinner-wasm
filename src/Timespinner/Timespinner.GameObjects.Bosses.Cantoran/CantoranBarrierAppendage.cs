using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Cantoran;

internal sealed class CantoranBarrierAppendage : Appendage
{
	private const int AnchorOffsetY = 4;

	private const float TimeForElectricToFadeInFadeOut = 0.1f;

	private const float RotationFrequency = 10f;

	private static readonly Color BarrierColor = new Color(0.525f, 0.45f, 0.3f, 0.15f);

	private readonly Appendage _overGlowAppendage;

	private readonly Appendage _underRaysAppendage;

	private readonly Appendage _underRaysAppendage2;

	private bool _isElectricAppendageFadingIn;

	private bool _isElectricAppendageFadingOut;

	private float _electricAppendageFadeTimer;

	private CantoranOrb _hostOrb;

	internal bool IsActive { get; set; }

	public CantoranBarrierAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		base.DoesDrawTrail = true;
		base.DoesDrawBrushTrail = true;
		base.BrushTrailSize = 40;
		base.TrailColor = new Color(0.8f, 0.65f, 0.3f, 0.5f);
		base.TrailLength = 8;
		base.TrailFadeRate = 2f;
		base.DrawColor = Color.White * 0.8f;
		ChangeAnimation(58, 4, 0.05f, EAnimationType.Cycle);
		_doAppendagesInheritDrawColor = false;
		_overGlowAppendage = new Appendage(this, new Point(40, 40), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 16),
			DrawPriority = 1,
			DrawOrigin = new Vector2(20f, 20f)
		};
		_overGlowAppendage.ChangeAnimation(66);
		_underRaysAppendage = new Appendage(this, new Point(39, 39), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(-1, 15),
			DrawPriority = -1,
			DrawOrigin = new Vector2(19.5f, 19.5f)
		};
		_underRaysAppendage.ChangeAnimation(67);
		_underRaysAppendage2 = new Appendage(this, new Point(39, 39), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(-1, 15),
			DrawPriority = -1,
			DrawOrigin = new Vector2(19.5f, 19.5f)
		};
		_underRaysAppendage2.ChangeAnimation(67);
		_appendages.Add(_overGlowAppendage);
		_appendages.Add(_underRaysAppendage);
		_appendages.Add(_underRaysAppendage2);
	}

	internal void Reset(Point position, CantoranOrb orb)
	{
		_hostOrb = orb;
		IsActive = true;
		_electricAppendageFadeTimer = 0f;
		_isElectricAppendageFadingIn = true;
		_isElectricAppendageFadingOut = false;
		Position = position;
		SnapBboxToPosition();
		ClearTrailHistory();
	}

	internal void Hide()
	{
		_electricAppendageFadeTimer = 0f;
		_isElectricAppendageFadingIn = false;
		_isElectricAppendageFadingOut = true;
	}

	internal void UpdateBarrier(float delta, bool isFrozen)
	{
		if (isFrozen)
		{
			delta = 0f;
		}
		if (!IsActive)
		{
			return;
		}
		float num = 1f;
		if (_isElectricAppendageFadingIn || _isElectricAppendageFadingOut)
		{
			_electricAppendageFadeTimer += delta;
			if (_electricAppendageFadeTimer >= 0.1f)
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
				}
			}
			else
			{
				float num2 = (float)Math.Sin((float)Math.PI / 2f * _electricAppendageFadeTimer / 0.1f);
				num = (_isElectricAppendageFadingOut ? (1f - num2) : num2);
			}
		}
		base.DrawColor = Color.White * num;
		_underRaysAppendage.DrawColor = base.DrawColor * 0.5f;
		_underRaysAppendage2.DrawColor = base.DrawColor * 0.5f;
		_overGlowAppendage.DrawColor = BarrierColor * num;
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
		Update(delta);
		Point position = _hostOrb.Position;
		Position = new Point(position.X, position.Y + 4);
		SnapBboxToPosition();
	}
}
