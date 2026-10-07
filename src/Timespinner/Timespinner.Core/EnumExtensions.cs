using System;
using System.Reflection;

namespace Timespinner.Core;

public static class EnumExtensions
{
	public static T[] GetEnumValues<T>()
	{
		Type typeFromHandle = typeof(T);
		FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Static | BindingFlags.Public);
		T[] array = new T[fields.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (T)fields[i].GetValue(null);
		}
		return array;
	}

	public static T EnumParse<T>(string text)
	{
		return (T)Enum.Parse(typeof(T), text);
	}

	public static T EnumTryParse<T>(string text)
	{
		T result;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				return EnumParse<T>(text);
			}
			catch (ArgumentException)
			{
				result = default(T);
			}
		}
		else
		{
			result = default(T);
		}
		return result;
	}
}
