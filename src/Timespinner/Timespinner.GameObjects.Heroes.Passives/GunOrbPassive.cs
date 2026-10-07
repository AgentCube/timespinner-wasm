using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class GunOrbPassive : LunaisPassive
{
	private readonly PassiveBuffSparkleParticleSystem _sparklesParticleSystem;

	private static readonly Vector4 PassiveColor = new Vector4(0.85f, 0.5f, 0.75f, 1f);

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Gun;

	public GunOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_sparklesParticleSystem = new PassiveBuffSparkleParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = PassiveColor
		};
		base.ParentLunais.AuraCostMultiplier = 0.5f;
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

	public override void Unequip()
	{
		base.ParentLunais.AuraCostMultiplier = 1f;
		base.Unequip();
	}
}
