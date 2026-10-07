using System;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications;

[Serializable]
public class CharacterAction
{
	public bool DoesBlock { get; set; }

	public bool BoolArgument { get; set; }

	public ECharacterActionType ActionType { get; set; }

	public ECharacterActionInterpolationType InterpolationType { get; set; }

	public int AppendageTarget { get; set; }

	public int IntArgument { get; set; }

	public float FloatArgument { get; set; }

	public float Duration { get; set; }

	public Point PointArgument { get; set; }

	public Point PointArgument2 { get; set; }

	public Vector2 Vector2Argument { get; set; }

	public CharacterAction Duplicate()
	{
		CharacterAction characterAction = new CharacterAction();
		characterAction.BoolArgument = BoolArgument;
		characterAction.DoesBlock = DoesBlock;
		characterAction.ActionType = ActionType;
		characterAction.InterpolationType = InterpolationType;
		characterAction.AppendageTarget = AppendageTarget;
		characterAction.IntArgument = IntArgument;
		characterAction.Duration = Duration;
		characterAction.FloatArgument = FloatArgument;
		characterAction.PointArgument = PointArgument;
		characterAction.PointArgument2 = PointArgument2;
		characterAction.Vector2Argument = Vector2Argument;
		return characterAction;
	}
}
