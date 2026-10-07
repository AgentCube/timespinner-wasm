using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.FamiliarParticleEffects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;
using Timespinner.GameObjects.Items;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal class FamiliarCrow : FamiliarBase
{
	private const float IdleAnimationSpeed = 0.1f;

	private const float AttackAnimationSpeed = 0.05f;

	private const float TimeBeforeCreatingDamageArea = 0.2f;

	private const float TimeForEntireMeleeAttack = 0.4f;

	private const float SpellCastAnimationSpeed = 0.075f;

	private const float TimeIntoSpellAnimationBeforeCasting = 0.15f;

	private const float TimeIntoSpellAnimationBeforeEnding = 0.75f;

	private readonly FamiliarCrowSporeParticleSystem _sporeParticles;

	private FamiliarBaseDamageArea _currentSpellDamageArea;

	public FamiliarCrow(Point inPosition, Level inLevel, LunaisObj parentObject, SpriteSheet inSprite, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, parentObject, EInventoryFamiliarType.MerchantCrow, switchFamiliar)
	{
		_bboxOffset = new Point(5, 5);
		_sporeParticles = new FamiliarCrowSporeParticleSystem(_sprite, 10);
		_particleSystems.Add(_sporeParticles);
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			if (base.LastFamiliarAIState == EFamiliarAIStateType.Sitting)
			{
				ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle, 14, 3, 0.1f);
				OnEndSit();
			}
			else
			{
				ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Running:
			ChangeAnimation(5, 5, 0.1f, EAnimationType.Cycle);
			break;
		}
		base.SetState(state);
	}

	protected override void DoSittingAnimation()
	{
		ChangeAnimation(10, 4, 0.1f, EAnimationType.Once);
	}

	public override void CarryOutAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[5]
				{
					new AnimationSpec
					{
						Start = 17,
						Length = 3,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 19,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 20,
						Length = 2,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 17,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 5,
						Speed = 0.1f,
						Type = EAnimationType.Cycle,
						InitialIndex = 4
					}
				});
			}
			else if (_abilityTimer >= 0.2f && _lastAbilityTimer < 0.2f)
			{
				_level.AddProjectile(new FamiliarCrowMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.Damage, this));
			}
			else if (_abilityTimer > 0.4f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[3]
				{
					new AnimationSpec
					{
						Start = 22,
						Length = 5,
						Speed = 0.075f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 22,
						Length = 5,
						Speed = 0.075f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 5,
						Speed = 0.1f,
						Type = EAnimationType.Cycle
					}
				});
			}
			else if (_abilityTimer >= 0.15f && _lastAbilityTimer < 0.15f)
			{
				Point inPosition = ((base.SpellTargetMonster == null) ? Position : base.SpellTargetMonster.Bbox.Center);
				int inDamage = (int)Math.Ceiling((float)base.Damage * 2f);
				_currentSpellDamageArea = new FamiliarCrowSpellDamageArea(_level, inPosition, ETeamSide.Heroes, base.SpellTargetMonster, inDamage, this);
				_level.AddProjectile(_currentSpellDamageArea);
				_level.PlayCue(ESFX.FamiliarCoinSpell, Position);
				base.SpellTargetMonster = null;
			}
			else if (_abilityTimer >= 0.75f)
			{
				EndAbility();
				_currentSpellDamageArea = null;
			}
			if (_abilityTimer >= 0.15f && _currentSpellDamageArea != null)
			{
				Vector2 where = new Vector2(_currentSpellDamageArea.Position.X, _currentSpellDamageArea.Bbox.Top);
				_sporeParticles.AddParticles(where);
			}
			break;
		}
	}

	internal override void OnSuccessfulEnemyHit(Alive enemy, bool isMelee)
	{
		if (enemy.HP <= 0)
		{
			int gemAmountFromLotteryRoll = GemItem.GetGemAmountFromLotteryRoll((float)_level.NextRandomDouble());
			_level.AddItem(EItemType.Money, gemAmountFromLotteryRoll, enemy.Bbox.Center, _level.NextObjectTicketID);
		}
		base.OnSuccessfulEnemyHit(enemy, isMelee);
	}

	protected override void DecideNextAction(float delta)
	{
		bool flag = false;
		if (!_level.IsTimeFrozen && base.SpellCastTimer <= 0f && base.FamiliarAIState != EFamiliarAIStateType.Sitting && TargetNearestEnemy())
		{
			flag = true;
			base.SpellCastTimer = base.TimeBeforeCastingSpell;
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

	internal override void ChangeSkin(bool isAltSkin)
	{
		_sprite = (isAltSkin ? _level.GCM.SpFamiliarAltCrow : _level.GCM.SpFamiliarCrow);
	}
}
