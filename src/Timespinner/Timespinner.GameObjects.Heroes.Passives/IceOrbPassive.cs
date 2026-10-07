using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.LunaisParticleEffects;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class IceOrbPassive : LunaisPassive
{
	private readonly SmallSnowingParticleSystem _mainSnowflakesParticleSystem;

	private readonly SmallSnowingParticleSystem _subSnowflakesParticleSystem;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Ice;

	public IceOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_mainSnowflakesParticleSystem = new SmallSnowingParticleSystem(base.ParentLunais.Level.GCM.TxParticleEnergy, 4);
		_subSnowflakesParticleSystem = new SmallSnowingParticleSystem(base.ParentLunais.Level.GCM.TxParticleEnergy, 4);
	}

	public override void Update(float delta)
	{
		LunaisOrb mainOrb = base.ParentLunais.MainOrb;
		if (mainOrb != null)
		{
			_mainSnowflakesParticleSystem.AddParticles(mainOrb.ParticleEmissionPoint.ToVector2());
			if (mainOrb.IsAtAttackApex)
			{
				ShootBullet(mainOrb);
			}
		}
		LunaisOrb subOrb = base.ParentLunais.SubOrb;
		if (subOrb != null)
		{
			_subSnowflakesParticleSystem.AddParticles(subOrb.ParticleEmissionPoint.ToVector2());
			if (subOrb.IsAtAttackApex)
			{
				ShootBullet(subOrb);
			}
		}
		_mainSnowflakesParticleSystem.Update(delta);
		_subSnowflakesParticleSystem.Update(delta);
		base.Update(delta);
	}

	public override void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		Level level = base.ParentLunais.Level;
		if (isMainOrb)
		{
			_mainSnowflakesParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
		}
		else
		{
			_subSnowflakesParticleSystem.Draw(spriteBatch, level.LevelRenderCenter, level.CameraPosition, level.CameraZoom);
		}
		Draw(spriteBatch);
	}

	public override void ChangeRoom()
	{
		_mainSnowflakesParticleSystem.KillOffParticles(0f);
		_subSnowflakesParticleSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}

	private void ShootBullet(LunaisOrb emissionOrb)
	{
		Level level = emissionOrb.Level;
		bool flag = !emissionOrb.IsThrowingLeft;
		Vector2 value = new Vector2(flag ? 1f : (-1f), 0f);
		Point position = emissionOrb.Position;
		value = Vector2.Multiply(value, 500f);
		int orbPassiveDamage = _level.GameSave.GetOrbPassiveDamage(EInventoryOrbType.Ice);
		level.AddProjectile(new IceOrbPassiveProjectile(level, position, value, ETeamSide.Heroes, this, orbPassiveDamage));
		level.PlayCue(ESFX.LunaisOrbPassiveIce, position);
	}
}
