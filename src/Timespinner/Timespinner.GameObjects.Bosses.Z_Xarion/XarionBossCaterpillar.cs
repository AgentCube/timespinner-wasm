using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Bosses.Z_Xarion;

internal sealed class XarionBossCaterpillar : Monster
{
	private enum ECaterpillarState
	{
		Falling,
		Bouncing,
		Unfurling,
		Moving
	}

	private const int Anim_MoveStart = 9;

	private const int Anim_MoveLength = 6;

	private const int Anim_FallStart = 15;

	private const int Anim_FallLength = 3;

	private const float Anim_FallSpeed = 0.075f;

	private const float TimeToUnfurl = 0.15f;

	private static readonly int[] MovementFrames = new int[3] { 0, 4, 5 };

	private ECaterpillarState _caterpillarState;

	private float _caterpillarStateTimer;

	private float _caterpillarInvulnerableTimer;

	public XarionBossCaterpillar(Point inPosition, Level inLevel, SpriteSheet inSprite, ObjectTileSpecification objectSpec, int baseDamage)
		: base(inPosition, inLevel, inSprite, -1, objectSpec)
	{
		_caterpillarState = ECaterpillarState.Falling;
		ChangeAnimation(15);
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_isAlwaysAggroed = true;
		Bbox = new Rectangle(0, 0, 20, 7);
		_bboxOffset = new Point(2, 8);
		_isAffectedByGravity = true;
		_isFlying = false;
		_agility = 0.3f;
		_damageCaused = (int)Math.Ceiling((float)baseDamage * 0.95f);
		_doesCollideWithTiles = true;
		base.IsMinion = true;
		_doesDropBasicLoot = false;
		Reset(inPosition);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (_caterpillarInvulnerableTimer > 0f)
		{
			_caterpillarInvulnerableTimer -= delta;
			if (_caterpillarInvulnerableTimer <= 0f)
			{
				_canBeDamaged = true;
			}
		}
		switch (_caterpillarState)
		{
		case ECaterpillarState.Unfurling:
			_caterpillarStateTimer += delta;
			if (_caterpillarStateTimer >= 0.15f)
			{
				_caterpillarState = ECaterpillarState.Moving;
				ChangeAnimation(9, 6, 0.1f, EAnimationType.Cycle);
			}
			break;
		case ECaterpillarState.Moving:
		{
			bool flag = false;
			int[] movementFrames = MovementFrames;
			foreach (int num in movementFrames)
			{
				if (num == base.AnimationIndex)
				{
					flag = true;
					break;
				}
			}
			if (_isCollidingWithWall)
			{
				IsFacingLeft = !IsFacingLeft;
			}
			_movementX = ((!flag) ? 0f : (IsFacingLeft ? (0f - _agility) : _agility));
			break;
		}
		case ECaterpillarState.Falling:
		case ECaterpillarState.Bouncing:
			break;
		}
	}

	protected override void DoBounce(Point impactPoint)
	{
		if (_caterpillarState == ECaterpillarState.Falling)
		{
			_caterpillarState = ECaterpillarState.Bouncing;
			_bounceDecay = 2f;
		}
		else if (_caterpillarState == ECaterpillarState.Bouncing)
		{
			_caterpillarState = ECaterpillarState.Unfurling;
			_doesBounceOnGround = false;
			ChangeAnimation(15, 3, 0.075f, EAnimationType.Once);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			Point center = Bbox.Center;
			BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.WetSplashSmall, center, ETeamSide.Enemies, IsFacingLeft, _level, doesPlaySFX: true);
			battleAnimation.DrawColor = new Color(0.9f, 0.5f, 0.65f);
			_level.AddAnimation(battleAnimation);
			DropLootAndRemove();
		}
		else
		{
			SilentKill();
		}
		_deathScriptTimer += delta;
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		if (_damageCaused > 0)
		{
			target.GiveStatusEffect(EStatusEffectType.Poison, 0);
		}
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	internal bool Reset(Point position)
	{
		bool result = true;
		Monster enemyByID = _level.GetEnemyByID(base.ID);
		if (enemyByID == this)
		{
			result = false;
		}
		else
		{
			base.ID = -1;
		}
		_caterpillarInvulnerableTimer = 0.1f;
		_canBeDamaged = false;
		ChangeAnimation(15);
		IsFacingLeft = _level.NextRandomDouble() < 0.5;
		Position = position;
		SnapBboxToPosition();
		base.Velocity = Vector2.Zero;
		_isInvulnerable = true;
		_invulnerableTimer = 0.5f;
		base.HP = base.MaxHP;
		_isDeadButFrozen = false;
		base.IsDead = false;
		_isFinallyDead = false;
		_isRunningDeathScript = false;
		_deathScriptTimer = 0f;
		_damageFlashFrame = 0;
		_isInvulnerable = false;
		_damagedTimer = 0f;
		SnapBboxToPosition();
		_caterpillarState = ECaterpillarState.Falling;
		_caterpillarStateTimer = 0f;
		_bounceDecay = 1.5f;
		_doesBounceOnGround = true;
		_movementX = 0f;
		SnapFrameToBbox();
		RefreshDrawPos();
		_intermediatePositions.Clear();
		return result;
	}

	public void Pacify()
	{
		_damageCaused = 0;
	}
}
