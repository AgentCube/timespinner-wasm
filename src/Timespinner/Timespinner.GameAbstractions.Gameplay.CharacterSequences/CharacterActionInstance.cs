using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Gameplay.CharacterSequences;

internal class CharacterActionInstance
{
	private readonly CharacterAction _specification;

	private readonly Animate _target;

	private bool _hasStarted;

	private float _timer;

	private float _duration;

	private float _startFloat;

	private Point _startPoint;

	private Point _startPoint2;

	private Color _startColor;

	internal bool IsFinished { get; private set; }

	internal bool DoesBlock { get; private set; }

	internal CharacterActionInstance(CharacterAction action, Animate target)
	{
		_specification = action;
		DoesBlock = _specification.DoesBlock || _specification.ActionType == ECharacterActionType.Wait;
		_duration = _specification.Duration;
		bool flag = true;
		if (_specification.AppendageTarget > 0)
		{
			Animate appendageByNumber = GetAppendageByNumber(_specification.AppendageTarget, target);
			if (appendageByNumber != null)
			{
				_target = appendageByNumber;
				flag = false;
			}
		}
		if (flag)
		{
			_target = target;
		}
	}

	private static Animate GetAppendageByNumber(int number, Animate target)
	{
		int num = number;
		Animate animate = target;
		Stack<int> stack = new Stack<int>();
		while (num >= 64)
		{
			stack.Push(num & 0x3F);
			num >>= 6;
		}
		stack.Push(num - 1);
		while (stack.Count > 0)
		{
			int num2 = stack.Pop();
			if (num2 < animate.Appendages.Count)
			{
				animate = animate.Appendages[num2];
			}
		}
		return animate;
	}

	internal void Update(float delta)
	{
		_timer += delta;
		switch (_specification.ActionType)
		{
		case ECharacterActionType.ChangeAnimation:
			if (!_hasStarted)
			{
				int x = _specification.PointArgument.X;
				int y = _specification.PointArgument.Y;
				float floatArgument = _specification.FloatArgument;
				EAnimationType intArgument = (EAnimationType)_specification.IntArgument;
				bool boolArgument = _specification.BoolArgument;
				_target.ChangeAnimation(new AnimationSpec
				{
					Start = x,
					Length = y,
					Speed = floatArgument,
					Type = intArgument,
					IsInReverse = boolArgument
				});
				if (_specification.DoesBlock && _duration == 0f)
				{
					_duration = (float)y * floatArgument;
				}
			}
			break;
		case ECharacterActionType.ChangeBbox:
			ChangeBbox();
			break;
		case ECharacterActionType.ChangeAnchorOffset:
			ChangeAnchorOffset();
			break;
		case ECharacterActionType.ChangeDrawOrigin:
			_target.DrawOrigin = _specification.Vector2Argument;
			break;
		case ECharacterActionType.ChangeIsFlipped:
			_target.IsFacingLeft = _specification.BoolArgument;
			break;
		case ECharacterActionType.ChangeIsFlippedVertically:
			_target.IsFlippedVertically = _specification.BoolArgument;
			break;
		case ECharacterActionType.ChangePosition:
			_target.CollisionSetPosition(_specification.PointArgument, _target);
			break;
		case ECharacterActionType.ChangeRotation:
			ChangeRotation();
			break;
		case ECharacterActionType.ChangeDrawColor:
			ChangeColor();
			break;
		case ECharacterActionType.ChangeScale:
			ChangeScale();
			break;
		case ECharacterActionType.ChangeDrawPriority:
			if (_target is Appendage appendage)
			{
				appendage.DrawPriority = _specification.IntArgument;
			}
			break;
		case ECharacterActionType.TriggerAction:
			_target.TriggerCharacterAction(_specification);
			break;
		}
		_hasStarted = true;
		if (_timer >= _duration)
		{
			IsFinished = true;
		}
	}

	private void ChangeBbox()
	{
		Point point = _specification.PointArgument;
		Point point2 = _specification.PointArgument2;
		if (_duration > 0f && _timer < _duration)
		{
			if (!_hasStarted)
			{
				_startPoint = new Point(_target.Bbox.Width, _target.Bbox.Height);
				_startPoint2 = new Point(_target.BboxOffset.X, _target.BboxOffset.Y);
			}
			point = GetInterpolation(_startPoint, point);
			point2 = GetInterpolation(_startPoint2, point2);
		}
		_target.ChangeBboxDimensions(point, point2);
	}

	private void ChangeAnchorOffset()
	{
		Point point = _specification.PointArgument;
		if (_duration > 0f && _timer < _duration)
		{
			if (!_hasStarted)
			{
				_startPoint = _target.GetAnchorOffset();
			}
			point = GetInterpolation(_startPoint, point);
		}
		_target.ChangeAnchorOffset(point);
	}

	private void ChangeRotation()
	{
		float num = _specification.Vector2Argument.X;
		if (_duration > 0f && _timer < _duration)
		{
			if (!_hasStarted)
			{
				_startFloat = _target.Rotation;
			}
			num = GetInterpolation(_startFloat, num);
		}
		_target.Rotation = num;
	}

	private void ChangeColor()
	{
		Color color = new Color(_specification.PointArgument.X, _specification.PointArgument.Y, _specification.PointArgument2.X, _specification.PointArgument2.Y);
		if (_duration > 0f && _timer < _duration)
		{
			if (!_hasStarted)
			{
				_startColor = _target.DrawColor;
			}
			color = GetInterpolation(_startColor, color);
		}
		_target.DrawColor = color;
	}

	private void ChangeScale()
	{
		float num = _specification.Vector2Argument.X;
		if (_duration > 0f && _timer < _duration)
		{
			if (!_hasStarted)
			{
				_startFloat = _target.Scale;
			}
			num = GetInterpolation(_startFloat, num);
		}
		_target.Scale = num;
	}

	private float GetInterpolation(float start, float end)
	{
		float result = end;
		float num = _timer / _duration;
		switch (_specification.InterpolationType)
		{
		case ECharacterActionInterpolationType.Linear:
			result = MathHelper.Lerp(start, end, num);
			break;
		case ECharacterActionInterpolationType.Sine:
			result = MathEx.SineInterpolate(start, end, num);
			break;
		case ECharacterActionInterpolationType.Cos:
			result = MathEx.CosInterpolate(start, end, num);
			break;
		case ECharacterActionInterpolationType.SineArc:
			result = MathEx.SineInterpolate(start, end, num);
			break;
		case ECharacterActionInterpolationType.CosArc:
			result = MathEx.CosInterpolate(start, end, num);
			break;
		}
		return result;
	}

	private Point GetInterpolation(Point start, Point end)
	{
		Point result = end;
		float amount = _timer / _duration;
		switch (_specification.InterpolationType)
		{
		case ECharacterActionInterpolationType.Linear:
			result = start.Lerp(end, amount);
			break;
		case ECharacterActionInterpolationType.Sine:
			result = start.SineInterpolate(end, amount);
			break;
		case ECharacterActionInterpolationType.Cos:
			result = start.CosInterpolate(end, amount);
			break;
		case ECharacterActionInterpolationType.SineArc:
			result = start.SineInterpolate(end, amount);
			break;
		case ECharacterActionInterpolationType.CosArc:
			result = start.CosInterpolate(end, amount);
			break;
		}
		return result;
	}

	private Color GetInterpolation(Color start, Color end)
	{
		Color result = end;
		float amount = _timer / _duration;
		switch (_specification.InterpolationType)
		{
		case ECharacterActionInterpolationType.Linear:
			result = start.Lerp(end, amount);
			break;
		case ECharacterActionInterpolationType.Sine:
			result = start.SineInterpolate(end, amount);
			break;
		case ECharacterActionInterpolationType.Cos:
			result = start.CosInterpolate(end, amount);
			break;
		case ECharacterActionInterpolationType.SineArc:
			result = start.SineInterpolate(end, amount);
			break;
		case ECharacterActionInterpolationType.CosArc:
			result = start.CosInterpolate(end, amount);
			break;
		}
		return result;
	}

	internal void ForceEnd()
	{
		_timer = _duration;
		Update(0f);
		IsFinished = true;
	}
}
