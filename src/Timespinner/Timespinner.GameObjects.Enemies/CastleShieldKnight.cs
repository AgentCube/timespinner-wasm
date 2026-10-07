using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CastleShieldKnight : Monster
{
	private const int IdleAnimationLength = 4;

	private const float IdleAnimationSpeed = 0.15f;

	private const float WindupAnimationSpeed = 0.15f;

	private const float SwingAnimationSpeed = 0.067f;

	private const float RecoveryAnimationSpeed = 0.067f;

	private const float TimeForWindup = 0.45000002f;

	private const float TimeForEntireAttack = 0.9f;

	private const float LungeVelocity = 100f;

	private const float SwingVelocityX = 2500f;

	private const float SwingVelocityY = -5000f;

	private const int MaxDamageBboxThresholdX = 64;

	private readonly bool _isExterminationQuestActive;

	private static readonly Point ShieldDimensions = new Point(12, 32);

	private static readonly Point DefaultShieldOffset = new Point(0, -7);

	private bool _isShieldUp;

	internal Point ShieldOffset
	{
		get
		{
			if (!IsFacingLeft)
			{
				return DefaultShieldOffset;
			}
			return new Point(-DefaultShieldOffset.X, DefaultShieldOffset.Y);
		}
	}

	public CastleShieldKnight(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_isAfraidOfFalling = true;
		_isAfraidOfAttackingNearCliff = true;
		_attackDistanceThresholdX = 60;
		_agility = 0.15f;
		_isAffectedByGravity = true;
		_isFlying = false;
		base.DoesTouchDamageKnockback = true;
		_bboxOffset = new Point(5, 10);
		Bbox = new Rectangle(_position.X, _position.Y, 32, 40);
		ChangeAnimation(0, 4, 0.15f, EAnimationType.Cycle);
		_isShieldUp = true;
		_timeToTurnAround = 0.1f;
		_isExterminationQuestActive = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Captain, _level.GameSave) == 2 && NPCBase.GetSubQuestState(NPCBase.ENPCType.Captain, _level.GameSave) > 0;
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Idle:
			ChangeAnimation(0, 4, 0.15f, EAnimationType.Cycle);
			break;
		case EAFSM.Running:
		{
			AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
			animationSpecCollection.DoesRepeat = true;
			AnimationSpecCollection animationSpecCollection2 = animationSpecCollection;
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 3,
				Length = 3,
				Speed = 0.15f,
				Type = EAnimationType.Once
			});
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 3,
				Length = 3,
				Speed = 0.15f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			ChangeAnimation(animationSpecCollection2);
			break;
		}
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_currentState == EAFSM.Running)
		{
			AnimationSpec animationSpec = new AnimationSpec();
			animationSpec.Start = 15;
			animationSpec.Length = 1;
			animationSpec.Speed = 0.1f;
			animationSpec.Type = EAnimationType.Once;
			AnimationSpec animationSpec2 = animationSpec;
			AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
			animationSpecCollection.DoesRepeat = true;
			AnimationSpecCollection animationSpecCollection2 = animationSpecCollection;
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 3,
				Length = 3,
				Speed = 0.15f,
				Type = EAnimationType.Once
			});
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 3,
				Length = 3,
				Speed = 0.15f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			ChangeAnimation(new AnimationSpec[2] { animationSpec2, animationSpecCollection2 });
		}
		else
		{
			ChangeAnimation(0, 4, base.AnimationSpeed, EAnimationType.Cycle, 15, 1, 0.2f);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			if (_isAfraidOfMoving)
			{
				_isCarryingOutAbility = false;
				_currentAction = EAIAction.Idle;
				_nextActionTimer = _timeToIdleAfterAttacking;
			}
			else
			{
				SetState(EAFSM.Idle);
				ChangeAnimation(6, 3, 0.15f, EAnimationType.Once);
				_isShieldUp = false;
				PlayCue(ESFX.EnemyShieldKnightAttack);
			}
		}
		if (_abilityTimer >= 0.45000002f && _lastAbilityTimer < 0.45000002f)
		{
			ChangeAnimation(9, 5, 0.067f, EAnimationType.Once);
		}
		if (_abilityTimer >= 0.45000002f && _abilityTimer <= 0.651f)
		{
			_velocity.X = 2500f * (float)((!IsFacingLeft) ? 1 : (-1));
			if (_abilityTimer < 0.517f)
			{
				_velocity.Y = -5000f * delta;
			}
		}
		if (_abilityTimer >= 0.9f)
		{
			_isCarryingOutAbility = false;
			ChangeAnimation(0, 4, 0.15f, EAnimationType.Cycle, 14, 1, 0.067f);
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
			_velocity.X = 100f * (float)((!IsFacingLeft) ? 1 : (-1));
			_isShieldUp = true;
		}
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result = false;
		if (_isShieldUp)
		{
			Rectangle shieldBbox = GetShieldBbox();
			bool flag = (sourceRectangle.Intersects(shieldBbox) || shieldBbox.Contains(sourceRectangle)) && sourceRectangle.Width < 64;
			if (flag)
			{
				flag = (IsFacingLeft ? (sourceRectangle.Left < shieldBbox.Right) : (sourceRectangle.Right > shieldBbox.Left));
			}
			if (flag)
			{
				if (type != EDamageType.Projectile || !doesKnockBack || sourceRectangle.Width != 18 || sourceRectangle.Height != 18)
				{
					Point position = new Point(IsFacingLeft ? shieldBbox.Left : shieldBbox.Right, where.Y);
					_level.AddAnimation(EBattleAnimationType.SmallFail, position, ETeamSide.Heroes, IsFacingLeft);
				}
			}
			else
			{
				result = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
			}
		}
		else
		{
			result = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		}
		return result;
	}

	private Rectangle GetShieldBbox()
	{
		Point point = new Point(IsFacingLeft ? Bbox.Left : Bbox.Right, Position.Y).Add(ShieldOffset);
		Rectangle result = new Rectangle(point.X + ((!IsFacingLeft) ? (-ShieldDimensions.X) : 0), point.Y - ShieldDimensions.Y, ShieldDimensions.X, ShieldDimensions.Y);
		return result;
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
		if (_isExterminationQuestActive && !_level.GameSave.GetSaveBool("HasShownSoldierQuestFinished"))
		{
			int num = NPCBase.GetNewEnemyKillCount(EEnemyTileType.CastleArcher, _level.GameSave) + NPCBase.GetNewEnemyKillCount(EEnemyTileType.CastleShieldKnight, _level.GameSave);
			if (num >= 20)
			{
				_level.AddScript(new ScriptAction(NPCBase.ENPCType.Captain, 3));
				_level.GameSave.SetValue("HasShownSoldierQuestFinished", value: true);
			}
		}
	}
}
