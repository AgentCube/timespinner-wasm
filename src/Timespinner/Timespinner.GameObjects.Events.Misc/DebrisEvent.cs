using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class DebrisEvent : GameEvent
{
	internal enum EDebrisDeathType
	{
		None,
		Fire,
		Dust,
		HeavyLand
	}

	private const int MinHorizontal = 5;

	private const int HorizontalMultiplier = 20;

	private const int HeightDifferenceThreshold = 24;

	private const int Uplift = -100;

	private const int HorizontalJitterMultiplier = 5;

	private const int RotationJitterMultiplier = 2;

	private const int MaxRotationSpeed = 25;

	private const int HeavyLandDustIntervalX = 12;

	private const float RotationSpeedMultiplier = 0.5f;

	private const float MinTimeBeforeDyingOnGround = 0.1f;

	private float _rotationSpeed;

	private float _timeSinceConception;

	internal EDebrisDeathType DeathType { get; set; }

	internal float GravityAcceleration
	{
		get
		{
			return _gravityAcceleration;
		}
		set
		{
			_gravityAcceleration = value;
		}
	}

	internal DebrisEvent(Animate inObject, SpriteSheet sprite, int inID, ObjectTileSpecification objectSpec)
		: base(inObject.Level, inObject.Position, inID, objectSpec)
	{
		_sprite = sprite;
		BboxOffset = inObject.BboxOffset;
		Bbox = new Rectangle(0, 0, inObject.Bbox.Width, inObject.Bbox.Height);
		if (inObject.Rotation > 0f || inObject.Rotation < 0f)
		{
			base.Rotation = inObject.Rotation;
			DrawOrigin = inObject.DrawOrigin;
		}
		else
		{
			DrawOrigin = new Vector2((float)Bbox.Width / 2f, (float)Bbox.Height / 2f);
		}
		IsFacingLeft = inObject.IsFacingLeft;
		IsImageFacingLeft = IsFacingLeft;
		SnapBboxToPosition();
		ChangeAnimation(inObject.AnimationStart);
		_isAffectedByGravity = true;
		_isAffectedByLevelBounds = false;
		base.DoesCollideWithTiles = true;
		_airDragFactor = 0.015f;
		_isAffectedByTime = true;
		base.CanBeTriggered = false;
	}

	internal void Push(Vector2 inVelocity, Point origin)
	{
		Point point = Bbox.Center.Subtract(origin);
		float num = (float)_level.NextRandomDouble();
		float num2 = num * 5f;
		float num3 = (num + 0.5f) * 2f;
		float num4 = 5f;
		bool flag;
		if (inVelocity.X > 1f || inVelocity.X < -1f)
		{
			flag = inVelocity.X < -1f;
			num4 = Math.Abs(inVelocity.X);
		}
		else
		{
			flag = point.X < 0;
		}
		int num5 = Math.Abs(point.Y);
		num4 = ((num5 <= 24) ? (num4 * (1f - (float)(num5 / 24))) : 5f);
		if (num4 < 5f)
		{
			num4 = 5f;
		}
		num4 += num2;
		num4 *= (float)(((!flag) ? 1 : (-1)) * 20);
		float y = _velocity.Y + -100f;
		_velocity = new Vector2(num4, y);
		float num6 = (float)point.Y * num3 * 0.5f;
		num6 = ((num6 < 0f) ? Math.Max(num6, -25f) : Math.Min(num6, 25f));
		_rotationSpeed = num6 * (float)((!flag) ? 1 : (-1));
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_timeSinceConception < 0.1f)
			{
				_timeSinceConception += delta;
			}
			if (Math.Abs(_rotationSpeed) > 0.1f)
			{
				float num = base.Rotation + delta * _rotationSpeed;
				if (num >= 6.28f)
				{
					num -= 6.28f;
				}
				base.Rotation = num;
			}
		}
		base.Update(delta);
	}

	public override bool DetectTileCollisions()
	{
		if (_timeSinceConception >= 0.1f)
		{
			return base.DetectTileCollisions();
		}
		return false;
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool flag = false;
		if (tile.Position.Y >= Bbox.Top)
		{
			flag = base.CollideSolidTile(tile, depth);
			if (flag)
			{
				switch (DeathType)
				{
				case EDebrisDeathType.Fire:
					_level.AddAnimation(EBattleAnimationType.SmallBoom, new Point(Position.X, tile.Bbox.Top), ETeamSide.Neutral);
					break;
				case EDebrisDeathType.Dust:
					_level.AddAnimation(EBattleAnimationType.DustBoom, new Point(Position.X, tile.Bbox.Top), ETeamSide.Neutral);
					break;
				case EDebrisDeathType.HeavyLand:
				{
					_level.RequestScreenShake(new Vector2(0f, 3f), 0.2f, 6f, isAffectedByTime: true);
					int num = (int)Math.Ceiling((float)base.FrameSource.Width / 12f);
					int num2 = Position.X - base.FrameSource.Width / 2;
					for (int i = 0; i < num; i++)
					{
						_level.AddAnimation(EBattleAnimationType.DustBoom, new Point(num2 + i * 12, tile.Bbox.Top), ETeamSide.Neutral, isFacingRight: true, i == 0);
					}
					break;
				}
				}
				_level.RequestRemoveObject(this);
			}
		}
		return flag;
	}

	internal static void CreateFromObject(Animate target, SpriteSheet sprite, EDebrisDeathType deathType)
	{
		Level level = target.Level;
		if (target.DoesDrawBaseSprite && target.AnimationStart != -1)
		{
			DebrisEvent debrisEvent = new DebrisEvent(target, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
			debrisEvent.DeathType = deathType;
			DebrisEvent newEvent = debrisEvent;
			level.AddEvent(newEvent);
		}
		foreach (Appendage appendage in target.Appendages)
		{
			if (appendage.AnimationStart != -1)
			{
				DebrisEvent debrisEvent2 = new DebrisEvent(appendage, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
				debrisEvent2.DeathType = deathType;
				DebrisEvent newEvent2 = debrisEvent2;
				level.AddEvent(newEvent2);
			}
		}
	}

	internal static void CreateFromObject(Animate target, Vector2 force, Point origin, SpriteSheet sprite)
	{
		CreateFromObject(target, force, origin, sprite, EDebrisDeathType.Fire);
	}

	internal static void CreateFromObject(Animate target, Vector2 force, Point origin, SpriteSheet sprite, EDebrisDeathType deathType)
	{
		Level level = target.Level;
		foreach (Appendage appendage in target.Appendages)
		{
			if (appendage.AnimationStart != -1)
			{
				DebrisEvent debrisEvent = new DebrisEvent(appendage, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
				debrisEvent.DeathType = deathType;
				DebrisEvent debrisEvent2 = debrisEvent;
				debrisEvent2.Push(force, origin);
				level.RequestAddObject(debrisEvent2);
			}
		}
	}

	internal static void CreateFromAppendage(Appendage appendage, Vector2 force, Point origin, SpriteSheet sprite, EDebrisDeathType deathType)
	{
		Level level = appendage.Level;
		DebrisEvent debrisEvent = new DebrisEvent(appendage, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
		debrisEvent.DeathType = deathType;
		DebrisEvent debrisEvent2 = debrisEvent;
		debrisEvent2.Push(force, origin);
		level.RequestAddObject(debrisEvent2);
	}

	internal static void CreateFromAppendages(List<Appendage> appendages, Vector2 force, Point origin, SpriteSheet sprite, EDebrisDeathType deathType, Point offset)
	{
		if (appendages.Count <= 0)
		{
			return;
		}
		Level level = appendages[0].Level;
		foreach (Appendage appendage in appendages)
		{
			if (appendage.AnimationStart != -1)
			{
				DebrisEvent debrisEvent = new DebrisEvent(appendage, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
				debrisEvent.DeathType = deathType;
				DebrisEvent debrisEvent2 = debrisEvent;
				debrisEvent2.Push(force, origin);
				level.RequestAddObject(debrisEvent2);
			}
		}
	}

	internal static void CreateFromSquare(Animate target, Vector2 force, Point origin, SpriteSheet sprite)
	{
		Level level = target.Level;
		int num = level.NextRandomInt(4, 12);
		int num2 = level.NextRandomInt(4, 12);
		Rectangle frameSource = target.FrameSource;
		for (int i = 0; i < 2; i++)
		{
			int num3 = i * num;
			int num4 = ((i == 0) ? num : (16 - num));
			for (int j = 0; j < 2; j++)
			{
				int num5 = j * num2;
				int num6 = ((j == 0) ? num2 : (16 - num2));
				DebrisEvent debrisEvent = new DebrisEvent(target, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
				debrisEvent.Bbox = new Rectangle(0, 0, num4, num6);
				debrisEvent.Position = new Point(target.Position.X - 8 + num3 + num4 / 2, target.Position.Y - ((j == 0) ? (16 - num6) : 0));
				debrisEvent.DrawOrigin = new Vector2((float)num4 / 2f, (float)num6 / 2f);
				debrisEvent.DeathType = EDebrisDeathType.Dust;
				DebrisEvent debrisEvent2 = debrisEvent;
				debrisEvent2.SnapBboxToPosition();
				Rectangle frameSource2 = new Rectangle(frameSource.Left + num3, frameSource.Top + num5, num4, num6);
				debrisEvent2.SetFrameSource(frameSource2);
				debrisEvent2.SnapFrameToBbox();
				debrisEvent2.Push(force, target.Position);
				level.AddEvent(debrisEvent2);
			}
		}
	}

	internal static void CreateAndBreakIntoPieces(Animate target, Vector2 force, SpriteSheet sprite, int xSegments, int ySegments)
	{
		Level level = target.Level;
		int num = level.NextRandomInt(4, 12);
		int num2 = level.NextRandomInt(4, 12);
		Rectangle frameSource = target.FrameSource;
		for (int i = 0; i < xSegments; i++)
		{
			int num3 = i * num;
			int num4 = ((i == 0) ? num : (16 - num));
			for (int j = 0; j < ySegments; j++)
			{
				int num5 = j * num2;
				int num6 = ((j == 0) ? num2 : (16 - num2));
				DebrisEvent debrisEvent = new DebrisEvent(target, sprite, level.NextObjectTicketID, new ObjectTileSpecification());
				debrisEvent.Bbox = new Rectangle(0, 0, num4, num6);
				debrisEvent.Position = new Point(target.Position.X - 8 + num3 + num4 / 2, target.Position.Y - ((j == 0) ? (16 - num6) : 0));
				debrisEvent.DrawOrigin = new Vector2((float)num4 / 2f, (float)num6 / 2f);
				debrisEvent.DeathType = EDebrisDeathType.Dust;
				debrisEvent.IsFacingLeft = target.IsFacingLeft;
				debrisEvent.IsFlippedVertically = target.IsFlippedVertically;
				DebrisEvent debrisEvent2 = debrisEvent;
				debrisEvent2.SnapBboxToPosition();
				Rectangle frameSource2 = new Rectangle(frameSource.Left + num3, frameSource.Top + num5, num4, num6);
				debrisEvent2.SetFrameSource(frameSource2);
				debrisEvent2.SnapFrameToBbox();
				debrisEvent2.Push(force, target.Position);
				level.RequestAddObject(debrisEvent2);
			}
		}
	}
}
