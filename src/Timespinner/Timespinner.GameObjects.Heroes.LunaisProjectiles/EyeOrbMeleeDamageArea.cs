using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class EyeOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const int MinDamageRadius = 10;

	private const int MaxDamageRadius = 48;

	private const float MaxLife = 1f;

	private const float GrowTime = 0.2f;

	private const float FullTime = 0.7f;

	private const float ShrinkTime = 71f / (226f * (float)Math.PI);

	public EyeOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		_sprite = null;
		_doesDrawSpriteAndAppendages = false;
		_damageDimensions = new Point(10, 10);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 1f;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		PlayCue(ESFX.LunaisOrbEyeWhiff);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(EBattleAnimationType.MediumHitYellow, intersectionCenter, _teamSide, _anchorObject.Position.X < target.Position.X, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisOrbImpactSharp, intersectionCenter);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			float num = 1f - _life;
			if (num < 0.2f)
			{
				float amount = num / 0.2f;
				int num2 = (int)MathHelper.Lerp(10f, 48f, amount);
				base.DamageDimensions = new Point(num2, num2);
			}
			else if (num < 0.9f)
			{
				base.DamageDimensions = new Point(48, 48);
			}
			else if (num < 1f)
			{
				float amount2 = (num - 0.9f) / (71f / (226f * (float)Math.PI));
				int num3 = (int)MathHelper.Lerp(48f, 10f, amount2);
				base.DamageDimensions = new Point(num3, num3);
			}
			else
			{
				base.DamageDimensions = new Point(10, 10);
			}
		}
		base.Update(delta);
	}
}
