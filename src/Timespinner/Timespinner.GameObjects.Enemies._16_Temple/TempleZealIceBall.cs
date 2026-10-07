using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleZealIceBall : Appendage
{
	private readonly BattleAnimation _flashAnimation;

	internal Point DeathPoint { get; set; }

	internal Color DeathColor { get; set; }

	internal Color DeathTrailColor { get; set; }

	public TempleZealIceBall(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		ChangeAnimation(15);
		DrawOrigin = new Vector2(8f, 8f);
		base.DoesCollideWithAnything = false;
		base.DoesInheritDrawColor = false;
		_flashAnimation = new BattleAnimation(_sprite, Position, _level)
		{
			AnimationStart = 11,
			AnimationLength = 4,
			DoesFadeOut = true
		};
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 10;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 15;
		_trailShrinkRate = 0.00033f;
		_trailLength = 75;
	}

	internal void AddFlash()
	{
		_flashAnimation.Reset(Bbox.Center, isFacingLeft: true);
		AddBattleAnimation(_flashAnimation);
	}
}
