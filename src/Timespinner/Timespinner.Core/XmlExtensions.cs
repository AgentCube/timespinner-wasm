using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Timespinner.Core;

public static class XmlExtensions
{
	public static void AddAttribute(this XmlNode node, string name, string value)
	{
		XmlAttribute val = node.OwnerDocument.CreateAttribute(name);
		((XmlNode)val).Value = value;
		node.Attributes.Append(val);
	}

	public static void AddAttribute(this XmlNode node, string name, object value)
	{
		node.AddAttribute(name, value.ToString());
	}

	public static void AddAttributeNoDefault(this XmlNode node, string name, bool value)
	{
		if (value)
		{
			node.AddAttribute(name, true.ToString());
		}
	}

	public static void AddAttributeNoDefault<T>(this XmlNode node, string name, T value)
	{
		if (!EqualityComparer<T>.Default.Equals(value, default(T)))
		{
			node.AddAttribute(name, value.ToString());
		}
	}

	public static void AddAttributeNoDefault(this XmlNode node, string name, int value)
	{
		if (value != 0)
		{
			node.AddAttribute(name, value.ToString(CultureInfo.InvariantCulture));
		}
	}

	public static void AddAttributeNoDefault(this XmlNode node, string name, float value)
	{
		if (Math.Abs(value) > 1E-05f)
		{
			node.AddAttribute(name, value.ToString(CultureInfo.InvariantCulture));
		}
	}

	public static void AddAttributeNoDefault(this XmlNode node, string name, Point value)
	{
		if (value != Point.Zero)
		{
			node.AddAttribute(name, MathEx.PointToString(value));
		}
	}

	public static void AddAttributeNoDefault(this XmlNode node, string name, Vector2 value)
	{
		if (value != Vector2.Zero)
		{
			node.AddAttribute(name, MathEx.Vector2ToString(value));
		}
	}

	public static void AddAttributeNoDefault(this XmlNode node, string name, DateTime value)
	{
		if (!EqualityComparer<DateTime>.Default.Equals(value, default(DateTime)))
		{
			node.AddAttribute(name, value.ToString(CultureInfo.InvariantCulture));
		}
	}

	public static XmlNode AddElement(this XmlDocument document, string name)
	{
		XmlElement val = document.CreateElement(name);
		((XmlNode)document).AppendChild((XmlNode)(object)val);
		return (XmlNode)(object)val;
	}

	public static XmlNode AddElement(this XmlNode node, string name)
	{
		XmlElement val = node.OwnerDocument.CreateElement(name);
		node.AppendChild((XmlNode)(object)val);
		return (XmlNode)(object)val;
	}
}
