using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CastleArcher : Monster
{
	private const float TimeForArrowToStart = 0.1f;

	private const float TimeForArrowToAppear = 0.3f;

	private const float TimeForWindup = 0.5f;

	private const float TimeForEntireAttack = 1f;

	private readonly bool _isExterminationQuestActive;

	private readonly Point _arrowStartOffsetA = new Point(-14, -27);

	private readonly Point _arrowStartOffsetB = new Point(-11, -30);

	private readonly Appendage _arrowAppendage;

	internal Point ArrowStartOffsetA
	{
		get
		{
			if (!IsFacingLeft)
			{
				return new Point(-_arrowStartOffsetA.X, _arrowStartOffsetA.Y);
			}
			return _arrowStartOffsetA;
		}
	}

	internal Point ArrowStartOffsetB
	{
		get
		{
			if (!IsFacingLeft)
			{
				return new Point(-_arrowStartOffsetB.X, _arrowStartOffsetB.Y);
			}
			return _arrowStartOffsetB;
		}
	}

	public CastleArcher(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_timeToMove = 0.4f;
		_isAfraidOfFalling = true;
		_isAfraidOfBeingTooClose = true;
		_attackDistanceThresholdX = 225;
		_retreatDistanceThresholdX = 160;
		_agility = 0.2f;
		_isAffectedByGravity = true;
		_isFlying = false;
		_bboxOffset = new Point(16, 13);
		Bbox = new Rectangle(_position.X, _position.Y, 18, 41);
		base.AggroBboxDimensions = new Point(450, 150);
		base.DeaggroBboxDimensions = new Point(900, 300);
		ChangeAnimation(0);
		_timeToTurnAround = 0.07f;
		_arrowAppendage = new Appendage(this, new Point(24, 2), new Point(0, 2), _level, _sprite)
		{
			DrawPriority = 1
		};
		_arrowAppendage.ChangeAnimation(12);
		_isExterminationQuestActive = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Captain, _level.GameSave) == 2 && NPCBase.GetSubQuestState(NPCBase.ENPCType.Captain, _level.GameSave) > 0;
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Idle:
			ChangeAnimation(0);
			break;
		case EAFSM.Running:
			ChangeAnimation(0, 3, 0.15f, EAnimationType.PingPong);
			break;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_currentState == EAFSM.Running)
		{
			ChangeAnimation(0, 3, 0.15f, EAnimationType.PingPong, 11, 1, _timeToTurnAround);
		}
		else
		{
			ChangeAnimation(0, 1, 0.5f, EAnimationType.None, 11, 1, 0.2f);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			ChangeAnimation(3, 4, 0.1f, EAnimationType.Once);
			PlayCue(ESFX.EnemyArcherBowDraw, Position);
		}
		if (_abilityTimer > 0.1f && _abilityTimer <= 0.4f)
		{
			if (_lastAbilityTimer <= 0.1f)
			{
				_appendages.Add(_arrowAppendage);
				_arrowAppendage.IsFacingLeft = IsFacingLeft;
			}
			float num = (_abilityTimer - 0.1f) / 0.3f;
			Point b = ArrowStartOffsetA.Lerp(ArrowStartOffsetB, num);
			_arrowAppendage.Position = Position.Add(b);
			_arrowAppendage.DrawColor = Color.White * num;
		}
		if (_abilityTimer >= 0.5f && _lastAbilityTimer < 0.5f)
		{
			ChangeAnimation(7, 3, 0.066f, EAnimationType.Once);
			int num2 = ((!IsFacingLeft) ? 1 : (-1));
			Point center = _bbox.Center;
			center.X += 20 * num2;
			center.Y -= 9;
			Vector2 iV = new Vector2((float)num2 * 450f, 0f);
			_level.AddProjectile(new CastleArcherArrow(_level, center, iV, base.DefaultTeam, _sprite, base.Damage));
			PlayCue(ESFX.EnemyArcherBowShoot, center);
			_appendages.Remove(_arrowAppendage);
		}
		if (_abilityTimer > 1f)
		{
			_isCarryingOutAbility = false;
			ChangeAnimation(0, 1, 0.15f, EAnimationType.None, 10, 0, 0.066f);
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
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
