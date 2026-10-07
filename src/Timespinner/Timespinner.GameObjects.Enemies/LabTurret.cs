using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LabTurret : Monster
{
	private const int BoltCount = 3;

	private const int ProjectileSpeed = 450;

	private const int TargetClampOffset = 12;

	private const float MaxWheelTurnSpeed = 0.033f;

	private const float TimeToCharge = 1f;

	private const float TimeToWaitAfterShooting = 2f;

	private const float TimeBetweenShots = 0.1f;

	private const float TimeToFireAllShots = 0.3f;

	private readonly Point _trackTop;

	private readonly Point _trackBottom;

	private readonly Point _trackTop16;

	private readonly Point _trackBottom16;

	private readonly Point _startPoint;

	private readonly LunaisChargeParticleSystem _chargeParticleSystem;

	private readonly Appendage _eyeAppendage;

	private readonly Appendage _wheelAppendage;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _eyeCloseSequence;

	private readonly CharacterSequenceSpecification _eyeOpenSequence;

	private readonly CharacterSequenceSpecification _eyeBlinkSequence;

	private readonly LabTurretBolt[] _bolts = new LabTurretBolt[3];

	private bool _isChargingUp;

	private bool _isCoolingDown;

	private bool _isDoneDying;

	private float _wheelTurnSpeed = 0.05f;

	private float _chargeTimer;

	private float _cooldownTimer;

	public LabTurret(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_startPoint = inPosition;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		_bboxOffset = new Point(4, 0);
		Bbox = new Rectangle(_position.X, _position.Y, 12, 16);
		base.DoesDrawWhenOutsideOfObjectVisibleArea = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByFriction = true;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_maxMoveSpeed = 100 + _level.NextRandomInt(0, 50);
		Position = new Point(Position.X + (IsFacingLeft ? (-4) : 3), Position.Y);
		_trackTop16 = FindTrackEnd(EDirection.North, _startPoint);
		_trackBottom16 = FindTrackEnd(EDirection.South, _startPoint);
		_trackTop = _trackTop16.Multiply(16f);
		_trackBottom = _trackBottom16.Multiply(16f);
		PlaceTracks(_trackTop16, _trackBottom16);
		if (base.CharacterSpecification != null && base.Appendages.Count > 2)
		{
			_wheelAppendage = base.Appendages[0];
			_eyeAppendage = base.Appendages[2];
		}
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 3)
		{
			_idleSequence = base.CharacterSpecification.Sequences[0];
			_eyeCloseSequence = base.CharacterSpecification.Sequences[1];
			_eyeOpenSequence = base.CharacterSpecification.Sequences[2];
			_eyeBlinkSequence = base.CharacterSpecification.Sequences[3];
			SetCharacterSequence(_idleSequence);
		}
		for (int i = 0; i < 3; i++)
		{
			_bolts[i] = new LabTurretBolt(_level, Position, ETeamSide.Enemies, _sprite, base.Damage);
		}
		_chargeParticleSystem = new LunaisChargeParticleSystem(_level.GCM.TxParticleEnergy, 2)
		{
			BaseColor = Color.Red.ToVector4()
		};
		_particleSystems.Add(_chargeParticleSystem);
	}

	private Point FindTrackEnd(EDirection direction, Point startPosition)
	{
		Point point = new Point(startPosition.X / 16, startPosition.Y / 16);
		Point result = point;
		bool flag = false;
		while (point.Y >= 0 && point.Y < _level.RoomSize16.Y)
		{
			Point key = new Point(point.X + (IsFacingLeft ? 1 : (-1)), point.Y);
			bool flag2 = _level.ForegroundTiles.ContainsKey(point);
			if (_level.SolidTiles.ContainsKey(point) || (flag && flag2) || (!_level.SolidTiles.ContainsKey(key) && !_level.ForegroundTiles.ContainsKey(key)))
			{
				break;
			}
			result = point;
			point = Level.GetPointFromDirection(point, direction);
			flag = flag2;
		}
		return result;
	}

	private void PlaceTracks(Point top, Point bottom)
	{
		if (top.Y == bottom.Y)
		{
			top = new Point(top.X, top.Y - 2);
		}
		for (int i = top.Y; i <= bottom.Y; i++)
		{
			bool flag = i == top.Y;
			bool flag2 = i == bottom.Y;
			bool flag3 = i % 2 == 0;
			Appendage appendage = new Appendage(this, new Point(16, 16), Point.Zero, _level, _sprite);
			appendage.DoesCollideWithAnything = false;
			appendage.IsFlippedVertically = flag2;
			appendage.IsFacingLeft = IsFacingLeft;
			appendage.FollowType = EAppendageFollowType.None;
			appendage.Position = new Point(_startPoint.X, (i + 1) * 16);
			appendage.DrawPriority = -1;
			appendage.DoesInheritDrawColor = false;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation((flag || flag2) ? 13 : (flag3 ? 15 : 14));
			_appendages.Add(appendage2);
		}
	}

	internal override void InitializeForBestiary()
	{
		SetCharacterSequenceByName("EyeOpenInstant");
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		float movementY = _movementY;
		if (!_isAggroed)
		{
			_movementY = 0f;
		}
		else
		{
			if (_isChargingUp)
			{
				float chargeTimer = _chargeTimer;
				_chargeTimer += delta;
				if (_chargeTimer >= 1f)
				{
					float num = _chargeTimer - 1f;
					float num2 = chargeTimer - 1f;
					for (int i = 0; i < 3; i++)
					{
						float num3 = (float)i * 0.1f;
						if (num > num3 && num2 <= num3)
						{
							FireBolt(i);
							break;
						}
					}
					if (num >= 0.3f)
					{
						_isCoolingDown = true;
						_isChargingUp = false;
						if (_eyeAppendage != null)
						{
							_eyeAppendage.IsGlowing = false;
						}
						if (_eyeBlinkSequence != null)
						{
							SetCharacterSequence(_eyeBlinkSequence);
						}
					}
				}
				else if (_eyeAppendage != null)
				{
					float num4 = (float)Math.Cos(_chargeTimer / 1f * ((float)Math.PI / 2f));
					float num5 = num4;
					_eyeAppendage.GlowBase = 2f;
					_eyeAppendage.IsGlowing = true;
					_eyeAppendage.GlowColor = new Color(1f, num5, num5, num4);
					_chargeParticleSystem.AddParticles(_eyeAppendage.Bbox.Center.ToVector2());
				}
			}
			else if (_isCoolingDown)
			{
				_cooldownTimer += delta;
				if (_cooldownTimer >= 2f)
				{
					_isCoolingDown = false;
				}
			}
			else
			{
				_isChargingUp = true;
				_chargeTimer = 0f;
				_cooldownTimer = 0f;
				PlayCue(ESFX.EnemyLabTurretCharge);
			}
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			nearestProtagonistPosition = new Point(nearestProtagonistPosition.X, (int)MathHelper.Clamp(nearestProtagonistPosition.Y + 8, _trackTop.Y + 12, _trackBottom.Y + 12));
			int num6 = Math.Abs(nearestProtagonistPosition.Y - Position.Y);
			if (num6 > 8)
			{
				if (nearestProtagonistPosition.Y > Position.Y)
				{
					_movementY = 1f;
				}
				else
				{
					_movementY = -1f;
				}
			}
			else
			{
				_movementY = 0f;
			}
		}
		if (_wheelAppendage != null)
		{
			float num7 = Math.Abs(_velocity.Y);
			_wheelTurnSpeed = 0.033f;
			if (_wheelTurnSpeed > 10000f)
			{
				_wheelTurnSpeed = 10000f;
			}
			if (movementY != _movementY)
			{
				_wheelAppendage.ChangeAnimation(new AnimationSpec
				{
					Start = 6,
					Length = 4,
					Speed = _wheelTurnSpeed,
					Type = EAnimationType.Cycle,
					IsInReverse = (_movementY < 0f)
				});
			}
			else
			{
				_wheelAppendage.AnimationSpeed = ((num7 < 1f) ? 10000f : _wheelTurnSpeed);
			}
		}
	}

	protected override void OnAggroed()
	{
		if (_eyeOpenSequence != null)
		{
			SetCharacterSequence(_eyeOpenSequence);
		}
		base.OnAggroed();
	}

	protected override void OnDeAggroed()
	{
		if (_eyeCloseSequence != null)
		{
			SetCharacterSequence(_eyeCloseSequence);
		}
		if (_eyeAppendage != null)
		{
			_eyeAppendage.IsGlowing = false;
		}
		if (_wheelAppendage != null)
		{
			_wheelAppendage.ChangeAnimation(6);
		}
		_isCoolingDown = false;
		_isCharging = false;
		_chargeTimer = 0f;
		base.OnDeAggroed();
	}

	private void FireBolt(int index)
	{
		int num = ((!IsFacingLeft) ? 1 : (-1));
		Point position = ((_eyeAppendage != null) ? _eyeAppendage.Bbox.Center : Bbox.Center);
		position.Y++;
		Vector2 iV = new Vector2(num * 450, 0f);
		LabTurretBolt labTurretBolt = _bolts[index];
		labTurretBolt.Reset(position, iV);
		_level.AddProjectile(labTurretBolt);
		PlayCue(ESFX.EnemyLabTurretShoot, position);
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_isDoneDying)
		{
			return;
		}
		if (_appendages.Any())
		{
			List<Appendage> list = new List<Appendage>();
			for (int num = base.Appendages.Count - 1; num > 3; num--)
			{
				list.Add(base.Appendages[num]);
				base.Appendages.RemoveAt(num);
			}
			_level.AddAnimation(EBattleAnimationType.Boom, Bbox.Center, ETeamSide.Enemies);
			DebrisEvent.CreateFromObject(this, Vector2.One, Bbox.Center, _sprite);
			_appendages.Clear();
			foreach (Appendage item in list)
			{
				base.Appendages.Add(item);
			}
		}
		DropLoot();
		KillButLeaveCorpse();
		_isDoneDying = true;
	}
}
