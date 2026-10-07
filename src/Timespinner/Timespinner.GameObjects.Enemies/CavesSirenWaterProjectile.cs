using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesSirenWaterProjectile : Projectile
{
	private const int BboxWidth = 14;

	private const int BboxHeight = 14;

	private const int DashVelocity = 500;

	private const float MaxLife = 4f;

	private const float TimeBeforeDashing = 3f;

	internal const float TimeToStartDash = 1f;

	private bool _isDashing;

	private Vector2 _originalIV;

	public CavesSirenWaterProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int damage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_originalIV = new Vector2((iV.X > 0f) ? 1 : (-1), 0f);
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 7, inPosition.Y - 7, 14, 14);
		_bboxOffset = new Point(1, 1);
		DrawOrigin = new Vector2(8f, 8f);
		_power = damage;
		_force = 0;
		_life = 4f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = true;
		_airDragFactor = 0.1f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		base.DoesDieOnImpact = true;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithFloors = true;
		base.DoesDieToEnemyProjectiles = true;
		base.DoesKnockBack = true;
		ChangeAnimation(25, 3, 0.066f, EAnimationType.Cycle, 21, 4, 0.033f);
	}

	public override void Update(float delta)
	{
		if (!_isDashing && _life < 3f)
		{
			_isDashing = true;
			_velocity.X = 500f * _originalIV.X;
			_velocity.Y = 0f;
			_isAffectedByGravity = false;
			_isFlying = true;
			_isAffectedByFriction = false;
			base.DoesDieToEnemyProjectiles = false;
			_bboxOffset = new Point(5, 1);
			ChangeAnimation(101, 3, 0.07f, EAnimationType.Once);
		}
		base.Update(delta);
	}

	public override void FadeKill()
	{
		if (!_isFading)
		{
			DoDeathAnimation(Bbox.Center, isSilent: true);
		}
		base.FadeKill();
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collisionRectangle)
	{
		base.AddImpactAnimation(target, collisionRectangle);
		_level.PlayCue(ESFX.EnemySirenSplashImpact, Position);
		FadeKill();
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		DoDeathAnimation(contactPoint, isSilent: false);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	private void DoDeathAnimation(Point position, bool isSilent)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, position, _level)
		{
			TeamSide = _defaultTeam,
			AnimationStart = 28,
			AnimationLength = 4
		});
		if (!isSilent)
		{
			_level.PlayCue(ESFX.EnemySirenSplashImpact, position);
		}
	}
}
