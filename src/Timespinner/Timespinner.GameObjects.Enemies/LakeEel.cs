using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LakeEel : Monster
{
	private const int LinkCount = 24;

	private const int HistoryJump = 5;

	private const int HistoryStartOffset = 1;

	private const int HistorySize = 125;

	private const int ChompThreshold = 7500;

	private const int GrowlThreshold = 50000;

	private const float TimeBeforeRetargeting = 3f;

	private const float TrackingMultiplier = 3.5f;

	private const float BaseSwimVelocity = 125f;

	private const float AggroSwimVelocity = 200f;

	private const float AppendageKillRate = 0.05f;

	private readonly float _swimVelocityRandomizer;

	private readonly float _randomMultiplier;

	private readonly Point _spawnPoint;

	private readonly Appendage _tailAppendage;

	private readonly LakeEelDustParticleSystem _eelDustParticles;

	private readonly List<DrawHistory> _headHistories = new List<DrawHistory>();

	private bool _isNearHero;

	private bool _isWithinGrowlingDistanceToHero;

	private bool _isDigging;

	private bool _wasDigging;

	private int _deadAppendageCount;

	private float _actionTimer;

	private float _targetRotation;

	private float _appendageDeathTimer;

	private float _digStartEndSFXTimer = 1f;

	private float _timeSpentDigging;

	private float _diggingCryTimer;

	private Point _lastPosition;

	private SFXCueInstance _diggingLoopCue;

	public LakeEel(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 0.5f;
		_bboxOffset = new Point(7, 3);
		Bbox = new Rectangle(_position.X, _position.Y, 13, 13);
		DrawOrigin = new Vector2(13f, 8f);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAlwaysAggroed = true;
		_doesIgnoreOutOfBoundsDeath = true;
		base.CannotBeGrabbed = true;
		_randomMultiplier = (float)_level.NextRandomDouble();
		_swimVelocityRandomizer = (0f - _randomMultiplier) * 25f;
		_actionTimer = _randomMultiplier;
		ChangeAnimation(0);
		_tailAppendage = new Appendage(this, new Point(7, 7), new Point(0, 4), _level, _sprite)
		{
			Position = Position,
			DrawOrigin = new Vector2(3f, 7f)
		};
		_tailAppendage.ChangeAnimation(4);
		_appendages.Add(_tailAppendage);
		for (int i = 0; i < 24; i++)
		{
			Appendage appendage = new Appendage(this, new Point(14, 12), new Point(2, 3), _level, _sprite)
			{
				DrawOrigin = new Vector2(9f, 9.5f)
			};
			appendage.ChangeAnimation(3);
			_appendages.Add(appendage);
		}
		_eelDustParticles = new LakeEelDustParticleSystem(_sprite, 15);
		_particleSystems.Add(_eelDustParticles);
		_spawnPoint = Position;
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		Point position = tile.Position;
		if (position.X > 16 && position.X < _level.RoomSize.X - 16 && position.Y > 16 && position.Y < _level.RoomSize.Y - 16)
		{
			bool flag = false;
			Vector2 velocity = new Vector2(0f - _velocity.X, 0f - _velocity.Y);
			if (Math.Abs(depth.Y) < Math.Abs(depth.X))
			{
				bool flag2 = _level.CheckNearbyIgnoreSlopes(EDirection.North, tile.DictKey);
				bool flag3 = _level.CheckNearbyIgnoreSlopes(EDirection.South, tile.DictKey);
				if (!flag2 || !flag3)
				{
					flag = true;
					if (flag3 == velocity.Y >= 0f)
					{
						velocity = _velocity;
					}
				}
			}
			else
			{
				bool flag4 = _level.CheckNearbyIgnoreSlopes(EDirection.West, tile.DictKey);
				bool flag5 = _level.CheckNearbyIgnoreSlopes(EDirection.East, tile.DictKey);
				if (!flag4 || !flag5)
				{
					flag = true;
					if (flag5 == velocity.X > 0f)
					{
						velocity = _velocity;
					}
				}
			}
			if (flag)
			{
				velocity.Normalize();
				Vector2 where = tile.Bbox.Center.ToVector2();
				_eelDustParticles.AddParticles(where, velocity);
				if (_digStartEndSFXTimer <= 0f)
				{
					if (_isDigging && !_wasDigging)
					{
						PlayCue(ESFX.EnemyEelDiggingLoopStart, Position);
						_digStartEndSFXTimer = 1f;
					}
					else if (_timeSpentDigging > 1f && _wasDigging && !_isDigging)
					{
						PlayCue(ESFX.EnemyEelDiggingLoopEnd, Position);
						_digStartEndSFXTimer = 1f;
					}
				}
			}
		}
		_isDigging = true;
		return false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateRotation(delta);
			UpdateLinks();
			_isAffectedByGravity = !IsInWater;
			if (_digStartEndSFXTimer > 0f)
			{
				_digStartEndSFXTimer -= delta;
			}
			if (IsOutsideOfLevel())
			{
				_isDigging = true;
			}
			if (_isDigging)
			{
				_timeSpentDigging += delta;
				if (!_wasDigging)
				{
					if (_diggingLoopCue == null)
					{
						_diggingLoopCue = CreateCue(ESFX.EnemyEelDiggingLoop, Position, isLooped: true);
						if (_diggingLoopCue != null)
						{
							_diggingLoopCue.Anchor = this;
							_diggingLoopCue.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
							_diggingLoopCue.PlayWhenInRange();
						}
					}
					else
					{
						_diggingLoopCue.Resume();
					}
				}
				if (base.IsAggroed)
				{
					bool flag = false;
					if (!_wasDigging)
					{
						flag = true;
					}
					else
					{
						_diggingCryTimer -= delta;
						if (_diggingCryTimer <= 0f)
						{
							PlayCue(ESFX.EnemyEelDiggingCry, Position);
							flag = true;
						}
					}
					if (flag)
					{
						_diggingCryTimer = 2f + (float)(_level.NextRandomDouble() * 5.0);
					}
				}
			}
			else if (_wasDigging)
			{
				_timeSpentDigging = 0f;
				if (_diggingLoopCue != null && !_diggingLoopCue.IsPaused)
				{
					_diggingLoopCue.Pause(0.1f);
				}
			}
			_wasDigging = _isDigging;
			_isDigging = false;
		}
		base.Update(delta);
	}

	private void UpdateRotation(float delta)
	{
		if (_isRunningDeathScript)
		{
			return;
		}
		if (Math.Abs(base.Rotation - _targetRotation) < 0.1f)
		{
			_actionTimer = 3f;
			base.Rotation = _targetRotation;
			return;
		}
		float num = 3.5f - _randomMultiplier;
		if (_targetRotation > base.Rotation)
		{
			base.Rotation += num * delta;
		}
		else
		{
			base.Rotation -= num * delta;
		}
	}

	private void UpdateLinks()
	{
		if (_lastPosition != Position)
		{
			DrawHistory drawHistory = new DrawHistory();
			drawHistory.DrawPosition = Bbox.Center;
			drawHistory.Rotation = base.Rotation;
			drawHistory.IsImageFacingLeft = IsFacingLeft;
			DrawHistory drawHistory2 = drawHistory;
			int count = _headHistories.Count;
			DrawHistory drawHistory3 = ((count > 0) ? _headHistories[count - 1] : drawHistory2);
			for (int i = 0; i <= 24; i++)
			{
				Appendage appendage = _appendages[i];
				int num = i * 5;
				DrawHistory drawHistory4 = ((count > num) ? _headHistories[num] : drawHistory3);
				appendage.Position = drawHistory4.DrawPosition.Add(new Point(0, appendage.Bbox.Height / 2));
				appendage.Rotation = drawHistory4.Rotation;
				appendage.IsFacingLeft = drawHistory4.IsImageFacingLeft;
			}
			_headHistories.Add(drawHistory2);
			if (count > 125)
			{
				_headHistories.RemoveAt(0);
			}
			_lastPosition = Position;
		}
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		bool isNearHero = _isNearHero;
		bool isWithinGrowlingDistanceToHero = _isWithinGrowlingDistanceToHero;
		_actionTimer += delta;
		if (_actionTimer >= 3f)
		{
			_isNearHero = false;
			_actionTimer -= 3f;
			_startPosition = _position;
			Protagonist nearestProtagonist = _level.GetNearestProtagonist(_position);
			if (nearestProtagonist != null && nearestProtagonist.Bbox.Intersects(base.DeaggroBbox))
			{
				Point position = nearestProtagonist.Position;
				int height = nearestProtagonist.Bbox.Height;
				Point position2 = new Point(position.X, position.Y - height);
				int waterTopFromBelowWater = _level.GetWaterTopFromBelowWater(position2);
				if (waterTopFromBelowWater > 5)
				{
					position2 = new Point(position2.X, Math.Max(position2.Y, waterTopFromBelowWater));
					_targetPosition = position2;
					int num = position.DistanceSquared(_position);
					_isNearHero = num < 7500;
					_isWithinGrowlingDistanceToHero = num < 50000;
				}
				else
				{
					_targetPosition = FindRandomNearbyWaterTile();
				}
			}
			else
			{
				_targetPosition = FindRandomNearbyWaterTile();
			}
			_targetRotation = (float)Math.Atan2(_targetPosition.X - _startPosition.X, -(_targetPosition.Y - _startPosition.Y)) + (float)Math.PI / 2f;
			if (_targetRotation < 0f && base.Rotation > (float)Math.PI)
			{
				_targetRotation += (float)Math.PI * 2f;
			}
			if (!isNearHero && _isNearHero)
			{
				ChangeAnimation(0, 3, 0.1f, EAnimationType.Cycle);
				PlayCue(ESFX.EnemyEelGrowlBig);
			}
			else if (isNearHero && !_isNearHero)
			{
				ChangeAnimation(0);
			}
			if (!isWithinGrowlingDistanceToHero && _isWithinGrowlingDistanceToHero)
			{
				PlayCue(ESFX.EnemyEelGrowlSmall);
			}
		}
		ManageState(EAFSM.Moving);
		float num2 = (_isNearHero ? 200f : 125f) + _swimVelocityRandomizer;
		_velocity = new Vector2(0f - (float)Math.Cos(base.Rotation), (float)(0.0 - Math.Sin(base.Rotation))) * num2;
		if (_isAffectedByGravity)
		{
			_velocity = new Vector2(_velocity.X, _velocity.Y + delta * _gravityAcceleration * 2f);
		}
	}

	private Point FindRandomNearbyWaterTile()
	{
		return _spawnPoint;
	}

	protected override void UpdateDeathScript(float delta)
	{
		UpdateParticleSystems(delta);
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			PlayCue(ESFX.EnemyEelDeathLoop, Position, isLooped: true);
			DropLoot();
		}
		_appendageDeathTimer -= delta;
		_deathScriptTimer += delta;
		if (!(_appendageDeathTimer <= 0f))
		{
			return;
		}
		bool flag = false;
		foreach (Appendage appendage in _appendages)
		{
			if (appendage.DoesDrawBaseSprite)
			{
				_level.AddAnimation(EBattleAnimationType.DustBoom, appendage.Bbox.Center, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
				appendage.DoesDrawBaseSprite = false;
				appendage.DoesCollideWithAnything = false;
				_deadAppendageCount++;
				flag = true;
				break;
			}
		}
		if (!flag || _deadAppendageCount >= _headHistories.Count / 5)
		{
			Point center = Bbox.Center;
			_level.AddAnimation(EBattleAnimationType.DustBoom, center.Add(0, 8), ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
			_level.AddAnimation(EBattleAnimationType.DustBoom, center.Add(0, -8), ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
			_level.AddAnimation(EBattleAnimationType.DustBoom, center.Add(8, 0), ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
			_level.AddAnimation(EBattleAnimationType.DustBoom, center.Add(-8, 0), ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
			RemoveInstance();
		}
		_appendageDeathTimer = 0.05f;
	}
}
