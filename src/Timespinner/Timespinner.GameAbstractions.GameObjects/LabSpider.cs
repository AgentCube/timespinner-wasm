using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._11_Laboratory;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class LabSpider : Monster
{
	private const int OtherSpiderMarginX = 80;

	private const int WallBufferFloorX = 56;

	private const int WallBufferCeilingX = 20;

	private const float TimeBetweenCheckingForOtherSpiders = 0.25f;

	private static readonly Color DeathColor = new Color(255, 32, 32);

	private readonly Appendage _eyeGlowAppendage;

	private readonly LabSpiderLazer _lazer;

	private bool _isOnCeiling;

	private bool _isBlockedByWall;

	private bool _isWallToLeft;

	private float _spiderCheckTimer;

	private SFXCueInstance _walkingCueInstance;

	public bool IsOnCeiling
	{
		get
		{
			return _isOnCeiling;
		}
		set
		{
			_isOnCeiling = value;
			_isAffectedByGravity = !value;
			IsFlippedVertically = value;
		}
	}

	public LabSpider(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_nonAggroStrategy = EAIStrategy.Wander;
		_movementType = EAIMovementType.Walk;
		base.IsAffectedByTime = false;
		_agility = 0.275f;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_bboxOffset = new Point(6, 4);
		Bbox = new Rectangle(_position.X, _position.Y, 35, 20);
		base.AggroBboxDimensions = new Point(300, 360);
		_eyeGlowAppendage = new Appendage(this, new Point(8, 5), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, -11),
			DrawPriority = 1,
			DoesInheritDrawColor = false,
			DrawColor = Color.White * 0.25f,
			DoesDrawAura = true,
			AuraColor = new Color(200, 32, 32, 200),
			AuraCount = 4f,
			AuraFrequency = 8f
		};
		_appendages.Add(_eyeGlowAppendage);
		_eyeGlowAppendage.ChangeAnimation(19);
		IsOnCeiling = objectSpec.IsFlippedVertically;
		_nextActionTimer = 0.5f;
		_lazer = new LabSpiderLazer(_level, Position, this, _sprite, base.Damage, IsOnCeiling);
		_level.RequestAddObject(_lazer);
	}

	public override void Update(float delta)
	{
		if (Math.Abs(_movementX) > 0.1f)
		{
			if (Math.Abs(_velocity.X) < 0.1f)
			{
				_isBlockedByWall = true;
				_isWallToLeft = _movementX < 0f;
				StopMoving();
			}
			else
			{
				bool flag = _movementX < 0f;
				Point center = Bbox.Center;
				Tile nearestSolidTile = _level.GetNearestSolidTile(center, (!flag) ? EDirection.East : EDirection.West, 3);
				if (nearestSolidTile != null)
				{
					int num = Math.Abs(center.X - nearestSolidTile.Position.X);
					int num2 = (_isOnCeiling ? 20 : 56);
					if (num <= num2)
					{
						_isBlockedByWall = true;
						_isWallToLeft = flag;
						StopMoving();
					}
				}
			}
		}
		_spiderCheckTimer -= delta;
		if (_spiderCheckTimer <= 0f)
		{
			_spiderCheckTimer = 0.25f;
			if (AreOtherSpidersInWay())
			{
				StopMoving();
			}
		}
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentTarget = _level.GetNearestProtagonist(_position);
		_targetPosition = _currentTarget.Bbox.Center;
		IsFacingLeft = _targetPosition.X < _position.X;
		_isMovingLeft = IsFacingLeft;
		_currentAction = EAIAction.Custom;
		_nextActionTimer = (float)_random.Next(5, 15) * 0.1f;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (_isCollidingWithWall)
		{
			if (_currentAI == EAIStrategy.Pace)
			{
				IsFacingLeft = !IsFacingLeft;
				_isMovingLeft = IsFacingLeft;
				_nextActionTimer = _paceLength;
			}
			else
			{
				_isAfraidOfMoving = true;
				_currentAction = EAIAction.Idle;
				_nextActionTimer = 0.01f;
			}
		}
		if (_isAfraidOfFalling && CheckIfFloorEnds())
		{
			_currentAction = EAIAction.Idle;
			_nextActionTimer = 0.1f;
			_movementX = 0f;
			_isAfraidOfMoving = true;
		}
		else if (!AreOtherSpidersInWay() && (!_isBlockedByWall || _isWallToLeft != _isMovingLeft))
		{
			if (!_isMovingLeft)
			{
				DoHorizontalRun(_agility);
			}
			else
			{
				DoHorizontalRun(0f - _agility);
			}
		}
	}

	private bool AreOtherSpidersInWay()
	{
		bool result = false;
		IEnumerable<Monster> enemiesOfType = _level.GetEnemiesOfType(EEnemyTileType.FleshSpider);
		foreach (Monster item in enemiesOfType)
		{
			if (item == this)
			{
				continue;
			}
			int num = Position.X - item.Position.X;
			if (Math.Abs(num) < 80)
			{
				bool flag = num > 0;
				if (IsFacingLeft == flag)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private void StopMoving()
	{
		SetState(EAFSM.Idle);
		_currentAction = EAIAction.Idle;
		_nextActionTimer = 0.5f;
		_movementX = 0f;
		_velocity = Vector2.Zero;
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			if ((!_isBlockedByWall || _isWallToLeft != IsFacingLeft) && !AreOtherSpidersInWay())
			{
				_isBlockedByWall = false;
				ChangeAnimation(1, 5, 0.1f, EAnimationType.Cycle);
				if (_walkingCueInstance != null)
				{
					if (_walkingCueInstance.IsPaused)
					{
						_walkingCueInstance.Resume();
					}
					else
					{
						_walkingCueInstance.Play();
					}
					break;
				}
				_walkingCueInstance = CreateCue(ESFX.EnemySpiderWalk, Position, isLooped: true);
				if (_walkingCueInstance != null)
				{
					_walkingCueInstance.PlayWhenInRange();
				}
			}
			else
			{
				StopMoving();
			}
			break;
		case EAFSM.Idle:
			if (_walkingCueInstance != null)
			{
				_walkingCueInstance.Pause();
			}
			ChangeAnimation(0);
			break;
		}
	}

	public override void Kill()
	{
		if (_walkingCueInstance != null)
		{
			_walkingCueInstance.Stop();
		}
		if (_lazer != null)
		{
			_lazer.OnParentDeath();
		}
		base.Kill();
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			DropLoot();
		}
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 0.15f)
		{
			float amount = _deathScriptTimer / 0.15f;
			base.DrawColor = Color.White.CosInterpolate(DeathColor, amount);
		}
		else
		{
			_level.AddAnimation(EBattleAnimationType.Boom, _bbox.Center, ETeamSide.Enemies);
			RemoveInstance();
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_lazer != null)
		{
			_lazer.RemoteDraw(spriteBatch);
		}
	}

	internal override void InitializeForBestiary()
	{
		base.InitializeForBestiary();
		if (_lazer != null)
		{
			_lazer.Update(0f);
		}
	}
}
