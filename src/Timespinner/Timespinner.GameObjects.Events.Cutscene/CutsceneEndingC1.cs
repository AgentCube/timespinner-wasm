using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingC1 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 272;

	private const int NelisteStartX = 352;

	private const int SeykisStartX = 432;

	private const int EschemStartX = 464;

	private const int PortalStartX = 160;

	private const int SeykisWalkX = 784;

	private const int NelisteWalk1X = 368;

	private const int NelisteWalk2X = 464;

	private const int NelisteWalk3X = 432;

	private readonly bool _isEschemPresent;

	private readonly AstrologerNPC _neliste;

	private readonly QuartermasterNPC _seykis;

	private readonly SickSoldierNPC _eschem;

	public CutsceneEndingC1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_isEschemPresent = NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.SickSoldier, _level.GameSave) && NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Quartermaster, _level.GameSave);
		AddLockCamera();
		AddHidePlayer();
		AddCameraPan(new Point(400, 200), 0f, doesBlockQueue: false);
		_neliste = new AstrologerNPC(_level, new Point(352, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 0
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_seykis = new QuartermasterNPC(_level, new Point(432, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 2
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_eschem = new SickSoldierNPC(_level, new Point(464, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 4
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_eschem.Initialize();
		_level.RequestAddObject(_neliste);
		_level.RequestAddObject(_seykis);
		if (_isEschemPresent)
		{
			_level.RequestAddObject(_eschem);
		}
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_endc_1_sey_00");
		AddDialogue("cs_endc_1_nel_01");
		AddDialogue("cs_endc_1_sey_02");
		MoveCharacterToPosition(_neliste, new Point(368, 272), doesBlockQueue: false, 0f);
		AddDialogue("cs_endc_1_nel_03");
		AddDialogue("cs_endc_1_sey_04");
		if (_isEschemPresent)
		{
			AddDialogue("cs_endc_1_esc_05");
		}
		AddDialogue("cs_endc_1_sey_06");
		if (!NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Astrologer, _level.GameSave))
		{
			AddDialogue("cs_endc_1_nel_07");
			MoveCharacterToPosition(_neliste, new Point(784, 272), doesBlockQueue: false, 0f);
			AddWaitScript(0.1f);
			MoveCharacterToPosition(_seykis, new Point(784, 272), doesBlockQueue: false, 0f);
			if (_isEschemPresent)
			{
				MoveCharacterToPosition(_eschem, new Point(784, 272), doesBlockQueue: false, 0.1f);
			}
			AddWaitScript(0.75f);
		}
		else
		{
			AddDialogue("cs_endc_1_nel_08");
			MoveCharacterToPosition(_seykis, new Point(784, 272), doesBlockQueue: false, 0f);
			MoveCharacterToPosition(_eschem, new Point(784, 272), doesBlockQueue: false, 0.1f);
			AddWaitScript(2.5f);
			MoveCharacterToPosition(_neliste, new Point(464, 272), doesBlockQueue: false, 0f);
			AddWaitScript(0.75f);
			PlayScriptedSFX(ESFX.LunaisTimeGateWarpin, new Point(160, 272));
			AddWaitScript(0.75f);
			MoveCharacterToPosition(_neliste, new Point(432, 272), doesBlockQueue: false, 0f);
			AddWaitScript(0.5f);
			AddDialogue("cs_endc_1_nel_09");
			MoveCharacterToPosition(_neliste, new Point(160, 272), doesBlockQueue: false, 0f);
			AddWaitScript(0.75f);
		}
		TeleportToLevelAndRoom(17, 1, ECutsceneType.EndingC2_Past1);
	}
}
