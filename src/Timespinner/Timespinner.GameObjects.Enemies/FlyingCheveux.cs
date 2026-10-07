using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class FlyingCheveux : Monster
{
	private const float PropellerAnimationSpeed = 0.01f;

	private readonly SFXCueInstance _flyingLoopCueInstance;

	public FlyingCheveux(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FlyTowards;
		_agility = 0.6f;
		_timeToMove = 1f;
		_isAffectedByGravity = false;
		_bboxOffset = new Point(5, 3);
		Bbox = new Rectangle(_position.X, _position.Y, 18, 20);
		IsFacingLeft = objectSpec != null && !objectSpec.IsFlippedHorizontally;
		_nonAggroAction = EAIAction.FloatInPlace;
		base.AggroBboxDimensions = new Point(400, 300);
		_currentState = EAFSM.Idle;
		ChangeAnimation(0, 3, 0.01f, EAnimationType.Cycle);
		_flyingLoopCueInstance = CreateCue(ESFX.EnemyFlyingCheveuxIdle, Position, isLooped: true);
		if (_flyingLoopCueInstance != null)
		{
			_flyingLoopCueInstance.PlayWhenInRange();
		}
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			if (_flyingLoopCueInstance != null)
			{
				_flyingLoopCueInstance.Pitch = 1f;
			}
			ChangeAnimation(4, 3, 0.01f, EAnimationType.Cycle, 3, 0, 0.03f);
			break;
		case EAFSM.Idle:
			if (_flyingLoopCueInstance != null)
			{
				_flyingLoopCueInstance.Pitch = 0f;
			}
			ChangeAnimation(new AnimationSpec[6]
			{
				new AnimationSpec
				{
					Start = 3,
					Length = 1,
					Speed = 0.05f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 0,
					Length = 1,
					Speed = 0.05f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 8,
					Length = 1,
					Speed = 0.1f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 1,
					Length = 1,
					Speed = 0.05f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 3,
					Length = 1,
					Speed = 0.05f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 0,
					Length = 3,
					Speed = 0.01f,
					Type = EAnimationType.Cycle
				}
			});
			break;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		ChangeAnimation(4, 3, 0.01f, EAnimationType.Cycle, 7, 0, 0.075f);
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}
}
