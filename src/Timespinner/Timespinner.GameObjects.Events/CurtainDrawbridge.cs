using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events;

internal sealed class CurtainDrawbridge : GameEvent
{
	private const int BridgeLinkCount = 5;

	private const int BridgeLinkWidth = 80;

	private const int BridgeLinkSectionWidth = 400;

	private const int BridgeCapWidth = 32;

	private const int BridgeWidth = 432;

	private const int BridgeChainCount = 20;

	private const int BridgeBaseOffset = -13;

	private const float TimeToRaiseLowerBridge = 4f;

	private readonly int _farthestBridgeLeft;

	private readonly Point _basePosition;

	private readonly Appendage _bridgeAppendage;

	private readonly Appendage _chainAppendage;

	private bool _isRaising;

	private bool _isEngineerDead;

	private float _raiseLowerCounter;

	private float _lowerPercentage;

	public CurtainDrawbridge(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(8, -3);
		_basePosition = Position;
		_farthestBridgeLeft = _basePosition.X - 432;
		Position.Add(32, 0);
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = true;
		_sprite = _level.GCM.SpMiscCurtain;
		_isSolid = false;
		base.DrawPlane = EDrawPlane.Normal;
		base.CanBeTriggeredByFamiliar = true;
		Bbox = new Rectangle(_position.X, _position.Y, 24, 24);
		_isAffectedByTime = true;
		_bridgeAppendage = new Appendage(this, new Point(1, 1), new Point(0, 28), _level, _sprite);
		_bridgeAppendage.ChangeAnimation(0);
		_bridgeAppendage.Position = Position.Add(-400, 0);
		_bridgeAppendage.DrawOrigin = new Vector2(0f, 28f);
		_bridgeAppendage.AddRigidLinks(5, new Point(1, 1), new Point(0, 12), 1, new Vector2(0f, 12f), new Point(32, 1));
		_chainAppendage = new Appendage(this, new Point(1, 1), new Point(0, 5), _level, _sprite)
		{
			DrawPriority = -1,
			DrawOrigin = new Vector2(0f, 5f)
		};
		_chainAppendage.ChangeAnimation(2);
		_chainAppendage.AddRigidLinks(20, new Point(1, 1), new Point(0, 5), 2, new Vector2(0f, 5f), new Point(16, 5));
		int num = 0;
		foreach (Appendage appendage in _chainAppendage.Appendages)
		{
			if (num % 2 == 0)
			{
				appendage.ChangeAnimation(3);
				appendage.DrawOrigin = new Vector2(0f, 2f);
				appendage.BboxOffset = new Point(0, 2);
				appendage.DrawPriority = 1;
			}
			else
			{
				appendage.DrawPriority = -1;
			}
			num++;
		}
		_appendages.Add(_bridgeAppendage);
		_bridgeAppendage.AddAppendage(_chainAppendage);
	}

	public override void Initialize()
	{
		if (!((!_level.GetLevelSaveBool("HasWinchBeenUsed")) ? (!_level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.PyramidsKey)) : _level.GetLevelSaveBool("IsDrawbridgeRaised")))
		{
			_isEngineerDead = true;
			_isRaising = true;
			_raiseLowerCounter = 0f;
		}
		else
		{
			_isRaising = false;
			_raiseLowerCounter = 4f;
		}
		Update(0f);
		base.Initialize();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isEngineerDead)
			{
				if (_isRaising)
				{
					_raiseLowerCounter -= delta;
				}
				else
				{
					_raiseLowerCounter += delta;
				}
			}
			float num = _raiseLowerCounter / 4f;
			if (num > 1f)
			{
				num = 1f;
			}
			else if (num < 0f)
			{
				num = 0f;
			}
			_lowerPercentage = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
			_bridgeAppendage.Position = _basePosition.Add(new Point(-(int)(Math.Cos(_lowerPercentage * ((float)Math.PI / 8f)) * 400.0), -(int)(Math.Sin(_lowerPercentage * ((float)Math.PI / 8f)) * 400.0)));
			_bridgeAppendage.Rotation = MathHelper.Lerp(0f, (float)Math.PI / 8f, _lowerPercentage);
			_chainAppendage.Position = _bridgeAppendage.Position.Add(5 + (int)(Math.Sin(_lowerPercentage * ((float)Math.PI / 2f)) * 6.0), -22 + (int)(Math.Sin(_lowerPercentage * ((float)Math.PI / 2f)) * 3.0));
			_chainAppendage.Rotation = MathHelper.Lerp(-(float)Math.PI / 4f, -(float)Math.PI / 16f, _lowerPercentage);
		}
		base.Update(delta);
		base.TriggerBbox = base.OuterBbox;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		return CollideDynamicRamp(who);
	}

	private bool CollideDynamicRamp(Mobile who)
	{
		bool result = false;
		int num = who.Position.X - _farthestBridgeLeft;
		if (num > 0 && num < 432)
		{
			float num2 = (float)num / 432f;
			int num3 = (int)(Math.Asin(_bridgeAppendage.Rotation) * 432.0);
			int num4 = Position.Y + -13 - (int)((1f - num2) * (float)num3) - (int)(_lowerPercentage * 2f);
			int num5 = num4 + 24;
			int num6 = who.Bbox.Right - base.OuterBbox.Left;
			if (num6 < who.Bbox.Width / 2 && who.Position.Y > num4 && who.Bbox.Top < num5 && who.Velocity.X > 0f)
			{
				who.Position = new Point(who.Position.X - num6, who.Position.Y);
				who.Velocity = new Vector2(0f, who.Velocity.Y);
				result = true;
			}
			else if (who.Position.Y > num4 && who.Bbox.Top < num4)
			{
				int x = who.Position.X;
				float x2 = who.Velocity.X;
				float num7 = MathHelper.Min(who.Velocity.Y, 0f);
				if (!base.IsFrozen && _isRaising && _lowerPercentage < 1f && _lowerPercentage > 0f)
				{
					num7 += 100f;
				}
				who.CollisionSetPosition(new Point(x, num4), null);
				who.IsGrounded = true;
				who.Velocity = new Vector2(x2, num7);
				if (_lowerPercentage != 0f)
				{
					who.IsOnSlope = true;
					who.CurrentSlopeAngle = MathHelper.Clamp(_lowerPercentage * 1f, 0.5f, 3f);
				}
				who.SnapBboxToPosition();
				who.SnapFrameToBbox();
				result = true;
			}
			else if (who.Position.Y > num5 && who.Bbox.Top < num5 && who.Velocity.Y <= 0f)
			{
				who.Position = new Point(who.Position.X, num5 + who.Bbox.Height);
				who.Velocity = new Vector2(who.Velocity.X, Math.Max(who.Velocity.Y, 0f));
				who.IsHittingOnHeadOnCeiling = true;
			}
		}
		return result;
	}
}
