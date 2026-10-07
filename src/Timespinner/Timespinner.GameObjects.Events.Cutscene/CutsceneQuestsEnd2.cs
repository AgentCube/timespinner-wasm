using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneQuestsEnd2 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 128;

	private const int NelisteStartX = 0;

	private const int LunaisStartX = 0;

	private const int LunaisWalk1X = 112;

	private const int LunaisWalk2X = 96;

	private const int NelisteWalkX = 80;

	private readonly AstrologerNPC _neliste;

	public CutsceneQuestsEnd2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_neliste = new AstrologerNPC(_level, new Point(0, 128), -1, new ObjectTileSpecification(487)
		{
			Argument = 0
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_level.RequestAddObject(_neliste);
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 23;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 23;
		animationSpec2.Length = 3;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.IsInReverse = true;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 0;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.125f;
		animationSpec3.Type = EAnimationType.Cycle;
		AnimationSpec newAnim3 = animationSpec3;
		WarpPlayerToPosition(new Point(0, 128));
		AddHideFamiliar(0f);
		MoveCharacterToPosition(_neliste, new Point(80, 128), doesBlockQueue: false, 0f);
		MovePlayerToPosition(new Point(112, 128), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddWaitScript(1f);
		AddDialogue("cs_qend_lun_57");
		AddDialogue("cs_qend_nel_58");
		AddWaitScript(2f);
		AddDialogue("cs_qend_nel_59");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			ActionTimer = 0.01f,
			DoesBlockQueue = true,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_qend_lun_60");
		_level.AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = SetNelisteBboxToGiving,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction(newAnim, _neliste)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_qend_nel_61");
		MovePlayerToPosition(new Point(96, 128), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AddScript(new ScriptAction(newAnim2, _neliste)
		{
			DoesBlockQueue = true
		});
		_level.AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = SetNelisteBboxToDefault,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction(newAnim3, _neliste)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_qend_nel_62");
		AddDialogue("cs_qend_lun_63");
		AddDialogue("cs_qend_nel_64");
		AddDialogue("cs_qend_nel_65");
		AddDialogue("cs_qend_lun_66");
		TeleportToLevelAndRoom(3, 0, ECutsceneType.Misc5_QuestsEnd3);
	}

	private void SetNelisteBboxToGiving()
	{
		_neliste.SetBboxOffsetToGiving();
	}

	private void SetNelisteBboxToDefault()
	{
		_neliste.SetBboxOffsetToDefault();
	}
}
