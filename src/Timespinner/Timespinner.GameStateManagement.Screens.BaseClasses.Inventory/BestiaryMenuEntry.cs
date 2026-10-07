using Timespinner.Core.Specifications;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Inventory;

internal class BestiaryMenuEntry : MenuEntry
{
	private readonly int _killCount;

	private readonly string _indexString;

	private readonly BestiaryEntrySpecification _bestiaryEntry;

	internal int KillCount => _killCount;

	internal string IndexString => _indexString;

	internal BestiaryEntrySpecification BestiaryEntry => _bestiaryEntry;

	public BestiaryMenuEntry(BestiaryEntrySpecification bestiaryEntry, int killCount, string title)
		: base(title)
	{
		_killCount = killCount;
		_bestiaryEntry = bestiaryEntry;
		_indexString = bestiaryEntry.Index.ToString("D2");
		base.Description = ((_killCount > 0) ? bestiaryEntry.VisibleDescription : "???");
	}
}
