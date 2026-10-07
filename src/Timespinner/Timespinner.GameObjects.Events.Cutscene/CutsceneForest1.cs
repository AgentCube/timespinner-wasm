using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest1 : CutsceneBase
{
	private const int StartX = 288;

	private const int StartY = 221;

	private const int EndX = 240;

	private const int Neliste1Xa = 48;

	private const int Neliste1Xb = 112;

	private const int Neliste1Y = 128;

	private const int Neliste2Xa = 144;

	private const int Neliste2Xb = 192;

	private const int Neliste2Y = 234;

	private const int Neliste3Xa = 272;

	private const int Neliste3Xb = 224;

	private const int Neliste3Y = 234;

	private const float PostFadeWait = 2f;

	private const float SleepingFadeOutTime = 1f;

	private const float SleepingBlackTime = 1f;

	private const float SleepingFadeInTime = 1f;

	private readonly AstrologerNPC _neliste;

	public CutsceneForest1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_level.RequestScreenFadeOut(0f, 0.5f, 0.75f, 0f);
		_neliste = new AstrologerNPC(_level, Point.Zero, -1, new ObjectTileSpecification(487))
		{
			IsFacingLeft = false
		};
		_level.RequestAddObject(_neliste);
	}

	private void DoSleepingFadeInOut()
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(1f, 1f, 1f, 0f)
		});
		AddWaitScript(1.5f);
	}

	internal override void DoCutscene()
	{
		_level.SetCameraToPlayerPosition();
		_level.SetCameraUpdateDisable(isDisabled: true);
		AddScript(new ScriptAction(EScriptActionType.Drowning, 0f, 15f, new Vector4(288f, 221f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesClearSameType = true
		});
		AddWaitScript(2f);
		DoSleepingFadeInOut();
		WarpCharacterToPosition(_neliste, new Point(48, 128));
		MoveCharacterToPosition(_neliste, new Point(112, 128), doesBlockQueue: false, 0.5f);
		AddWaitScript(2f);
		DoSleepingFadeInOut();
		WarpCharacterToPosition(_neliste, new Point(144, 234));
		AddDelegateScript(PlaceNelisteInWater);
		MoveCharacterToPosition(_neliste, new Point(192, 234), doesBlockQueue: false, 0.5f);
		AddWaitScript(2f);
		DoSleepingFadeInOut();
		WarpCharacterToPosition(_neliste, new Point(272, 234));
		AddScript(new ScriptAction(EScriptActionType.Drowning, 0f, 3f, new Vector4(288f, 221f, 240f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesClearSameType = true
		});
		MoveCharacterToPosition(_neliste, new Point(224, 234), doesBlockQueue: false, 0.5f);
		AddWaitScript(2f);
		FadeOut(0.5f);
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.Drowning,
			TargetType = EScriptTargetType.Player1,
			Arguments = new Vector4(288f, 221f, 0f, 1f),
			DoesClearSameType = true,
			DoesBlockQueue = true
		});
		TeleportToLevelAndRoom(3, 0, ECutsceneType.Forest2_WakeUp);
	}

	private void PlaceNelisteInWater()
	{
		_neliste.DoesNotMakeSplashesInWater = true;
		_neliste.Agility = 0.1f;
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 11;
		animationSpec.Length = 5;
		animationSpec.Speed = 0.2f;
		animationSpec.Type = EAnimationType.Cycle;
		AnimationSpec newAnimation = animationSpec;
		_neliste.ChangeAnimation(newAnimation);
	}
}
