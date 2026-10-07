using System;
using System.Collections.Generic;

namespace Timespinner.Core.Specifications;

[Serializable]
public class CharacterActionSet
{
	public bool DoesRepeat { get; set; }

	public int RepeatCount { get; set; }

	public List<CharacterAction> Actions { get; set; }

	public CharacterActionSet()
	{
		Actions = new List<CharacterAction>();
	}

	public CharacterActionSet Duplicate()
	{
		CharacterActionSet characterActionSet = new CharacterActionSet();
		characterActionSet.DoesRepeat = DoesRepeat;
		characterActionSet.RepeatCount = RepeatCount;
		CharacterActionSet characterActionSet2 = characterActionSet;
		foreach (CharacterAction action in Actions)
		{
			characterActionSet2.Actions.Add(action.Duplicate());
		}
		return characterActionSet2;
	}
}
