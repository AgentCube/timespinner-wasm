using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Localization;

public class StringLibrary
{
	private const string DirectoryPath = "Content/Localization/";

	private const string FileExtension = ".txt";

	private const string DefaultFilename = "TimespinnerStrings";

	private const string StringNotFoundError = "!!!SNF:";

	private readonly ELanguageLocale _language;

	private readonly Dictionary<string, StringInstance> _stringInstances = new Dictionary<string, StringInstance>();

	internal ELanguageLocale Language => _language;

	internal StringLibrary(ELanguageLocale targetLanguage)
	{
		_language = targetLanguage;
		LoadTables();
	}

	private void LoadTables()
	{
		string arg;
		switch (_language)
		{
		case ELanguageLocale.BP:
		case ELanguageLocale.CN:
		case ELanguageLocale.DE:
		case ELanguageLocale.ES:
		case ELanguageLocale.FR:
		case ELanguageLocale.JP:
		case ELanguageLocale.RU:
		case ELanguageLocale.PD:
			arg = _language.ToString();
			break;
		default:
			arg = "TimespinnerStrings";
			break;
		}
		using Stream stream = TitleContainer.OpenStream(string.Format("{0}{1}{2}", "Content/Localization/", arg, ".txt"));
		using StreamReader streamReader = new StreamReader(stream);
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (text == null)
			{
				continue;
			}
			string[] array = text.Split(new char[1] { '\t' });
			if (array[0] != "Category")
			{
				StringInstance stringInstance = new StringInstance();
				stringInstance.Key = array[1];
				stringInstance.Text = ProcessText(array[2], _language);
				stringInstance.Speaker = array[3];
				StringInstance stringInstance2 = stringInstance;
				if (!string.IsNullOrEmpty(stringInstance2.Key) && !_stringInstances.ContainsKey(stringInstance2.Key))
				{
					_stringInstances.Add(stringInstance2.Key, stringInstance2);
				}
			}
		}
	}

	private static string ProcessText(string text, ELanguageLocale locale)
	{
		string text2 = text;
		text2 = text2.Replace("\\n", "\n");
		text2 = text2.TrimStart(new char[1] { '"' });
		text2 = text2.TrimEnd(new char[1] { '"' });
		switch (locale)
		{
		case ELanguageLocale.EN:
			text2 = text2.Replace("’", "'");
			text2 = text2.Replace("‘", "'");
			text2 = text2.Replace("“", "\"");
			text2 = text2.Replace("”", "\"");
			break;
		case ELanguageLocale.BP:
		case ELanguageLocale.RU:
			text2 = text2.Replace("“", "\"");
			text2 = text2.Replace("”", "\"");
			break;
		}
		return text2;
	}

	internal string Get(string key)
	{
		if (_stringInstances.ContainsKey(key))
		{
			return _stringInstances[key].Text;
		}
		return "!!!SNF:" + key + "_" + _language;
	}

	internal bool DoesExist(string key)
	{
		return _stringInstances.ContainsKey(key);
	}

	internal StringInstance GetDialogue(string key)
	{
		if (_stringInstances.ContainsKey(key))
		{
			return _stringInstances[key];
		}
		StringInstance stringInstance = new StringInstance();
		stringInstance.Text = "!!!SNF:" + key + "_" + _language;
		stringInstance.Speaker = key;
		return stringInstance;
	}

	public string SurroundWithQuotationMarks(string description)
	{
		return $"\"{description}\"";
	}
}
