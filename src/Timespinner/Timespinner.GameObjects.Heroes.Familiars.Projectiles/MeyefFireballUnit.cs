using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal sealed class MeyefFireballUnit : Appendage
{
	private const int Height = 6;

	private const int HalfHeight = 3;

	private const float MaxLife = 0.3f;

	internal bool IsFinished { get; private set; }

	internal bool HasBeenRemoved { get; set; }

	internal bool CanDamageEnemies { get; private set; }

	internal float Life { get; private set; }

	public MeyefFireballUnit(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(6, 6), Point.Zero, inLevel, inSprite)
	{
		DrawOrigin = new Vector2(3f, 3f);
		ChangeAnimation(32, 3, 0.03f, EAnimationType.Cycle);
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
				base.Rotation = MathEx.RotationFromVector2(_velocity);
				IsFacingLeft = _velocity.X < 0f;
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
		_bbox.Location = new Point(_position.X - 3, _position.Y - 3);
	}

	public void ChangeSprite(SpriteSheet sprite)
	{
		_sprite = sprite;
	}
}
