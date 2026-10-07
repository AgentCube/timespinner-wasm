using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesSnailSpitDamageArea : DamageArea
{
	private const int SpitStorageSize = 32;

	private const float MaxLife = 5f;

	private readonly int _floorHeight;

	private readonly CavesSnailFloorGoopEvent _floorGoopEvent;

	private readonly CaveSnailSpitUnit[] _spits = new CaveSnailSpitUnit[32];

	private int _spitCreationCount;

	internal bool IsFinished { get; private set; }

	public CavesSnailSpitDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, SpriteSheet sprite, int floorHeight)
		: base(inLevel, inPosition, inSide, -1, null)
	{
		_sprite = sprite;
		_floorHeight = floorHeight;
		_power = baseDamage;
		_force = 0;
		_life = 5f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Blunt;
		base.DamageTimeoutTime = 0.1f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_isFlying = true;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithFloors = true;
		_doesCollideWithWalls = false;
		_doesCollideWithCeilings = true;
		_isIgnoringPlatform = false;
		_doesUseAppendageCollision = true;
		_doesDieOutsideOfVisibleArea = false;
		_doesDrawBaseSprite = false;
		_doAppendagesInheritDrawColor = false;
		_floorGoopEvent = new CavesSnailFloorGoopEvent(_level, Position, -1, new ObjectTileSpecification());
		_level.RequestAddObject(_floorGoopEvent);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !IsFinished && _spitCreationCount > 0)
		{
			for (int i = 0; i < _spitCreationCount; i++)
			{
				CaveSnailSpitUnit caveSnailSpitUnit = _spits[i];
				if (caveSnailSpitUnit.IsFinished && !caveSnailSpitUnit.HasBeenRemoved)
				{
					base.Appendages.Remove(caveSnailSpitUnit);
					caveSnailSpitUnit.HasBeenRemoved = true;
				}
			}
		}
		base.Update(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.PlayCue(ESFX.EnemySnailSpitLand, intersectionCenter);
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			TeamSide = _teamSide,
			AnimationSpeed = 0.03f,
			AnimationStart = 35,
			AnimationLength = 4,
			IsFacingLeft = !target.IsFacingLeft
		});
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.Appendages.Clear();
		base.SilentKill();
	}

	internal void End()
	{
		float num = 0f;
		for (int i = 0; i < _spitCreationCount; i++)
		{
			CaveSnailSpitUnit caveSnailSpitUnit = _spits[i];
			if (!caveSnailSpitUnit.IsFinished && caveSnailSpitUnit.Life > num)
			{
				num = caveSnailSpitUnit.Life;
			}
		}
		_life = num;
	}

	internal void Reset(Point position)
	{
		Position = position;
		SnapBboxToPosition();
		base.ID = -1;
		_isFading = false;
		IsFinished = false;
		base.CanDamageEnemies = true;
		_life = 5f;
		base.Appendages.Clear();
	}

	internal void EmitSpit(Point position, Vector2 iV)
	{
		CaveSnailSpitUnit caveSnailSpitUnit = null;
		if (_spitCreationCount < 32)
		{
			caveSnailSpitUnit = new CaveSnailSpitUnit(this, _level, _sprite, _floorHeight, AddGoopUnit);
			_spits[_spitCreationCount] = caveSnailSpitUnit;
			_spitCreationCount++;
		}
		else
		{
			for (int i = 0; i < 32; i++)
			{
				if (_spits[i].IsFinished)
				{
					caveSnailSpitUnit = _spits[i];
					break;
				}
			}
		}
		if (caveSnailSpitUnit != null)
		{
			caveSnailSpitUnit.Reset(position, iV);
			base.Appendages.Add(caveSnailSpitUnit);
		}
	}

	private void AddGoopUnit(Point goopPosition)
	{
		PlayCue(ESFX.EnemySnailSpitLand, goopPosition);
		_floorGoopEvent.AddGoopUnit(goopPosition);
	}
}
