using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Shapeshifter;

internal class ShapeshifterBackstabber : DamageArea
{
	private const int GroundTravelSpeedX = 7500;

	private const float TimeBetweenRetargeting = 0.1f;

	private const float SpikeAppearAnimationSpeed = 0.065f;

	private const float SpikeDisappearAnimationSpeed = 0.1f;

	private const float TimeBeforeSpikeDamages = 0.13f;

	private const float TimeForSpikeToSit = 0.5f;

	private const float TimeForSpikeToStartDisappearing = 0.695f;

	private const float TimeForSpikeToDisappear = 0.3f;

	private const float TimeForSpikeToLive = 0.995f;

	private const float BlobWibbleAnimationSpeed = 0.07f;

	private readonly bool _isTravelingLeft;

	private readonly int _spikeDamage;

	private bool _hasTouchedGround;

	private bool _hasTurnedIntoSpikes;

	private int _groundY;

	private int _playerX;

	private float _retargetTimer;

	private float _spikeTimer;

	private SFXCueInstance _goopLoop;

	public ShapeshifterBackstabber(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_isTravelingLeft = iV.X < 0f;
		_velocity = iV;
		_power = 0;
		_life = 10f;
		_bboxOffset = new Point(2, 2);
		Bbox = new Rectangle(0, 0, 12, 12);
		_spikeDamage = (int)Math.Ceiling((float)baseDamage * 1.2f);
		ChangeAnimation(68, 3, 0.07f, EAnimationType.Cycle);
		_isAffectedByGravity = true;
		_isAffectedByFriction = false;
		_doesRotateBasedOnVelocity = false;
		base.DoesKnockBack = true;
		base.DoesDieOnImpact = false;
		base.DoesDieToEnemyProjectiles = false;
		base.DoesCollideWithTiles = true;
		_doesCollideWithFloors = true;
		_doesDieOnTiles = true;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_hasTouchedGround && !_hasTurnedIntoSpikes)
			{
				_retargetTimer += delta;
				if (_retargetTimer >= 0.1f)
				{
					_retargetTimer = 0f;
					TargetPlayer();
				}
				if (!(_isTravelingLeft ? (Position.X < _playerX + 8) : (Position.X > _playerX - 8)))
				{
					_velocity = new Vector2(7500f * delta * (float)((!_isTravelingLeft) ? 1 : (-1)), 0f);
					if (Position.Y != _groundY)
					{
						Position = new Point(Position.X, _groundY);
					}
				}
				else
				{
					_bboxOffset = new Point(2, 0);
					Bbox = new Rectangle(0, 0, 32, 32);
					SnapBboxToPosition();
					ChangeAnimation(55, 3, 0.065f, EAnimationType.Once);
					PlayCue(ESFX.BossShapeshifterGoopSpike);
					if (_goopLoop != null)
					{
						_goopLoop.Stop(0.1f);
					}
					_velocity = Vector2.Zero;
					_hasTurnedIntoSpikes = true;
					_life = 0.995f;
				}
			}
			else if (_hasTurnedIntoSpikes)
			{
				float spikeTimer = _spikeTimer;
				_spikeTimer += delta;
				if (_spikeTimer >= 0.13f && spikeTimer < 0.13f)
				{
					_power = _spikeDamage;
					base.DamageTimeoutTime = 2f;
				}
				else if (_spikeTimer >= 0.695f && spikeTimer < 0.695f)
				{
					ChangeAnimation(58, 3, 0.1f, EAnimationType.Once);
					_power = 0;
				}
			}
		}
		base.Update(delta);
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool result = false;
		if (depth != Vector2.Zero && !_hasTouchedGround)
		{
			result = true;
			_hasTouchedGround = true;
			_groundY = tile.Bbox.Top;
			TargetPlayer();
			_isAffectedByGravity = false;
			ChangeAnimation(71, 3, 0.07f, EAnimationType.Cycle);
			Position = new Point(Position.X, _groundY);
			_goopLoop = PlayCue(ESFX.BossShapeshifterGoopLoop, isLooped: true);
		}
		return result;
	}

	private void TargetPlayer()
	{
		Protagonist nearestProtagonist = _level.GetNearestProtagonist(Position);
		if (nearestProtagonist != null)
		{
			_playerX = (_isTravelingLeft ? nearestProtagonist.Bbox.Left : nearestProtagonist.Bbox.Right);
		}
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
	}
}
