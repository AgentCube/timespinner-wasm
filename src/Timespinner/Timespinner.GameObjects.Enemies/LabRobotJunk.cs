using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LabRobotJunk : Monster
{
	private enum ERobotJunkBrand
	{
		Bird,
		Cat,
		Star
	}

	private readonly List<GameObject> _squishers = new List<GameObject>();

	private bool _isBeingHeldByCrusher;

	private bool _isMelting;

	private Point _frozenBboxOffset;

	private Point _unfrozenBboxOffset;

	private ERobotJunkBrand _brandType;

	private ERobotJunkType _junkType;

	private Rectangle _frozenRectangle;

	private Rectangle _unfrozenRectangle;

	internal bool IsFinished { get; private set; }

	public LabRobotJunk(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec, ERobotJunkType junkType)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		base.MaxHP = 32;
		base.HP = base.MaxHP;
		base.IsMinion = true;
		base.IsDormant = true;
		_currentAI = EAIStrategy.None;
		base.MaxMP = 5;
		_agility = 0.75f;
		_doesDropBasicLoot = false;
		_isAffectedByLevelBounds = false;
		_maxFallSpeed = 500f;
		_doesUse16X16TileCollisionBbox = true;
		base.AggroBboxDimensions = new Point(1, 1);
		Reset(Position, junkType);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_damageCaused = ((!base.IsGrounded && !(_timeSinceGrounded < 0.25f)) ? 48 : 0);
			base.DoesTouchDamageKnockback = !_wasGrounded;
		}
		if (_junkType == ERobotJunkType.Junk_Crushed || _junkType == ERobotJunkType.Junk_Half_Crushed)
		{
			base.IsSolidWhenFrozen = !base.IsGrounded;
		}
		if (_isBeingHeldByCrusher)
		{
			base.CurrentMovingPlatforms.Clear();
		}
		_isBeingHeldByCrusher = false;
		base.Update(delta);
	}

	public override void Freeze()
	{
		BboxOffset = _frozenBboxOffset;
		Bbox = _frozenRectangle;
		SnapBboxToPosition();
		base.Freeze();
	}

	public override void Unfreeze()
	{
		BboxOffset = _unfrozenBboxOffset;
		Bbox = _unfrozenRectangle;
		SnapBboxToPosition();
		base.Unfreeze();
	}

	protected override void DoLandingAction()
	{
		if (!IsInWater && !_isFrozen)
		{
			_level.AddAnimation(EBattleAnimationType.Dust, new Point(_bbox.Center.X, _bbox.Bottom));
		}
	}

	internal override void ManageSquishedDamage(GameObject whoDunIt)
	{
		if (whoDunIt.SquishDamage <= 0 || !(whoDunIt is JunkCrusherEvent junkCrusherEvent))
		{
			return;
		}
		Vector2 intersectionDepth = whoDunIt.Bbox.GetIntersectionDepth(Bbox);
		bool flag = Math.Abs(intersectionDepth.Y) < Math.Abs(intersectionDepth.X) && junkCrusherEvent.CrusherState != JunkCrusherEvent.ECrusherState.Rising;
		_isBeingHeldByCrusher = true;
		if (!flag)
		{
			return;
		}
		foreach (GameObject squisher in _squishers)
		{
			if (squisher == whoDunIt)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ChangeSquishedState(shouldIncrement: true);
			_squishers.Add(whoDunIt);
			_level.AddAnimation(EBattleAnimationType.Shrapnel, new Point(Position.X, Bbox.Top), ETeamSide.Enemies);
		}
		else
		{
			_isBeingHeldByCrusher = true;
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, ERobotJunkType junkType)
	{
		base.ID = -1;
		base.HP = base.MaxHP;
		_damageCaused = 48;
		_isMelting = false;
		_isRunningDeathScript = false;
		base.IsGrounded = false;
		base.CurrentMovingPlatforms.Clear();
		_deathScriptTimer = 0f;
		_isDeadButFrozen = false;
		_isFinallyDead = false;
		base.IsDead = false;
		IsFinished = false;
		Position = position;
		_intermediatePositions.Clear();
		_squishers.Clear();
		base.Velocity = Vector2.Zero;
		_brandType = (ERobotJunkBrand)_level.NextRandomInt(0, 2);
		_junkType = junkType;
		ChangeSquishedState(shouldIncrement: false);
	}

	private void ChangeSquishedState(bool shouldIncrement)
	{
		if (shouldIncrement)
		{
			int num = (int)(_junkType + 1);
			if (num <= 4)
			{
				_junkType = (ERobotJunkType)num;
			}
		}
		switch (_brandType)
		{
		case ERobotJunkBrand.Cat:
			switch (_junkType)
			{
			case ERobotJunkType.Junk_Fresh:
				ChangeAnimation(3);
				_bboxOffset = new Point(1, 4);
				Bbox = new Rectangle(0, 0, 35, 29);
				break;
			case ERobotJunkType.Junk_Half_Crushed:
				ChangeAnimation(4);
				_bboxOffset = new Point(2, 3);
				Bbox = new Rectangle(0, 0, 33, 7);
				break;
			case ERobotJunkType.Junk_Crushed:
				ChangeAnimation(5);
				_bboxOffset = new Point(2, 0);
				Bbox = new Rectangle(0, 0, 35, 5);
				break;
			}
			break;
		case ERobotJunkBrand.Star:
			switch (_junkType)
			{
			case ERobotJunkType.Junk_Fresh:
				ChangeAnimation(6);
				_bboxOffset = new Point(3, 1);
				Bbox = new Rectangle(0, 0, 13, 18);
				break;
			case ERobotJunkType.Junk_Half_Crushed:
				ChangeAnimation(7);
				_bboxOffset = new Point(2, 1);
				Bbox = new Rectangle(0, 0, 13, 9);
				break;
			case ERobotJunkType.Junk_Crushed:
				ChangeAnimation(8);
				_bboxOffset = new Point(2, 0);
				Bbox = new Rectangle(0, 0, 13, 5);
				break;
			}
			break;
		default:
			switch (_junkType)
			{
			case ERobotJunkType.Junk_Fresh:
				ChangeAnimation(0);
				_bboxOffset = new Point(1, 2);
				Bbox = new Rectangle(0, 0, 24, 30);
				break;
			case ERobotJunkType.Junk_Half_Crushed:
				ChangeAnimation(1);
				_bboxOffset = new Point(1, 0);
				Bbox = new Rectangle(0, 0, 30, 10);
				break;
			case ERobotJunkType.Junk_Crushed:
				ChangeAnimation(2);
				_bboxOffset = new Point(1, 0);
				Bbox = new Rectangle(0, 0, 30, 5);
				break;
			}
			break;
		}
		_frozenRectangle = Bbox;
		_frozenBboxOffset = _bboxOffset;
		_unfrozenRectangle = new Rectangle(0, 0, Bbox.Width, 32);
		_unfrozenBboxOffset = new Point(_bboxOffset.X, -(32 - Bbox.Height) + _bboxOffset.Y);
		if (!base.IsFrozen)
		{
			_bboxOffset = _unfrozenBboxOffset;
			Bbox = _unfrozenRectangle;
		}
		SnapBboxToPosition();
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result;
		if (type == EDamageType.Spike)
		{
			result = true;
			_isMelting = true;
			Kill();
		}
		else
		{
			result = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		}
		return result;
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_isMelting)
		{
			SilentKill();
		}
		else
		{
			base.UpdateDeathScript(delta);
		}
	}
}
