using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutscenePrologue0 : CutsceneBase
{
	private const int NPCArgument = 487;

	private const int PrefabArgument = 491;

	private const int FloorY = 2992;

	private const int OffscreenFloorY = 2976;

	private const int NPCStartX = 0;

	private const int TableStartX = 216;

	private const int RockStartX = 248;

	private const int CameraX = 216;

	private const int CameraStartY = 120;

	private const int CameraMove1Y = 2624;

	private const int CameraEndY = 2920;

	private const int YorneWalk1X = 152;

	private const int YorneWalk2X = 192;

	private const int MarellaWalk1X = 204;

	private const int MarellaWalk2X = 272;

	private const int MarellaWalk3X = 288;

	private const int PlayerStartX = 312;

	private const int SelenStartX = 480;

	private readonly MarellaNPC _marellaNPC;

	private readonly YorneNPC _yorneNPC;

	private readonly EnvPrefabProTable _table;

	private readonly EnvPrefabProRock _rock;

	public CutscenePrologue0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.CutsceneDisappearType = ECutsceneDisappearType.Never;
		FadeOut(0f, 0.5f, 0f);
		AddLockCamera();
		AddCameraPan(new Point(216, 120), 0f, doesBlockQueue: false);
		_marellaNPC = new MarellaNPC(_level, new Point(0, 2976), -1, new ObjectTileSpecification(487)
		{
			Argument = 11
		});
		_yorneNPC = new YorneNPC(_level, new Point(0, 2976), -1, new ObjectTileSpecification(487)
		{
			Argument = 10
		});
		_table = new EnvPrefabProTable(_level, new Point(216, 2992), -1, new ObjectTileSpecification(491)
		{
			Argument = 1
		}, EEnvironmentPrefabType.L0_Table);
		_rock = new EnvPrefabProRock(_level, new Point(248, 2992), -1, new ObjectTileSpecification(491)
		{
			Argument = 2
		}, EEnvironmentPrefabType.L0_Rock)
		{
			TargetYorne = _yorneNPC
		};
		SelenNPC newObject = new SelenNPC(_level, new Point(480, 2976), -1)
		{
			IsFacingLeft = true
		};
		_marellaNPC.ToggleCake();
		_level.RequestAddObject(_rock);
		_level.RequestAddObject(_marellaNPC);
		_level.RequestAddObject(_yorneNPC);
		_level.RequestAddObject(_table);
		_level.RequestAddObject(newObject);
		_table.Initialize();
		_rock.Initialize();
		_level.RequestScreenFadeOut(0f, 0.5f, 0.5f, 0f);
	}

	internal override void DoCutscene()
	{
		InstantLevelFade();
		WarpPlayerToPosition(new Point(312, 2992));
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 237;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.PingPong;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 8;
		animationSpec2.Length = 5;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Cycle;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 242;
		animationSpec3.Length = 1;
		animationSpec3.Speed = 0f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 240;
		animationSpec4.Length = 3;
		animationSpec4.Speed = 0.1f;
		animationSpec4.Type = EAnimationType.Once;
		animationSpec4.IsInReverse = true;
		AnimationSpec newAnim4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 221;
		animationSpec5.Length = 7;
		animationSpec5.Speed = 0.1f;
		animationSpec5.Type = EAnimationType.Once;
		AnimationSpec newAnim5 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 109;
		animationSpec6.Length = 5;
		animationSpec6.Speed = 0.1f;
		animationSpec6.Type = EAnimationType.Once;
		AnimationSpec newAnim6 = animationSpec6;
		AnimationSpec animationSpec7 = new AnimationSpec();
		animationSpec7.Start = 0;
		animationSpec7.Length = 5;
		animationSpec7.Speed = 0.11f;
		animationSpec7.Type = EAnimationType.Cycle;
		AnimationSpec newAnim7 = animationSpec7;
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
		AddAutoplayGhostDialogue("cs_pro_area_00");
		AddDelegateScript(base.StartLevelFadeIn);
		AddCameraPan(new Point(216, 120), new Point(216, 2624), 5f, doesBlockQueue: true, ECameraScriptPanType.Cos);
		AddScript(new ScriptAction(EBGM.CsBirthday));
		AddCameraPan(new Point(216, 2624), new Point(216, 2920), 0.8f, doesBlockQueue: true, ECameraScriptPanType.Sine);
		MoveCharacterToPosition(_marellaNPC, new Point(204, 2992), doesBlockQueue: true, 0f);
		AddDelegateScript(_marellaNPC.SetDownCake);
		AddWaitScript(0.28f);
		AddDelegateScript(_table.AddCake);
		AddWaitScript(0.5f);
		AddDelegateScript(_marellaNPC.ToggleCake);
		MoveCharacterToPosition(_marellaNPC, new Point(272, 2992), doesBlockQueue: true, 0f);
		AddDialogue("cs_pro_0_mar_00");
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_proa_0_lun_00");
		AddDialogue("cs_proa_0_mar_01");
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_pro_0_lun_01");
		AddDialogue("cs_pro_0_mar_02");
		AddDialogue("cs_pro_0_mar_03");
		AddDialogue("cs_prob_0_yor_00");
		MoveCharacterToPosition(_yorneNPC, new Point(152, 2992), doesBlockQueue: false, 0f);
		AddWaitScript(1f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _marellaNPC,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_pro_0_yor_04");
		AddDialogue("cs_pro_0_lun_05");
		AddDialogue("cs_pro_0_yor_06");
		AddDialogue("cs_pro_0_mar_07");
		AddDelegateScript(_yorneNPC.CrossArms);
		AddDialogue("cs_pro_0_lun_08");
		AddDelegateScript(_yorneNPC.Spit);
		AddWaitScript(0.25f);
		AddDelegateScript(_yorneNPC.EmitSpit);
		AddDialogue("cs_pro_0_yor_09");
		AddDialogue("cs_pro_0_lun_10");
		AddScript(new ScriptAction(newAnim5, mainHero)
		{
			DoesBlockQueue = false
		});
		PlayScriptedSFX(ESFX.CsPrologueRock);
		AddDelegateScript(_rock.Levitate);
		AddWaitScript(1f);
		AddWaitScript(0.25f);
		AddDelegateScript(_rock.Shoot);
		AddScript(new ScriptAction(newAnim6, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim7, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_proc_0_yor_11");
		MoveCharacterToPosition(_yorneNPC, new Point(192, 2992), doesBlockQueue: false, 0f);
		AddDialogue("cs_pro_0_yor_11");
		AddDialogue("cs_pro_0_lun_12");
		AddDelegateScript(_yorneNPC.DoTableFlip);
		AddWaitScript(0.25f);
		PlayScriptedSFX(ESFX.CsPrologueTableFlip);
		AddDelegateScript(_table.FlipTable);
		AddDelegateScript(_marellaNPC.ToggleMoonWalking);
		AddWaitScript(0.1f);
		MoveCharacterToPosition(_marellaNPC, new Point(288, 2992), doesBlockQueue: false, 0f);
		AddWaitScript(1.15f);
		MoveCharacterToPosition(_yorneNPC, new Point(0, 2992), doesBlockQueue: true, 0f);
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _marellaNPC,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_pro_0_mar_13");
		AddDialogue("cs_prod_0_lun_14");
		AddDialogue("cs_pro_0_lun_14");
		AddDialogue("cs_pro_0_mar_15");
		AddDelegateScript(_marellaNPC.ToggleMoonWalking);
		MoveCharacterToPosition(_marellaNPC, new Point(0, 2992), doesBlockQueue: false, 0f);
		AddCameraPan(new Point(312, 2920), 1.25f, doesBlockQueue: true);
		AddWaitScript(0.01f);
		AddUnlockCamera();
		AddDelegateScript(CleanUpCharacters);
	}

	private void CleanUpCharacters()
	{
		_yorneNPC.RemoveNPC(0f);
		_marellaNPC.RemoveNPC(2f);
	}
}
