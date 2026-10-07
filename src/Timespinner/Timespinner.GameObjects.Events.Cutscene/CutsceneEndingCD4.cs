using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingCD4 : CutsceneBase
{
	private const int NPCArgument = 487;

	private const int FloorY = 192;

	private const int SelenStartX = 144;

	private const int ChildStartX = 156;

	private const int ChildRunX = 176;

	private readonly SelenNPC _selen;

	private readonly ChildNPC _child;

	public CutsceneEndingCD4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddHidePlayer();
		AddCameraPan(new Point(240, 120), 0f, doesBlockQueue: false);
		_selen = new SelenNPC(_level, new Point(144, 192), -1);
		_child = new ChildNPC(_level, new Point(156, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 21
		});
		_level.RequestAddObject(_child);
		_level.RequestAddObject(_selen);
	}

	internal override void DoCutscene()
	{
		AddPlaySong(EBGM.CsBirthday, doesStopOtherSong: false);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 59;
		animationSpec.Length = 1;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnimation = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 56;
		animationSpec2.Length = 4;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.IsInReverse = true;
		AnimationSpec newAnim = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 0;
		animationSpec3.Length = 1;
		animationSpec3.Speed = 0.1f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 59;
		animationSpec4.Length = 3;
		animationSpec4.Speed = 0.1f;
		animationSpec4.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 59;
		animationSpec5.Length = 3;
		animationSpec5.Speed = 0.1f;
		animationSpec5.Type = EAnimationType.Once;
		animationSpec5.IsInReverse = true;
		AnimationSpec newAnim4 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 48;
		animationSpec6.Length = 1;
		animationSpec6.Speed = 0.1f;
		animationSpec6.Type = EAnimationType.Once;
		AnimationSpec newAnim5 = animationSpec6;
		AnimationSpec animationSpec7 = new AnimationSpec();
		animationSpec7.Start = 53;
		animationSpec7.Length = 2;
		animationSpec7.Speed = 0.1f;
		animationSpec7.Type = EAnimationType.Once;
		AnimationSpec newAnim6 = animationSpec7;
		_selen.ChangeAnimation(newAnimation);
		_child.ChangeAnimation(-1);
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(newAnim3, _selen)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_endc_4_chi_00");
		AddDialogue("cs_endc_4_sel_01");
		AddDialogue("cs_endc_4_sel_02");
		AddScript(new ScriptAction(newAnim4, _selen)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_endc_4_sel_03");
		AddScript(new ScriptAction(newAnim3, _selen)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_endc_4_sel_04");
		AddDialogue("cs_endc_4_sel_05");
		AddScript(new ScriptAction(newAnim4, _selen)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_endc_4_chi_06");
		AddScript(new ScriptAction(newAnim, _selen)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim5, _child)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, _selen)
		{
			DoesBlockQueue = true
		});
		MoveCharacterToPosition(_child, new Point(176, 192), doesBlockQueue: true, 0f);
		AddDialogue("cs_endc_4_sel_07");
		AddScript(new ScriptAction(newAnim6, _child)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_endc_4_chi_08");
		TeleportToLevelAndRoom(17, 13, ECutsceneType.EndingC5_Meyef);
	}
}
