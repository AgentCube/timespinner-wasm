using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class EmpireOrbPassive : LunaisPassive
{
	private const int LeashTimeMultiplier = 30;

	private const float TimeToFadeOutAfterAttacking = 0.1f;

	private const float MaxOrbCollisionLeash = 22f;

	private bool _isVisible;

	private EInventoryOrbType _orbType;

	private int _orbAttackType;

	private float _fadeOutTimer;

	private Vector2 _lastOffset;

	private LunaisOrb _ghostOrb;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Empire;

	public EmpireOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		InitializeGhostOrb();
	}

	private void InitializeGhostOrb()
	{
		LunaisOrb lunaisOrb = base.ParentLunais.MainOrb ?? base.ParentLunais.SubOrb;
		if (lunaisOrb != null)
		{
			if (_ghostOrb != null)
			{
				_ghostOrb.DisposeOrb();
			}
			_ghostOrb = LunaisOrbManager.CreateOrbFromType(lunaisOrb.OrbColor, _level, base.ParentLunais, base.ParentLunais.Bbox.Center, 0f, isMainOrb: true);
			_orbType = _ghostOrb.OrbColor;
			_orbAttackType = (int)_orbType;
			_ghostOrb.ShadowDamageMultiplier = 1f;
			_ghostOrb.UpdateDamage(_level.GameSave.GetOrbDamage(_orbType));
			_ghostOrb.IsThirdOrb = true;
		}
	}

	internal override void OnRefreshStats(GameSave inSave)
	{
		if (inSave != null)
		{
			EInventoryOrbType eInventoryOrbType = ((inSave.Inventory.EquippedMeleeOrbA != 0) ? inSave.Inventory.EquippedMeleeOrbA : inSave.Inventory.EquippedMeleeOrbB);
			if (eInventoryOrbType != _orbType)
			{
				InitializeGhostOrb();
			}
			else if (_ghostOrb != null)
			{
				int orbDamage = _level.GameSave.GetOrbDamage(_ghostOrb.OrbColor);
				_ghostOrb.UpdateDamage(orbDamage);
			}
		}
	}

	internal override void OnAttackWhenAllOrbsAreBusy()
	{
		if (_ghostOrb != null && !_ghostOrb.IsAttacking)
		{
			_isVisible = true;
			_fadeOutTimer = 0f;
			_ghostOrb.IsThrowingLeft = base.ParentLunais.IsFacingLeft;
			_ghostOrb.StartMeleeAttack(_orbAttackType);
		}
	}

	public override void ChangeRoom()
	{
		if (_ghostOrb != null)
		{
			_ghostOrb.ChangeRoom();
		}
		base.ChangeRoom();
	}

	public override void Update(float delta)
	{
		if (_ghostOrb != null)
		{
			Point orbCenterTarget = GetOrbCenterTarget(delta);
			_ghostOrb.Update(delta, orbCenterTarget);
			if (_isVisible && !_ghostOrb.IsAttacking)
			{
				_fadeOutTimer += delta;
				if (_fadeOutTimer >= 0.1f)
				{
					_isVisible = false;
				}
			}
		}
		base.Update(delta);
	}

	private Point GetOrbCenterTarget(float delta)
	{
		Point bulletOffset = base.ParentLunais.GetBulletOffset(22f);
		_lastOffset = _lastOffset.EaseTo(bulletOffset.ToVector2(), delta * 30f);
		Point point = _lastOffset.ToPoint();
		return new Point(base.ParentLunais.Position.X + point.X, base.ParentLunais.Position.Y + point.Y);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_isVisible = true;
		if (_ghostOrb != null && _isVisible)
		{
			_ghostOrb.DefaultDrawColor = Color.White;
			_ghostOrb.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}

	internal override void OnTeleportToPoint(Point newPosition)
	{
		if (_ghostOrb != null)
		{
			_ghostOrb.ClearTrailHistory();
		}
	}

	public override void Unequip()
	{
		if (_ghostOrb != null)
		{
			_ghostOrb.DisposeOrb();
		}
		base.Unequip();
	}

	internal override void ShiftTrailHistory(Point offset)
	{
		if (_ghostOrb != null)
		{
			_ghostOrb.ShiftTrailHistory(offset);
		}
		base.ShiftTrailHistory(offset);
	}
}
