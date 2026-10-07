using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class MoonOrbPassive : LunaisPassive
{
	private readonly MoonOrbPassiveBubbleAppendage _bubbleAppendage;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Moon;

	public MoonOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_bubbleAppendage = new MoonOrbPassiveBubbleAppendage(parentLunais, _level, _level.GCM.SpOrbMeleeMoon);
		_bubbleAppendage.SetTimer(base.ParentLunais.BarrierPassiveTimer);
	}

	public override void Update(float delta)
	{
		float barrierPassiveTimer = _bubbleAppendage.UpdateTimer(delta);
		base.ParentLunais.BarrierPassiveTimer = barrierPassiveTimer;
		_bubbleAppendage.Update(delta);
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_bubbleAppendage.Draw(spriteBatch);
		base.Draw(spriteBatch);
	}

	public override void Unequip()
	{
		_bubbleAppendage.SilentKill();
		base.Unequip();
	}

	public override int ManageDamage(int power, EDamageType type)
	{
		int result = power;
		if (_bubbleAppendage.IsAvailable)
		{
			result = _bubbleAppendage.ShieldDamage(power);
		}
		return result;
	}
}
