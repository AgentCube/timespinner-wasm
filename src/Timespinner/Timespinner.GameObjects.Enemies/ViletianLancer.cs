using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ViletianLancer : Monster
{
	private const int Anim_Start = 17;

	private const int Anim_Length = 3;

	private bool _isMoving;

	private int _movementTargetX;

	private int _startingMovementX;

	private float _movementTimer;

	private float _totalMovementTime;

	private float _sleepTimer;

	public ViletianLancer(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_bboxOffset = new Point(5, 10);
		Bbox = new Rectangle(_position.X, _position.Y, 32, 40);
		ChangeAnimation(17);
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_nonAggroAction = EAIAction.Custom;
		_agility = 0.75f;
		_isAlwaysAggroed = true;
		_doesDropBasicLoot = false;
		_isAffectedByGravity = false;
		_isAffectedByLevelBounds = false;
		_doesCollideWithTiles = false;
		_isAffectedByLevelBounds = false;
	}

	public override void Update(float delta)
	{
		if (_isMoving)
		{
			if (_sleepTimer > 0f)
			{
				_sleepTimer -= delta;
			}
			if (_sleepTimer <= 0f)
			{
				IsFacingLeft = false;
				_movementTimer -= delta;
				if (_movementTimer > 0f)
				{
					float amount = 1f - _movementTimer / _totalMovementTime;
					Position = new Point(_startingMovementX, Position.Y).Lerp(new Point(_movementTargetX, Position.Y), amount);
				}
				else
				{
					SetState(EAFSM.Idle);
					Position = new Point(_movementTargetX, Position.Y);
					_isMoving = false;
				}
			}
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Idle:
			ChangeAnimation(17);
			break;
		case EAFSM.Running:
			ChangeAnimation(17, 3, 0.1f, EAnimationType.PingPong);
			break;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		RemoveInstance();
	}

	internal void StartMoving(float timeToMove, float waitTime, int targetX)
	{
		_isMoving = true;
		_movementTimer = timeToMove;
		_totalMovementTime = timeToMove;
		_sleepTimer = waitTime;
		_movementTargetX = targetX;
		_startingMovementX = Position.X;
		SetState(EAFSM.Running);
	}
}
