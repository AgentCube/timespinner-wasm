using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest9 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 128;

	private const int EschemStartX = 0;

	private const int LunaisStartX = 0;

	private const int SeykisStartX = -16;

	private const int LunaisWalk1X = 80;

	private const int EschemWalkX = 112;

	private const int SeykisWalkX = 48;

	private SickSoldierNPC _eschem;

	private QuartermasterNPC _seykis;

	public CutsceneForest9(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
	}

	internal override bool AreTriggerConditionsMet()
	{
		return _level.GetLevelSaveBool("IsEscortMissionActive");
	}

	internal override void DoCutscene()
	{
		_level.SetLevelSaveBool("IsEscortMissionActive", value: false);
		_eschem = new SickSoldierNPC(_level, new Point(0, 128), -1, new ObjectTileSpecification(487)
		{
			Argument = 4
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_eschem.Initialize();
		_eschem.StartEscortQuest(shouldSetState: false);
		_level.RequestAddObject(_eschem);
		_seykis = new QuartermasterNPC(_level, new Point(-16, 128), -1, new ObjectTileSpecification(487)
		{
			Argument = 2
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_seykis.Initialize();
		WarpPlayerToPosition(new Point(0, 128));
		MoveCharacterToPosition(_eschem, new Point(112, 128), doesBlockQueue: false, 0f);
		MovePlayerToPosition(new Point(80, 128), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddDialogue("q_esc_2_esc_13");
		AddDialogue("q_esc_2_esc_14");
		AddDialogue("q_esc_2_lun_15");
		AddCharacterFaceDirection(_eschem, isFacingLeft: true, 0f);
		AddDialogue("q_esc_2_esc_16");
		AddDialogue("q_esc_2_lun_17");
		AddDialogue("q_esc_2_esc_18");
		AddDialogue("q_esc_2_sey_19");
		AddDelegateScript(AddSeykis);
		AddPlayerFaceDirection(isFacingLeft: true, 0f);
		MoveCharacterToPosition(_seykis, new Point(48, 128), doesBlockQueue: false, 0f);
		AddDialogue("q_esc_2_sey_20");
		AddDialogue("q_esc_2_lun_21");
		AddDialogue("q_esc_2_esc_22");
		AddDialogue("q_esc_2_sey_23");
		AddDialogue("q_esc_2_lun_24");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Run,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			DoesClearSameType = true,
			Arguments = new Vector4(-1f, 0f, 0f, 0f),
			IsUnskippable = true
		});
		AddDelegateScript(EndQuest);
	}

	private void AddSeykis()
	{
		if (_seykis != null)
		{
			_level.RequestAddObject(_seykis);
		}
	}

	private void EndQuest()
	{
		if (_eschem != null)
		{
			_eschem.SetPrimaryProgress(3);
		}
	}
}
