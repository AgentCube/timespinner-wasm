using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events;

internal class TimespinnerAppendage : Appendage
{
	public enum ETimespinnerAppendageType
	{
		Wheel,
		Spindle,
		Gear
	}

	private const float TimeToUnhingeOut = 0.15f;

	private const float StartingUnhingeRadiusMultiplier = 1f;

	private const float FinalUnhingeRadiusMultiplier = 1.1f;

	private const float TimeToAbsorb = 1f;

	private const float TimeToChangeSize = 0.15f;

	private const float FinalGlowBase = 16f;

	private readonly ETimespinnerAppendageType _timespinnerAppendageType;

	private readonly float _rotationMultiplier = 1f;

	private readonly TheTimespinner _parentTimespinner;

	private bool _isUnhinged;

	private bool _isAbsorbing;

	private bool _isChangingSize;

	private bool _isShrinking;

	protected bool _isReverseRotation;

	protected float _wheelRotation;

	private float _unhingedTimer;

	private float _sizeChangeTimer;

	private Point _unhingedEnd;

	private Point _unhingedStart;

	private Point _unhingedDelta;

	private SoulStreamEvent _soulStream;

	public ETimespinnerAppendageType TimespinnerAppendageType => _timespinnerAppendageType;

	public TimespinnerAppendage(TheTimespinner parent, Point inBboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, ETimespinnerAppendageType inAppendageType)
		: base(parent, inBboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_parentTimespinner = parent;
		_timespinnerAppendageType = inAppendageType;
		base.DoesInheritDrawColor = false;
		base.FollowType = EAppendageFollowType.ParentObjectLocked;
		base.DrawPriority = 1;
		switch (_timespinnerAppendageType)
		{
		case ETimespinnerAppendageType.Wheel:
			ChangeAnimation(1);
			base.AnchorOffset = new Point(0, -38);
			break;
		case ETimespinnerAppendageType.Spindle:
			ChangeAnimation(2);
			base.AnchorOffset = new Point(-69, -97);
			_rotationMultiplier = 1.5f;
			break;
		case ETimespinnerAppendageType.Gear:
			ChangeAnimation(3);
			break;
		}
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		UpdateUnhinging(delta);
		UpdateChangingSize(delta);
		if (_soulStream != null)
		{
			_soulStream.Update(delta);
		}
	}

	private void UpdateChangingSize(float delta)
	{
		if (!_isChangingSize)
		{
			return;
		}
		float num = (_isShrinking ? 0.2f : 1f);
		float start = (_isShrinking ? 1f : 0.2f);
		_sizeChangeTimer += delta;
		if (_sizeChangeTimer >= 0.15f)
		{
			_scale = num;
			_isChangingSize = false;
			base.IsGlowing = _isShrinking;
			if (!_isShrinking)
			{
				base.DrawColor = Color.White;
			}
		}
		else
		{
			float num2 = _sizeChangeTimer / 0.15f;
			_scale = MathEx.SineInterpolate(start, num, num2);
			float num3 = (_isShrinking ? num2 : (1f - num2));
			base.IsGlowing = true;
			base.GlowBase = MathEx.SineInterpolate(2f, 16f, num3);
			base.GlowColor = new Color(0.9f, 0.85f, 0.7f, num3);
		}
	}

	private void UpdateUnhinging(float delta)
	{
		if (!_isUnhinged)
		{
			return;
		}
		_unhingedTimer += delta;
		if (_isAbsorbing)
		{
			if (_unhingedTimer <= 1f)
			{
				float percentage = _unhingedTimer / 1f;
				float b = MathEx.SineInterpolate(1f, 0.1f, percentage);
				Position = _unhingedEnd.Add(_unhingedDelta.Multiply(b));
			}
		}
		else if (_unhingedTimer < 0.15f)
		{
			float percentage2 = _unhingedTimer / 0.15f;
			float b2 = MathEx.SineInterpolate(1f, 1.1f, percentage2);
			Position = _unhingedEnd.Add(_unhingedDelta.Multiply(b2));
		}
		else
		{
			Position = _unhingedEnd.Add(_unhingedDelta.Multiply(1.1f));
		}
	}

	internal void IncrementRotation(float rotation)
	{
		_wheelRotation += (_isReverseRotation ? (0f - rotation) : rotation) * _rotationMultiplier;
		if (_wheelRotation > (float)Math.PI * 2f)
		{
			_wheelRotation -= (float)Math.PI * 2f;
		}
		else if (_wheelRotation < 0f)
		{
			_wheelRotation += (float)Math.PI * 2f;
		}
		base.Rotation = _wheelRotation;
	}

	internal void Unhinge()
	{
		_isUnhinged = true;
		_unhingedTimer = 0f;
		base.FollowType = EAppendageFollowType.None;
		_unhingedStart = Position;
		_unhingedEnd = _parentTimespinner.PortalCenter;
		_unhingedDelta = new Point(_unhingedStart.X - _unhingedEnd.X, _unhingedStart.Y - _unhingedEnd.Y);
	}

	internal void Absorb()
	{
		_isAbsorbing = true;
		_unhingedTimer = 0f;
		_unhingedStart = Position;
		_unhingedEnd = _parentTimespinner.PortalCenter;
		_unhingedDelta = new Point(_unhingedStart.X - _unhingedEnd.X, _unhingedStart.Y - _unhingedEnd.Y);
	}

	internal void ChangeSize(bool isShrinking)
	{
		if (!_isShrinking && !isShrinking)
		{
			_scale = 0.2f;
			base.IsGlowing = true;
			base.GlowBase = 20f;
			base.GlowColor = new Color(0.9f, 0.8f, 0.7f, 0f);
		}
		_isChangingSize = true;
		_sizeChangeTimer = 0f;
		_isShrinking = isShrinking;
	}

	internal SoulStreamEvent AddSoulStream(bool isAnchoredToPlayer)
	{
		_soulStream = new SoulStreamEvent(_level, Bbox.Center, isAnchoredToPlayer);
		return _soulStream;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_soulStream != null)
		{
			_soulStream.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}
}
