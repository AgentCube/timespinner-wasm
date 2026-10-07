using System.Collections.Generic;

namespace Timespinner.Core.Definitions;

internal sealed class TicketIDDispenser
{
	private readonly List<int> _recycledNumbers = new List<int>();

	private int _nextIncrementalNumber;

	internal void Reset()
	{
		_recycledNumbers.Clear();
		_nextIncrementalNumber = 0;
	}

	internal int GetNext()
	{
		int result;
		if (_recycledNumbers.Count > 0)
		{
			result = _recycledNumbers[0];
			_recycledNumbers.RemoveAt(0);
		}
		else
		{
			result = _nextIncrementalNumber;
			_nextIncrementalNumber++;
		}
		return result;
	}

	internal void Recycle(int toRecycle)
	{
		_recycledNumbers.Add(toRecycle);
	}
}
