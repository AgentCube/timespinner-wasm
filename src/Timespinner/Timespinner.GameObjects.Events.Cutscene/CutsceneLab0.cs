using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Doors;
using Timespinner.GameObjects.NPCs.City;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLab0 : CutsceneBase
{
	private readonly ScientistNPC _scientist1;

	private readonly ScientistNPC _scientist2;

	private KeycardDoorEvent _keycardDoor;

	public CutsceneLab0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.DoesHideOrbsAutomatically = false;
		SpriteSheet spMerchantCrow = _level.GCM.SpMerchantCrow;
		Point inPosition2 = new Point(416, 160);
		_scientist1 = new ScientistNPC(_level, inPosition2, spMerchantCrow, isScientist1: true, isFlippedHorizontally: false);
		_scientist2 = new ScientistNPC(_level, inPosition2, spMerchantCrow, isScientist1: false, isFlippedHorizontally: false);
	}

	public override void Initialize()
	{
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.KeycardDoor);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			_keycardDoor = (KeycardDoorEvent)item;
			if (_keycardDoor != null)
			{
				break;
			}
		}
		base.Initialize();
	}

	internal override void DoCutscene()
	{
		MovePlayerToPosition(new Point(Position.X - 32, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		if (_keycardDoor != null)
		{
			AddDelegateScript(delegate
			{
				_keycardDoor.RemotelyOpenDoor();
			});
		}
		AddWaitScript(0.5f);
		HideOrbs();
		AddLevelScriptAction(new ScriptAction(EScriptActionType.ChangeColor, 0f, 0.5f, new Vector4(1f, 0f, 1f, 0f))
		{
			TargetType = EScriptTargetType.Familiar,
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction(EScriptActionType.ChangeColor, 0f, 0.5f, new Vector4(1f, 0.5f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		MovePlayerToPosition(new Point(Position.X, Position.Y), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AddDialogue("cs_lab_sci_00");
		AddDialogue("cs_lab_sci_01");
		AddDialogue("cs_lab_sci_02");
		AddDialogue("cs_lab_sci_03");
		AddDialogue("cs_lab_sci_04");
		AddDelegateScript(AddScientists);
		ScriptAction scriptAction = new ScriptAction(EScriptActionType.GoToPoint, 0f, 3f, new Vector4(-32f, 0f, 0f, 0f));
		scriptAction.ScriptTarget = _scientist1;
		scriptAction.TargetType = EScriptTargetType.Specified;
		ScriptAction inAction = scriptAction;
		ScriptAction scriptAction2 = new ScriptAction(EScriptActionType.GoToPoint, 0.2f, 3f, new Vector4(-32f, 0f, 0f, 0f));
		scriptAction2.ScriptTarget = _scientist2;
		scriptAction2.TargetType = EScriptTargetType.Specified;
		ScriptAction inAction2 = scriptAction2;
		AddLevelScriptAction(inAction);
		AddLevelScriptAction(inAction2);
		AddWaitScript(3.5f);
		AddLevelScriptAction(new ScriptAction(EScriptActionType.ChangeColor, 0f, 0.5f, new Vector4(0f, 1f, 1f, 0f))
		{
			TargetType = EScriptTargetType.Familiar,
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction(EScriptActionType.ChangeColor, 0f, 0.5f, new Vector4(0.5f, 1f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		UnhideOrbs();
		AddDelegateScript(RemoveScientists);
	}

	private void AddScientists()
	{
		_level.RequestAddObject(_scientist1);
		_level.RequestAddObject(_scientist2);
	}

	private void RemoveScientists()
	{
		_level.RequestRemoveObject(_scientist1);
		_level.RequestRemoveObject(_scientist2);
	}
}
