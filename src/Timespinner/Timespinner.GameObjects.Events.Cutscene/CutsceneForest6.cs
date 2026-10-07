using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest6 : CutsceneBase
{
	private const int SecondFloorY = 160;

	private const int MedicWarpX = 1084;

	private const int MedicWalkX = 1024;

	private const int LunaisWarpX = 1000;

	private const int LunaisWalkX2 = 976;

	private SickSoldierNPC _eschem;

	public CutsceneForest6(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesHideOrbsAutomatically = false;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 233;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 236;
		animationSpec2.Length = 1;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 8;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.15f;
		animationSpec3.Type = EAnimationType.Cycle;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 41;
		animationSpec4.Length = 1;
		animationSpec4.Type = EAnimationType.Once;
		AnimationSpec newAnim4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 42;
		animationSpec5.Length = 2;
		animationSpec5.Type = EAnimationType.Once;
		animationSpec5.Speed = 0.125f;
		AnimationSpec newAnim5 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 30;
		animationSpec6.Length = 5;
		animationSpec6.Speed = 0.125f;
		animationSpec6.Type = EAnimationType.Cycle;
		AnimationSpec newAnim6 = animationSpec6;
		Protagonist mainHero = _level.MainHero;
		NPCBase nPCBase = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Medic);
		NPCBase nPCBase2 = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.SickSoldier);
		_eschem = nPCBase2 as SickSoldierNPC;
		if (_eschem != null)
		{
			_eschem.CannotBeTalkedTo = true;
		}
		FadeOut(0.5f);
		AddWaitScript(0.25f);
		WarpPlayerToPosition(new Point(1000, 160));
		WarpCharacterToPosition(nPCBase, new Point(1084, 160));
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = nPCBase
		});
		AddScript(new ScriptAction(newAnim4, nPCBase));
		EschemWakeUp();
		AddDialogue("q_ram_3_ram_00");
		EschemCough();
		AddDialogue("q_ram_3_esc_01");
		AddDialogue("q_ram_3_ram_02");
		EschemCough();
		AddDialogue("q_ram_3_esc_03");
		AddDialogue("q_ram_3_ram_04");
		EschemThumbsUp();
		AddDialogue("q_ram_3_lun_05");
		EschemSleep();
		AddWaitScript(0.25f);
		AddScript(new ScriptAction(newAnim5, nPCBase)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim6, nPCBase)
		{
			DoesBlockQueue = false
		});
		AddDialogue("q_ram_3_ram_06");
		MoveCharacterToPosition(nPCBase, new Point(1024, 160), doesBlockQueue: true, 0f);
		AddDialogue("q_ram_3_ram_07");
		AddDialogue("q_ram_3_lun_08");
		AddDialogue("q_ram_3_ram_09");
		MoveCharacterToPosition(nPCBase, new Point(1084, 160), doesBlockQueue: false, 0f);
		AddWaitScript(0.75f);
		MovePlayerToPosition(new Point(976, 160), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(0.5f);
		AddDialogue("q_ram_3_lun_10");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = nPCBase
		});
	}

	private void EschemWakeUp()
	{
		if (_eschem != null)
		{
			_eschem.AddWakeUpAnimation();
		}
	}

	private void EschemCough()
	{
		if (_eschem != null)
		{
			_eschem.AddCoughAnimation();
		}
	}

	private void EschemSleep()
	{
		if (_eschem != null)
		{
			_eschem.AddFallAsleepAnimation();
		}
	}

	private void EschemThumbsUp()
	{
		if (_eschem != null)
		{
			_eschem.AddThumbsUpAnimation();
		}
	}
}
