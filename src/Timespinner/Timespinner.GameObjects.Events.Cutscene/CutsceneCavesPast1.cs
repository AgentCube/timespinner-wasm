using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCavesPast1 : CutsceneBase
{
	private const int StartY = 160;

	private const int LunaisStartX = 976;

	private const int CaptainStartX = 1016;

	private const int MedicStartX = 1056;

	public CutsceneCavesPast1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		_level.RequestScreenFadeOut(0f, 0.25f, 0.25f, 0f);
	}

	internal override void DoCutscene()
	{
		int primaryQuestState = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Quartermaster, _level.GameSave);
		if (primaryQuestState == 1)
		{
			NPCBase nPCBase = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Quartermaster);
			if (nPCBase != null)
			{
				nPCBase.DoesNeedZoneBeforeNextQuest = true;
			}
		}
		WarpPlayerToPosition(new Point(976, 160));
		NPCBase nPCBase2 = CutsceneBase.GrabOrCreateNPC(_level, NPCBase.ENPCType.Medic, new Point(1056, 160));
		NPCBase nPCBase3 = CutsceneBase.GrabOrCreateNPC(_level, NPCBase.ENPCType.Captain, new Point(1016, 160));
		NPCBase nPCBase4 = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.SickSoldier);
		if (nPCBase2 != null)
		{
			nPCBase2.IsFacingLeft = true;
			nPCBase2.ShrinkTriggerBboxToBbox();
		}
		if (nPCBase3 != null)
		{
			nPCBase3.IsFacingLeft = false;
			nPCBase3.ShrinkTriggerBboxToBbox();
		}
		nPCBase4?.ShrinkTriggerBboxToBbox();
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			ActionTimer = 0.25f,
			DoesBlockQueue = true,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddWaitScript(0.5f);
		AddDialogue("cs_cav_1_har_00");
		AddDialogue("cs_cav_1_ram_01");
		AddDialogue("cs_cav_1_har_02");
		AddDialogue("cs_cav_1_ram_03");
		AddDialogue("cs_cav_1_har_04");
		AddDialogue("cs_cav_1_lun_05");
		AddDialogue("cs_cav_1_ram_06");
		AddDialogue("cs_cav_1_ram_07");
		AddDialogue("cs_cav_1_lun_08");
		AddDialogue("cs_cav_1_ram_09");
		AddDelegateScript(EndRamedaFoundCutscene);
	}

	private void EndRamedaFoundCutscene()
	{
		_level.GameSave.SetValue("IsDoingRamedaFoundCutscene", value: false);
	}
}
