using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._14_Gyre;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreKain : Monster
{
	private const int DeathPushOffset = 32;

	private const float TimeDeadBeforeBurning = 0.5f;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _throwSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly GyreKainScytheDamageArea _scythe;

	private Point _deathPoint;

	public GyreKain(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		_doAppendagesMatchImageFacing = true;
		base.DoesDrawAppendageAuras = true;
		_doesUse16X16TileCollisionBbox = false;
		_movementType = EAIMovementType.Fly;
		_attackDistanceThresholdX = 128;
		_retreatDistanceThresholdX = 60;
		_currentAI = EAIStrategy.FollowAttack;
		_isGrounded = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = true;
		base.FollowAttackTargetOffset = new Point(0, 32);
		_agility = 0.25f;
		_timeToMove = 0.5f;
		_timeToIdleAfterAttacking = 2f;
		_bboxOffset = new Point(1, 0);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 37);
		base.CannotBeGrabbed = true;
		base.DoesTouchDamageKnockback = true;
		SetDoesDrawAppendageTrails(value: true, isHost: true, 6, 3f);
		base.DoesDrawAura = true;
		base.AuraColor = Color.Purple * 0.5f;
		base.AuraSize = 0.075f;
		base.AuraFrequency = 9f;
		base.AuraOffset = new Vector2(2f, 4f);
		_scythe = new GyreKainScytheDamageArea(_level, Position, ETeamSide.Enemies, this, _sprite, base.Damage);
		_level.RequestAddObject(_scythe);
		ChangeAnimation(5);
		_idleSequence = GetCharacterSequenceByName("Idle");
		_throwSequence = GetCharacterSequenceByName("Throw");
		_turnSequence = GetCharacterSequenceByName("Turn");
		_deathSequence = GetCharacterSequenceByName("Death");
		SetCharacterSequence(_idleSequence);
		_timeToTurnAround = 0.075f;
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_turnSequence != null)
		{
			SetCharacterSequence(_turnSequence);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			PlayCue(ESFX.EnemyKainAttackPrep);
			SetCharacterSequence(_throwSequence);
			_scythe.DoThrowWindup(IsFacingLeft);
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer > 0f && deathScriptTimer <= 0f)
		{
			DropLoot();
			PlayCue(ESFX.EnemyRoyalGuardDeathCry);
			SetCharacterSequence(_deathSequence);
			_scythe.KillScythe();
			_deathPoint = Position;
		}
		if (_deathScriptTimer > 0.5f)
		{
			DistintegrateEvent newEvent = new DistintegrateEvent(this, _sprite, EDisintegrateType.Chaos, 1, 4, new Vector4(0.8f, 0.3f, 0.9f, 1f));
			_level.AddEvent(newEvent);
			RemoveInstance();
			return;
		}
		float num = _deathScriptTimer / 0.5f;
		int num2 = (int)(Math.Sin(num * ((float)Math.PI / 2f)) * 32.0);
		Position = new Point(_deathPoint.X + (IsFacingLeft ? num2 : (-num2)), _deathPoint.Y + (int)((float)(-num2) * 0.5f));
		SnapBboxToPosition();
		SnapFrameToBbox();
		UpdateCharacterSequences(delta);
		UpdateAnimation(delta);
		UpdateAppendages(delta);
		UpdateTrail(delta);
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification.IntArgument == 0)
		{
			_scythe.DoThrow(IsFacingLeft);
		}
	}
}
