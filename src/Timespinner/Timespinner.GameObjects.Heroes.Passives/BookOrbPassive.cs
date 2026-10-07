using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class BookOrbPassive : LunaisPassive
{
	private const float TimeToRestoreHealth = 3f;

	private readonly PassiveBuffSparkleParticleSystem _sparklesParticleSystem;

	private static readonly Vector4 PassiveColor = new Vector4(1f, 0.75f, 0.5f, 1f);

	private int _healthRestoreAmount;

	private float _healthRestoreTimer;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Book;

	public BookOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_sparklesParticleSystem = new PassiveBuffSparkleParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = PassiveColor
		};
		SetHealthRegenRate();
	}

	private void SetHealthRegenRate()
	{
		float num = 0.25f;
		if (_level.GameSave.Inventory.OrbInventory.Inventory.ContainsKey(13))
		{
			int level = _level.GameSave.Inventory.OrbInventory.Inventory[13].Level;
			num += (float)level * 0.025f;
		}
		_healthRestoreAmount = (int)Math.Ceiling(num * (float)base.ParentLunais.MaxHP * 0.01f);
	}

	public override void Update(float delta)
	{
		_sparklesParticleSystem.AddParticles(base.ParentLunais.Position.ToVector2());
		_sparklesParticleSystem.Update(delta);
		if (!_level.IsPlayerInputBlocked)
		{
			_healthRestoreTimer += delta;
			if (_healthRestoreTimer >= 3f)
			{
				_healthRestoreTimer -= 3f;
				DoRestoreHealth();
			}
		}
		base.Update(delta);
	}

	private void DoRestoreHealth()
	{
		if (base.ParentLunais.HP < base.ParentLunais.MaxHP)
		{
			base.ParentLunais.ManageHeal(_healthRestoreAmount, shouldShowAnimation: false);
		}
	}

	public override void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		Level level = base.ParentLunais.Level;
		_sparklesParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
	}

	public override void ChangeRoom()
	{
		_sparklesParticleSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}

	internal override void OnRefreshStats(GameSave inSave)
	{
		SetHealthRegenRate();
	}

	public override void Unequip()
	{
		base.ParentLunais.AuraRegenRate = 1.25f;
		base.Unequip();
	}
}
