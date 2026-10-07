using System;
using System.IO;

namespace Timespinner;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		TimespinnerGame timespinnerGame = null;
		try
		{
			PlatformHelper platformHelper = new PlatformHelper();
			if (!platformHelper.DoesNeedToExit)
			{
				timespinnerGame = new TimespinnerGame(platformHelper);
				timespinnerGame.Run();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("CRASH! +" + ex.ToString());
			File.WriteAllText($"CrashLog_{DateTime.Now.ToFileTime()}.txt", ex.ToString());
			throw;
		}
		try
		{
			timespinnerGame?.Dispose();
		}
		catch (InvalidOperationException value)
		{
			Console.WriteLine(value);
		}
	}
}
