using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingAB5 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int FloorY = 192;

	private const int MessengerX = 408;

	private const int MarellaX = 320;

	private const int YorneX = 624;

	private const int LunaisWalk1X = 560;

	private const int LunaisWalk2X = 512;

	private const int LunaisWalk3X = 672;

	private const int LunaisWalk4X = 656;

	private const int LunaisWalk5X = 784;

	private const int MarellaWalk1X = 432;

	private const int MarellaWalk2X = 536;

	private readonly bool _isEndingA;

	private readonly bool _shouldPlayAltOutro;

	private readonly MarellaNPC _marella;

	private readonly MessengerNPC _messenger;

	private readonly YorneNPC _yorne;

	public CutsceneEndingAB5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, bool isEndingA)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_isEndingA = isEndingA;
		_shouldPlayAltOutro = _level.GameSave.GetSaveBool("IsPrinceDead") || _level.GameSave.GetSaveBool("IsTerrilisDead");
		AddLockCamera();
		AddCameraPan(new Point(496, 120), 0f, doesBlockQueue: false);
		_marella = new MarellaNPC(_level, new Point(320, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 11
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_messenger = new MessengerNPC(_level, new Point(408, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 14
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_yorne = new YorneNPC(_level, new Point(624, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 10
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_level.RequestAddObject(_marella);
		_level.RequestAddObject(_messenger);
		_level.RequestAddObject(_yorne);
	}

	internal override void DoCutscene()
	{
		AddPlaySong(EBGM.CsBirthday, doesStopOtherSong: false);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 237;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.PingPong;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 233;
		animationSpec2.Length = 3;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 235;
		animationSpec3.Length = 2;
		animationSpec3.Speed = 0.15f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 8;
		animationSpec4.Length = 5;
		animationSpec4.Speed = 0.15f;
		animationSpec4.Type = EAnimationType.Cycle;
		AnimationSpec newAnim4 = animationSpec4;
		Protagonist mainHero = _level.MainHero;
		AddHideFamiliar(0f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 2f,
			DoesBlockQueue = false,
			Arguments = new Vector4(560f, 192f, 0f, 0f)
		});
		AddWaitScript(1f);
		AddDialogue("cs_enda_5_mar_00");
		MoveCharacterToPosition(_marella, new Point(432, 192), doesBlockQueue: false, 0f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddDialogue("cs_enda_5_lun_01");
		MoveCharacterToPosition(_marella, new Point(536, 192), doesBlockQueue: true, 0f);
		AddDialogue("cs_enda_5_lun_02");
		AddDialogue("cs_enda_5_lun_03");
		AddDialogue("cs_enda_5_mar_04");
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = false
		});
		if (_isEndingA)
		{
			AddDialogue("cs_enda_5_lun_05");
			AddDialogue("cs_enda_5_lun_06");
		}
		else
		{
			AddDialogue("cs_endb_5_lun_00");
			AddDialogue("cs_endb_5_lun_01");
		}
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_enda_5_lun_07");
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = true
		});
		MovePlayerToPosition(new Point(512, 192), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _marella,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_enda_5_lun_08");
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(0.75f);
		MovePlayerToPosition(new Point(672, 192), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _marella,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddWaitScript(0.5f);
		MovePlayerToPosition(new Point(656, 192), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
		AddDialogue("cs_enda_5_lun_09");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _yorne,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_enda_5_yor_10");
		AddDialogue("cs_enda_5_lun_11");
		AddDialogue("cs_enda_5_lun_12");
		AddDialogue("cs_enda_5_lun_13");
		AddDialogue("cs_enda_5_lun_14");
		MovePlayerToPosition(new Point(784, 192), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		if (_shouldPlayAltOutro)
		{
			AddDelegateScript(base.StartLevelFadeOut);
			AddWaitScript(1.5f);
			AddAutoplayGhostDialogue("cs_end_ex_1_lun_00");
			AddSongFadeOut(5f, doesBlock: true);
			AddWaitScript(3.5f);
			AddDelegateScript(base.RollCredits);
		}
		else
		{
			AddSongFadeOut(5f, doesBlock: true);
			AddWaitScript(1.5f);
			AddRollCredits();
		}
	}
}
