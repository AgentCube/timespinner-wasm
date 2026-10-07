using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class ConveyorBeltFloorEvent : GameEvent
{
	public enum BeltMovementType
	{
		None,
		Right,
		Left
	}

	private const int HalfWidth = 8;

	private const int TileWidth = 16;

	private const int BboxHeight = 17;

	private const float BeltSpeed = 80f;

	private const float BeltGravity = 120f;

	private bool _hasUpdated;

	private int _left;

	private int _right;

	public Vector2 CurrentVector = Vector2.Zero;

	public ConveyorBeltFloorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.EventType = EEventTileType.ConveyorBelt;
		_sprite = _level.GCM.SpMiscLab;
		_bbox = new Rectangle(0, 0, 16, 17);
		_bboxOffset = new Point(0, -1);
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = true;
		_isAffectedByTime = true;
		base.CanBeTriggeredByFamiliar = true;
		base.IsTriggerableByMonsters = true;
		CurrentVector = new Vector2(objectSpec.IsFlippedHorizontally ? (-80f) : 80f, 120f);
		ChangeAnimation(0, 3, 0.1f, EAnimationType.Cycle);
		base.DrawPlane = EDrawPlane.Front;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_left = inPosition.X - 8;
		_right = inPosition.X + 8;
		RefreshDimensions();
	}

	private void RefreshDimensions()
	{
		int num = _right - _left;
		if (num > 0)
		{
			int x = _left + num / 2;
			Bbox = new Rectangle(x, Position.Y, num, 17);
			Position = new Point(x, Position.Y);
			SnapBboxToPosition();
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		Vector2 vector = depth;
		if (who.IsAffectedByGravity)
		{
			Rectangle rectA = new Rectangle(who.Position.X - 8, who.Position.Y - 16, 16, 16);
			vector = rectA.GetIntersectionDepth(Bbox);
			if (vector != Vector2.Zero)
			{
				who.AddMovingPlatform(this);
			}
		}
		return base.TriggerEvent(who, vector);
	}

	public override void Update(float delta)
	{
		bool flag = false;
		if (!_isFrozen && !_level.IsPowerOff)
		{
			_hasUpdated = true;
			base.Update(delta);
			base.AmountMovedLastStep = new Vector2(CurrentVector.X * delta, CurrentVector.Y * delta);
			flag = true;
		}
		else if (!_hasUpdated)
		{
			_hasUpdated = true;
			base.Update(0f);
			flag = true;
		}
		if (!flag)
		{
			UpdateIsWithinObjectVisibleArea();
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (!base.IsWithinObjectVisibleArea)
		{
			return;
		}
		Rectangle visibleArea = _level.VisibleArea16;
		int num = visibleArea.Left - 1;
		int num2 = visibleArea.Right + 1;
		for (int i = num; i <= num2; i++)
		{
			int num3 = i * 16;
			if (num3 > _left && num3 < _right)
			{
				Vector2 value = CameraizePoint(new Vector2(num3, Position.Y - 16));
				SpriteEffects spriteEffects = _spriteEffects;
				if (!IsImageFacingLeft)
				{
					spriteEffects = SpriteEffects.FlipHorizontally | spriteEffects;
				}
				DrawBaseSprite(spriteBatch, _sprite, Vector2.Subtract(_level.LevelRenderCenter, value), _frameSource, base.DrawColor, base.Rotation, DrawOrigin, _scale, spriteEffects, 0f);
			}
		}
	}

	internal void FlipHorizontalDirection()
	{
		CurrentVector = new Vector2(0f - CurrentVector.X, CurrentVector.Y);
		IsFacingLeft = !IsFacingLeft;
	}

	internal void AddUnit(Point position)
	{
		int num = position.X - 8;
		int num2 = position.X + 8;
		bool flag = false;
		if (num < _left)
		{
			flag = true;
			_left = num;
		}
		if (num2 > _right)
		{
			flag = true;
			_right = num2;
		}
		if (flag)
		{
			RefreshDimensions();
		}
	}
}
