using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L10_Hangar;

internal sealed class BulwarkGemAppendage : Appendage
{
	private const int Anim_IndexPlasma = 6;

	private const int Anim_IndexChaos = 7;

	private const int Anim_IndexBlood = 8;

	private const int Anim_IndexEnergy = 9;

	private const int PlasmaAnchorX = -30;

	private const int PlasmaAnchorY = -129;

	private const int ChaosAnchorX = 35;

	private const int ChaosAnchorY = -96;

	private const int BloodAnchorX = -30;

	private const int BloodAnchorY = -17;

	private const float Energy1RotationSpeed = 40f;

	private const float Energy2RotationSpeed = 20f;

	private static readonly Color ActiveGemDrawColor = Color.White * 0.85f;

	private static readonly Color DeadGemDrawColor = Color.White * 0.65f;

	private static readonly Color BackgroundActiveGemDrawColor = new Color(128, 128, 128, 192);

	private static readonly Color BackgroundDeadGemDrawColor = new Color(96, 96, 96, 128);

	private static readonly Color Energy1DrawColor = Color.Blue * 0.5f;

	private static readonly Color Energy2DrawColor = Color.Red * 0.5f;

	private readonly bool _isDead;

	private readonly EBulwarkGemType _gemType;

	private readonly Appendage _energyAppendage1;

	private readonly Appendage _energyAppendage2;

	public BulwarkGemAppendage(Animate parent, Level inLevel, SpriteSheet inSprite, EBulwarkGemType gemType, bool isDead)
		: base(parent, new Point(29, 29), Point.Zero, inLevel, inSprite)
	{
		_gemType = gemType;
		_isDead = isDead;
		_doAppendagesInheritDrawColor = false;
		base.DoesInheritDrawColor = false;
		base.DrawPriority = 1;
		base.FollowType = EAppendageFollowType.AnchorLocked;
		if (!_isDead)
		{
			_energyAppendage1 = new Appendage(this, new Point(29, 29), Point.Zero, _level, _sprite)
			{
				DrawOrigin = new Vector2(14.5f, 14.5f),
				DrawPriority = -1,
				FollowType = EAppendageFollowType.AnchorLocked
			};
			_energyAppendage2 = new Appendage(this, new Point(29, 29), Point.Zero, _level, _sprite)
			{
				DrawOrigin = new Vector2(14.5f, 14.5f),
				DrawPriority = -1,
				FollowType = EAppendageFollowType.AnchorLocked
			};
			_energyAppendage1.ChangeAnimation(9);
			_energyAppendage2.ChangeAnimation(9);
			_appendages.Add(_energyAppendage1);
			_appendages.Add(_energyAppendage2);
		}
		switch (_gemType)
		{
		case EBulwarkGemType.Plasma:
			ChangeAnimation(6);
			base.AnchorOffset = new Point(-30, -129);
			break;
		case EBulwarkGemType.Chaos:
			ChangeAnimation(7);
			base.AnchorOffset = new Point(35, -96);
			break;
		case EBulwarkGemType.Blood:
			ChangeAnimation(8);
			base.AnchorOffset = new Point(-30, -17);
			break;
		}
		if (_isDead)
		{
			base.DrawColor = ((_gemType == EBulwarkGemType.Chaos) ? BackgroundDeadGemDrawColor : DeadGemDrawColor);
		}
		else
		{
			base.DrawColor = ((_gemType == EBulwarkGemType.Chaos) ? BackgroundActiveGemDrawColor : ActiveGemDrawColor);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isDead)
		{
			_energyAppendage1.Rotation += delta * 40f;
			if (_energyAppendage1.Rotation >= (float)Math.PI * 2f)
			{
				_energyAppendage1.Rotation -= (float)Math.PI * 2f;
			}
			_energyAppendage2.Rotation -= delta * 20f;
			if (_energyAppendage2.Rotation < 0f)
			{
				_energyAppendage2.Rotation += (float)Math.PI * 2f;
			}
			_energyAppendage1.DrawColor = Energy1DrawColor;
			_energyAppendage2.DrawColor = Energy2DrawColor;
		}
		base.Update(delta);
	}
}
