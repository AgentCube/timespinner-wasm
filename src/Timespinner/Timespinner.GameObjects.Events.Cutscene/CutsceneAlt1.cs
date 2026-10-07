using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Bosses;
using Timespinner.GameObjects.Bosses.Emperor;
using Timespinner.GameObjects.Enemies;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneAlt1 : CutsceneBase
{
	private const int EmperorObjectIndex = 439;

	private const int EmperorVolArgument = 1;

	private const int AelanaObjectIndex = 435;

	private const int GateIndex = 481;

	private const int ShieldKnightObjectIndex = 397;

	private const int LancerArgument = 1;

	private const int LancerCount = 3;

	private const int VolStartX = 316;

	private const int AelanaStartX = 264;

	private const int FloorY = 208;

	private const int GateX = 168;

	private const int GateY = 144;

	private const int LancerStartX = 16;

	private const int LunaisWalk1X = 208;

	private const int CameraPan0X = 288;

	private const int CameraPan1X = 264;

	private const int CameraPanY = 120;

	private const int AelanaMoveX = 560;

	private const int AelanaMoveY = 192;

	private const int LancerTargetX = 168;

	private const int LancerOffsetX = 40;

	private const float LancerMovementSleepTime = 0.25f;

	private const float LancerMovementTime = 1f;

	private readonly EInventoryOrbType _equippedSpellType;

	private readonly EInventoryOrbType _equippedPassiveType;

	private readonly EmperorBoss _emperor;

	private readonly AelanaBoss _aelana;

	private readonly TimeGateEvent _timeGate;

	private readonly ViletianLancer[] _lancers = new ViletianLancer[3];

	public CutsceneAlt1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		base.DoesMakePlayerIdleAtStart = false;
		AddHidePlayer();
		AddHideFamiliar(0f);
		AddLockCamera();
		AddCameraPan(new Point(288, 120), 0f, doesBlockQueue: false);
		_emperor = new EmperorBoss(new Point(316, 208), _level, _level.GCM.SpEmperorVilete, -1, new ObjectTileSpecification(439)
		{
			Argument = 1
		})
		{
			IsFacingLeft = true
		};
		_aelana = new AelanaBoss(new Point(264, 208), _level, _level.GCM.SpAelana, -1, new ObjectTileSpecification(435))
		{
			IsSpawnedForCutscene = true,
			IsFacingLeft = false
		};
		Point inPosition2 = new Point(16, 208);
		SpriteSheet spCastleShieldKnight = _level.GCM.SpCastleShieldKnight;
		ObjectTileSpecification objectSpec2 = new ObjectTileSpecification(397)
		{
			Argument = 1
		};
		for (int i = 0; i < 3; i++)
		{
			ViletianLancer viletianLancer = new ViletianLancer(inPosition2, _level, spCastleShieldKnight, -1, objectSpec2);
			_lancers[i] = viletianLancer;
		}
		_timeGate = new TimeGateEvent(_level, new Point(168, 144), -1, new ObjectTileSpecification(481))
		{
			IsCameraLockedAfterJump = true
		};
		_level.RequestAddObject(_timeGate);
		_level.RequestAddObject(_aelana);
		_level.RequestAddObject(_emperor);
		_timeGate.Initialize();
		_emperor.InitializeMob();
		_aelana.InitializeMob();
		if (_level.MainHero is LunaisObj lunaisObj)
		{
			lunaisObj.HealAura(1000);
			_equippedSpellType = lunaisObj.EquippedSpellType;
			_equippedPassiveType = lunaisObj.EquipPassiveType;
			lunaisObj.ChangeEquippedSpell(EInventoryOrbType.Blue);
			lunaisObj.ChangeEquippedPassive(EInventoryOrbType.None);
		}
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_vol_ael_03");
		AddScript(new ScriptAction(new Vector2(264f, 120f), 1f, shouldBlock: false));
		AddDelegateScript(StartTimeGate);
		AddUnskippableWaitScript(1f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _aelana,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddUnskippableWaitScript(2.5f);
		MovePlayerToPosition(new Point(208, 208), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		AddDialogue("cs_vol_ter_04");
		AddDialogue("cs_vol_lun_05");
		AddDialogue("cs_vol_ter_06");
		AddDelegateScript(AddSoldiers);
		AddDialogue("cs_vol_lun_07");
		UnhideOrbs();
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.ChargeSpell
		});
		AddDialogue("cs_vol_ter_08");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddWaitScript(0.1f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.CastSpell
		});
		_aelana.AddLiftOffSequence();
		PlayScriptedSFX(ESFX.BossEmperorFightBegin, new Point(316, 208));
		AddDelegateScript(EmperorLiftOff);
		AddDialogue("cs_vol_lun_09");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_vol_lun_10");
		AddDialogue("cs_vol_lun_11");
		AddDialogue("cs_vol_ter_12");
		AddDialogue("cs_vol_lun_13");
		AddDialogue("cs_vol_lun_14");
		AddDialogue("cs_vol_lun_15");
		AddDialogue("cs_vol_ter_16");
		AddDialogue("cs_vol_lun_17");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_vol_lun_18");
		AddDelegateScript(MoveAelanaToExit);
		AddWaitScript(0.25f);
		AddScript(new ScriptAction(new Vector2(208f, 120f), 1f, shouldBlock: false));
		AddWaitScript(1f);
		AddUnlockCamera();
		AddUnhideFamiliar();
		AddDelegateScript(StartBattle);
	}

	private void StartTimeGate()
	{
		_timeGate.ChangeTargetLevel(11);
		_timeGate.IsActive = true;
	}

	private void StartBattle()
	{
		_emperor.StartBattle();
		_aelana.SilentKill();
		ViletianLancer[] lancers = _lancers;
		foreach (ViletianLancer viletianLancer in lancers)
		{
			viletianLancer.SilentKill();
		}
		if (_level.MainHero is LunaisObj lunaisObj)
		{
			if (_equippedSpellType != 0)
			{
				lunaisObj.ChangeEquippedSpell(_equippedSpellType);
			}
			if (_equippedPassiveType != 0)
			{
				lunaisObj.ChangeEquippedPassive(_equippedPassiveType);
			}
		}
	}

	private void EmperorLiftOff()
	{
		_emperor.ShowOrbs(doesShowAnimation: true);
		_emperor.TakeOffFromGround();
	}

	private void MoveAelanaToExit()
	{
		_aelana.CutsceneMoveToPoint(new Point(560, 192), 1.25f);
	}

	private void AddSoldiers()
	{
		float num = 0f;
		int num2 = 168;
		ViletianLancer[] lancers = _lancers;
		foreach (ViletianLancer viletianLancer in lancers)
		{
			_level.RequestAddObject(viletianLancer);
			viletianLancer.StartMoving(1f, num, num2);
			num += 0.25f;
			num2 -= 40;
		}
	}
}
