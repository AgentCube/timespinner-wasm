using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Bird;

internal sealed class GodBirdBossStatue : GameEvent
{
	private enum EBirdBossStatueState
	{
		Inactive,
		FullRising,
		PartialRising,
		Floating,
		Targeting,
		Crushing
	}

	private const int StatueFrameIndex = 21;

	private const int QuarterWidth = 8;

	private const int HalfWidth = 16;

	private const int Width = 32;

	private const int CollisionHeight = 48;

	private const int FullHeight = 56;

	private const int SpikeOffset = 8;

	private const int FloorY = 240;

	private const int MinLeftX = 192;

	private const int MaxRightX = 368;

	private const int SeparationX = 58;

	private const int StartingOffsetX = 36;

	private const int StartingOffsetY = 44;

	private const float TimeBeforeDroppingPlayer = 0.05f;

	private static readonly Color FadedColor = new Color(0.675f, 0.65f, 0.7f);

	private readonly int _statueIndex;

	private bool _isActive;

	private EBirdBossStatueState _statueState;

	private float _timeSincePlayerStoodOnUs;

	private float _statueStateTimer;

	private float _lastStatueStateTimer;

	public GodBirdBossStatue(Level inLevel, SpriteSheet sprite, ObjectTileSpecification objectSpec, int index)
		: base(inLevel, new Point(228 + index * 58, 284), -1, objectSpec)
	{
		_sprite = sprite;
		_statueIndex = index;
		ChangeAnimation(21);
		Bbox = new Rectangle(0, 0, 32, 48);
		IsFacingLeft = true;
		base.IsAffectedByTime = true;
		_defaultTeam = ETeamSide.Enemies;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		base.DrawColor = FadedColor;
		_doAppendagesMatchImageFacing = false;
		_doAppendagesInheritDrawColor = true;
		_doesUseAppendageCollision = false;
		Appendage appendage = new Appendage(this, new Point(16, 56), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-8, 8),
			IsFacingLeft = false
		};
		appendage.ChangeAnimation(21);
		base.Appendages.Add(appendage);
	}

	internal void SetIsActive(bool isActive)
	{
		_isActive = isActive;
		_isSolid = isActive;
	}

	internal void DoFullRise()
	{
		SetStatueState(EBirdBossStatueState.FullRising, shouldDelay: true);
		base.DrawColor = FadedColor;
	}

	private void SetStatueState(EBirdBossStatueState state, bool shouldDelay)
	{
		float num = 0.1f;
		_statueState = state;
		_statueStateTimer = 0f;
		_lastStatueStateTimer = -1f;
		if (shouldDelay)
		{
			_statueStateTimer -= (float)_statueIndex * num;
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_statueStateTimer >= 0f)
			{
				switch (_statueState)
				{
				case EBirdBossStatueState.FullRising:
					UpdateFullRiseSequence();
					break;
				case EBirdBossStatueState.Floating:
					UpdateFloatingSequence();
					break;
				}
			}
			_lastStatueStateTimer = _statueStateTimer;
			_statueStateTimer += delta;
		}
		if (_timeSincePlayerStoodOnUs < 10f)
		{
			_timeSincePlayerStoodOnUs += delta;
		}
		base.Update(delta);
	}

	private void UpdateFullRiseSequence()
	{
		int num = 56;
		int num2 = num + 44;
		float num3 = 1f;
		float num4 = 4f;
		float num5 = 0.66f;
		float num6 = 0.5f;
		float num7 = num5 + num6;
		if (_statueStateTimer <= num7)
		{
			if (_statueStateTimer < num5)
			{
				float num8 = (float)Math.Sin(num4 * ((float)Math.PI * 2f) * (_statueStateTimer / num5));
				Position = new Point(Position.X, (int)Math.Ceiling(num8 * num3) + 284);
			}
			else
			{
				float num9 = 1f - (float)Math.Cos((float)Math.PI / 2f * (_statueStateTimer - num5) / num6);
				Position = new Point(Position.X, 284 - (int)(num9 * (float)num2));
				base.DrawColor = FadedColor.SineInterpolate(Color.White, num9);
			}
		}
		else
		{
			SetStatueState(EBirdBossStatueState.Floating, shouldDelay: false);
			SetIsActive(isActive: true);
		}
	}

	private void UpdateFloatingSequence()
	{
		int num = 56;
		int num2 = 4;
		float num3 = 1f;
		float num4 = 1f - (float)Math.Cos((float)Math.PI * 2f * _statueStateTimer / num3);
		Position = new Point(Position.X, 240 - num - (int)Math.Ceiling(num4 * (float)num2));
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (who is Protagonist protagonist && _isActive)
		{
			bool flag2 = Math.Abs(depth.Y) < Math.Abs(depth.X);
			if (!protagonist.IsIgnoringPlatforms && flag2 && protagonist.Position.Y < Bbox.Bottom && (protagonist.IsGrounded || (protagonist.LastPosition.Y <= base.OuterBbox.Top && protagonist.Velocity.Y > 0f) || _timeSincePlayerStoodOnUs < 0.05f))
			{
				flag = base.TriggerEvent(who, depth);
				if (flag)
				{
					_timeSincePlayerStoodOnUs = 0f;
					protagonist.AddMovingPlatform(this);
				}
			}
		}
		return flag;
	}
}
