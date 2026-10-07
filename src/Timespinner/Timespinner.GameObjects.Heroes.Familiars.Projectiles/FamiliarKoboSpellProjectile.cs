using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal sealed class FamiliarKoboSpellProjectile : FamiliarBaseProjectile
{
	private const int BboxHeight = 16;

	private const int BboxWidth = 16;

	private const float MaxLife = 1.5f;

	internal bool IsFinished { get; set; }

	public FamiliarKoboSpellProjectile(Level inLevel, Point inPosition, ETeamSide inSide, SpriteSheet sprite, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, Vector2.Zero, inSide, 0f, parentFamiliar)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 8, inPosition.Y - 8, 16, 16);
		_bboxOffset = new Point(0, -4);
		DrawOrigin = new Vector2(4f, 8f);
		_force = 0;
		_life = 1.5f;
		_damageElement = EDamageElement.Aura;
		_doesDieOnTiles = false;
		base.DoesCollideWithTiles = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 400f;
		base.DoesDieToEnemyProjectiles = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesDieOnImpact = true;
		_doesRotateBasedOnVelocity = true;
		ChangeAnimation(32, 3, 0.1f, EAnimationType.Cycle);
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_trailLength = 4;
		_trailFadeRate = 2f;
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		Point position = FindDeathPoint(target, collidingRectangle);
		_level.AddAnimation(EBattleAnimationType.MediumHit, position, _teamSide, isFacingRight: false);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Vector2 iV, Point newPosition, Point newTarget, int power)
	{
		_initialVector = iV;
		_velocity = iV;
		Position = newPosition;
		_targetPosition = newTarget;
		ClearTrailHistory();
		SnapBboxToPosition();
		_power = power;
		base.ID = -1;
		_isFading = false;
		_life = 1.5f;
		IsFinished = false;
		_isFading = false;
		_canDamageThings = true;
		_fadeTimer = 0f;
		base.DrawColor = Color.White;
	}
}
