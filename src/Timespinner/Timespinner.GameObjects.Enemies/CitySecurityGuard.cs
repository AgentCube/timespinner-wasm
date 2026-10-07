using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._02_Metropolis;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CitySecurityGuard : Monster
{
	private const int ThrowDistanceThreshold = 80;

	private const float BatonDamageLife = 0.033f;

	private const float TimeToWindUp = 1f;

	private const float TimeToStopSliding = 1.15f;

	private static readonly Point BatonDamageDimensions = new Point(32, 16);

	private readonly int _batonPower;

	private readonly int _grenadeBouncePower;

	private readonly int _grenadeExplodePower;

	private readonly CharacterSequenceSpecification _attackSequence;

	private readonly CharacterSequenceSpecification _throwSequence;

	private Point BatonDamageOffset => new Point(IsFacingLeft ? (-18) : 18, -32);

	public CitySecurityGuard(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = objectSpec != null && !objectSpec.IsFlippedHorizontally;
		_currentAI = EAIStrategy.StandAttack;
		_agility = 0.25f;
		_timeToIdleAfterAttacking = 1f;
		_batonPower = (int)Math.Ceiling((float)base.Damage * 1.25f);
		_grenadeBouncePower = (int)Math.Ceiling((float)base.Damage * 0.5f);
		_grenadeExplodePower = (int)Math.Ceiling((float)base.Damage * 1.5f);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 2)
		{
			SetCharacterSequenceByName("Idle");
			_attackSequence = GetCharacterSequenceByName("Attack");
			_throwSequence = GetCharacterSequenceByName("Throw");
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Bbox.Center);
			int num = Math.Abs(nearestProtagonistPosition.X - Position.X);
			int num2 = nearestProtagonistPosition.Y - Position.Y;
			if (num2 <= -72)
			{
				_isCarryingOutAbility = false;
				_currentAction = EAIAction.Idle;
			}
			else if (num > 80)
			{
				PlayCue(ESFX.EnemySecGuardGrenadePrep);
				SetCharacterSequence(_throwSequence);
			}
			else
			{
				PlayCue(ESFX.EnemySecGuardSlashPrep);
				SetCharacterSequence(_attackSequence);
			}
		}
		if (_abilityTimer > 1.15f)
		{
			_movementX = 0f;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
		{
			PlayCue(ESFX.EnemySecGuardSlash, Position);
			_movementX = ((!IsFacingLeft) ? 1 : (-1));
			DamageArea damageArea = new DamageArea(_level, Position, base.DefaultTeam, -1, this);
			damageArea.DamageDimensions = BatonDamageDimensions;
			damageArea.Power = _batonPower;
			damageArea.AnchorOffset = BatonDamageOffset;
			damageArea.Life = 0.033f;
			damageArea.DoesKnockBack = true;
			DamageArea newProjectile2 = damageArea;
			_level.AddProjectile(newProjectile2);
			break;
		}
		case 1:
			_movementX = 0f;
			break;
		case 2:
		{
			int num = _level.GetNearestProtagonistPosition(Bbox.Center).Y - Position.Y;
			int num2 = 400;
			int num3 = -250;
			if (num > 0)
			{
				num3 = -150;
			}
			else if (num < -32)
			{
				num3 = -600;
				num2 = 600;
			}
			Vector2 iV = new Vector2(((!IsFacingLeft) ? 1 : (-1)) * num2, num3);
			Point inPosition = Bbox.Center.Add(IsFacingLeft ? (-16) : 16, 0);
			CitySecurityGuardGrenade newProjectile = new CitySecurityGuardGrenade(_level, _sprite, inPosition, iV, _grenadeBouncePower, _grenadeExplodePower);
			PlayCue(ESFX.EnemySecGuardGrenadeToss);
			_level.AddProjectile(newProjectile);
			break;
		}
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
	}
}
