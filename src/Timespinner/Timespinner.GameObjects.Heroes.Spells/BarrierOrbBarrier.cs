using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal sealed class BarrierOrbBarrier : GameEvent
{
	private const int BarrierWidth = 24;

	private const int BarrierHeight = 64;

	private const int BarrierAnimationIndex = 25;

	private const float TimeToCreate = 0.3f;

	private const float TimeToLive = 7f;

	private const float TimeToFade = 0.3f;

	private const float TimeBeforeFading = 6.7f;

	private static readonly Color BaseDrawColor = new Color(0.85f, 0.8f, 0.65f, 0.8f);

	private static readonly Color RimDrawColor = new Color(0.8f, 0.75f, 0.65f, 0.75f);

	private static readonly Color BaseAuraColor = new Color(0.75f, 0.65f, 0.5f, 0.75f);

	private readonly Rectangle _baseFrameSource;

	private readonly Appendage _rimAppendage;

	private readonly Appendage _creationLineAppendage;

	private readonly BarrierOrbSpellDamageArea _damageArea;

	private readonly List<Alive> _peopleInsideDuringCreation = new List<Alive>();

	private readonly List<Alive> _peopleWhoTouchedThisFrame = new List<Alive>();

	private bool _isFinishedCreating;

	private bool _isDead;

	private float _createTimer;

	private float _lifeTimer;

	public BarrierOrbBarrier(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, SpriteSheet sprite, int spellDamage, LunaisSpell parentSpell)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 24, 64);
		_sprite = sprite;
		_baseFrameSource = _sprite.GetFrameSource(25);
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = false;
		_doesPersist = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = true;
		base.CannotBeGrabbed = false;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		base.DrawPlane = EDrawPlane.Normal;
		base.DoesDrawAura = true;
		_doAppendagesInheritDrawColor = false;
		_doesUseAppendageCollision = false;
		_rimAppendage = new Appendage(this, new Point(24, 64), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_rimAppendage.ChangeAnimation(26);
		_creationLineAppendage = new Appendage(this, new Point(24, 1), new Point(2, 0), _level, _sprite)
		{
			DrawColor = Color.White * 0.9f,
			DrawPriority = 1
		};
		_creationLineAppendage.ChangeAnimation(27);
		_damageArea = new BarrierOrbSpellDamageArea(_level, Position, ETeamSide.Heroes, spellDamage, parentSpell, 6.7f, _sprite, this);
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			float lifeTimer = _lifeTimer;
			_lifeTimer += delta;
			if (_lifeTimer >= 7f)
			{
				Kill();
			}
			else if (_lifeTimer >= 6.7f)
			{
				if (lifeTimer < 6.7f)
				{
					_level.PlayCue(ESFX.LunaisOrbRadiantSpellWallFade);
				}
				float num = 1f - (_lifeTimer - 6.7f) / 0.3f;
				base.DrawColor = BaseDrawColor * num;
				base.AuraColor = BaseAuraColor * num;
				_rimAppendage.DrawColor = RimDrawColor * num;
			}
			if (!_isFinishedCreating)
			{
				if (_createTimer <= 0f)
				{
					base.Appendages.Add(_creationLineAppendage);
				}
				_createTimer += delta;
				int num2 = 64;
				if (_createTimer < 0.3f)
				{
					float num3 = _createTimer / 0.3f;
					num2 = (int)(Math.Sin(num3 * ((float)Math.PI / 2f)) * 64.0);
					_frameSource = new Rectangle(_baseFrameSource.X, _baseFrameSource.Y + (_baseFrameSource.Height - num2), _baseFrameSource.Width, num2);
					_creationLineAppendage.Position = new Point(Position.X, Position.Y - num2);
					base.DrawColor = BaseDrawColor * num3;
					base.AuraColor = BaseAuraColor;
					_peopleInsideDuringCreation.Clear();
				}
				else
				{
					_isSolid = true;
					_isFinishedCreating = true;
					_frameSource = _baseFrameSource;
					base.Appendages.Clear();
					base.Appendages.Add(_rimAppendage);
					base.DrawColor = BaseDrawColor;
					base.AuraColor = BaseAuraColor;
					_rimAppendage.DrawColor = RimDrawColor;
				}
				Bbox = new Rectangle(Position.X, Position.Y, 24, num2);
			}
			else
			{
				if (_peopleInsideDuringCreation.Count > 0)
				{
					for (int num4 = _peopleInsideDuringCreation.Count - 1; num4 >= 0; num4--)
					{
						Alive alive = _peopleInsideDuringCreation[num4];
						bool flag = false;
						foreach (Alive item in _peopleWhoTouchedThisFrame)
						{
							if (item == alive)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							_peopleInsideDuringCreation.RemoveAt(num4);
						}
					}
				}
				_peopleWhoTouchedThisFrame.Clear();
			}
			_damageArea.SetDimensions(Bbox.Width, Bbox.Height);
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (!who.IsABoss && who.DoesCollideWithTiles)
		{
			if (!_isFinishedCreating)
			{
				_peopleInsideDuringCreation.Add(who);
				flag = base.TriggerEvent(who, depth);
			}
			else
			{
				bool flag2 = false;
				if (_peopleInsideDuringCreation.Count > 0)
				{
					foreach (Alive item in _peopleInsideDuringCreation)
					{
						if (item == who)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag2)
				{
					_peopleWhoTouchedThisFrame.Add(who);
				}
				else
				{
					flag = base.TriggerEvent(who, depth);
					if (flag && who.ScriptActionList.Count > 0 && who is LunaisObj)
					{
						_isSolid = false;
					}
				}
			}
		}
		return flag;
	}

	public override void SilentKill()
	{
		_isDead = true;
		base.CannotBeGrabbed = true;
		base.SilentKill();
	}

	internal bool CreateReset(Point castPosition)
	{
		bool flag = _isDead || _lifeTimer <= 0f;
		if (flag)
		{
			base.ID = -1;
		}
		Position = castPosition;
		_isDead = false;
		_isFinishedCreating = false;
		_isSolid = false;
		_createTimer = 0f;
		_lifeTimer = 0f;
		base.CannotBeGrabbed = false;
		_peopleInsideDuringCreation.Clear();
		_peopleWhoTouchedThisFrame.Clear();
		base.Appendages.Clear();
		_damageArea.Reset(flag);
		if (flag)
		{
			_level.AddProjectile(_damageArea);
		}
		Update(0f);
		return flag;
	}
}
