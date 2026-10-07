using System;

namespace Timespinner.GameStateManagement;

internal class ConfirmationMenuEntryCollection : MenuEntryCollection
{
	public const float YesNoDisplayPositionRatioX = 0.125f;

	public const float YesNoDisplayPositionRatioY = 0.5f;

	public ConfirmationMenuEntryCollection(string yesString, string noString, string promptString, Action<object, PlayerIndexEventArgs> yesAction, Action<object, PlayerIndexEventArgs> noAction)
	{
		base.ColumnCount = 2;
		MenuEntry menuEntry = new MenuEntry(yesString)
		{
			Description = promptString,
			DoesConfirmationPlaySound = false
		};
		MenuEntry menuEntry2 = new MenuEntry(noString)
		{
			Description = promptString,
			DoesConfirmationPlaySound = false
		};
		EventHandler<PlayerIndexEventArgs> value = delegate(object a, PlayerIndexEventArgs b)
		{
			yesAction(a, b);
		};
		menuEntry.Selected += value;
		menuEntry2.Selected += delegate(object a, PlayerIndexEventArgs b)
		{
			noAction(a, b);
		};
		base.Entries.Add(menuEntry);
		base.Entries.Add(menuEntry2);
	}

	internal void SetDescription(string description)
	{
		foreach (MenuEntry entry in base.Entries)
		{
			entry.Description = description;
		}
	}
}
