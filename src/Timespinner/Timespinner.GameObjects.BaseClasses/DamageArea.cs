using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.BaseClasses;

public class DamageArea : Projectile
{
	protected readonly Mobile _anchorObject;

	protected readonly Dictionary<int, float> _damagedEnemiesDictionary = new Dictionary<int, float>();

	private Point _anchorOffset;

	protected Point _damageDimensions;

	internal bool HasInfiniteLife { get; set; }

	internal bool IsAnchored { get; set; }

	public float DamageTimeoutTime { get; set; }

	public override float DamageTimeout => DamageTimeoutTime;

	public Point AnchorOffset
	{
		get
		{
			return new Point(IsImageFacingLeft ? _anchorOffset.X : (-_anchorOffset.X), _anchorOffset.Y);
		}
		set
		{
			_anchorOffset = value;
		}
	}

	public Point DamageDimensions
	{
		get
		{
			return _damageDimensions;
		}
		set
		{
			_damageDimensions = value;
			_bbox = new Rectangle(Position.X, Position.Y, _damageDimensions.X, _damageDimensions.Y);
		}
	}

	public DamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int inID, Mobile inAnchor)
		: base(inLevel, inPosition, Vector2.Zero, inSide, 0f, inID)
	{
		_anchorObject = inAnchor;
		IsAnchored = _anchorObject != null;
		base.DoesDieOnImpact = false;
		base.IsDamageArea = true;
		_isAffectedByGravity = false;
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			bool isImageFacingLeft = IsImageFacingLeft;
			if (IsAnchored && _anchorObject != null)
			{
				Position = _anchorObject.LastPosition.Add(AnchorOffset);
			}
			if (HasInfiniteLife)
			{
				base.Life = 1f;
			}
			UpdateDamageTimeout(delta);
			base.Update(delta);
			if (isImageFacingLeft != IsImageFacingLeft && IsAnchored && _anchorObject != null)
			{
				Position = _anchorObject.Position.Add(AnchorOffset);
				SnapBboxToPosition();
				SnapFrameToBbox();
			}
		}
	}

	protected virtual void UpdateDamageTimeout(float delta)
	{
		if (!(DamageTimeoutTime >= 0f))
		{
			return;
		}
		List<int> list = new List<int>(_damagedEnemiesDictionary.Keys);
		foreach (int item in list)
		{
			_damagedEnemiesDictionary[item] -= delta;
			if (_damagedEnemiesDictionary[item] <= 0f)
			{
				_damagedEnemiesDictionary.Remove(item);
			}
		}
	}

	public override bool DetermineDamage(Alive target, Rectangle collidingBbox)
	{
		bool result = false;
		if (!target.IsInvulnerable && !_isDormant && DamageTimeoutTime >= 0f && !_damagedEnemiesDictionary.ContainsKey(target.ID))
		{
			Vector2 velocity = _velocity;
			if (velocity == Vector2.Zero)
			{
				velocity.X = ((target.Position.X < collidingBbox.Center.X) ? (-_force) : _force);
			}
			Point where = FindDamagePoint(target, collidingBbox);
			result = target.ManageDamage(base.EffectiveDamage, velocity, where, collidingBbox, EDamageType.Projectile, base.DamageElement, base.DoesKnockBack);
			Point intersectionCenter = RectangleExtensions.GetIntersectionCenter(target.GetFirstInsectingBbox(collidingBbox), collidingBbox);
			DoDamageAnimation(target, intersectionCenter);
			if (DamageTimeoutTime >= 0f)
			{
				_damagedEnemiesDictionary.Add(target.ID, DamageTimeoutTime);
			}
		}
		return result;
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		return false;
	}

	protected virtual void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
	}

	protected virtual Point FindDamagePoint(Alive target, Rectangle collisionRectangle)
	{
		if (collisionRectangle.Width <= 16)
		{
			return collisionRectangle.Center;
		}
		return FindDeathPoint(target, collisionRectangle);
	}
}
