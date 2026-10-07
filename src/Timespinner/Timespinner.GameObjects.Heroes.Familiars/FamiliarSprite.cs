using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal class FamiliarSprite : FamiliarBase
{
	private const float IdleAnimationSpeed = 0.07f;

	private const float FlyingAnimationSpeed = 0.05f;

	private const float TimeBeforeCastingHealSpell25 = 10f;

	private const float TimeBeforeCastingHealSpell50 = 20f;

	private const float TimeBeforeCastingHealSpell75 = 30f;

	private const float TimeBeforeCastingHealSpell90 = 40f;

	private const float HealthHealPercentThreshold25 = 0.25f;

	private const float HealthHealPercentThreshold50 = 0.5f;

	private const float HealthHealPercentThreshold75 = 0.75f;

	private const float HealthHealPercentThreshold90 = 0.9f;

	private const float TimeIntoSpellAnimationBeforeHealing = 0.4f;

	private const float AttackAnimationSpeed = 0.05f;

	private const float TimeBeforeCreatingDamageArea = 0.2f;

	private const float TimeForEntireMeleeAttack = 0.35f;

	private readonly Alive _livingParentObject;

	private readonly SparklesParticleSystem _healSparkleParticles;

	public FamiliarSprite(Point inPosition, Level inLevel, LunaisObj parentObject, SpriteSheet inSprite, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, parentObject, EInventoryFamiliarType.Sprite, switchFamiliar)
	{
		_bboxOffset = new Point(7, 5);
		base.TimeBeforeCastingSpell = 20f;
		base.SpellCastTimer = base.TimeBeforeCastingSpell;
		_livingParentObject = parentObject;
		_healSparkleParticles = new SparklesParticleSystem(_sprite, 5, 32, 3);
		_particleSystems.Add(_healSparkleParticles);
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			if (base.LastFamiliarAIState == EFamiliarAIStateType.Sitting)
			{
				ChangeAnimation(0, 6, 0.07f, EAnimationType.Cycle, 16, 4, 0.07f);
				OnEndSit();
			}
			else
			{
				ChangeAnimation(0, 6, 0.07f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Running:
			ChangeAnimation(6, 6, 0.05f, EAnimationType.Cycle);
			break;
		}
		base.SetState(state);
	}

	protected override void DoSittingAnimation()
	{
		ChangeAnimation(12, 4, 0.1f, EAnimationType.Once);
	}

	public override void CarryOutAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[4]
				{
					new AnimationSpec
					{
						Start = 20,
						Length = 2,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 21,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 22,
						Length = 3,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 6,
						Speed = 0.07f,
						Type = EAnimationType.Cycle,
						InitialIndex = 5
					}
				});
			}
			else if (_abilityTimer >= 0.2f && _lastAbilityTimer < 0.2f)
			{
				_level.AddProjectile(new FamiliarSpriteMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.Damage, this));
			}
			else if (_abilityTimer > 0.35f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[2]
				{
					new AnimationSpec
					{
						Start = 26,
						Length = 4,
						Speed = 0.1f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 6,
						Speed = 0.07f,
						Type = EAnimationType.Cycle
					}
				});
			}
			else if (_abilityTimer >= 0.4f && _lastAbilityTimer < 0.4f)
			{
				if (_livingParentObject != null)
				{
					_livingParentObject.ManageHeal((float)base.Damage * 3f, shouldShowAnimation: true);
					BattleAnimation battleAnimation = new BattleAnimation(null, _livingParentObject.Bbox.Center, _level);
					battleAnimation.ParticleSystem = _healSparkleParticles;
					battleAnimation.TeamSide = ETeamSide.Heroes;
					BattleAnimation newAnimation = battleAnimation;
					_level.AddAnimation(newAnimation);
					_level.PlayCue(ESFX.FamiliarHealSpell, _livingParentObject.Bbox.Center);
					base.SpellTargetMonster = null;
				}
				EndAbility();
			}
			break;
		}
	}

	protected override void DecideNextAction(float delta)
	{
		bool flag = false;
		if (base.SpellCastTimer <= 0f && _livingParentObject != null && _livingParentObject.HPPercentage <= 0.9f && base.FamiliarAIState != EFamiliarAIStateType.Sitting)
		{
			flag = true;
			float hPPercentage = _livingParentObject.HPPercentage;
			if (hPPercentage <= 0.25f)
			{
				base.SpellCastTimer = 10f;
			}
			else if (hPPercentage <= 0.5f)
			{
				base.SpellCastTimer = 20f;
			}
			else if (hPPercentage <= 0.75f)
			{
				base.SpellCastTimer = 30f;
			}
			else
			{
				base.SpellCastTimer = 40f;
			}
		}
		if (flag)
		{
			base.FamiliarAIState = EFamiliarAIStateType.Casting;
			StartAbility(1);
		}
		else
		{
			base.DecideNextAction(delta);
		}
	}
}
