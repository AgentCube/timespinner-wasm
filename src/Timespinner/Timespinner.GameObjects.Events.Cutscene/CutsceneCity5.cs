using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Treasure;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCity5 : CutsceneBase
{
	private const int WarOfSistersTerminalArgument = 68;

	private JournalComputerEvent _warComputerTerminal;

	public CutsceneCity5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.DoesFadeOutWhenSkipped = false;
	}

	public override void Initialize()
	{
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.JournalEntry);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item.ObjectArgument == 68)
			{
				_warComputerTerminal = (JournalComputerEvent)item;
				if (_warComputerTerminal != null)
				{
					break;
				}
			}
		}
		base.Initialize();
	}

	internal override void DoCutscene()
	{
		MovePlayerToPosition(new Point(Position.X - 8, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddWaitScript(0.15f);
		PlayScriptedSFX(ESFX.DoorKeycardAccessGranted, Position);
		AddDialogue("cs_nel2_lun_00");
		AddDialogue("cs_nel2_ter_01");
		AddDialogue("inv_jou_File4_0");
		AddDialogue("cs_nel2_lun_02");
		AddDialogue("inv_jou_File4_1");
		AddDialogue("cs_nel2_lun_03");
		AddDialogue("cs_nel2_lun_04");
		AddDelegateScript(DownloadFile);
	}

	private void DownloadFile()
	{
		if (_warComputerTerminal != null)
		{
			_warComputerTerminal.GetJournalEntry();
		}
	}
}
