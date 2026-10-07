using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class BladeOrbPassive : LunaisPassive
{
	private const float BladeIdleDamageTimeout = 1f;

	private const float BladeIdleAnimationSpeed = 0.05f;

	private readonly Appendage _mainBladeAppendage;

	private readonly Appendage _subBladeAppendage;

	private LunaisOrb _mainMeleeOrb;

	private LunaisOrb _subMeleeOrb;

	private GreenOrbPassiveDamageArea _mainDamageArea;

	private GreenOrbPassiveDamageArea _subDamageArea;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Blade;

	public BladeOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		Level level = base.ParentLunais.Level;
		SpriteSheet spOrbMeleeBlade = level.GCM.SpOrbMeleeBlade;
		_mainBladeAppendage = new Appendage(base.ParentLunais, new Rectangle(0, 0, 16, 16), new Point(0, -8), level, spOrbMeleeBlade);
		_subBladeAppendage = new Appendage(base.ParentLunais, new Rectangle(0, 0, 16, 16), new Point(0, -8), level, spOrbMeleeBlade);
		_mainBladeAppendage.ChangeAnimation(25, 4, 0.05f, EAnimationType.Cycle);
		_subBladeAppendage.ChangeAnimation(25, 4, 0.05f, EAnimationType.Cycle);
		_mainBladeAppendage.Position = new Point(-16, -16);
		_subBladeAppendage.Position = new Point(-16, -16);
	}

	public override void Update(float delta)
	{
		if (base.ParentLunais.MainOrb != _mainMeleeOrb)
		{
			_mainMeleeOrb = base.ParentLunais.MainOrb;
			Level level = base.ParentLunais.Level;
			if (_mainDamageArea != null)
			{
				level.RequestRemoveObject(_mainDamageArea);
			}
			_mainDamageArea = new GreenOrbPassiveDamageArea(level, base.ParentLunais.Position, _mainMeleeOrb)
			{
				DamageTimeoutTime = 1f
			};
			level.AddProjectile(_mainDamageArea);
		}
		if (base.ParentLunais.SubOrb != _subMeleeOrb)
		{
			_subMeleeOrb = base.ParentLunais.SubOrb;
			Level level2 = base.ParentLunais.Level;
			if (_subDamageArea != null)
			{
				level2.RequestRemoveObject(_subDamageArea);
			}
			_subDamageArea = new GreenOrbPassiveDamageArea(level2, base.ParentLunais.Position, _subMeleeOrb)
			{
				DamageTimeoutTime = 1f
			};
			level2.AddProjectile(_subDamageArea);
		}
		if (_mainMeleeOrb != null)
		{
			_mainBladeAppendage.Update(delta);
			_mainBladeAppendage.Position = _mainMeleeOrb.OrbPassiveCenter;
		}
		if (_subMeleeOrb != null)
		{
			_subBladeAppendage.Update(delta);
			_subBladeAppendage.Position = _subMeleeOrb.OrbPassiveCenter;
		}
		base.Update(delta);
	}

	public override void ChangeRoom()
	{
		Level level = base.ParentLunais.Level;
		if (_mainMeleeOrb != null && _mainDamageArea != null)
		{
			level.AddProjectile(_mainDamageArea);
		}
		if (_subMeleeOrb != null && _subDamageArea != null)
		{
			level.AddProjectile(_subDamageArea);
		}
		base.ChangeRoom();
	}

	public override void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		if (isMainOrb)
		{
			if (_mainMeleeOrb != null)
			{
				_mainBladeAppendage.Draw(spriteBatch);
			}
		}
		else if (_subMeleeOrb != null)
		{
			_subBladeAppendage.Draw(spriteBatch);
		}
		base.DrawUnderOrb(spriteBatch, isMainOrb);
	}

	internal override void OnRefreshStats(GameSave inSave)
	{
		int orbPassiveDamage = inSave.GetOrbPassiveDamage(PassiveType);
		if (_mainDamageArea != null)
		{
			_mainDamageArea.Power = orbPassiveDamage;
		}
		if (_subDamageArea != null)
		{
			_subDamageArea.Power = orbPassiveDamage;
		}
	}

	public override void Unequip()
	{
		if (_mainDamageArea != null)
		{
			_mainDamageArea.Kill();
		}
		if (_subDamageArea != null)
		{
			_subDamageArea.Kill();
		}
		base.Unequip();
	}
}
