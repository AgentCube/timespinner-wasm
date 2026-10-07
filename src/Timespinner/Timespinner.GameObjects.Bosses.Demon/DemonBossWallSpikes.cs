using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Demon;

internal sealed class DemonBossWallSpikes : GameEvent
{
	private const int SpikeSize = 16;

	private const int SpikeWidthActiveThreshold = 10;

	private const int SpikeCount = 13;

	private const int FloorPositionY = 224;

	private const int LeftWallPositionX = 8;

	private const int RightWallPositionX = 392;

	private const float TimeToExtendRetract = 0.5f;

	private bool _isExtendingRetracting;

	private bool _canDamage;

	private float _extendRetractTimer;

	internal bool IsExtended { get; private set; }

	public DemonBossWallSpikes(Level inLevel, Point inPosition, bool isFacingLeft, SpriteSheet sprite, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		IsFacingLeft = isFacingLeft;
		base.DrawPlane = EDrawPlane.Front;
		Position = new Point(IsFacingLeft ? 392 : 8, 224);
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = false;
		base.DoesCollideWithTiles = false;
		int num = 0;
		Point bboxDimensions = new Point(16, 16);
		for (int i = 0; i < 13; i++)
		{
			Appendage appendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(0, num)
			};
			appendage.ChangeAnimation(4);
			num -= 16;
			base.Appendages.Add(appendage);
		}
	}

	internal void Extend()
	{
		if (!IsExtended)
		{
			IsExtended = true;
			_isExtendingRetracting = true;
			_extendRetractTimer = 0f;
			PlayCue(ESFX.BossDemonSpikesExtend);
		}
	}

	internal void Retract()
	{
		if (IsExtended)
		{
			IsExtended = false;
			_isExtendingRetracting = true;
			_extendRetractTimer = 0f;
			PlayCue(ESFX.BossDemonSpikesRetract);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isExtendingRetracting)
		{
			float num = 1f;
			_extendRetractTimer += delta;
			if (_extendRetractTimer >= 0.5f)
			{
				_isExtendingRetracting = false;
			}
			else
			{
				num = (float)Math.Sin((float)Math.PI / 2f * (_extendRetractTimer / 0.5f));
			}
			int num2 = (int)(16f * (IsExtended ? num : (1f - num)));
			_canDamage = num2 > 10;
			int num3 = (IsFacingLeft ? 392 : 8);
			int x = num3 + num2 * ((!IsFacingLeft) ? 1 : (-1));
			Position = new Point(x, 224);
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (IsExtended && _canDamage && who is Protagonist protagonist)
		{
			flag = protagonist.ManageDamage(5, new Vector2(3f * depth.X, 0f), protagonist.Bbox.Center, base.OuterBbox, EDamageType.Spike, EDamageElement.Sharp, doesKnockBack: true);
			if (flag)
			{
				Point point = new Point(Bbox.Center.X + (IsFacingLeft ? (-8) : 8), protagonist.Bbox.Center.Y);
				_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, point, _level)
				{
					TeamSide = base.DefaultTeam,
					IsFacingLeft = IsFacingLeft,
					AnimationSpeed = 0.03f,
					AnimationStart = 15,
					AnimationLength = 4
				});
				_level.PlayCue(ESFX.FoleySpikeDamage, point);
			}
		}
		return flag;
	}
}
