using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreZel : Monster
{
	private const int MaxSimultaneousSouls = 12;

	private const int SwordCount = 5;

	private const int Anim_Book_IdleStart = 0;

	private const int Anim_Book_IdleLength = 18;

	private const int Anim_Book_OpenStart = 18;

	private const int Anim_Book_OpenLength = 6;

	private const int Anim_Book_ReadStart = 24;

	private const int Anim_Book_ReadLength = 2;

	private const float Anim_Book_IdleSpeed = 0.1f;

	private const float TimeBeforeSummoningSwords = 0.5f;

	private const float TimeBetweenEachSword = 0.1f;

	private const float TimeForOneSword = 1f;

	private const float TimeForAllSwords = 1.4f;

	private const float TimeForEntireAbility = 1.9f;

	private const int DeathPushOffsetX = 48;

	private const int DeathPushOffsetY = -16;

	private const float DeathTimeToDisappear = 1.25f;

	private const float DeathTimeToWait = 0.5f;

	private const float DeathTimeForBookToClose = 1f;

	private const float TimeBeforeDeathBookClose = 1.75f;

	private const float TimeForEntireDeathSequence = 2.75f;

	private const float TimeBetweenEmittingDeathSouls = 0.1f;

	private const int BossIntroFinalGlowColor = 16;

	private const float TimeBetweenEmittingBossIntroChargeParticles = 0.033f;

	private const float IntroTimeForIdling = 1f;

	private const float IntroTimeForChargingUp = 2f;

	private const float IntroScreenFlashDuration = 0.4f;

	private const float IntroTimeBeforeAddingScreenFlash = 2.8f;

	private static readonly Color BaseAuraColor = new Color(0.75f, 0.5f, 0.25f, 0.25f);

	private static readonly Color SoulTrailColor = new Color(0.75f, 0.3f, 0.1f, 0.25f);

	private static readonly Color BossIntroScreenFlashColor = new Color(255, 248, 240);

	private static readonly Color BossIntroLevelDrawColor = new Color(160, 128, 96);

	private static readonly Vector4 SoulParticlesColor = new Vector4(0.75f, 0.3f, 0.1f, 0.5f);

	private readonly int _swordDamage;

	private readonly CharacterSequenceSpecification _attackSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly Appendage _bookAppendage;

	private readonly SpriteSheet _bookSheet;

	private readonly GyreZelSwordAppendage[] _swords = new GyreZelSwordAppendage[5];

	private readonly SoulTrailAppendage[] _deathSouls = new SoulTrailAppendage[12];

	private bool _isDoingBossIntro;

	private float _deathSoulEmissionTimer;

	private float _lastDeathScriptTimer;

	private float _bossIntroTimer;

	private Point _deathPoint;

	private XarionChargeLazerParticleSystem _bossIntroChargeParticles;

	internal float TimeForBossIntroSequence { get; private set; }

	internal Point BookCenter
	{
		get
		{
			if (_bookAppendage != null)
			{
				return _bookAppendage.Bbox.Center;
			}
			return Point.Zero;
		}
	}

	public GyreZel(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Fly;
		_nonAggroAction = EAIAction.FloatInPlace;
		_agility = 0.25f;
		_bboxOffset = new Point(4, 3);
		Bbox = new Rectangle(_position.X, _position.Y, 14, 32);
		_isAlwaysAggroed = true;
		_canLoseAggro = false;
		_swordDamage = (int)Math.Ceiling((float)base.Damage * 1.15f);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_isAffectedByGravity = false;
		_isFlying = false;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(1f, 1f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.1f;
		_auraCount = 5f;
		ChangeAnimation(0, 4, 0.12f, EAnimationType.Cycle);
		_bookSheet = _level.GCM.SpOrbMeleeBook;
		Animate parent = this;
		if (_appendages.Count > 1)
		{
			parent = _appendages[1];
		}
		_bookAppendage = new Appendage(parent, new Point(8, 8), new Point(4, 4), _level, _bookSheet)
		{
			DoesDrawTrail = true,
			TrailLength = 4,
			TrailFadeRate = 4f,
			DrawPriority = 1,
			FollowType = EAppendageFollowType.AnchorLocked,
			DoesInheritDrawColor = false
		};
		_bookAppendage.ChangeAnimation(0, 18, 0.1f, EAnimationType.Cycle);
		base.Appendages.Add(_bookAppendage);
		for (int i = 0; i < 5; i++)
		{
			GyreZelSwordAppendage gyreZelSwordAppendage = new GyreZelSwordAppendage(this, new Point(1, 1), new Point(36, 6), _level, _bookSheet, i);
			_swords[i] = gyreZelSwordAppendage;
		}
		_attackSequence = GetCharacterSequenceByName("Attack");
		_deathSequence = GetCharacterSequenceByName("Death");
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			GyreZelSwordAppendage[] swords = _swords;
			foreach (GyreZelSwordAppendage gyreZelSwordAppendage in swords)
			{
				if (!gyreZelSwordAppendage.IsFinished)
				{
					gyreZelSwordAppendage.CurrentTarget = Position;
				}
				else if (!gyreZelSwordAppendage.IsAvailable)
				{
					base.Appendages.Remove(gyreZelSwordAppendage);
					gyreZelSwordAppendage.IsAvailable = true;
				}
			}
			if (_isDoingBossIntro)
			{
				UpdateBossIntroSequence(delta);
			}
		}
		base.Update(delta);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer > 1.9f)
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.FloatInPlace;
			_nextActionTimer = _timeToIdleAfterAttacking;
			CloseBook();
			return;
		}
		if (_abilityTimer <= 0f)
		{
			SetCharacterSequence(_attackSequence);
			PlayCue(ESFX.BossZelHumanCast);
			OpenBook();
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0.1f * (float)i + 0.5f;
			if (_abilityTimer >= num && _lastAbilityTimer < num)
			{
				GyreZelSwordAppendage gyreZelSwordAppendage = _swords[i];
				gyreZelSwordAppendage.Reset(Position, IsFacingLeft, _swordDamage);
				base.Appendages.Add(gyreZelSwordAppendage);
			}
		}
		_currentAction = EAIAction.FloatInPlace;
		DoAction(delta);
		_currentAction = EAIAction.DoAbility;
	}

	private void OpenBook()
	{
		_bookAppendage.ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 18,
				Length = 6,
				Speed = 0.075f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 24,
				Length = 2,
				Speed = 0.03f,
				Type = EAnimationType.Cycle
			}
		});
	}

	private void CloseBook()
	{
		_bookAppendage.ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 18,
				Length = 6,
				Speed = 0.1f,
				Type = EAnimationType.Once,
				IsInReverse = true
			},
			new AnimationSpec
			{
				Start = 0,
				Length = 18,
				Speed = 0.1f,
				Type = EAnimationType.Cycle
			}
		});
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			DropLoot();
			base.IsSolidWhenFrozen = false;
			SetCharacterSequence(_deathSequence);
			_level.PlayCue(ESFX.BossZelHumanDeath, Position);
			_deathPoint = Position;
			_doAppendagesInheritDrawColor = false;
			_bookAppendage.FollowType = EAppendageFollowType.None;
			OpenBook();
			DrawOrigin = new Vector2(18f, 25f);
		}
		if (_deathScriptTimer >= 2.75f)
		{
			_level.AddAnimation(EBattleAnimationType.Poof, _bookAppendage.Bbox.Center, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: false);
			RemoveInstance();
		}
		else
		{
			if (_deathScriptTimer < 1.25f)
			{
				float num = _deathScriptTimer / 1.25f;
				int num2 = (int)(Math.Sin(num * ((float)Math.PI / 2f)) * 48.0);
				int num3 = (int)Math.Ceiling(Math.Sin(num * (float)Math.PI) * -16.0);
				Position = new Point(_deathPoint.X + (IsFacingLeft ? num2 : (-num2)), _deathPoint.Y + num3);
				float num4 = (_scale = 1f - num);
				base.DrawColor = Color.White * num4;
				base.AuraColor = BaseAuraColor * num4;
				_deathSoulEmissionTimer -= delta;
				if (_deathSoulEmissionTimer <= 0f)
				{
					AddDeathSoul();
					_deathSoulEmissionTimer += 0.1f;
				}
			}
			else if (_deathScriptTimer < 1.75f)
			{
				if (_lastDeathScriptTimer < 1.25f)
				{
					ChangeAnimation(-1);
				}
			}
			else if (_lastDeathScriptTimer < 1.75f)
			{
				CloseBook();
			}
			SnapBboxToPosition();
			SnapFrameToBbox();
			UpdateCharacterSequences(delta);
			UpdateAnimation(delta);
			UpdateAppendages(delta);
			UpdateTrail(delta);
			UpdateParticleSystems(delta);
		}
		_lastDeathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
	}

	private void AddDeathSoul()
	{
		Point position = new Point(Position.X + (IsFacingLeft ? 8 : 0), Position.Y - 4);
		Point position2 = _bookAppendage.Position;
		for (int i = 0; i < 12; i++)
		{
			if (_deathSouls[i] == null)
			{
				SoulTrailAppendage soulTrailAppendage = new SoulTrailAppendage(this, new Point(8, 8), Point.Zero, _level, _sprite, SoulTrailColor, SoulParticlesColor);
				soulTrailAppendage.Position = position;
				soulTrailAppendage.TargetOrigin = position2;
				SoulTrailAppendage soulTrailAppendage2 = soulTrailAppendage;
				base.Appendages.Add(soulTrailAppendage2);
				_deathSouls[i] = soulTrailAppendage2;
				break;
			}
			if (_deathSouls[i].IsFinished)
			{
				SoulTrailAppendage soulTrailAppendage3 = _deathSouls[i];
				soulTrailAppendage3.Reset(position);
				soulTrailAppendage3.TargetOrigin = position2;
				break;
			}
		}
	}

	internal void StartBossIntroCutscene()
	{
		_isDoingBossIntro = true;
		_currentAI = EAIStrategy.None;
		_currentAction = EAIAction.None;
		_canBeDamaged = false;
		_isAggroed = true;
		_isAlwaysAggroed = true;
		_damageCaused = 0;
		TimeForBossIntroSequence = 4f;
		SetCharacterSequenceByName("BossIntro");
		_bossIntroChargeParticles = new XarionChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 16);
		_particleSystems.Add(_bossIntroChargeParticles);
	}

	private void UpdateBossIntroSequence(float delta)
	{
		TimeForBossIntroSequence = 3f;
		float bossIntroTimer = _bossIntroTimer;
		_bossIntroTimer += delta;
		if (!(_bossIntroTimer <= TimeForBossIntroSequence) || !(_bossIntroTimer >= 1f))
		{
			return;
		}
		if (bossIntroTimer < 1f)
		{
			SetCharacterSequenceByName("BossIntro2");
			OpenBook();
			base.IsGlowing = true;
			base.GlowBase = 1f;
			base.GlowColor = new Color(1f, 0.9f, 0.5f, 0.5f);
			_bookAppendage.DoesInheritDrawColor = true;
			_level.PlayCue(ESFX.BossZelTransform, Position);
		}
		float amount = (_bossIntroTimer - 1f) / TimeForBossIntroSequence;
		base.GlowBase = MathHelper.Lerp(1f, 16f, amount);
		Color levelDrawColor = Color.White.Lerp(BossIntroLevelDrawColor, amount);
		_level.SetLevelDrawColor(levelDrawColor);
		if (_bossIntroTimer >= 2.8f && bossIntroTimer < 2.8f)
		{
			Color bossIntroScreenFlashColor = BossIntroScreenFlashColor;
			_level.RequestScreenFlash(new ScreenFlash(0.4f)
			{
				Frequency = 1f,
				EffectColor = bossIntroScreenFlashColor
			});
		}
		if (_bossIntroChargeParticles != null)
		{
			_deathSoulEmissionTimer -= delta;
			if (_deathSoulEmissionTimer <= 0f)
			{
				_bossIntroChargeParticles.MaxStartRadius = MathHelper.Lerp(32f, 64f, amount);
				_bossIntroChargeParticles.AddParticles(_bookAppendage.Bbox.Center.ToVector2());
				_deathSoulEmissionTimer += 0.033f;
			}
		}
	}
}
