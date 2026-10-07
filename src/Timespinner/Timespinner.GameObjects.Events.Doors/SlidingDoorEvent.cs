using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

public abstract class SlidingDoorEvent : GameEvent
{
	private const float TimeBetweenOpeningDebrisEmission = 0.25f;

	private const float RaiseRate = 60f;

	private const float FallRate = 20f;

	private const float BounceRate = 30f;

	protected bool _isLocked;

	protected ESlidingDoorState _doorState;

	protected int _baseY;

	protected int _bouncePass;

	protected float _closeTimer;

	protected float _fallVelocity;

	private float _debrisTimer;

	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;
		}
	}

	internal bool IsOpenForever { get; set; }

	internal ESlidingDoorState DoorState => _doorState;

	protected SlidingDoorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isLocked = false;
		_bbox = new Rectangle(0, 0, 16, 80);
		_baseY = inPosition.Y;
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		base.IsTriggerableByMonsters = true;
		base.DrawPlane = EDrawPlane.Normal;
		_doorState = ESlidingDoorState.Closed;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = base.TriggerEvent(who, depth);
		if (!_isLocked && _doorState == ESlidingDoorState.Closed && who.DefaultTeam == ETeamSide.Heroes)
		{
			DoOpenScript();
		}
		return result;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		UpdateDoorState(delta);
	}

	protected virtual void UpdateDoorState(float delta)
	{
		switch (_doorState)
		{
		case ESlidingDoorState.Opened:
			if (_closeTimer > 0f && !IsOpenForever)
			{
				_closeTimer -= delta;
				if (_closeTimer < 0f)
				{
					CloseAndLock();
					_debrisTimer = 0f;
				}
			}
			break;
		case ESlidingDoorState.Opening:
		{
			_floatPosition = new Vector2(_floatPosition.X, _floatPosition.Y - 60f * delta);
			int num = _baseY - _bbox.Height;
			if (_floatPosition.Y < (float)num)
			{
				Position = new Point(Position.X, num);
				_doorState = ESlidingDoorState.Opened;
			}
			_debrisTimer -= delta;
			if (_debrisTimer <= 0f)
			{
				_debrisTimer = 0.25f;
				EmitOpeningParticles(num);
			}
			break;
		}
		case ESlidingDoorState.Falling:
			if (_fallVelocity <= 0f)
			{
				_level.PlayCue(ESFX.DoorBossClose, Position);
				_level.PlayCue(ESFX.DoorBossClose2D);
			}
			_fallVelocity += 20f;
			Position = new Point(_position.X, (int)((float)_position.Y + _fallVelocity * delta));
			if (_position.Y > _baseY)
			{
				Position = new Point(_position.X, _baseY);
				_doorState = ESlidingDoorState.Bouncing;
				_fallVelocity = -100f;
				EmitClosedParticles();
			}
			break;
		case ESlidingDoorState.Bouncing:
			_fallVelocity += 30f;
			Position = new Point(_position.X, (int)((float)_position.Y + _fallVelocity * delta));
			if (_position.Y > _baseY)
			{
				_bouncePass++;
				_fallVelocity = -150f / ((float)_bouncePass + 1f);
				if (_bouncePass > 3)
				{
					Position = new Point(_position.X, _baseY);
					_doorState = ESlidingDoorState.Closed;
					_fallVelocity = 0f;
					_bouncePass = 0;
					_debrisTimer = 0f;
				}
			}
			break;
		}
	}

	protected virtual void EmitOpeningParticles(int doorTop)
	{
		Point position = new Point(Bbox.Center.X, doorTop);
		BattleAnimation newAnimation = BattleAnimation.Create(EBattleAnimationType.CrackingDust, position, ETeamSide.Heroes, imageFacingRight: false, _level, doesPlaySFX: true);
		BattleAnimation newAnimation2 = BattleAnimation.Create(EBattleAnimationType.Pebbles, position, ETeamSide.Heroes, imageFacingRight: false, _level, doesPlaySFX: true);
		AddBattleAnimation(newAnimation);
		AddBattleAnimation(newAnimation2);
	}

	protected virtual void EmitClosedParticles()
	{
		_level.AddAnimation(EBattleAnimationType.Dust, Position);
	}

	public virtual void DoOpenScript()
	{
		_doorState = ESlidingDoorState.Opening;
		_level.PlayCue(ESFX.DoorBossOpen, Position);
	}

	public void LockDoor(float waiter)
	{
		_closeTimer = waiter;
		if (_doorState == ESlidingDoorState.Closed)
		{
			_isLocked = true;
		}
	}

	protected virtual void CloseAndLock()
	{
		_doorState = ESlidingDoorState.Falling;
		_isLocked = true;
	}

	public void OpenDoor(float waitTime)
	{
		if (waitTime < 0f)
		{
			SetPositionToOpen();
			_doorState = ESlidingDoorState.Opened;
		}
		else
		{
			PlayCue(ESFX.DoorBossOpen);
			_doorState = ESlidingDoorState.Opening;
			_isLocked = false;
		}
	}

	internal virtual void SetPositionToOpen()
	{
		Position = new Point(_position.X, _baseY - _bbox.Height);
	}
}
