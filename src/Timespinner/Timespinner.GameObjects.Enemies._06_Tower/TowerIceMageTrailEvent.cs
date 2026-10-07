using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._06_Tower;

internal sealed class TowerIceMageTrailEvent : GameEvent
{
	private const int ExtrapolateLength = 12;

	private const int MaxWidth = 56;

	private const int MaxLengthSquared = 3136;

	private const float TimeBeforeDroppingPlayer = 0.05f;

	private readonly Point _startPoint;

	private readonly Point _parentCenterOffset;

	private readonly Vector2 _iV;

	private bool _isGoingUp;

	private bool _isGoingRight;

	private bool _isAtMaxLength;

	private float _timeSincePlayerStoodOnUs;

	private Point _endPoint;

	public TowerIceMageTrailEvent(Level inLevel, Point inPosition, Vector2 iV, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_startPoint = inPosition;
		_iV = iV;
		_parentCenterOffset = (_iV * 12f).ToPoint();
		base.DoesDrawBaseSprite = false;
		_sprite = sprite;
		ChangeAnimation(-1);
		_endPoint = inPosition;
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.IsLostWhenNotTouching = false;
		base.IsConsideredPassablePlatform = true;
	}

	public override void Update(float delta)
	{
		if (_timeSincePlayerStoodOnUs < 10f)
		{
			_timeSincePlayerStoodOnUs += delta;
		}
		base.Update(delta);
	}

	internal void UpdateParentProjectile(TowerIceMageProjectile parentIce)
	{
		_endPoint = parentIce.Bbox.Center.Add(_parentCenterOffset);
		if (!_isAtMaxLength)
		{
			int num = _endPoint.X - _startPoint.X;
			int num2 = _endPoint.Y - _startPoint.Y;
			int num3 = Math.Abs(num);
			int num4 = Math.Abs(num2);
			if (num3 * num3 + num4 * num4 >= 3136)
			{
				_isAtMaxLength = true;
				num3 = (int)Math.Ceiling(Math.Abs(_iV.X) * 56f);
				num4 = (int)Math.Ceiling(Math.Abs(_iV.Y) * 56f);
				Bbox = new Rectangle(Bbox.Left, Bbox.Top, num3, num4);
			}
			else
			{
				if (num3 < 0)
				{
					num3 = 1;
				}
				if (num4 < 0)
				{
					num4 = 1;
				}
				_isGoingRight = num > 0;
				_isGoingUp = num2 < 0;
				Bbox = new Rectangle(_startPoint.X + ((!_isGoingRight) ? (-num3) : 0), _startPoint.Y + ((!_isGoingUp) ? (-num4) : 0), num3, num4);
				Position = new Point(_startPoint.X + (int)((_isGoingRight ? 0.5f : (-0.5f)) * (float)num3), _startPoint.Y + ((!_isGoingUp) ? num4 : 0));
			}
		}
		if (_isAtMaxLength)
		{
			int x = ((!_isGoingRight) ? (_endPoint.X + Bbox.Width / 2) : (_endPoint.X - Bbox.Width / 2));
			int y = ((!_isGoingUp) ? _endPoint.Y : (_endPoint.Y + Bbox.Height));
			Position = new Point(x, y);
		}
		SnapBboxToPosition();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (base.IsFrozen)
		{
			return CollideDynamicRamp(who);
		}
		return false;
	}

	private bool CollideDynamicRamp(Mobile who)
	{
		bool result = false;
		int num = who.Position.X - Bbox.Left;
		int width = Bbox.Width;
		int height = Bbox.Height;
		if (num > 0 && num < width)
		{
			float num2 = (float)num / (float)width;
			if ((!_isGoingUp && _isGoingRight) || (_isGoingUp && !_isGoingRight))
			{
				num2 = 1f - num2;
			}
			int num3 = Position.Y - (int)(num2 * (float)height);
			if (!who.IsIgnoringPlatforms && who.Position.Y > num3 && who.Bbox.Top < num3 && (who.IsGrounded || (who.LastPosition.Y <= num3 && who.Velocity.Y > 0f) || _timeSincePlayerStoodOnUs < 0.05f))
			{
				_timeSincePlayerStoodOnUs = 0f;
				int x = who.Position.X;
				float x2 = who.Velocity.X;
				float num4 = MathHelper.Min(who.Velocity.Y, 0f);
				num4 += 100f;
				who.CollisionSetPosition(new Point(x, num3), null);
				who.IsGrounded = true;
				who.Velocity = new Vector2(x2, num4);
				who.AddMovingPlatform(this);
				who.SnapBboxToPosition();
				who.SnapFrameToBbox();
				result = true;
			}
		}
		return result;
	}

	public void ParentIsDying()
	{
		SilentKill();
	}
}
