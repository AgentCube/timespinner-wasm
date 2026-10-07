using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal sealed class ZelBossSpike : DamageArea
{
	private const int SpikeHeight = 104;

	private const int BboxOffsetY = 2;

	private const int SpikeFrameHeight = 128;

	private const int SpikeBottomOffset = 22;

	private const int FloorY = 208;

	private const int SpikeDepth = 9;

	private const int HalfHeight = 52;

	private const int EndY = 217;

	private const int SpikePreviewHeight = 32;

	private const float TimeForSpikeToStab = 0.5f;

	private const float TimeBeforeSpikeAppears = 0.75f;

	private const float TimeForSpikeToBeVisible = 20.5f;

	private const float StabPercentageBeforeCanDamage = 0.15f;

	private readonly ZelPebblesParticleSystem _pebbleParticles;

	private readonly ZelSpikeDustParticleSystem _dustParticles;

	private readonly ZelBossSolidSpike _solidArea;

	private readonly ZelBossSpikeShardsDamageArea _shardDamageArea;

	private bool _hasBeenHitByInferno;

	private float _sleepTimer;

	private float _stabTimer;

	private Point _initialPosition;

	private Vector2 _spikeBasePosition;

	internal bool IsFinished { get; private set; }

	public ZelBossSpike(Level inLevel, Point inPosition, SpriteSheet inSprite, float sleepTime, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = inSprite;
		_pebbleParticles = new ZelPebblesParticleSystem(_level.GCM.TxParticleEnergy, 1);
		_particleSystems.Add(_pebbleParticles);
		_dustParticles = new ZelSpikeDustParticleSystem(_level.GCM.TxParticleSmoke, 1);
		_particleSystems.Add(_dustParticles);
		_doesAutomaticallyEmitParticles = false;
		Reset(inPosition, sleepTime);
		base.DrawPlane = EDrawPlane.Front;
		_power = (int)Math.Ceiling((float)baseDamage * 1.1f);
		Bbox = new Rectangle(_position.X, _position.Y, 64, 104);
		_doesDrawBaseSprite = false;
		ChangeAnimation(-1);
		_doesRotateBasedOnVelocity = false;
		_doesCollideWithFloors = false;
		_isAffectedByGravity = false;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = false;
		_canDamageThings = false;
		base.DoesKnockBack = true;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = false;
		_solidArea = new ZelBossSolidSpike(_level, Position, new ObjectTileSpecification(), OnHitByZelInferno);
		Appendage appendage = new Appendage(this, new Point(32, 126), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-16, 74),
			IsFacingLeft = false
		};
		appendage.ChangeAnimation(41);
		base.Appendages.Add(appendage);
		Appendage appendage2 = new Appendage(this, new Point(32, 126), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-16, 74),
			IsFacingLeft = true
		};
		appendage2.ChangeAnimation(41);
		base.Appendages.Add(appendage2);
		_shardDamageArea = new ZelBossSpikeShardsDamageArea(_level, Position, this, _power, _sprite);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_sleepTimer > 0f)
			{
				_canDamageThings = false;
				_doesDrawBaseSprite = false;
				float sleepTimer = _sleepTimer;
				_sleepTimer -= delta;
				if (_sleepTimer < 0.75f)
				{
					if (sleepTimer >= 0.75f)
					{
						PlayCue(ESFX.BossZelSpike);
					}
					float percentage = 1f - _sleepTimer / 0.75f;
					int num = _initialPosition.Y - 22;
					int num2 = _initialPosition.Y - 22 + 32;
					Position = new Point(_initialPosition.X, (int)Math.Ceiling(MathEx.SineInterpolate(num, num2, percentage)) - 52);
				}
			}
			else
			{
				if (_stabTimer <= 0f)
				{
					_doesDrawBaseSprite = true;
				}
				float stabTimer = _stabTimer;
				_stabTimer += delta;
				if (_stabTimer < 0.5f)
				{
					float num3 = _stabTimer / 0.5f;
					num3 = 1f - (float)Math.Cos(num3 * ((float)Math.PI / 2f));
					if (num3 > 0.15f)
					{
						_canDamageThings = true;
					}
					num3 = ((num3 > 1f) ? 1f : num3);
					int num4 = _initialPosition.Y - 22 + 32;
					Position = new Point(_initialPosition.X, (int)Math.Ceiling(MathEx.CosInterpolate(num4, 217f, num3)) - 52);
				}
				else
				{
					_canDamageThings = false;
					if (stabTimer < 0.5f)
					{
						Position = new Point(_initialPosition.X, 165);
						_level.RequestScreenShake(new Vector2(0f, 3f), 0.3f, 8f, isAffectedByTime: true);
						_dustParticles.AddParticles(new Vector2(Position.X, 208f));
						_solidArea.Reset(new Point(Position.X, Position.Y + 52));
						_level.RequestAddObject(_solidArea);
					}
				}
			}
		}
		base.Update(delta);
		if (_fadeTimer >= _timeToFade)
		{
			IsFinished = true;
			_solidArea.Finish();
			_level.RequestRemoveObject(_solidArea);
		}
	}

	private void OnHitByZelInferno(ZelInfernoProjectile inferno)
	{
		if (!_hasBeenHitByInferno && !_canDamageThings && !_isFading)
		{
			Point intersectionCenter = RectangleExtensions.GetIntersectionCenter(base.OuterBbox, inferno.DamageBbox);
			_shardDamageArea.Reset();
			_shardDamageArea.Push(inferno.Velocity, intersectionCenter);
			_level.RequestAddObject(_shardDamageArea);
			_hasBeenHitByInferno = true;
			IsFinished = true;
			_solidArea.Finish();
			_level.RequestRemoveObject(_solidArea);
			_level.PlayCue(ESFX.BossZelStonePillarsBreak, Position);
			SilentKill();
		}
	}

	internal void Reset(Point position, float sleepTime)
	{
		_hasBeenHitByInferno = false;
		_stabTimer = 0f;
		_isFading = false;
		base.DrawColor = Color.White;
		_fadeTimer = 0f;
		IsFinished = false;
		IsFacingLeft = position.X % 16 == 0;
		_initialPosition = position;
		Position = new Point(position.X, position.Y - 22 - 52);
		_spikeBasePosition = _initialPosition.ToVector2();
		SnapBboxToPosition();
		_sleepTimer = sleepTime + 0.75f;
		_life = 20.5f + _sleepTimer;
		_pebbleParticles.AddParticles(_spikeBasePosition);
	}
}
