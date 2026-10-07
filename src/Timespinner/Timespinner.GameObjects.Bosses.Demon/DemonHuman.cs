using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.StatusParticleEffects;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Demon;

internal sealed class DemonHuman : Monster
{
	private enum EDemonHumanAction
	{
		Idle,
		RedRover,
		Petals,
		Death
	}

	private const string HasSpawnedIdolKey = "HasSpawnedIdol";

	private const int DemonObjectTileID = 436;

	private const int BossStartX = 200;

	private const int BossStartY = 176;

	private const int LeftBasePositionX = 72;

	private const int RightBasePositionX = 328;

	private const int BasePositionY = 208;

	private const int MovementDistance = 32;

	private const float TimeToDoMove = 1f;

	private const float TimeToAppear = 0.5f;

	private const float TimeToDisappear = 0.5f;

	private const int FloatRadiusX = 6;

	private const int FloatRadiusY = 8;

	private const float FloatFrequency = 1f;

	private const float TimeToBattleIdle = 4f;

	private const int BattleIdleHeight = 16;

	private const int BattleIdleWidth = 32;

	private const int RedRoverHopHeight = 32;

	private const int RedRoverHopWidth = 72;

	private const int RedRoverHopCount = 4;

	private const int RedRoverTotalWidth = 288;

	private const float TimeForRedRoverGo = 3f;

	private const float TimeForRedRoverReturn = 3f;

	private const float TimeForEntireRedRoverSequence = 6f;

	private const float TimeForDramaticDeath = 1.5f;

	private readonly bool _isMale;

	private readonly int _baseDamageCaused;

	private readonly Point _basePosition;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _laughSequence;

	private readonly CharacterSequenceSpecification _endLaughSequence;

	private readonly CharacterSequenceSpecification _deadSequence;

	private readonly DemonBossFireParticleSystem _burningParticleSystem;

	private bool _isWarping;

	private bool _isAppearing;

	private bool _isFighting;

	private bool _doesNeedToStopLaughing;

	private bool _isPantingOnGround;

	private EDemonHumanAction _humanAction;

	private int _startX;

	private int _startY;

	private float _battleStateTimer;

	private float _lastBattleStateTimer;

	private float _offsetX;

	private float _warpTimer;

	private float _floatTimer;

	private float _moveTimer;

	private float _positionMatchTimer;

	private float _timeToMatchPosition;

	private Point _lastActionEndPoint;

	private Color _warpStartColor = Color.White;

	private SFXCueInstance _laughCueLoopInstance;

	internal DemonHuman Sibling { get; set; }

	public DemonHuman(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Bbox = new Rectangle(0, 0, 16, 16);
		_baseDamageCaused = _damageCaused;
		_damageCaused = 0;
		_isInvulnerable = true;
		_isAlwaysInvulnerable = true;
		_isMale = objectSpec.Argument == 3;
		IsFacingLeft = objectSpec.IsFlippedHorizontally;
		_basePosition = new Point(_isMale ? 72 : 328, 208);
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_humanAction = EDemonHumanAction.Idle;
		_isAlwaysAggroed = true;
		_doesDrawBaseSprite = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.CannotBeGrabbed = true;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		_idleSequence = GetCharacterSequenceByName("Idle");
		_laughSequence = GetCharacterSequenceByName("Laugh");
		_endLaughSequence = GetCharacterSequenceByName("EndLaugh");
		_deadSequence = GetCharacterSequenceByName("Dead");
		_burningParticleSystem = new DemonBossFireParticleSystem(_sprite, 90, 4, 20);
		_particleSystems.Add(_burningParticleSystem);
		SetCharacterSequence(_idleSequence);
		_level.SetLevelSaveBool("HasSpawnedIdol", value: false);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isFighting)
			{
				if (!_isPantingOnGround)
				{
					UpdateFloating(delta);
				}
			}
			else
			{
				_lastBattleStateTimer = _battleStateTimer;
				_battleStateTimer += delta;
			}
			UpdateWarping(delta);
		}
		base.Update(delta);
	}

	private void UpdateFloating(float delta)
	{
		if (_moveTimer > 0f)
		{
			_moveTimer -= delta;
			if (_moveTimer > 0f)
			{
				_offsetX = (float)((!IsFacingLeft) ? 1 : (-1)) * (float)(Math.Sin((1f - _moveTimer) * ((float)Math.PI / 2f)) * 32.0);
			}
		}
		_floatTimer += delta * 1f;
		if (_floatTimer >= (float)Math.PI * 2f)
		{
			_floatTimer -= (float)Math.PI * 2f;
		}
		int num = (int)Math.Round(Math.Sin(_floatTimer * 2f) * 8.0);
		int num2 = (int)Math.Round(Math.Cos(_floatTimer) * 6.0) * ((!IsFacingLeft) ? 1 : (-1));
		Position = new Point(_startX + (int)_offsetX + num2, _startY + num);
	}

	private void UpdateWarping(float delta)
	{
		if (!_isWarping)
		{
			return;
		}
		_warpTimer += delta;
		if (_isAppearing)
		{
			float num = 1f;
			if (_warpTimer >= 0.5f)
			{
				_isWarping = false;
			}
			else
			{
				num = (float)Math.Sin(_warpTimer / 0.5f * ((float)Math.PI / 2f));
			}
			base.DrawColor = _warpStartColor * num;
			return;
		}
		float num2 = 1f;
		if (_warpTimer >= 0.5f)
		{
			_isWarping = false;
			SilentKill();
		}
		else
		{
			num2 = (float)Math.Sin(_warpTimer / 0.5f * ((float)Math.PI / 2f));
		}
		base.DrawColor = _warpStartColor * (1f - num2);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (!_isFighting)
		{
			return;
		}
		switch (_humanAction)
		{
		case EDemonHumanAction.Idle:
			UpdateBattleIdle();
			break;
		case EDemonHumanAction.RedRover:
			UpdateBattleRedRover();
			break;
		case EDemonHumanAction.Petals:
			UpdateBattlePetals();
			break;
		case EDemonHumanAction.Death:
			UpdateDramaticDeath();
			break;
		}
		if (_positionMatchTimer < _timeToMatchPosition && _timeToMatchPosition > 0f)
		{
			_positionMatchTimer += delta;
			if (_positionMatchTimer < _timeToMatchPosition)
			{
				float amount = _positionMatchTimer / _timeToMatchPosition;
				Position = _lastActionEndPoint.SineInterpolate(Position, amount);
			}
		}
	}

	private void PickNextAction()
	{
		_lastBattleStateTimer = -0.0001f;
		_battleStateTimer = 0f;
		_positionMatchTimer = 0f;
		_lastActionEndPoint = Position;
		if (_humanAction == EDemonHumanAction.Idle)
		{
			_timeToMatchPosition = 1f;
			SetCharacterSequence(_laughSequence);
			_humanAction = EDemonHumanAction.RedRover;
			return;
		}
		_timeToMatchPosition = 1f;
		if (_humanAction == EDemonHumanAction.RedRover)
		{
			SetCharacterSequence(_endLaughSequence);
		}
		_humanAction = EDemonHumanAction.Idle;
	}

	private void UpdateBattleIdle()
	{
		if (_battleStateTimer > 4f)
		{
			PickNextAction();
			return;
		}
		float num = _battleStateTimer / 4f;
		int num2 = (int)(Math.Cos(num * ((float)Math.PI * 2f)) * (double)(_isMale ? 1 : (-1)) * 32.0);
		int num3 = (int)(Math.Sin(num * ((float)Math.PI * 2f) * 2f) * 16.0);
		Position = new Point(_basePosition.X + num2, _basePosition.Y + num3);
		if (_doesNeedToStopLaughing && _battleStateTimer > 1.25f)
		{
			_doesNeedToStopLaughing = false;
			SetCharacterSequence(_idleSequence);
		}
	}

	private void UpdateBattleRedRover()
	{
		if (_battleStateTimer < 6f)
		{
			if (_lastBattleStateTimer <= 0f)
			{
				PlayCue2D(ESFX.BossDemonIncSuccWaveAttack);
				if (_laughCueLoopInstance == null)
				{
					_laughCueLoopInstance = PlayCue(_isMale ? ESFX.BossDemonIncSuccLaughMLoop : ESFX.BossDemonIncSuccLaughFLoop, isLooped: true);
				}
				else if (_laughCueLoopInstance.IsPaused)
				{
					_laughCueLoopInstance.Resume();
				}
			}
			int num2;
			int num3;
			if (_battleStateTimer < 3f)
			{
				float num = _battleStateTimer / 3f;
				num2 = (int)(288f * num);
				num3 = (int)(Math.Sin(num * 4f * (float)Math.PI) * 32.0);
			}
			else
			{
				float num4 = (_battleStateTimer - 3f) / 3f;
				num2 = (int)(288f * (1f - num4));
				num3 = (int)(Math.Sin(num4 * 4f * (float)Math.PI) * 32.0);
			}
			if (!_isMale)
			{
				num2 = -num2;
			}
			Position = new Point(_basePosition.X + num2, _basePosition.Y + num3);
		}
		else
		{
			PickNextAction();
			if (_laughCueLoopInstance != null && !_laughCueLoopInstance.IsPaused)
			{
				_laughCueLoopInstance.Pause();
			}
		}
	}

	private void UpdateBattlePetals()
	{
	}

	private void UpdateDramaticDeath()
	{
		_burningParticleSystem.AddParticles(base.OuterBbox);
		if (_battleStateTimer < 1.5f)
		{
			float amount = _battleStateTimer / 1.5f;
			base.DrawColor = Color.White.SineInterpolate(Color.Black, amount);
		}
		else if (!_isWarping)
		{
			StartMainBattle();
		}
	}

	internal void Appear(Point position)
	{
		Position = position;
		_startX = position.X;
		_startY = Position.Y;
		_isWarping = true;
		_isAppearing = true;
		_warpTimer = 0f;
		UpdateFloating(0f);
		UpdateWarping(0f);
		PlayCue(ESFX.BossDemonIncSuccAppear);
	}

	internal void Disappear()
	{
		_isWarping = true;
		_isAppearing = false;
		_warpTimer = 0f;
		UpdateWarping(0f);
		_level.PlayCue(ESFX.BossDemonIncSuccDisappear, Position);
	}

	internal void DoLaugh()
	{
		SetCharacterSequence(_laughSequence);
	}

	internal void DoMove()
	{
		_moveTimer = 1f;
	}

	internal void StartFakeBattle()
	{
		_isFighting = true;
		_humanAction = EDemonHumanAction.Idle;
		_doesNeedToStopLaughing = true;
		_lastActionEndPoint = Position;
		_timeToMatchPosition = 2f;
		_damageCaused = _baseDamageCaused;
		_isInvulnerable = false;
		_isAlwaysInvulnerable = false;
		base.DoesTouchDamageKnockback = true;
	}

	internal void StartMainBattle()
	{
		if (!_level.GetLevelSaveBool("HasSpawnedIdol"))
		{
			_level.SetLevelSaveBool("HasSpawnedIdol", value: true);
			_level.RequestScreenFadeOut(0.5f);
			AddWaitScript(0.5f);
			AddDelegateScript(SpawnDemonBoss);
		}
		_warpStartColor = Color.Black;
		Disappear();
	}

	private void SpawnDemonBoss()
	{
		DemonBoss demonBoss = new DemonBoss(new Point(200, 176), _level, _level.GCM.SpDemonBoss, -1, new ObjectTileSpecification(436));
		demonBoss.InitializeMob();
		_level.RequestAddObject(demonBoss);
		_level.PlayCue(ESFX.BossDemonPhase2Intro);
	}

	private void StartDramaticDeath()
	{
		if (_isMale && _humanAction != EDemonHumanAction.Death)
		{
			_level.JukeBox.FadeOutSong(3f);
			_level.PlayCue(ESFX.BossDemonIncSuccFinalHit);
		}
		_humanAction = EDemonHumanAction.Death;
		_battleStateTimer = 0f;
		SetCharacterSequence(_laughSequence);
		_damageCaused = 0;
	}

	internal void PostBossAppear()
	{
		SetCharacterSequence(_deadSequence);
		_isPantingOnGround = true;
		Appear(Position);
	}

	public override void Kill()
	{
		if (_humanAction != EDemonHumanAction.Death)
		{
			_level.RequestScreenFlash(new ScreenFlash(0.2f)
			{
				Frequency = 2f
			});
			StartDramaticDeath();
			if (Sibling != null)
			{
				Sibling.StartDramaticDeath();
			}
		}
	}
}
