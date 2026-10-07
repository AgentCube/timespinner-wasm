using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.LunaisParticleEffects;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class BloodOrbPassive : LunaisPassive
{
	private const float MaxEmissionDelay = 2f;

	private static readonly Vector4 PassiveColor = new Vector4(1f, 0.2f, 0.2f, 1f);

	private readonly SmallBloodParticleSystem _mainBloodParticleSystem;

	private readonly SmallBloodParticleSystem _subBloodParticleSystem;

	private float _healthCounter;

	private float _mainEmissionTimer;

	private float _subEmissionTimer;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Blood;

	public BloodOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_mainBloodParticleSystem = new SmallBloodParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 1)
		{
			BaseColor = PassiveColor
		};
		_subBloodParticleSystem = new SmallBloodParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 1)
		{
			BaseColor = PassiveColor
		};
	}

	public override void Update(float delta)
	{
		LunaisOrb mainOrb = base.ParentLunais.MainOrb;
		if (mainOrb != null)
		{
			_mainEmissionTimer -= delta;
			if (_mainEmissionTimer <= 0f)
			{
				_mainBloodParticleSystem.AddParticles(mainOrb.ParticleEmissionPoint.ToVector2());
				_mainEmissionTimer = (float)(_level.NextRandomDouble() * 2.0);
			}
		}
		LunaisOrb subOrb = base.ParentLunais.SubOrb;
		if (subOrb != null)
		{
			_subEmissionTimer -= delta;
			if (_subEmissionTimer <= 0f)
			{
				_subBloodParticleSystem.AddParticles(subOrb.ParticleEmissionPoint.ToVector2());
				_subEmissionTimer = (float)(_level.NextRandomDouble() * 2.0);
			}
		}
		_mainBloodParticleSystem.Update(delta);
		_subBloodParticleSystem.Update(delta);
		base.Update(delta);
	}

	public override void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		Level level = base.ParentLunais.Level;
		if (isMainOrb)
		{
			_mainBloodParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
		}
		else
		{
			_subBloodParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
		}
	}

	public override void ChangeRoom()
	{
		_mainBloodParticleSystem.KillOffParticles(0f);
		_subBloodParticleSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}

	internal override void OnSuccessfulMeleeEnemyHit(Alive enemy)
	{
		float num = 0.3f;
		if (_level.GameSave.Inventory.OrbInventory.Inventory.ContainsKey(12))
		{
			int level = _level.GameSave.Inventory.OrbInventory.Inventory[12].Level;
			num += (float)level * 0.05f;
		}
		if (num > 1f)
		{
			num = 1f + (num - 1f) * 0.5f;
		}
		_healthCounter += num;
		if (_healthCounter >= 1f)
		{
			int num2 = (int)Math.Floor(_healthCounter);
			_healthCounter -= num2;
			base.ParentLunais.ManageHeal(num2, shouldShowAnimation: false);
		}
	}
}
