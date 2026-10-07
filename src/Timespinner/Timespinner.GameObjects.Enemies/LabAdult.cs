using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._11_Laboratory;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LabAdult : Monster
{
	internal const int GlassShardCount = 4;

	private const float TimeBeforeStartingFadeOut = 0.5f;

	private const float TimeToFadeOut = 0.75f;

	private const float TimeForEntireFade = 1.25f;

	private const float TimeToWaitAfterStartingAttack = 3f;

	private const float TimeBetweenShardAttacks = 0.33f;

	private static readonly Color FadedColor = new Color(0.65f, 0.65f, 0.7f);

	private static readonly Color AdultAuraColor = new Color(0.5f, 0.1f, 0.15f, 0.25f);

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly LabAdultBlockerEvent _blocker;

	private readonly LabAdultGlassShard[] _shards = new LabAdultGlassShard[4];

	private bool _isPlayerToLeft;

	public LabAdult(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_doesDrawBaseSprite = false;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		base.DoesTouchDamageKnockback = true;
		base.CannotBeGrabbed = true;
		ChangeAnimation(-1);
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 0.25f;
		_bboxOffset = new Point(8, 16);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		base.DoesDrawAura = true;
		base.AuraColor = AdultAuraColor;
		base.AuraSize = 0.2f;
		base.DoesDrawAppendageAuras = true;
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		_auraCount = 5f;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 2)
		{
			_idleSequence = base.CharacterSpecification.Sequences[0];
			_deathSequence = base.CharacterSpecification.Sequences[2];
			SetCharacterSequence(_idleSequence);
		}
		for (int i = 0; i < 4; i++)
		{
			LabAdultGlassShard labAdultGlassShard = new LabAdultGlassShard(_level, Position.Add(i * 16, -80), this, _sprite, i, base.Damage);
			_shards[i] = labAdultGlassShard;
			_level.AddProjectile(labAdultGlassShard);
		}
		_blocker = new LabAdultBlockerEvent(_level, Position, -1, new ObjectTileSpecification());
		_level.RequestAddObject(_blocker);
	}

	public override void InitializeMob()
	{
		base.InitializeMob();
		if (_level.GameSave.GetSaveBool("11_Exp13"))
		{
			SetDeadOnStart();
			return;
		}
		DoMiniBossDoorLock();
		PlayCue(ESFX.EnemyLabAdultGroan);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_lastAction == EAIAction.DoAbility)
		{
			_currentAction = EAIAction.Idle;
			_nextActionTimer = (float)_random.Next(1, 3) * _timeToIdleAfterAttacking;
		}
		else
		{
			_currentAction = EAIAction.DoAbility;
			_selectedAbility = 1;
			_nextActionTimer = 0f;
		}
	}

	public override void StartAbility(int whichAbility)
	{
		if (_level.NextRandomInt(0, 1) == 0)
		{
			whichAbility = 2;
		}
		_isPlayerToLeft = _level.GetPlayerPosition().X < Position.X;
		base.StartAbility(whichAbility);
	}

	public override void UpdateAbility(float delta)
	{
		for (int i = 0; i < 4; i++)
		{
			float num = (float)(i + 1) * 0.33f;
			if (_lastAbilityTimer < num && _abilityTimer >= num)
			{
				_shards[i].StartAttack(_selectedAbility == 2, _isPlayerToLeft);
			}
		}
		if (_abilityTimer >= 3f)
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification.IntArgument == 0 && base.Appendages.Count >= 5)
		{
			Appendage appendage = base.Appendages[4];
			_level.AddAnimation(EBattleAnimationType.Boom, appendage.Bbox.Center, ETeamSide.Enemies);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			PlayCue(ESFX.EnemyLabAdultGroan);
			DropLoot();
			SetCharacterSequence(_deathSequence);
			base.IsSolidWhenFrozen = false;
			_level.RequestRemoveObject(_blocker);
			base.DoesDrawAura = false;
			LabAdultGlassShard[] shards = _shards;
			foreach (LabAdultGlassShard labAdultGlassShard in shards)
			{
				labAdultGlassShard.KillShard();
			}
		}
		if (!(_deathScriptTimer < 1.25f))
		{
			return;
		}
		_deathScriptTimer += delta;
		if (_deathScriptTimer >= 0.5f)
		{
			float num = (_deathScriptTimer - 0.5f) / 0.75f;
			if (num > 1f)
			{
				num = 1f;
			}
			base.DrawColor = Color.White.SineInterpolate(FadedColor, num);
		}
		if (_deathScriptTimer >= 1.25f)
		{
			DoMiniBossOpenDoors();
			_level.GameSave.SetValue("11_Exp13", value: true);
		}
		UpdateCharacterSequences(delta);
		UpdateAppendages(delta);
		UpdateAnimation(delta);
	}

	private void SetDeadOnStart()
	{
		SetCharacterSequenceByName("Dead");
		base.IsSolidWhenFrozen = false;
		_level.RequestRemoveObject(_blocker);
		_damageCaused = 0;
		_canBeDamaged = false;
		base.IsDead = true;
		base.DoesDrawAura = false;
		base.DrawColor = FadedColor;
		LabAdultGlassShard[] shards = _shards;
		foreach (LabAdultGlassShard deadObject in shards)
		{
			_level.RequestRemoveObject(deadObject);
		}
	}

	private void DoMiniBossDoorLock()
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
	}

	private void DoMiniBossOpenDoors()
	{
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
		_level.OpenAllBossDoors(1f);
		_level.ToggleExits(isEnabled: true);
	}
}
