using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.BaseClasses;

public class GameEvent : Animate
{
	private readonly ObjectTileSpecification _objectSpec;

	private bool _isTriggerBboxUnique;

	protected bool _doesPersist;

	protected bool _isTriggered;

	protected bool _wasTriggered;

	protected bool _isRepeatedTrigger;

	protected bool _isBeingTriggered;

	protected bool _wasBeingTriggered;

	protected bool _isSolid;

	protected float _triggerCooldownTimer;

	protected float _timeForTriggerToCooldown;

	private Rectangle _triggerBbox;

	internal bool DoesDrawTriggerBbox { get; set; }

	internal bool CanBeTriggered { get; set; }

	internal bool DoesPreventCrouching { get; set; }

	internal bool IsConsideredPassablePlatform { get; set; }

	internal bool CanBeTriggeredByFamiliar { get; set; }

	public bool CanBeUsedWhenFrozen { get; protected set; }

	public bool DoesCollideWithProjectiles { get; protected set; }

	public bool IsTriggerableByMonsters { get; protected set; }

	public bool IsLostWhenNotTouching { get; protected set; }

	public bool IsLostWhenNotGrounded { get; protected set; }

	public EEventTileType EventType { get; protected set; }

	internal int ObjectArgument
	{
		get
		{
			if (_objectSpec == null)
			{
				return -1;
			}
			return _objectSpec.Argument;
		}
	}

	public Vector2 AmountMovedLastStep { get; protected set; }

	internal Rectangle TriggerBbox
	{
		get
		{
			if (!_isTriggerBboxUnique)
			{
				if (!_doesUseAppendageCollision)
				{
					return Bbox;
				}
				return base.OuterBbox;
			}
			return _triggerBbox;
		}
		set
		{
			_triggerBbox = value;
			_isTriggerBboxUnique = true;
		}
	}

	public GameEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inID)
	{
		base.BaseType = EGameObjectBaseType.Event;
		_objectSpec = objectSpec;
		IsFacingLeft = false;
		_isAffectedByGravity = false;
		_isAffectedByTime = false;
		base.DrawPlane = EDrawPlane.Back;
		base.DoesCollideWithTiles = false;
		CanBeTriggered = true;
		IsLostWhenNotGrounded = true;
		IsLostWhenNotTouching = true;
		base.DoesDrawWhenOutsideOfObjectVisibleArea = false;
		base.CharacterSpecification = _level.GetCharacterSpecification(objectSpec);
		if (base.CharacterSpecification != null && base.CharacterSpecification.CoreAppendage != null)
		{
			CreateAppendagesFromSpecification(base.CharacterSpecification.CoreAppendage);
		}
	}

	public virtual void Initialize()
	{
		bool isFrozen = base.IsFrozen;
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		if (_sprite != null && _appendages.Count > 0)
		{
			foreach (Appendage appendage in _appendages)
			{
				appendage.SetSpriteIfNull(_sprite);
			}
		}
		Update(0f);
		if (isFrozen)
		{
			_isFrozen = true;
		}
	}

	public override void SnapBboxToPosition()
	{
		if (_isTriggerBboxUnique)
		{
			_triggerBbox.Location = new Point(Bbox.Center.X - _triggerBbox.Width / 2, Bbox.Center.Y - _triggerBbox.Height / 2);
		}
		base.SnapBboxToPosition();
	}

	public virtual bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && CanBeTriggered && (_timeForTriggerToCooldown <= 0f || _triggerCooldownTimer <= 0f))
		{
			_isBeingTriggered = true;
			_isTriggered = true;
			_triggerCooldownTimer = _timeForTriggerToCooldown;
		}
		bool flag = false;
		if (_isSolid && who.DoesCollideWithSolidEvents && DoesCollideWith(who.Bbox))
		{
			List<Rectangle> list = FindIntersectingBoundingBoxes(who.Bbox, -1);
			if (list.Count > 0)
			{
				foreach (Rectangle item in list)
				{
					Vector2 intersectionDepth = who.Bbox.GetIntersectionDepth(item);
					flag = who.CollideSolidObject(this, ETileType.Event, intersectionDepth);
				}
			}
		}
		if (flag && DoesPreventCrouching)
		{
			who.IsCrouchingDisabled = true;
		}
		return flag;
	}

	public virtual bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		if (CanBeTriggered)
		{
			_isBeingTriggered = true;
			_isTriggered = true;
		}
		return true;
	}

	public virtual bool RemoteTriggerEvent(GameEvent otherEvent)
	{
		return false;
	}

	public override void Kill()
	{
		base.Kill();
		_level.RequestRemoveObject(this);
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			base.Update(delta);
			if (!_isRepeatedTrigger)
			{
				CanBeTriggered = !_isBeingTriggered;
			}
			if (_timeForTriggerToCooldown > 0f && _triggerCooldownTimer > 0f)
			{
				_triggerCooldownTimer -= delta;
				if (_triggerCooldownTimer < 0f)
				{
					_triggerCooldownTimer = 0f;
				}
			}
			_wasBeingTriggered = _isBeingTriggered;
			_wasTriggered = _isTriggered;
			_isBeingTriggered = false;
		}
		UpdateIsWithinObjectVisibleArea();
	}

	public override void Freeze()
	{
		if (_isAffectedByTime)
		{
			base.Freeze();
		}
	}

	public override void Unfreeze()
	{
		if (_isAffectedByTime)
		{
			base.Unfreeze();
		}
	}
}
