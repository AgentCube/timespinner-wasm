using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleConvictionShield : DamageArea
{
	internal const int BaseAnchorOffsetX = -40;

	internal const int BaseAnchorOffsetY = -8;

	private const int Anim_ShieldStart = 3;

	private const float TimeBetweenPlayingBlockSFX = 0.1f;

	private const float TimeToTurnDisappearFadeOut = 0.2f;

	private const float TimeToTurnDisappearFadeIn = 0.3f;

	private const float TimeForEntireTurnDisappear = 0.5f;

	private static readonly Color BaseAuraColor = new Color(0.7f, 0.5f, 0.2f, 0.1f);

	private readonly TempleConviction _parentConviction;

	private bool _wasParentLastFacingLeft;

	private bool _isTurnDisappearing;

	private float _turnDisappearTimer;

	private float _timeSincePlayingBlockSFX;

	public TempleConvictionShield(Level inLevel, Point inPosition, TempleConviction inAnchor, SpriteSheet sprite)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, inAnchor)
	{
		_sprite = sprite;
		_parentConviction = inAnchor;
		ChangeAnimation(3);
		_bboxOffset = new Point(4, 2);
		_bbox = new Rectangle(0, 0, 12, 56);
		base.AnchorOffset = new Point(-40, -8);
		IsFacingLeft = inAnchor.IsFacingLeft;
		_wasParentLastFacingLeft = IsFacingLeft;
		base.Power = _parentConviction.Damage;
		base.CanDamageThings = true;
		base.CanDamageEnemies = true;
		base.CanDamageEvents = false;
		base.CanDamageEnemyProjectiles = true;
		base.DoesKillProjectilesOnImpact = true;
		base.DoesKnockBack = true;
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_trailLength = 8;
		_trailFadeRate = 2f;
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(1f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		_life = 100f;
		_doesRotateBasedOnVelocity = false;
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		_life = 100f;
		if (_timeSincePlayingBlockSFX < 0.1f)
		{
			_timeSincePlayingBlockSFX += delta;
		}
		if (_parentConviction != null)
		{
			if (_parentConviction.IsFacingLeft != _wasParentLastFacingLeft)
			{
				_isTurnDisappearing = true;
				_turnDisappearTimer = 0f;
				base.CanDamageThings = false;
			}
			_wasParentLastFacingLeft = _parentConviction.IsFacingLeft;
		}
		if (_isTurnDisappearing)
		{
			float turnDisappearTimer = _turnDisappearTimer;
			_turnDisappearTimer += delta;
			if (_turnDisappearTimer < 0.5f)
			{
				float num = 1f;
				if (_turnDisappearTimer < 0.2f)
				{
					float num2 = _turnDisappearTimer / 0.2f;
					num = 1f - num2;
				}
				else
				{
					if (turnDisappearTimer < 0.2f && _parentConviction != null)
					{
						IsFacingLeft = _parentConviction.IsFacingLeft;
						ClearTrailHistory();
					}
					float num3 = (_turnDisappearTimer - 0.2f) / 0.3f;
					num = num3;
				}
				base.DrawColor = Color.White * num;
				base.AuraColor = BaseAuraColor * num;
			}
			else
			{
				_isTurnDisappearing = false;
				base.DrawColor = Color.White;
				base.AuraColor = BaseAuraColor;
				base.CanDamageThings = true;
				if (_parentConviction != null)
				{
					IsFacingLeft = _parentConviction.IsFacingLeft;
				}
			}
		}
		base.Update(delta);
	}

	internal override void OnKillOtherProjectile(Projectile enemyProjectile, Vector2 depth)
	{
		if (_timeSincePlayingBlockSFX >= 0.1f)
		{
			PlayCue(ESFX.LunaisShieldDeflect);
			_timeSincePlayingBlockSFX = 0f;
		}
	}

	internal void ForceImageFacing(bool isFacingLeft)
	{
		IsFacingLeft = isFacingLeft;
		_wasParentLastFacingLeft = isFacingLeft;
	}
}
