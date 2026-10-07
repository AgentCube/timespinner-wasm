using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.NPCs;
using Timespinner.GameObjects.NPCs.Misc;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingD2 : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int KneelStart = 16;

	private const int FloorY = 192;

	private const int NelisteStartX = 72;

	private const int NelisteWalkX1 = 184;

	private const int NelisteWalkX2 = 272;

	private const int NelisteWalkX3 = 240;

	private const int PortalStartX = -16;

	private const int ArcherStartX = 80;

	private static readonly Color AmuletGlowColor = new Color(200, 112, 160, 200);

	private readonly CutscenePropAppendage _amulet;

	private readonly AstrologerNPC _neliste;

	private readonly PastSickArcherNPC _sickArcher;

	public CutsceneEndingD2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_neliste = new AstrologerNPC(_level, new Point(72, 192), -1, new ObjectTileSpecification(487)
		{
			Argument = 0
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		_sickArcher = new PastSickArcherNPC(_level, new Point(80, 192), _level.GCM.SpMerchantCrow);
		_amulet = new CutscenePropAppendage(_neliste, new Point(4, 4), Point.Zero, _level, _level.GCM.SpItems)
		{
			BaseGlowColor = AmuletGlowColor
		};
		_amulet.ChangeAnimation(221);
		_neliste.ChangeAnimation(16);
		_neliste.Appendages.Add(_amulet);
		List<KeyValuePair<int, Point>> pairs = new List<KeyValuePair<int, Point>>
		{
			new KeyValuePair<int, Point>(16, new Point(-12, -9)),
			new KeyValuePair<int, Point>(17, new Point(-10, -9)),
			new KeyValuePair<int, Point>(18, new Point(-2, -13)),
			new KeyValuePair<int, Point>(19, new Point(-1, -14))
		};
		_amulet.AddOffsets(pairs);
		_amulet.Update(0f);
		_level.RequestAddObject(_sickArcher);
		_level.RequestAddObject(_neliste);
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 16;
		animationSpec.Length = 4;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 16;
		animationSpec2.Length = 4;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.IsInReverse = true;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 26;
		animationSpec3.Length = 2;
		animationSpec3.Speed = 0.1f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AddWaitScript(0.5f);
		AddDialogue("cs_endd_1_nel_00");
		AddScript(new ScriptAction(newAnim, _neliste)
		{
			DoesBlockQueue = true
		});
		AddDelegateScript(_amulet.StartGlowing);
		AddWaitScript(2f);
		AddDelegateScript(_amulet.StopGlowing);
		AddScript(new ScriptAction(newAnim2, _neliste)
		{
			DoesBlockQueue = true
		});
		AddDelegateScript(_amulet.Hide);
		AddWaitScript(1f);
		AddScript(new ScriptAction(newAnim3, _neliste)
		{
			DoesBlockQueue = true
		});
		MoveCharacterToPosition(_neliste, new Point(184, 192), doesBlockQueue: true, 0f);
		AddWaitScript(0.5f);
		AddDialogue("cs_endd_1_nel_01");
		MoveCharacterToPosition(_neliste, new Point(272, 192), doesBlockQueue: false, 0f);
		AddWaitScript(0.75f);
		PlayScriptedSFX(ESFX.LunaisTimeGateWarpin, new Point(-16, 192));
		AddWaitScript(0.75f);
		MoveCharacterToPosition(_neliste, new Point(240, 192), doesBlockQueue: false, 0f);
		AddWaitScript(0.5f);
		AddDialogue("cs_endd_1_nel_02");
		MoveCharacterToPosition(_neliste, new Point(-16, 192), doesBlockQueue: false, 0f);
		AddWaitScript(0.75f);
		TeleportToLevelAndRoom(17, 1, ECutsceneType.EndingD3_Past3);
	}
}
