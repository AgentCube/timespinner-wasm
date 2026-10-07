using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal abstract class LunaisPassive : LunaisOrbAbility
{
	private readonly LunaisObj _parentLunais;

	public abstract EInventoryOrbType PassiveType { get; }

	public LunaisObj ParentLunais => _parentLunais;

	protected LunaisPassive(LunaisObj parentLunais)
		: base(Point.Zero, parentLunais.Level, 0)
	{
		_parentLunais = parentLunais;
		base.SlotType = EOrbSlot.Passive;
	}

	public static LunaisPassive FromPassiveType(EInventoryOrbType passiveType, LunaisObj parentLunais)
	{
		LunaisPassive result = null;
		switch (passiveType)
		{
		case EInventoryOrbType.Blue:
			result = new BlueOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Blade:
			result = new BladeOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Flame:
			result = new FlameOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Pink:
			result = new PinkOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Iron:
			result = new IronOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Ice:
			result = new IceOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Wind:
			result = new WindOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Gun:
			result = new GunOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Umbra:
			result = new UmbraOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Empire:
			result = new EmpireOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Eye:
			result = new EyeOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Blood:
			result = new BloodOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Book:
			result = new BookOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Moon:
			result = new MoonOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Nether:
			result = new NetherOrbPassive(parentLunais);
			break;
		case EInventoryOrbType.Barrier:
			result = new BarrierOrbPassive(parentLunais);
			break;
		}
		return result;
	}

	public override void Update(float delta)
	{
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
	}

	public virtual void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
	}

	public virtual void ChangeRoom()
	{
	}

	public virtual void Unequip()
	{
	}

	public virtual int ManageDamage(int power, EDamageType type)
	{
		return power;
	}

	internal virtual void OnMeleeEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBBox)
	{
	}

	internal virtual void OnSuccessfulMeleeEnemyHit(Alive enemy)
	{
	}

	internal virtual void OnAttackWhenAllOrbsAreBusy()
	{
	}

	internal virtual void OnRefreshStats(GameSave inSave)
	{
	}

	internal virtual void OnEnemyDeath()
	{
	}

	internal virtual void OnTeleportToPoint(Point newPosition)
	{
	}
}
