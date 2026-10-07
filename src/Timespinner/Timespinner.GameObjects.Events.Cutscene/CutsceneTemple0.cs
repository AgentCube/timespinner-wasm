using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneTemple0 : CutsceneBase
{
	private const int SandmanTileID = 440;

	private const int CameraCenterX = 352;

	private const int CameraCenterY = 136;

	private const int LunaisWalkOffsetX1 = -24;

	private const int LunaisWalkOffsetX2 = 0;

	private const int SandmanX = 494;

	private const int SandmanY = 224;

	private readonly SandmanBoss _sandman;

	public CutsceneTemple0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.CutsceneDisappearType = ECutsceneDisappearType.Never;
		base.DoesFadeOutWhenSkipped = false;
		string saveKeyByBossType = BossClass.GetSaveKeyByBossType(EBossType.Sandman);
		if (_level.GameSave.GetSaveBool(saveKeyByBossType))
		{
			_level.GameSave.SetValue(saveKeyByBossType, value: false);
		}
		Monster monster = BossClass.CreateFromTileType(tilePosition: new Point(494, 224), level: _level, gcm: _level.GCM, newObjectID: -1, objectTileSpec: new ObjectTileSpecification(440));
		_sandman = monster as SandmanBoss;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!base.IsCutsceneTriggered)
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
	}

	internal override void DoCutscene()
	{
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
		Protagonist mainHero = _level.MainHero;
		MovePlayerToPosition(new Point(Position.X + -24, Position.Y), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
		AddDialogue("cs_tem_0_lun_00");
		AddDelegateScript(AddSandman);
		AddGhostDialogue("cs_tem_0_san_01");
		MovePlayerToPosition(new Point(Position.X, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddDialogue("cs_tem_0_lun_02");
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_tem_0_lun_03");
		AddGhostDialogue("cs_tem_0_san_04");
		AddDialogue("cs_tem_0_lun_05");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_tem_0_lun_06");
		AddLockCamera();
		AddCameraPan(new Point(352, 136), 0.5f, doesBlockQueue: true);
		AddDelegateScript(StartBattle);
		AddCameraPan(new Point(352, 136), 0f, doesBlockQueue: true);
	}

	private void StartBattle()
	{
		_sandman.StartBattle();
	}

	private void AddSandman()
	{
		_sandman.PlayCue(ESFX.BossSandmanAppear);
		_level.RequestAddObject(_sandman);
	}
}
