using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class PinkOrbPassive : LunaisPassive
{
	private readonly PassiveBuffSparkleParticleSystem _sparklesParticleSystem;

	private static readonly Vector4 PassiveColor = new Vector4(1f, 0.5f, 0.75f, 1f);

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Pink;

	public PinkOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_sparklesParticleSystem = new PassiveBuffSparkleParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = PassiveColor
		};
		SetAuraRegenRate();
	}

	private void SetAuraRegenRate()
	{
		float num = 1.2f;
		if (_level.GameSave.Inventory.OrbInventory.Inventory.ContainsKey(4))
		{
			int level = _level.GameSave.Inventory.OrbInventory.Inventory[4].Level;
			num += (float)level * 0.025f;
		}
		base.ParentLunais.AuraRegenRate = 1.25f * num;
	}

	public override void Update(float delta)
	{
		_sparklesParticleSystem.AddParticles(base.ParentLunais.Position.ToVector2());
		_sparklesParticleSystem.Update(delta);
		base.Update(delta);
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
		SetAuraRegenRate();
	}

	public override void Unequip()
	{
		base.ParentLunais.AuraRegenRate = 1.25f;
		base.Unequip();
	}
}
