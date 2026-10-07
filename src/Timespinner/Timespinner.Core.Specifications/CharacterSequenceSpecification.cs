using System;
using System.Collections.Generic;
using System.Xml;

namespace Timespinner.Core.Specifications;

[Serializable]
public class CharacterSequenceSpecification
{
	public bool DoesRepeat { get; set; }

	public bool DoesRunConcurrently { get; set; }

	public int RepeatCount { get; set; }

	public int FollowingSequenceIndex { get; set; }

	public float DefaultWait { get; set; }

	public string Name { get; set; }

	public List<CharacterActionSet> ActionSets { get; set; }

	public CharacterSequenceSpecification()
	{
		ActionSets = new List<CharacterActionSet>();
	}

	public static CharacterSequenceSpecification FromXml(XmlReader reader)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Invalid comparison between Unknown and I4
		CharacterSequenceSpecification characterSequenceSpecification = new CharacterSequenceSpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Name":
				characterSequenceSpecification.Name = reader.Value;
				break;
			case "DoesRepeat":
				characterSequenceSpecification.DoesRepeat = bool.Parse(reader.Value);
				break;
			case "XmlDoesRunConcurrently":
				characterSequenceSpecification.DoesRunConcurrently = bool.Parse(reader.Value);
				break;
			case "RepeatCount":
				characterSequenceSpecification.RepeatCount = reader.Value.ParseInt32();
				break;
			case "FollowingSequence":
				characterSequenceSpecification.FollowingSequenceIndex = reader.Value.ParseInt32();
				break;
			case "DefaultWait":
				characterSequenceSpecification.DefaultWait = reader.Value.ParseFloat();
				break;
			}
		}
		CharacterAction characterAction = null;
		CharacterActionSet characterActionSet = null;
		bool flag = false;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType != 1)
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "ActionSet":
				if (characterActionSet != null)
				{
					if (characterAction != null)
					{
						characterActionSet.Actions.Add(characterAction);
						characterAction = null;
					}
					characterSequenceSpecification.ActionSets.Add(characterActionSet);
				}
				characterActionSet = new CharacterActionSet();
				while (reader.MoveToNextAttribute())
				{
					switch (reader.Name)
					{
					case "DoesRepeat":
						characterActionSet.DoesRepeat = bool.Parse(reader.Value);
						break;
					case "RepeatCount":
						characterActionSet.RepeatCount = reader.Value.ParseInt32();
						break;
					}
				}
				break;
			case "Action":
				if (characterAction != null)
				{
					characterActionSet?.Actions.Add(characterAction);
				}
				characterAction = new CharacterAction();
				while (reader.MoveToNextAttribute())
				{
					switch (reader.Name)
					{
					case "DoesBlock":
						characterAction.DoesBlock = bool.Parse(reader.Value);
						break;
					case "ActionType":
						characterAction.ActionType = EnumExtensions.EnumParse<ECharacterActionType>(reader.Value);
						break;
					case "InterpType":
						characterAction.InterpolationType = EnumExtensions.EnumParse<ECharacterActionInterpolationType>(reader.Value);
						break;
					case "AppendageTarget":
						characterAction.AppendageTarget = reader.Value.ParseInt32();
						break;
					case "BoolArgument":
						characterAction.BoolArgument = bool.Parse(reader.Value);
						break;
					case "IntArgument":
						characterAction.IntArgument = reader.Value.ParseInt32();
						break;
					case "FloatArgument":
						characterAction.FloatArgument = reader.Value.ParseFloat();
						break;
					case "Duration":
						characterAction.Duration = reader.Value.ParseFloat();
						break;
					case "PointArgument":
						characterAction.PointArgument = MathEx.ParsePoint(reader.Value);
						break;
					case "PointArgument2":
						characterAction.PointArgument2 = MathEx.ParsePoint(reader.Value);
						break;
					case "VectorArgument":
						characterAction.Vector2Argument = MathEx.ParseVector2(reader.Value);
						break;
					}
				}
				break;
			case "CharacterSpecification":
				flag = true;
				break;
			default:
				flag = true;
				break;
			case "ActionSets":
			case "Actions":
				break;
			}
		}
		if (characterActionSet != null)
		{
			if (characterAction != null)
			{
				characterActionSet.Actions.Add(characterAction);
			}
			characterSequenceSpecification.ActionSets.Add(characterActionSet);
		}
		return characterSequenceSpecification;
	}

	public CharacterSequenceSpecification Duplicate()
	{
		CharacterSequenceSpecification characterSequenceSpecification = new CharacterSequenceSpecification();
		characterSequenceSpecification.Name = Name;
		characterSequenceSpecification.DoesRepeat = DoesRepeat;
		characterSequenceSpecification.DoesRunConcurrently = DoesRunConcurrently;
		characterSequenceSpecification.RepeatCount = RepeatCount;
		characterSequenceSpecification.FollowingSequenceIndex = FollowingSequenceIndex;
		characterSequenceSpecification.DefaultWait = DefaultWait;
		CharacterSequenceSpecification characterSequenceSpecification2 = characterSequenceSpecification;
		foreach (CharacterActionSet actionSet in ActionSets)
		{
			characterSequenceSpecification2.ActionSets.Add(actionSet.Duplicate());
		}
		return characterSequenceSpecification2;
	}

	public float EstimateDuration()
	{
		float num = 0f;
		foreach (CharacterActionSet actionSet in ActionSets)
		{
			foreach (CharacterAction action in actionSet.Actions)
			{
				if (action.DoesBlock || action.ActionType == ECharacterActionType.Wait)
				{
					num = ((action.ActionType != ECharacterActionType.ChangeAnimation) ? (num + action.Duration) : (num + action.FloatArgument * (float)action.IntArgument));
				}
			}
			if (DefaultWait > 0f)
			{
				num += DefaultWait;
			}
		}
		return num;
	}
}
