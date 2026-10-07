using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal class CavesMushroomSporeDamageArea : DamageArea
{
	private const int DamageHeight = 48;

	private const int DamageWidthStart = 32;

	private const float DamageWidthGrowthRate = 100f;

	private int _targetDamageWidth;

	private float _currentDamageWidth;

	public CavesMushroomSporeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int damage)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
	{
		base.AnchorOffset = new Point(0, -24);
		_doesDrawSpriteAndAppendages = false;
		base.Power = damage;
		base.Life = 0f;
		_currentDamageWidth = 32f;
		base.DamageDimensions = new Point(32, 48);
	}

	public void Refresh(int damageWidth, bool isFrozen)
	{
		_isFrozen = isFrozen;
		_targetDamageWidth = damageWidth;
		if ((float)_targetDamageWidth < _currentDamageWidth)
		{
			_currentDamageWidth = 32f;
		}
		base.Life = 100f;
		_isFading = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _currentDamageWidth < (float)_targetDamageWidth)
		{
			_currentDamageWidth += delta * 100f;
			if (_currentDamageWidth > (float)_targetDamageWidth)
			{
				_currentDamageWidth = _targetDamageWidth;
			}
			base.DamageDimensions = new Point((int)_currentDamageWidth, 48);
		}
		base.Update(delta);
	}

	public override bool DetermineDamage(Alive target, Rectangle collidingBbox)
	{
		bool flag = base.DetermineDamage(target, collidingBbox);
		if (flag)
		{
			_level.PlayCue(ESFX.EnemyMushroomTowerSporeHit, target.Position);
			target.GiveStatusEffect(EStatusEffectType.Poison, 0);
		}
		return flag;
	}
}
