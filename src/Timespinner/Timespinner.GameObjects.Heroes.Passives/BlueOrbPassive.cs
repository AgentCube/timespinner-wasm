using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class BlueOrbPassive : LunaisPassive
{
	private const int MaxFrenzyLevel = 10;

	private const int FrenzyHitCountNextLevelThreshold = 5;

	private const float TimeBeforeFrenzyDecay = 3f;

	private const float TimeBeforeEmittingParticles = 0.33f;

	private readonly LunaisBluePassiveParticleSystem _passiveParticleSystem;

	private static readonly Vector4 PassiveColor = new Vector4(0.5f, 0.5f, 0.75f, 1f);

	private int _frenzyLevel;

	private int _frenzyHitCount;

	private float _timeSinceLastAttack;

	private float _damageMultiplier;

	private float _particleEmissionTimer;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Blue;

	public BlueOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_passiveParticleSystem = new LunaisBluePassiveParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 25)
		{
			BaseColor = PassiveColor
		};
	}

	public override void Update(float delta)
	{
		if (_frenzyHitCount > 0 || _frenzyLevel > 0)
		{
			_timeSinceLastAttack += delta;
			if (_timeSinceLastAttack >= 3f)
			{
				_frenzyLevel--;
				if (_frenzyLevel < 0)
				{
					_frenzyLevel = 0;
				}
				_frenzyHitCount = 0;
				_timeSinceLastAttack = 0f;
				RefreshDamageMultiplier();
			}
		}
		_particleEmissionTimer += delta * (float)_frenzyLevel;
		if (_particleEmissionTimer >= 0.33f)
		{
			_particleEmissionTimer = 0f;
			_passiveParticleSystem.AddParticles(base.ParentLunais.Position.ToVector2());
		}
		_passiveParticleSystem.Update(delta);
		base.Update(delta);
	}

	private void RefreshDamageMultiplier()
	{
		_damageMultiplier = ((_frenzyLevel > 0) ? ((float)_frenzyLevel / 10f) : 0f) * 1f;
	}

	public override void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		Level level = base.ParentLunais.Level;
		_passiveParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
	}

	public override void ChangeRoom()
	{
		_passiveParticleSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}

	internal override void OnMeleeEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBBox)
	{
		if (damageArea != null)
		{
			_timeSinceLastAttack = 0f;
			if (_frenzyLevel < 10)
			{
				_frenzyHitCount++;
				if (_frenzyHitCount > 5)
				{
					_frenzyHitCount = 0;
					_frenzyLevel++;
					RefreshDamageMultiplier();
				}
			}
			damageArea.DamageMultiplier *= 1f + _damageMultiplier;
		}
		base.OnMeleeEnemyContact(enemy, damageArea, contactBBox);
	}

	public override int ManageDamage(int power, EDamageType type)
	{
		return (int)Math.Ceiling((float)power * (1f + _damageMultiplier));
	}
}
