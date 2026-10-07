using System.Collections.Generic;
using Timespinner.Core.Specifications;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Gameplay.CharacterSequences;

internal class CharacterSequenceInstance
{
	private readonly Animate _target;

	private readonly CharacterSequenceSpecification _specification;

	private readonly Queue<CharacterActionSetInstance> _waitingActionSets = new Queue<CharacterActionSetInstance>();

	private readonly List<CharacterActionSetInstance> _activeActionSets = new List<CharacterActionSetInstance>();

	internal bool IsFinished { get; private set; }

	internal bool DoesRunConcurrently { get; private set; }

	internal int FollowingSequenceIndex { get; private set; }

	internal float DefaultWait { get; private set; }

	internal string Name { get; private set; }

	internal CharacterSequenceInstance(CharacterSequenceSpecification specification, Animate target)
	{
		_specification = specification;
		_target = target;
		DoesRunConcurrently = _specification.DoesRunConcurrently;
		FollowingSequenceIndex = _specification.FollowingSequenceIndex;
		DefaultWait = _specification.DefaultWait;
		Name = specification.Name;
		Start();
	}

	private void Start()
	{
		foreach (CharacterActionSet actionSet in _specification.ActionSets)
		{
			_waitingActionSets.Enqueue(new CharacterActionSetInstance(actionSet, _target, DefaultWait));
		}
	}

	internal void Update(float delta)
	{
		bool flag = false;
		foreach (CharacterActionSetInstance activeActionSet in _activeActionSets)
		{
			activeActionSet.Update(delta);
			flag = flag || (!activeActionSet.IsFinished && activeActionSet.DoesBlock);
		}
		while (!flag && _waitingActionSets.Count > 0)
		{
			CharacterActionSetInstance characterActionSetInstance = _waitingActionSets.Dequeue();
			characterActionSetInstance.Update(delta);
			_activeActionSets.Add(characterActionSetInstance);
			flag = !characterActionSetInstance.IsFinished && characterActionSetInstance.DoesBlock;
		}
		for (int num = _activeActionSets.Count - 1; num >= 0; num--)
		{
			CharacterActionSetInstance characterActionSetInstance2 = _activeActionSets[num];
			if (characterActionSetInstance2.IsFinished)
			{
				_activeActionSets.RemoveAt(num);
			}
		}
		if (_activeActionSets.Count == 0)
		{
			if (_specification.DoesRepeat)
			{
				Start();
			}
			else
			{
				IsFinished = true;
			}
		}
	}

	internal void ForceEnd()
	{
		while (_waitingActionSets.Count > 0)
		{
			_activeActionSets.Add(_waitingActionSets.Dequeue());
		}
		foreach (CharacterActionSetInstance activeActionSet in _activeActionSets)
		{
			activeActionSet.ForceEnd();
		}
		IsFinished = true;
	}
}
