using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisUmbraOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const float ThrowTimeToAttack = 0.15f;

	private const float ThrowTimeToReturn = 0.3f;

	private const float ThrowRadius = 64f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private readonly Point _shadowOffset;

	private readonly Appendage _shadowAppendage;

	private UmbraOrbMeleeDamageArea _damageArea;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Umbra;

	public LunaisUmbraOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(0, 10, 0.05f, EAnimationType.Cycle);
		_shadowOffset = new Point(0, 5);
		_shadowAppendage = new Appendage(this, new Point(10, 10), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = _shadowOffset,
			DoesInheritDrawColor = false,
			DrawColor = Color.White * 0.5f,
			DoesDrawAura = true,
			AuraColor = Color.Purple * 0.8f,
			AuraSize = 0.1f,
			AuraCount = 5f,
			AuraFrequency = 9f,
			AuraOffset = new Vector2(0f, 1f),
			DoesDrawTrail = true,
			TrailLength = 6,
			TrailFadeRate = 1f
		};
		_shadowAppendage.ChangeAnimation(-1);
		base.Appendages.Add(_shadowAppendage);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
		_damageArea = new UmbraOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, _shadowAppendage, base.OrbDamage, this)
		{
			AnchorOffset = new Point(0, -_shadowOffset.Y)
		};
		_level.AddProjectile(_damageArea);
		_shadowAppendage.ChangeAnimation(10);
		_level.PlayCue(ESFX.LunaisOrbUmbraWhiff, Position);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		Position = _baseOrbitPosition;
		_shadowAppendage.FollowType = EAppendageFollowType.None;
		if (_currentThrowTime > 0.45000002f)
		{
			base.State = EOrbState.Idle;
			_damageArea.SilentKill();
			_damageArea = null;
			_shadowAppendage.FollowType = EAppendageFollowType.AnchorLocked;
			_shadowAppendage.ChangeAnimation(-1);
			return;
		}
		if (_currentThrowTime >= 0.15f && currentThrowTime < 0.15f)
		{
			base.IsAtAttackApex = true;
		}
		if (_damageArea != null)
		{
			float num = ((_currentThrowTime <= 0.15f) ? (_currentThrowTime / 0.3f) : (0.5f + (_currentThrowTime - 0.15f) / 0.6f));
			float num2 = 64f * (float)Math.Sin((double)num * Math.PI);
			_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
			Vector2 value = new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
			float num3 = ((_currentThrowTime <= 0.15f) ? (_currentThrowTime / 0.15f) : (1f + (_currentThrowTime - 0.15f) / 0.3f));
			float num4 = (float)Math.Sin(num3 * ((float)Math.PI / 2f));
			float scaleFactor = 1f - num4;
			_shadowAppendage.Position = _shadowOffset.Add(Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor), Vector2.Multiply(value, num4)));
		}
	}
}
