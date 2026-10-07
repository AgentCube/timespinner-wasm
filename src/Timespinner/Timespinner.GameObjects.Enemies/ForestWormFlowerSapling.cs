using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestWormFlowerSapling : Monster
{
	public ForestWormFlowerSapling(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec, int baseHP, int baseDamage)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		base.MaxHP = (int)Math.Ceiling(0.33f * (float)baseHP);
		base.HP = base.MaxHP;
		_damageCaused = (int)Math.Ceiling(0.75f * (float)baseDamage);
		base.IsMinion = true;
		_currentAI = EAIStrategy.None;
		_bboxOffset = new Point(2, 0);
		_doesDropBasicLoot = false;
		Bbox = new Rectangle(_position.X, _position.Y, 4, 16);
		base.CannotBeGrabbed = true;
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 18,
				Length = 3,
				Speed = 0.05f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 21,
				Length = 3,
				Speed = 0.15f,
				Type = EAnimationType.PingPong
			}
		});
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.WetSplashSmall, _bbox.Center, ETeamSide.Enemies);
		DropLootAndRemove();
	}
}
