using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest7 : CutsceneBase
{
	private const int SecondFloorY = 160;

	private const int MedicWarpX = 1084;

	private const int MedicWalkX = 1040;

	private const int LunaisWarpX = 1008;

	private SickSoldierNPC _eschem;

	public CutsceneForest7(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		base.DoesHideOrbsAutomatically = false;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 41;
		animationSpec.Length = 1;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 42;
		animationSpec2.Length = 2;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.Speed = 0.125f;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 30;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.125f;
		animationSpec3.Type = EAnimationType.Cycle;
		AnimationSpec newAnim3 = animationSpec3;
		NPCBase nPCBase = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Medic);
		NPCBase nPCBase2 = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.SickSoldier);
		_eschem = nPCBase2 as SickSoldierNPC;
		GameSave gameSave = _level.GameSave;
		bool saveBool = gameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		bool saveBool2 = gameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw));
		if (_eschem != null)
		{
			_eschem.CannotBeTalkedTo = true;
		}
		FadeOut(0.5f);
		AddWaitScript(0.25f);
		WarpCharacterToPosition(nPCBase, new Point(1084, 160));
		WarpPlayerToPosition(new Point(1008, 160));
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = nPCBase
		});
		AddScript(new ScriptAction(newAnim, nPCBase));
		EschemWakeUp();
		AddDialogue("q_ram_3_esc_15");
		EschemSlowSleep();
		AddScript(new ScriptAction(newAnim2, nPCBase)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, nPCBase)
		{
			DoesBlockQueue = false
		});
		AddSummonMeyef();
		AddWaitScript(0.75f);
		AddMeyefMew();
		AddWaitScript(0.1f);
		MoveCharacterToPosition(nPCBase, new Point(1040, 160), doesBlockQueue: true, 0f);
		AddDialogue("q_ram_3_ram_16");
		AddDialogue("q_ram_3_ram_17");
		AddDialogue("q_ram_3_ram_18");
		AddDialogue("q_ram_3_lun_19");
		AddDialogue("q_ram_3_ram_20");
		AddMeyefMew();
		AddDialogue("q_ram_3_lun_21");
		AddDialogue("q_ram_3_ram_22");
		AddDialogue((!saveBool) ? "q_ram_3_lun_23" : "q_ram_3_lun_24");
		AddDialogue("q_ram_3_ram_25");
		if (saveBool2)
		{
			AddDialogue("q_ram_3_ram_26");
		}
		AddDialogue(saveBool ? "q_ram_3_ram_27" : "q_ram_3_ram_28");
		AddDialogue("q_ram_3_ram_29");
		AddDismissMeyef();
	}

	private void EschemWakeUp()
	{
		if (_eschem != null)
		{
			_eschem.AddWakeUpAnimation();
		}
	}

	private void EschemSlowSleep()
	{
		if (_eschem != null)
		{
			_eschem.AddSlowFallAsleepAnimation();
		}
	}
}
