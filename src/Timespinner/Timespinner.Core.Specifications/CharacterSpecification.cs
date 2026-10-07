using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Timespinner.Core.Specifications;

[Serializable]
public class CharacterSpecification
{
	public const string XmlCharacterSpecificationDatabaseNodeName = "CharacterSpecificationDatabase";

	public const string XmlCharacterSpecificationNodeName = "CharacterSpecification";

	public const string XmlAppendagesNodeName = "Appendages";

	public const string XmlAppendageNodeName = "Appendage";

	public const string XmlCoreAppendageNodeName = "CoreAppendage";

	public const string XmlChildrenNodeName = "Children";

	public const string XmlSequencesNodeName = "Sequences";

	public const string XmlSequenceNodeName = "Sequence";

	public const string XmlActionSetsNodeName = "ActionSets";

	public const string XmlActionSetNodeName = "ActionSet";

	public const string XmlActionsNodeName = "Actions";

	public const string XmlActionNodeName = "Action";

	public const string XmlNameAttribute = "Name";

	public const string XmlKeyAttribute = "Key";

	public const string XmlFollowingSequenceAttribute = "FollowingSequence";

	public const string XmlDefaultWait = "DefaultWait";

	public const string XmlIndexAttribute = "Index";

	public const string XmlBboxAttribute = "Bbox";

	public const string XmlBboxOffsetAttribute = "BboxOffset";

	public const string XmlAnchorOffsetAttribute = "AnchorOffset";

	public const string XmlFollowTypeAttribute = "FollowType";

	public const string XmlDrawPriorityAttribute = "DrawPriority";

	public const string XmlStartOffsetAttribute = "StartOffset";

	public const string XmlEndOffsetAttribute = "EndOffset";

	public const string XmlDoesIgnoreColorAttribute = "DoesIgnoreColor";

	public const string XmlDoesIgnoreCollision = "DoesIgnoreCollision";

	public const string XmlActionTypeAttribute = "ActionType";

	public const string XmlInterpolationTypeAttribute = "InterpType";

	public const string XmlAppendageTargetAttribute = "AppendageTarget";

	public const string XmlDoesBlockAttribute = "DoesBlock";

	public const string XmlDoesRepeatAttribute = "DoesRepeat";

	public const string XmlDoesRunConcurrently = "XmlDoesRunConcurrently";

	public const string XmlDurationAttribute = "Duration";

	public const string XmlRepeatCountAttribute = "RepeatCount";

	public const string XmlBoolArgumentAttribute = "BoolArgument";

	public const string XmlIntArgumentAttribute = "IntArgument";

	public const string XmlFloatArgumentAttribute = "FloatArgument";

	public const string XmlPointArgumentAttribute = "PointArgument";

	public const string XmlPointArgument2Attribute = "PointArgument2";

	public const string XmlVectorArgumentAttribute = "VectorArgument";

	public const string XmlDrawOriginAttribute = "DrawOrigin";

	public const string XmlAmplitudeAttribute = "Amplitude";

	public const string XmlDeltaAttribute = "Delta";

	public const string XmlFrequencyAttribute = "Frequency";

	public const string XmlIncrementAttribute = "Increment";

	public const string XmlSpeedAttribute = "Speed";

	public const string XmlIsFacingOppositeParentAttribute = "IsFacingOppositeParent";

	public const string XmlIsFlippedVerticallyAttribute = "IsFlippedVertically";

	public const string XmlIsFacingLockedAttribute = "IsFacingLocked";

	public string Name { get; set; }

	public string Key { get; set; }

	public CharacterAppendageSpecification CoreAppendage { get; set; }

	public List<CharacterSequenceSpecification> Sequences { get; set; }

	public CharacterSpecification()
	{
		Sequences = new List<CharacterSequenceSpecification>();
	}

	public static CharacterSpecification LoadXmlCharacterSpecification(XmlReader reader)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Invalid comparison between Unknown and I4
		bool flag = false;
		CharacterSpecification characterSpecification = new CharacterSpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Name":
				characterSpecification.Name = reader.Value;
				break;
			case "Key":
				characterSpecification.Key = reader.Value;
				break;
			}
		}
		CharacterSequenceSpecification characterSequenceSpecification = null;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType != 1)
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "CoreAppendage":
				characterSpecification.CoreAppendage = CharacterAppendageSpecification.FromXml(reader);
				break;
			case "Sequence":
				while ((int)reader.NodeType == 1 && reader.LocalName == "Sequence")
				{
					if (characterSequenceSpecification != null)
					{
						characterSpecification.Sequences.Add(characterSequenceSpecification);
					}
					characterSequenceSpecification = CharacterSequenceSpecification.FromXml(reader);
				}
				if (reader.LocalName == "CharacterSpecification")
				{
					flag = true;
				}
				break;
			default:
				flag = true;
				break;
			case "Appendages":
			case "Sequences":
				break;
			}
		}
		if (characterSequenceSpecification != null)
		{
			characterSpecification.Sequences.Add(characterSequenceSpecification);
		}
		return characterSpecification;
	}

	public static string KeyFromObjectSpecification(ObjectTileSpecification objectTileSpecification)
	{
		string arg = objectTileSpecification.Category switch
		{
			EObjectTileCategory.Enemy => objectTileSpecification.GetEnemyType().ToString(), 
			EObjectTileCategory.Event => objectTileSpecification.GetEventType().ToString(), 
			EObjectTileCategory.Item => objectTileSpecification.GetItemType().ToString(), 
			_ => objectTileSpecification.ID.ToString(CultureInfo.InvariantCulture), 
		};
		bool flag = objectTileSpecification.DoesUseArgumentForKey();
		return string.Format("{0}_{1}{2}", objectTileSpecification.Category, arg, (!flag || objectTileSpecification.Argument == 0) ? "" : ("_" + objectTileSpecification.Argument));
	}

	public static string KeyFromEnemyType(EEnemyTileType enemyType, int argument)
	{
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification();
		objectTileSpecification.Category = EObjectTileCategory.Enemy;
		objectTileSpecification.Argument = argument;
		objectTileSpecification.ObjectID = (int)enemyType;
		ObjectTileSpecification objectTileSpecification2 = objectTileSpecification;
		return KeyFromObjectSpecification(objectTileSpecification2);
	}
}
