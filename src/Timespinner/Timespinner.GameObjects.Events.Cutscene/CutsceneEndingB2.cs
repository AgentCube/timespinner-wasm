using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingB2 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 272;

	private const int NelisteStartX = 368;

	private const int SeykisStartX = 432;

	private const int EschemStartX = 464;

	private const int LunaisStartX = 160;

	private const int WarpSFXPositionX = 64;

	private const int NelisteWalkX = 356;

	private const int LunaisWalkX = 332;

	private const int SeykisWalkX = 784;

	private readonly bool _isEschemPresent;

	private readonly bool _isNelsQuestlineFinished;

	private readonly AstrologerNPC _neliste;

	private readonly QuartermasterNPC _seykis;

	private readonly SickSoldierNPC _eschem;

	public CutsceneEndingB2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_isEschemPresent = NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.SickSoldier, _level.GameSave) && NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Quartermaster, _level.GameSave);
		_isNelsQuestlineFinished = NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Astrologer, _level.GameSave);
		AddLockCamera();
		AddHidePlayer();
		AddCameraPan(new Point(400, 200), 0f, doesBlockQueue: false);
		_neliste = new AstrologerNPC(_level, new Point(368, 272), -1, new ObjectTileSpecification(487)
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
		AddDialogue("cs_endb_2_sey_00");
		AddDialogue("cs_endb_2_nel_01");
		if (_isNelsQuestlineFinished)
		{
			AddDialogue("cs_endb_2_nel_02");
		}
		AddDialogue("cs_endb_2_sey_03");
		if (_isEschemPresent)
		{
			AddDialogue("cs_endb_2_esc_04");
			AddDialogue("cs_endb_2_nel_05");
		}
		PlayScriptedSFX(ESFX.LunaisTimeGateWarpin, new Point(64, 272));
		WarpPlayerToPosition(new Point(160, 272));
		AddHideFamiliar(0f);
		AddUnhidePlayer();
		AddWaitScript(2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _neliste,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_endb_2_nel_06");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 2f,
			DoesBlockQueue = false,
			Arguments = new Vector4(332f, 272f, 0f, 0f)
		});
		AddWaitScript(0.25f);
		MoveCharacterToPosition(_neliste, new Point(356, 272), doesBlockQueue: false, 0f);
		AddWaitScript(0.25f);
		AddDialogue("cs_endb_2_nel_07");
		_ = _isNelsQuestlineFinished;
		AddDialogue("cs_endb_2_sey_08");
		AddDialogue("cs_endb_2_lun_09");
		AddDialogue("cs_endb_2_nel_10");
		AddDialogue("cs_endb_2_lun_11");
		AddDialogue("cs_endb_2_nel_12");
		AddDialogue("cs_endb_2_lun_13");
		if (_isNelsQuestlineFinished)
		{
			MoveCharacterToPosition(_seykis, new Point(784, 272), doesBlockQueue: false, 0f);
			if (_isEschemPresent)
			{
				MoveCharacterToPosition(_eschem, new Point(784, 272), doesBlockQueue: false, 0.1f);
			}
			AddDialogue("cs_endb_2_nel_14");
			AddDialogue("cs_endb_2_lun_15");
			AddDialogue("cs_endb_2_nel_16");
			AddDialogue("cs_endb_2_lun_17");
			AddDialogue("cs_endb_2_nel_18");
		}
		TeleportToLevelAndRoom(17, 1, ECutsceneType.EndingB3_Past1);
	}
}
