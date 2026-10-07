using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest2 : CutsceneBase
{
	private const int StartY = 160;

	private const int LunaisStartX = 176;

	private const int NelisteStartX = 168;

	private const int NelisteStandXa = 88;

	private const int NelisteStandXb = 136;

	private const int LunaisDeathIndex = 204;

	private const int KneelStart = 16;

	private static readonly Color AmuletGlowColor = new Color(112, 200, 160, 200);

	private readonly AnimationSpec _deadAnimation;

	private readonly CutscenePropAppendage _amulet;

	private readonly NPCBase _neliste;

	private readonly Protagonist _lunais;

	public CutsceneForest2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		_deadAnimation = new AnimationSpec
		{
			Start = 204,
			Length = 1
		};
		_lunais = _level.MainHero;
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		_level.GameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Astrologer), value: true);
		_neliste = NPCBase.FromArgumentAndLevel(_level, new Point(168, 160), -1, new ObjectTileSpecification(487));
		_amulet = new CutscenePropAppendage(_neliste, new Point(4, 4), Point.Zero, _level, _level.GCM.SpItems)
		{
			BaseGlowColor = AmuletGlowColor
		};
		_amulet.ChangeAnimation(223);
		_neliste.IsFacingLeft = false;
		_neliste.ChangeAnimation(16);
		_neliste.Appendages.Add(_amulet);
		List<KeyValuePair<int, Point>> pairs = new List<KeyValuePair<int, Point>>
		{
			new KeyValuePair<int, Point>(16, new Point(10, -10)),
			new KeyValuePair<int, Point>(17, new Point(8, -10)),
			new KeyValuePair<int, Point>(18, new Point(-2, -13)),
			new KeyValuePair<int, Point>(19, new Point(-1, -14))
		};
		_amulet.AddOffsets(pairs);
		_level.RequestAddObject(_neliste);
		_level.IsMufflingPlayerSFX = true;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 205;
		animationSpec.Length = 1;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 244;
		animationSpec2.Length = 1;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 212;
		animationSpec3.Length = 2;
		animationSpec3.Speed = 0.1f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 0;
		animationSpec4.Length = 5;
		animationSpec4.Speed = 0.11f;
		animationSpec4.Type = EAnimationType.Cycle;
		AnimationSpec newAnim4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 16;
		animationSpec5.Length = 4;
		animationSpec5.Speed = 0.1f;
		animationSpec5.Type = EAnimationType.Once;
		AnimationSpec newAnim5 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 0;
		animationSpec6.Length = 5;
		animationSpec6.Speed = 0.125f;
		animationSpec6.Type = EAnimationType.Cycle;
		AnimationSpec newAnim6 = animationSpec6;
		WarpPlayerToPosition(new Point(176, 160));
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		AddWaitScript(0.1f);
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		AddWaitScript(0.4f);
		AddScript(new ScriptAction(_deadAnimation, _lunais));
		AddDialogue("cs_for_0_lun_00");
		AddDialogue("cs_for_0_lun_01");
		AddDialogue("cs_for_0_lun_02");
		AddDialogue("cs_for_0_lun_03");
		AddScript(new ScriptAction(newAnim5, _neliste)
		{
			DoesBlockQueue = true
		});
		AddDelegateScript(_amulet.StartGlowing);
		AddWaitScript(1f);
		FadeOut(1f, 1f, 1f);
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(newAnim6, _neliste));
		WarpCharacterToPosition(_neliste, new Point(88, 160));
		AddDelegateScript(_amulet.Hide);
		AddDelegateScript(StartSong);
		AddWaitScript(1.5f);
		AddScript(new ScriptAction(newAnim, _lunais)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, _lunais)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(0.5f);
		AddDialogue("cs_for_0_lun_04");
		MoveCharacterToPosition(_neliste, new Point(136, 160), doesBlockQueue: false, 0f);
		AddDialogue("cs_for_0_nel_05");
		AddDialogue("cs_for_0_lun_06");
		AddDialogue("cs_for_0_nel_07");
		AddDialogue("cs_for_0_lun_08");
		AddDialogue("cs_for_0_nel_09");
		AddDialogue("cs_for_0_lun_10");
		AddDialogue("cs_for_0_nel_11");
		AddDialogue("cs_for_0_lun_12");
		AddDialogue("cs_for_0_nel_13");
		AddDialogue("cs_for_0_lun_14");
		AddDialogue("cs_for_0_lun_15");
		AddDialogue("cs_for_0_nel_16");
		AddDialogue("cs_for_0_lun_17");
		AddDialogue("cs_for_0_nel_18");
		AddDialogue("cs_for_0_lun_19");
		AddDialogue("cs_for_0_nel_20");
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(newAnim3, _lunais)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim4, _lunais)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_for_0_nel_21");
		AddDialogue("cs_for_0_lun_22");
		AddDialogue("cs_for_0_nel_23");
		AddDialogue("cs_for_0_lun_24");
		AddDialogue("cs_for_0_nel_25");
		AddUnhideFamiliar();
	}

	private void StartSong()
	{
		_level.PlayLevelSong();
		_level.IsMufflingPlayerSFX = false;
	}
}
