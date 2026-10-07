using System;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies;

namespace Timespinner.GameObjects.Events;

internal sealed class JunkCrusherEvent : GameEvent
{
	internal enum ECrusherState
	{
		Hanging,
		Falling,
		Sitting,
		Rising
	}

	private const int Width = 104;

	private const int Height = 160;

	private const int DefaultFallLength = 64;

	private const int SensorWidth = 32;

	private const float TimeToCooldownDamage = 1f;

	private const float TimeToHang = 0.2f;

	private const float TimeToFall = 0.4f;

	private const float TimeToSit = 0.5f;

	private const float TimeToRise = 0.8f;

	internal const float Crush_Cycle_Period = 1.9f;

	private readonly int _basePositionY;

	private readonly Point _sensorLocation;

	private readonly LandingDustParticleSystem _dustParticleSystemRight;

	private readonly LandingDustParticleSystem _dustParticleSystemLeft;

	private bool _isPlayerInsideUsWhileFrozen;

	private bool _wasPlayerInsideUsWhileFrozen;

	private ECrusherState _crusherState;

	private int _fallLength;

	private float _crushTimer;

	private float _damageCooldown;

	private Point _floorTilePosition;

	private Point _targetFallPosition;

	internal ECrusherState CrusherState => _crusherState;

	public JunkCrusherEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpPlatforms;
		_bbox = new Rectangle(0, 0, 104, 160);
		_bboxOffset = Point.Zero;
		_doesDrawBaseSprite = false;
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = true;
		IsFacingLeft = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = true;
		_squishDamage = 3;
		_sensorLocation = new Point(Position.X, Position.Y + 80);
		_basePositionY = inPosition.Y;
		_dustParticleSystemRight = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 5, _level.ID, 200);
		_dustParticleSystemLeft = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 5, _level.ID, 200);
		_particleSystems.Add(_dustParticleSystemRight);
		_particleSystems.Add(_dustParticleSystemLeft);
		_crusherState = (ECrusherState)_level.NextRandomInt(0, 3);
	}

	public override void Initialize()
	{
		Tile tile = _level.FindFirstSolidTileInDirection(Position, EDirection.South);
		_floorTilePosition = ((tile != null) ? new Point(tile.Bbox.Center.X, tile.Bbox.Top) : new Point(Position.X, Position.Y + 64));
		_targetFallPosition = new Point(Position.X, _floorTilePosition.Y);
		_fallLength = _targetFallPosition.Y - Position.Y;
		Point zero = Point.Zero;
		TiledAppendage tiledAppendage = new TiledAppendage(this, new Rectangle(zero.X, zero.Y, 104, 16), new Point(4, 0), _level, _sprite);
		tiledAppendage.TilesIndexStart = 0;
		tiledAppendage.TilesIndexLength = 7;
		tiledAppendage.TilesColumnWidth = 7;
		tiledAppendage.TilesColumnMirrorWidth = 4;
		tiledAppendage.FollowType = EAppendageFollowType.ParentObjectLocked;
		TiledAppendage tiledAppendage2 = tiledAppendage;
		_appendages.Add(tiledAppendage2);
		Point anchorOffset = new Point(0, -16);
		TiledAppendage tiledAppendage3 = new TiledAppendage(this, new Rectangle(zero.X, zero.Y, 104, 32), new Point(4, 0), _level, _sprite);
		tiledAppendage3.TilesIndexStart = 4;
		tiledAppendage3.TilesIndexLength = 14;
		tiledAppendage3.TilesColumnWidth = 7;
		tiledAppendage3.TilesColumnMirrorWidth = 4;
		tiledAppendage3.FollowType = EAppendageFollowType.ParentObjectLocked;
		tiledAppendage3.AnchorOffset = anchorOffset;
		TiledAppendage tiledAppendage4 = tiledAppendage3;
		tiledAppendage2.AddAppendage(tiledAppendage4);
		anchorOffset.X = anchorOffset.X;
		for (int i = 0; i < 4; i++)
		{
			anchorOffset.Y -= 32;
			TiledAppendage tiledAppendage5 = new TiledAppendage(this, new Rectangle(zero.X, zero.Y, 80, 32), Point.Zero, _level, _sprite);
			tiledAppendage5.TilesIndexStart = 12;
			tiledAppendage5.TilesIndexLength = 10;
			tiledAppendage5.TilesColumnWidth = 5;
			tiledAppendage5.TilesColumnMirrorWidth = 3;
			tiledAppendage5.FollowType = EAppendageFollowType.ParentObjectLocked;
			tiledAppendage5.AnchorOffset = anchorOffset;
			TiledAppendage newAppendage = tiledAppendage5;
			tiledAppendage4.AddAppendage(newAppendage);
		}
		base.Initialize();
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_level.IsPowerOff)
			{
				UpdateCrushing(delta);
			}
			base.Update(delta);
		}
		else
		{
			UpdateIsWithinObjectVisibleArea();
		}
		_wasPlayerInsideUsWhileFrozen = _isPlayerInsideUsWhileFrozen;
		_isPlayerInsideUsWhileFrozen = false;
	}

	private void UpdateCrushing(float delta)
	{
		if (_damageCooldown > 0f)
		{
			_damageCooldown -= delta;
		}
		_crushTimer += delta;
		switch (_crusherState)
		{
		case ECrusherState.Hanging:
			if (_crushTimer > 0.2f && LookForAliveObjectBelow())
			{
				_crushTimer = 0f;
				_crusherState = ECrusherState.Falling;
				PlayCue(ESFX.EnvCrusherDown);
			}
			break;
		case ECrusherState.Falling:
			if (_crushTimer > 0.4f)
			{
				_crushTimer -= 0.4f;
				_crusherState = ECrusherState.Sitting;
				Position = new Point(Position.X, _targetFallPosition.Y);
				_dustParticleSystemRight.AddParticles(new Vector2(Position.X - 8, Position.Y + 8), 300f);
				_dustParticleSystemLeft.AddParticles(new Vector2(Position.X + 8, Position.Y + 8), 300f);
				_level.RequestScreenShake(new Vector2(0f, 3f), 0.2f, 6f, isAffectedByTime: true);
				PlayCue(ESFX.EnvCrusherCrush);
			}
			else
			{
				float num2 = _crushTimer / 0.4f;
				_floatPosition.Y = (float)_basePositionY + (float)(Math.Pow(num2, 2.0) * (double)_fallLength);
			}
			break;
		case ECrusherState.Sitting:
			Position = new Point(Position.X, _targetFallPosition.Y);
			if (_crushTimer > 0.5f)
			{
				_crushTimer -= 0.5f;
				_crusherState = ECrusherState.Rising;
				PlayCue(ESFX.EnvCrusherUp);
			}
			break;
		case ECrusherState.Rising:
			if (_crushTimer > 0.8f)
			{
				_crushTimer -= 0.8f;
				_crusherState = ECrusherState.Hanging;
				Position = new Point(Position.X, _basePositionY);
			}
			else
			{
				float num = _crushTimer / 0.8f;
				_floatPosition.Y = (float)_basePositionY + (float)(Math.Cos(num * ((float)Math.PI / 2f)) * (double)_fallLength);
			}
			break;
		}
	}

	private bool LookForAliveObjectBelow()
	{
		bool result = false;
		if (Math.Abs(_level.GetNearestProtagonistPosition(_sensorLocation).X - _sensorLocation.X) < 32)
		{
			result = true;
		}
		else
		{
			Monster nearestEnemy = _level.GetNearestEnemy(_sensorLocation, shouldBeVisible: false, shouldBeAggroed: false);
			if (nearestEnemy != null && Math.Abs(nearestEnemy.Position.X - _sensorLocation.X) < 32)
			{
				result = true;
			}
		}
		return result;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (who is LabRobotJunk)
		{
			who.ManageSquishedDamage(this);
		}
		else if (Math.Abs(depth.Y) < Math.Abs(depth.X))
		{
			if (Math.Abs(Position.Y - _targetFallPosition.Y) <= 40)
			{
				_isPlayerInsideUsWhileFrozen = true;
				if (!base.IsFrozen && _damageCooldown <= 0f && _crusherState != ECrusherState.Rising && !who.IsInvulnerable)
				{
					who.ManageSquishedDamage(this);
					_damageCooldown = 1f;
				}
			}
			else
			{
				result = base.TriggerEvent(who, depth);
			}
		}
		else if (!_isPlayerInsideUsWhileFrozen && !_wasPlayerInsideUsWhileFrozen)
		{
			result = base.TriggerEvent(who, depth);
		}
		else
		{
			_isPlayerInsideUsWhileFrozen = true;
		}
		return result;
	}
}
