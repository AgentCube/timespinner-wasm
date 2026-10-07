using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.LunaisParticleEffects;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class FlameOrbPassive : LunaisPassive
{
	private readonly SmallBurningParticleSystem _mainBurningParticleSystem;

	private readonly SmallBurningParticleSystem _subBurningParticleSystem;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Flame;

	public FlameOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_mainBurningParticleSystem = new SmallBurningParticleSystem(base.ParentLunais.Level.GCM.SpOrbMeleeFire, 5);
		_subBurningParticleSystem = new SmallBurningParticleSystem(base.ParentLunais.Level.GCM.SpOrbMeleeFire, 5);
	}

	public override void Update(float delta)
	{
		LunaisOrb mainOrb = base.ParentLunais.MainOrb;
		if (mainOrb != null)
		{
			_mainBurningParticleSystem.AddParticles(mainOrb.ParticleEmissionPoint.ToVector2());
		}
		LunaisOrb subOrb = base.ParentLunais.SubOrb;
		if (subOrb != null)
		{
			_subBurningParticleSystem.AddParticles(subOrb.ParticleEmissionPoint.ToVector2());
		}
		_mainBurningParticleSystem.Update(delta);
		_subBurningParticleSystem.Update(delta);
		base.Update(delta);
	}

	internal override void OnSuccessfulMeleeEnemyHit(Alive enemy)
	{
		int orbPassiveDamage = _level.GameSave.GetOrbPassiveDamage(EInventoryOrbType.Flame);
		enemy?.GiveStatusEffect(EStatusEffectType.Burn, orbPassiveDamage);
		base.OnSuccessfulEnemyHit(enemy);
	}

	public override void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		Level level = base.ParentLunais.Level;
		if (isMainOrb)
		{
			_mainBurningParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
		}
		else
		{
			_subBurningParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
		}
		Draw(spriteBatch);
	}

	public override void ChangeRoom()
	{
		_mainBurningParticleSystem.KillOffParticles(0f);
		_subBurningParticleSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}
}
