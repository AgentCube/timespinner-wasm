using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Emperor;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneAlt0 : CutsceneBase
{
	private const int EmperorLandX = 392;

	private const int FloorY = 212;

	private const int CameraPanX = 344;

	private const int CameraPanY = 120;

	private EmperorBoss _emperor;

	public CutsceneAlt0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesMakePlayerIdleAtStart = false;
		AddHideFamiliar(0f);
		AddLockCamera();
	}

	public override void Initialize()
	{
		base.Initialize();
		IEnumerable<Monster> enemiesOfType = _level.GetEnemiesOfType(EEnemyTileType.EmperorBoss);
		foreach (Monster item in enemiesOfType)
		{
			if (item is EmperorBoss emperor)
			{
				_emperor = emperor;
				break;
			}
		}
	}

	internal override void DoCutscene()
	{
		AddDelegateScript(EmperorLiftOff);
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(new Vector2(344f, 120f), 1f, shouldBlock: false));
		AddDialogue("cs_nuv_nuv_02");
		AddDialogue("cs_nuv_lun_03");
		AddDialogue("cs_nuv_nuv_04");
		AddDialogue("cs_nuv_lun_05");
		AddDialogue("cs_nuv_nuv_06");
		AddDialogue("cs_nuv_lun_07");
		AddDialogue("cs_nuv_nuv_08");
		AddDialogue("cs_nuv_lun_09");
		AddDialogue("cs_nuv_nuv_10");
		PlayScriptedSFX(ESFX.BossEmperorOrbsVanish, new Point(392, 212));
		AddDelegateScript(EmperorLand);
		AddWaitScript(1f);
		AddDialogue("cs_nuv_nuv_11");
		AddDialogue("cs_nuv_lun_12");
		AddDialogue("cs_nuv_nuv_13");
		AddDialogue("cs_nuv_lun_14");
		AddDialogue("cs_nuv_lun_15");
		AddDialogue("cs_nuv_lun_16");
		AddDialogue("cs_nuv_nuv_17");
		AddDialogue("cs_nuv_lun_18");
		AddDialogue("cs_nuv_nuv_19");
		AddDialogue("cs_nuv_lun_20");
		AddDialogue("cs_nuv_nuv_21");
		AddDialogue("cs_nuv_lun_22");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_nuv_lun_23");
		PlayScriptedSFX(ESFX.BossEmperorFightBegin, new Point(392, 212));
		AddDelegateScript(EmperorLiftOff);
		AddScript(new ScriptAction(new Vector2(Position.X, Position.Y), 1f, shouldBlock: false));
		AddWaitScript(1f);
		AddUnlockCamera();
		AddUnhideFamiliar();
		AddDelegateScript(StartBattle);
	}

	private void StartBattle()
	{
		if (_emperor != null)
		{
			_emperor.StartBattle();
		}
	}

	private void EmperorLiftOff()
	{
		if (_emperor != null)
		{
			_emperor.ShowOrbs(doesShowAnimation: true);
			_emperor.TakeOffFromGround();
		}
	}

	private void EmperorLand()
	{
		if (_emperor != null)
		{
			_emperor.HideOrbs(doesShowAnimation: true, isSilent: true);
			_emperor.LandAtLocation(new Point(392, 212), 1f, shouldDoLandingAnimation: true);
		}
	}
}
