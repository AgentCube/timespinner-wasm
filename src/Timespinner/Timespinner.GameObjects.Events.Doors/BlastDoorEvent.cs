using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class BlastDoorEvent : SlidingDoorEvent
{
	public BlastDoorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isLocked = false;
		_sprite = _level.GCM.SpSlidingDoors;
		base.EventType = EEventTileType.BossDoor;
		base.CanBeTriggeredByFamiliar = true;
		IsFacingLeft = true;
		ChangeAnimation(12);
	}

	public override void Initialize()
	{
		if (!_level.GameSave.GetSaveBool("11_LabPower"))
		{
			_doorState = ESlidingDoorState.Opened;
			base.IsOpenForever = true;
			SetPositionToOpen();
		}
		else
		{
			_doorState = ESlidingDoorState.Closed;
			base.IsLocked = true;
		}
		base.Initialize();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _doorState == ESlidingDoorState.Opened && _level.IsPowerOff)
		{
			CloseAndLock();
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = base.TriggerEvent(who, depth);
		if (Position.X <= 16 && who.Position.X <= Position.X)
		{
			who.Position = new Point(Bbox.Right, who.Position.Y);
		}
		return result;
	}
}
