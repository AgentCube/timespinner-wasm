using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Platforms;

internal sealed class DonutPlatformEvent : GameEvent
{
	private const float TimeToReappear = 4f;

	private const float TimeToFadeIn = 0.5f;

	private const float TimeToChainReactionFall = 0.1f;

	private readonly bool _doesChainReact;

	private readonly Vector2 _originalPosition;

	private bool _isTouched;

	private bool _isDead;

	private float _reappearTimer;

	private float _chainReactionTimer;

	public DonutPlatformEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.EventType = EEventTileType.DonutTile;
		_bbox = new Rectangle(0, 0, 16, 8);
		_bboxOffset = new Point(0, 0);
		IsFacingLeft = true;
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = true;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		_doesChainReact = true;
		base.CanBeTriggeredByFamiliar = true;
		_sprite = _level.GCM.TsEventTiles;
		ChangeAnimation(_level.NextRandomInt(14, 16), 0, 1f, EAnimationType.None);
		_floatPosition.Y -= 8f;
		_originalPosition = _floatPosition;
		SnapBboxToPosition();
		Update(0f);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		StartDonutFall();
		if (_isSolid && who.IsGrabbing)
		{
			who.StopGrabbing();
		}
		return base.TriggerEvent(who, depth);
	}

	public override bool RemoteTriggerEvent(GameEvent otherEvent)
	{
		if (_isSolid)
		{
			_chainReactionTimer = 0.1f;
		}
		return true;
	}

	private void StartDonutFall()
	{
		if (!_isSolid)
		{
			return;
		}
		_isAffectedByGravity = true;
		_isTouched = true;
		base.DoesCollideWithTiles = true;
		base.IsTriggerableByMonsters = false;
		PlayCue(ESFX.FoleyCrumble, Position);
		if (!_doesChainReact)
		{
			return;
		}
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(base.EventType);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item.Position.X <= Position.X + 16 && item.Position.X >= Position.X - 16)
			{
				item.RemoteTriggerEvent(this);
			}
		}
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			if (_isTouched)
			{
				if (_isSolid)
				{
					_level.AddAnimation(EBattleAnimationType.CrackingDust, Bbox.Center, ETeamSide.Neutral);
					_level.AddAnimation(EBattleAnimationType.Pebbles, Bbox.Center, ETeamSide.Neutral);
				}
				_isSolid = false;
				_reappearTimer += delta;
				if (_reappearTimer >= 4f)
				{
					_isSolid = true;
					base.DrawColor = Color.White;
					_reappearTimer = 0f;
					_isTouched = false;
					_isDead = false;
					_chainReactionTimer = 0f;
					base.DoesCollideWithTiles = false;
					base.IsTriggerableByMonsters = true;
				}
				else if (_reappearTimer >= 3.5f)
				{
					_isAffectedByGravity = false;
					_isTriggered = false;
					_floatPosition = _originalPosition;
					SnapBboxToPosition();
					_velocity = Vector2.Zero;
					base.DrawColor = Color.White * (1f - (4f - _reappearTimer) / 0.5f);
				}
			}
			else if (_chainReactionTimer > 0f)
			{
				_chainReactionTimer -= delta;
				if (_chainReactionTimer <= 0f)
				{
					StartDonutFall();
					_chainReactionTimer = 0f;
				}
			}
			base.Update(delta);
		}
		else
		{
			UpdateIsWithinObjectVisibleArea();
		}
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool result = false;
		if (!_isDead && _isTouched)
		{
			_level.AddAnimation(EBattleAnimationType.CrackingDust, new Point(tile.Bbox.Center.X, tile.Bbox.Top), ETeamSide.Neutral);
			_isDead = true;
			base.DrawColor = Color.Transparent;
			base.DoesCollideWithTiles = false;
			base.IsTriggerableByMonsters = false;
			result = true;
		}
		return result;
	}
}
