using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BloodOrbMeleeDamageDroplet : LunaisBaseOrbDamageArea
{
	private const int EndRadiusSquared = 128;

	private const int BaseTrailInterpolationAmount = 5;

	private const float StartPushX = 1000f;

	private const float DropletOffset = (float)Math.PI * 2f / 3f;

	private const float TrailCleanTimeThreshold = 0.033f;

	private static readonly Color BaseTrailColor = new Color(104, 24, 32, 56);

	private static readonly Color DarkTrailColor = new Color(88, 16, 24, 48);

	private static readonly Color LightTrailColor = new Color(112, 32, 40, 64);

	private readonly int _dropletIndex;

	private readonly LunaisOrbAbility _parentOrb;

	private bool _hasBeenThrownInThisRoom;

	private float _timeSinceThrow;

	private float _oscillationDeltaOffset;

	private float _bloodTrailTimer;

	private Point _bloodSpawnPoint;

	internal bool IsFinished { get; set; }

	public BloodOrbMeleeDamageDroplet(Level inLevel, Point inPosition, int inDamage, int dropletIndex, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, ETeamSide.Heroes, -1, parentOrb, parentOrb)
	{
		_parentOrb = parentOrb;
		_sprite = _level.GCM.SpOrbMeleeBlood;
		_dropletIndex = dropletIndex;
		_bbox = new Rectangle(inPosition.X - 5, inPosition.Y - 5, 10, 10);
		_bboxOffset = new Point(-3, -3);
		_power = inDamage;
		_force = 2;
		_life = 100f;
		base.DamageTimeoutTime = 1f;
		_damageElement = EDamageElement.Dark;
		_isAffectedByGravity = false;
		_isAffectedByFriction = true;
		_doesRotateBasedOnVelocity = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = false;
		base.DoesCollideWithTiles = false;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 6;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 3.3E-05f;
		_trailColor = BaseTrailColor;
		_trailLength = 75;
		_isTrailLengthAffectedByTime = false;
		ChangeAnimation(12);
	}

	public override void Update(float delta)
	{
		_life = 100f;
		if (!IsFinished && _hasBeenThrownInThisRoom)
		{
			_timeSinceThrow += delta;
			if (_timeSinceThrow > 100000f)
			{
				_timeSinceThrow = 0f;
			}
			float num = (_timeSinceThrow + _oscillationDeltaOffset) * 10f * ((float)Math.PI / 2f);
			float num2 = (float)Math.Sin(num);
			float scaleFactor = num2 * 300f;
			Vector2 value = new Vector2(_parentOrb.Position.X - _position.X, _parentOrb.Position.Y - _position.Y);
			Vector2 value2 = Vector2.Normalize(value);
			Vector2 vector = Vector2.Multiply(value2, 300f);
			if (_timeSinceThrow < 0.25f)
			{
				float amount = _timeSinceThrow / 0.25f;
				vector = Vector2.Negate(vector).SineInterpolate(vector, amount);
			}
			_velocity = Vector2.Add(vector, Vector2.Multiply(new Vector2(0f - value2.Y, value2.X), scaleFactor));
			float num3 = (float)Math.Cos(num);
			if (num3 > 0f)
			{
				_trailColor = BaseTrailColor.SineInterpolate(LightTrailColor, num3);
			}
			else if (num3 < 0f)
			{
				_trailColor = BaseTrailColor.SineInterpolate(DarkTrailColor, 0f - num3);
			}
			float num4 = value.LengthSquared();
			if (num4 < 128f)
			{
				IsFinished = true;
			}
		}
		else
		{
			_trailColor = BaseTrailColor;
			base.CanDamageEnemies = false;
			base.IsAnchored = true;
		}
		base.Update(delta);
	}

	public void ResetPosition(Point newPosition, bool isFacingLeft)
	{
		base.ID = -1;
		_hasBeenThrownInThisRoom = true;
		IsFinished = false;
		base.IsAnchored = false;
		_doesDrawBaseSprite = true;
		base.CanDamageEnemies = true;
		IsFacingLeft = isFacingLeft;
		_life = 1000f;
		_timeSinceThrow = 0f;
		_oscillationDeltaOffset = (float)_dropletIndex * ((float)Math.PI * 2f / 3f);
		_bloodSpawnPoint = newPosition;
		Position = _bloodSpawnPoint;
		_velocity = new Vector2(IsFacingLeft ? (-1000f) : 1000f, 0f);
		_damagedEnemiesDictionary.Clear();
		SnapBboxToPosition();
		ClearTrailHistory();
		_trailInterpolationAmount = 5;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			AnimationStart = 21,
			AnimationLength = 4,
			IsFacingLeft = IsFacingLeft,
			TeamSide = ETeamSide.Heroes
		});
	}

	internal void UpdateBloodTrail(float delta)
	{
		Update(delta);
		int num = _drawHistories.Count;
		if (num <= 0)
		{
			return;
		}
		_bloodTrailTimer += delta;
		if (_bloodTrailTimer > 0.033f)
		{
			_bloodTrailTimer = 0f;
			int num2 = 0;
			while (num > 0 && num2 < 10)
			{
				_drawHistories.RemoveAt(0);
				num--;
				num2++;
			}
			if (_trailInterpolationAmount > 0)
			{
				_trailInterpolationAmount--;
			}
		}
	}

	public void MakeInvisible()
	{
		_doesDrawBaseSprite = false;
	}
}
