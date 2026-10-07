using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L17_End;

internal class EnvPrefabEndAscend : EnvironmentPrefabBase
{
	private enum EEndAscendState
	{
		Phase0,
		Phase1,
		Phase2,
		Phase3,
		Phase4,
		Phase5
	}

	private const int ShockwaveOffset = 4;

	private const int PlayerGlowOffsetY = -16;

	private const int RoomCenterX = 200;

	private const int RoomCenterY = 120;

	private const float TimeForFirstCueLoopToFadeIn = 0.5f;

	private const float TimeForCueLoopsToFadeIn = 0.25f;

	private const float TimeForCueLoopsToFadeOut = 0.25f;

	private const float TimeForLevelFadeIn = 2f;

	private const float TimeForPhase2Brighten = 1f;

	internal const float TimeForPhase3Brighten = 3f;

	private const float TimeForPhase4Unbrighten = 1f;

	private const float TimeForPhase5Brighten = 3f;

	private const float TimeForPhase5WhiteScreen = 1.5f;

	private const float TimeBeforePhase5RaisingHand = 1f;

	private const float TimeForEntirePhase5 = 5.5f;

	private const float Phase2GlowMultiplier = 0.2f;

	private const float Phase3GlowMultiplier = 1f;

	private static readonly Color Phase1LevelDrawColor = new Color(16, 14, 12, 128);

	private static readonly Color Phase2LevelDrawColor = new Color(16, 14, 12, 16);

	private static readonly Color Phase3LevelDrawColor = new Color(255, 248, 240, 255);

	private static readonly Color Phase4LevelDrawColor = new Color(24, 20, 16, 20);

	private static readonly Color ShockWaveColor = new Color(200, 180, 128, 32);

	private static readonly Color Phase5LevelDrawColor = Color.White;

	private static readonly Color AscendedDrawColor = new Color(1f, 0.85f, 0.5f, 0.1f);

	private readonly Point _roomCenter;

	private readonly Background _levelFadeForeground;

	private readonly GlowTexture _glowTexture;

	private readonly AscendedDrawHelper _ascendedDrawHelper;

	private readonly SandStreamerEvent _sparkleStreamer;

	private EEndAscendState _phaseState;

	private float _phaseTimer;

	private float _lastPhaseTimer;

	private float _glowTextureColorMultiplier;

	private Color _lastPhaseForegroundColor;

	private SFXCueInstance _phase1LoopCueInstance;

	private SFXCueInstance _phase2LoopCueInstance;

	private SFXCueInstance _phase3LoopCueInstance;

	internal bool IsAscendedVisible { get; set; }

	public EnvPrefabEndAscend(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpSelen;
		ChangeAnimation(-1);
		_roomCenter = new Point(200, 120);
		IsAscendedVisible = true;
		_phaseState = EEndAscendState.Phase0;
		_lastPhaseForegroundColor = Color.Black;
		_doAppendagesMatchImageFacing = true;
		_ascendedDrawHelper = new AscendedDrawHelper(this, 5);
		_ascendedDrawHelper.SetGlowColor(AscendedDrawColor);
		SetCharacterSequenceByName("Hover");
		SetCharacterSequenceByName("Idle");
		_sparkleStreamer = new SandStreamerEvent(_level, inPosition, ESandStreamerType.Ascended);
		_levelFadeForeground = new Background(new BackgroundSpecification
		{
			IsForeground = true,
			DoesTileEast = true,
			DoesTileWest = true,
			DoesTileNorth = true,
			DoesTileSouth = true,
			DrawColor = Color.White,
			TextureType = EBackgroundTextureType.EndingBackdrops1,
			FrameIndex = 13
		}, _level)
		{
			DrawColor = Color.Black
		};
		_level.Foregrounds.Add(_levelFadeForeground);
		_glowTexture = new GlowTexture(_level)
		{
			GlowCircleCount = 6,
			GlowCircleRadius = 64,
			GlowCircleConsecutiveSizeReduction = 0.95f,
			GlowColorMultiplier = 0.1f,
			GlowFrequency = 3f,
			GlowOffset = 1f,
			GlowAmplitude = 0.15f
		};
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdatePhase(delta);
			_ascendedDrawHelper.Update(delta);
			if (!IsAscendedVisible)
			{
				_glowTexture.Center = _level.GetPlayerPosition().Add(0, -16);
				_glowTexture.BaseColor = AscendedDrawColor * _glowTextureColorMultiplier;
				_glowTexture.Update(delta);
			}
		}
		base.Update(delta);
		_sparkleStreamer.Update(delta);
	}

	private void UpdatePhase(float delta)
	{
		switch (_phaseState)
		{
		case EEndAscendState.Phase0:
			_levelFadeForeground.DrawColor = Color.Black;
			break;
		case EEndAscendState.Phase1:
			UpdatePhase1();
			break;
		case EEndAscendState.Phase2:
			UpdatePhase2();
			break;
		case EEndAscendState.Phase3:
			UpdatePhase3();
			break;
		case EEndAscendState.Phase4:
			UpdatePhase4();
			break;
		case EEndAscendState.Phase5:
			UpdatePhase5();
			break;
		}
		_lastPhaseTimer = _phaseTimer;
		_phaseTimer += delta;
	}

	private void UpdatePhase1()
	{
		if (_phaseTimer < 2f)
		{
			float amount = _phaseTimer / 2f;
			_levelFadeForeground.DrawColor = Color.Black.Lerp(Phase1LevelDrawColor, amount);
		}
		else
		{
			_levelFadeForeground.DrawColor = Phase1LevelDrawColor;
		}
	}

	private void UpdatePhase2()
	{
		if (_phaseTimer < 1f)
		{
			float num = _phaseTimer / 1f;
			_levelFadeForeground.DrawColor = _lastPhaseForegroundColor.Lerp(Phase2LevelDrawColor, num);
			_glowTextureColorMultiplier = 0.2f * num;
		}
		else
		{
			_levelFadeForeground.DrawColor = Phase2LevelDrawColor;
		}
	}

	private void UpdatePhase3()
	{
		if (_phaseTimer < 3f)
		{
			float amount = _phaseTimer / 3f;
			_levelFadeForeground.DrawColor = _lastPhaseForegroundColor.Lerp(Phase3LevelDrawColor, amount);
			_glowTextureColorMultiplier = MathHelper.Lerp(0.2f, 1f, amount);
		}
		else
		{
			_levelFadeForeground.DrawColor = Phase3LevelDrawColor;
		}
	}

	private void UpdatePhase4()
	{
		if (_phaseTimer < 1f)
		{
			float amount = _phaseTimer / 1f;
			_levelFadeForeground.DrawColor = _lastPhaseForegroundColor.Lerp(Phase4LevelDrawColor, amount);
		}
		else
		{
			_levelFadeForeground.DrawColor = Phase4LevelDrawColor;
		}
	}

	private void UpdatePhase5()
	{
		if (_phaseTimer >= 1f && _lastPhaseTimer < 1f)
		{
			SetCharacterSequenceByName("ToHand");
			_sparkleStreamer.SetStreamerPhase(EAscendStreamerPhase.CenterLeave);
		}
		if (_phaseTimer < 5.5f)
		{
			if (_phaseTimer > 1f)
			{
				float num = (_phaseTimer - 1f) / 3f;
				if (num > 1f)
				{
					num = 1f;
				}
				_levelFadeForeground.DrawColor = _lastPhaseForegroundColor.Lerp(Phase5LevelDrawColor, num);
			}
		}
		else
		{
			_levelFadeForeground.DrawColor = Phase5LevelDrawColor;
		}
	}

	private void SetPhase(EEndAscendState phase)
	{
		_phaseState = phase;
		_phaseTimer = 0f;
		_lastPhaseTimer = -1f;
		_lastPhaseForegroundColor = _levelFadeForeground.DrawColor;
	}

	internal void StartPhase1()
	{
		SetPhase(EEndAscendState.Phase1);
		_sparkleStreamer.SetStreamerPhase(EAscendStreamerPhase.EdgeWander);
		_phase1LoopCueInstance = CreateCue(ESFX.CsAscendPhase1Loop, _roomCenter, isLooped: true);
		if (_phase1LoopCueInstance != null)
		{
			_phase1LoopCueInstance.FadeIn(0.5f);
			_phase1LoopCueInstance.Play();
		}
	}

	internal void StartPhase2()
	{
		SetPhase(EEndAscendState.Phase2);
		_sparkleStreamer.SetStreamerPhase(EAscendStreamerPhase.SlowAbsorb);
		_phase2LoopCueInstance = CreateCue(ESFX.CsAscendPhase2Loop, _roomCenter, isLooped: true);
		if (_phase2LoopCueInstance != null)
		{
			_phase2LoopCueInstance.FadeIn(0.25f);
			_phase2LoopCueInstance.Play();
		}
		if (_phase1LoopCueInstance != null)
		{
			_phase1LoopCueInstance.Stop(0.25f);
		}
	}

	internal void StartPhase3()
	{
		SetPhase(EEndAscendState.Phase3);
		_sparkleStreamer.SetStreamerPhase(EAscendStreamerPhase.FastAbsorb);
	}

	internal void StartPhase4()
	{
		SetPhase(EEndAscendState.Phase4);
		IsAscendedVisible = true;
		_lastPhaseForegroundColor = Phase3LevelDrawColor;
		_levelFadeForeground.DrawColor = Phase3LevelDrawColor;
		ShockwaveAnimation newAnimation = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, new Point(Position.X + 4, Position.Y + 4), _level, ShockWaveColor);
		_level.AddAnimation(newAnimation);
		_sparkleStreamer.SetStreamerPhase(EAscendStreamerPhase.CenterCircle);
		_phase3LoopCueInstance = CreateCue(ESFX.CsAscendPhase3Loop, _roomCenter, isLooped: true);
		if (_phase3LoopCueInstance != null)
		{
			_phase3LoopCueInstance.FadeIn(0.25f);
			_phase3LoopCueInstance.Play();
		}
		if (_phase2LoopCueInstance != null)
		{
			_phase2LoopCueInstance.Stop(0.25f);
		}
	}

	internal void StartPhase5()
	{
		SetPhase(EEndAscendState.Phase5);
		_sparkleStreamer.SetStreamerPhase(EAscendStreamerPhase.CenterCircleAbsorb);
		if (_phase3LoopCueInstance != null)
		{
			_phase3LoopCueInstance.Stop(0.25f);
		}
	}

	internal void DoNod()
	{
		SetCharacterSequenceByName("Nod");
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_sparkleStreamer.Draw(spriteBatch);
		if (IsAscendedVisible)
		{
			_ascendedDrawHelper.Draw(spriteBatch, isAbove: false);
			base.Draw(spriteBatch);
			_ascendedDrawHelper.Draw(spriteBatch, isAbove: true);
		}
		else
		{
			_glowTexture.Draw(spriteBatch);
		}
	}
}
