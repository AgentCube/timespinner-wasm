using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class SirenInkUnit : Appendage
{
	private const int Radius = 16;

	private const int HalfRadius = 8;

	private const int BboxRadius = 14;

	private const int MaxRotationSpeed = 10;

	private const float MaxLife = 2f;

	private const float LifePercentageBeforeFading = 0.8f;

	private static readonly Color BaseInkColor = new Color(0.3f, 0.3f, 0.3f, 1f);

	private float _baseRotationSpeed;

	private Vector2 _iV;

	internal bool IsFinished { get; set; }

	internal bool HasBeenRemoved { get; set; }

	internal bool CanDamageEnemies { get; private set; }

	internal float Life { get; private set; }

	public SirenInkUnit(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(7, 7), new Point(1, 1), inLevel, inSprite)
	{
		DrawOrigin = new Vector2(8f, 8f);
		ChangeAnimation(100);
		Life = 2f;
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
				base.DrawColor = Color.Transparent;
			}
			else
			{
				float num = 1f - Life / 2f;
				_scale = 1.25f * num + 0.75f;
				if (num < 0.8f)
				{
					base.DrawColor = BaseInkColor;
				}
				else
				{
					float num2 = 1f - (num - 0.8f) / 0.19999999f;
					base.DrawColor = BaseInkColor * num2;
				}
				if (Life < 0.1f)
				{
					CanDamageEnemies = false;
				}
				float num3 = (float)(1.0 - Math.Sin(num * ((float)Math.PI / 4f)));
				_velocity = _iV * num3;
				float num4 = _baseRotationSpeed * (num3 * num3) * delta;
				base.Rotation += num4;
				if (base.Rotation > (float)Math.PI * 2f)
				{
					base.Rotation -= (float)Math.PI * 2f;
				}
				else if (base.Rotation < 0f)
				{
					base.Rotation += (float)Math.PI * 2f;
				}
			}
		}
		base.Update(delta);
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Life = 2f;
		CanDamageEnemies = true;
		IsFinished = false;
		HasBeenRemoved = false;
		base.DrawColor = Color.White;
		Position = position;
		SnapBboxToPosition();
		_velocity = iV;
		_iV = iV;
		_baseRotationSpeed = _level.NextRandomInt(-10, 10);
		base.Rotation = 0f;
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - 8, _position.Y - 8);
	}
}
