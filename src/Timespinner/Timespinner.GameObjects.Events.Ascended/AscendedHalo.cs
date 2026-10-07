using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Ascended;

internal class AscendedHalo : Animate
{
	private const int PieceCount = 4;

	private const int Anim_FrameIndex = 15;

	private const int FrameWidth = 64;

	private const int HalfFrameWidth = 32;

	private const float FrontHaloRotationSpeed = 2f;

	private const float BehindHaloRotationSpeed = 1f;

	private const float PieceRotationOffset = (float)Math.PI / 2f;

	private readonly Appendage[] _behindHalos = new Appendage[4];

	private readonly Appendage[] _frontHalos = new Appendage[4];

	private float _frontRotation;

	private float _behindRotation;

	public AscendedHalo(Point inPosition, Level inLevel, SpriteSheet sprite)
		: base(inPosition, inLevel, -1)
	{
		_sprite = sprite;
		ChangeAnimation(-1);
		_isAffectedByGravity = false;
		_isFlying = true;
		Bbox = new Rectangle(0, 0, 16, 16);
		_doesDrawBaseSprite = false;
		_doesDrawAppendages = true;
		for (int i = 0; i < 4; i++)
		{
			Appendage appendage = new Appendage(this, new Point(64, 64), Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(32, 0),
				DrawColor = Color.White,
				DrawOrigin = new Vector2(0f, 64f),
				Rotation = (float)i * ((float)Math.PI / 2f),
				DrawPriority = -1
			};
			appendage.ChangeAnimation(15);
			_behindHalos[i] = appendage;
			base.Appendages.Add(appendage);
			Appendage appendage2 = new Appendage(this, new Point(64, 64), Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(32, 0),
				DrawColor = Color.White,
				DrawOrigin = new Vector2(0f, 64f),
				Rotation = (float)i * ((float)Math.PI / 2f),
				DrawPriority = 1
			};
			appendage2.ChangeAnimation(15);
			_frontHalos[i] = appendage2;
			base.Appendages.Add(appendage2);
		}
	}

	internal void SetDrawColor(Color color)
	{
		color *= 0.1f;
		base.DrawColor = color;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_frontRotation += delta * 2f;
			if (_frontRotation >= (float)Math.PI * 2f)
			{
				_frontRotation -= (float)Math.PI * 2f;
			}
			_behindRotation -= delta * 1f;
			if (_behindRotation < 0f)
			{
				_behindRotation += (float)Math.PI * 2f;
			}
			int num = 0;
			Appendage[] frontHalos = _frontHalos;
			foreach (Appendage appendage in frontHalos)
			{
				float rotation = _frontRotation + (float)num * ((float)Math.PI / 2f);
				appendage.Rotation = rotation;
				num++;
			}
			num = 0;
			Appendage[] behindHalos = _behindHalos;
			foreach (Appendage appendage2 in behindHalos)
			{
				float rotation2 = _behindRotation + (float)num * ((float)Math.PI / 2f);
				appendage2.Rotation = rotation2;
				num++;
			}
		}
		base.Update(delta);
	}
}
