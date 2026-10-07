using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies._04_Ramparts;

internal sealed class CastleLargeSoldier : Monster
{
	private const int HammerSpriteIndexOffset = 4;

	private const float WindupAnimationSpeed = 0.1f;

	private const float SwingAnimationSpeed = 0.066f;

	private const float RecoverAnimationSpeed = 0.1f;

	private const float TimeToWindup = 0.6f;

	private const float TimeForEntireAttack = 1.9300001f;

	private const float MaxDamageAreaLifetime = 0.1f;

	private static readonly Point DefaultHammerOffset = new Point(1, -76);

	private static readonly Point DamageAreaDimensions1 = new Point(48, 120);

	private static readonly Point DamageAreaDimensions2 = new Point(90, 90);

	private static readonly Point DamageAreaDimensions3 = new Point(114, 38);

	private static readonly Point DefaultDustParticleEmissionOffset = new Point(-76, 0);

	private readonly int _hammerDamage;

	private readonly DamageArea _hammerDamageArea;

	private readonly LandingDustParticleSystem _dustParticles;

	private readonly Appendage _hammer;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _attackSequence;

	private readonly CharacterSequenceSpecification _moveSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly Queue<HammerState> _hammerAnimationStates = new Queue<HammerState>();

	private bool _isDamageAreaActive;

	private float _hammerAnimationCounter;

	private Point _hammerOffset;

	internal Point HammerOffset
	{
		get
		{
			if (!IsImageFacingLeft)
			{
				return new Point(-_hammerOffset.X, _hammerOffset.Y);
			}
			return _hammerOffset;
		}
		set
		{
			_hammerOffset = value;
		}
	}

	internal Point DustParticleEmissionOffset
	{
		get
		{
			if (!IsImageFacingLeft)
			{
				return new Point(-DefaultDustParticleEmissionOffset.X, DefaultDustParticleEmissionOffset.Y);
			}
			return DefaultDustParticleEmissionOffset;
		}
	}

	public CastleLargeSoldier(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		ChangeAnimation(-1);
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_isAfraidOfFalling = false;
		_isAfraidOfBeingTooClose = true;
		_attackDistanceThresholdX = 128;
		_retreatDistanceThresholdX = 60;
		_timeToTurnAround = 0.15f;
		_agility = 0.2f;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		_hammerDamage = (int)Math.Ceiling((float)base.Damage * 1.25f);
		base.CannotBeGrabbed = true;
		base.DoesTouchDamageKnockback = true;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		if (base.CharacterSpecification != null)
		{
			if (base.Appendages.Count > 1)
			{
				_hammer = base.Appendages[1];
				_hammer.DoesCollideWithAnything = false;
			}
			if (base.CharacterSpecification.Sequences.Count > 4)
			{
				_idleSequence = base.CharacterSpecification.Sequences[0];
				_attackSequence = base.CharacterSpecification.Sequences[1];
				_moveSequence = base.CharacterSpecification.Sequences[2];
				_deathSequence = base.CharacterSpecification.Sequences[3];
				_turnSequence = base.CharacterSpecification.Sequences[4];
				SetCharacterSequence(_idleSequence);
			}
		}
		HammerOffset = DefaultHammerOffset;
		_hammerDamageArea = new DamageArea(_level, Position, base.DefaultTeam, -1, this)
		{
			Power = _hammerDamage,
			DamageDimensions = new Point(24, 24),
			Life = 0.1f,
			DamageTimeoutTime = 0.25f,
			DoesKnockBack = true
		};
		_dustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 100);
		_particleSystems.Add(_dustParticles);
		_hammerAnimationStates.Enqueue(new HammerState
		{
			Frame = 0,
			Offset = DefaultHammerOffset,
			Time = 0.1f,
			Rotation = 0f
		});
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		UpdateHammer(delta);
	}

	private void UpdateHammer(float delta)
	{
		bool flag = false;
		if (_hammer == null)
		{
			return;
		}
		if (!base.IsFrozen)
		{
			if (_hammerAnimationStates.Count > 0)
			{
				_hammerAnimationCounter -= delta;
				if (_hammerAnimationCounter <= 0f)
				{
					HammerState hammerState = _hammerAnimationStates.Dequeue();
					_hammerAnimationCounter = hammerState.Time + _hammerAnimationCounter;
					HammerOffset = hammerState.Offset;
					_hammer.ChangeAnimation(hammerState.Frame + 4);
					_hammer.Rotation = hammerState.Rotation;
					flag = true;
					if (!IsImageFacingLeft && _hammer.Rotation != 0f)
					{
						if (Math.Abs(_hammer.Rotation - (float)Math.PI) < 0.01f)
						{
							_hammerOffset.X -= 144;
						}
						else
						{
							_hammer.Rotation = 0f - _hammer.Rotation;
							_hammerOffset.X -= 21;
							_hammerOffset.Y -= 51;
						}
						_hammer.AnchorOffset = _hammerOffset;
					}
					if (hammerState.DamageDimensions == Point.Zero && _isDamageAreaActive)
					{
						_level.RequestRemoveObject(_hammerDamageArea);
						_isDamageAreaActive = false;
					}
					else if (hammerState.DamageDimensions != Point.Zero)
					{
						_hammerDamageArea.DamageDimensions = hammerState.DamageDimensions;
						int x = (24 - hammerState.DamageDimensions.X / 2) * (IsImageFacingLeft ? 1 : (-1));
						_hammerDamageArea.AnchorOffset = new Point(x, -hammerState.DamageDimensions.Y / 2);
						_hammerDamageArea.ExtendLife(0.1f);
						if (hammerState.DamageDimensions == DamageAreaDimensions3)
						{
							_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: true);
							_dustParticles.AddParticles(Position.Add(DustParticleEmissionOffset).ToVector2(), 100f);
						}
						if (!_isDamageAreaActive)
						{
							_isDamageAreaActive = true;
							_level.AddProjectile(_hammerDamageArea);
						}
					}
				}
			}
			else
			{
				_hammerAnimationCounter = 0f;
			}
		}
		_hammer.Position = Position.Add(HammerOffset);
		if (flag)
		{
			_hammer.Update(0f);
			return;
		}
		_hammer.SnapBboxToPosition();
		_hammer.SnapFrameToBbox();
	}

	public override void PostCollisionUpdate()
	{
		base.PostCollisionUpdate();
		UpdateHammer(0f);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Idle:
			SetCharacterSequence(_idleSequence);
			break;
		case EAFSM.Moving:
		case EAFSM.Running:
			SetCharacterSequence(_moveSequence);
			break;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		SetCharacterSequence(_turnSequence);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			PlayCue(ESFX.EnemyLargeSoldierHammer);
			SetCharacterSequence(_attackSequence);
		}
		if (_abilityTimer >= 1.9300001f)
		{
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
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 0,
				Offset = DefaultHammerOffset.Add(13, -18),
				Time = 0.1f
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(27, -14),
				Time = 0.1f
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(28, -14),
				Time = 0.1f
			});
			break;
		case 1:
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(5, 8),
				Time = 0.066f,
				Rotation = -(float)Math.PI / 4f,
				DamageDimensions = DamageAreaDimensions1
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(20, 39),
				Time = 0.066f,
				Rotation = (float)Math.PI,
				DamageDimensions = DamageAreaDimensions2
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(20, 40),
				Time = 0.066f,
				Rotation = (float)Math.PI,
				DamageDimensions = DamageAreaDimensions3
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(20, 40),
				Time = 0.066f,
				Rotation = (float)Math.PI
			});
			break;
		case 3:
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(1, 22),
				Time = 0.1f,
				Rotation = -(float)Math.PI / 4f
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 0,
				Offset = DefaultHammerOffset,
				Time = 0.1f,
				Rotation = 0f
			});
			PlayCue(ESFX.EnemyLargeSoldierReset);
			break;
		case 4:
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 1,
				Offset = DefaultHammerOffset.Add(1, 22),
				Time = 0.1f,
				Rotation = -(float)Math.PI / 4f
			});
			_hammerAnimationStates.Enqueue(new HammerState
			{
				Frame = 0,
				Offset = DefaultHammerOffset,
				Time = 0.1f,
				Rotation = 0f
			});
			break;
		case 5:
			_agility = 0.2f;
			break;
		case 6:
			_agility = 0.0001f;
			break;
		case 2:
			break;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			DropLoot();
			SetCharacterSequence(_deathSequence);
			Update(0f);
			if (_hammer.Rotation >= (float)Math.PI)
			{
				int num = (int)Math.Ceiling((float)_hammer.FrameSource.Width / 12f);
				int num2 = _hammer.Position.X - 65 - _hammer.FrameSource.Width / 2;
				for (int i = 0; i < num; i++)
				{
					_level.AddAnimation(EBattleAnimationType.DustBoom, new Point(num2 + i * 12, Position.Y), ETeamSide.Neutral, isFacingRight: true, i == 0);
				}
			}
			else
			{
				_hammer.Position = _hammer.Position.Add(0, 32);
				_hammer.ChangeBboxDimensions(new Point(_hammer.FrameSource.Width, _hammer.FrameSource.Height), new Point(0, 0));
				_hammer.SnapBboxToPosition();
				DebrisEvent.CreateFromObject(_hammer, _sprite, DebrisEvent.EDebrisDeathType.HeavyLand);
			}
			base.Appendages.Remove(_hammer);
			DistintegrateEvent newEvent = new DistintegrateEvent(this, _sprite, EDisintegrateType.Chaos, 41, 4, new Vector4(0.8f, 0.3f, 0.9f, 1f));
			_level.AddEvent(newEvent);
			RemoveInstance();
		}
		_deathScriptTimer += delta;
	}
}
