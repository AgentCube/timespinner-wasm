using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCity4 : CutsceneBase
{
	private const EInventoryFamiliarType MeyefFamiliarType = EInventoryFamiliarType.Meyef;

	private const EInventoryRelicType SpindleRelicType = EInventoryRelicType.TimespinnerSpindle;

	public CutsceneCity4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
	}

	internal override bool AreTriggerConditionsMet()
	{
		PlayerInventory inventory = _level.GameSave.Inventory;
		bool flag = inventory.FamiliarInventory.Inventory.ContainsKey(1);
		bool result = inventory.RelicInventory.Inventory.ContainsKey(2);
		if (!flag)
		{
			return result;
		}
		return false;
	}

	internal override void DoCutscene()
	{
		AddSummonMeyef();
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.SitStill));
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Familiar,
			ActionType = EScriptActionType.WarpToPoint,
			Arguments = new Vector4(0f, 112f, 0f, 0f)
		});
		AddMeyefFlyAround(new Point(Position.X - 32, Position.Y - 24), 1.25f, 16f);
		AddWaitScript(0.5f);
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(EScriptActionType.Backdash, 0f, 0.52f, Vector4.Zero)
		{
			TargetType = EScriptTargetType.Player1
		});
		AddDialogue("cs_mey_lun_02");
		AddMeyefFlyTo(new Point(Position.X, Position.Y - 16), 1.5f, doesBlock: true);
		AddWaitScript(0.65f);
		AddMeyefMew();
		AddDialogue("cs_mey_lun_03");
		AddMeyefMew();
		AddWaitScript(0.05f);
		AddDelegateScript(EquipMeyef);
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Dismiss)
		{
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
	}

	private void EquipMeyef()
	{
		_level.GameSave.Inventory.EquippedFamiliar = EInventoryFamiliarType.Meyef;
	}
}
