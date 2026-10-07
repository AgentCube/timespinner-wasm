using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class FlameOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const int FireballArcHeight = 20;

	private const float StartingRotation = (float)Math.PI / 8f;

	private const float SplitDamageMultiplier = 0.5f;

	private readonly bool _isTopFireball;

	private readonly float _fireballLifetime;

	private readonly ParticleSystem _trailParticles;

	private bool _isThrowingLeft;

	private float _timeSinceThrow;

	private BattleAnimation _impactAnimation;

	public FlameOrbMeleeDamageArea(Level inLevel, Point inPosition, Mobile anchorObject, float fireballLife, bool isTopFireball, int inDamage, LunaisOrb parentOrb, SpriteSheet sprite)
		: base(inLevel, inPosition, ETeamSide.Heroes, -1, anchorObject, parentOrb)
	{
		_isTopFireball = isTopFireball;
		_fireballLifetime = fireballLife;
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 5, inPosition.Y - 5, 10, 10);
		_bboxOffset = new Point(1, 3);
		DrawOrigin = new Vector2(6f, 8f);
		UpdateDamage(inDamage);
		_force = 2;
		_life = 100f;
		base.DamageTimeoutTime = 0.225f;
		_damageElement = EDamageElement.Fire;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesRotateBasedOnVelocity = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = false;
		base.DoesCollideWithTiles = false;
		_trailParticles = new LunaisBullet1ParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = Color.Red.ToVector4()
		};
		_particleSystems.Add(_trailParticles);
		_doesDrawTrail = true;
		_trailFadeRate = 1.25f;
		_normalTrailLength = 2;
		_trailLength = _normalTrailLength;
		_trailShrinkRate = 0.15f;
		ChangeAnimation(12, 3, 0.05f, EAnimationType.Cycle);
	}

	internal void UpdateDamage(int damage)
	{
		_power = (int)Math.Ceiling(0.5f * (float)damage);
	}

	public override void Update(float delta)
	{
		_timeSinceThrow += delta;
		float num = _timeSinceThrow / _fireballLifetime;
		float num2 = (float)Math.Sin(num * (float)Math.PI);
		float num3 = 20f * num2;
		base.AnchorOffset = new Point(base.AnchorOffset.X, (int)(num3 * (float)((!_isTopFireball) ? 1 : (-1))));
		float num4 = ((_isTopFireball == _isThrowingLeft) ? ((float)Math.PI / 8f) : (-(float)Math.PI / 8f));
		float value = 0f - num4;
		base.Rotation = MathHelper.Lerp(num4, value, num);
		if (num > 1f)
		{
			_doesDrawBaseSprite = false;
		}
		base.Update(delta);
	}

	public void ResetPosition(Point newPosition, bool isFacingLeft)
	{
		base.ID = -1;
		_timeSinceThrow = 0f;
		Position = newPosition;
		SnapBboxToPosition();
		ClearTrailHistory();
		base.Rotation = 0f;
		_isThrowingLeft = isFacingLeft;
		IsFacingLeft = isFacingLeft;
		_doesDrawBaseSprite = true;
		_life = 100f;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		if (_impactAnimation == null)
		{
			_impactAnimation = new BattleAnimation(_sprite, intersectionCenter, _level)
			{
				TeamSide = _teamSide,
				AnimationSpeed = 0.03f,
				AnimationStart = 38,
				AnimationLength = 4,
				IsFacingLeft = !target.IsFacingLeft
			};
		}
		else
		{
			_impactAnimation.Reset(intersectionCenter, !target.IsFacingLeft);
		}
		_level.AddAnimation(_impactAnimation);
		_level.PlayCue(ESFX.LunaisOrbImpactBurn, intersectionCenter);
	}
}
