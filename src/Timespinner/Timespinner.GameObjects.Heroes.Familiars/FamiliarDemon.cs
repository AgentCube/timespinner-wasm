using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal sealed class FamiliarDemon : FamiliarBase
{
	private const float IdleAnimationSpeed = 0.1f;

	private const float AttackAnimationSpeed = 0.05f;

	private const float TimeBeforeCreatingDamageArea = 0.15f;

	private const float TimeForEntireMeleeAttack = 0.35f;

	private const float SpellCastAnimationSpeed = 0.1f;

	private const float TimeIntoSpellAnimationBeforeCasting = 0.5f;

	private const float TimeIntoSpellAnimationBeforeEnding = 0.7f;

	private static readonly Color DemonAuraColor = new Color(0.6f, 0.25f, 0.6f, 0.25f);

	private readonly FamiliarDemonSpellDamageArea _spellDamageArea;

	public FamiliarDemon(Point inPosition, Level inLevel, LunaisObj parentObject, SpriteSheet inSprite, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, parentObject, EInventoryFamiliarType.Demon, switchFamiliar)
	{
		_bboxOffset = new Point(5, 5);
		_spellDamageArea = new FamiliarDemonSpellDamageArea(_level, Position, Vector2.Zero, ETeamSide.Heroes, _sprite, this, base.Damage);
		base.DoesDrawAura = true;
		base.BaseAuraColor = DemonAuraColor;
		base.AuraColor = base.BaseAuraColor;
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
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
				ChangeAnimation(new AnimationSpec[4]
				{
					new AnimationSpec
					{
						Start = 17,
						Length = 2,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 18,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 19,
						Length = 3,
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
			else if (_abilityTimer >= 0.15f && _lastAbilityTimer < 0.15f)
			{
				_level.AddProjectile(new FamiliarDemonMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.Damage, this));
			}
			else if (_abilityTimer > 0.35f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_abilityTimer <= 0f)
			{
				if (!_spellDamageArea.IsFinished && _spellDamageArea.HasBeenAdded)
				{
					_spellDamageArea.SilentKill();
				}
				_spellDamageArea.Position = Bbox.Center;
				_spellDamageArea.PlayCue(ESFX.EnemyDemonCast);
				ChangeAnimation(new AnimationSpec[2]
				{
					new AnimationSpec
					{
						Start = 21,
						Length = 7,
						Speed = 0.1f,
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
			else if (_abilityTimer >= 0.5f && _lastAbilityTimer < 0.5f)
			{
				if (base.SpellTargetMonster != null || base.IsPlayable)
				{
					Point center = Bbox.Center;
					Point point = ((base.SpellTargetMonster == null || base.IsPlayable) ? new Point(center.X + (IsFacingLeft ? (-10) : 10), center.Y) : base.SpellTargetMonster.Bbox.Center);
					Vector2 iV = new Vector2(point.X - center.X, point.Y - center.Y);
					iV.Normalize();
					iV *= 100f;
					int spellDamage = (int)Math.Ceiling((float)base.Damage * 2f);
					_spellDamageArea.Reset(center, iV, spellDamage);
					_level.AddProjectile(_spellDamageArea);
					base.SpellTargetMonster = null;
				}
			}
			else if (_abilityTimer >= 0.7f)
			{
				EndAbility();
			}
			break;
		}
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
}
