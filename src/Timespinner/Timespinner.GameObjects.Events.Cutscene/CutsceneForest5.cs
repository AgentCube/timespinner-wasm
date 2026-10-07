using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest5 : CutsceneBase
{
	private const int FirstFloorY = 368;

	private const int SecondFloorY = 160;

	private const int LunaisStartX = 992;

	private const int MedicStartX = 1056;

	private const int SeykisStartX = 160;

	private const int MedicEndX = 1092;

	private const int LunaisWalkX1 = 1024;

	private const int MedicFinalX = 1056;

	private SickSoldierNPC _eschem;

	public CutsceneForest5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		NPCBase character = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Medic);
		NPCBase nPCBase = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.SickSoldier);
		_eschem = nPCBase as SickSoldierNPC;
		AddDialogue("q_ram_1_lun_18");
		AddDialogue("q_ram_1_ram_19");
		AddDialogue("q_ram_1_lun_20");
		AddDialogue("q_ram_1_ram_21");
		FadeOut(0.5f);
		AddWaitScript(0.25f);
		WarpPlayerToPosition(new Point(992, 160));
		WarpCharacterToPosition(character, new Point(1056, 160));
		NPCBase nPCBase2 = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Quartermaster);
		if (nPCBase2 != null && nPCBase2.Bbox.Top < 160)
		{
			nPCBase2.DoesNeedZoneBeforeNextQuest = true;
			WarpCharacterToPosition(nPCBase2, new Point(160, 368));
		}
		MoveCharacterToPosition(character, new Point(1092, 160), doesBlockQueue: false, 0f);
		MovePlayerToPosition(new Point(1024, 160), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddWaitScript(1.5f);
		EschemSlowWakeUp();
		AddWaitScript(0.25f);
		EschemCough();
		AddWaitScript(0.25f);
		EschemCough();
		AddDialogue("q_ram_1_ram_22");
		EschemCough();
		AddDialogue("q_ram_1_ram_23");
		EschemSleep();
		AddWaitScript(0.5f);
		MoveCharacterToPosition(character, new Point(1056, 160), doesBlockQueue: true, 0f);
		AddDialogue("q_ram_1_ram_24");
	}

	private void EschemSlowWakeUp()
	{
		if (_eschem != null)
		{
			_eschem.AddSlowWakeUpAnimation();
		}
	}

	private void EschemCough()
	{
		if (_eschem != null)
		{
			_eschem.AddCoughAnimation();
		}
	}

	private void EschemSleep()
	{
		if (_eschem != null)
		{
			_eschem.AddFallAsleepAnimation();
		}
	}
}
