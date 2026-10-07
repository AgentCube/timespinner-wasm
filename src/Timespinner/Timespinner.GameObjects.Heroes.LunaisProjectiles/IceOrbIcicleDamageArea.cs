using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class IceOrbIcicleDamageArea : LunaisBaseOrbDamageArea
{
	private const float MaxLife = 0.8f;

	private const float TimeToRise = 0.15f;

	private readonly Point _startingPoint;

	private readonly Rectangle _baseFrameSource;

	private bool _isAtTop;

	private float _riseTimer;

	public IceOrbIcicleDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inDamage, int inID, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, inSide, inID, null, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeIce;
		int num = _level.NextRandomInt(0, 3);
		int num2 = ((num % 2 == 1) ? 11 : 29);
		IsFacingLeft = num < 2;
		_damageElement = EDamageElement.Ice;
		_baseFrameSource = _sprite.GetFrameSource(num2);
		_startingPoint = Position;
		_life = 0.8f;
		base.DamageTimeoutTime = 0.7f;
		_power = inDamage;
		ChangeAnimation(num2);
		PlayCue(ESFX.LunaisOrbIceGrow);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isAtTop)
		{
			float num = 1f;
			_riseTimer += delta;
			if (_riseTimer < 0.15f)
			{
				num = _riseTimer / 0.15f;
			}
			else
			{
				_isAtTop = true;
			}
			int num2 = (int)((1.0 - Math.Cos((float)Math.PI / 2f * num)) * (double)_baseFrameSource.Height);
			base.DamageDimensions = new Point(_baseFrameSource.Width, num2);
			_frameSource = new Rectangle(_baseFrameSource.X, _baseFrameSource.Y, _baseFrameSource.Width, num2);
			Position = new Point(_startingPoint.X, _startingPoint.Y - num2 / 2);
		}
		base.Update(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			AnimationStart = 12,
			AnimationLength = 4,
			IsFacingLeft = IsFacingLeft,
			TeamSide = base.TeamSide
		});
		_level.PlayCue(ESFX.LunaisOrbImpactIce, intersectionCenter);
	}
}
