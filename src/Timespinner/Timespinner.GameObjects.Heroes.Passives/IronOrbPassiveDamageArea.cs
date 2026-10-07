using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Passives;

internal sealed class IronOrbPassiveDamageArea : DamageArea
{
	private const float TurnAroundAnimationSpeed = 0.05f;

	private const float TimeToFlipDuringTurnAround = 0.15f;

	private const float TimeBetweenPlayingBlockSFX = 0.1f;

	private readonly LunaisObj _parentLunais;

	private bool _wasParentLastFacingLeft;

	private float _timeSincePlayingBlockSFX;

	public IronOrbPassiveDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inID, LunaisObj inAnchor)
		: base(inLevel, inPosition, inSide, inID, inAnchor)
	{
		_sprite = _level.GCM.SpOrbMeleeIron;
		_parentLunais = inAnchor;
		_bbox = new Rectangle(0, 0, 4, 20);
		base.AnchorOffset = new Point(-20, -12);
		IsFacingLeft = inAnchor.IsFacingLeft;
		_wasParentLastFacingLeft = IsFacingLeft;
		base.CanDamageEnemies = false;
		base.CanDamageEvents = false;
		base.CanDamageEnemyProjectiles = true;
		base.DoesKillProjectilesOnImpact = true;
		_life = 100f;
		_doesRotateBasedOnVelocity = false;
		_timeToTurnAround = 0.15f;
		ChangeAnimation(12);
	}

	public override void Update(float delta)
	{
		_life = 100f;
		if (_timeSincePlayingBlockSFX < 0.1f)
		{
			_timeSincePlayingBlockSFX += delta;
		}
		if (_parentLunais != null)
		{
			_doesDrawSpriteAndAppendages = _parentLunais.IsDrawingSelf && !_parentLunais.AreWeaponsSheathed;
			if (_parentLunais.IsFacingLeft != _wasParentLastFacingLeft)
			{
				TurnAround();
				IsFacingLeft = _parentLunais.IsFacingLeft;
			}
			_wasParentLastFacingLeft = _parentLunais.IsFacingLeft;
		}
		base.Update(delta);
	}

	private void TurnAround()
	{
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 12,
				Length = 3,
				Speed = 0.05f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 12,
				Length = 3,
				Speed = 0.05f,
				Type = EAnimationType.Once,
				IsInReverse = true
			}
		});
	}

	internal override void OnKillOtherProjectile(Projectile enemyProjectile, Vector2 depth)
	{
		Point position = new Point(IsFacingLeft ? Bbox.Left : Bbox.Right, enemyProjectile.Bbox.Center.Y);
		_level.AddAnimation(EBattleAnimationType.SmallFail, position, ETeamSide.Heroes, IsFacingLeft);
		if (_timeSincePlayingBlockSFX >= 0.1f)
		{
			PlayCue(ESFX.LunaisShieldDeflect);
			_timeSincePlayingBlockSFX = 0f;
		}
	}
}
