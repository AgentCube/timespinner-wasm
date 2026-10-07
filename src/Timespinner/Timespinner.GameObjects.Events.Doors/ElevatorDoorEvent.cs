using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Events.Platforms;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class ElevatorDoorEvent : SlidingDoorEvent
{
	private const int Rfid_ThresholdX = 300;

	private const int Rfid_ThresholdY = 80;

	private const float TimeToOpen = 0.5f;

	private const float TimeToClose = 0.5f;

	private const float TimeBetweenCheckingForPlayer = 1f;

	private const float OrbGlowFrequency = 12f;

	private static readonly Vector4 BaseOrbGlowColor = new Vector4(0.9f, 0.95f, 1f, 1f);

	private readonly int _doorTop;

	private readonly int _baseX;

	private readonly ElevatorEvent _parentElevator;

	private readonly Appendage _gemAppendage;

	private bool _canBeUsed;

	private int _openCloseStartingY;

	private float _openTimer;

	private float _playerCheckTimer;

	private float _oscillDelta;

	internal int FloorIndex { get; set; }

	public ElevatorDoorEvent(Level inLevel, Point inPosition, SpriteSheet sprite, ObjectTileSpecification objectSpec, ElevatorEvent parentElevator)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		_parentElevator = parentElevator;
		_bbox = new Rectangle(0, 0, 16, 64);
		IsFacingLeft = objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CanBeTriggeredByFamiliar = true;
		ChangeAnimation(20);
		_baseX = Position.X;
		_doorTop = _baseY - _bbox.Height;
		_playerCheckTimer = 1f;
		_doAppendagesInheritDrawColor = false;
		_gemAppendage = new Appendage(this, new Point(8, 8), Point.Zero, _level, _sprite)
		{
			DrawPriority = 1,
			FollowType = EAppendageFollowType.ParentObjectLocked,
			AnchorOffset = new Point(0, -28)
		};
		_gemAppendage.ChangeAnimation(21);
		_appendages.Add(_gemAppendage);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_playerCheckTimer += delta;
			if (_playerCheckTimer >= 1f)
			{
				_playerCheckTimer -= 1f;
				if (_level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ElevatorKeycard))
				{
					_canBeUsed = true;
					Point playerPosition = _level.GetPlayerPosition();
					if (Math.Abs(playerPosition.X - _baseX) < 300 && Math.Abs(playerPosition.Y - _baseY) < 80)
					{
						_parentElevator.CallToFloor(FloorIndex);
					}
				}
				else
				{
					_canBeUsed = false;
				}
			}
			if (_canBeUsed)
			{
				_oscillDelta += delta * 12f;
				if (_oscillDelta > (float)Math.PI * 2f)
				{
					_oscillDelta -= (float)Math.PI * 2f;
				}
				Vector4 baseOrbGlowColor = BaseOrbGlowColor;
				baseOrbGlowColor.W = (float)Math.Cos(_oscillDelta) * 0.3f + 0.7f;
				_gemAppendage.IsGlowing = true;
				_gemAppendage.GlowBase = 2f;
				_gemAppendage.GlowColor = new Color(baseOrbGlowColor);
			}
			else
			{
				_gemAppendage.IsGlowing = false;
			}
			base.IsLocked = _parentElevator.CurrentElevatorIndex != FloorIndex || !_canBeUsed;
		}
		base.Update(delta);
	}

	protected override void UpdateDoorState(float delta)
	{
		if (base.IsFrozen)
		{
			return;
		}
		switch (_doorState)
		{
		case ESlidingDoorState.Opened:
			if (_closeTimer > 0f)
			{
				_closeTimer -= delta;
				if (_closeTimer < 0f)
				{
					CloseAndLock();
				}
			}
			break;
		case ESlidingDoorState.Opening:
			if (_openTimer <= 0f)
			{
				_openCloseStartingY = _position.Y;
			}
			_openTimer += delta;
			if (_openTimer < 0.5f)
			{
				float percentage2 = _openTimer / 0.5f;
				Position = new Point(Position.X, (int)MathEx.SineInterpolate(_openCloseStartingY, _doorTop, percentage2));
			}
			else
			{
				Position = new Point(Position.X, _doorTop);
				_doorState = ESlidingDoorState.Opened;
				_openTimer = 0f;
			}
			break;
		case ESlidingDoorState.Falling:
			if (_closeTimer <= 0f)
			{
				_openCloseStartingY = _position.Y;
			}
			_closeTimer += delta;
			if (_closeTimer < 0.5f)
			{
				float percentage = _closeTimer / 0.5f;
				Position = new Point(Position.X, (int)MathEx.SineInterpolate(_openCloseStartingY, _baseY, percentage));
			}
			else
			{
				Position = new Point(_position.X, _baseY);
				_doorState = ESlidingDoorState.Closed;
				_closeTimer = 0f;
			}
			break;
		case ESlidingDoorState.Bouncing:
			_doorState = ESlidingDoorState.Closed;
			break;
		}
	}

	public override void DoOpenScript()
	{
		_doorState = ESlidingDoorState.Opening;
		_openTimer = 0f;
	}

	protected override void CloseAndLock()
	{
		_doorState = ESlidingDoorState.Falling;
		_closeTimer = 0f;
	}

	internal void CloseDoor()
	{
		CloseAndLock();
	}

	internal void ForceOpen()
	{
		Position = new Point(Position.X, _doorTop);
		_doorState = ESlidingDoorState.Opened;
		_openTimer = 0f;
	}
}
