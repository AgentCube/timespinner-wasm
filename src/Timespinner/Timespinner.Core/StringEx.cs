using System;
using System.Globalization;

namespace Timespinner.Core;

public static class StringEx
{
	public static string SafeSubstring(this string input, int start, int length)
	{
		string result = string.Empty;
		if (start < 0)
		{
			start = 0;
		}
		if (input.Length >= start + length && length >= 0)
		{
			result = input.Substring(start, length);
		}
		else if (input.Length > start)
		{
			result = input.Substring(start);
		}
		return result;
	}

	public static byte ParseByte(this string input)
	{
		return byte.Parse(input, CultureInfo.InvariantCulture);
	}

	public static int ParseInt32(this string input)
	{
		return int.Parse(input, CultureInfo.InvariantCulture);
	}

	public static float ParseFloat(this string input)
	{
		return float.Parse(input, CultureInfo.InvariantCulture);
	}

	public static DateTime ParseDateTime(this string input)
	{
		if (!DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			return DateTime.MinValue;
		}
		return result;
	}
}
