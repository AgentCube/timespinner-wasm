using System;
using System.Collections.Generic;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class QuestMenuEntryCollection : MenuEntryCollection
{
	private readonly Action<QuestMenuEntry> _selectionChangedAction;

	private readonly List<QuestMenuEntry> _questEntries = new List<QuestMenuEntry>();

	private int _selectedIndex = -1;

	internal List<QuestMenuEntry> QuestEntries => _questEntries;

	public QuestMenuEntryCollection(Action<QuestMenuEntry> selectionChangedAction)
	{
		_selectionChangedAction = selectionChangedAction;
	}

	internal void AddQuestEntry(QuestMenuEntry questEntry)
	{
		_questEntries.Add(questEntry);
		base.Entries.Add(questEntry);
	}

	public override void Update(float delta, bool isScreenActive, float transitionPercentage)
	{
		base.Update(delta, isScreenActive, transitionPercentage);
		if (_selectedIndex != base.SelectedIndex)
		{
			_selectionChangedAction(_questEntries[base.SelectedIndex]);
			_selectedIndex = base.SelectedIndex;
		}
	}
}
