using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeSerene0 : CutsceneBase
{
	private const int SeykisStartX = 192;

	private const int StartY = 224;

	private const int MinibossStartX = 128;

	private readonly NPCBase _seykis;

	private readonly KeepWarCheveux _miniboss;

	public CutsceneLakeSerene0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.DoesFadeOutWhenSkipped = false;
		_seykis = NPCBase.FromArgumentAndLevel(_level, new Point(192, 224), -1, new ObjectTileSpecification(487)
		{
			Argument = 2
		});
		_seykis.CannotBeTalkedTo = true;
		ObjectTileSpecification objectSpec2 = new ObjectTileSpecification(416)
		{
			Argument = 1,
			IsFlippedHorizontally = true
		};
		_miniboss = new KeepWarCheveux(new Point(128, 224), _level, _level.GCM.SpKeepWarCheveux, -1, objectSpec2);
		_miniboss.MakeIntoMiniboss();
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 61;
		animationSpec.Length = 4;
		animationSpec.Type = EAnimationType.Once;
		animationSpec.Speed = 0.07f;
		AnimationSpec newAnim = animationSpec;
		_level.AddNPC(_seykis);
		_level.RequestAddObject(_miniboss);
		DoMiniBossDoorLock();
		AddWaitScript(0.5f);
		AddDelegateScript(DoMiniBossScream);
		AddScript(new ScriptAction(newAnim, _seykis));
		AddLevelScriptAction(new ScriptAction(EScriptActionType.ChangeColor, 0f, 1f, new Vector4(1f, 0.8f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _seykis
		});
		AddDialogue("cs_lse_0_sol_00");
		AddWaitScript(0.5f);
		AddDelegateScript(StartFight);
		_level.GameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Quartermaster), value: true);
	}

	private void DoMiniBossDoorLock()
	{
		bool flag = _level.GetNearestProtagonistPosition(Position).X < Position.X;
		_level.ToggleExits(isEnabled: false);
		_level.OpenAllBossDoors(-1f);
		_level.LockAllBossDoors(0.5f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Run, 0f, 0.3f, new Vector4(flag ? 1 : (-1), 0f, 0f, 0f))
		{
			DoesBlockQueue = true
		});
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
	}

	private void DoMiniBossScream()
	{
		_miniboss.DoSquawk();
	}

	private void StartFight()
	{
		_miniboss.ResumeMiniboss();
	}
}
