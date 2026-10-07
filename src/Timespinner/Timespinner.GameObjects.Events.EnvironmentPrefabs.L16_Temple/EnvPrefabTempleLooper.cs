using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal class EnvPrefabTempleLooper : EnvironmentPrefabBase
{
	private const int WarpOffsetX = 0;

	private const int WarpOffsetY = 32;

	private const int HeroTrailOffsetX = -8;

	private const int EnemyTrailOffset = -3;

	private const float TrailOffsetTime = 1f / 60f;

	private readonly int _roomWidth;

	private readonly int _roomHeight;

	private readonly int _warpLeft;

	private readonly int _warpRight;

	private readonly int _warpTop;

	private readonly int _warpBottom;

	private float _shiftTrailTimer;

	public EnvPrefabTempleLooper(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_roomWidth = _level.RoomSize.X;
		_roomHeight = _level.RoomSize.Y;
		_warpLeft = 0;
		_warpRight = _roomWidth;
		_warpTop = -32;
		_warpBottom = _roomHeight + 32;
	}

	public override void Initialize()
	{
		base.Initialize();
		foreach (KeyValuePair<int, Protagonist> hero in _level.Heroes)
		{
			hero.Value.IsAffectedByLevelBounds = false;
		}
	}

	public override void SilentKill()
	{
		foreach (KeyValuePair<int, Protagonist> hero in _level.Heroes)
		{
			hero.Value.IsAffectedByLevelBounds = true;
		}
		base.SilentKill();
	}

	public override void Update(float delta)
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			if (mainHero.Position.X < _warpLeft)
			{
				DoWarp(mainHero, EDirection.West);
				if (mainHero.Velocity.X < 0f)
				{
					mainHero.Velocity = new Vector2(0f, mainHero.Velocity.Y);
				}
			}
			else if (mainHero.Position.X > _warpRight)
			{
				DoWarp(mainHero, EDirection.East);
				if (mainHero.Velocity.X > 0f)
				{
					mainHero.Velocity = new Vector2(0f, mainHero.Velocity.Y);
				}
			}
			if (mainHero.Position.Y < _warpTop)
			{
				DoWarp(mainHero, EDirection.North);
			}
			else if (mainHero.Position.Y > _warpBottom)
			{
				DoWarp(mainHero, EDirection.South);
			}
		}
		if (!_level.IsTimeFrozen)
		{
			_shiftTrailTimer += delta;
			if (_shiftTrailTimer >= 1f / 60f)
			{
				_shiftTrailTimer -= 1f / 60f;
				Point offset = new Point(-8, 0);
				Point offset2 = new Point(-3, 0);
				foreach (KeyValuePair<int, Protagonist> hero in _level.Heroes)
				{
					hero.Value.ShiftTrailHistory(offset);
				}
				foreach (Monster visibleEnemy in _level.GetVisibleEnemies())
				{
					visibleEnemy.ShiftTrailHistory(offset2);
				}
			}
		}
		base.Update(delta);
	}

	private void DoWarp(Protagonist hero, EDirection startDirection)
	{
		hero.TeleportToPoint(startDirection switch
		{
			EDirection.East => new Point(_warpRight, hero.Position.Y), 
			EDirection.West => new Point(_warpLeft, hero.Position.Y), 
			EDirection.North => new Point(hero.Position.X, _warpBottom), 
			_ => new Point(hero.Position.X, _warpTop), 
		});
	}
}
