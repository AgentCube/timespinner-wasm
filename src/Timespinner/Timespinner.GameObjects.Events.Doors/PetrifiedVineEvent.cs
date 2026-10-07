using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.StatusParticleEffects;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class PetrifiedVineEvent : GameEvent
{
	private const int VineCount = 12;

	private const int BurnHeight = 64;

	private const int BurnOffsetX = 32;

	private const int BurnOffsetY = -16;

	private const float TimeForBurning = 2f;

	private const float TimeForBurnGrowth = 1f;

	private const float TimeForFadeOut = 0.2f;

	private const float TimeBeforeStartingFade = 1.8f;

	private const string SaveKeyFormat = "PV_{0}";

	private static readonly Point VineBboxDimensions = new Point(64, 80);

	private static readonly Point VineCapPastOffset = new Point(32, -72);

	private static readonly Point VineCapPresentOffset = new Point(0, -72);

	private static readonly Color BurntVineColor = new Color(1f, 0.6f, 0.6f, 1f);

	private static readonly Vector4 AshColor = new Vector4(0.8f, 0.6f, 0.4f, 1f);

	private readonly bool _isInPast;

	private readonly Point _minimapBlock;

	private readonly DisintegrateAshParticleSystem _ashParticleSystem;

	private readonly DisintegrateFireParticleSystem _burningParticleSystem;

	private readonly List<VineDangleAppendage> _vines = new List<VineDangleAppendage>();

	private bool _isBurned;

	private bool _isBurning;

	private float _burningScriptTimer;

	private Point _lastTriggerLocation;

	private Color _baseVineColor = Color.White;

	internal string SaveKey => $"PV_{_minimapBlock.X}";

	public override Point AnchorPosition => base.AnchorPosition.Add(0, -VineBboxDimensions.Y);

	public PetrifiedVineEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpPetrifiedVines;
		_minimapBlock = _level.Minimap.GetRoomFromLevelAndRoom(_level.ID, _level.RoomID)?.Position ?? Point.Zero;
		_isBurned = _level.GameSave.GetSaveBool(SaveKey);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		_doAppendagesInheritDrawColor = true;
		base.CannotBeGrabbed = true;
		base.DrawPlane = EDrawPlane.Front;
		base.CanBeTriggeredByFamiliar = true;
		Bbox = new Rectangle(0, 0, VineBboxDimensions.X, VineBboxDimensions.Y);
		Position = Position.Add(0, VineBboxDimensions.Y - 16);
		SnapBboxToPosition();
		_isInPast = _level.Era == EEraType.Past;
		if (_isInPast)
		{
			if (_isBurned)
			{
				ChangeAnimation(-1);
				base.DrawColor = BurntVineColor;
			}
			else
			{
				_doesDrawBaseSprite = false;
				base.DoesCollideWithProjectiles = true;
				Point point = Point.Zero;
				for (int i = 0; i < 12; i++)
				{
					VineDangleAppendage item = new VineDangleAppendage(this, new Point(8, 8), Point.Zero, _level, _sprite, point)
					{
						DrawPriority = ((i % 2 != 0) ? 1 : (-1)),
						EndPointOffset = point,
						DoesInheritDrawColor = false
					};
					_appendages.Add(item);
					_vines.Add(item);
					point = new Point(9 * i % VineBboxDimensions.X, 0);
				}
				_ashParticleSystem = new DisintegrateAshParticleSystem(_level.GCM.TxParticleEnergy, 12)
				{
					BaseColor = AshColor
				};
				_burningParticleSystem = new DisintegrateFireParticleSystem(_sprite, 23, 4, 64);
				_particleSystems.Add(_ashParticleSystem);
				_particleSystems.Add(_burningParticleSystem);
			}
			Appendage appendage = new Appendage(this, new Point(80, 32), Point.Zero, _level, _sprite)
			{
				Position = Position.Add(VineCapPastOffset)
			};
			appendage.ChangeAnimation(21);
			_appendages.Add(appendage);
		}
		else if (_isBurned)
		{
			ChangeAnimation(-1);
			Appendage appendage2 = new Appendage(this, new Point(80, 32), Point.Zero, _level, _sprite)
			{
				Position = Position.Add(VineCapPresentOffset)
			};
			appendage2.ChangeAnimation(22);
			_appendages.Add(appendage2);
		}
		else
		{
			_isSolid = true;
			ChangeAnimation(20);
			IsFacingLeft = true;
			BboxOffset = new Point(12, 24);
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = base.TriggerEvent(who, depth);
		if (!base.IsFrozen && !_isBurned && _isInPast)
		{
			ShiftRoots(who, depth, isProjectile: false);
		}
		return result;
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		bool flag = false;
		if (!base.IsFrozen && !_isBurned && _isInPast)
		{
			flag = base.ProjectileTriggerEvent(projectile, depth);
			if (flag)
			{
				if (projectile.DamageElement == EDamageElement.Fire || (projectile.TeamSide == ETeamSide.Heroes && _level.GameSave.Inventory.EquippedPassiveOrb == EInventoryOrbType.Flame))
				{
					StartBurning();
				}
				else
				{
					ShiftRoots(projectile, depth, isProjectile: true);
				}
			}
		}
		return flag;
	}

	private void StartBurning()
	{
		_isBurning = true;
		_isBurned = true;
		_level.GameSave.SetValue(SaveKey, value: true);
		PlayCue(ESFX.EnvVinesBurning);
	}

	private void ShiftRoots(Mobile who, Vector2 depth, bool isProjectile)
	{
		Point lastTriggerLocation = who.Bbox.Center.Subtract(depth);
		int num = lastTriggerLocation.X + lastTriggerLocation.Y;
		int num2 = _lastTriggerLocation.X + _lastTriggerLocation.Y;
		int num3 = Math.Abs(num - num2);
		if ((num == num2 || num3 <= 5) && !isProjectile)
		{
			return;
		}
		foreach (VineDangleAppendage vine in _vines)
		{
			if (vine != null && vine.OuterBbox.Intersects(who.Bbox))
			{
				vine.DoShift(who, depth);
			}
		}
		_lastTriggerLocation = lastTriggerLocation;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isBurning)
		{
			UpdateDeathScript(delta);
		}
		base.Update(delta);
	}

	private void UpdateDeathScript(float delta)
	{
		_burningScriptTimer += delta;
		if (_burningScriptTimer < 2f)
		{
			float num = 1f;
			if (_burningScriptTimer < 1f)
			{
				num = _burningScriptTimer / 1f;
				_baseVineColor = Color.White.Lerp(BurntVineColor, num);
				base.DrawColor = _baseVineColor;
				foreach (VineDangleAppendage vine in _vines)
				{
					vine.DrawColor = _baseVineColor;
				}
			}
			else if (_burningScriptTimer >= 1.8f)
			{
				float num2 = 1f - (_burningScriptTimer - 1.8f) / 0.2f;
				Color drawColor = _baseVineColor * num2;
				foreach (VineDangleAppendage vine2 in _vines)
				{
					vine2.DrawColor = drawColor;
				}
			}
			float num3 = num * -64f;
			int bottom = (int)(_level.NextRandomDouble() * (double)num3) + Position.Y + -16;
			int left = Bbox.Left + 32;
			int right = Bbox.Right + 32;
			_burningParticleSystem.AddParticles(left, right, bottom);
			_ashParticleSystem.AddParticles(left, right, bottom);
		}
		else
		{
			_isBurning = false;
			int count = _vines.Count;
			for (int i = 0; i < count; i++)
			{
				base.Appendages.RemoveAt(0);
			}
			_vines.Clear();
		}
	}
}
