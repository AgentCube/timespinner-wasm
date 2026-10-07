using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingA3 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 272;

	private const int NelisteStartX = 368;

	private const int SeykisStartX = 432;

	private const int EschemStartX = 464;

	private readonly bool _isEschemPresent;

	private readonly AstrologerNPC _neliste;

	private readonly QuartermasterNPC _seykis;

	private readonly SickSoldierNPC _eschem;

	public CutsceneEndingA3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_isEschemPresent = NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.SickSoldier, _level.GameSave) && NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Quartermaster, _level.GameSave);
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
		AddDialogue("cs_enda_3_sey_00");
		AddDialogue("cs_enda_3_nel_01");
		if (NPCBase.GetAreQuestsFinished(NPCBase.ENPCType.Astrologer, _level.GameSave))
		{
			AddDialogue("cs_enda_3_nel_02");
			AddDialogue("cs_enda_3_nel_03");
		}
		else
		{
			AddDialogue("cs_enda_3_nel_04");
		}
		AddDialogue("cs_enda_3_sey_05");
		if (_isEschemPresent)
		{
			AddDialogue("cs_enda_3_esc_06");
		}
		AddDialogue("cs_enda_3_nel_07");
		TeleportToLevelAndRoom(17, 3, ECutsceneType.EndingA4_Present2);
	}
}
