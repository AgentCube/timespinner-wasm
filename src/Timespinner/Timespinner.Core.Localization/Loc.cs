namespace Timespinner.Core.Localization;

internal static class Loc
{
	private static ELanguageLocale _currentLocale;

	internal static bool IsAsianLocale;

	private static StringLibrary _currentLibrary;

	internal static ELanguageLocale CurrentLocale => _currentLocale;

	internal static string Get(string key)
	{
		if (_currentLibrary == null)
		{
			_currentLibrary = new StringLibrary(ELanguageLocale.EN);
		}
		return _currentLibrary.Get(key);
	}

	internal static bool DoesExist(string key)
	{
		if (_currentLibrary == null)
		{
			_currentLibrary = new StringLibrary(ELanguageLocale.EN);
		}
		return _currentLibrary.DoesExist(key);
	}

	internal static StringInstance GetDialogue(string key)
	{
		if (_currentLibrary == null)
		{
			_currentLibrary = new StringLibrary(ELanguageLocale.EN);
		}
		return _currentLibrary.GetDialogue(key);
	}

	internal static string SurroundWithQuotationMarks(string description)
	{
		if (_currentLibrary == null)
		{
			_currentLibrary = new StringLibrary(ELanguageLocale.EN);
		}
		return _currentLibrary.SurroundWithQuotationMarks(description);
	}

	internal static void SwitchLoc(ELanguageLocale locale)
	{
		if (_currentLibrary == null || _currentLibrary.Language != locale)
		{
			_currentLibrary = new StringLibrary(locale);
		}
		IsAsianLocale = locale == ELanguageLocale.CN || locale == ELanguageLocale.JP;
		_currentLocale = locale;
	}
}
