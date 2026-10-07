using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._14_Gyre;

internal sealed class GyreZelSwordDamageArea : DamageArea
{
	private const float MaxLife = 1f;

	internal bool IsFinished { get; set; }

	public GyreZelSwordDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inDamage, SpriteSheet sprite)
		: base(inLevel, inPosition, inSide, -1, null)
	{
		_sprite = sprite;
		_doesDrawSpriteAndAppendages = false;
		ChangeAnimation(-1);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 1f;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Sharp;
		_doesProjectileChangeFacingBasedOnVelocity = false;
		_doesRotateBasedOnVelocity = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void SetBoundingBox(Point position, int width, int height)
	{
		Position = position;
		base.DamageDimensions = new Point(width, height);
	}

	internal void Reset(Point position, bool isFacingLeft, int damage)
	{
		_power = damage;
		Position = position;
		IsFacingLeft = isFacingLeft;
		IsFinished = false;
		_isFading = false;
		_fadeTimer = 0f;
		_life = 1f;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(EBattleAnimationType.SmallGrayHit, intersectionCenter, _teamSide, Position.X < target.Position.X, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisOrbImpactSharp, intersectionCenter);
	}
}
