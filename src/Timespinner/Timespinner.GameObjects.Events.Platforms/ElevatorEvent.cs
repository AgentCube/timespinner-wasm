using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Doors;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;

namespace Timespinner.GameObjects.Events.Platforms;

internal sealed class ElevatorEvent : GameEvent
{
	private enum EElevatorState
	{
		Stopped,
		GoingDown,
		GoingUp
	}

	private const int SlowDownThresholdDistanceY = 64;

	private const int CavesLineTopY = 48;

	private const int CaveElevatorLineBottomOffsetY = 84;

	private const int CaveElevatorLineOffsetX = -3;

	private const int CaveLineIntervalY = 16;

	private const float TimeToAccelerate = 0.5f;

	private const float TimeToDeccelerate = 0.5f;

	private const float CityElevatorMaxSpeed = 900f;

	private const float CavesElevatorMaxSpeed = 600f;

	private const string ElevatorFloorKey = "Elevator_Floor_{0}";

	private const string CaveElevatorBlockadeKey = "Cave_Elevator";

	private const string CaveElevatorHasBeenUsedSinceEntering = "Cave_Elevator_Used";

	private readonly bool _isCityElevator;

	private readonly float _elevatorMaxSpeed;

	private readonly List<Point> _elevatorNodes = new List<Point>();

	private readonly List<ElevatorDoorEvent> _elevatorDoors = new List<ElevatorDoorEvent>();

	private readonly GamePadWrapper _gamePad;

	private EElevatorState _elevatorState;

	private bool _isPlayerHoldingUp;

	private bool _isPlayerHoldingDown;

	private bool _wasPlayerHoldingUp;

	private bool _wasPlayerHoldingDown;

	private bool _isHeroTouchingUs;

	private bool _isLockedIntoStopping;

	private bool _isMovingToFloorWithoutPlayer;

	private bool _isCaveBlockadeThere;

	private int _currentElevatorIndex;

	private int _targetElevatorIndex;

	private int _slowDownStartY;

	private int _blockadeDestroyThresholdY;

	private float _travelTimer;

	private Point _targetElevatorPosition;

	private Vector2 _currentVector;

	private ScriptAction _playerWalkScript;

	private ScriptAction _playerIdleScript;

	private SFXCueInstance _elevatorLoopInstance;

	private EnvPrefabCavesElevatorBlockade _elevatorBlockade;

	internal int CurrentElevatorIndex => _currentElevatorIndex;

	public ElevatorEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_isCityElevator = objectSpec == null || objectSpec.Argument == 0;
		base.EventType = EEventTileType.Elevator;
		_sprite = _level.GCM.SpPlatforms;
		_bbox = new Rectangle(0, 0, 48, 16);
		_bboxOffset = new Point(0, 0);
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		_doesUseAppendageCollision = _isCityElevator;
		base.DrawPlane = (_isCityElevator ? EDrawPlane.Front : EDrawPlane.Back);
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = true;
		IsFacingLeft = objectSpec?.IsFlippedHorizontally ?? true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = true;
		base.DoesPreventCrouching = true;
		_currentVector = Vector2.Zero;
		_elevatorMaxSpeed = (_isCityElevator ? 900f : 600f);
		_gamePad = new GamePadWrapper(_level.Player1Controller, _level.ConfigSave.PlayerControllerMapping, canVibrate: false);
		AddNode(inPosition);
		SetCharacterSequenceByName("Idle");
	}

	public override void Initialize()
	{
		_elevatorNodes.Sort((Point a, Point b) => a.Y.CompareTo(b.Y));
		if (_isCityElevator)
		{
			_elevatorDoors.Sort(delegate(ElevatorDoorEvent a, ElevatorDoorEvent b)
			{
				int y = a.Position.Y;
				return y.CompareTo(b.Position.Y);
			});
			int num = 0;
			foreach (ElevatorDoorEvent elevatorDoor in _elevatorDoors)
			{
				elevatorDoor.FloorIndex = num;
				num++;
			}
		}
		_elevatorState = EElevatorState.Stopped;
		string key = $"Elevator_Floor_{_level.RoomID}";
		_targetElevatorIndex = _level.GetLevelSaveInt(key);
		if (!_isCityElevator)
		{
			_isCaveBlockadeThere = !_level.GameSave.GetSaveBool("Cave_Elevator");
			if (_isCaveBlockadeThere)
			{
				_targetElevatorIndex = 1;
				ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification(491);
				objectTileSpecification.Argument = 803;
				ObjectTileSpecification objectSpec = objectTileSpecification;
				_elevatorBlockade = new EnvPrefabCavesElevatorBlockade(_level, Position.Add(0, 10), -1, objectSpec, EEnvironmentPrefabType.L8_ElevatorBlockade);
				_blockadeDestroyThresholdY = Position.Y + 90;
				_level.RequestAddObject(_elevatorBlockade);
				_elevatorBlockade.Initialize();
			}
			else if (!_level.GetLevelSaveBool("Cave_Elevator_Used"))
			{
				_targetElevatorIndex = 1;
			}
		}
		if (_targetElevatorIndex > _elevatorNodes.Count)
		{
			_targetElevatorIndex = 0;
		}
		if (_targetElevatorIndex < _elevatorNodes.Count)
		{
			_targetElevatorPosition = _elevatorNodes[_targetElevatorIndex];
		}
		StopElevator();
		if (_isCityElevator && _level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.ElevatorKeycard))
		{
			_elevatorDoors[_currentElevatorIndex].ForceOpen();
		}
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!_isFrozen && who.BaseType == EGameObjectBaseType.Hero)
		{
			Vector2 intersectionDepth = who.Bbox.GetIntersectionDepth(_bbox);
			if (intersectionDepth != Vector2.Zero && who.Bbox.Top < Bbox.Top)
			{
				_isHeroTouchingUs = true;
				if (_elevatorState == EElevatorState.Stopped)
				{
					_level.RequestButtonPrompt(5, new Point(Position.X - 2, Position.Y - 32));
				}
			}
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (!_isFrozen)
		{
			UpdateControls();
			UpdateMovement(delta);
			if (_isCaveBlockadeThere && _elevatorBlockade != null && Position.Y < _blockadeDestroyThresholdY)
			{
				_isCaveBlockadeThere = false;
				_elevatorBlockade.DoDestroy();
				_level.GameSave.SetValue("Cave_Elevator", value: true);
				_level.PlayCue(ESFX.EnvCavesElevatorBreakThru, Position);
				_elevatorBlockade = null;
			}
		}
		else if (_elevatorState != 0)
		{
			_level.AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.StopCast
			});
		}
		_isHeroTouchingUs = false;
	}

	private void UpdateControls()
	{
		_wasPlayerHoldingDown = _isPlayerHoldingDown;
		_wasPlayerHoldingUp = _isPlayerHoldingUp;
		_gamePad.UpdateState(areControlsLocked: false);
		_isPlayerHoldingUp = _gamePad.IsUpDown;
		_isPlayerHoldingDown = _gamePad.IsDownDown;
		if (_isMovingToFloorWithoutPlayer)
		{
			return;
		}
		if (_elevatorState == EElevatorState.Stopped)
		{
			if (_isHeroTouchingUs && (_isPlayerHoldingUp || _isPlayerHoldingDown) && !_gamePad.IsLeftDown && !_gamePad.IsRightDown)
			{
				StartElevator(_isPlayerHoldingUp ? EDirection.North : EDirection.South);
				if (!_isCityElevator)
				{
					_level.SetLevelSaveBool("Cave_Elevator_Used", value: true);
				}
			}
		}
		else
		{
			if (_isLockedIntoStopping || (_elevatorState != EElevatorState.GoingDown && _elevatorState != EElevatorState.GoingUp))
			{
				return;
			}
			bool flag = _elevatorState == EElevatorState.GoingDown;
			bool flag2 = (flag ? _isPlayerHoldingDown : _isPlayerHoldingUp);
			bool flag3 = (flag ? _wasPlayerHoldingDown : _wasPlayerHoldingUp);
			if (!flag2 && flag3)
			{
				int y = Position.Y;
				for (int i = 0; i < _elevatorNodes.Count; i++)
				{
					if (flag)
					{
						if (_elevatorNodes[i].Y > y)
						{
							_targetElevatorIndex = i;
							break;
						}
					}
					else
					{
						if (_elevatorNodes[i].Y >= y)
						{
							break;
						}
						_targetElevatorIndex = i;
					}
				}
				_targetElevatorPosition = _elevatorNodes[_targetElevatorIndex];
			}
			else if (flag2)
			{
				_targetElevatorIndex = (flag ? (_elevatorNodes.Count - 1) : 0);
				_targetElevatorPosition = _elevatorNodes[_targetElevatorIndex];
			}
		}
	}

	private void StartElevator(EDirection direction)
	{
		bool flag = direction == EDirection.South;
		int num = (flag ? (_elevatorNodes.Count - 1) : 0);
		if (num != _currentElevatorIndex)
		{
			_travelTimer = 0f;
			_isLockedIntoStopping = false;
			_targetElevatorIndex = num;
			_targetElevatorPosition = _elevatorNodes[_targetElevatorIndex];
			if (_isCityElevator)
			{
				_elevatorDoors[_currentElevatorIndex].CloseDoor();
				PlayCue(ESFX.EnvElevatorDoorClose, Position);
			}
			else
			{
				PlayCue(ESFX.EnvCavesElevatorLoopStart, Position);
			}
			MovePlayerToCenter();
			_elevatorState = (flag ? EElevatorState.GoingDown : EElevatorState.GoingUp);
			PlayLoopCue();
		}
	}

	private void StartElevator(int floorIndex)
	{
		_targetElevatorIndex = floorIndex;
		int y = _elevatorNodes[floorIndex].Y;
		_travelTimer = 0f;
		_isLockedIntoStopping = false;
		_targetElevatorPosition = _elevatorNodes[_targetElevatorIndex];
		_elevatorState = ((y > _position.Y) ? EElevatorState.GoingDown : EElevatorState.GoingUp);
		if (_isCityElevator)
		{
			_elevatorDoors[_currentElevatorIndex].CloseDoor();
		}
		PlayLoopCue();
	}

	private void PlayLoopCue()
	{
		if (_elevatorLoopInstance == null)
		{
			_elevatorLoopInstance = PlayCue(_isCityElevator ? ESFX.EnvElevator : ESFX.EnvCavesElevatorLoop, isLooped: true);
		}
		else
		{
			_elevatorLoopInstance.Resume();
		}
	}

	private void UpdateMovement(float delta)
	{
		if (_elevatorState != 0 && ((_playerWalkScript != null && _playerWalkScript.IsFinished) || _isMovingToFloorWithoutPlayer))
		{
			float num = _elevatorMaxSpeed;
			if (!_isLockedIntoStopping)
			{
				if (Math.Abs(Position.Y - _targetElevatorPosition.Y) < 64)
				{
					_isLockedIntoStopping = true;
					_slowDownStartY = Position.Y;
					_travelTimer = 0f;
				}
				else
				{
					_travelTimer += delta;
					if (_travelTimer < 0.5f)
					{
						float num2 = _travelTimer / 0.5f;
						num *= (float)Math.Sin(num2 * ((float)Math.PI / 4f));
					}
				}
			}
			Vector2 value;
			if (_isLockedIntoStopping)
			{
				_travelTimer += delta;
				Vector2 floatPosition = _floatPosition;
				if (_travelTimer < 0.5f)
				{
					float percentage = _travelTimer / 0.5f;
					float num3 = MathEx.SineInterpolate(_slowDownStartY, _targetElevatorPosition.Y, percentage);
					Position = new Point(Position.X, (int)num3);
				}
				else
				{
					StopElevator();
					Position = _targetElevatorPosition;
					if (_isCityElevator)
					{
						_elevatorDoors[_targetElevatorIndex].DoOpenScript();
						PlayCue(ESFX.EnvElevatorDoorOpen, Position);
					}
					else
					{
						PlayCue(ESFX.EnvCavesElevatorLoopEnd, Position);
					}
				}
				value = floatPosition.Subtract(Position);
			}
			else
			{
				_currentVector = new Vector2(0f, (_elevatorState == EElevatorState.GoingDown) ? num : (0f - num));
				value = new Vector2(0f, _currentVector.Y * delta);
				_floatPosition = Vector2.Add(_floatPosition, value);
				_position = new Point((int)Math.Floor(_floatPosition.X), (int)Math.Floor(_floatPosition.Y));
			}
			float num4 = _gravityAcceleration * delta;
			base.AmountMovedLastStep = new Vector2(value.X, value.Y + num4);
		}
		else
		{
			base.AmountMovedLastStep = Vector2.Zero;
		}
	}

	private void StopElevator()
	{
		_elevatorState = EElevatorState.Stopped;
		_currentVector = Vector2.Zero;
		Position = _targetElevatorPosition;
		if (_elevatorDoors.Count > _currentElevatorIndex)
		{
			ElevatorDoorEvent elevatorDoorEvent = _elevatorDoors[_currentElevatorIndex];
			if (elevatorDoorEvent.DoorState == ESlidingDoorState.Opened)
			{
				elevatorDoorEvent.CloseDoor();
			}
		}
		_currentElevatorIndex = _targetElevatorIndex;
		_isMovingToFloorWithoutPlayer = false;
		_level.SetLevelSaveInt($"Elevator_Floor_{_level.RoomID}", _currentElevatorIndex);
		if (_playerIdleScript != null)
		{
			_playerIdleScript.ActionTimer = -1f;
		}
		if (_elevatorLoopInstance != null)
		{
			_elevatorLoopInstance.Pause(0.1f);
		}
	}

	internal void AddNode(Point position)
	{
		_elevatorNodes.Add(position);
		if (_isCityElevator)
		{
			Point inPosition = new Point(position.X + (IsFacingLeft ? (-32) : 32), position.Y - 16);
			ElevatorDoorEvent elevatorDoorEvent = new ElevatorDoorEvent(_level, inPosition, _sprite, new ObjectTileSpecification
			{
				IsFlippedHorizontally = IsFacingLeft
			}, this);
			_level.AddEvent(elevatorDoorEvent);
			_elevatorDoors.Add(elevatorDoorEvent);
		}
	}

	internal void MovePlayerToCenter()
	{
		_level.AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		Point point = new Point(Position.X, Position.X - 16);
		_playerWalkScript = new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			Arguments = new Vector4(point.X, point.Y, 0f, 0f)
		};
		_level.AddScript(_playerWalkScript);
		_playerIdleScript = new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 10f,
			DoesBlockQueue = true
		};
		_level.AddScript(_playerIdleScript);
	}

	public void CallToFloor(int floorIndex)
	{
		if (!_isMovingToFloorWithoutPlayer && _currentElevatorIndex != floorIndex && !_isHeroTouchingUs && _elevatorState == EElevatorState.Stopped)
		{
			_isMovingToFloorWithoutPlayer = true;
			StartElevator(floorIndex);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isCityElevator)
		{
			Rectangle frameSource = _sprite.GetFrameSource(27);
			Rectangle visibleArea = _level.VisibleArea;
			Vector2 cameraPosition = _level.CameraPosition;
			float cameraZoom = _level.CameraZoom;
			Vector2 levelRenderCenter = _level.LevelRenderCenter;
			Color white = Color.White;
			int num = visibleArea.Top - 20;
			int bottom = visibleArea.Bottom;
			int num2 = Position.X + -3;
			int num3 = base.Appendages[0].Position.Y - 84;
			for (int num4 = num3; num4 >= 48; num4 -= 16)
			{
				if (num4 >= num && num4 <= bottom)
				{
					Vector2 value = new Vector2(cameraPosition.X - (float)num2, cameraPosition.Y - (float)num4);
					value = Vector2.Multiply(value, cameraZoom);
					spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(levelRenderCenter, value), frameSource, white, base.Rotation, Vector2.Zero, cameraZoom, SpriteEffects.None, 0f);
				}
			}
		}
		base.Draw(spriteBatch);
	}
}
