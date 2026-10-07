using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CaveSnailSpitUnit : Appendage
{
	private const int Height = 16;

	private const int HalfHeight = 8;

	private const float MaxLife = 1f;

	private readonly int _floorHeight;

	private readonly Action<Point> _onLandAction;

	private bool _hasCreatedGoop;

	internal bool IsFinished { get; private set; }

	internal bool HasBeenRemoved { get; set; }

	internal bool CanDamageEnemies { get; private set; }

	internal float Life { get; private set; }

	public CaveSnailSpitUnit(Animate parent, Level inLevel, SpriteSheet inSprite, int floorHeight, Action<Point> onLandAction)
		: base(parent, new Point(16, 16), Point.Zero, inLevel, inSprite)
	{
		DrawOrigin = new Vector2(8f, 8f);
		_onLandAction = onLandAction;
		_floorHeight = floorHeight;
		ChangeAnimation(16, 4, 0.05f, EAnimationType.Cycle);
		_animationIndex = _level.NextRandomInt(0, 2);
		Life = 1f;
		CanDamageEnemies = true;
		_isAffectedByGravity = true;
		base.DoesDrawTrail = true;
		_trailLength = 3;
		_trailFadeRate = 2f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (Bbox.Top >= _floorHeight)
			{
				Life = 0f;
			}
			if (!_hasCreatedGoop && Bbox.Bottom >= _floorHeight - 8)
			{
				_onLandAction(new Point(Position.X, _floorHeight));
				_hasCreatedGoop = true;
			}
			Life -= delta;
			if (Life <= 0f)
			{
				IsFinished = true;
				CanDamageEnemies = false;
				base.DrawColor = new Color(0, 0, 0, 0);
			}
			else
			{
				_scale = 1.25f * (1f - Life / 1f) + 0.75f;
				if (Life < 0.1f)
				{
					CanDamageEnemies = false;
				}
				base.Rotation = MathEx.RotationFromVector2(_velocity);
				IsFacingLeft = _velocity.X < 0f;
			}
		}
		_doesOverrideVelocity = false;
		_isAffectedByFriction = false;
		_gravityAcceleration = 500f;
		base.Update(delta);
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Life = 1f;
		CanDamageEnemies = true;
		IsFinished = false;
		HasBeenRemoved = false;
		base.DrawColor = Color.White;
		_hasCreatedGoop = false;
		Position = position;
		SnapBboxToPosition();
		ClearTrailHistory();
		_velocity = iV;
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - 8, _position.Y - 8);
	}
}
