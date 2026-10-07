using System.Collections.Generic;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Etc;

public class MinionContainer
{
	private readonly int _containerSize;

	private readonly List<Monster> _minions = new List<Monster>();

	private readonly Level _level;

	public bool IsFull => _minions.Count >= _containerSize;

	public MinionContainer(int size, Level level)
	{
		_containerSize = size;
		_level = level;
	}

	public void Initialize()
	{
	}

	public void Update(float delta)
	{
		for (int i = 0; i < _minions.Count; i++)
		{
			Monster monster = _minions[i];
			if (monster.IsDead)
			{
				_minions.RemoveAt(i);
			}
		}
	}

	public void AddMinion(Monster newMinion)
	{
		if (newMinion != null)
		{
			_ = newMinion.ID;
			_minions.Add(newMinion);
			_level.RequestAddObject(newMinion);
		}
	}

	public void KillAll()
	{
		foreach (Monster minion in _minions)
		{
			minion.Kill();
		}
	}
}
