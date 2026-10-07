using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal class WindOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	private const int ChildAppendageCount = 4;

	private const int PieceSize = 16;

	private const int MaxGrowthWidth = 80;

	private const int MaxGrowthHeight = 80;

	private const float TimeToGrow = 0.25f;

	private const float MaxLife = 3f;

	private readonly List<WindSpellAppendage> _windChildren = new List<WindSpellAppendage>();

	private bool _isDoneGrowing;

	private float _growthTimer;

	internal bool IsFinished => _isFading;

	public WindOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell, LunaisObj parentLunais)
		: base(inLevel, inPosition, inSide, -1, parentLunais, parentSpell)
	{
		_sprite = _level.GCM.SpOrbMeleeWind;
		_doesUseAppendageCollision = false;
		_power = baseDamage;
		_force = 4;
		_life = 3f;
		base.DamageTimeoutTime = 0.15f;
		_damageElement = EDamageElement.Sharp;
		_doesDrawBaseSprite = false;
		base.AnchorOffset = new Point(0, -16);
		for (int i = 0; i < 4; i++)
		{
			WindSpellAppendage item = new WindSpellAppendage(this, new Point(16, 16), Point.Zero, _level, _sprite, i)
			{
				FollowType = EAppendageFollowType.AnchorLocked
			};
			base.Appendages.Add(item);
			_windChildren.Add(item);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isDoneGrowing)
			{
				_growthTimer += delta;
				float num = _growthTimer / 0.25f;
				if (num >= 1f)
				{
					_isDoneGrowing = true;
					num = 1f;
				}
				float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
				base.DamageDimensions = new Point((int)(num2 * 80f), (int)(num2 * 80f));
			}
			if (_isFading && _timeToFade > 0f)
			{
				foreach (WindSpellAppendage windChild in _windChildren)
				{
					windChild.FadeMultiplier = 1f - _fadeTimer / _timeToFade;
				}
			}
		}
		base.Update(delta);
	}
}
