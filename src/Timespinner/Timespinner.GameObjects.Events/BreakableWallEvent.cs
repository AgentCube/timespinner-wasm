using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events;

internal sealed class BreakableWallEvent : GameEvent
{
	private const string SaveKeyFormat = "BW_{0}_{1}_{2}";

	private const float MaxHealth = 6f;

	private const float InvulnerableTime = 0.5f;

	private readonly int _argument;

	private bool _isDeactivated;

	private bool _isDying;

	private float _health = 6f;

	private float _invulnerableTimer;

	internal bool IsUnderwater { get; private set; }

	internal int Argument => _argument;

	private string SaveKey => $"BW_{_level.ID}_{_level.RoomID}_{_argument}";

	public BreakableWallEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.EventType = EEventTileType.BreakableWall;
		_bbox = new Rectangle(0, 0, 16, 16);
		_bboxOffset = new Point(0, 0);
		_isSolid = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = true;
		_isAffectedByTime = true;
		base.IsTriggerableByMonsters = true;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		base.DrawPlane = EDrawPlane.Normal;
		base.CanBeTriggeredByFamiliar = true;
		_sprite = _level.CurrentTileset;
		ChangeAnimation(0, 0, 1f, EAnimationType.None);
		_argument = objectSpec?.Argument ?? 0;
	}

	public override void Initialize()
	{
		Point point = new Point(Position.X / 16, (Position.Y - 8) / 16);
		if (_level.SolidTiles.ContainsKey(point))
		{
			Tile tile = _level.SolidTiles[point];
			ChangeAnimation(tile.TileIndex, 0, 1f, EAnimationType.None);
			IsFlippedVertically = tile.SpriteEffects.HasFlag(SpriteEffects.FlipVertically);
			IsFacingLeft = !tile.SpriteEffects.HasFlag(SpriteEffects.FlipHorizontally);
			_level.SolidTiles.Remove(point);
		}
		if (_level.GameSave.GetSaveBool(SaveKey))
		{
			_isDeactivated = true;
			_isSolid = false;
			if (!_level.WaterTiles.ContainsKey(point) && (_level.WaterTiles.ContainsKey(new Point(point.X + 1, point.Y)) || _level.WaterTiles.ContainsKey(new Point(point.X - 1, point.Y))))
			{
				_level.WaterTiles.Add(point, new WaterTile(new Point(point.X * 16, point.Y * 16), point, _level, _level.GCM.TsEventTiles, WaterFillerEvent.EWaterType.Bottom));
			}
		}
		else if (_level.WaterTiles.ContainsKey(point))
		{
			_level.WaterTiles.Remove(point);
			IsUnderwater = true;
		}
		base.Initialize();
	}

	public override bool ProjectileTriggerEvent(Projectile projectile, Vector2 depth)
	{
		if (!base.IsFrozen && _invulnerableTimer <= 0f && projectile.TeamSide == ETeamSide.Heroes)
		{
			_health -= 2f;
			BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.CrackingDust, Bbox.Center, ETeamSide.Neutral, imageFacingRight: true, _level, doesPlaySFX: true);
			BattleAnimation battleAnimation2 = BattleAnimation.Create(EBattleAnimationType.Pebbles, Bbox.Center, ETeamSide.Neutral, imageFacingRight: true, _level, doesPlaySFX: true);
			if (_health > 0f)
			{
				_battleAnimations.Add(battleAnimation);
				_battleAnimations.Add(battleAnimation2);
				PlayCue(ESFX.FoleyBreakableWallHit);
				if (projectile.DoesDieOnImpact)
				{
					projectile.Kill(useAnimation: true);
				}
			}
			else
			{
				_level.AddAnimation(battleAnimation);
				_level.AddAnimation(battleAnimation2);
				if (!_isDying)
				{
					_level.PlayCue(ESFX.FoleyBreakableWallBreak, Position);
				}
				Kill();
				KillAllOtherWallPieces();
			}
			_invulnerableTimer = 0.5f;
		}
		return true;
	}

	public override void Update(float delta)
	{
		if (_isDeactivated)
		{
			SilentKill();
		}
		if (_invulnerableTimer > 0f)
		{
			_invulnerableTimer -= delta;
			if (_invulnerableTimer < 0f)
			{
				_invulnerableTimer = 0f;
			}
		}
		if (!_isFrozen)
		{
			base.Update(delta);
		}
		else
		{
			UpdateIsWithinObjectVisibleArea();
		}
	}

	public override void Kill()
	{
		if (!_isDying)
		{
			_isDying = true;
			DebrisEvent.CreateFromSquare(this, new Vector2(0f, 0f), Bbox.Center, _sprite);
			_level.GameSave.SetValue(SaveKey, value: true);
			base.Kill();
		}
	}

	private void KillAllOtherWallPieces()
	{
		bool flag = IsUnderwater;
		List<Point> list = new List<Point>();
		list.Add(new Point(Bbox.Left, Bbox.Top));
		List<Point> list2 = list;
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.BreakableWall);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item != this && item is BreakableWallEvent breakableWallEvent && breakableWallEvent.Argument == Argument)
			{
				flag = flag || breakableWallEvent.IsUnderwater;
				list2.Add(new Point(breakableWallEvent.Bbox.Left, breakableWallEvent.Bbox.Top));
				breakableWallEvent.Kill();
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (Point item2 in list2)
		{
			Point point = new Point(item2.X / 16, item2.Y / 16);
			if (!_level.WaterTiles.ContainsKey(point))
			{
				_level.WaterTiles.Add(point, new WaterTile(item2, point, _level, _level.GCM.TsEventTiles, WaterFillerEvent.EWaterType.Bottom));
			}
		}
	}
}
