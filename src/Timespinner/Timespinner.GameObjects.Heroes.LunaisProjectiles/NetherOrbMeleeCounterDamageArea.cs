using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class NetherOrbMeleeCounterDamageArea : LunaisBaseOrbDamageArea
{
	private const int Anim_SwirlStart = 14;

	private const int DamageSize = 48;

	private const int SwirlCount = 8;

	private const float MaxLife = 1f;

	private const float SwirlMaxSize = 1.25f;

	private const float SwirlLifeTime = 1f;

	private const float SwirlLifeOffsetMultiplier = 0.125f;

	private const float SwirlRotationCount = 0.1f;

	private static readonly Color SwirlColorA = new Color(0.4f, 0.25f, 0.5f, 0.5f);

	private static readonly Color SwirlColorB = new Color(0.35f, 0.55f, 0.3f, 0.5f);

	private readonly Appendage[] _swirls;

	private float _swirlTimer;

	internal bool IsFinished { get; private set; }

	public NetherOrbMeleeCounterDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_sprite = inLevel.GCM.SpOrbMeleeNether;
		ChangeAnimation(-1);
		_damageDimensions = new Point(48, 48);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 48, 48);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 1f;
		base.DamageTimeoutTime = 0.3f;
		_damageElement = EDamageElement.Dark;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		base.DoesKillProjectilesOnImpact = true;
		base.DoesDieOnImpact = false;
		base.DoesDieToEnemyProjectiles = false;
		_doesUseAppendageCollision = false;
		_doAppendagesInheritDrawColor = false;
		_doesLifetimeAffectAlpha = false;
		_swirls = new Appendage[8];
		Point bboxDimensions = new Point(48, 48);
		Point inBboxOffset = new Point(8, 8);
		for (int i = 0; i < 8; i++)
		{
			Appendage appendage = new Appendage(this, bboxDimensions, inBboxOffset, _level, _sprite)
			{
				DrawOrigin = new Vector2(31.5f, 31.5f)
			};
			appendage.ChangeAnimation(14);
			appendage.Position = new Point(Position.X, Position.Y + 24);
			_swirls[i] = appendage;
			_appendages.Add(appendage);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			UpdateSwirls(delta);
		}
		base.Update(delta);
	}

	private void UpdateSwirls(float delta)
	{
		_swirlTimer += delta;
		float num = 1f;
		if (_timeToFade > 0f)
		{
			num = 1f - _fadeTimer / _timeToFade;
		}
		for (int i = 0; i < 8; i++)
		{
			Appendage appendage = _swirls[i];
			float num2 = (float)i * 0.125f;
			float num3 = _swirlTimer - num2;
			if (num3 < 0f)
			{
				appendage.DrawColor = Color.Transparent;
				continue;
			}
			Color color = ((i % 2 == 0) ? SwirlColorA : SwirlColorB);
			float num4 = num3.Mod(1f);
			float num5 = num4 / 1f;
			float num6 = (float)Math.Sin(num5 * (float)Math.PI);
			appendage.DrawColor = color * num6 * num;
			appendage.Scale = MathEx.SineInterpolate(0.1f, 1.25f, num5);
			appendage.Rotation = (float)Math.PI * 2f * num5 * 0.1f;
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position)
	{
		Position = position;
		SnapBboxToPosition();
		_swirlTimer = 0f;
		Appendage[] swirls = _swirls;
		foreach (Appendage appendage in swirls)
		{
			appendage.DrawColor = Color.Transparent;
			appendage.Position = new Point(position.X, position.Y + 24);
		}
		base.ID = -1;
		_isFading = false;
		_fadeTimer = 0f;
		_life = 1f;
		IsFinished = false;
	}
}
