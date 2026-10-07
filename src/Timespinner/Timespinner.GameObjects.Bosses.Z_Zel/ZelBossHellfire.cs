using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal sealed class ZelBossHellfire : Projectile
{
	private const int Anim_FlameStart = 28;

	private const int Anim_FlameLength = 4;

	private const int Anim_DeathStart = 32;

	private const int Anim_DeathLength = 4;

	private const int Anim_AppearStart = 36;

	private const int Anim_AppearLength = 5;

	private const int BboxWidth = 12;

	private const float MaxLife = 2.25f;

	private readonly int _basePower;

	private readonly BattleAnimation _appearAnimation;

	private readonly BattleAnimation _deathAnimation;

	public ZelBossHellfire(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, int basePower)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_sprite = sprite;
		_basePower = basePower;
		_bboxOffset = new Point(3, 4);
		Bbox = new Rectangle(0, 0, 12, 12);
		_doesDrawTrail = true;
		_trailLength = 4;
		_trailFadeRate = 2f;
		_isTrailLengthAffectedByTime = false;
		ChangeAnimation(28, 4, 0.1f, EAnimationType.Cycle);
		_appearAnimation = new BattleAnimation(_sprite, Position, _level)
		{
			AnimationStart = 36,
			AnimationLength = 5
		};
		_deathAnimation = new BattleAnimation(_sprite, Position, _level)
		{
			TeamSide = _defaultTeam,
			AnimationStart = 32,
			AnimationLength = 4,
			DoesFadeOut = true
		};
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collisionRectangle)
	{
		base.AddImpactAnimation(target, collisionRectangle);
		FadeKill();
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		DoDeathAnimation(contactPoint);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	internal void DoDeathAnimation(Point position)
	{
		_deathAnimation.Reset(position, IsFacingLeft);
		_level.AddAnimation(_deathAnimation);
		_level.PlayCue(ESFX.FoleyLanternExtinguish, position);
	}

	public void Reset(Point startPoint, Vector2 iV)
	{
		_isDormant = true;
		_dormantTimer = 0.1f;
		_isFading = false;
		_fadeTimer = 0f;
		_life = 2.25f;
		base.ID = -1;
		base.CanDamageThings = true;
		_power = _basePower;
		_initialVector = iV;
		Position = startPoint;
		SnapBboxToPosition();
		_intermediatePositions.Clear();
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_airDragFactor = 0.1f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		base.DoesDieOnImpact = true;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithWalls = false;
		_doesCollideWithFloors = false;
		base.DoesDieToEnemyProjectiles = true;
		base.DrawColor = Color.White;
		ClearTrailHistory();
		_appearAnimation.Reset(startPoint, iV.X < 0f);
		_level.AddAnimation(_appearAnimation);
		Update(0f);
	}
}
