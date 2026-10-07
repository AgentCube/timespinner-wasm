using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class GyrePortalEvent : GameEvent
{
	private enum EGyrePortalType
	{
		Main,
		DungeonStart,
		DungeonEnd,
		BossEnd,
		Prologue
	}

	private const int GyreLevelID = 14;

	private const int DarkForestLevelID = 15;

	private const int MainRoomID = 0;

	private const int DungeonStartRoomID = 11;

	private const int RavenRoomID = 8;

	private const int ZelRoomID = 6;

	private const int EmissionPerTick = 3;

	private const int MainPositionOffsetX = -8;

	private const int MainCircleOffsetY = -87;

	private const int BossEndPositionOffsetX = 0;

	private const int BossEndCircleOffsetY = -64;

	private const int NightmareSmokeSimulationCount = 60;

	private const int NightmareSmokeMinRadius = 16;

	private const int NightmareSmokeMaxRadius = 28;

	private const int NightmareSmokeOffsetY = -2;

	private const float TimeBetweenNightmareSmokeParticles = 0.03f;

	private static readonly Point NightmareSize = new Point(92, 47);

	private static readonly Color MainPortalColor = new Color(0.15f, 0.05f, 0.15f);

	private static readonly Color BossPortalColor = new Color(0.25f, 0f, 0f);

	private static readonly Vector4 BossParticlesColor = new Vector4(0.15f, 0.15f, 0.2f, 0.2f);

	private static readonly Vector4 MainParticlesColor = new Vector4(0.3f, 0.15f, 0.2f, 0.1f);

	private readonly bool _isUsable;

	private readonly EGyrePortalType _portalType;

	private readonly Point _warpInPoint;

	private readonly Appendage _nightmareTopAppendage;

	private readonly Appendage _nightmareBottomAppendage;

	private readonly TimeGateSwirlParticleSystem _nightmareSwirlParticles;

	private bool _hasBeenActivated;

	private bool _isPlayerTouchingUs;

	private float _maxTimeSpentUntouched;

	private float _timeSpentBeingTouched;

	private float _nightmareSmokeEmissionTimer;

	public GyrePortalEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_portalType = (EGyrePortalType)objectSpec.Argument;
		_isUsable = _portalType != EGyrePortalType.Prologue;
		int num = ((_portalType != EGyrePortalType.BossEnd) ? (-8) : 0);
		int num2 = ((_portalType == EGyrePortalType.BossEnd) ? (-64) : (-87));
		Vector4 baseColor;
		Color color;
		switch (_portalType)
		{
		case EGyrePortalType.DungeonEnd:
			baseColor = BossParticlesColor;
			color = BossPortalColor;
			break;
		case EGyrePortalType.Prologue:
			baseColor = new Vector4(0.15f, 0.15f, 0.2f, 0.1f);
			color = new Color(0.2f, 0f, 0.1f);
			break;
		default:
			baseColor = MainParticlesColor;
			color = MainPortalColor;
			break;
		}
		_bbox = new Rectangle(0, 0, 64, 32);
		Position = new Point(_position.X + num, _position.Y);
		_warpInPoint = new Point(inPosition.X + num, inPosition.Y + num2);
		_sprite = _level.GCM.SpTimeGateAnimation;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = false;
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		base.IsTriggerableByMonsters = false;
		_nightmareSwirlParticles = new TimeGateSwirlParticleSystem(_sprite, 128)
		{
			BaseColor = baseColor
		};
		_particleSystems.Add(_nightmareSwirlParticles);
		_nightmareTopAppendage = new Appendage(this, NightmareSize, Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, num2),
			DrawPriority = -1,
			DoesInheritDrawColor = false,
			DrawColor = color,
			DoesDrawAura = true,
			AuraCount = 4f,
			AuraColor = color
		};
		_nightmareBottomAppendage = new Appendage(this, NightmareSize, Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, -num2 + 2),
			DrawPriority = -1,
			DoesInheritDrawColor = false,
			DrawColor = color,
			IsFlippedVertically = true,
			DoesDrawAura = true,
			AuraCount = 4f,
			AuraColor = color
		};
		_nightmareTopAppendage.ChangeAnimation(4);
		_nightmareBottomAppendage.ChangeAnimation(4);
		base.Appendages.Add(_nightmareTopAppendage);
		base.Appendages.Add(_nightmareBottomAppendage);
		for (int i = 0; i < 60; i++)
		{
			UpdateNightmareGate(0.016666f);
			UpdateParticleSystems(0.016666f);
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!_hasBeenActivated && _isUsable && who is Protagonist protagonist)
		{
			_isPlayerTouchingUs = true;
			if (_maxTimeSpentUntouched > 0.2f || _timeSpentBeingTouched >= 1.5f)
			{
				_isTriggered = true;
				_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
				if (protagonist.CheckButton(4, isNewPressOnly: true) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
				{
					_hasBeenActivated = true;
					UsePortal();
				}
			}
		}
		return false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateNightmareGate(delta);
		}
		base.Update(delta);
		if (!_isPlayerTouchingUs && _maxTimeSpentUntouched < 1f)
		{
			_maxTimeSpentUntouched += delta;
		}
		if (_isPlayerTouchingUs && _timeSpentBeingTouched < 1.5f)
		{
			_timeSpentBeingTouched += delta;
		}
		_isPlayerTouchingUs = false;
	}

	private void UpdateNightmareGate(float delta)
	{
		_nightmareSmokeEmissionTimer -= delta;
		if (_nightmareSmokeEmissionTimer <= 0f)
		{
			_nightmareSmokeEmissionTimer += 0.03f;
			for (int i = 0; i < 3; i++)
			{
				int num = _level.NextRandomInt(16, 28);
				double num2 = _level.NextRandomDouble() * 6.2831854820251465;
				float num3 = (float)(Math.Cos(num2) * (double)num);
				float num4 = (float)(Math.Sin(num2) * (double)num);
				_nightmareSwirlParticles.AddParticles(new Vector2((float)_warpInPoint.X + num3, (float)_warpInPoint.Y + num4 + -2f));
			}
		}
	}

	private void UsePortal()
	{
		LevelChangeRequest levelChangeRequest = null;
		switch (_portalType)
		{
		case EGyrePortalType.Main:
			levelChangeRequest = StartAndTeleportToDungeon();
			break;
		case EGyrePortalType.DungeonStart:
			levelChangeRequest = TeleportToMainPortal();
			break;
		case EGyrePortalType.DungeonEnd:
			levelChangeRequest = TeleportToBoss();
			break;
		case EGyrePortalType.BossEnd:
			levelChangeRequest = TeleportToMainPortal();
			break;
		}
		if (levelChangeRequest != null)
		{
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.CutsceneStart,
				DoesBlockQueue = false
			});
			AddLevelScriptAction(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.StopCast
			});
			AddLevelScriptAction(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.Idle,
				ActionTimer = 2f,
				DoesBlockQueue = false
			});
			AddLevelScriptAction(new ScriptAction(ESFX.FoleyWarpGyreIn, Point.Zero));
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.FadeInFadeOut,
				Arguments = new Vector4(0.5f, 0.5f, 0f, 1f)
			});
			AddUnskippableWaitScript(0.5f);
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.FadeInFadeOut,
				Arguments = new Vector4(0.4f, 0.2f, 0.2f, 0f)
			});
			AddUnskippableWaitScript(0.4f);
			AddLevelScriptAction(new ScriptAction(levelChangeRequest));
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.FadeInFadeOut,
				Arguments = new Vector4(0f, 1f, 0.5f, 1f),
				DoesBlockQueue = false
			});
			AddLevelScriptAction(new ScriptAction
			{
				ScriptType = EScriptType.FadeInFadeOut,
				Arguments = new Vector4(0f, 0.5f, 0.5f, 0f)
			});
			if (_portalType == EGyrePortalType.Main)
			{
				AddLevelScriptAction(new ScriptAction(EBGM.Level14));
			}
			AddUnskippableWaitScript(0.5f);
			AddLevelScriptAction(new ScriptAction(ESFX.FoleyWarpGyreOut, Point.Zero));
			AddUnskippableWaitScript(0.5f);
		}
	}

	private static LevelChangeRequest CreateLevelChangeRequest(int roomID, bool shouldReuseLevel)
	{
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 14;
		levelChangeRequest.PreviousLevelID = (shouldReuseLevel ? 14 : 15);
		levelChangeRequest.RoomID = roomID;
		levelChangeRequest.IsUsingWarp = true;
		levelChangeRequest.AdditionalBlackScreenTime = 0.25f;
		levelChangeRequest.FadeOutTime = 0.5f;
		levelChangeRequest.FadeInTime = 0.5f;
		return levelChangeRequest;
	}

	private LevelChangeRequest TeleportToMainPortal()
	{
		_level.JukeBox.FadeOutSong(1f);
		return CreateLevelChangeRequest(0, shouldReuseLevel: true);
	}

	private LevelChangeRequest TeleportToBoss()
	{
		int levelSaveInt = _level.GetLevelSaveInt("GyreDungeonSeed");
		int roomID = (GetIsDungeonBossTheRaven(levelSaveInt) ? 8 : 6);
		_level.JukeBox.FadeOutSong(1f);
		return CreateLevelChangeRequest(roomID, shouldReuseLevel: true);
	}

	private LevelChangeRequest StartAndTeleportToDungeon()
	{
		GameSave gameSave = _level.GameSave;
		bool saveBool = gameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Zel));
		bool saveBool2 = gameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Raven));
		bool flag = saveBool != saveBool2;
		Random random = new Random();
		int num = random.Next();
		_level.ClearLevelSaveData();
		if (flag && ((saveBool2 && num % 2 == 0) || (saveBool && num % 2 == 1)))
		{
			num = (num + 1) % int.MaxValue;
		}
		_level.SetLevelSaveInt("GyreDungeonSeed", num);
		EBossType bossType = (GetIsDungeonBossTheRaven(num) ? EBossType.Raven : EBossType.Zel);
		string saveKeyByBossType = BossClass.GetSaveKeyByBossType(bossType);
		_level.GameSave.SetValue(saveKeyByBossType, value: false);
		_level.GameSave.LastWarpLevel = _level.ID;
		_level.GameSave.LastWarpRoom = _level.RoomID;
		_level.JukeBox.StopSong();
		return CreateLevelChangeRequest(11, shouldReuseLevel: true);
	}

	private static bool GetIsDungeonBossTheRaven(int seed)
	{
		return seed % 2 == 0;
	}
}
