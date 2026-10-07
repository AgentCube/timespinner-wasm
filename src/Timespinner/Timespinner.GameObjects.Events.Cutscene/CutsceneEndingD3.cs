using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;
using Timespinner.GameObjects.NPCs.Misc;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingD3 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 192;

	private const int HaristelWalkX = 432;

	private const int AelanaStartX = 368;

	private const int HaristelStartX = 464;

	private const int PastAdvisorAStartX = 278;

	private const int PastAdvisorBStartX = 238;

	private const int PastKnightAStartX = 520;

	private const int PastKnightBStartX = 568;

	private readonly bool _isHaristelPresent;

	private readonly AelanaNPC _aelana;

	private readonly PhiliaNPC _philia;

	private readonly CaptainNPC _haristel;

	private readonly PastAdvisorNPC _pastAdvisorA;

	private readonly PastAdvisorNPC _pastAdvisorB;

	private readonly PastKnightNPC _pastKnightA;

	private readonly PastKnightNPC _pastKnightB;

	public CutsceneEndingD3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_isHaristelPresent = NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Captain, _level.GameSave);
		AddLockCamera();
		AddHidePlayer();
		AddCameraPan(new Point(400, 120), 0f, doesBlockQueue: false);
		_aelana = new AelanaNPC(_level, new Point(368, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 16
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_philia = new PhiliaNPC(_level, new Point(392, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 17
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
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
		_pastKnightA = new PastKnightNPC(_level, new Point(520, 192), spMerchantCrow)
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_pastKnightB = new PastKnightNPC(_level, new Point(568, 192), spMerchantCrow)
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_level.RequestAddObject(_aelana);
		_level.RequestAddObject(_philia);
		if (_isHaristelPresent)
		{
			_haristel = new CaptainNPC(_level, new Point(464, 192), -1, new ObjectTileSpecification(487)
			{
				Argument = 3
			})
			{
				CannotBeTalkedTo = true,
				IsSpawnedForCutscene = true,
				IsFacingLeft = true
			};
			_level.RequestAddObject(_haristel);
		}
		_level.RequestAddObject(_pastAdvisorA);
		_level.RequestAddObject(_pastAdvisorB);
		_level.RequestAddObject(_pastKnightA);
		_level.RequestAddObject(_pastKnightB);
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_endd_2_ael_00");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _aelana,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_endd_2_ael_01");
		if (_isHaristelPresent)
		{
			AddDialogue("cs_endd_2_ael_02");
			MoveCharacterToPosition(_haristel, new Point(432, 192), doesBlockQueue: true, 0f);
			AddDialogue("cs_endd_2_har_03");
			AddDialogue("cs_endd_2_ael_04");
		}
		AddDialogue("cs_endd_2_phi_05");
		AddDialogue("cs_endd_2_ael_06");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _philia,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_endd_2_ael_07");
		AddDialogue("cs_endd_2_phi_08");
		TeleportToLevelAndRoom(17, 11, ECutsceneType.EndingD4_Present0);
	}
}
