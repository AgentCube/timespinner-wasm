using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.GameObjects;

namespace Timespinner.Core.Specifications;

[Serializable]
public class CharacterAppendageSpecification
{
	public EAppendageFollowType FollowType { get; set; }

	public bool IsFacingOppositeParent { get; set; }

	public bool IsFlippedVertically { get; set; }

	public bool IsFacingLocked { get; set; }

	public bool DoesIgnoreParentDrawColor { get; set; }

	public bool DoesIgnoreCollision { get; set; }

	public int AnimationIndex { get; set; }

	public int DrawPriority { get; set; }

	public float OscillAmplitude { get; set; }

	public float OscillDelta { get; set; }

	public float OscillFrequency { get; set; }

	public float OscillSpeed { get; set; }

	public float OscillIncrement { get; set; }

	public string Name { get; set; }

	public Point BboxDimensions { get; set; }

	public Point BboxOffset { get; set; }

	public Point AnchorOffset { get; set; }

	public Point StartOffset { get; set; }

	public Point EndOffset { get; set; }

	public Vector2 DrawOrigin { get; set; }

	public List<CharacterAppendageSpecification> Children { get; set; }

	public CharacterAppendageSpecification()
	{
		Children = new List<CharacterAppendageSpecification>();
	}

	public CharacterAppendageSpecification Duplicate()
	{
		CharacterAppendageSpecification characterAppendageSpecification = new CharacterAppendageSpecification();
		characterAppendageSpecification.IsFacingOppositeParent = IsFacingOppositeParent;
		characterAppendageSpecification.IsFacingLocked = IsFacingLocked;
		characterAppendageSpecification.IsFlippedVertically = IsFlippedVertically;
		characterAppendageSpecification.DoesIgnoreParentDrawColor = DoesIgnoreParentDrawColor;
		characterAppendageSpecification.DoesIgnoreCollision = DoesIgnoreCollision;
		characterAppendageSpecification.FollowType = FollowType;
		characterAppendageSpecification.AnimationIndex = AnimationIndex;
		characterAppendageSpecification.DrawPriority = DrawPriority;
		characterAppendageSpecification.Name = Name;
		characterAppendageSpecification.BboxDimensions = BboxDimensions;
		characterAppendageSpecification.BboxOffset = BboxOffset;
		characterAppendageSpecification.AnchorOffset = AnchorOffset;
		characterAppendageSpecification.StartOffset = StartOffset;
		characterAppendageSpecification.EndOffset = EndOffset;
		characterAppendageSpecification.DrawOrigin = DrawOrigin;
		characterAppendageSpecification.OscillAmplitude = OscillAmplitude;
		characterAppendageSpecification.OscillDelta = OscillDelta;
		characterAppendageSpecification.OscillFrequency = OscillFrequency;
		characterAppendageSpecification.OscillIncrement = OscillIncrement;
		characterAppendageSpecification.OscillSpeed = OscillSpeed;
		CharacterAppendageSpecification characterAppendageSpecification2 = characterAppendageSpecification;
		foreach (CharacterAppendageSpecification child in Children)
		{
			characterAppendageSpecification2.Children.Add(child.Duplicate());
		}
		return characterAppendageSpecification2;
	}

	public static CharacterAppendageSpecification FromXml(XmlReader reader)
	{
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Invalid comparison between Unknown and I4
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Invalid comparison between Unknown and I4
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Invalid comparison between Unknown and I4
		CharacterAppendageSpecification characterAppendageSpecification = new CharacterAppendageSpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Name":
				characterAppendageSpecification.Name = reader.Value;
				break;
			case "Index":
				characterAppendageSpecification.AnimationIndex = reader.Value.ParseInt32();
				break;
			case "Bbox":
				characterAppendageSpecification.BboxDimensions = MathEx.ParsePoint(reader.Value);
				break;
			case "BboxOffset":
				characterAppendageSpecification.BboxOffset = MathEx.ParsePoint(reader.Value);
				break;
			case "AnchorOffset":
				characterAppendageSpecification.AnchorOffset = MathEx.ParsePoint(reader.Value);
				break;
			case "FollowType":
				characterAppendageSpecification.FollowType = EnumExtensions.EnumParse<EAppendageFollowType>(reader.Value);
				break;
			case "DrawPriority":
				characterAppendageSpecification.DrawPriority = reader.Value.ParseInt32();
				break;
			case "StartOffset":
				characterAppendageSpecification.StartOffset = MathEx.ParsePoint(reader.Value);
				break;
			case "EndOffset":
				characterAppendageSpecification.EndOffset = MathEx.ParsePoint(reader.Value);
				break;
			case "DrawOrigin":
				characterAppendageSpecification.DrawOrigin = MathEx.ParseVector2(reader.Value);
				break;
			case "IsFacingOppositeParent":
				characterAppendageSpecification.IsFacingOppositeParent = bool.Parse(reader.Value);
				break;
			case "IsFlippedVertically":
				characterAppendageSpecification.IsFlippedVertically = bool.Parse(reader.Value);
				break;
			case "IsFacingLocked":
				characterAppendageSpecification.IsFacingLocked = bool.Parse(reader.Value);
				break;
			case "DoesIgnoreColor":
				characterAppendageSpecification.DoesIgnoreParentDrawColor = bool.Parse(reader.Value);
				break;
			case "DoesIgnoreCollision":
				characterAppendageSpecification.DoesIgnoreCollision = bool.Parse(reader.Value);
				break;
			case "Amplitude":
				characterAppendageSpecification.OscillAmplitude = reader.Value.ParseFloat();
				break;
			case "Delta":
				characterAppendageSpecification.OscillDelta = reader.Value.ParseFloat();
				break;
			case "Frequency":
				characterAppendageSpecification.OscillFrequency = reader.Value.ParseFloat();
				break;
			case "Increment":
				characterAppendageSpecification.OscillIncrement = reader.Value.ParseFloat();
				break;
			case "Speed":
				characterAppendageSpecification.OscillSpeed = reader.Value.ParseFloat();
				break;
			}
		}
		CharacterAppendageSpecification characterAppendageSpecification2 = null;
		bool flag = false;
		bool flag2 = false;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType == 1)
			{
				switch (reader.LocalName)
				{
				case "Children":
					flag2 = true;
					break;
				case "Appendage":
					if (!flag2)
					{
						flag = true;
						break;
					}
					while ((int)reader.NodeType == 1 && reader.LocalName == "Appendage")
					{
						if (characterAppendageSpecification2 != null)
						{
							characterAppendageSpecification.Children.Add(characterAppendageSpecification2);
						}
						characterAppendageSpecification2 = FromXml(reader);
					}
					if (reader.LocalName == "CharacterSpecification" || reader.LocalName == "Sequences")
					{
						flag = true;
					}
					break;
				case "Sequences":
					flag = true;
					break;
				case "CharacterSpecification":
					flag = true;
					break;
				case "CoreAppendage":
					flag = true;
					break;
				default:
					flag = true;
					break;
				}
			}
			else if ((int)reader.NodeType == 15 && ((reader.LocalName == "Children" && !flag2) || reader.LocalName == "Appendage"))
			{
				flag = true;
			}
		}
		if (characterAppendageSpecification2 != null)
		{
			characterAppendageSpecification.Children.Add(characterAppendageSpecification2);
		}
		return characterAppendageSpecification;
	}

	public static CharacterAppendageSpecification DefaultCoreAppendage()
	{
		CharacterAppendageSpecification characterAppendageSpecification = new CharacterAppendageSpecification();
		characterAppendageSpecification.Name = "Core";
		return characterAppendageSpecification;
	}

	public static bool IsDefaultCoreAppendage(CharacterAppendageSpecification spec)
	{
		if (spec != null && spec.Children.Count == 0 && spec.Name == "Core" && spec.BboxDimensions == Point.Zero)
		{
			return spec.BboxOffset == Point.Zero;
		}
		return false;
	}
}
