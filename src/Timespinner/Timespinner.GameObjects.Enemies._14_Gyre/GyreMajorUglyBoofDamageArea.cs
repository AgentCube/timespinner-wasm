using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._14_Gyre;

internal class GyreMajorUglyBoofDamageArea : DamageArea
{
	private static readonly Color ShockwaveColor = new Color(0.5f, 0.4f, 0.3f, 0.25f);

	private readonly BoofWaveAnimation _shockwaveAnimation;

	private int _radius;

	public GyreMajorUglyBoofDamageArea(Level inLevel, Point inPosition, int inDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = _level.GCM.SpOrbMeleeBarrier;
		base.DoesDrawBaseSprite = false;
		_life = 0.297f;
		base.DamageTimeoutTime = _life;
		_power = inDamage;
		base.DoesKnockBack = true;
		_damageElement = EDamageElement.Blunt;
		_isAffectedByGravity = false;
		_shockwaveAnimation = new BoofWaveAnimation(_sprite, inPosition, inLevel, ShockwaveColor);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_shockwaveAnimation.AnimationIndex <= 6 && _shockwaveAnimation.AnimationIndex > 0)
			{
				_radius = _shockwaveAnimation.FrameSource.Width;
				base.DamageDimensions = new Point(_radius * 2, _radius * 2);
				base.CanDamageEnemies = true;
				base.CanDamageThings = true;
			}
			else
			{
				base.CanDamageEnemies = false;
				base.CanDamageThings = false;
			}
		}
		base.Update(delta);
	}

	internal void Reset(Point position)
	{
		_isFading = false;
		_fadeTimer = 0f;
		base.CanDamageEnemies = true;
		base.CanDamageThings = true;
		Position = position;
		_shockwaveAnimation.Reset(position, isFacingLeft: true);
		_life = 0.297f;
		_level.AddAnimation(_shockwaveAnimation);
		base.DamageDimensions = new Point(8, 8);
		Update(0f);
	}
}
