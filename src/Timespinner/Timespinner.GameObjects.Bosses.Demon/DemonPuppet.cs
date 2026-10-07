using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Demon;

internal sealed class DemonPuppet : Monster
{
	internal enum EDemonPuppetAction
	{
		Idle,
		RedRover,
		Resting,
		StopResting,
		BossDeath
	}

	private const int OffscreenArmOffsetX = 16;

	private const int OffscreenArmOffsetY = 128;

	private const int LeftBasePositionX = 56;

	private const int RightBasePositionX = 344;

	private const int BasePositionY = 216;

	private const int IdleHeight = 8;

	private const int IdleWidth = 24;

	private const float TimeToIdle = 2f;

	private const int RedRoverHopHeight = 16;

	private const int RedRoverHopWidth = 36;

	private const int RedRoverHopCount = 8;

	private const int RedRoverTotalWidth = 288;

	private const float TimeForRedRoverGo = 2f;

	private const float TimeForRedRoverReturn = 2f;

	internal const float TimeForEntireRedRoverSequence = 4f;

	private const int RestingOffsetX = -16;

	private const int RestingOffsetY = 12;

	private const float TimeToRest = 0.5f;

	private const float TimeToPlayDead = 15f;

	private const float TimeToFadeColor = 0.5f;

	private const float TimeToDelayTimeFreeze = 1f;

	private const int DeathHopHeight = 4;

	private const int DeathHopWidth = 20;

	private const int DeathSequenceBaseOffsetX = 0;

	private const int DeathSequenceBaseOffsetY = -24;

	private const float TimeToGetToBossDeathPosition = 0.5f;

	private const float TimeForDeathHop = 0.3f;

	private static readonly Color PlayingDeadColor = new Color(64, 64, 64);

	private readonly bool _isMale;

	private readonly int _baseTouchDamage;

	private readonly Point _basePosition;

	private readonly Point _offscreenArmBasePosition;

	private readonly Appendage _offscreenArm;

	private bool _shouldRefreezeAfterUpdate;

	private EDemonPuppetAction _puppetAction;

	private EDemonPuppetAction _nextPuppetAction;

	private EDemonPuppetAction _lastIgnoredPuppetAction;

	private float _puppetActionTimer;

	private float _lastPuppetActionTimer;

	private float _playDeadTimer;

	private float _playDeadFadeColorTimer;

	private float _timeStopDelayTimer;

	private Point _deathSequenceStartPosition;

	private Point _deathSequenceOffscreenArmStartPosition;

	internal bool IsPlayingDead { get; private set; }

	public DemonPuppet(Point inPosition, Level inLevel, SpriteSheet inSprite, int objectID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, objectID, objectSpec)
	{
		_isMale = objectSpec.Argument == 1;
		_baseTouchDamage = _damageCaused;
		Bbox = new Rectangle(0, 0, 16, 16);
		IsFacingLeft = objectSpec.IsFlippedHorizontally;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_puppetAction = EDemonPuppetAction.Idle;
		_isAlwaysAggroed = true;
		base.IsDormant = true;
		_doesDrawBaseSprite = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.CannotBeGrabbed = true;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		IsPlayingDead = false;
		_basePosition = new Point(_isMale ? 56 : 344, 216);
		Position = _basePosition;
		SnapBboxToPosition();
		_offscreenArm = base.Appendages[1];
		_offscreenArmBasePosition = new Point(_basePosition.X + (_isMale ? (-16) : 16), _basePosition.Y + 128);
		_offscreenArm.Position = _offscreenArmBasePosition;
		UpdateAppendages(0f);
	}

	internal void SetIsActive(bool isActive)
	{
		if (!isActive || !IsPlayingDead)
		{
			_damageCaused = (isActive ? _baseTouchDamage : 0);
			_canBeDamaged = isActive;
			base.IsSolidWhenFrozen = isActive;
		}
		base.IsDormant = !isActive || IsPlayingDead;
	}

	internal void SetDrawColor(Color newColor)
	{
		if (!IsPlayingDead)
		{
			base.DrawColor = newColor;
			_playDeadFadeColorTimer = 0f;
		}
	}

	public override void Update(float delta)
	{
		_shouldRefreezeAfterUpdate = false;
		if (!base.IsFrozen)
		{
			UpdatePlayDead(delta);
			_timeStopDelayTimer = 0f;
		}
		else if (_timeStopDelayTimer < 1f)
		{
			_timeStopDelayTimer += delta;
			_shouldRefreezeAfterUpdate = true;
			_isFrozen = false;
		}
		base.Update(delta);
		if (_shouldRefreezeAfterUpdate)
		{
			_isFrozen = true;
		}
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_puppetActionTimer = 0f;
		_lastPuppetActionTimer = -0.0001f;
		_puppetAction = _nextPuppetAction;
		_currentAction = EAIAction.Custom;
		_nextActionTimer = 1000f;
		_nextPuppetAction = EDemonPuppetAction.Idle;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (!base.IsFrozen && !_shouldRefreezeAfterUpdate)
		{
			switch (_puppetAction)
			{
			case EDemonPuppetAction.Idle:
				UpdatePuppetIdle();
				break;
			case EDemonPuppetAction.RedRover:
				UpdateRedRoverAction();
				break;
			case EDemonPuppetAction.Resting:
				UpdateRestingAction(isResting: true);
				break;
			case EDemonPuppetAction.StopResting:
				UpdateRestingAction(isResting: false);
				break;
			case EDemonPuppetAction.BossDeath:
				UpdateBossDeathAction();
				break;
			}
			_lastPuppetActionTimer = _puppetActionTimer;
			_puppetActionTimer += delta;
		}
	}

	private void UpdateRedRoverAction()
	{
		if (_puppetActionTimer < 4f)
		{
			if (_lastPuppetActionTimer < 0f)
			{
				PlayCue(_isMale ? ESFX.BossDemonPuppetLeftCharge : ESFX.BossDemonPuppetRightCharge);
			}
			int num2;
			int num3;
			if (_puppetActionTimer < 2f)
			{
				float num = _puppetActionTimer / 2f;
				num2 = (int)(288f * num);
				num3 = -(int)Math.Abs(Math.Sin(num * 8f * (float)Math.PI) * 16.0);
			}
			else
			{
				float num4 = (_puppetActionTimer - 2f) / 2f;
				num2 = (int)(288f * (1f - num4));
				num3 = -(int)Math.Abs(Math.Sin(num4 * 8f * (float)Math.PI) * 16.0);
			}
			if (!_isMale)
			{
				num2 = -num2;
			}
			_offscreenArm.Position = new Point(_offscreenArmBasePosition.X + num2, _offscreenArmBasePosition.Y);
			Position = new Point(_basePosition.X + num2, _basePosition.Y + num3);
		}
		else
		{
			FinishAttack();
		}
	}

	private void UpdatePuppetIdle()
	{
		if (_puppetActionTimer > 2f)
		{
			FinishAttack();
			return;
		}
		float num = _puppetActionTimer / 2f;
		int num2 = (int)(Math.Sin(num * ((float)Math.PI * 2f)) * (double)(_isMale ? 1 : (-1)) * 24.0);
		int num3 = -(int)Math.Abs(Math.Sin(num * ((float)Math.PI * 2f) * 2f) * 8.0);
		Position = new Point(_basePosition.X + num2, _basePosition.Y + num3);
	}

	private void UpdateRestingAction(bool isResting)
	{
		if (_puppetActionTimer < 0.5f)
		{
			if (_lastPuppetActionTimer < 0f && !isResting)
			{
				PlayCue(_isMale ? ESFX.BossDemonPuppetLeftRevive : ESFX.BossDemonPuppetRightRevive);
			}
			float num = _puppetActionTimer / 0.5f;
			if (!isResting)
			{
				num = 1f - num;
			}
			int num2 = (int)Math.Ceiling(num * -16f);
			int num3 = (int)Math.Ceiling(num * 12f);
			if (!_isMale)
			{
				num2 = -num2;
			}
			Position = new Point(_basePosition.X + num2, _basePosition.Y + num3);
			_offscreenArm.Position = new Point(_offscreenArmBasePosition.X + num2, _offscreenArmBasePosition.Y);
		}
		else if (!isResting || _nextPuppetAction == EDemonPuppetAction.StopResting)
		{
			FinishAttack();
		}
	}

	private void UpdateBossDeathAction()
	{
		Point end = new Point(_basePosition.X + (_isMale ? 0 : 0), _basePosition.Y + -24);
		if (_puppetActionTimer < 0.5f)
		{
			float amount = _puppetActionTimer / 0.5f;
			Position = _deathSequenceStartPosition.SineInterpolate(end, amount);
			_offscreenArm.Position = _deathSequenceOffscreenArmStartPosition.SineInterpolate(_offscreenArmBasePosition, amount);
			return;
		}
		float num = _puppetActionTimer - 0.5f;
		float num2 = num / 0.3f;
		num2 = (float)Math.Sin(num2 * ((float)Math.PI * 2f));
		int num3 = (int)(num2 * 20f);
		int num4 = (int)(num2 * 4f);
		if (num4 < 0)
		{
			num4 = -num4;
		}
		if (!_isMale)
		{
			num3 = -num3;
		}
		Position = new Point(end.X + num3, end.Y + num4);
	}

	private void FinishAttack()
	{
		_currentAction = EAIAction.Idle;
		_nextActionTimer = 0f;
		_offscreenArm.Position = _offscreenArmBasePosition;
	}

	public override void Kill()
	{
		SetIsActive(isActive: false);
		IsPlayingDead = true;
		_playDeadTimer = 0f;
		_playDeadFadeColorTimer = 0f;
		_nextPuppetAction = EDemonPuppetAction.Resting;
		PlayCue(_isMale ? ESFX.BossDemonPuppetLeftDeath : ESFX.BossDemonPuppetRightDeath);
	}

	private void Revive()
	{
		IsPlayingDead = false;
		base.HP = base.MaxHP;
		if (_lastIgnoredPuppetAction != EDemonPuppetAction.Resting)
		{
			SetIsActive(isActive: true);
			_nextPuppetAction = EDemonPuppetAction.StopResting;
		}
	}

	private void UpdatePlayDead(float delta)
	{
		if (IsPlayingDead)
		{
			_playDeadTimer += delta;
			if (_playDeadTimer >= 15f)
			{
				Revive();
			}
			if (_playDeadFadeColorTimer < 0.5f)
			{
				_playDeadFadeColorTimer += delta;
				float amount = 1f;
				if (_playDeadFadeColorTimer < 0.5f)
				{
					amount = _playDeadFadeColorTimer / 0.5f;
				}
				base.DrawColor = Color.White.SineInterpolate(PlayingDeadColor, amount);
			}
		}
		else
		{
			if (!(_playDeadFadeColorTimer > 0f))
			{
				return;
			}
			if (_lastIgnoredPuppetAction == EDemonPuppetAction.Resting)
			{
				_playDeadFadeColorTimer = 0f;
				return;
			}
			_playDeadFadeColorTimer -= delta;
			float amount2 = 0f;
			if (_playDeadFadeColorTimer > 0f)
			{
				amount2 = _playDeadFadeColorTimer / 0.5f;
			}
			base.DrawColor = Color.White.SineInterpolate(PlayingDeadColor, amount2);
		}
	}

	internal void SetNextPuppetAction(EDemonPuppetAction nextAction)
	{
		if (!IsPlayingDead)
		{
			_nextPuppetAction = nextAction;
			_lastIgnoredPuppetAction = EDemonPuppetAction.Idle;
		}
		else
		{
			_lastIgnoredPuppetAction = nextAction;
		}
	}

	internal void StartBossDeathSequence()
	{
		_deathSequenceStartPosition = Position;
		_deathSequenceOffscreenArmStartPosition = _offscreenArm.Position;
		_puppetAction = EDemonPuppetAction.BossDeath;
		_nextPuppetAction = EDemonPuppetAction.Idle;
		IsPlayingDead = false;
		_playDeadFadeColorTimer = 0f;
		_playDeadTimer = 0f;
		SetIsActive(isActive: false);
		_puppetActionTimer = 0f;
		_lastPuppetActionTimer = -0.0001f;
		_currentAction = EAIAction.Custom;
		_nextActionTimer = 1000f;
	}
}
