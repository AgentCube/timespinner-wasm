using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

public abstract class CutsceneBase : GameEvent
{
	internal enum ECutsceneTriggerType
	{
		Called,
		Instant,
		WallTrigger
	}

	internal enum ECutsceneDisappearType
	{
		WhenTriggered,
		WhenBossIsDead,
		Never
	}

	internal enum ECutsceneType
	{
		None = 0,
		Prologue0_Start = 1,
		Prologue1_Elders = 2,
		Prologue2_Forest = 3,
		Prologue3_Temple = 4,
		Prologue4_TutorialM = 5,
		Prologue5_TutorialS = 6,
		LakeDesolation0_Warp = 10,
		LakeDesolation1_Entrance = 11,
		LakeDesolation2_Ship = 12,
		LakeDesolation3_City = 13,
		City0_Scientists = 20,
		City1_Frame = 21,
		City2_Spindle = 22,
		City3_Warp = 23,
		City4_Meyef = 24,
		City5_WarTerminal = 25,
		City6_TurnAround = 26,
		Forest0_Warp = 30,
		Forest1_Pond = 31,
		Forest2_WakeUp = 32,
		Forest3_Haristel = 33,
		Forest4_LeaveCamp = 34,
		Forest5_RamedaQ2End = 35,
		Forest6_RamedaQ4Start = 36,
		Forest7_RamedaQ4End = 37,
		Forest8_EscortStart = 38,
		Forest9_EscortEnd = 39,
		Keep0_Demons0 = 50,
		Keep1_Demons1 = 51,
		LakeSerene0_Seykis = 70,
		LakeSerene1_SeykisEnd = 71,
		LakeSerene2_Warp = 72,
		LakeSerene3_VileteSaved = 73,
		CavesPast0_Rameda = 80,
		CavesPast1_Camp = 81,
		CavesPast2_MawDoor = 82,
		CavesPast3_MawSuck = 83,
		CavesPast4_MawSpit = 84,
		CavesPast5_MawDie = 85,
		CavesPast6_MawBoom = 86,
		Hangar0_Blocked = 100,
		Hangar1_PostWarp = 101,
		Lab0_Scientists = 110,
		Lab1_Gear1 = 111,
		Lab2_Gear2 = 112,
		Lab3_Glass = 113,
		Lab4_Documents = 114,
		EmpTower0_Door = 120,
		EmpTower1_Win = 121,
		EmpTower2_Throne = 122,
		EmpTower3_Exit = 123,
		EmpTower4_DoorAlt = 124,
		EmpTower5_After = 125,
		Alt0_Nuvius = 130,
		Alt1_Vol = 131,
		Alt2_Win = 132,
		Alt3_Teleport = 133,
		DarkForest0_Start = 150,
		DarkForest1_End = 151,
		Temple0_Boss = 160,
		Temple1_BossEnd = 161,
		Temple2_End = 162,
		Temple_3_Blocked = 163,
		EndingA0_Present0 = 200,
		EndingA1_Present1 = 201,
		EndingA2_Past0 = 202,
		EndingA3_Past1 = 203,
		EndingA4_Present2 = 204,
		EndingA5_Winderia0 = 205,
		EndingB0_Present0 = 210,
		EndingB1_Present1 = 211,
		EndingB2_Past0 = 212,
		EndingB3_Past1 = 213,
		EndingB4_Winderia0 = 214,
		EndingC0_Present0 = 220,
		EndingC1_Past0 = 221,
		EndingC2_Past1 = 222,
		EndingC3_Present1 = 223,
		EndingC4_Winderia0 = 224,
		EndingC5_Meyef = 225,
		EndingC2_Past1B = 226,
		EndingD0_Past0 = 230,
		EndingD1_Past1 = 231,
		EndingD2_Past2 = 232,
		EndingD3_Past3 = 233,
		EndingD4_Present0 = 234,
		EndingD5_Winderia0 = 235,
		EndingD6_Meyef = 236,
		Misc0_Revive = 240,
		Misc1_DreamGate = 241,
		Misc2_Gyre = 242,
		Misc3_QuestsEnd1 = 243,
		Misc4_QuestsEnd2 = 244,
		Misc5_QuestsEnd3 = 245,
		Misc6_Gear1 = 246,
		Misc7_Gear2 = 247,
		Misc8_Gear3 = 248,
		Misc9_HaristelMaw = 249
	}

	private const int DefaultWallTriggerWidth = 16;

	private const string CutsceneTriggeredSaveKeyFormat = "Cutscene_{0}";

	private bool _isDrawingLevelFade;

	private bool _isFadingIn;

	private float _levelFadeTimer;

	private float _timeForLevelFade;

	private Color _levelFadeColor;

	private Background _levelFadeForeground;

	internal bool DoesHideOrbsAutomatically { get; set; }

	internal bool DoesHideOrbsShowAnimation { get; set; }

	internal bool DoesMakePlayerIdleAtStart { get; set; }

	internal bool DoesFadeOutWhenSkipped { get; set; }

	internal bool IsCutsceneTriggered { get; set; }

	internal bool IsCreatedAfterWarp { get; set; }

	internal bool IsWarpingAtEndOfCutscene { get; set; }

	internal int WallTriggerWidth { get; set; }

	internal ECutsceneTriggerType CutsceneTriggerType { get; set; }

	internal ECutsceneType CutsceneType { get; set; }

	internal ECutsceneDisappearType CutsceneDisappearType { get; set; }

	internal EBossType BossTypeLinkedTo { get; set; }

	internal string CutsceneTriggeredSaveKey => $"Cutscene_{(int)CutsceneType}";

	internal CutsceneBase(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.IsAffectedByTime = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_doesDrawSpriteAndAppendages = false;
		DoesHideOrbsAutomatically = true;
		DoesHideOrbsShowAnimation = true;
		DoesMakePlayerIdleAtStart = true;
		DoesFadeOutWhenSkipped = true;
		CutsceneDisappearType = ECutsceneDisappearType.WhenTriggered;
		WallTriggerWidth = 16;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, WallTriggerWidth, WallTriggerWidth);
	}

	internal virtual bool AreTriggerConditionsMet()
	{
		return true;
	}

	public override void Initialize()
	{
		base.Initialize();
		switch (CutsceneDisappearType)
		{
		case ECutsceneDisappearType.Never:
			IsCutsceneTriggered = false;
			break;
		case ECutsceneDisappearType.WhenBossIsDead:
			IsCutsceneTriggered = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(BossTypeLinkedTo));
			break;
		default:
			IsCutsceneTriggered = _level.GameSave.GetSaveBool(CutsceneTriggeredSaveKey);
			break;
		}
		if (!IsCutsceneTriggered && AreTriggerConditionsMet())
		{
			if (CutsceneTriggerType == ECutsceneTriggerType.Instant)
			{
				StartCutscene();
			}
			else
			{
				if (CutsceneTriggerType != ECutsceneTriggerType.WallTrigger)
				{
					return;
				}
				Point point = new Point(Position.X / 16, Position.Y / 16);
				Point key = new Point(point.X, point.Y - 1);
				for (int num = key.Y; num >= 0; num--)
				{
					key = new Point(key.X, num);
					if (_level.SolidTiles.ContainsKey(key))
					{
						Tile tile = _level.SolidTiles[key];
						if (tile.Type == ETileType.Solid)
						{
							break;
						}
					}
				}
				Bbox = new Rectangle(Position.X, Position.Y, WallTriggerWidth, (point.Y - key.Y) * 16);
			}
		}
		else if (CutsceneTriggerType != 0)
		{
			SilentKill();
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (!IsCutsceneTriggered && CutsceneTriggerType == ECutsceneTriggerType.WallTrigger && depth != Vector2.Zero)
		{
			if (CutsceneDisappearType != 0 || !_level.GameSave.GetSaveBool(CutsceneTriggeredSaveKey) || CutsceneDisappearType == ECutsceneDisappearType.Never)
			{
				StartCutscene();
				result = true;
			}
			else
			{
				IsCutsceneTriggered = true;
			}
		}
		return result;
	}

	internal void StartCutscene()
	{
		IsCutsceneTriggered = true;
		_level.GameSave.SetValue(CutsceneTriggeredSaveKey, value: true);
		ScriptAction scriptAction = new ScriptAction();
		scriptAction.ScriptType = EScriptType.CutsceneStart;
		scriptAction.DoesBlockQueue = false;
		ScriptAction scriptAction2 = scriptAction;
		if (DoesFadeOutWhenSkipped)
		{
			scriptAction2.Arguments = new Vector4(1f, 0f, 0f, 0f);
		}
		AddLevelScriptAction(scriptAction2);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		if (DoesMakePlayerIdleAtStart)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.Idle,
				ActionTimer = 0.1f,
				DoesBlockQueue = true
			});
		}
		if (DoesHideOrbsAutomatically)
		{
			HideOrbs();
		}
		DoCutscene();
		EndCutscene();
	}

	internal abstract void DoCutscene();

	internal void EndCutscene()
	{
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.03f,
			DoesBlockQueue = true
		});
		if (DoesHideOrbsAutomatically)
		{
			UnhideOrbs();
		}
	}

	internal void AddScript(ScriptAction newScript)
	{
		_level.AddScript(newScript);
	}

	internal void HideOrbs()
	{
		bool flag = DoesHideOrbsShowAnimation && !IsCreatedAfterWarp;
		Vector4 arguments = new Vector4(0f, (!flag) ? 1 : 0, 0f, 0f);
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon,
			Arguments = arguments
		});
	}

	internal void UnhideOrbs()
	{
		bool flag = !IsWarpingAtEndOfCutscene;
		Vector4 arguments = new Vector4(1f, (!flag) ? 1 : 0, 0f, 0f);
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon,
			Arguments = arguments
		});
	}

	internal void MovePlayerToPosition(Point targetPosition, bool shouldFaceLeftAfter, bool shouldStandFancyAfter)
	{
		bool flag = _level.GetPlayerPosition().X > targetPosition.X == !shouldFaceLeftAfter;
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 2f,
			DoesBlockQueue = true,
			DoesClearSameType = true,
			Arguments = new Vector4(targetPosition.X, targetPosition.Y, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.1f,
			DoesBlockQueue = true
		});
		if (shouldStandFancyAfter)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.FancyIdle,
				DoesBlockQueue = false
			});
		}
		if (flag)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.LookDirection,
				ActionTimer = 0.25f,
				DoesBlockQueue = true,
				Arguments = new Vector4((!shouldFaceLeftAfter) ? 1 : (-1), 0f, 0f, 0f)
			});
		}
	}

	internal void MoveCharacterToPosition(Alive character, Point targetPosition, bool doesBlockQueue, float sleepTime)
	{
		AddScript(new ScriptAction
		{
			ScriptTarget = character,
			TargetType = EScriptTargetType.Specified,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 2f,
			SleepTime = sleepTime,
			DoesBlockQueue = doesBlockQueue,
			DoesClearSameType = true,
			Arguments = new Vector4(targetPosition.X, targetPosition.Y, 0f, 0f)
		});
	}

	internal void AddPlayerFaceRoomCenter()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			int num = _level.RoomSize.X / 2;
			bool flag = mainHero.Position.X > num;
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.LookDirection,
				ActionTimer = 0.01f,
				DoesBlockQueue = true,
				Arguments = new Vector4((!flag) ? 1 : (-1), 0f, 0f, 0f)
			});
		}
	}

	internal void AddPlayerFaceDirection(bool isFacingLeft, float sleepTime)
	{
		AddScript(new ScriptAction(EScriptActionType.LookDirection, sleepTime, 0f, new Vector4((!isFacingLeft) ? 1 : (-1), 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
	}

	internal void AddCharacterFaceDirection(Alive character, bool isFacingLeft, float sleepTime)
	{
		AddScript(new ScriptAction(EScriptActionType.LookDirection, sleepTime, 0f, new Vector4((!isFacingLeft) ? 1 : (-1), 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = character
		});
	}

	internal void FadeOut(float timeToFade)
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(timeToFade, 0f, 0f, 0f)
		});
		AddUnskippableWaitScript(timeToFade);
	}

	internal void FadeOut(float timeToFadeOut, float blackTime, float timeToFadeIn)
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(timeToFadeOut, blackTime, timeToFadeIn, 0f)
		});
		AddUnskippableWaitScript(timeToFadeOut);
	}

	internal void AddScreenFlash(float duration, float amplitude, float frequency)
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.ScreenFlash,
			Arguments = new Vector4(duration, amplitude, frequency, 0f)
		});
	}

	internal void AddScreenShake(Vector2 dimensions, float time, float frequency)
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.ScreenShake,
			Arguments = new Vector4(dimensions.X, dimensions.Y, time, frequency)
		});
	}

	internal void WarpPlayerToPosition(Point position)
	{
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.WarpToPoint,
			Arguments = new Vector4(position.X, position.Y, 0f, 0f)
		});
	}

	internal void WarpCharacterToPosition(Alive character, Point position)
	{
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = character,
			ActionType = EScriptActionType.WarpToPoint,
			Arguments = new Vector4(position.X, position.Y, 0f, 0f)
		});
	}

	internal void TeleportToLevelAndRoom(int level, int room, ECutsceneType cutscene)
	{
		IsWarpingAtEndOfCutscene = true;
		FadeOut(0.5f);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = level;
		levelChangeRequest.RoomID = room;
		levelChangeRequest.CutsceneToCall = cutscene;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
	}

	internal void PlayScriptedSFX(ESFX sfx)
	{
		AddLevelScriptAction(new ScriptAction(sfx, new Point(-1, -1)));
	}

	internal void PlayScriptedSFX(ESFX sfx, Point position)
	{
		AddLevelScriptAction(new ScriptAction(sfx, position));
	}

	internal void AddPlaySong(EBGM song, bool doesStopOtherSong)
	{
		AddLevelScriptAction(new ScriptAction(song, shouldForceRestart: false, doesStopOtherSong));
	}

	internal void AddSongFadeOut(float timeToFade, bool doesBlock)
	{
		AddLevelScriptAction(new ScriptAction(EScriptType.FadeOutSong, 0f, 0f, doesBlock, new Vector4(timeToFade, 0f, 0f, 0f)));
	}

	internal void AddSummonMeyef()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Summon));
	}

	internal void AddSummonMeyefSilent()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Summon)
		{
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
	}

	internal void AddDismissMeyef()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Dismiss));
	}

	internal void AddDismissMeyefSilent()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Dismiss)
		{
			Arguments = new Vector4(0f, 1f, 0f, 0f)
		});
	}

	internal void AddMeyefFlyTo(Point point, float duration, bool doesBlock)
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.CutsceneFlyTo)
		{
			ActionTimer = duration,
			Arguments = new Vector4(point.X, point.Y, 0f, 0f),
			DoesBlockQueue = doesBlock
		});
	}

	internal void AddMeyefFlyInFrontOfPlayer(float duration, bool doesBlock)
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.CutsceneFlyInFrontOfPlayer)
		{
			ActionTimer = duration,
			DoesBlockQueue = doesBlock
		});
	}

	internal void AddMeyefFlyAround(Point point, float duration, float radius)
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.CutsceneFlyAround)
		{
			ActionTimer = duration,
			Arguments = new Vector4(point.X, point.Y, radius, 0f)
		});
	}

	internal void AddMeyefMew()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.MeyefMew));
		AddWaitScript(0.1f);
		PlayScriptedSFX(ESFX.MeyefMeow, _level.GetFamiliarPosition());
		AddWaitScript(0.5f);
	}

	internal void AddMeyefPurr()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.MeyefMew));
		AddWaitScript(0.1f);
		PlayScriptedSFX(ESFX.MeyefPurr, _level.GetFamiliarPosition());
		AddWaitScript(0.5f);
	}

	internal void AddMeyefSqueak()
	{
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.MeyefMew));
		AddWaitScript(0.1f);
		PlayScriptedSFX(ESFX.MeyefDamaged, _level.GetFamiliarPosition());
		AddWaitScript(0.5f);
	}

	internal void AddFamiliarAnimation(int start, int length, float speed, EAnimationType type, bool doesBlock)
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = start;
		animationSpec.Length = length;
		animationSpec.Speed = speed;
		animationSpec.Type = type;
		AnimationSpec newAnim = animationSpec;
		AddScript(new ScriptAction(newAnim, EScriptTargetType.Familiar)
		{
			DoesBlockQueue = doesBlock
		});
	}

	internal void AddHideFamiliar(float duration)
	{
		AddScript(new ScriptAction(EScriptActionType.ChangeColor, 0f, duration, new Vector4(1f, 0f, 1f, 0f))
		{
			TargetType = EScriptTargetType.Familiar,
			DoesBlockQueue = false
		});
	}

	internal void AddUnhideFamiliar()
	{
		AddScript(new ScriptAction(EScriptActionType.ChangeColor, 0f, 0.5f, new Vector4(0f, 1f, 1f, 0f))
		{
			TargetType = EScriptTargetType.Familiar,
			DoesBlockQueue = false
		});
	}

	public void AddAutoplayGhostDialogue(string key)
	{
		_level.ShowAutoplayGhostDialogueMessage(key);
	}

	internal void AddLockCamera()
	{
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.LockUnlockCamera
		});
	}

	internal void AddUnlockCamera()
	{
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.LockUnlockCamera,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
	}

	internal void AddHidePlayer()
	{
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.HideShowPlayer
		});
	}

	internal void AddUnhidePlayer()
	{
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.HideShowPlayer,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
	}

	internal void AddCameraPan(Point cameraEnd, float timeToPan, bool doesBlockQueue)
	{
		AddScript(new ScriptAction(new Vector2(cameraEnd.X, cameraEnd.Y), timeToPan, doesBlockQueue));
	}

	internal void AddCameraPan(Point cameraStart, Point cameraEnd, float timeToPan, bool doesBlockQueue)
	{
		AddScript(new ScriptAction(new Vector2(cameraStart.X, cameraStart.Y), 0f, doesBlockQueue));
		AddScript(new ScriptAction(new Vector2(cameraEnd.X, cameraEnd.Y), timeToPan, doesBlockQueue));
	}

	internal void AddCameraPan(Point cameraStart, Point cameraEnd, float timeToPan, bool doesBlockQueue, ECameraScriptPanType panType)
	{
		AddScript(new ScriptAction(new Vector2(cameraStart.X, cameraStart.Y), 0f, doesBlockQueue));
		AddScript(new ScriptAction(new Vector2(cameraEnd.X, cameraEnd.Y), timeToPan, doesBlockQueue)
		{
			IntArgument = (int)panType
		});
	}

	internal void AddSepiaFade(float timeToFade, bool doesBlock)
	{
		AddScript(new ScriptAction(EScriptType.SepiaFade, timeToFade, 0f, doesBlock, new Vector4(1f, 0f, 0f, 0f)));
	}

	internal void AddEndSepiaFade(float timeToFade, bool doesBlock)
	{
		AddScript(new ScriptAction(EScriptType.SepiaFade, timeToFade, 0f, doesBlock, Vector4.Zero)
		{
			DoesClearSameType = true
		});
	}

	internal static CutsceneBase FromSpecification(Level level, Point position, int inID, ObjectTileSpecification objectSpec)
	{
		CutsceneBase cutsceneBase = null;
		if (objectSpec != null)
		{
			ECutsceneType argument = (ECutsceneType)objectSpec.Argument;
			switch (argument)
			{
			case ECutsceneType.Prologue0_Start:
				cutsceneBase = new CutscenePrologue0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Prologue1_Elders:
				cutsceneBase = new CutscenePrologue1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Prologue2_Forest:
				cutsceneBase = new CutscenePrologue2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Prologue3_Temple:
				cutsceneBase = new CutscenePrologue3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Prologue4_TutorialM:
				cutsceneBase = new CutscenePrologue4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Prologue5_TutorialS:
				cutsceneBase = new CutscenePrologue5(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeDesolation0_Warp:
				cutsceneBase = new CutsceneLakeDesolation0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeDesolation1_Entrance:
				cutsceneBase = new CutsceneLakeDesolation1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeDesolation2_Ship:
				cutsceneBase = new CutsceneLakeDesolation2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeDesolation3_City:
				cutsceneBase = new CutsceneLakeDesolation3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City0_Scientists:
				cutsceneBase = new CutsceneCity0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City1_Frame:
				cutsceneBase = new CutsceneCity1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City2_Spindle:
				cutsceneBase = new CutsceneCity2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City3_Warp:
				cutsceneBase = new CutsceneCity3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City4_Meyef:
				cutsceneBase = new CutsceneCity4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City5_WarTerminal:
				cutsceneBase = new CutsceneCity5(level, position, inID, objectSpec);
				break;
			case ECutsceneType.City6_TurnAround:
				cutsceneBase = new CutsceneCity6(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest0_Warp:
				cutsceneBase = new CutsceneForest0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest1_Pond:
				cutsceneBase = new CutsceneForest1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest2_WakeUp:
				cutsceneBase = new CutsceneForest2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest3_Haristel:
				cutsceneBase = new CutsceneForest3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest4_LeaveCamp:
				cutsceneBase = new CutsceneForest4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest5_RamedaQ2End:
				cutsceneBase = new CutsceneForest5(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest6_RamedaQ4Start:
				cutsceneBase = new CutsceneForest6(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest7_RamedaQ4End:
				cutsceneBase = new CutsceneForest7(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest8_EscortStart:
				cutsceneBase = new CutsceneForest8(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Forest9_EscortEnd:
				cutsceneBase = new CutsceneForest9(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeSerene0_Seykis:
				cutsceneBase = new CutsceneLakeSerene0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeSerene1_SeykisEnd:
				cutsceneBase = new CutsceneLakeSerene1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeSerene2_Warp:
				cutsceneBase = new CutsceneLakeSerene2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.LakeSerene3_VileteSaved:
				cutsceneBase = new CutsceneLakeSerene3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast0_Rameda:
				cutsceneBase = new CutsceneCavesPast0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast1_Camp:
				cutsceneBase = new CutsceneCavesPast1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast2_MawDoor:
				cutsceneBase = new CutsceneCavesPast2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast3_MawSuck:
				cutsceneBase = new CutsceneCavesPast3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast4_MawSpit:
				cutsceneBase = new CutsceneCavesPast4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast5_MawDie:
				cutsceneBase = new CutsceneCavesPast5(level, position, inID, objectSpec);
				break;
			case ECutsceneType.CavesPast6_MawBoom:
				cutsceneBase = new CutsceneCavesPast6(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Keep0_Demons0:
				cutsceneBase = new CutsceneKeep0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Keep1_Demons1:
				cutsceneBase = new CutsceneKeep1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Hangar0_Blocked:
				cutsceneBase = new CutsceneHangar0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Hangar1_PostWarp:
				cutsceneBase = new CutsceneHangar1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Lab0_Scientists:
				cutsceneBase = new CutsceneLab0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Lab1_Gear1:
				cutsceneBase = new CutsceneLab1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Lab2_Gear2:
				cutsceneBase = new CutsceneLab2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Lab3_Glass:
				cutsceneBase = new CutsceneLab3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Lab4_Documents:
				cutsceneBase = new CutsceneLab4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EmpTower0_Door:
				cutsceneBase = new CutsceneEmpTower0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EmpTower1_Win:
				cutsceneBase = new CutsceneEmpTower1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EmpTower2_Throne:
				cutsceneBase = new CutsceneEmpTower2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EmpTower3_Exit:
				cutsceneBase = new CutsceneEmpTower3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EmpTower4_DoorAlt:
				cutsceneBase = new CutsceneEmpTower4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EmpTower5_After:
				cutsceneBase = new CutsceneEmpTower5(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Alt0_Nuvius:
				cutsceneBase = new CutsceneAlt0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Alt1_Vol:
				cutsceneBase = new CutsceneAlt1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Alt2_Win:
				cutsceneBase = new CutsceneAlt2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Alt3_Teleport:
				cutsceneBase = new CutsceneAlt3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.DarkForest0_Start:
				cutsceneBase = new CutsceneDarkForest0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.DarkForest1_End:
				cutsceneBase = new CutsceneDarkForest1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Temple0_Boss:
				cutsceneBase = new CutsceneTemple0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Temple1_BossEnd:
				cutsceneBase = new CutsceneTemple1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Temple2_End:
				cutsceneBase = new CutsceneTemple2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Temple_3_Blocked:
				cutsceneBase = new CutsceneTemple3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingA0_Present0:
				cutsceneBase = new CutsceneEndingA0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingA1_Present1:
				cutsceneBase = new CutsceneEndingA1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingA2_Past0:
				cutsceneBase = new CutsceneEndingA2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingA3_Past1:
				cutsceneBase = new CutsceneEndingA3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingA4_Present2:
				cutsceneBase = new CutsceneEndingA4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingA5_Winderia0:
			case ECutsceneType.EndingB4_Winderia0:
				cutsceneBase = new CutsceneEndingAB5(level, position, inID, objectSpec, argument == ECutsceneType.EndingA5_Winderia0);
				break;
			case ECutsceneType.EndingB0_Present0:
				cutsceneBase = new CutsceneEndingB0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingB1_Present1:
				cutsceneBase = new CutsceneEndingB1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingB2_Past0:
				cutsceneBase = new CutsceneEndingB2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingB3_Past1:
				cutsceneBase = new CutsceneEndingB3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingC0_Present0:
				cutsceneBase = new CutsceneEndingC0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingC1_Past0:
				cutsceneBase = new CutsceneEndingC1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingC2_Past1:
				cutsceneBase = new CutsceneEndingC2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingC2_Past1B:
				cutsceneBase = new CutsceneEndingC2B(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingC3_Present1:
			case ECutsceneType.EndingD4_Present0:
				cutsceneBase = new CutsceneEndingCD3(level, position, inID, objectSpec, argument == ECutsceneType.EndingC3_Present1);
				break;
			case ECutsceneType.EndingC4_Winderia0:
			case ECutsceneType.EndingD5_Winderia0:
				cutsceneBase = new CutsceneEndingCD4(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingC5_Meyef:
			case ECutsceneType.EndingD6_Meyef:
				cutsceneBase = new CutsceneEndingCD5(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingD0_Past0:
				cutsceneBase = new CutsceneEndingD0(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingD1_Past1:
				cutsceneBase = new CutsceneEndingD1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingD2_Past2:
				cutsceneBase = new CutsceneEndingD2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.EndingD3_Past3:
				cutsceneBase = new CutsceneEndingD3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc0_Revive:
				cutsceneBase = new CutsceneRevive(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc1_DreamGate:
				cutsceneBase = new CutsceneDreamGate(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc2_Gyre:
				cutsceneBase = new CutsceneGyre(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc3_QuestsEnd1:
				cutsceneBase = new CutsceneQuestsEnd1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc4_QuestsEnd2:
				cutsceneBase = new CutsceneQuestsEnd2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc5_QuestsEnd3:
				cutsceneBase = new CutsceneQuestsEnd3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc6_Gear1:
				cutsceneBase = new CutsceneGear1(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc7_Gear2:
				cutsceneBase = new CutsceneGear2(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc8_Gear3:
				cutsceneBase = new CutsceneGear3(level, position, inID, objectSpec);
				break;
			case ECutsceneType.Misc9_HaristelMaw:
				cutsceneBase = new CutsceneHaristelMaw(level, position, inID, objectSpec);
				break;
			}
			if (cutsceneBase != null)
			{
				cutsceneBase.CutsceneType = argument;
			}
		}
		return cutsceneBase;
	}

	internal static CutsceneBase CreateAndCallCutscene(ECutsceneType cutsceneType, Level level, Point position)
	{
		return CreateAndCallCutscene(cutsceneType, level, position, isAfterWarp: false);
	}

	internal static CutsceneBase CreateAndCallCutscene(ECutsceneType cutsceneType, Level level, Point position, bool isAfterWarp)
	{
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification();
		objectTileSpecification.ID = 495;
		objectTileSpecification.ObjectID = 40;
		objectTileSpecification.Argument = (int)cutsceneType;
		objectTileSpecification.Layer = ETileLayerType.Objects;
		objectTileSpecification.Category = EObjectTileCategory.Event;
		ObjectTileSpecification objectSpec = objectTileSpecification;
		CutsceneBase cutsceneBase = FromSpecification(level, position, -1, objectSpec);
		if (cutsceneBase != null)
		{
			level.RequestAddObject(cutsceneBase);
			cutsceneBase.Initialize();
			if (isAfterWarp)
			{
				cutsceneBase.IsCreatedAfterWarp = true;
			}
			cutsceneBase.StartCutscene();
		}
		return cutsceneBase;
	}

	internal static NPCBase GrabOrCreateNPC(Level level, NPCBase.ENPCType npcType, Point startPosition)
	{
		NPCBase nPCBase = GrabNPC(level, npcType);
		if (nPCBase == null)
		{
			nPCBase = NPCBase.FromArgumentAndLevel(level, startPosition, -1, new ObjectTileSpecification(487)
			{
				Argument = (int)npcType
			});
			level.RequestAddObject(nPCBase);
		}
		else
		{
			nPCBase.Position = startPosition;
		}
		return nPCBase;
	}

	internal static NPCBase GrabNPC(Level level, NPCBase.ENPCType npcType)
	{
		NPCBase result = null;
		foreach (NPCBase value in level.NPCs.Values)
		{
			if (value.NPCType == npcType)
			{
				result = value;
			}
		}
		return result;
	}

	internal static void StartSandmanEnding(Level level)
	{
		bool saveBool = level.GameSave.GetSaveBool("IsTerrilisDead");
		level.GameSave.SavePostEndingCD(saveBool);
		StartEnding(saveBool ? 3 : 2, level);
	}

	internal static void StartEnding(int index, Level level)
	{
		level.JukeBox.PlaySong(EBGM.CsSelen);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 17;
		levelChangeRequest.RoomID = 6;
		LevelChangeRequest levelChangeRequest2 = levelChangeRequest;
		switch (index)
		{
		case 0:
			levelChangeRequest2.CutsceneToCall = ECutsceneType.EndingA0_Present0;
			break;
		case 1:
			levelChangeRequest2.CutsceneToCall = ECutsceneType.EndingB0_Present0;
			break;
		case 2:
			levelChangeRequest2.CutsceneToCall = ECutsceneType.EndingC0_Present0;
			levelChangeRequest2.RoomID = 7;
			break;
		case 3:
			levelChangeRequest2.CutsceneToCall = ECutsceneType.EndingD0_Past0;
			levelChangeRequest2.RoomID = 7;
			break;
		}
		level.GameSave.Inventory.EquippedPassiveOrb = EInventoryOrbType.None;
		level.RequestChangeLevel(levelChangeRequest2);
	}

	internal static void StartAltEmperorCutscene(bool isVilete, Level level)
	{
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 13;
		levelChangeRequest.RoomID = (isVilete ? 1 : 0);
		levelChangeRequest.PreviousLevelID = 11;
		levelChangeRequest.CutsceneToCall = (isVilete ? ECutsceneType.Alt1_Vol : ECutsceneType.Alt0_Nuvius);
		levelChangeRequest.IsUsingWarp = !isVilete;
		levelChangeRequest.IsUsingWhiteFadeOut = true;
		levelChangeRequest.AdditionalBlackScreenTime = 0.25f;
		levelChangeRequest.FadeOutTime = 0.5f;
		levelChangeRequest.FadeInTime = 0.5f;
		LevelChangeRequest request = levelChangeRequest;
		level.RequestChangeLevel(request);
	}

	internal void AddRollCredits()
	{
		AddDelegateScript(StartLevelFadeOut);
		AddWaitScript(2f);
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = RollCredits
		});
	}

	internal void RollCredits()
	{
		_level.RequestRollCredits();
	}

	public static IEnumerable<Tuple<string, int>> GetAllOptionTypes()
	{
		List<Tuple<string, int>> list = new List<Tuple<string, int>>();
		foreach (ECutsceneType value in Enum.GetValues(typeof(ECutsceneType)))
		{
			list.Add(new Tuple<string, int>(value.ToString(), (int)value));
		}
		return list;
	}

	internal void StartLevelFadeIn()
	{
		StartLevelFade(1f, isFadingIn: true);
	}

	internal void StartLevelFadeOut()
	{
		StartLevelFade(1f, isFadingIn: false);
	}

	internal void InstantLevelFade()
	{
		StartLevelFade(0f, isFadingIn: false);
	}

	internal void StartLevelFade(float timeToFade, bool isFadingIn)
	{
		_isFadingIn = isFadingIn;
		_levelFadeTimer = 0f;
		_timeForLevelFade = timeToFade;
		_isDrawingLevelFade = true;
		_levelFadeColor = (isFadingIn ? Color.Black : Color.Transparent);
		if (_levelFadeForeground == null)
		{
			_levelFadeForeground = new Background(new BackgroundSpecification
			{
				IsForeground = true,
				DoesTileEast = true,
				DoesTileWest = true,
				DoesTileNorth = true,
				DoesTileSouth = true,
				DrawColor = _levelFadeColor,
				TextureType = EBackgroundTextureType.EndingBackdrops1,
				FrameIndex = 7
			}, _level);
			_level.Foregrounds.Add(_levelFadeForeground);
		}
	}

	public override void Update(float delta)
	{
		if (_isDrawingLevelFade && _levelFadeForeground != null)
		{
			base.DrawPlane = EDrawPlane.Front;
			_levelFadeTimer += delta;
			if (_levelFadeTimer < _timeForLevelFade)
			{
				float num = _levelFadeTimer / _timeForLevelFade;
				if (_isFadingIn)
				{
					num = 1f - num;
				}
				_levelFadeColor = new Color(num, num, num, num);
			}
			else
			{
				_levelFadeColor = (_isFadingIn ? Color.Transparent : Color.Black);
				_levelFadeTimer = _timeForLevelFade;
			}
			_levelFadeForeground.DrawColor = _levelFadeColor;
		}
		base.Update(delta);
	}

	internal static bool GetIsCutsceneTriggeredByType(ECutsceneType cutsceneType, GameSave save)
	{
		return save.GetSaveBool($"Cutscene_{(int)cutsceneType}");
	}

	internal static void AddLunaisPalmPunch(Level level)
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 250;
		animationSpec.Length = 2;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec item = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 251;
		animationSpec2.Length = 1;
		animationSpec2.Speed = 0.2f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec item2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 252;
		animationSpec3.Length = 4;
		animationSpec3.Speed = 0.07f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec item3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 255;
		animationSpec4.Length = 1;
		animationSpec4.Speed = 0.6f;
		animationSpec4.Type = EAnimationType.Once;
		AnimationSpec item4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 256;
		animationSpec5.Length = 1;
		animationSpec5.Speed = 0.1f;
		animationSpec5.Type = EAnimationType.Once;
		AnimationSpec item5 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 0;
		animationSpec6.Length = 5;
		animationSpec6.Speed = 0.11f;
		animationSpec6.Type = EAnimationType.Cycle;
		AnimationSpec item6 = animationSpec6;
		AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
		animationSpecCollection.Collection.Add(item);
		animationSpecCollection.Collection.Add(item2);
		animationSpecCollection.Collection.Add(item3);
		animationSpecCollection.Collection.Add(item4);
		animationSpecCollection.Collection.Add(item5);
		animationSpecCollection.Collection.Add(item6);
		Protagonist mainHero = level.MainHero;
		level.AddScript(new ScriptAction(animationSpecCollection, mainHero)
		{
			DoesBlockQueue = false
		});
		level.AddScript(new ScriptAction(ESFX.LunaisFistClasp, mainHero.Position)
		{
			DoesBlockQueue = false
		});
		level.AddScript(new ScriptAction
		{
			DoesBlockQueue = true,
			ScriptType = EScriptType.Wait,
			ActionTimer = 0.5f
		});
	}

	internal static void SetIsCutsceneTriggeredByType(ECutsceneType cutsceneType, bool value, GameSave save)
	{
		save.SetValue($"Cutscene_{(int)cutsceneType}", value);
	}
}
