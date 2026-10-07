using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L8_Caves;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutscenePrologue1 : CutsceneBase
{
	private const int ScreenOffsetX = -256;

	private const int SmokeOffsetX = -48;

	private const int ExplosionOffsetY = -48;

	private const int DebrisOffsetY = 0;

	private const int DebrisUnitOffsetY = -16;

	private const int BaseDebrisSpeed = 400;

	private const float TimeToDarken = 1f;

	private static readonly Color ScreenDarkenColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

	private readonly PrologueExplosionDebris _debris1;

	private readonly PrologueExplosionDebris _debris2;

	private readonly PrologueExplosionDebris _debris3;

	private readonly PrologueExplosionDebris _debris4;

	private readonly PrologueExplosionDebris _debris5;

	private readonly PrologueExplosionDebris _debris6;

	private readonly PrologueExplosionDebris _debris7;

	private readonly PrologueExplosionDebris _debris8;

	private readonly PrologueExplosionSmokeParticleSystem _smokeParticles;

	private readonly PrologueExplosionBurstParticleSystem _burstParticles;

	private readonly SelenNPC _selen;

	private bool _isDarknessFadingIn;

	private bool _isDrawingDarkness;

	private float _darknessFadeTimer;

	private NPCBase _messenger;

	private NPCBase _elder;

	private Background _screenDarkenBackground;

	private SFXCueInstance _rumbleCue;

	public CutscenePrologue1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.CutsceneDisappearType = ECutsceneDisappearType.Never;
		_selen = new SelenNPC(_level, new Point(inPosition.X + 32, inPosition.Y), -1)
		{
			IsFacingLeft = false
		};
		_level.RequestAddObject(_selen);
		_debris1 = new PrologueExplosionDebris(_level, Point.Zero, 0);
		_debris2 = new PrologueExplosionDebris(_level, Point.Zero, 1);
		_debris3 = new PrologueExplosionDebris(_level, Point.Zero, 2);
		_debris4 = new PrologueExplosionDebris(_level, Point.Zero, 3);
		_debris5 = new PrologueExplosionDebris(_level, Point.Zero, 7);
		_debris6 = new PrologueExplosionDebris(_level, Point.Zero, 6);
		_debris7 = new PrologueExplosionDebris(_level, Point.Zero, 5);
		_debris8 = new PrologueExplosionDebris(_level, Point.Zero, 4);
		_smokeParticles = new PrologueExplosionSmokeParticleSystem(_level.GCM.TxParticleSmoke, 1, isExplodingToTheLeft: false);
		_burstParticles = new PrologueExplosionBurstParticleSystem(_level.GCM.TxParticleSmoke, 1, isExplodingToTheLeft: false);
	}

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		_messenger = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Messenger);
		_elder = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Elder);
		Vector4 arguments = new Vector4(-1f, 0f, 0f, 0f);
		Vector4 arguments2 = new Vector4(1f, 0f, 0f, 0f);
		_level.GameSave.SetValue("IsJianaShocked", value: true);
		AddDialogue("cs_pro_1_sel_00");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _selen,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddDialogue("cs_pro_1_sel_01");
		AddSongFadeOut(1f, doesBlock: false);
		AddScreenShake(new Vector2(0f, 2f), -1f, 8f);
		AddDelegateScript(StartScreenDarken);
		AddDelegateScript(StopCrickets);
		PlayScriptedSFX(ESFX.CsPrologueExplosion);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _messenger,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _selen,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments2
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _elder,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments2
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _messenger,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments2
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _selen,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _elder,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddWaitScript(0.2f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments2
		});
		AddWaitScript(0.2f);
		AddPlaySong(EBGM.CsSoldiers, doesStopOtherSong: true);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _messenger,
			ActionType = EScriptActionType.LookDirection,
			Arguments = arguments
		});
		AddScreenFlash(0.2f, 0.78f, 2f);
		AddDelegateScript(AddDebris);
		MovePlayerToPosition(new Point(Position.X - 48, Position.Y), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
		AddDialogue("cs_pro_1_jia_02");
		MoveCharacterToPosition(_selen, new Point(Position.X + 8, Position.Y), doesBlockQueue: false, 0f);
		MovePlayerToPosition(new Point(Position.X - 20, Position.Y), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 218,
			Length = 3,
			Speed = 0.1f,
			Type = EAnimationType.Once
		}, mainHero));
		AddDialogue("cs_pro_1_lun_03");
		AddDialogue("cs_pro_1_sel_04");
		MoveCharacterToPosition(_selen, new Point(Position.X + 32, Position.Y), doesBlockQueue: false, 0f);
		MovePlayerToPosition(new Point(Position.X + 12, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		AddDialogue("cs_pro_1_eld_05");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _selen,
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.Run,
			Arguments = new Vector4(1f, 0f, 0f, 0f),
			ActionTimer = 0.55f,
			DoesBlockQueue = false
		});
		AddWaitScript(0.1f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.Run,
			Arguments = new Vector4(1f, 0f, 0f, 0f),
			ActionTimer = 0.55f,
			DoesBlockQueue = false
		});
		TeleportToLevelAndRoom(0, 1, ECutsceneType.Prologue2_Forest);
		AddScreenShake(Vector2.Zero, 0f, 0f);
	}

	public override void Update(float delta)
	{
		if (_isDrawingDarkness && _darknessFadeTimer < 1f)
		{
			_darknessFadeTimer += delta;
			float num = 1f;
			if (_darknessFadeTimer < 1f)
			{
				num = _darknessFadeTimer / 1f;
			}
			float amount = num;
			if (!_isDarknessFadingIn)
			{
				amount = 1f - num;
			}
			_screenDarkenBackground.DrawColor = Color.Transparent.SineInterpolate(ScreenDarkenColor, amount);
		}
		base.Update(delta);
	}

	private void StopCrickets()
	{
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.EnvironmentPrefab);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item.ObjectArgument == 5 && item is EnvPrefabProCricketsAmbient envPrefabProCricketsAmbient)
			{
				envPrefabProCricketsAmbient.StopCrickets();
			}
		}
	}

	private void AddDebris()
	{
		_level.RequestAddObject(_debris1);
		_level.RequestAddObject(_debris2);
		_level.RequestAddObject(_debris3);
		_level.RequestAddObject(_debris4);
		_level.RequestAddObject(_debris5);
		_level.RequestAddObject(_debris6);
		_level.RequestAddObject(_debris7);
		_level.RequestAddObject(_debris8);
		_particleSystems.Add(_smokeParticles);
		_particleSystems.Add(_burstParticles);
		_rumbleCue = CreateCue2D(ESFX.CsPrologueRumble, isLooped: true);
		if (_rumbleCue != null)
		{
			_rumbleCue.FadeIn(0.5f);
			_rumbleCue.Play();
		}
		ResetDebris();
	}

	private void ResetDebris()
	{
		Point point = new Point(Position.X + -256, Position.Y);
		_debris1.Reset(new Point(point.X, point.Y + -64), new Vector2(1.5f, -0.9f) * 400f, 10f);
		_debris2.Reset(new Point(point.X, point.Y + -48), new Vector2(1.65f, -0.75f) * 400f, 15f);
		_debris3.Reset(new Point(point.X, point.Y + -32), new Vector2(1.75f, -0.75f) * 400f, 20f);
		_debris4.Reset(new Point(point.X, point.Y + -16), new Vector2(1.6f, -1.15f) * 400f, 22f);
		_debris5.Reset(new Point(point.X + 64, point.Y + -64), new Vector2(1.25f, -1.2f) * 400f, 10f);
		_debris6.Reset(new Point(point.X + 64, point.Y + -48), new Vector2(1.45f, -0.105f) * 400f, 15f);
		_debris7.Reset(new Point(point.X + 64, point.Y + -32), new Vector2(1.55f, -0.105f) * 400f, 20f);
		_debris8.Reset(new Point(point.X + 64, point.Y + -16), new Vector2(1.4f, -1.45f) * 400f, 22f);
		Vector2 where = new Vector2(point.X, point.Y + -48);
		Vector2 where2 = new Vector2(point.X + -48, point.Y + -48);
		_smokeParticles.AddParticles(where2);
		_burstParticles.AddParticles(where);
	}

	private void StartScreenDarken()
	{
		_isDrawingDarkness = true;
		_isDarknessFadingIn = true;
		_darknessFadeTimer = 0f;
		if (_screenDarkenBackground == null)
		{
			_screenDarkenBackground = new Background(new BackgroundSpecification
			{
				IsForeground = true,
				DoesTileEast = true,
				DoesTileWest = true,
				DoesTileNorth = true,
				DoesTileSouth = true,
				DrawColor = ScreenDarkenColor,
				TextureType = EBackgroundTextureType.EndingBackdrops1,
				FrameIndex = 7
			}, _level);
			_level.Foregrounds.Add(_screenDarkenBackground);
		}
	}
}
