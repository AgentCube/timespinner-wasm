using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Demon;

internal sealed class DemonBossPlatform : GameEvent
{
	private const int LeftWallPositionX = 8;

	private const int RightWallPositionX = 392;

	private const int TopRowPositionY = 104;

	private const int BottomRowPositionY = 168;

	private const int TopColumnCount = 7;

	private const int BottomColumnCount = 9;

	private const int SegmentWidth = 16;

	private const int SegmentHeight = 8;

	private const int PreExtendWidth = 3;

	private const float TimeToPreRetract = 0.2f;

	private const float TimeToExtend = 1f;

	private const float TimeToRetract = 1f;

	private const float TimeBeforeDroppingPlayer = 0.05f;

	private const float PlayerStandingDecayRate = 0.33f;

	private readonly bool _isOnRightSide;

	private readonly bool _isOnBottomRow;

	private readonly int _platformWidth;

	private readonly int _startingX;

	private readonly int _startingY;

	private bool _isExtendingRetracting;

	private bool _isActive;

	private float _extendRetractTimer;

	private float _timeSincePlayerStoodOnUs;

	internal bool IsRetracted { get; private set; }

	internal float TimePlayerSpentStandingOnMe { get; private set; }

	public DemonBossPlatform(Level inLevel, Point inPosition, bool isOnBottomRow, bool isFacingLeft, SpriteSheet sprite, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_isOnRightSide = isFacingLeft;
		_isOnBottomRow = isOnBottomRow;
		_sprite = sprite;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		IsFacingLeft = isFacingLeft;
		base.DrawPlane = EDrawPlane.Back;
		_startingX = (_isOnRightSide ? 392 : 8);
		_startingY = (_isOnBottomRow ? 168 : 104);
		Position = new Point(_startingX, _startingY);
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.DoesCollideWithTiles = false;
		base.CannotBeGrabbed = true;
		base.IsLostWhenNotTouching = false;
		base.IsConsideredPassablePlatform = true;
		int num = 0;
		Point bboxDimensions = new Point(16, 8);
		int num2 = (_isOnBottomRow ? 9 : 7);
		for (int i = 0; i < num2; i++)
		{
			Appendage appendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(num, 0)
			};
			appendage.ChangeAnimation((i == num2 - 1) ? 5 : 6);
			num -= 16;
			base.Appendages.Add(appendage);
			_platformWidth += 16;
		}
	}

	internal void SetIsActive(bool isActive)
	{
		_isActive = isActive;
	}

	internal void SetIsSolid(bool isSolid)
	{
		_isSolid = isSolid;
	}

	internal void Extend()
	{
		if (IsRetracted)
		{
			IsRetracted = false;
			_isExtendingRetracting = true;
			_extendRetractTimer = 0f;
		}
	}

	internal void Retract()
	{
		if (!IsRetracted)
		{
			IsRetracted = true;
			_isExtendingRetracting = true;
			_extendRetractTimer = 0f;
			TimePlayerSpentStandingOnMe = 0f;
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isExtendingRetracting)
			{
				float num = 1f;
				float num2 = (IsRetracted ? 1f : 1f);
				float num3 = 0f;
				_extendRetractTimer += delta;
				if (_extendRetractTimer >= num2)
				{
					_isExtendingRetracting = false;
				}
				else if (!IsRetracted)
				{
					num = _extendRetractTimer / num2;
				}
				else if (_extendRetractTimer < 0.2f)
				{
					num = 0f;
					num3 = (0f - (float)Math.Sin((float)Math.PI * _extendRetractTimer / 0.2f)) * 3f;
				}
				else
				{
					num = (_extendRetractTimer - 0.2f) / (num2 - 0.2f);
				}
				float num4 = (float)_platformWidth * (IsRetracted ? num : (1f - num)) + num3;
				int num5 = (IsFacingLeft ? 392 : 8);
				float num6 = (float)num5 - num4 * (float)((!IsFacingLeft) ? 1 : (-1));
				base.AmountMovedLastStep = new Vector2(num6 - _floatPosition.X, 0f);
				_floatPosition = new Vector2(num6, _startingY);
				_position = new Point((int)Math.Floor(_floatPosition.X), (int)Math.Floor(_floatPosition.Y));
			}
			else
			{
				base.AmountMovedLastStep = Vector2.Zero;
			}
		}
		if (_timeSincePlayerStoodOnUs <= 0.05f)
		{
			TimePlayerSpentStandingOnMe += delta;
		}
		else if (TimePlayerSpentStandingOnMe > 0f)
		{
			TimePlayerSpentStandingOnMe -= delta * 0.33f;
			if (TimePlayerSpentStandingOnMe < 0f)
			{
				TimePlayerSpentStandingOnMe = 0f;
			}
		}
		if (_timeSincePlayerStoodOnUs < 10f)
		{
			_timeSincePlayerStoodOnUs += delta;
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (who is Protagonist protagonist && _isActive)
		{
			bool flag2 = Math.Abs(depth.Y) < Math.Abs(depth.X);
			if (!protagonist.IsIgnoringPlatforms && flag2 && protagonist.Position.Y < Bbox.Bottom && (protagonist.IsGrounded || (protagonist.LastPosition.Y <= base.OuterBbox.Top && protagonist.Velocity.Y > 0f) || _timeSincePlayerStoodOnUs < 0.05f))
			{
				flag = base.TriggerEvent(who, depth);
				if (flag)
				{
					_timeSincePlayerStoodOnUs = 0f;
					protagonist.AddMovingPlatform(this);
				}
			}
		}
		return flag;
	}
}
