using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies;

namespace Timespinner.GameObjects.Events;

internal sealed class JunkSpawnerEvent : GameEvent
{
	private const int JunkStorageCount = 8;

	private const float DropInterval = 2.15f;

	private const float DropIntervalSlow = 4.3f;

	private const float DustIntervalDifference = 0.5f;

	private readonly bool _doesSpawnJunk;

	private readonly ERobotJunkType _junkType;

	private readonly float _dropInterval;

	private readonly float _warnDebrisInterval;

	private readonly Point _spawnPoint;

	private readonly ObjectTileSpecification _objectSpec;

	private readonly LabRobotJunk[] _junkStorage = new LabRobotJunk[8];

	private int _junkCount;

	private float _dropTimer;

	public JunkSpawnerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_objectSpec = objectSpec;
		_sprite = null;
		_bbox = new Rectangle(0, 0, 16, 16);
		Position = new Point(_position.X, _position.Y);
		base.CanBeTriggered = false;
		_isAffectedByTime = true;
		_spawnPoint = new Point(Position.X - 8, Position.Y - 16);
		_junkType = (ERobotJunkType)(objectSpec?.Argument ?? 0);
		_doesSpawnJunk = _junkType < ERobotJunkType.Junk_Fresh;
		_dropInterval = ((_junkType == ERobotJunkType.Spawn_Fresh_Slow) ? 4.3f : 2.15f);
		_warnDebrisInterval = _dropInterval - 0.5f;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!_doesSpawnJunk)
		{
			CreateJunk(Position, _junkType);
			_level.RequestRemoveObject(this);
		}
	}

	public override void Update(float delta)
	{
		if (!_level.IsTimeFrozen)
		{
			float dropTimer = _dropTimer;
			if (!_level.IsPowerOff)
			{
				_dropTimer += delta;
			}
			if (_dropTimer >= _dropInterval)
			{
				DropJunk();
				_dropTimer -= _dropInterval;
			}
			else if (_dropTimer >= _warnDebrisInterval && dropTimer < _warnDebrisInterval)
			{
				_level.AddAnimation(EBattleAnimationType.Pebbles, _spawnPoint, ETeamSide.Enemies);
			}
		}
		base.Update(delta);
	}

	private void DropJunk()
	{
		CreateJunk(_spawnPoint, (_junkType == ERobotJunkType.Spawn_Crushed) ? ERobotJunkType.Junk_Crushed : ERobotJunkType.Junk_Fresh);
	}

	private void CreateJunk(Point position, ERobotJunkType type)
	{
		LabRobotJunk labRobotJunk = null;
		if (_junkCount < 8)
		{
			ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification();
			objectTileSpecification.Category = EObjectTileCategory.Enemy;
			objectTileSpecification.ObjectID = 9;
			objectTileSpecification.IsFlippedHorizontally = _objectSpec.IsFlippedHorizontally;
			ObjectTileSpecification objectSpec = objectTileSpecification;
			SpriteSheet spRobotJunk = _level.GCM.SpRobotJunk;
			labRobotJunk = new LabRobotJunk(position, _level, spRobotJunk, -1, objectSpec, type);
			_junkStorage[_junkCount] = labRobotJunk;
			_junkCount++;
		}
		else
		{
			LabRobotJunk[] junkStorage = _junkStorage;
			foreach (LabRobotJunk labRobotJunk2 in junkStorage)
			{
				if (labRobotJunk2.IsFinished)
				{
					labRobotJunk2.Reset(position, type);
					labRobotJunk2.Update(0f);
					labRobotJunk = labRobotJunk2;
					break;
				}
			}
		}
		if (labRobotJunk != null)
		{
			_level.RequestAddObject(labRobotJunk);
			labRobotJunk.InitializeMob();
		}
	}
}
