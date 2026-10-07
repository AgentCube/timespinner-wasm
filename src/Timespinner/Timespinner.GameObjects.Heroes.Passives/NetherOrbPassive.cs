using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class NetherOrbPassive : LunaisPassive
{
	private readonly PassiveBuffSparkleParticleSystem _sparklesParticleSystem;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Nether;

	public NetherOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		Vector4 baseColor = new Vector4(0.55f, 0.7f, 0.65f, 1f);
		_sparklesParticleSystem = new PassiveBuffSparkleParticleSystem(parentLunais.Level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = baseColor
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
		if (damageArea != null)
		{
			float hPPercentage = base.ParentLunais.HPPercentage;
			float num = 1f + (1f - hPPercentage) * 2f;
			damageArea.DamageMultiplier *= num;
		}
		base.OnMeleeEnemyContact(enemy, damageArea, contactBBox);
	}
}
