using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal class CursedSporeVineDamageArea : DamageArea
{
	private const int DamageHeight = 42;

	private const int DamageWidth = 32;

	private static readonly Point BottomVineOffset = new Point(0, 14);

	private static readonly Point TopVineOffset = new Point(0, -32);

	private static readonly Point LeftVineOffset = new Point(-20, -7);

	private static readonly Point RightVineOffset = new Point(20, -7);

	private readonly HashSet<int> _damagingFrames = new HashSet<int> { 22, 23, 27, 28 };

	private bool _isHorizontal;

	private int _lastVineFrame;

	private Appendage _currentVine;

	public CursedSporeVineDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile anchor, int damage)
		: base(inLevel, inPosition, inSide, -1, anchor)
	{
		_doesDrawSpriteAndAppendages = false;
		base.Power = damage;
		base.Life = 0f;
		base.DoesKnockBack = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _currentVine != null)
		{
			int num = _currentVine.AnimationIndex + _currentVine.AnimationStart;
			if (num != _lastVineFrame)
			{
				bool canDamageThings = _damagingFrames.Contains(num);
				base.CanDamageThings = canDamageThings;
			}
			_lastVineFrame = num;
		}
		base.Update(delta);
	}

	private void SetIsHorizontal(bool isHorizontal)
	{
		if (isHorizontal != _isHorizontal || base.DamageDimensions == Point.Zero)
		{
			_isHorizontal = isHorizontal;
			base.DamageDimensions = (_isHorizontal ? new Point(32, 42) : new Point(42, 32));
		}
	}

	public void Refresh(CursedSporeVine.ECursedVineType targetVine, bool areVinesIntact, Appendage targetVineObject)
	{
		_lastVineFrame = -1;
		_currentVine = targetVineObject;
		bool isHorizontal = targetVine != CursedSporeVine.ECursedVineType.Bottom;
		SetIsHorizontal(isHorizontal);
		switch (targetVine)
		{
		case CursedSporeVine.ECursedVineType.Bottom:
			base.AnchorOffset = BottomVineOffset;
			break;
		case CursedSporeVine.ECursedVineType.Top:
			base.AnchorOffset = TopVineOffset;
			break;
		case CursedSporeVine.ECursedVineType.Left:
			base.AnchorOffset = LeftVineOffset;
			break;
		case CursedSporeVine.ECursedVineType.Right:
			base.AnchorOffset = RightVineOffset;
			break;
		}
		base.Life = 100f;
		_isFading = false;
	}

	public void Sleep()
	{
		base.Life = 0f;
	}
}
