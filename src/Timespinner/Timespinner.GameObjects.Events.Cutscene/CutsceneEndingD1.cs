using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingD1 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 192;

	private const int SeykisStartX = 400;

	private const int EschemStartX = 242;

	private const int SeykisWalkX1 = 176;

	private const int SeykisWalkX2 = 216;

	private readonly QuartermasterNPC _seykis;

	private readonly SickSoldierNPC _eschem;

	public CutsceneEndingD1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_level.GameSave.SetValue("IsEnding4", value: true);
		_seykis = new QuartermasterNPC(_level, new Point(400, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 2
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_eschem = new SickSoldierNPC(_level, new Point(242, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 4
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true,
			IsOnVilete = true
		};
		_eschem.Initialize();
		_level.RequestAddObject(_seykis);
		_level.RequestAddObject(_eschem);
	}

	internal override void DoCutscene()
	{
		MoveCharacterToPosition(_seykis, new Point(176, 192), doesBlockQueue: true, 0f);
		AddWaitScript(1f);
		MoveCharacterToPosition(_seykis, new Point(216, 192), doesBlockQueue: true, 0f);
		AddDialogue("cs_endd_0_sey_00");
		AddDialogue("cs_endd_0_esc_01");
		AddDialogue("cs_endd_0_sey_02");
		AddDialogue("cs_endd_0_esc_03");
		AddDialogue("cs_endd_0_sey_04");
		AddDelegateScript(ResetSaveData);
		if (NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Astrologer, _level.GameSave))
		{
			TeleportToLevelAndRoom(17, 10, ECutsceneType.EndingD2_Past2);
		}
		else
		{
			TeleportToLevelAndRoom(17, 1, ECutsceneType.EndingD3_Past3);
		}
	}

	private void ResetSaveData()
	{
		_level.GameSave.SetValue("IsEnding4", value: false);
	}
}
