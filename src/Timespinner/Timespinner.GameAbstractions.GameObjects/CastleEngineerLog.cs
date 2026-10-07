using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class CastleEngineerLog : Projectile
{
	private const float TimeToFadeIn = 0.25f;

	private const float TimeBeforeDamaging = 0.25f;

	private readonly int _baseDamage;

	private bool _hasPlayedBouncingCue;

	private float _timeSinceSpawning;

	private float _damageTimer;

	private SFXCueInstance _rollingSFXLoop;

	public CastleEngineerLog(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, -1f, inID)
	{
		_sprite = inSprite;
		_baseDamage = baseDamage;
		_power = 0;
		_bboxOffset = new Point(8, 2);
		Bbox = new Rectangle(_position.X, _position.Y, 28, 28);
		DrawOrigin = new Vector2(21f, 16f);
		_isAffectedByLevelBounds = false;
		_maxFallSpeed = 350f;
		_teamSide = ETeamSide.Neutral;
		_velocity = new Vector2(-400f, 0f);
		base.CanDamageEnemies = false;
		base.CanDamageThings = false;
		_doesRotateBasedOnVelocity = false;
		_doesBounceOnGround = true;
		_doesDieOnTiles = false;
		_doesCollideWithFloors = true;
		_isAffectedByGravity = true;
		base.DoesCollideWithTiles = true;
		base.DoesDieOnImpact = false;
		base.CanBeStoodOnWhenFrozen = true;
		base.DoesKnockBack = true;
		_doesLifetimeAffectAlpha = false;
		_life = 5f;
		_doesDieOnTiles = true;
		ChangeAnimation(6);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_damageTimer < 0.25f)
			{
				_damageTimer += delta;
				if (_damageTimer >= 0.25f)
				{
					_power = (int)Math.Ceiling((float)_baseDamage * 2.5f);
					base.CanDamageEnemies = true;
					base.CanDamageThings = true;
				}
			}
			_rotationSpeed = _velocity.X / 10f;
			_velocity.X = _velocity.X * 120f * delta;
			if (_rollingSFXLoop == null)
			{
				_rollingSFXLoop = PlayCue(ESFX.EnemyEngineerLogRollLoop, isLooped: true);
			}
			if (_timeSinceSpawning < 0.25f)
			{
				_timeSinceSpawning += delta;
				if (_timeSinceSpawning >= 0.25f)
				{
					base.DrawColor = Color.White;
				}
				else
				{
					float num = _timeSinceSpawning / 0.25f;
					base.DrawColor = Color.White * num;
				}
			}
			if (_isCollidingWithWall)
			{
				Kill();
			}
		}
		base.Update(delta);
	}

	protected override void DoBounce(Point impactPoint)
	{
		if (!_hasPlayedBouncingCue)
		{
			PlayCue(ESFX.EnemyEngineerLogBounce);
			_hasPlayedBouncingCue = true;
		}
		base.DoBounce(impactPoint);
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}

	public override void Kill()
	{
		for (int i = 0; i < 6; i++)
		{
			Point bboxDimensions = new Point(8, 8);
			Point anchorOffset = new Point(0, -16);
			switch (i)
			{
			case 0:
				bboxDimensions = new Point(24, 14);
				anchorOffset = new Point(4, -18);
				break;
			case 1:
				bboxDimensions = new Point(19, 16);
				anchorOffset = new Point(-12, 0);
				break;
			case 2:
				bboxDimensions = new Point(20, 17);
				anchorOffset = new Point(13, -12);
				break;
			case 3:
				bboxDimensions = new Point(25, 16);
				anchorOffset = new Point(8, 0);
				break;
			case 4:
				bboxDimensions = new Point(17, 13);
				anchorOffset = new Point(-13, -16);
				break;
			case 5:
				bboxDimensions = new Point(14, 13);
				anchorOffset = new Point(-7, -15);
				break;
			}
			Appendage appendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite);
			appendage.FollowType = EAppendageFollowType.AnchorLocked;
			appendage.AnchorOffset = anchorOffset;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation(i + 7);
			base.Appendages.Add(appendage2);
		}
		UpdateAppendages(0f);
		DebrisEvent.CreateFromObject(this, new Vector2(15f, -1200f), new Point(Position.X, Bbox.Top), _sprite, DebrisEvent.EDebrisDeathType.Dust);
		_level.PlayCue(ESFX.EnemyEngineerLogBreak, Bbox.Center);
		base.Kill();
	}
}
