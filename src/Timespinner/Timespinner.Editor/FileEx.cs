using System;
using System.IO;

namespace Timespinner.Editor;

public static class FileEx
{
	public static void AtomicSave(Action<string> saveAction, string filepath)
	{
		string text = filepath + ".tmp";
		saveAction(text);
		if (File.Exists(filepath))
		{
			File.Delete(filepath);
		}
		File.Move(text, filepath);
	}
}
