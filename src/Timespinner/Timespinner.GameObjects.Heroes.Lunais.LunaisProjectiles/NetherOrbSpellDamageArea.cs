using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles;

internal sealed class NetherOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	private static readonly Color ShockwaveColor = new Color(0.66f, 0.5f, 0.85f, 0.65f);

	private readonly ShockwaveAnimation _shockwaveAnimation;

	private int _radius;

	public NetherOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inDamage, LunaisOrbAbility parentOrb, SpriteSheet sprite)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_sprite = sprite;
		base.DoesDrawBaseSprite = false;
		_life = 0.297f;
		base.DamageTimeoutTime = 1f;
		_power = inDamage;
		_damageElement = EDamageElement.Dark;
		_isAffectedByGravity = false;
		_shockwaveAnimation = new ShockwaveAnimation(sprite, inPosition, inLevel, ShockwaveColor);
		_level.AddAnimation(_shockwaveAnimation);
		PlayCue(ESFX.LunaisOrbSlashLight);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_shockwaveAnimation.AnimationIndex <= 6)
			{
				_radius = _shockwaveAnimation.FrameSource.Width;
				base.DamageDimensions = new Point(_radius * 2, _radius * 2);
			}
			else
			{
				base.CanDamageEnemies = false;
			}
		}
		base.Update(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		if (_radius > 0)
		{
			Point center = Bbox.Center;
			if (!new Circle(center, _radius).ContainsPoint(intersectionCenter))
			{
				Vector2 vector = new Vector2(center.X - intersectionCenter.X, center.Y - intersectionCenter.Y);
				vector.Normalize();
				intersectionCenter = new Point(center.X - (int)(vector.X * (float)_radius), center.Y - (int)(vector.Y * (float)_radius));
			}
		}
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, intersectionCenter, _level);
		battleAnimation.TeamSide = ETeamSide.Enemies;
		battleAnimation.AnimationStart = 19;
		battleAnimation.AnimationLength = 5;
		battleAnimation.DrawColor = Color.White * 0.75f;
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
		_level.PlayCue(ESFX.LunaisOrbImpactIce, intersectionCenter);
	}
}
