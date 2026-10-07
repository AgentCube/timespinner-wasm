using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;

namespace Timespinner.GameObjects.BaseClasses;

public class Projectile : Animate
{
	internal const float DefaultTimeToFade = 0.2f;

	protected bool _hasBeenUpdatedOnce;

	protected bool _doesCollideWithSlopes;

	protected bool _doesCollideWithFloors;

	protected bool _doesCollideWithWalls;

	protected bool _doesCollideWithCeilings;

	protected bool _doesDieOnTiles;

	protected bool _canDamageThings = true;

	protected bool _doesDieOutsideOfVisibleArea;

	protected bool _doesRotateBasedOnVelocity = true;

	protected bool _doesDrawDamageBbox;

	protected bool _doesLifetimeAffectAlpha = true;

	protected bool _doesProjectileChangeFacingBasedOnVelocity = true;

	protected bool _isTrailLengthAffectedByTime = true;

	protected bool _isAlreadyTouchingHero;

	protected ETeamSide _teamSide = ETeamSide.Neutral;

	protected EDamageElement _damageElement;

	protected byte _timeStopTrailLength = 5;

	protected byte _normalTrailLength = 2;

	protected int _power;

	protected int _force;

	protected float _life = 5f;

	protected float _timeOffset;

	protected float _amplitude;

	protected float _frequency;

	protected float _rotationSpeed;

	protected float _rotationAmount;

	protected float _emitTimer;

	protected float _initialTimeOffset;

	protected Vector2 _initialVector;

	protected Vector2 _particleEmissionOffset;

	protected bool _isFading;

	protected float _fadeTimer;

	protected float _timeToFade = 0.2f;

	protected bool _isDormant;

	protected float _dormantTimer;

	public bool BackPane { get; set; }

	internal bool CanDamageThings
	{
		get
		{
			return _canDamageThings;
		}
		set
		{
			_canDamageThings = value;
		}
	}

	internal bool DoesSurviveImpactIfNoDamageDealt { get; set; }

	public bool DoesDieOnImpact { get; protected set; }

	public bool DoesDieToEnemyProjectiles { get; protected set; }

	public bool CanDamageEnemies { get; set; }

	public bool CanDamageEvents { get; set; }

	public bool CanDamageEnemyProjectiles { get; set; }

	public bool DoesKillProjectilesOnImpact { get; set; }

	public bool IsDamageArea { get; set; }

	internal bool CanBeStoodOnWhenFrozen { get; set; }

	internal bool DoesKnockBack { get; set; }

	internal bool IsDormant => _isDormant;

	internal int Force => _force;

	public ETeamSide TeamSide => _teamSide;

	internal EDamageElement DamageElement => _damageElement;

	internal int EffectiveDamage => (int)((float)_power * DamageMultiplier);

	public float Power
	{
		get
		{
			return _power;
		}
		set
		{
			_power = (int)value;
		}
	}

	internal float DormantTimer
	{
		get
		{
			return _dormantTimer;
		}
		set
		{
			_dormantTimer = value;
			_isDormant = value > 0f;
		}
	}

	internal float DamageMultiplier { get; set; }

	public float Life
	{
		get
		{
			return _life;
		}
		set
		{
			_life = value;
		}
	}

	public virtual float DamageTimeout => 0.15f;

	public virtual Rectangle DamageBbox => Bbox;

	internal Vector2 VisibleVelocity { get; private set; }

	public Projectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int inID)
		: this(inLevel, inPosition, iV, inSide, 0f, inID)
	{
	}

	public Projectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, float dormantTime, int inID)
		: base(inPosition, inLevel, inID)
	{
		DoesDieOnImpact = true;
		base.DoesCollideWithTiles = false;
		CanDamageEnemies = true;
		CanDamageEnemyProjectiles = true;
		CanDamageEvents = true;
		_initialVector = iV;
		_teamSide = inSide;
		base.BaseType = EGameObjectBaseType.Projectile;
		_dormantTimer = dormantTime;
		if (dormantTime > 0f)
		{
			_isDormant = true;
		}
		else
		{
			_velocity = iV;
		}
		DamageMultiplier = 1f;
	}

	public override void Update(float delta)
	{
		if (_isFrozen)
		{
			return;
		}
		if (_dormantTimer > 0f)
		{
			_dormantTimer -= delta;
			if (_dormantTimer <= 0f)
			{
				_dormantTimer = 0f;
				_isDormant = false;
				_velocity = _initialVector;
			}
		}
		if (!_isDormant)
		{
			if (_doesProjectileChangeFacingBasedOnVelocity)
			{
				if (_velocity.X > 0f)
				{
					IsFacingLeft = false;
				}
				else if (_velocity.X < 0f)
				{
					IsFacingLeft = true;
				}
			}
			ApplyBulletMechanics(delta);
			_life -= delta;
			if (_life <= 0f)
			{
				FadeKill();
			}
			if (_rotationSpeed < 0f || _rotationSpeed > 0f)
			{
				_rotationAmount += delta * _rotationSpeed;
				if (_rotationAmount >= (float)Math.PI * 2f)
				{
					_rotationAmount -= (float)Math.PI * 2f;
				}
				else if (_rotationAmount < 0f)
				{
					_rotationAmount += (float)Math.PI * 2f;
				}
				base.Rotation = _rotationAmount;
			}
			if (_doesRotateBasedOnVelocity)
			{
				base.Rotation = MathEx.RotationFromVector2(_velocity) + _rotationAmount;
			}
			if (_isTrailLengthAffectedByTime)
			{
				if (_level.IsTimeFrozen)
				{
					_trailLength = _timeStopTrailLength;
				}
				else
				{
					if (_trailLength == _timeStopTrailLength)
					{
						ClearTrailHistory();
					}
					_trailLength = _normalTrailLength;
				}
			}
			EmitParticles(delta);
			if (_isFading)
			{
				_fadeTimer += delta;
				if (_fadeTimer >= _timeToFade)
				{
					Kill();
				}
			}
			_hasBeenUpdatedOnce = true;
			VisibleVelocity = ((_velocity == Vector2.Zero) ? Position.Subtract(base.LastPosition).ToVector2() : _velocity);
			base.Update(delta);
		}
		if (_doesDieOutsideOfVisibleArea && !_isDormant && _level.IsOutsideProjectileVisibleArea(Position))
		{
			FadeKill();
		}
	}

	protected virtual void EmitParticles(float delta)
	{
		if (_particleSystems.Count > 0 && !_isFading && _particleSystems[0] != null && _doesAutomaticallyEmitParticles)
		{
			_emitTimer -= delta;
			if (_emitTimer < 0f)
			{
				_emitTimer = _particleSystems[0].MaxEmissionCounter;
				_particleSystems[0].AddParticles(_particleEmissionOffset.Add(_position), _velocity);
			}
		}
	}

	protected virtual void ApplyBulletMechanics(float delta)
	{
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height / 2);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isDormant || !_hasBeenUpdatedOnce)
		{
			return;
		}
		if (_doesLifetimeAffectAlpha && _isFading)
		{
			Vector4 vector = base.DrawColor.ToVector4();
			if (_timeToFade > 0f)
			{
				float num = 1f - _fadeTimer / _timeToFade;
				base.DrawColor = new Color(vector.X, vector.Y, vector.Z, num) * num;
			}
		}
		base.Draw(spriteBatch);
		if (_doesDrawDamageBbox)
		{
			Rectangle damageBbox = DamageBbox;
			spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)damageBbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)damageBbox.Y)), damageBbox.Width, damageBbox.Height), null, new Color(40, 100, 50, 100));
		}
	}

	public override bool DetectTileCollisions()
	{
		if (_doesBounceOnGround)
		{
			return base.DetectTileCollisions();
		}
		bool result = false;
		int num = (int)Math.Floor((float)Bbox.Left / 16f);
		int num2 = (int)Math.Ceiling((float)Bbox.Right / 16f) - 1;
		int num3 = (int)Math.Floor((float)Bbox.Top / 16f);
		int num4 = (int)Math.Ceiling((float)Bbox.Bottom / 16f) - 1;
		Dictionary<Point, Tile> solidTiles = _level.SolidTiles;
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				Point key = new Point(j, i);
				if (solidTiles.ContainsKey(key))
				{
					Tile tile = solidTiles[key];
					Vector2 intersectionDepth = Bbox.GetIntersectionDepth(tile.Bbox);
					if (intersectionDepth != Vector2.Zero)
					{
						result = CollideSolidTile(tile, intersectionDepth);
					}
				}
			}
		}
		return result;
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool result = false;
		if (_doesBounceOnGround)
		{
			result = base.CollideSolidTile(tile, depth);
		}
		else
		{
			bool flag = Math.Abs(depth.Y) < Math.Abs(depth.X);
			if (tile.Type != ETileType.Slope && tile.Type != ETileType.Platform)
			{
				if (!flag && _doesCollideWithWalls)
				{
					if ((!(depth.X > 0f) || !_level.CheckNearby(EDirection.East, tile.DictKey)) && (!(depth.X < 0f) || !_level.CheckNearby(EDirection.West, tile.DictKey)))
					{
						if (_doesDieOnTiles)
						{
							Point contactPoint = new Point((base.Velocity.X > 0f) ? tile.Bbox.Left : tile.Bbox.Right, Position.Y);
							KillOnGround(isVerticalCollision: false, contactPoint);
						}
						result = true;
					}
					else
					{
						flag = true;
					}
				}
				if (flag && ((_doesCollideWithCeilings && depth.Y > 0f && base.Velocity.Y < 0f) || (_doesCollideWithFloors && depth.Y < 0f && base.Velocity.Y > 0f)))
				{
					if (_doesDieOnTiles)
					{
						Point contactPoint2 = new Point(Position.X, (base.Velocity.Y > 0f) ? tile.Bbox.Top : tile.Bbox.Bottom);
						KillOnGround(isVerticalCollision: true, contactPoint2);
					}
					result = true;
				}
			}
			else if (tile.Type == ETileType.Slope)
			{
				if (_doesCollideWithSlopes && (_doesCollideWithCeilings || !tile.IsFlippedVertically))
				{
					int num = tile.LookupTileHeight(_position.X);
					if (num != -1)
					{
						if (depth.Y + (float)num <= 0f && base.Velocity.Y > 0f)
						{
							KillOnGround(isVerticalCollision: true, new Point(Position.X, tile.Bbox.Top + num));
							result = true;
						}
						else if (depth.Y > 0f && _doesCollideWithCeilings)
						{
							KillOnGround(isVerticalCollision: true, new Point(Position.X, tile.Bbox.Bottom));
							result = true;
						}
					}
				}
			}
			else if (tile.Type == ETileType.Platform && _doesCollideWithFloors && flag && depth.Y < 0f && base.Velocity.Y > 0f)
			{
				if (_doesDieOnTiles)
				{
					KillOnGround(isVerticalCollision: true, new Point(Position.X, tile.Bbox.Top));
				}
				result = true;
			}
		}
		return result;
	}

	internal virtual void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		Kill();
	}

	public virtual bool DetermineDamage(Alive target, Rectangle collidingBbox)
	{
		bool flag = false;
		Point point = FindDeathPoint(target, collidingBbox);
		bool isAlreadyTouchingHero = false;
		if (target.IsInvulnerable && DoesDieOnImpact)
		{
			Kill(useAnimation: true, point, deathFromInvulnerable: true);
		}
		else
		{
			isAlreadyTouchingHero = true;
			Vector2 velocity = _velocity;
			if (EffectiveDamage <= target.HP)
			{
				flag = target.ManageDamage(EffectiveDamage, velocity, point, collidingBbox, EDamageType.Projectile, DamageElement, DoesKnockBack);
				if (DoesDieOnImpact && (flag || !DoesSurviveImpactIfNoDamageDealt))
				{
					Kill(useAnimation: true, point, deathFromInvulnerable: false);
				}
			}
			else
			{
				flag = target.ManageDamage(EffectiveDamage, velocity, point, collidingBbox, EDamageType.Projectile, DamageElement, DoesKnockBack);
			}
			if (flag)
			{
				AddImpactAnimation(target, collidingBbox);
			}
		}
		_isAlreadyTouchingHero = isAlreadyTouchingHero;
		return flag;
	}

	protected virtual void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, FindDeathPoint(target, collidingRectangle), _level)
		{
			TeamSide = _teamSide,
			AnimationSpeed = 0.03f,
			AnimationStart = ((_teamSide == ETeamSide.Heroes) ? 11 : 15),
			AnimationLength = 4,
			IsFacingLeft = !target.IsFacingLeft
		});
	}

	public virtual void Kill(bool useAnimation)
	{
		Kill(useAnimation, _bbox.Center, deathFromInvulnerable: false);
	}

	public virtual void Kill(bool useAnimation, Point deathPoint, bool deathFromInvulnerable)
	{
		base.Kill();
	}

	public virtual void FadeKill()
	{
		_isFading = true;
	}

	internal Point FindDeathPoint(GameObject target, Rectangle collidingRectangle)
	{
		Point center = collidingRectangle.Center;
		Mobile mobile = target as Mobile;
		Rectangle rectangle = mobile?.OuterBbox ?? target.Bbox;
		if (mobile == null || !mobile.HasAppendageCollision || rectangle.Width < collidingRectangle.Width)
		{
			Vector2 intersectionDepth = collidingRectangle.GetIntersectionDepth(target.Bbox);
			bool flag = ((!(intersectionDepth == Vector2.Zero)) ? (intersectionDepth.X < 0f) : (collidingRectangle.X < target.Bbox.X));
			center.X = (flag ? Math.Min(collidingRectangle.Right, target.Bbox.Left) : Math.Max(collidingRectangle.Left, target.Bbox.Right));
		}
		if (rectangle.Height < collidingRectangle.Height)
		{
			center.Y = target.Bbox.Center.Y;
		}
		return center;
	}

	public void ExtendLife(float amount)
	{
		_isFading = false;
		if (_life < 0f)
		{
			_life = 0f;
		}
		_life += amount;
	}

	public virtual void KillOnProjectileImpact(Projectile proj)
	{
		KillOnGround(isVerticalCollision: false, Bbox.Center);
	}

	public bool CheckSolidCollision(Animate target)
	{
		bool result = false;
		if (base.IsFrozen && !_isAlreadyTouchingHero)
		{
			Vector2 intersectionDepth = target.Bbox.GetIntersectionDepth(Bbox);
			if (intersectionDepth != Vector2.Zero)
			{
				result = target.CollideSolidObject(this, ETileType.Monster, intersectionDepth);
			}
		}
		else if (_isAlreadyTouchingHero)
		{
			Vector2 intersectionDepth2 = target.Bbox.GetIntersectionDepth(Bbox);
			if (intersectionDepth2 == Vector2.Zero)
			{
				_isAlreadyTouchingHero = false;
			}
		}
		return result;
	}

	internal virtual void OnKillOtherProjectile(Projectile enemyProjectile, Vector2 depth)
	{
	}
}
