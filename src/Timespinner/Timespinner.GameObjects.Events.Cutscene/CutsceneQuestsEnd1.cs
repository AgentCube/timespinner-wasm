using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneQuestsEnd1 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int CenterX = 400;

	private const int FloorY = 272;

	private const int NelisteStartX = 456;

	private const int SeykisStartX = 375;

	private const int EschemStartX = 355;

	private const int HaristelStartX = 329;

	private const int RamedaStartX = 437;

	private const int CameraX = 400;

	private const int CameraY = 200;

	private readonly AstrologerNPC _neliste;

	private readonly MedicNPC _rameda;

	private readonly QuartermasterNPC _seykis;

	private readonly SickSoldierNPC _eschem;

	private readonly CaptainNPC _haristel;

	public CutsceneQuestsEnd1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddCameraPan(new Point(400, 200), 0f, doesBlockQueue: false);
		_neliste = new AstrologerNPC(_level, new Point(456, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 0
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_seykis = new QuartermasterNPC(_level, new Point(375, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 2
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_eschem = new SickSoldierNPC(_level, new Point(355, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 4
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_eschem.Initialize();
		_rameda = new MedicNPC(_level, new Point(437, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 1
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_haristel = new CaptainNPC(_level, new Point(329, 272), -1, new ObjectTileSpecification(487)
		{
			Argument = 3
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_level.RequestAddObject(_eschem);
		_level.RequestAddObject(_seykis);
		_level.RequestAddObject(_haristel);
		_level.RequestAddObject(_neliste);
		_level.RequestAddObject(_rameda);
		_rameda.ChangeAnimation(41);
		_seykis.ChangeAnimation(65);
		_eschem.ChangeAnimation(87);
		_neliste.ChangeAnimation(16);
		_haristel.ChangeAnimation(120);
		_eschem.ChangeBboxDimensions(new Point(32, 30), new Point(0, 1));
		_eschem.SnapBboxToPosition();
	}

	internal override void DoCutscene()
	{
		WarpPlayerToPosition(new Point(472, 272));
		AddPlayerFaceRoomCenter();
		AddHideFamiliar(0f);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 244;
		animationSpec.Length = 1;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 212;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 0;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.11f;
		animationSpec3.Type = EAnimationType.Cycle;
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(1f);
		AddDialogue("cs_qend_sey_00");
		AddDialogue("cs_qend_har_01");
		AddDialogue("cs_qend_esc_02");
		AddDialogue("cs_qend_sey_03");
		AddDialogue("cs_qend_esc_04");
		AddDialogue("cs_qend_ram_05");
		AddDialogue("cs_qend_sey_06");
		AddDialogue("cs_qend_ram_07");
		AddDialogue("cs_qend_har_08");
		AddDialogue("cs_qend_har_09");
		AddDialogue("cs_qend_nel_10");
		AddDialogue("cs_qend_esc_11");
		AddDialogue("cs_qend_ram_12");
		AddDialogue("cs_qend_sey_13");
		AddDialogue("cs_qend_ram_14");
		AddDialogue("cs_qend_esc_15");
		AddDialogue("cs_qend_sey_16");
		AddDialogue("cs_qend_ram_17");
		AddDialogue("cs_qend_har_18");
		AddDialogue("cs_qend_lun_19");
		AddDialogue("cs_qend_har_20");
		AddDialogue("cs_qend_ram_21");
		AddDialogue("cs_qend_har_22");
		AddDialogue("cs_qend_lun_23");
		AddDialogue("cs_qend_lun_24");
		AddDialogue("cs_qend_har_25");
		AddDialogue("cs_qend_ram_26");
		AddDialogue("cs_qend_lun_27");
		AddDialogue("cs_qend_sey_28");
		AddDialogue("cs_qend_ram_29");
		AddDialogue("cs_qend_sey_30");
		AddDialogue("cs_qend_lun_31");
		AddDialogue("cs_qend_sek_32");
		AddDialogue("cs_qend_lun_33");
		AddDialogue("cs_qend_nel_34");
		AddDialogue("cs_qend_lun_35");
		AddDialogue("cs_qend_har_36");
		AddDialogue("cs_qend_nel_37");
		AddDialogue("cs_qend_lun_38");
		AddDialogue("cs_qend_nel_39");
		AddDialogue("cs_qend_sey_40");
		AddDialogue("cs_qend_ram_41");
		AddDialogue("cs_qend_nel_42");
		AddDialogue("cs_qend_ram_43");
		AddDialogue("cs_qend_sey_44");
		AddDialogue("cs_qend_har_45");
		AddDialogue("cs_qend_sey_46");
		AddDialogue("cs_qend_esc_47");
		AddDialogue("cs_qend_sey_48");
		AddDialogue("cs_qend_esc_49");
		AddDialogue("cs_qend_ram_50");
		AddDialogue("cs_qend_har_51");
		AddDialogue("cs_qend_sey_52");
		AddDialogue("cs_qend_all_53");
		AddWaitScript(1f);
		AddDialogue("cs_qend_nel_54");
		AddDialogue("cs_qend_lun_55");
		AddDialogue("cs_qend_nel_56");
		AddSongFadeOut(2f, doesBlock: false);
		TeleportToLevelAndRoom(3, 23, ECutsceneType.Misc4_QuestsEnd2);
	}
}
