using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesSiren : Monster
{
	private const int InkSpeed = 75;

	private const float TimeToEmitInk = 0.75f;

	private const float TimeBetweenIndividualInks = 0.03f;

	private const float TimeForInkAttackCooldown = 4f;

	private const float TimeDeadBeforeBurning = 0f;

	private readonly bool _isExterminationQuestActive;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly CharacterSequenceSpecification _attackSequence;

	private readonly CavesSirenSplashParticleSystem _splashParticleSystem;

	private readonly CavesSirenInkDamageArea _inkDamageArea;

	private bool _isEmittingInk;

	private bool _hasAddInkToLevel;

	private float _inkEmissionTimer;

	private float _individualInkTimer;

	private float _inkCooldownTimer;

	private float _bubbleThrowTimer;

	private Point InkCreationPosition => new Point(Position.X, Position.Y + 24);

	public CavesSiren(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition.Add(0, -12), inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.StandAttack;
		_agility = 0.5f;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByWater = false;
		_doAppendagesMatchImageFacing = true;
		base.DoesNotMakeSplashesInWater = true;
		_doesDrawBaseSprite = false;
		_isExterminationQuestActive = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Captain, _level.GameSave) == 1 && NPCBase.GetSubQuestState(NPCBase.ENPCType.Captain, _level.GameSave) > 0;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count >= 6)
		{
			bool flag = _level.NextRandomInt(0, 1) == 0;
			_idleSequence = base.CharacterSpecification.Sequences[(!flag) ? 3 : 0];
			_turnSequence = base.CharacterSpecification.Sequences[flag ? 1 : 4];
			_attackSequence = base.CharacterSpecification.Sequences[flag ? 2 : 5];
		}
		_inkDamageArea = new CavesSirenInkDamageArea(_level, Point.Zero, ETeamSide.Enemies, base.Damage, _sprite);
		_splashParticleSystem = new CavesSirenSplashParticleSystem(_level.GCM.TxParticleEnergy, 3);
		_particleSystems.Add(_splashParticleSystem);
		_timeToTurnAround = 0.075f;
	}

	public override void InitializeMob()
	{
		StartIdleAnimation();
		base.InitializeMob();
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Idle)
		{
			StartIdleAnimation();
		}
	}

	private void StartIdleAnimation()
	{
		if (_idleSequence != null)
		{
			SetCharacterSequence(_idleSequence);
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_turnSequence != null)
		{
			SetCharacterSequence(_turnSequence);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_bubbleThrowTimer > 0f)
			{
				_bubbleThrowTimer -= delta;
				if (_bubbleThrowTimer <= 0f)
				{
					PlayCue(ESFX.EnemySirenSplashThrow, Position);
				}
			}
			if (_inkCooldownTimer > 0f)
			{
				_inkCooldownTimer -= delta;
			}
			if (_isEmittingInk)
			{
				if (_inkEmissionTimer <= 0f)
				{
					PlayCue(ESFX.EnemySirenInkAttack);
					_inkDamageArea.Reset(InkCreationPosition, base.Damage);
					if (!_hasAddInkToLevel)
					{
						_hasAddInkToLevel = true;
						_level.AddProjectile(_inkDamageArea);
					}
				}
				_inkEmissionTimer += delta;
				if (_inkEmissionTimer >= 0.75f)
				{
					EndInkEmission();
				}
				else
				{
					_individualInkTimer -= delta;
					if (_individualInkTimer <= 0f)
					{
						_individualInkTimer = 0.03f;
						ShootInk();
						ShootInk();
					}
				}
			}
		}
		base.Update(delta);
	}

	public override void UpdateAbility(float delta)
	{
		if (!(_abilityTimer <= 0f))
		{
			return;
		}
		if (_level.GetNearestProtagonistPosition(Position).Y < Position.Y + 16)
		{
			if (_attackSequence != null)
			{
				SetCharacterSequence(_attackSequence);
			}
			return;
		}
		if (_inkCooldownTimer <= 0f && !_isEmittingInk)
		{
			_isEmittingInk = true;
			_inkEmissionTimer = 0f;
			_inkCooldownTimer = 4f;
		}
		_isCarryingOutAbility = false;
		_abilityTimer = _totalAbilityTime;
	}

	private void SplashWater()
	{
		Point point = new Point(_bbox.Center.X, _bbox.Bottom);
		int num = ((!IsImageFacingLeft) ? 1 : (-1));
		Vector2 iV = new Vector2((float)num * 225f, -225f);
		_level.AddProjectile(new CavesSirenWaterProjectile(_level, point, iV, ETeamSide.Enemies, _sprite, base.Damage));
		iV = new Vector2((float)num * 225f * 0.75f, -112.5f);
		_level.AddProjectile(new CavesSirenWaterProjectile(_level, point, iV, ETeamSide.Enemies, _sprite, base.Damage));
		iV = new Vector2((float)num * 225f * 0.725f, -337.5f);
		_level.AddProjectile(new CavesSirenWaterProjectile(_level, point, iV, ETeamSide.Enemies, _sprite, base.Damage));
		iV = new Vector2((float)num * 225f * 0.35f, -450f);
		_level.AddProjectile(new CavesSirenWaterProjectile(_level, point, iV, ETeamSide.Enemies, _sprite, base.Damage));
		_splashParticleSystem.AddParticles(point.Add(0, -16).ToVector2(), new Vector2((!IsFacingLeft) ? 1 : (-1), 0f));
		PlayCue(ESFX.EnemySirenSplashStart, point);
		_bubbleThrowTimer = 1f;
	}

	private void ShootInk()
	{
		int num = _level.NextRandomInt(-75, 75);
		Vector2 iV = new Vector2(num, 75f);
		_inkDamageArea.EmitInk(InkCreationPosition, iV);
	}

	private void EndInkEmission()
	{
		_isEmittingInk = false;
		_inkEmissionTimer = 0f;
		_individualInkTimer = 0f;
		_inkDamageArea.End();
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			DropLoot();
			_inkDamageArea.SilentKill();
			if (_isExterminationQuestActive && !_level.GameSave.GetSaveBool("HasShownSirenQuestFinished"))
			{
				int newEnemyKillCount = NPCBase.GetNewEnemyKillCount(EEnemyTileType.CavesSiren, _level.GameSave);
				if (newEnemyKillCount >= 10)
				{
					_level.AddScript(new ScriptAction(NPCBase.ENPCType.Captain, 2));
					_level.GameSave.SetValue("HasShownSirenQuestFinished", value: true);
				}
			}
		}
		if (_deathScriptTimer > 0f)
		{
			DistintegrateEvent newEvent = new DistintegrateEvent(this, _sprite, EDisintegrateType.Chaos, 96, 4, new Vector4(0.8f, 0.3f, 0.9f, 1f));
			_level.AddEvent(newEvent);
			RemoveInstance();
		}
		else
		{
			UpdateCharacterSequences(delta);
			UpdateAnimation(delta);
			UpdateAppendages(delta);
			UpdateParticleSystems(delta);
		}
		_deathScriptTimer += delta;
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
			SplashWater();
			break;
		case 1:
			_isCarryingOutAbility = false;
			break;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_inkDamageArea.DrawInk(spriteBatch);
		base.Draw(spriteBatch);
	}
}
