using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal sealed class SandmanBossRubble : Projectile
{
	private const int Anim_FrameStart = 14;

	private const float MaxLife = 3f;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandmanBoulderBreakParticleSystem _boulderBreakParticles;

	private readonly SandDrawHelper _sandDrawHelper;

	private bool _isDrawingSand;

	internal bool IsFinished { get; private set; }

	internal bool IsParticleSystemFinished => _boulderBreakParticles.AreParticlesDone;

	public SandmanBossRubble(Point inPosition, Level inLevel, SpriteSheet inSprite, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, 0.5f, -1)
	{
		_sprite = inSprite;
		_power = (int)Math.Ceiling((float)baseDamage * 1.1f);
		_bboxOffset = new Point(5, 5);
		Bbox = new Rectangle(_position.X, _position.Y, 22, 22);
		DrawOrigin = new Vector2(16f, 16f);
		_isAffectedByLevelBounds = false;
		_maxFallSpeed = 350f;
		int num = _level.NextRandomInt(0, 10);
		_rotationSpeed = num - 5;
		_doesRotateBasedOnVelocity = false;
		base.DoesCollideWithTiles = true;
		_doesCollideWithFloors = true;
		_doesCollideWithCeilings = false;
		_doesCollideWithWalls = false;
		_isAffectedByGravity = true;
		_doesDieOnTiles = true;
		base.DoesDieOnImpact = false;
		base.IsDamageArea = true;
		base.CanDamageEnemyProjectiles = false;
		_life = 3f;
		base.DoesKnockBack = true;
		ChangeAnimation(14);
		_sandDrawHelper = new SandDrawHelper(this);
		_boulderBreakParticles = new SandmanBoulderBreakParticleSystem(_level.GCM.TxParticleEnergy, 1);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_sandDrawHelper.Update(delta);
		}
		base.Update(delta);
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		if (!_isFading)
		{
			_level.PlayCue(ESFX.BossSandmanGroundPoundImpact, Position);
			BattleAnimation battleAnimation = new BattleAnimation(null, new Point(Position.X, contactPoint.Y), _level);
			battleAnimation.ParticleSystem = _boulderBreakParticles;
			BattleAnimation newAnimation = battleAnimation;
			_level.AddAnimation(newAnimation);
			_isFading = true;
			_fadeTimer = 0f;
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position)
	{
		_isFading = false;
		base.DrawColor = Color.White;
		_fadeTimer = 0f;
		IsFinished = false;
		Position = position;
		SnapBboxToPosition();
		_life = 3f;
		base.ID = -1;
		_velocity = Vector2.Zero;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
