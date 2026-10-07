using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class LunaisFireballUnit : Appendage
{
	private const int Radius = 16;

	private const int HalfRadius = 8;

	private const int BboxRadius = 14;

	private const float MaxLife = 0.3f;

	internal bool IsFinished { get; private set; }

	internal bool HasBeenRemoved { get; set; }

	internal bool CanDamageEnemies { get; private set; }

	internal float Life { get; private set; }

	public LunaisFireballUnit(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(7, 7), new Point(1, 1), inLevel, inSprite)
	{
		DrawOrigin = new Vector2(8f, 8f);
		ChangeAnimation(35, 3, 0.03f, EAnimationType.Cycle);
		_animationIndex = _level.NextRandomInt(0, 2);
		Life = 0.3f;
		CanDamageEnemies = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			Life -= delta;
			if (Life <= 0f)
			{
				IsFinished = true;
				CanDamageEnemies = false;
				base.DrawColor = new Color(0, 0, 0, 0);
			}
			else
			{
				float num = Life / 0.3f;
				_scale = 1.25f * (1f - Life / 0.3f) + 0.75f;
				Vector4 vector = Color.White.ToVector4();
				base.DrawColor = new Color(vector.X, vector.Y, vector.Z, num) * num;
				if (Life < 0.1f)
				{
					CanDamageEnemies = false;
				}
				if (Math.Abs(_velocity.Y) > 1f)
				{
					base.Rotation = (float)Math.Atan2(_velocity.X, 0f - _velocity.Y);
					if (_velocity.X > 0f)
					{
						base.Rotation -= 1.57f;
					}
					else if (_velocity.X < 0f)
					{
						base.Rotation += 1.57f;
					}
				}
				else
				{
					base.Rotation = 0f;
				}
			}
		}
		base.Update(delta);
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Life = 0.3f;
		CanDamageEnemies = true;
		IsFinished = false;
		HasBeenRemoved = false;
		base.DrawColor = Color.White;
		Position = position;
		SnapBboxToPosition();
		_velocity = iV;
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - 8, _position.Y - 8);
	}
}
