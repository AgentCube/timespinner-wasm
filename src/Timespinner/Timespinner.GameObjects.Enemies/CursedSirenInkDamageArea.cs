using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CursedSirenInkDamageArea : DamageArea
{
	private const int InkStorageSize = 80;

	private const float MaxLife = 10f;

	private readonly SirenInkUnit[] _inks = new SirenInkUnit[80];

	private int _inkCreationCount;

	internal bool IsFinished { get; private set; }

	public CursedSirenInkDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int damage, SpriteSheet sprite)
		: base(inLevel, inPosition, inSide, -1, null)
	{
		_sprite = sprite;
		_power = damage;
		_force = 0;
		_life = 10f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Dark;
		base.DamageTimeoutTime = 1f;
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
		_isAffectedByWater = false;
	}

	public override void Update(float delta)
	{
		_life = 10f;
		if (!base.IsFrozen && !IsFinished && _inkCreationCount > 0)
		{
			for (int i = 0; i < _inkCreationCount; i++)
			{
				SirenInkUnit sirenInkUnit = _inks[i];
				if (sirenInkUnit.IsFinished && !sirenInkUnit.HasBeenRemoved)
				{
					base.Appendages.Remove(sirenInkUnit);
					sirenInkUnit.HasBeenRemoved = true;
				}
			}
		}
		base.Update(delta);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.Appendages.Clear();
		_power = 0;
		base.SilentKill();
	}

	internal void End()
	{
		float num = 0f;
		for (int i = 0; i < _inkCreationCount; i++)
		{
			SirenInkUnit sirenInkUnit = _inks[i];
			if (!sirenInkUnit.IsFinished && sirenInkUnit.Life > num)
			{
				num = sirenInkUnit.Life;
			}
		}
		_life = num;
	}

	internal void Reset(Point position, int power)
	{
		Position = position;
		SnapBboxToPosition();
		base.ID = -1;
		_power = power;
		_isFading = false;
		IsFinished = false;
		base.CanDamageEnemies = true;
		_life = 10f;
	}

	internal void EmitInk(Point position, Vector2 iV)
	{
		SirenInkUnit sirenInkUnit = null;
		if (_inkCreationCount < 80)
		{
			sirenInkUnit = new SirenInkUnit(this, _level, _sprite);
			_inks[_inkCreationCount] = sirenInkUnit;
			_inkCreationCount++;
		}
		else
		{
			for (int i = 0; i < 80; i++)
			{
				if (_inks[i].IsFinished)
				{
					sirenInkUnit = _inks[i];
					break;
				}
			}
		}
		if (sirenInkUnit != null)
		{
			sirenInkUnit.Reset(position, iV);
			base.Appendages.Add(sirenInkUnit);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
	}

	internal void DrawInk(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
	}
}
