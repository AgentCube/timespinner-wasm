using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CursedSnailFloorGoopEvent : GameEvent
{
	private const int MaxGoopUnits = 32;

	private const float GoopGravity = 120f;

	private const float TimeForFootstepCooldown = 0.25f;

	private readonly CursedSnailFloorGoopUnit[] _goopUnits = new CursedSnailFloorGoopUnit[32];

	private int _goopCreationCount;

	private float _footstepCueCooldownTimer;

	private Vector2 _currentVector = Vector2.Zero;

	public CursedSnailFloorGoopEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = sprite;
		_bbox = new Rectangle(0, 0, 24, 17);
		_bboxOffset = new Point(0, -1);
		Position = Position.Add(0, 16);
		SnapBboxToPosition();
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = true;
		IsFacingLeft = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = true;
		_doesUseAppendageCollision = true;
		_defaultTeam = ETeamSide.Neutral;
		base.DrawPlane = EDrawPlane.Front;
		_isAffectedByTime = true;
		base.CanBeUsedWhenFrozen = true;
		base.IsLostWhenNotTouching = true;
		base.IsLostWhenNotGrounded = true;
		_currentVector = new Vector2(0f, 120f);
		_doesDrawBaseSprite = false;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		Rectangle bbox = who.Bbox;
		bool flag = bbox.Intersects(base.OuterBbox);
		if (flag)
		{
			flag = false;
			foreach (Appendage appendage in base.Appendages)
			{
				if (appendage.Bbox.Intersects(bbox))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag && who.DefaultTeam != ETeamSide.Enemies)
		{
			who.AddMovingPlatform(this);
			_currentVector = new Vector2(0f - 2f * who.Velocity.X / 3f, 120f);
			if (_footstepCueCooldownTimer <= 0f && Math.Abs(who.Velocity.X) > 1f)
			{
				PlayCue(ESFX.EnemyCheveuxTowerVomitWalk, who.Position);
				_footstepCueCooldownTimer = 0.25f;
			}
		}
		return flag;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_footstepCueCooldownTimer > 0f)
			{
				_footstepCueCooldownTimer -= delta;
			}
			if (!base.IsFrozen && _goopCreationCount > 0)
			{
				for (int i = 0; i < _goopCreationCount; i++)
				{
					CursedSnailFloorGoopUnit cursedSnailFloorGoopUnit = _goopUnits[i];
					if (cursedSnailFloorGoopUnit.IsFinished && !cursedSnailFloorGoopUnit.HasBeenRemoved)
					{
						base.Appendages.Remove(cursedSnailFloorGoopUnit);
						cursedSnailFloorGoopUnit.HasBeenRemoved = true;
					}
				}
			}
		}
		base.Update(delta);
		base.AmountMovedLastStep = new Vector2(_currentVector.X * delta, _currentVector.Y * delta);
	}

	public void AddGoopUnit(Point goopPosition)
	{
		CursedSnailFloorGoopUnit cursedSnailFloorGoopUnit = null;
		if (_goopCreationCount < 32)
		{
			cursedSnailFloorGoopUnit = new CursedSnailFloorGoopUnit(this, new Point(24, 17), new Point(0, -1), _level, _sprite);
			_goopUnits[_goopCreationCount] = cursedSnailFloorGoopUnit;
			_goopCreationCount++;
		}
		else
		{
			for (int i = 0; i < 32; i++)
			{
				if (_goopUnits[i].IsFinished)
				{
					cursedSnailFloorGoopUnit = _goopUnits[i];
					break;
				}
			}
		}
		if (cursedSnailFloorGoopUnit != null)
		{
			cursedSnailFloorGoopUnit.Reset(goopPosition);
			base.Appendages.Add(cursedSnailFloorGoopUnit);
		}
	}
}
