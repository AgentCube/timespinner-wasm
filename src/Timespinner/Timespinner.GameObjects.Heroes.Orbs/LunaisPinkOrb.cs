using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisPinkOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const int ShockSearchStartOffsetX = 100;

	private const int DefaultShockWidth = 160;

	private const int MaxDistance = 160;

	private const int MaxDistanceSquared = 25600;

	private const float ThrowTimeToAttack = 0.07f;

	private const float ThrowTimeToReturn = 0.5f;

	private const float ThrowRadius = 48f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color WhiteTrailColor = new Color(0.95f, 0.95f, 1f, 0.8f);

	private readonly LunaisObj _parentLunais;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Pink;

	public LunaisPinkOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_parentLunais = parentLunais;
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		ClearTrailHistory();
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(29, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_trailColor = WhiteTrailColor;
		PlayCue(ESFX.LunaisOrbPlasmaWhiff);
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateThrowAttack(delta, 0.07f, 0.5f, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		Vector2 result;
		if (_currentThrowTime > 0.57f)
		{
			base.State = EOrbState.Idle;
			result = _baseOrbitPosition.ToVector2();
			base.Rotation = 0f;
		}
		else
		{
			if (_currentThrowTime >= 0.07f && currentThrowTime < 0.07f)
			{
				base.IsAtAttackApex = true;
				ThrowThunderbolt();
			}
			float num = ((_currentThrowTime <= 0.07f) ? (_currentThrowTime / 0.14f) : (0.5f + (_currentThrowTime - 0.07f) / 1f));
			_trailColor = ((_currentThrowTime <= 0.17f) ? WhiteTrailColor : base.IdleTrailColor);
			float num2 = 48f * (float)Math.Sin((double)num * Math.PI);
			_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
			result = new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
		}
		return result;
	}

	private void ThrowThunderbolt()
	{
		if (_parentLunais.Aura < 3)
		{
			return;
		}
		Point point = new Point(Position.X, Position.Y);
		Monster nearestVisibleEnemy = _level.GetNearestVisibleEnemy(new Point(point.X + 100 * ((!base.IsThrowingLeft) ? 1 : (-1)), point.Y));
		Point end = new Point(point.X + (base.IsThrowingLeft ? (-160) : 160), point.Y);
		if (nearestVisibleEnemy != null && nearestVisibleEnemy.CanBeDamaged && nearestVisibleEnemy.Bbox.X > point.X != base.IsThrowingLeft)
		{
			Point center = nearestVisibleEnemy.OuterBbox.Center;
			int num = point.DistanceSquared(center);
			if (num <= 25600)
			{
				end = center;
			}
		}
		List<Vector4> intervalsBetween = ThunderBoltDamageArea.GetIntervalsBetween(point, end, base.Level);
		ThunderBoltDamageArea thunderBoltDamageArea = new ThunderBoltDamageArea(_level, Bbox.Center, ETeamSide.Heroes, base.OrbDamage, intervalsBetween, base.IsThrowingLeft, this, EThunderBoltType.Lunais);
		thunderBoltDamageArea.AddEndPointAnimation(base.IsThrowingLeft, point);
		_level.AddProjectile(thunderBoltDamageArea);
		_parentLunais.ReduceAura(3);
	}
}
