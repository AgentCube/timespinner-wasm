using System.Collections.Generic;
using Timespinner.Core.Specifications;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Gameplay.CharacterSequences;

internal class CharacterActionSetInstance
{
	private readonly CharacterActionSet _specification;

	private readonly List<CharacterActionInstance> _activeActions = new List<CharacterActionInstance>();

	internal bool DoesBlock { get; private set; }

	internal bool IsFinished { get; private set; }

	internal CharacterActionSetInstance(CharacterActionSet actionSet, Animate target, float defaultWait)
	{
		_specification = actionSet;
		foreach (CharacterAction action in _specification.Actions)
		{
			_activeActions.Add(new CharacterActionInstance(action, target));
		}
		if (defaultWait > 0f)
		{
			_activeActions.Add(new CharacterActionInstance(new CharacterAction
			{
				ActionType = ECharacterActionType.Wait,
				Duration = defaultWait,
				DoesBlock = true
			}, target));
		}
	}

	internal void Update(float delta)
	{
		foreach (CharacterActionInstance activeAction in _activeActions)
		{
			activeAction.Update(delta);
		}
		bool doesBlock = false;
		for (int num = _activeActions.Count - 1; num >= 0; num--)
		{
			CharacterActionInstance characterActionInstance = _activeActions[num];
			if (characterActionInstance.IsFinished)
			{
				_activeActions.RemoveAt(num);
			}
			else if (characterActionInstance.DoesBlock)
			{
				doesBlock = true;
			}
		}
		DoesBlock = doesBlock;
		if (_activeActions.Count == 0)
		{
			IsFinished = true;
		}
	}

	internal void ForceEnd()
	{
		foreach (CharacterActionInstance activeAction in _activeActions)
		{
			activeAction.ForceEnd();
		}
		IsFinished = true;
	}
}
