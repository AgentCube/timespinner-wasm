using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutscenePrologue4 : CutsceneBase
{
	private const int SelenSpawnX = 264;

	private const int SelenSpawnY = 192;

	private readonly SelenNPC _selen;

	public CutscenePrologue4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_selen = new SelenNPC(_level, new Point(264, 192), -1)
		{
			IsDoingTutorial = true,
			IsFacingLeft = true
		};
		_selen.SpawnDummy();
		_level.RequestAddObject(_selen);
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 8;
		animationSpec.Length = 5;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Cycle;
		AnimationSpec newAnim = animationSpec;
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_protut_0_sel_00");
		AddDialogue("cs_protut_0_sel_01");
		AddDialogue("cs_protut_0_sel_02");
		AddWaitScript(0.01f);
		_level.GameSave.GiveOrb(EInventoryOrbType.Blue, EOrbSlot.Melee);
		AddScript(new ScriptAction(EInventoryOrbType.Blue, EOrbSlot.Melee));
		AddWaitScript(0.01f);
		AddDialogue("cs_protut_0_sel_03");
		AddDialogue("cs_protut_0_sel_05");
		_level.ShowMenuDialogueMessage("cs_protut_0_A");
	}
}
