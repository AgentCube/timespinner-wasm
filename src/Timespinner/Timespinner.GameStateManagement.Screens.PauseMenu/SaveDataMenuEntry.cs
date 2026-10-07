using System.Globalization;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class SaveDataMenuEntry : MenuEntry
{
	private readonly bool _isInteger;

	private readonly string _key;

	private bool _boolValue;

	private int _intValue;

	internal bool IsInteger => _isInteger;

	internal bool BoolValue => _boolValue;

	internal int IntValue => _intValue;

	internal string Key => _key;

	internal string Value
	{
		get
		{
			if (!_isInteger)
			{
				return _boolValue.ToString();
			}
			return _intValue.ToString(CultureInfo.InvariantCulture);
		}
	}

	public SaveDataMenuEntry(string key, bool boolValue)
		: base(key)
	{
		_key = key;
		_isInteger = false;
		SetValue(boolValue);
	}

	public SaveDataMenuEntry(string key, int intValue)
		: base(key)
	{
		_key = key;
		_isInteger = true;
		SetValue(intValue);
	}

	internal void SetValue(bool newValue)
	{
		_boolValue = newValue;
		SetText($"{Value} {Key}");
	}

	internal void SetValue(int newValue)
	{
		_intValue = newValue;
		SetText($"{Value} {Key}");
	}
}
