using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal sealed class CutsceneCavesPast0 : CutsceneBase
{
	private const int MedicOffsetX = 8;

	private const int SoldierOffsetX = -8;

	private readonly MedicNPC _medic;

	private readonly SickSoldierNPC _soldier;

	public CutsceneCavesPast0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.DoesFadeOutWhenSkipped = false;
		base.IsWarpingAtEndOfCutscene = true;
		_medic = new MedicNPC(_level, new Point(Position.X + 8, Position.Y), -1, new ObjectTileSpecification())
		{
			IsFacingLeft = false,
			IsMoonWalking = true,
			IsSpawnedForCutscene = true,
			CannotBeTalkedTo = true
		};
		_soldier = new SickSoldierNPC(_level, new Point(Position.X + -8, Position.Y), -1, new ObjectTileSpecification())
		{
			IsFacingLeft = false,
			BboxOffset = Point.Zero,
			IsSpawnedForCutscene = true,
			CannotBeTalkedTo = true
		};
		AnimationSpec newAnimation = new AnimationSpec
		{
			Start = 41,
			Length = 1,
			Type = EAnimationType.Once,
			Speed = 0f
		};
		_medic.ChangeAnimation(newAnimation);
		_medic.Initialize();
		_soldier.Initialize();
	}

	internal override void DoCutscene()
	{
		_level.GameSave.SetValue("IsDoingRamedaFoundCutscene", value: true);
		_level.RequestAddObject(_soldier);
		_level.RequestAddObject(_medic);
		AddSongFadeOut(2f, doesBlock: false);
		AddSummonMeyef();
		AddMeyefFlyAround(new Point(_medic.Position.X + 72, _medic.Position.Y - 40), 1.25f, 12f);
		MovePlayerToPosition(new Point(Position.X + 88, Position.Y + 16), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 42;
		animationSpec.Length = 2;
		animationSpec.Type = EAnimationType.Once;
		animationSpec.Speed = 0.125f;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 30;
		animationSpec2.Length = 5;
		animationSpec2.Speed = 0.125f;
		animationSpec2.Type = EAnimationType.Cycle;
		AnimationSpec newAnim2 = animationSpec2;
		ScriptAction scriptAction = new ScriptAction();
		scriptAction.TargetType = EScriptTargetType.Specified;
		scriptAction.ScriptTarget = _medic;
		scriptAction.ActionType = EScriptActionType.Run;
		scriptAction.ActionTimer = 0.1f;
		scriptAction.Arguments = new Vector4(-1f, 0f, 0f, 0f);
		ScriptAction newScript = scriptAction;
		AddScript(new ScriptAction(newAnim, _medic)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, _medic)
		{
			DoesBlockQueue = false
		});
		AddWaitScript(0.1f);
		AddScript(newScript);
		AddDialogue("cs_cav_0_med_00");
		AddMeyefMew();
		AddDialogue("cs_cav_0_med_01");
		AddDialogue("cs_cav_0_lun_02");
		AddDialogue("cs_cav_0_med_03");
		AddDialogue("cs_cav_0_lun_04");
		AddDialogue("cs_cav_0_ram_05");
		AddWaitScript(1f);
		AddDialogue("cs_cav_0_lun_06");
		AddDialogue("cs_cav_0_ram_07");
		AddDialogue("cs_cav_0_lun_08");
		AddDialogue("cs_cav_0_ram_09");
		_level.GameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Medic), value: true);
		_level.GameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.SickSoldier), value: true);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 3;
		levelChangeRequest.RoomID = 0;
		levelChangeRequest.CutsceneToCall = ECutsceneType.CavesPast1_Camp;
		levelChangeRequest.AdditionalBlackScreenTime = 0.5f;
		levelChangeRequest.FadeOutTime = 0.25f;
		levelChangeRequest.FadeInTime = 0.25f;
		levelChangeRequest.ShouldPlayLevelSong = true;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
	}
}
