using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorEmpireMeleeDamageArea : DamageArea
{
	private const int DamageWidth = 60;

	private const int DamageHeight = 20;

	private const float MaxLife = 0.25f;

	private const int ParentMovementMultiplier = 10;

	private const int BaseBirdThrowStartOffsetX = -8;

	private const int BaseBirdThrowEndOffsetX = 32;

	private const int BaseBirdThrowOffsetY = 14;

	private const float BirdFadeInTime = 0.1f;

	private const float BirdTimeBeforeFadingOut = 0.1f;

	private const float BirdFadeOutTime = 0.1f;

	internal const float ThrowTimeToAttack = 0.25f;

	internal const float ThrowTimeToReturn = 0.2f;

	private static readonly Color BaseBirdDrawColor = Color.White * 0.8f;

	private static readonly Color BirdAuraColor = new Color(0.35f, 0.2f, 0.45f, 0.25f);

	private readonly bool _isThrowingLeft;

	private readonly int _birdThrowEndOffsetX;

	private readonly Point _throwStartPoint;

	private float _currentThrowTime;

	public EmperorEmpireMeleeDamageArea(Level inLevel, Point inPosition, Mobile inAnchor, bool isThrowingLeft, int parentChangeInX, int baseDamage, bool isFront)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, inAnchor)
	{
		_throwStartPoint = inPosition;
		_isThrowingLeft = isThrowingLeft;
		IsFacingLeft = _isThrowingLeft;
		_bboxOffset = new Point(0, 6);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 60, 20);
		SnapBboxToPosition();
		_birdThrowEndOffsetX = Math.Abs(parentChangeInX * 10) + 32;
		_sprite = _level.GCM.SpOrbMeleeEmpire;
		ChangeAnimation(isFront ? 10 : 11);
		base.DoesDrawAura = true;
		base.AuraOffset = new Vector2(6f, 1f);
		base.AuraColor = BirdAuraColor;
		base.AuraFrequency = 9f;
		base.AuraSize = 0.15f;
		base.AuraCount = 5f;
		base.Power = (int)Math.Ceiling((float)baseDamage * 1.15f);
		_force = 1;
		_life = 0.25f;
		base.DoesKnockBack = true;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Aura;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_currentThrowTime += delta;
			float num = 1f;
			int num2;
			if (_currentThrowTime < 0.25f)
			{
				num2 = (int)MathEx.SineInterpolate(-8f, _birdThrowEndOffsetX, _currentThrowTime / 0.25f);
				if (_currentThrowTime < 0.1f)
				{
					num = _currentThrowTime / 0.1f;
				}
			}
			else
			{
				num2 = _birdThrowEndOffsetX;
				float num3 = _currentThrowTime - 0.25f;
				base.Power = 0f;
				if (num3 > 0.1f)
				{
					num3 -= 0.1f;
					float num4 = ((num3 > 0.1f) ? 0f : (1f - num3 / 0.1f));
					num = num4;
				}
			}
			Position = _throwStartPoint.Add(_isThrowingLeft ? (-num2) : num2, 14);
			base.DrawColor = BaseBirdDrawColor * num;
			base.AuraColor = BirdAuraColor * num;
		}
		base.Update(delta);
	}
}
