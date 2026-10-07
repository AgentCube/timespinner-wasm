using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Doors;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Misc;

internal class EscortMissionManager : GameEvent
{
	private const int NPCIndex = 487;

	internal const string IsEscortMissionActiveKey = "IsEscortMissionActive";

	private bool _isEscortActive;

	private bool _hasCheckedIfQuestIsActive;

	private SickSoldierNPC _eschem;

	private TeleportEvent _eastTeleport;

	public EscortMissionManager(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_doesDrawBaseSprite = false;
		base.IsAffectedByTime = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesDrawBoundingBox = true;
	}

	public override void Update(float delta)
	{
		if (!_hasCheckedIfQuestIsActive)
		{
			_hasCheckedIfQuestIsActive = true;
			_isEscortActive = _level.GetLevelSaveBool("IsEscortMissionActive");
			if (_isEscortActive)
			{
				BeginEscortMission();
			}
		}
		if (_isEscortActive)
		{
			UpdateEscortMission();
		}
		base.Update(delta);
	}

	private void BeginEscortMission()
	{
		_eschem = new SickSoldierNPC(_level, Position, -1, new ObjectTileSpecification(487)
		{
			Argument = 4
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_eschem.Initialize();
		_eschem.StartEscortQuest(shouldSetState: true);
		_level.RequestAddObject(_eschem);
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.EastTeleport);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item is TeleportEvent eastTeleport)
			{
				_eastTeleport = eastTeleport;
				_eastTeleport.IsActive = false;
				break;
			}
		}
		IEnumerable<GameEvent> eventAllEventsOfType2 = _level.GetEventAllEventsOfType(EEventTileType.WestTeleport);
		foreach (GameEvent item2 in eventAllEventsOfType2)
		{
			if (item2 is TeleportEvent teleportEvent)
			{
				teleportEvent.IsActive = false;
			}
		}
		_level.PreventPlayerFromWarpingOut();
	}

	private void UpdateEscortMission()
	{
		if (_eastTeleport != null && _eschem != null)
		{
			int num = _eastTeleport.Position.X - _eschem.Position.X;
			if (num <= 64)
			{
				_isEscortActive = false;
				_eastTeleport.IsActive = true;
			}
		}
	}
}
