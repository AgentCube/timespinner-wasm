using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class EmpireOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	private const int BallAnimationFrameIndex = 15;

	private const int HeadAnimationFrameIndex = 16;

	private const int BallBoundingSize = 32;

	private const int BallBoundingOffset = 6;

	private const int HeadBoundingSize = 26;

	private const int HeadBoundingOffsetX = 38;

	private const int HeadBoundingOffsetY = 16;

	private const int OscillationHeight = 32;

	private const int BallFollowOffsetY = -12;

	private const int BallCount = 24;

	private const float TravelFrequency = 9f;

	private const float TravelSpeed = 32f;

	private const float BallTimeOffset = 0.5f;

	private const float MaxLife = 2f;

	private static readonly Color BaseDrawColor = Color.White * 0.8f;

	private static readonly Color BaseAuraColor = new Color(0.35f, 0.2f, 0.45f, 0.25f);

	private bool _isCastingLeft;

	private bool _isUpsideDown;

	private float _travelTimer;

	private Point _snekStart;

	public EmpireOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell, bool isCastingLeft, bool isUpsideDown)
		: base(inLevel, inPosition, inSide, -1, null, parentSpell)
	{
		_sprite = _level.GCM.SpOrbMeleeEmpire;
		ChangeAnimation(16);
		_bboxOffset = new Point(38, 16);
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 26, 26);
		DrawOrigin = new Vector2(52f, 28.5f);
		_doesRotateBasedOnVelocity = false;
		_power = baseDamage;
		_force = 4;
		_life = 2f;
		base.DamageTimeoutTime = 0.1f;
		_damageElement = EDamageElement.Aura;
		base.DrawColor = BaseDrawColor;
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		base.AuraFrequency = 9f;
		base.AuraSize = 0.1f;
		base.AuraCount = 5f;
		_doAppendagesInheritDrawColor = true;
		_doesUseAppendageCollision = true;
		base.DoesDrawAppendageAuras = true;
		Point bboxDimensions = new Point(32, 32);
		Point inBboxOffset = new Point(6, 6);
		for (int i = 0; i < 24; i++)
		{
			Appendage appendage = new Appendage(this, bboxDimensions, inBboxOffset, _level, _sprite)
			{
				DrawOrigin = new Vector2(24f, 24f)
			};
			appendage.ChangeAnimation(15);
			base.Appendages.Add(appendage);
		}
		Reset(inPosition, isCastingLeft, baseDamage, isUpsideDown);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_travelTimer += delta * 9f;
			float num = (float)Math.Sin(_travelTimer);
			float num2 = (float)Math.Cos(_travelTimer);
			int num3 = ((!_isCastingLeft) ? 1 : (-1));
			base.Rotation = (float)Math.Atan2(num2 * (float)((!_isUpsideDown) ? 1 : (-1)), -num3);
			int num4 = -(int)(num * 32f);
			int num5 = (int)(_travelTimer * 32f);
			if (_isUpsideDown)
			{
				num4 = -num4;
			}
			Position = new Point(_snekStart.X + (_isCastingLeft ? (-num5) : num5), _snekStart.Y + num4 + -12);
			SnapBboxToPosition();
			float num6 = _travelTimer - 0.5f;
			foreach (Appendage appendage in base.Appendages)
			{
				if (num6 < 0f)
				{
					appendage.Position = _snekStart;
					appendage.Rotation = num6;
					appendage.DoesDrawBaseSprite = false;
				}
				else
				{
					appendage.DoesDrawBaseSprite = true;
					num = (float)Math.Sin(num6);
					num2 = (float)Math.Cos(num6);
					appendage.Rotation = (float)Math.Atan2(num2 * (float)((!_isUpsideDown) ? 1 : (-1)), -num3);
					num4 = -(int)(num * 32f);
					num5 = (int)(num6 * 32f);
					if (_isUpsideDown)
					{
						num4 = -num4;
					}
					appendage.Position = new Point(_snekStart.X + (_isCastingLeft ? (-num5) : num5), _snekStart.Y + num4);
				}
				appendage.SnapBboxToPosition();
				num6 -= 0.5f;
			}
		}
		base.Update(delta);
		if (_isFading && _timeToFade > 0f)
		{
			float num7 = 1f - _fadeTimer / _timeToFade;
			base.DrawColor = Color.White * 0.5f * num7;
			base.AuraColor = BaseAuraColor * num7;
			{
				foreach (Appendage appendage2 in base.Appendages)
				{
					appendage2.AuraColor = base.AuraColor;
					appendage2.DrawColor = base.DrawColor;
				}
				return;
			}
		}
		base.DrawColor = Color.White * 0.5f;
	}

	internal void Reset(Point startPoint, bool isCastingLeft, int baseDamage, bool isUpsideDown)
	{
		_isFading = false;
		_fadeTimer = 0f;
		_life = 2f;
		_power = baseDamage;
		_travelTimer = 0f;
		Position = startPoint;
		_snekStart = startPoint;
		_isCastingLeft = isCastingLeft;
		_isUpsideDown = isUpsideDown;
		Update(0f);
		base.AuraColor = BaseAuraColor;
		foreach (Appendage appendage in base.Appendages)
		{
			appendage.SnapFrameToBbox();
			appendage.RefreshDrawPos();
			appendage.AuraColor = BaseAuraColor;
		}
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			TeamSide = _teamSide,
			AnimationStart = 12,
			AnimationLength = 3,
			DrawColor = Color.White * 0.75f
		});
		_level.PlayCue(ESFX.LunaisOrbImpact, intersectionCenter);
	}
}
