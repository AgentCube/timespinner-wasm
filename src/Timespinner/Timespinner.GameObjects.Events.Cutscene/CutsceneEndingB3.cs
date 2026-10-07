using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;
using Timespinner.GameObjects.NPCs.Misc;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingB3 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 192;

	private const int LunaisStartX = 520;

	private const int AelanaStartX = 368;

	private const int HaristelStartX = 464;

	private const int PastAdvisorAStartX = 278;

	private const int PastAdvisorBStartX = 238;

	private const int PastKnightAStartX = 568;

	private const int HaristelWalkX = 432;

	private const int LunaisWalkX = 456;

	private readonly AelanaNPC _aelana;

	private readonly CaptainNPC _haristel;

	private readonly PastAdvisorNPC _pastAdvisorA;

	private readonly PastAdvisorNPC _pastAdvisorB;

	private readonly PastKnightNPC _pastKnightA;

	public CutsceneEndingB3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddUnhidePlayer();
		AddCameraPan(new Point(400, 120), 0f, doesBlockQueue: false);
		WarpPlayerToPosition(new Point(520, 192));
		if (_level.MainHero != null)
		{
			_level.MainHero.IsFacingLeft = true;
		}
		_aelana = new AelanaNPC(_level, new Point(368, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 16
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_haristel = new CaptainNPC(_level, new Point(464, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 3
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		SpriteSheet spMerchantCrow = _level.GCM.SpMerchantCrow;
		_pastAdvisorA = new PastAdvisorNPC(_level, new Point(278, 192), spMerchantCrow)
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_pastAdvisorB = new PastAdvisorNPC(_level, new Point(238, 192), spMerchantCrow)
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_pastKnightA = new PastKnightNPC(_level, new Point(568, 192), spMerchantCrow)
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_level.RequestAddObject(_aelana);
		_level.RequestAddObject(_haristel);
		_level.RequestAddObject(_pastAdvisorA);
		_level.RequestAddObject(_pastAdvisorB);
		_level.RequestAddObject(_pastKnightA);
	}

	internal override void DoCutscene()
	{
		AddHideFamiliar(0f);
		AddDialogue("cs_endb_3_ael_00");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _aelana,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_endb_3_ael_01");
		MoveCharacterToPosition(_haristel, new Point(432, 192), doesBlockQueue: true, 0f);
		AddDialogue("cs_endb_3_har_02");
		if (NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Captain, _level.GameSave))
		{
			AddDialogue("cs_endb_3_har_03");
		}
		MovePlayerToPosition(new Point(456, 192), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AddDialogue("cs_endb_3_lun_04");
		AddDialogue("cs_endb_3_ael_05");
		AddDialogue("cs_endb_3_ael_06");
		AddDialogue("cs_endb_3_ael_07");
		AddDialogue("cs_endb_3_lun_08");
		AddSepiaFade(5f, doesBlock: false);
		AddAutoplayGhostDialogue("cs_endb_3_lun_09");
		AddAutoplayGhostDialogue("cs_endb_3_lun_10");
		AddAutoplayGhostDialogue("cs_endb_3_lun_11");
		AddDelegateScript(base.StartLevelFadeOut);
		AddWaitScript(1f);
		AddAutoplayGhostDialogue("cs_endb_3_lun_12");
		AddEndSepiaFade(0f, doesBlock: true);
		AddSongFadeOut(3f, doesBlock: false);
		AddWaitScript(2f);
		TeleportToLevelAndRoom(17, 4, ECutsceneType.EndingB4_Winderia0);
	}
}
