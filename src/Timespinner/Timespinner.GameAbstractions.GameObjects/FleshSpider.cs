using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class FleshSpider : Monster
{
	private const float FallDistanceXThreshold = 32f;

	private readonly Appendage _eyeAppendage;

	private readonly Appendage _fleshAppendage;

	private bool _isOnCeiling;

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

	public FleshSpider(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.Chase;
		_nonAggroStrategy = EAIStrategy.Wander;
		_movementType = EAIMovementType.Walk;
		_agility = 0.275f;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_bboxOffset = new Point(6, 4);
		Bbox = new Rectangle(_position.X, _position.Y, 35, 20);
		base.AggroBboxDimensions = new Point(300, 360);
		_eyeAppendage = new Appendage(this, new Point(10, 8), new Point(1, 0), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(1, -7),
			DrawPriority = 1
		};
		_fleshAppendage = new Appendage(this, new Point(15, 3), new Point(0, 2), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(1, -12),
			DrawPriority = 1
		};
		_appendages.Add(_eyeAppendage);
		_appendages.Add(_fleshAppendage);
		_eyeAppendage.ChangeAnimation(14, 4, 1f, EAnimationType.PingPong);
		_fleshAppendage.ChangeAnimation(10, 1, 0.1f, EAnimationType.None);
		IsOnCeiling = objectSpec.IsFlippedVertically;
		_nextActionTimer = 0.5f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && IsOnCeiling && _currentTarget != null && _isAggroed && _currentTarget.Position.Y > _position.Y && (float)Math.Abs(_currentTarget.Position.X - _position.X) < 32f)
		{
			IsOnCeiling = false;
			PlayCue(ESFX.EnemySpiderDrop, Position);
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			ChangeAnimation(1, 5, 0.1f, EAnimationType.Cycle);
			Blink();
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
			break;
		case EAFSM.Idle:
			if (_walkingCueInstance != null)
			{
				_walkingCueInstance.Pause();
			}
			ChangeAnimation(0, 1, 1f, EAnimationType.None);
			break;
		}
	}

	private void Blink()
	{
		_fleshAppendage.ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 10,
				Length = 4,
				Speed = 0.03f
			},
			new AnimationSpec
			{
				Start = 10,
				Length = 4,
				Speed = 0.03f,
				IsInReverse = true
			}
		});
	}

	public override void Kill()
	{
		if (_walkingCueInstance != null)
		{
			_walkingCueInstance.Stop();
		}
		base.Kill();
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.Boom, _bbox.Center, ETeamSide.Enemies);
		DropLootAndRemove();
	}
}
