using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCity6 : CutsceneBase
{
	public CutsceneCity6(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override bool AreTriggerConditionsMet()
	{
		Dictionary<int, InventoryRelic> inventory = _level.GameSave.Inventory.RelicInventory.Inventory;
		bool flag = inventory.ContainsKey(1);
		bool flag2 = inventory.ContainsKey(2);
		bool saveBool = _level.GameSave.GetSaveBool("HasUsedCityTS");
		if (flag && flag2)
		{
			return !saveBool;
		}
		return false;
	}

	internal override void DoCutscene()
	{
		MovePlayerToPosition(new Point(Position.X + 32, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddWaitScript(0.1f);
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_city_4_lun_00");
	}
}
