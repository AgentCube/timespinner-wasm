using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class ThunderBoltDamageArea : LunaisBaseOrbDamageArea
{
	private const int ShockDetail = 16;

	private const int MinShockDisplacement = 32;

	private const int MediumShockDisplacement = 64;

	private const int DefaultShockDisplacement = 128;

	private const float MaxLife = 0.05f;

	private const float PreBoltSleepTime = 0.5f;

	private static readonly Color PreBoltColor = new Color(0.5f, 0.3f, 0.1f, 0.2f);

	private readonly bool _isParentFacingLeft;

	private readonly EThunderBoltType _thunderBoltType;

	private readonly ThunderSegment _lastAppendage;

	private readonly List<ThunderSegment> _thunderSegments = new List<ThunderSegment>();

	private bool _hasPlayedSFX;

	private bool _isPreBolt;

	private float _preBoltTimer;

	private Color _originalDrawColor;

	public ThunderBoltDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inDamage, IEnumerable<Vector4> intervals, bool isParentFacingLeft, LunaisOrbAbility parentOrb, EThunderBoltType thunderboltType)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_thunderBoltType = thunderboltType;
		_sprite = _level.GCM.SpOrbPlasma;
		_doesDrawBaseSprite = false;
		_doesDrawAppendages = true;
		_doesUseAppendageCollision = true;
		_damageDimensions = new Point(32, 32);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		_power = inDamage;
		_isParentFacingLeft = isParentFacingLeft;
		_force = 2;
		_life = 0.05f;
		base.DamageTimeoutTime = 1f;
		_damageElement = EDamageElement.Plasma;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		foreach (Vector4 interval in intervals)
		{
			ThunderSegment thunderSegment = new ThunderSegment(this, interval, _level, _sprite);
			_thunderSegments.Add(thunderSegment);
			_appendages.Add(thunderSegment);
			_lastAppendage = thunderSegment;
		}
		_isGlowing = true;
		_glowBase = 10f;
		_glowColor = new Color(1f, 1f, 1f, 0.5f);
	}

	internal void MakePreBolt()
	{
		_isPreBolt = true;
		_life += 0.5f;
		_preBoltTimer = 0.5f;
		_originalDrawColor = base.DrawColor;
		_canDamageThings = false;
		_isGlowing = false;
	}

	public override void SnapBboxToPosition()
	{
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isDormant)
		{
			if (_isPreBolt)
			{
				_preBoltTimer -= delta;
				if (_preBoltTimer <= 0f)
				{
					_isPreBolt = false;
					base.DrawColor = _originalDrawColor;
					_isGlowing = true;
					_life = 0.05f;
					_isFading = false;
					_canDamageThings = true;
				}
				else
				{
					float num = 1f - (float)Math.Cos(_preBoltTimer / 0.5f * ((float)Math.PI / 4f));
					base.DrawColor = PreBoltColor * num;
				}
			}
			if (!_isPreBolt)
			{
				if (!_hasPlayedSFX)
				{
					_hasPlayedSFX = true;
					PlayZapCue();
					if (_lastAppendage != null)
					{
						AddEndPointAnimation(!_isParentFacingLeft, _lastAppendage.IntervalEnd);
					}
				}
				_canDamageThings = !_isFading && _life >= 0f;
			}
		}
		base.Update(delta);
	}

	private void PlayZapCue()
	{
		switch (_thunderBoltType)
		{
		case EThunderBoltType.PlasmaPod:
			_level.PlayCue(ESFX.EnemyPlasmaPodAttackCast, Position);
			break;
		default:
			_level.PlayCue(ESFX.LunaisOrbLightningThrow, Position);
			break;
		case EThunderBoltType.VolTerrilis:
			break;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isGlowing && !_isDormant)
		{
			_isGlowing = false;
		}
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.PlayCue(ESFX.LunaisOrbImpactLightning, intersectionCenter);
	}

	internal void AddEndPointAnimation(bool isFacingLeft, Point location)
	{
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpOrbPlasma, location, _level)
		{
			TeamSide = base.TeamSide,
			AnimationStart = 15,
			AnimationLength = 6,
			AnimationSpeed = 0.04f,
			DrawColor = Color.White * 0.9f,
			IsFacingLeft = isFacingLeft
		});
	}

	internal static List<Vector4> GetIntervalsBetween(Point start, Point end, Level level)
	{
		List<Vector4> list = new List<Vector4>();
		int num = 128;
		int num2 = end.DistanceSquared(start);
		if (num2 < 5000)
		{
			num = 32;
		}
		else if (num2 < 30000)
		{
			num = 64;
		}
		AddInterval(new Vector4(start.X, start.Y, end.X, end.Y), num, level, list);
		return list;
	}

	private static void AddInterval(Vector4 startEnd, float displacement, Level level, ICollection<Vector4> intervals)
	{
		if (displacement < 16f)
		{
			intervals.Add(startEnd);
			return;
		}
		float num = (startEnd.X + startEnd.Z) / 2f;
		float num2 = (startEnd.Y + startEnd.W) / 2f;
		num += (float)(level.NextRandomDouble() - 0.5) * displacement;
		num2 += (float)(level.NextRandomDouble() - 0.5) * displacement;
		float displacement2 = displacement / 2f;
		AddInterval(new Vector4(startEnd.X, startEnd.Y, num, num2), displacement2, level, intervals);
		AddInterval(new Vector4(num, num2, startEnd.Z, startEnd.W), displacement2, level, intervals);
	}

	public void Reset(Point startPoint, int spellDamage, List<Vector4> intervals, bool isFacingLeft, float dormantTime)
	{
		_life = 0.05f;
		_power = spellDamage;
		_isFading = false;
		_fadeTimer = 0f;
		if (dormantTime > 0f)
		{
			_dormantTimer = dormantTime;
			_isDormant = true;
		}
		base.ID = -1;
		IsFacingLeft = isFacingLeft;
		Position = startPoint;
		SnapBboxToPosition();
		_isGlowing = true;
		_glowBase = 10f;
		_glowColor = new Color(1f, 1f, 1f, 0.5f);
		_hasPlayedSFX = false;
		int num = 0;
		foreach (Vector4 interval in intervals)
		{
			_thunderSegments[num].Reset(interval);
			num++;
		}
	}

	internal void Cancel()
	{
		if (_isDormant)
		{
			SilentKill();
		}
	}
}
