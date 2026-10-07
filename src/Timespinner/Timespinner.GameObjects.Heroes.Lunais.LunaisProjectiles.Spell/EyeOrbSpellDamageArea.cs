using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class EyeOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	private const int FleshHaloCount = 2;

	private const int BladeCount = 4;

	private const int MaxGrowthHeight = 256;

	private const int MaxGrowthWidth = 256;

	private const float BladeRotationOffset = (float)Math.PI / 2f;

	private const float HaloRotationOffset = (float)Math.PI;

	private const float MaxBoneRotationRate = -10f;

	private const float DamageStopLifeThreshold = 0.4f;

	private const float TimeToGrow = 0.5f;

	private const float MaxLife = 1.5f;

	private readonly bool _wasParentFacingLeft;

	private readonly Appendage[] _boneBlades = new Appendage[4];

	private readonly Appendage[] _fleshHalos = new Appendage[2];

	private bool _isDoneGrowing;

	private float _growthTimer;

	public EyeOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell, LunaisObj parentLunais)
		: base(inLevel, inPosition, inSide, -1, parentLunais, parentSpell)
	{
		_sprite = _level.GCM.SpOrbMeleeEye;
		_power = baseDamage;
		_force = 4;
		_life = 1.5f;
		base.DamageTimeoutTime = 0.15f;
		_damageElement = EDamageElement.Sharp;
		_doesUseAppendageCollision = false;
		_doesDrawBaseSprite = false;
		base.AnchorOffset = new Point(0, -24);
		_wasParentFacingLeft = parentLunais.IsFacingLeft;
		for (int i = 0; i < 2; i++)
		{
			Appendage appendage = new Appendage(this, new Point(32, 64), Point.Zero, _level, _sprite)
			{
				Rotation = (float)i * (float)Math.PI,
				DrawOrigin = new Vector2(0f, 32f),
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(16, 36)
			};
			appendage.ChangeAnimation(25);
			_appendages.Add(appendage);
			_fleshHalos[i] = appendage;
		}
		for (int j = 0; j < 4; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(128, 42), Point.Zero, _level, _sprite)
			{
				Rotation = (float)j * ((float)Math.PI / 2f),
				DrawOrigin = new Vector2(_wasParentFacingLeft ? 148 : (-20), 21f),
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(-83, 24),
				DoesDrawTrail = true,
				TrailLength = 5,
				TrailFadeRate = 2f,
				IsFacingLeft = _wasParentFacingLeft
			};
			appendage2.ChangeAnimation(24);
			_appendages.Add(appendage2);
			_boneBlades[j] = appendage2;
		}
	}

	public override void Update(float delta)
	{
		UpdateRotations(delta);
		if (base.CanDamageEnemies && _life < 0.4f)
		{
			base.CanDamageEnemies = false;
		}
		if (!_isDoneGrowing)
		{
			_growthTimer += delta;
			float num = _growthTimer / 0.5f;
			if (num >= 1f)
			{
				_isDoneGrowing = true;
				num = 1f;
			}
			float num2 = num * 2f;
			bool flag = num2 < 1f;
			Color glowColor = (flag ? new Color(1f, 1f, 1f, num2) : Color.White);
			float num3 = (float)Math.Sin(num * ((float)Math.PI / 2f));
			foreach (Appendage appendage in _appendages)
			{
				appendage.Scale = num3;
				appendage.IsGlowing = flag;
				appendage.GlowColor = glowColor;
				appendage.GlowBase = 12f;
			}
			base.DamageDimensions = new Point((int)(num3 * 256f), (int)(num3 * 256f));
		}
		base.Update(delta);
	}

	private void UpdateRotations(float delta)
	{
		float num = _life / 1.5f;
		float num2 = delta * (float)Math.Sin(num * ((float)Math.PI / 2f)) * -10f;
		if (!_wasParentFacingLeft)
		{
			num2 = 0f - num2;
		}
		Appendage[] boneBlades = _boneBlades;
		foreach (Appendage appendage in boneBlades)
		{
			appendage.Rotation += num2;
			if (appendage.Rotation > (float)Math.PI * 2f)
			{
				appendage.Rotation -= (float)Math.PI * 2f;
			}
			if (appendage.Rotation < 0f)
			{
				appendage.Rotation += (float)Math.PI * 2f;
			}
		}
		Appendage[] fleshHalos = _fleshHalos;
		foreach (Appendage appendage2 in fleshHalos)
		{
			appendage2.Rotation -= num2;
			if (appendage2.Rotation > (float)Math.PI * 2f)
			{
				appendage2.Rotation -= (float)Math.PI * 2f;
			}
			if (appendage2.Rotation < 0f)
			{
				appendage2.Rotation += (float)Math.PI * 2f;
			}
		}
	}
}
