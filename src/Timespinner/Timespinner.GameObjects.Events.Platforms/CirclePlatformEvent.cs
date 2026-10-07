using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Platforms;

internal sealed class CirclePlatformEvent : GameEvent
{
	private const int PieceCount = 4;

	private const int GearRadius = 32;

	private const float TimeBeforeDroppingPlayer = 0.05f;

	private const float PlayerRotationVelocity = 1f;

	private const float GearRotationRate = 2.5f;

	private readonly bool _isRotatingCounterClockwise;

	private readonly int _centerX;

	private readonly int _centerY;

	private readonly Appendage[] _gearAppendages = new Appendage[4];

	private float _playerStandingPercentageX;

	private float _gearRotation;

	private float _timeSincePlayerStoodOnUs;

	public CirclePlatformEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_centerX = inPosition.X;
		_centerY = inPosition.Y - 8;
		_isRotatingCounterClockwise = objectSpec != null && objectSpec.Argument == 1;
		_sprite = _level.GCM.SpPlatforms;
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = false;
		_bbox = new Rectangle(0, 0, 64, 64);
		Position = new Point(Position.X, Position.Y + 24);
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isRepeatedTrigger = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.IsLostWhenNotTouching = false;
		base.IsLostWhenNotGrounded = true;
		base.IsConsideredPassablePlatform = true;
		Point anchorOffset = new Point(16, -32);
		Vector2 vector = new Vector2(0f, 32f);
		Vector2 vector2 = new Vector2(32f, 32f);
		for (int i = 0; i < 4; i++)
		{
			bool flag = i % 2 == 0;
			Appendage appendage = new Appendage(this, new Point(32, 32), Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				Rotation = ((i < 2) ? 0f : ((float)Math.PI)),
				DrawOrigin = (flag ? vector : vector2),
				AnchorOffset = anchorOffset,
				IsFacingLeft = flag
			};
			appendage.ChangeAnimation(42);
			_gearAppendages[i] = appendage;
			base.Appendages.Add(appendage);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_gearRotation += delta * 2.5f;
			if (_gearRotation >= (float)Math.PI * 2f)
			{
				_gearRotation -= (float)Math.PI * 2f;
			}
			float num = (_isRotatingCounterClockwise ? (0f - _gearRotation) : _gearRotation);
			for (int i = 0; i < 4; i++)
			{
				_gearAppendages[i].Rotation = num + ((i >= 2) ? ((float)Math.PI) : 0f);
			}
			float num2 = (float)Math.Ceiling(Math.Cos(_playerStandingPercentageX * ((float)Math.PI / 2f)) * 1.0);
			float num3 = (float)Math.Ceiling(Math.Sin(_playerStandingPercentageX * ((float)Math.PI / 2f)) * 1.0);
			if (_isRotatingCounterClockwise)
			{
				num2 = 0f - num2;
				num3 = 0f - num3;
			}
			base.AmountMovedLastStep = new Vector2(num2, num3);
		}
		base.Update(delta);
		if (_timeSincePlayerStoodOnUs < 10f)
		{
			_timeSincePlayerStoodOnUs += delta;
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (who is LunaisObj { IsIgnoringPlatforms: false } lunaisObj && lunaisObj.Position.Y < _centerY)
		{
			int num = who.Position.X - _centerX;
			float num2 = (float)num / 32f;
			double a = Math.Cos(num2 * ((float)Math.PI / 2f)) * 32.0;
			int num3 = _centerY - (int)Math.Ceiling(a);
			if (lunaisObj.Position.Y >= num3 - 2 && (lunaisObj.IsGrounded || (lunaisObj.LastPosition.Y <= num3 && lunaisObj.Velocity.Y > 0f) || _timeSincePlayerStoodOnUs < 0.05f))
			{
				result = true;
				int x = who.Position.X;
				float x2 = who.Velocity.X;
				float y = MathHelper.Min(who.Velocity.Y, 0f);
				who.CollisionSetPosition(new Point(x, num3), null);
				who.IsGrounded = true;
				who.Velocity = new Vector2(x2, y);
				who.IsOnSlope = true;
				who.SnapBboxToPosition();
				who.SnapFrameToBbox();
				_timeSincePlayerStoodOnUs = 0f;
				lunaisObj.AddMovingPlatform(this);
				_playerStandingPercentageX = num2;
			}
		}
		return result;
	}
}
