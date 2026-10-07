using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal sealed class RavenBossSingleProjectile : Projectile
{
	private const float MaxLife = 5f;

	private readonly int _levelWidth;

	private readonly int _levelHeight;

	private readonly RavenBossFeathersParticleSystem _featherParticles;

	private readonly Appendage _outlineAppendage;

	internal bool IsFinished { get; private set; }

	public RavenBossSingleProjectile(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_levelWidth = _level.RoomSize.X;
		_levelHeight = _level.RoomSize.Y;
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 6, inPosition.Y - 6, 12, 12);
		_bboxOffset = new Point(9, 17);
		_power = (int)Math.Ceiling(0.9f * (float)baseDamage);
		_force = 0;
		_life = 5f;
		_doesDieOnTiles = true;
		base.DoesCollideWithTiles = true;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = false;
		base.DoesDieToEnemyProjectiles = true;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = false;
		_isAffectedByLevelBounds = true;
		_animationSpeed = 0f;
		_maxMoveSpeed = 300f;
		_doesRotateBasedOnVelocity = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = false;
		_doesUseAppendageCollision = false;
		_doesDrawTrail = true;
		_trailLength = 8;
		_trailFadeRate = 4f;
		_isTrailLengthAffectedByTime = false;
		ChangeAnimation(2, 4, 0.1f, EAnimationType.Cycle);
		_featherParticles = new RavenBossFeathersParticleSystem(_sprite, 1, 6, 4);
		_outlineAppendage = new Appendage(this, new Point(32, 40), Point.Zero, _level, _sprite)
		{
			DrawColor = Color.Red * 0.5f,
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(1, 17)
		};
		_outlineAppendage.ChangeAnimation(17, 4, 0.1f, EAnimationType.Cycle);
		_appendages.Add(_outlineAppendage);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && (Position.X < -16 || Position.X > _levelWidth || Position.Y < -16 || Position.Y > _levelHeight))
		{
			SilentKill();
		}
		base.Update(delta);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	private void AddFeathers()
	{
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, _bbox.Center, _level);
		battleAnimation.TeamSide = base.DefaultTeam;
		battleAnimation.AnimationStart = 10;
		battleAnimation.AnimationLength = 6;
		battleAnimation.AnimationSpeed = 0.04f;
		battleAnimation.DrawColor = Color.White * 0.8f;
		battleAnimation.ParticleSystem = _featherParticles;
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
		_level.PlayCue(ESFX.BossRavenBirdDeath, Position);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		AddFeathers();
	}

	public override void Kill()
	{
		AddFeathers();
		base.Kill();
	}

	public void Reset(Point startPoint, Vector2 iV)
	{
		_isFading = false;
		_fadeTimer = 0f;
		_life = 5f;
		IsFinished = false;
		IsFacingLeft = iV.X <= 0f;
		Position = startPoint;
		_initialVector = iV;
		_velocity = iV;
		SnapBboxToPosition();
		base.DrawColor = Color.White;
		ClearTrailHistory();
	}
}
