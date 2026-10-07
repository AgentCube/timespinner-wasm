using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class MawBossMinion : Monster
{
	private const int FlightZoneLeft = 96;

	private const int FlightZoneRight = 544;

	private const int FlightZoneTop = 96;

	private const int FlightZoneBottom = 192;

	private const float RotationRate = 5f;

	private const float TimeToColorChangeToNormal = 0.5f;

	private static readonly Color ColorChangeStart = new Color(160, 32, 48);

	private float _colorChangeTimer;

	public MawBossMinion(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		base.IsMinion = true;
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 0.2f;
		_timeToMove = 2f;
		_isAffectedByGravity = false;
		_bboxOffset = new Point(6, 6);
		Bbox = new Rectangle(_position.X, _position.Y, 17, 17);
		DrawOrigin = new Vector2(14.5f, 14.5f);
		IsFacingLeft = objectSpec != null && !objectSpec.IsFlippedHorizontally;
		_nonAggroAction = EAIAction.FloatInPlace;
		base.AggroBboxDimensions = new Point(400, 300);
		_doesDropBasicLoot = false;
		_currentState = EAFSM.Idle;
		_nonAggroAction = EAIAction.None;
		_isAlwaysAggroed = true;
		ChangeAnimation(27);
		base.DrawColor = ColorChangeStart;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_colorChangeTimer < 0.5f)
			{
				_colorChangeTimer += delta;
				if (_colorChangeTimer < 0.5f)
				{
					float amount = _colorChangeTimer / 0.5f;
					base.DrawColor = ColorChangeStart.SineInterpolate(Color.White, amount);
				}
				else
				{
					base.DrawColor = Color.White;
				}
			}
			base.Rotation += 5f * delta;
			if (base.Rotation > (float)Math.PI * 2f)
			{
				base.Rotation -= (float)Math.PI * 2f;
			}
		}
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_currentAction == _nonAggroAction)
		{
			Point targetPosition = new Point(_level.NextRandomInt(96, 544), _level.NextRandomInt(96, 192));
			_currentAction = EAIAction.GoTowards;
			_targetPosition = targetPosition;
			_startPosition = _position;
			_nextActionTimer = _timeToMove;
			_totalActionTimer = _nextActionTimer;
			_followTimer = 0f;
		}
		else
		{
			_currentAction = _nonAggroAction;
			_nextActionTimer = (float)_random.Next(5, 10) * 0.1f;
		}
	}
}
