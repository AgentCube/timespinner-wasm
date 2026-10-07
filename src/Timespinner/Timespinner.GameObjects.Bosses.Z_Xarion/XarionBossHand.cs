using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Bosses.Z_Xarion;

internal class XarionBossHand : Monster
{
	private enum EXarionBossHandState
	{
		Idle,
		Attacking,
		Dying,
		Dead
	}

	private const int ClawStartX = 192;

	private const int ClawStartY = 152;

	private const int LeftWallX = 60;

	private const int ClawWindupY = -20;

	private const int ClawWallSlamX = 60;

	private const int FloorY = 238;

	private const int WallSlamDustX = 35;

	private const int IdleRadiusX = 4;

	private const int IdleRadiusY = 10;

	private const float TimeForIdle = 1.2f;

	private const float TimeForSwipeAppear = 0.5f;

	private const float TimeForSwipeDown = 0.2f;

	private const float TimeForSwipeFloorSlam = 0.75f;

	private const float TimeForSwipeOut = 0.2f;

	private const float TimeForSwipeWallSlam = 0.5f;

	private const float TimeForSwipeReturn = 0.5f;

	private const float TimeForSwipeFinish = 0f;

	private const float TimeBeforeSwipeFloorSlam = 0.7f;

	private const float TimeBeforeSwipeOut = 1.45f;

	private const float TimeBeforeSwipeWallSlam = 1.65f;

	private const float TimeBeforeSwipeReturn = 2.15f;

	private const float TimeBeforeSwipeFinish = 2.65f;

	internal const float TimeForEntireSwipeSequence = 2.65f;

	private const float DeathTimeToWrithe = 1f;

	private const float DeathTimeToFall = 0.4f;

	private const float TimeForEntireDeathSequence = 1.4f;

	private const int DeathShakeRadius = 8;

	private static readonly Color BaseAuraColor = new Color(0.5f, 0.25f, 0.75f, 0.35f);

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _windupSequence;

	private readonly CharacterSequenceSpecification _attackSequence;

	private readonly CharacterSequenceSpecification _returnSequence;

	private readonly LandingDustParticleSystem _landingDustParticles;

	private readonly XarionWallDustParticleSystem _wallDustParticles;

	private readonly XarionChargeLazerParticleSystem _deathLazerParticles;

	private bool _isWaitingToDie;

	private EXarionBossHandState _handState;

	private float _stateTimer;

	private float _lastStateTimer;

	private Point _idlePosition;

	internal bool IsDying
	{
		get
		{
			if (!_isWaitingToDie && _handState != EXarionBossHandState.Dying)
			{
				return _handState == EXarionBossHandState.Dead;
			}
			return true;
		}
	}

	public XarionBossHand(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		ChangeAnimation(-1);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
		_isAffectedByGravity = false;
		_isFlying = true;
		_doesCollideWithTiles = false;
		base.DoesTouchDamageKnockback = true;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(-1f, 0f);
		base.AuraFrequency = 1f;
		base.AuraSize = 0.02f;
		_auraCount = 5f;
		SetDoesDrawAppendageTrails(value: true, isHost: true, 6, 3f);
		_deathLazerParticles = new XarionChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 16);
		_idleSequence = GetCharacterSequenceByName("Idle");
		_windupSequence = GetCharacterSequenceByName("Windup");
		_attackSequence = GetCharacterSequenceByName("Attack");
		_returnSequence = GetCharacterSequenceByName("Return");
		StartIdle();
		_landingDustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 1, _level.ID, 100);
		_wallDustParticles = new XarionWallDustParticleSystem(_level.GCM.TxParticleDust, 1, _level.ID, 100);
		_particleSystems.Add(_landingDustParticles);
		_particleSystems.Add(_wallDustParticles);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_lastStateTimer = _stateTimer;
			_stateTimer += delta;
			if (_handState == EXarionBossHandState.Idle && _isWaitingToDie)
			{
				SetCharacterSequenceByName("Death");
				_stateTimer = 0f;
				_lastStateTimer = -1f;
				_handState = EXarionBossHandState.Dying;
				_isWaitingToDie = false;
			}
			switch (_handState)
			{
			case EXarionBossHandState.Idle:
				UpdateIdle();
				break;
			case EXarionBossHandState.Attacking:
				UpdateAttacking();
				break;
			case EXarionBossHandState.Dying:
				UpdateDying();
				break;
			case EXarionBossHandState.Dead:
				UpdateDead();
				break;
			}
		}
		base.Update(delta);
	}

	private void UpdateIdle()
	{
		if (_stateTimer > 1.2f)
		{
			_stateTimer = 0f;
			SetCharacterSequence(_idleSequence);
		}
		float num = _stateTimer / 1.2f;
		int num2 = (int)Math.Round(Math.Cos(num * ((float)Math.PI * 2f)) * 4.0);
		int num3 = -(int)Math.Round(Math.Sin(num * ((float)Math.PI * 2f)) * 10.0);
		Position = new Point(192 + num2, 152 + num3);
		_idlePosition = Position;
	}

	private void UpdateAttacking()
	{
		if (_stateTimer < 2.65f)
		{
			if (_stateTimer < 0.5f)
			{
				if (_lastStateTimer <= 0f)
				{
					PlayCue(ESFX.BossXarionClaw);
				}
				float num = _stateTimer / 0.5f;
				int num2 = (int)Math.Floor(Math.Sin(num * ((float)Math.PI / 2f)) * -20.0);
				int y = (int)Math.Round(MathEx.CosInterpolate(_idlePosition.Y, _idlePosition.Y + num2, num));
				Position = new Point(_idlePosition.X, y);
			}
			else if (_stateTimer < 0.7f)
			{
				if (_lastStateTimer < 0.5f)
				{
					SetCharacterSequence(_attackSequence);
				}
				float percentage = (_stateTimer - 0.5f) / 0.2f;
				int y2 = (int)Math.Round(MathEx.CosInterpolate(_idlePosition.Y + -20, 238f, percentage));
				Position = new Point(_idlePosition.X, y2);
			}
			else if (_stateTimer < 1.45f)
			{
				if (_lastStateTimer < 0.7f)
				{
					Position = new Point(192, 238);
					OnGroundSlam();
				}
			}
			else if (_stateTimer < 1.65f)
			{
				if (_lastStateTimer < 1.45f)
				{
					SetCharacterSequence(_windupSequence);
				}
				float percentage2 = (_stateTimer - 1.45f) / 0.2f;
				int x = (int)Math.Round(MathEx.CosInterpolate(192f, 60f, percentage2));
				Position = new Point(x, 238);
			}
			else if (_stateTimer < 2.15f)
			{
				if (_lastStateTimer < 1.65f)
				{
					Position = new Point(60, 238);
					OnWallSlam();
				}
			}
			else if (_stateTimer < 2.65f)
			{
				if (_lastStateTimer < 2.15f)
				{
					SetCharacterSequence(_returnSequence);
				}
				float num3 = (_stateTimer - 2.15f) / 0.5f;
				int x2 = (int)Math.Round(MathHelper.Lerp(60f, 192f, num3));
				int y3 = (int)Math.Round(MathEx.CosInterpolate(238f, 152f, num3));
				Position = new Point(x2, y3);
			}
		}
		else
		{
			Position = new Point(192, 152);
			StartIdle();
		}
	}

	private void UpdateDying()
	{
		if (_stateTimer < 1f)
		{
			float amount = _stateTimer / 1f;
			int num = (int)(_level.NextRandomDouble() * 8.0);
			Position = new Point(_idlePosition.X + num, _idlePosition.Y + num);
			base.IsGlowing = true;
			base.GlowColor = new Color(1f, 1f, 1f, 0.5f);
			base.GlowBase = (int)MathHelper.Lerp(2f, 8f, amount);
			_deathLazerParticles.AddParticles(base.OuterBbox.Center.ToVector2());
		}
		else if (_stateTimer < 1.4f)
		{
			if (_lastStateTimer < 1f)
			{
				SetCharacterSequence(_returnSequence);
			}
			float num2 = (_stateTimer - 1f) / 0.4f;
			num2 = 1f - (float)Math.Cos(num2 * ((float)Math.PI / 2f));
			int y = (int)Math.Round(MathEx.CosInterpolate(_idlePosition.Y, 238f, num2));
			Position = new Point(_idlePosition.X, y);
			base.GlowBase = (int)MathHelper.Lerp(8f, 2f, num2);
		}
		else
		{
			_stateTimer = 0f;
			_lastStateTimer = -1f;
			_handState = EXarionBossHandState.Dead;
			OnGroundSlam();
			_level.AddAnimation(EBattleAnimationType.Boom, base.OuterBbox.Center, ETeamSide.Enemies);
			DebrisEvent.CreateFromObject(this, new Vector2(0f, -10f), base.OuterBbox.Center, _sprite, DebrisEvent.EDebrisDeathType.Fire);
			_appendages.Clear();
			SilentKill();
		}
	}

	private void UpdateDead()
	{
	}

	internal void SetPosition(Point position)
	{
		Position = position;
		SnapBboxToPosition();
		SnapFrameToBbox();
		_intermediatePositions.Clear();
	}

	internal void StartAttack()
	{
		if (_handState != EXarionBossHandState.Dying && _handState != EXarionBossHandState.Dead)
		{
			_stateTimer = 0f;
			_lastStateTimer = -1f;
			_handState = EXarionBossHandState.Attacking;
			SetCharacterSequence(_windupSequence);
		}
	}

	private void StartIdle()
	{
		_stateTimer = 0f;
		_lastAbilityTimer = -1f;
		_handState = EXarionBossHandState.Idle;
		SetCharacterSequence(_idleSequence);
	}

	internal void OnGroundSlam()
	{
		_landingDustParticles.AddParticles(new Vector2(Position.X, Bbox.Bottom), 200f);
		_level.RequestScreenShake(new Vector2(0f, 4f), 0.75f, 20f, isAffectedByTime: true);
	}

	internal void OnWallSlam()
	{
		_wallDustParticles.AddParticles(new Vector2(35f, Position.Y), 200f);
		_level.RequestScreenShake(new Vector2(4f, 0f), 0.75f, 20f, isAffectedByTime: true);
	}

	protected override void StartDeathScript()
	{
		_isWaitingToDie = true;
		_particleSystems.Add(_deathLazerParticles);
		_level.PlayCue(ESFX.BossXarionClawDeath, Position);
		base.StartDeathScript();
	}

	protected override void UpdateDeathScript(float delta)
	{
		SetPosition(Position);
		UpdateAppendages(delta);
		UpdateCharacterSequences(delta);
		UpdateParticleSystems(delta);
		UpdateTrail(delta);
	}
}
