using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisEmpireOrb : LunaisOrb
{
	private const int NearFistFrameIndex = 11;

	private const int FarFistFrameIndex = 10;

	private const int BaseBirdThrowStartOffsetX = -8;

	private const int BaseBirdThrowEndOffsetX = 24;

	private const int BaseBirdThrowOffsetY = 8;

	private const int OrbTrailLength = 75;

	private const float BirdFadeInTime = 0.1f;

	private const float BirdTimeBeforeFadingOut = 0.1f;

	private const float BirdFadeOutTime = 0.1f;

	private const float ThrowRadius = 42f;

	internal const float ThrowTimeToAttack = 0.25f;

	internal const float ThrowTimeToReturn = 0.2f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 FistAuraOffsetLeft = new Vector2(5f, 2f);

	private static readonly Vector2 FistAuraOffsetRight = new Vector2(2f, 2f);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color BaseFistDrawColor = Color.White * 0.8f;

	private static readonly Color FistAuraColor = new Color(0.35f, 0.2f, 0.45f, 0.25f);

	private readonly int _fistThrowOffsetY;

	private readonly int _fistThrowStartOffsetX;

	private readonly int _fistThrowEndOffsetX;

	private readonly Appendage _fistAppendage;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Empire;

	public LunaisEmpireOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		_doAppendagesInheritDrawColor = false;
		_fistAppendage = new Appendage(this, new Point(60, 20), new Point(2, 5), _level, _sprite)
		{
			DrawPriority = -1,
			DoesDrawAura = true,
			AuraOffset = FistAuraOffsetLeft,
			AuraColor = FistAuraColor,
			AuraFrequency = 9f,
			AuraSize = 0.1f,
			AuraCount = 5f
		};
		_fistAppendage.ChangeAnimation(isFrontPlane ? 11 : 10);
		_trailColor = base.IdleTrailColor;
		ChangeAnimation(0, 10, 0.05f, EAnimationType.Cycle);
		_fistThrowOffsetY = (base.IsMainOrb ? 6 : 8);
		_fistThrowStartOffsetX = (base.IsMainOrb ? (-4) : (-8));
		_fistThrowEndOffsetX = (base.IsMainOrb ? 28 : 24);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_doesDrawBaseSprite = false;
		_appendages.Clear();
		_appendages.Add(_fistAppendage);
		_fistAppendage.IsFacingLeft = base.IsThrowingLeft;
		_fistAppendage.AuraOffset = (base.IsThrowingLeft ? FistAuraOffsetLeft : FistAuraOffsetRight);
		_level.AddProjectile(new EmpireOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, _fistAppendage, base.OrbDamage, this, _sprite));
		PlayCue(ESFX.LunaisOrbEmpireMelee);
		UpdateThrow(0f);
		_fistAppendage.Update(0f);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateThrowAttack(delta, 0.25f, 0.2f, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		base.IsOverridingDrawPlane = true;
		base.IsDrawingOnFrontPlaneOverride = !base.IsMainOrb;
		Vector2 result;
		if (_currentThrowTime > 0.45f)
		{
			base.State = EOrbState.Idle;
			result = _baseOrbitPosition.ToVector2();
			base.Rotation = 0f;
			_appendages.Clear();
			base.IsOverridingDrawPlane = false;
		}
		else
		{
			if (_currentThrowTime >= 0.25f && currentThrowTime < 0.25f)
			{
				base.IsAtAttackApex = true;
				_doesDrawBaseSprite = true;
			}
			float num = ((_currentThrowTime <= 0.25f) ? (_currentThrowTime / 0.5f) : (0.5f + (_currentThrowTime - 0.25f) / 0.4f));
			float num2 = 42f * (float)Math.Sin((double)num * Math.PI);
			_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
			result = new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
			float num3 = 1f;
			int num4;
			if (_currentThrowTime < 0.25f)
			{
				num4 = (int)MathEx.SineInterpolate(_fistThrowStartOffsetX, _fistThrowEndOffsetX, _currentThrowTime / 0.25f);
				if (_currentThrowTime < 0.1f)
				{
					num3 = _currentThrowTime / 0.1f;
				}
			}
			else
			{
				num4 = _fistThrowEndOffsetX;
				float num5 = _currentThrowTime - 0.25f;
				if (num5 > 0.1f)
				{
					num5 -= 0.1f;
					float num6 = ((num5 > 0.1f) ? 0f : (1f - num5 / 0.1f));
					num3 = num6;
				}
			}
			_fistAppendage.Position = base.CurrentTarget.Add(base.IsThrowingLeft ? (-num4) : num4, _fistThrowOffsetY);
			_fistAppendage.DrawColor = BaseFistDrawColor * num3;
			_fistAppendage.AuraColor = FistAuraColor * num3;
		}
		return result;
	}
}
