using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class WindOrbPassive : LunaisPassive
{
	private readonly PassiveBuffSparkleParticleSystem _sparklesParticleSystem;

	private static readonly Vector4 PassiveColor = new Vector4(0.2f, 0.75f, 0.5f, 1f);

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Wind;

	public WindOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_sparklesParticleSystem = new PassiveBuffSparkleParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = PassiveColor
		};
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

	internal override void OnMeleeEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBBox)
	{
		if (damageArea != null && base.ParentLunais.Aura > 5)
		{
			base.ParentLunais.Aura -= 5;
			damageArea.DamageMultiplier *= 1.5f;
		}
		base.OnMeleeEnemyContact(enemy, damageArea, contactBBox);
	}
}
