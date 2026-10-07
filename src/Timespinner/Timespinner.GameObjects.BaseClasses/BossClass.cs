using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.Bosses;
using Timespinner.GameObjects.Bosses.Bird;
using Timespinner.GameObjects.Bosses.Demon;
using Timespinner.GameObjects.Bosses.Emperor;
using Timespinner.GameObjects.Bosses.OtherBosses;
using Timespinner.GameObjects.Bosses.RoboKitty;
using Timespinner.GameObjects.Bosses.Varndagroth;
using Timespinner.GameObjects.Bosses.Z_Raven;
using Timespinner.GameObjects.Bosses.Z_Xarion;
using Timespinner.GameObjects.Enemies;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.BaseClasses;

internal class BossClass : Monster
{
	private const int TalkingOffset = 48;

	internal const string BossKillSaveKeyFormat = "IsBossDead_{0}";

	protected const float DeathAnimationChargeTime = 2f;

	protected const float DeathAnimationExplodeTime = 4f;

	private readonly EBossType _bossType;

	private readonly int _bossArgument;

	protected Color _deathParticlesColor = Color.White;

	protected Vector4 _deathParticleColorVect = Vector4.Zero;

	protected int _deathParticleType;

	protected ParticleSystem[] _deathParticleSystems = new ParticleSystem[3];

	protected int _currentDestinationNode;

	protected Point[] _destinationNodes;

	protected float _updateTargetTimer;

	protected float _maxUpdateTargetTime = 0.5f;

	private Point _deathPosition;

	internal bool IsBossIntroInProgress { get; set; }

	internal override bool IsABoss => true;

	internal EBossType BossType => _bossType;

	internal Point DeathPosition
	{
		get
		{
			return _deathPosition;
		}
		set
		{
			_deathPosition = value;
		}
	}

	private string KillSaveKey => $"IsBossDead_{_bossType}";

	public BossClass(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_doesDropBasicLoot = false;
		_defaultInvulnerableTime = 0.066f;
		_isAlwaysAggroed = true;
		_doesDeathScriptIgnoreFrozen = true;
		base.IsDormant = true;
		base.DoesTouchDamageKnockback = true;
		base.DoesRecoil = false;
		base.DoesDrawWhenOutsideOfObjectVisibleArea = true;
		_bossType = GetBossTypeFromEnemyType(objectSpec.GetEnemyType());
		_bossArgument = objectSpec.Argument;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isRunningDeathScript)
		{
			ParticleSystem[] deathParticleSystems = _deathParticleSystems;
			for (int i = 0; i < deathParticleSystems.Length; i++)
			{
				deathParticleSystems[i]?.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			}
		}
		base.Draw(spriteBatch);
	}

	public override void InitializeMob()
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
		IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected virtual void StartBossIntroCutscene()
	{
		EndBossIntroCutscene();
	}

	protected virtual void EndBossIntroCutscene()
	{
		IsBossIntroInProgress = false;
		base.IsDormant = false;
		_level.HasPlayerBeenDamagedInThisRoom = false;
		_level.HasPlayerFrozenTimeInThisRoom = false;
		if (!_level.JukeBox.DoesNotPlaySounds)
		{
			PlaySongByBossType(_level.JukeBox, _bossType);
		}
	}

	protected override void DetermineAction(float delta)
	{
		if (!IsBossIntroInProgress)
		{
			base.DetermineAction(delta);
			return;
		}
		_isAggroed = false;
		_currentAction = EAIAction.None;
	}

	protected override void DropLoot()
	{
		StartRestoreSandSequence();
	}

	protected override void StartDeathScript()
	{
		_deathPosition = Bbox.Center;
		GiveExperience();
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 2f, Vector4.Zero));
		_level.RequestScreenFlash(new ScreenFlash(0.2f)
		{
			Frequency = 2f
		});
		_level.JukeBox.FadeOutSong(4f);
		_level.TogglePlayerIsInvulnerable(isInvulnerable: true);
		base.StartDeathScript();
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		Vector2 where = new Vector2(_bbox.Center.X, _bbox.Center.Y);
		if (deathScriptTimer <= 0f)
		{
			_deathParticleColorVect = _deathParticlesColor.ToVector4();
			_deathScriptTimer = 1E-06f;
			_deathParticleSystems[0] = new BossDeathChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathParticleSystems[1] = new BossDeathChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathParticleSystems[0].BaseColor = _deathParticleColorVect;
			_deathParticleSystems[1].BaseColor = _deathParticleColorVect;
			_deathParticleSystems[0].AddParticles(where);
			_deathParticleSystems[1].AddParticles(where);
			base.DrawColor = Color.White;
		}
		else if (_deathScriptTimer < 2f)
		{
			_deathParticleSystems[0].AddParticles(where);
			_deathParticleSystems[1].AddParticles(where);
			float num = _deathScriptTimer / 1f;
			if (num > 1f)
			{
				num = 1f;
			}
			Vector4 vector = Vector4.SmoothStep(base.DrawColor.ToVector4(), _deathParticleColorVect, num);
			base.DrawColor = new Color(vector.X, vector.Y, vector.Z, 1f);
		}
		else if (_deathScriptTimer >= 2f && deathScriptTimer <= 2f)
		{
			_level.AddAnimation(EBattleAnimationType.Boom, Bbox.Center, ETeamSide.Enemies);
			_deathScriptTimer = 2.000001f;
			_doesDrawSpriteAndAppendages = false;
			_doesDrawTrail = false;
			_doesDrawBrushTrail = false;
			base.DoesDrawAura = false;
			DropLoot();
			_deathParticleSystems[1] = null;
			_deathParticleSystems[0] = new BossDeathShrapnelParticleSystem(_level.GCM.TxParticleEnergy, 2);
			_deathParticleSystems[0].BaseColor = _deathParticleColorVect;
			_deathParticleSystems[0].AddParticles(where);
		}
		else if (!(_deathScriptTimer < 4f) && _deathScriptTimer >= 4f)
		{
			EndBossDeathScript();
		}
		for (int i = 0; i < _deathParticleSystems.Length; i++)
		{
			if (_deathParticleSystems[i] != null)
			{
				_deathParticleSystems[i].Update(delta);
			}
		}
	}

	internal void MovePlayerToTalkingPosition(bool shouldBeFaceToFace, int talkOffsetX)
	{
		if (talkOffsetX == -1)
		{
			talkOffsetX = 48;
		}
		Point playerPosition = _level.GetPlayerPosition();
		int y = Position.Y;
		int num = Position.X - talkOffsetX;
		int num2 = Position.X + talkOffsetX;
		bool flag = IsFacingLeft;
		if (!shouldBeFaceToFace && Math.Abs(num - playerPosition.X) < Math.Abs(num2 - playerPosition.X) && !flag)
		{
			flag = true;
		}
		Point point = (flag ? new Point(num, y) : new Point(num2, y));
		if (point.X <= 24)
		{
			point = new Point(25, y);
		}
		else if (point.X >= 376)
		{
			point = new Point(375, y);
		}
		bool flag2 = playerPosition.X > point.X == flag;
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
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0f,
			DoesBlockQueue = false,
			DoesClearSameType = true
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			Arguments = new Vector4(point.X, point.Y, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.1f,
			DoesBlockQueue = true
		});
		if (flag2)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.LookDirection,
				ActionTimer = 0.25f,
				DoesBlockQueue = true,
				Arguments = new Vector4(IsFacingLeft ? 1 : (-1), 0f, 0f, 0f)
			});
		}
	}

	internal void AddScript(ScriptAction newScript)
	{
		_level.AddScript(newScript);
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (flag)
		{
			_level.GiveExperience(damage, base.ID, where, isBossHit: true);
		}
		return flag;
	}

	internal void StartRestoreSandSequence()
	{
		_level.AddEvent(new SandStreamerEvent(_level, _deathPosition, ESandStreamerType.BossDeath));
	}

	protected void EndBossDeathScript()
	{
		SaveBossDeath();
		BossDeathOpenDoors(shouldPlaySong: true);
		RemoveInstance();
	}

	protected void SaveBossDeath()
	{
		int num = 1;
		if (!_level.HasPlayerBeenDamagedInThisRoom)
		{
			num |= 2;
		}
		if (!_level.HasPlayerFrozenTimeInThisRoom)
		{
			num |= 4;
		}
		if (!_level.HasPlayerBeenDamagedInThisRoom && !_level.HasPlayerFrozenTimeInThisRoom)
		{
			num |= 8;
			_level.GameSave.UnlockFeat(EGameFeatType.PerfectBoss);
		}
		bool flag = _level.GameSave.UnlockFeat(_bossType, num);
		_level.GameSave.SetValue(KillSaveKey, value: true);
		if (flag)
		{
			_level.AddScript(new ScriptAction(EInventoryEquipmentType.GlassPumpkin, 1));
			_level.AddScript(new ScriptAction(EInventoryEquipmentType.GlassPumpkin));
		}
		if (_bossType == EBossType.Demon || _bossType == EBossType.Maw || _bossType == EBossType.Sorceress)
		{
			bool saveBool = _level.GameSave.GetSaveBool(GetSaveKeyByBossType(EBossType.Demon));
			bool saveBool2 = _level.GameSave.GetSaveBool(GetSaveKeyByBossType(EBossType.Maw));
			bool saveBool3 = _level.GameSave.GetSaveBool(GetSaveKeyByBossType(EBossType.Sorceress));
			if (saveBool && saveBool2 && saveBool3)
			{
				_level.GameSave.SetValue("IsPastCleared", value: true);
			}
		}
	}

	protected void BossDeathOpenDoors(bool shouldPlaySong)
	{
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
		_level.OpenAllBossDoors(1f);
		if (shouldPlaySong)
		{
			_level.PlayLevelSong();
		}
		_level.ToggleExits(isEnabled: true);
	}

	internal static Monster CreateFromTileType(Level level, Point tilePosition, GCM gcm, int newObjectID, ObjectTileSpecification objectTileSpec)
	{
		Monster result = null;
		EBossType bossTypeFromEnemyType = GetBossTypeFromEnemyType(objectTileSpec.GetEnemyType());
		if (!level.GameSave.GetSaveBool(GetSaveKeyByBossType(bossTypeFromEnemyType)))
		{
			switch (bossTypeFromEnemyType)
			{
			case EBossType.RoboKitty:
				result = new RoboKittyBoss(tilePosition, level, gcm.SpRoboKitty, newObjectID, objectTileSpec);
				break;
			case EBossType.Varndagroth:
				result = new VarndagrothBoss(tilePosition, level, gcm.SpVarndagroth, newObjectID, objectTileSpec);
				break;
			case EBossType.Sorceress:
				result = new AelanaBoss(tilePosition, level, gcm.SpAelana, newObjectID, objectTileSpec);
				break;
			case EBossType.Demon:
				switch (objectTileSpec.Argument)
				{
				case 0:
					result = new DemonBoss(tilePosition, level, gcm.SpDemonBoss, newObjectID, objectTileSpec);
					break;
				case 1:
				case 2:
					result = new DemonPuppet(tilePosition, level, gcm.SpDemonBoss, newObjectID, objectTileSpec);
					break;
				case 3:
				case 4:
					result = new DemonHuman(tilePosition, level, gcm.SpDemonBoss, newObjectID, objectTileSpec);
					break;
				}
				break;
			case EBossType.Bird:
				result = new GodBirdBoss(tilePosition, level, gcm.SpBirdBoss, newObjectID, objectTileSpec);
				break;
			case EBossType.Maw:
			{
				int argument2 = objectTileSpec.Argument;
				result = ((argument2 != 1) ? ((Monster)new MawBoss(tilePosition, level, gcm.SpMawBoss, newObjectID, objectTileSpec)) : ((Monster)new MawBossMinion(tilePosition, level, gcm.SpMawBoss, newObjectID, objectTileSpec)));
				break;
			}
			case EBossType.Shapeshift:
				result = new ShapeshifterBoss(tilePosition, level, gcm.SpShapeshifter, newObjectID, objectTileSpec);
				break;
			case EBossType.Emperor:
				result = new EmperorBoss(tilePosition, level, objectTileSpec.Argument switch
				{
					1 => gcm.SpEmperorVilete, 
					2 => gcm.SpEmperorWinderia, 
					_ => gcm.SpEmperor, 
				}, newObjectID, objectTileSpec);
				break;
			case EBossType.Sandman:
				result = new SandmanBoss(tilePosition, level, gcm.SpSandmanBoss, newObjectID, objectTileSpec);
				break;
			case EBossType.Nightmare:
				result = new NightmareBoss(tilePosition, level, gcm.SpNightmareBoss, newObjectID, objectTileSpec);
				break;
			case EBossType.Raven:
				result = new RavenBoss(tilePosition, level, gcm.SpRavenBoss, newObjectID, objectTileSpec);
				break;
			case EBossType.Xarion:
			{
				int argument = objectTileSpec.Argument;
				result = ((argument != 1) ? ((Monster)new XarionBoss(tilePosition, level, gcm.SpXarionBoss, newObjectID, objectTileSpec)) : ((Monster)new XarionBossHand(tilePosition, level, gcm.SpXarionBoss, newObjectID, objectTileSpec)));
				break;
			}
			case EBossType.Zel:
				result = new ZelBoss(tilePosition, level, gcm.SpZelBoss, newObjectID, objectTileSpec);
				break;
			case EBossType.Cantoran:
				result = new CantoranBoss(tilePosition, level, gcm.SpCantoranBoss, newObjectID, objectTileSpec);
				break;
			}
		}
		else
		{
			switch (bossTypeFromEnemyType)
			{
			case EBossType.RoboKitty:
				RoboKittyBoss.PlaceBladeOrb(tilePosition, level);
				break;
			case EBossType.Shapeshift:
				ShapeshifterBoss.PlaceKeycard(tilePosition, level);
				break;
			case EBossType.Demon:
				DemonBoss.PlaceHairpin(tilePosition, level);
				break;
			case EBossType.Cantoran:
				CantoranBoss.PlaceRadiantOrb(tilePosition, level);
				break;
			}
		}
		return result;
	}

	private static EBossType GetBossTypeFromEnemyType(EEnemyTileType enemyType)
	{
		EBossType result = EBossType.None;
		switch (enemyType)
		{
		case EEnemyTileType.BirdBoss:
			result = EBossType.Bird;
			break;
		case EEnemyTileType.RoboKittyBoss:
			result = EBossType.RoboKitty;
			break;
		case EEnemyTileType.VarndagrothBoss:
			result = EBossType.Varndagroth;
			break;
		case EEnemyTileType.AelanaBoss:
			result = EBossType.Sorceress;
			break;
		case EEnemyTileType.IncubusBoss:
			result = EBossType.Demon;
			break;
		case EEnemyTileType.MawBoss:
			result = EBossType.Maw;
			break;
		case EEnemyTileType.ShapeshiftBoss:
			result = EBossType.Shapeshift;
			break;
		case EEnemyTileType.EmperorBoss:
			result = EBossType.Emperor;
			break;
		case EEnemyTileType.SandmanBoss:
			result = EBossType.Sandman;
			break;
		case EEnemyTileType.NightmareBoss:
			result = EBossType.Nightmare;
			break;
		case EEnemyTileType.RavenBoss:
			result = EBossType.Raven;
			break;
		case EEnemyTileType.XarionBoss:
			result = EBossType.Xarion;
			break;
		case EEnemyTileType.ZelBoss:
			result = EBossType.Zel;
			break;
		case EEnemyTileType.CantoranBoss:
			result = EBossType.Cantoran;
			break;
		}
		return result;
	}

	internal static EEnemyTileType GetEnemyTypeFromBossType(EBossType bossType)
	{
		EEnemyTileType result = EEnemyTileType.CheveuxTank;
		switch (bossType)
		{
		case EBossType.RoboKitty:
			result = EEnemyTileType.RoboKittyBoss;
			break;
		case EBossType.Varndagroth:
			result = EEnemyTileType.VarndagrothBoss;
			break;
		case EBossType.Bird:
			result = EEnemyTileType.BirdBoss;
			break;
		case EBossType.Demon:
			result = EEnemyTileType.IncubusBoss;
			break;
		case EBossType.Maw:
			result = EEnemyTileType.MawBoss;
			break;
		case EBossType.Sorceress:
			result = EEnemyTileType.AelanaBoss;
			break;
		case EBossType.Shapeshift:
			result = EEnemyTileType.ShapeshiftBoss;
			break;
		case EBossType.Emperor:
			result = EEnemyTileType.EmperorBoss;
			break;
		case EBossType.Sandman:
			result = EEnemyTileType.SandmanBoss;
			break;
		case EBossType.Nightmare:
			result = EEnemyTileType.NightmareBoss;
			break;
		case EBossType.Raven:
			result = EEnemyTileType.RavenBoss;
			break;
		case EBossType.Xarion:
			result = EEnemyTileType.XarionBoss;
			break;
		case EBossType.Zel:
			result = EEnemyTileType.ZelBoss;
			break;
		case EBossType.Cantoran:
			result = EEnemyTileType.CantoranBoss;
			break;
		}
		return result;
	}

	private static EBossType GetBossTypeFromLevelID(int levelID)
	{
		EBossType result = EBossType.None;
		switch (levelID)
		{
		case 1:
			result = EBossType.RoboKitty;
			break;
		case 2:
			result = EBossType.Varndagroth;
			break;
		case 5:
			result = EBossType.Demon;
			break;
		case 6:
			result = EBossType.Sorceress;
			break;
		case 7:
			result = EBossType.Bird;
			break;
		case 8:
			result = EBossType.Maw;
			break;
		case 9:
			result = EBossType.Xarion;
			break;
		case 11:
			result = EBossType.Shapeshift;
			break;
		case 12:
			result = EBossType.Emperor;
			break;
		case 13:
			result = EBossType.Emperor;
			break;
		case 14:
			result = EBossType.Raven;
			break;
		case 16:
			result = EBossType.Sandman;
			break;
		}
		return result;
	}

	internal static string GetSaveKeyByBossType(EBossType bossType)
	{
		return $"IsBossDead_{bossType}";
	}

	internal static string GetBossKillKeyFromLevelID(int levelID)
	{
		return GetSaveKeyByBossType(GetBossTypeFromLevelID(levelID));
	}

	internal static void PlaySongByBossType(Jukebox jukebox, EBossType bossType)
	{
		EBGM song;
		switch (bossType)
		{
		case EBossType.Varndagroth:
		case EBossType.Raven:
		case EBossType.Xarion:
			song = EBGM.Boss02;
			break;
		case EBossType.Bird:
			song = EBGM.Boss07;
			break;
		case EBossType.Sorceress:
		case EBossType.Cantoran:
			song = EBGM.Boss06;
			break;
		case EBossType.Demon:
			song = EBGM.Boss05B;
			break;
		case EBossType.Maw:
		case EBossType.Zel:
			song = EBGM.Boss08;
			break;
		case EBossType.Shapeshift:
			song = EBGM.Boss11;
			break;
		case EBossType.Emperor:
			song = EBGM.Boss12;
			break;
		case EBossType.Sandman:
			song = EBGM.Boss15;
			break;
		case EBossType.Nightmare:
			song = EBGM.Boss16;
			break;
		default:
			song = EBGM.Boss01;
			break;
		}
		jukebox.PlaySong(song, shouldForceRestart: true, shouldImmediatelyStopPreviousSong: true);
	}
}
