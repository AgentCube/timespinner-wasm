using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LakeAnemone : Monster
{
	private enum EMorningStarState
	{
		None,
		Hidden,
		Rise,
		Searching
	}

	private const int BouyancyOffsetY = 12;

	private const int BouyancyVelocity = 100;

	private const float FloatAmplitude = 6f;

	private const float FloatFrequency = 1f;

	private const int MorningStarLinkCount = 20;

	private const int BaseStarOffsetX = 1;

	private const int BaseStarOffsetY = -112;

	private const int StarOscillationRadius = 8;

	private const int StarHiddenAmplitude = 2;

	private const int StarRiseAmplitude = 8;

	private const float TimeToAppear = 1.6f;

	private const float TimeToHide = 3f;

	private const float TimeToTransitionIntoSearching = 0.75f;

	private const float StarOscillationFrequency = 1f;

	private const float DeathOscillationFrequency = 5f;

	private const float TimeBetweenAppendageDeaths = 0.05f;

	private static readonly Point HiddenOffset = Point.Zero;

	private static readonly Point RiseOffset = new Point(1, -112);

	private readonly bool _isFloating;

	private readonly bool _isOnCeiling;

	private readonly Appendage _morningStarAppendage;

	private readonly CharacterSequenceSpecification _riseSequence;

	private readonly CharacterSequenceSpecification _aggroSequence;

	private readonly CharacterSequenceSpecification _deaggroSequence;

	private readonly CharacterSequenceSpecification _aggroAnchoredSequence;

	private readonly CharacterSequenceSpecification _deaggroAnchoredSequence;

	private EMorningStarState _morningStarState;

	private EMorningStarState _lastMorningStarState;

	private int _waterTop;

	private int _deathAppendagesRemaining = 20;

	private float _floatTimer;

	private float _morningStarTimer;

	private float _morningStarOscillationTimer;

	private float _appendageKillTimer;

	private Point _morningStarTargetOffset;

	private Point _lastMorningStarStateOffset = Point.Zero;

	public LakeAnemone(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.None;
		Bbox = new Rectangle(_position.X, _position.Y, 1, 1);
		base.DeaggroBboxDimensions = new Point(500, 500);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByWater = false;
		base.DoesCollideWithTiles = false;
		base.DoesTouchDamageKnockback = true;
		base.CannotBeGrabbed = true;
		_doesAggroOnTakingDamage = false;
		_doAppendagesMatchImageFacing = true;
		_doesDrawBaseSprite = false;
		_isFloating = objectSpec == null || objectSpec.Argument == 0;
		if (!_isFloating && objectSpec != null)
		{
			_isOnCeiling = objectSpec.IsFlippedVertically;
			IsFlippedVertically = _isOnCeiling;
			if (_isOnCeiling)
			{
				Position = Position.Add(0, -16);
				SnapBboxToPosition();
			}
		}
		_morningStarAppendage = base.Appendages[5];
		_morningStarAppendage.OscillAmplitude = 2f;
		_morningStarAppendage.OscillFrequency = 3f;
		_morningStarAppendage.OscillIncrement = 0.25f;
		_morningStarAppendage.OscillDelta = (float)(_level.NextRandomDouble() * 6.0);
		_morningStarAppendage.FollowType = EAppendageFollowType.None;
		_morningStarAppendage.Position = Position;
		_morningStarAppendage.AddLinks(20, EAppendageFollowType.TrigoInterpolate, new Rectangle(Position.X, Position.Y, 7, 7), new Point(1, 1), 23, Point.Zero);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 4)
		{
			_riseSequence = base.CharacterSpecification.Sequences[0];
			_aggroSequence = base.CharacterSpecification.Sequences[1];
			_deaggroSequence = base.CharacterSpecification.Sequences[2];
			_aggroAnchoredSequence = base.CharacterSpecification.Sequences[3];
			_deaggroAnchoredSequence = base.CharacterSpecification.Sequences[4];
		}
	}

	public override void InitializeMob()
	{
		SetCharacterSequence(_riseSequence);
		_waterTop = _level.GetWaterTopFromBelowWater(Position);
		base.InitializeMob();
	}

	protected override void OnAggroed()
	{
		SetCharacterSequence(_isFloating ? _aggroSequence : _aggroAnchoredSequence);
		_morningStarState = EMorningStarState.Rise;
		PlayCue(ESFX.EnemyBarbedAnemoneDeploy);
		base.OnAggroed();
	}

	protected override void OnDeAggroed()
	{
		SetCharacterSequence(_isFloating ? _deaggroSequence : _deaggroAnchoredSequence);
		_morningStarState = EMorningStarState.Hidden;
		PlayCue(ESFX.EnemyBarbedAnemoneRetract);
		base.OnDeAggroed();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isFloating)
			{
				UpdateFloating(delta);
			}
			UpdateMorningStarHead(delta);
		}
		base.Update(delta);
	}

	private void UpdateFloating(float delta)
	{
		_floatTimer += delta;
		if (_floatTimer >= 1f)
		{
			_floatTimer -= 1f;
		}
		float num = (float)Math.Sin(_floatTimer * ((float)Math.PI * 2f) * 1f) * 6f * delta;
		if (_isAggroed)
		{
			int num2 = Position.Y - _waterTop + 12;
			if (num2 > 0)
			{
				_velocity.Y -= (float)num2 * delta * 100f;
			}
		}
		else
		{
			int num3 = Position.Y - _waterTop - 12;
			if (num3 < 0)
			{
				_velocity.Y -= (float)num3 * delta * 100f;
			}
		}
		_floatPosition = new Vector2(_floatPosition.X, _floatPosition.Y + num);
	}

	private void UpdateMorningStarHead(float delta)
	{
		float num = _morningStarAppendage.OscillAmplitude;
		if (_lastMorningStarState != _morningStarState)
		{
			_lastMorningStarStateOffset = _morningStarTargetOffset;
			_morningStarTimer = 0f;
		}
		switch (_morningStarState)
		{
		case EMorningStarState.None:
			_morningStarState = EMorningStarState.Hidden;
			_morningStarTargetOffset = HiddenOffset;
			_morningStarTimer = 3f;
			break;
		case EMorningStarState.Hidden:
			if (_morningStarTimer <= 3f)
			{
				_morningStarTimer += delta;
				float num3 = _morningStarTimer / 3f;
				if (num3 >= 1f)
				{
					_morningStarTargetOffset = HiddenOffset;
					num = 2f;
				}
				else
				{
					_morningStarTargetOffset = _lastMorningStarStateOffset.SineInterpolate(HiddenOffset, num3);
					num = MathHelper.Lerp(num, 2f, num3);
				}
			}
			break;
		case EMorningStarState.Rise:
			if (_morningStarTimer <= 1.6f)
			{
				_morningStarTimer += delta;
				float num2 = _morningStarTimer / 1.6f;
				if (num2 >= 1f)
				{
					_morningStarTargetOffset = RiseOffset;
					num = 8f;
					_morningStarState = EMorningStarState.Searching;
					_morningStarTimer = 0f;
					_lastMorningStarStateOffset = RiseOffset;
				}
				else
				{
					_morningStarTargetOffset = _lastMorningStarStateOffset.SineInterpolate(RiseOffset, num2);
					num = MathHelper.Lerp(num, 8f, num2);
				}
			}
			break;
		case EMorningStarState.Searching:
			_morningStarOscillationTimer += delta * 1f;
			if (_morningStarOscillationTimer >= (float)Math.PI * 2f)
			{
				_morningStarOscillationTimer -= (float)Math.PI * 2f;
			}
			_morningStarTargetOffset = MathEx.Add(b: new Point((int)(Math.Cos(_morningStarOscillationTimer) * 8.0 * 2.0), (int)(Math.Sin(_morningStarOscillationTimer * 2f) * 8.0)), a: RiseOffset);
			_morningStarTimer += delta;
			if (_morningStarTimer < 0.75f)
			{
				float amount = _morningStarTimer / 0.75f;
				_morningStarTargetOffset = _lastMorningStarStateOffset.Lerp(_morningStarTargetOffset, amount);
			}
			break;
		}
		if (_isOnCeiling)
		{
			_morningStarTargetOffset = new Point(_morningStarTargetOffset.X, -_morningStarTargetOffset.Y);
		}
		_morningStarAppendage.Position = Position.Add(_morningStarTargetOffset);
		_morningStarAppendage.OscillAmplitude = num;
		foreach (Appendage appendage in _morningStarAppendage.Appendages)
		{
			appendage.OscillAmplitude = num;
		}
		_lastMorningStarState = _morningStarState;
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			DropLoot();
			foreach (Appendage appendage2 in _morningStarAppendage.Appendages)
			{
				appendage2.OscillFrequency = 5f;
			}
			_level.AddAnimation(EBattleAnimationType.WetSplashLarge, _morningStarAppendage.Bbox.Center, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
			_morningStarAppendage.ChangeAnimation(-1);
			_morningStarAppendage.DoesCollideWithAnything = false;
		}
		_deathScriptTimer += delta;
		_appendageKillTimer += delta;
		if (_isFloating)
		{
			UpdateFloating(delta);
			Position = _floatPosition.ToPoint();
		}
		UpdateMorningStarHead(delta);
		UpdateAppendages(delta);
		if (!(_appendageKillTimer >= 0.05f))
		{
			return;
		}
		_appendageKillTimer = 0f;
		if (_deathAppendagesRemaining > 0 && _isAggroed)
		{
			Appendage appendage = _morningStarAppendage.Appendages[20 - _deathAppendagesRemaining];
			_level.AddAnimation(EBattleAnimationType.WetSplashSmall, appendage.Bbox.Center, ETeamSide.Enemies, isFacingRight: true, _deathAppendagesRemaining % 4 == 0);
			appendage.ChangeAnimation(-1);
			appendage.DoesCollideWithAnything = false;
			_deathAppendagesRemaining--;
			return;
		}
		if (_isFloating && _isAggroed)
		{
			Point point = Position.Add(0, 8);
			_level.AddAnimation(EBattleAnimationType.WetSplashLarge, point, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
			_level.AddAnimation(EBattleAnimationType.WetSplashSmall, point.Add(0, 16), ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: false);
			_level.AddAnimation(EBattleAnimationType.WetSplashSmall, point.Add(0, -16), ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: false);
			_level.AddAnimation(EBattleAnimationType.WetSplashSmall, point.Add(16, 0), ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: false);
			_level.AddAnimation(EBattleAnimationType.WetSplashSmall, point.Add(-16, 0), ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: false);
		}
		else
		{
			_level.AddAnimation(EBattleAnimationType.WetSplashLarge, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		}
		RemoveInstance();
	}
}
