using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Platforms;

internal sealed class MovingPlatformEvent : GameEvent
{
	private const float MoveFrequency = 1.5f;

	private const float HorizontalVelocity = 100f;

	private const float TimeBeforeDroppingPlayer = 0.05f;

	private readonly Vector2 _initialVector;

	private float _moveTimer;

	private float _timeSincePlayerStoodOnUs;

	private Vector2 _lastFloatPosition;

	private Vector2 _lastAmountMoved;

	public Vector2 CurrentVector { get; private set; }

	public MovingPlatformEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpPlatforms;
		_bbox = new Rectangle(0, 0, 32, 16);
		_bboxOffset = new Point(1, 2);
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isRepeatedTrigger = true;
		IsFacingLeft = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = false;
		base.IsLostWhenNotTouching = false;
		base.IsLostWhenNotGrounded = true;
		base.IsConsideredPassablePlatform = true;
		base.CannotBeGrabbed = true;
		int argument = objectSpec.Argument;
		int num = (int)Math.Floor((float)argument * 0.5f);
		int num2 = ((argument % 2 != 1) ? 1 : (-1));
		switch (num)
		{
		case 0:
			_initialVector = new Vector2(num2, 0f);
			break;
		case 1:
			_initialVector = new Vector2(0f, num2);
			break;
		case 2:
			_initialVector = new Vector2(num2, num2);
			break;
		case 3:
			_initialVector = new Vector2(-num2, num2);
			break;
		}
		CurrentVector = _initialVector * 100f;
		ChangeAnimation(38, 4, 0.1f, EAnimationType.Cycle);
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			UpdateMovement(delta);
			base.Velocity = CurrentVector;
			_lastFloatPosition = _floatPosition;
			base.Update(delta);
			_lastAmountMoved = _floatPosition - _lastFloatPosition;
			base.AmountMovedLastStep = _lastAmountMoved;
		}
		else
		{
			UpdateIsWithinObjectVisibleArea();
		}
		if (_timeSincePlayerStoodOnUs < 10f)
		{
			_timeSincePlayerStoodOnUs += delta;
		}
	}

	private void UpdateMovement(float delta)
	{
		_moveTimer += delta * 1.5f;
		if (_moveTimer >= (float)Math.PI * 2f)
		{
			_moveTimer -= (float)Math.PI * 2f;
		}
		float num = (float)Math.Cos(_moveTimer) * 100f;
		CurrentVector = _initialVector * num;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (who is Protagonist protagonist)
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
