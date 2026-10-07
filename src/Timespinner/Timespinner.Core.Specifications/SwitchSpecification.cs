using System;
using System.Globalization;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications;

[Serializable]
public class SwitchSpecification
{
	public const string XmlKeyAttribute = "Key";

	public const string XmlKeyValueAttribute = "KeyValue";

	public ESwitchValueType SwitchValueType { get; private set; }

	public bool BoolValue { get; set; }

	public int IntValue { get; set; }

	public string StringValue { get; set; }

	public string Key { get; set; }

	public void SetSwitchValueType(ESwitchValueType newType)
	{
		SwitchValueType = newType;
	}

	public static SwitchSpecification FromXml(XmlReader reader)
	{
		SwitchSpecification switchSpecification = new SwitchSpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Key":
				switchSpecification.Key = reader.Value;
				break;
			case "KeyValue":
			{
				int result2;
				if (bool.TryParse(reader.Value, out var result))
				{
					switchSpecification.SwitchValueType = ESwitchValueType.Bool;
					switchSpecification.BoolValue = result;
				}
				else if (int.TryParse(reader.Value, out result2))
				{
					switchSpecification.SwitchValueType = ESwitchValueType.Int;
					switchSpecification.IntValue = result2;
				}
				else
				{
					switchSpecification.SwitchValueType = ESwitchValueType.String;
					switchSpecification.StringValue = reader.Value;
				}
				break;
			}
			}
		}
		return switchSpecification;
	}

	public string GetKeyValue()
	{
		string result = "";
		switch (SwitchValueType)
		{
		case ESwitchValueType.Bool:
			result = BoolValue.ToString();
			break;
		case ESwitchValueType.Int:
			result = IntValue.ToString(CultureInfo.InvariantCulture);
			break;
		case ESwitchValueType.String:
			result = StringValue;
			break;
		}
		return result;
	}

	public SwitchSpecification Duplicate()
	{
		SwitchSpecification switchSpecification = new SwitchSpecification();
		switchSpecification.BoolValue = BoolValue;
		switchSpecification.IntValue = IntValue;
		switchSpecification.StringValue = StringValue;
		switchSpecification.Key = Key;
		switchSpecification.SwitchValueType = SwitchValueType;
		return switchSpecification;
	}

	public static Color GetColorByKey(string key)
	{
		if (key != null)
		{
			int hashCode = key.GetHashCode();
			Random random = new Random(hashCode);
			return new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1f);
		}
		return Color.White;
	}
}
