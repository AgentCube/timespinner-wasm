using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CursedMushroomMinion : Monster
{
	private const int Anim_SpawnStart = 16;

	private const int Anim_MoveStart = 20;

	private const int Anim_DieStart = 24;

	private const int Anim_Length = 4;

	public CursedMushroomMinion(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec, int baseHP, int baseDamage)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		base.MaxHP = (int)Math.Ceiling(0.1f * (float)baseHP);
		base.HP = base.MaxHP;
		_damageCaused = (int)Math.Ceiling(0.75f * (float)baseDamage);
		base.IsMinion = true;
		_currentAI = EAIStrategy.RunForever;
		_doesDropBasicLoot = false;
		_bboxOffset = Point.Zero;
		Bbox = new Rectangle(_position.X, _position.Y, 8, 8);
		_agility = 0.2f;
		_isAlwaysAggroed = true;
		base.Appendages.Clear();
		base.CannotBeGrabbed = true;
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 16,
				Length = 4,
				Speed = 0.05f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 20,
				Length = 4,
				Speed = 0.05f,
				Type = EAnimationType.Cycle
			}
		});
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		target.GiveStatusEffect(EStatusEffectType.Poison, 0);
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	protected override void UpdateDeathScript(float delta)
	{
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, Bbox.Center, _level);
		battleAnimation.AnimationStart = 24;
		battleAnimation.AnimationLength = 4;
		battleAnimation.AnimationSpeed = 0.07f;
		battleAnimation.TeamSide = ETeamSide.Enemies;
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
		base.IsDead = true;
		SilentKill();
	}
}
