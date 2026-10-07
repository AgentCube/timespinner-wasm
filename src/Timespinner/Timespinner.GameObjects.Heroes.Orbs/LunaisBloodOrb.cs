using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisBloodOrb : LunaisOrb
{
	private const int BloodDropletCount = 3;

	private const int OrbTrailLength = 75;

	private const int ThrowRadius = 64;

	private const int ThrowRadiusSquared = 4096;

	private const int SeekRadiusThresholdSquared = 16384;

	private const int BloodSearchStartOffsetX = 32;

	private const float TimeBeforeAddingBloodDroplets = 0.2f;

	private const float TimeToRecover = 0.15f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private readonly BloodOrbMeleeDamageDroplet _dropletA;

	private readonly List<BloodOrbMeleeDamageDroplet> _bloodDamageAreas = new List<BloodOrbMeleeDamageDroplet>();

	private bool _isRecovering;

	private float _recoverTimer;

	private Point _dropletStart;

	private SFXCueInstance _returnLoopCueInstance;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Blood;

	public LunaisBloodOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(29, 10, 0.05f, EAnimationType.Cycle);
		for (int i = 0; i < 3; i++)
		{
			BloodOrbMeleeDamageDroplet bloodOrbMeleeDamageDroplet = new BloodOrbMeleeDamageDroplet(_level, Position, base.OrbDamage, i, this);
			_bloodDamageAreas.Add(bloodOrbMeleeDamageDroplet);
			if (i == 0)
			{
				_dropletA = bloodOrbMeleeDamageDroplet;
			}
		}
		Update(0f);
	}

	public override void Update(float delta, Point targetPoint)
	{
		if (base.State != EOrbState.Dead)
		{
			_trailColor = base.IdleTrailColor;
		}
		if (_isRecovering || base.State == EOrbState.Idle)
		{
			foreach (BloodOrbMeleeDamageDroplet bloodDamageArea in _bloodDamageAreas)
			{
				bloodDamageArea.UpdateBloodTrail(delta);
				if (!_isRecovering)
				{
					bloodDamageArea.MakeInvisible();
				}
			}
		}
		if (_isRecovering)
		{
			_recoverTimer += delta;
			if (_recoverTimer >= 0.15f)
			{
				_isRecovering = false;
				_recoverTimer = 0f;
				base.State = EOrbState.Idle;
				_doesDrawBaseSprite = true;
				_doesDrawTrail = true;
			}
		}
		if (base.State == EOrbState.Charge)
		{
			foreach (BloodOrbMeleeDamageDroplet bloodDamageArea2 in _bloodDamageAreas)
			{
				bloodDamageArea2.UpdateBloodTrail(delta);
			}
		}
		base.Update(delta, targetPoint);
		base.OrbPassiveCenter = Position;
		if (_doesDrawBaseSprite)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (BloodOrbMeleeDamageDroplet bloodDamageArea3 in _bloodDamageAreas)
		{
			if (bloodDamageArea3 != null && !bloodDamageArea3.IsFinished)
			{
				num++;
				num2 += bloodDamageArea3.Position.X;
				num3 += bloodDamageArea3.Position.Y;
			}
		}
		if (num > 0)
		{
			base.OrbPassiveCenter = new Point(num2 / num, num3 / num);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isRecovering || base.State == EOrbState.Idle)
		{
			foreach (BloodOrbMeleeDamageDroplet bloodDamageArea in _bloodDamageAreas)
			{
				bloodDamageArea.Draw(spriteBatch);
			}
		}
		base.Draw(spriteBatch);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (currentThrowTime <= 0f)
		{
			_level.AddAnimation(new BattleAnimation(_sprite, Bbox.Center, _level)
			{
				TeamSide = ETeamSide.Heroes,
				AnimationStart = 0,
				AnimationLength = 5,
				IsFacingLeft = !base.IsThrowingLeft,
				DrawColor = Color.White * 0.9f
			});
			_doesDrawBaseSprite = false;
			_doesDrawTrail = false;
			_dropletStart = FindDropletStart();
			_level.AddAnimation(new BattleAnimation(_sprite, _dropletStart, _level)
			{
				AnimationStart = 13,
				AnimationLength = 8,
				IsFacingLeft = base.IsThrowingLeft,
				DrawColor = Color.White * 0.9f,
				TeamSide = ETeamSide.Heroes
			});
			base.ParentLunais.PlayCue(ESFX.LunaisOrbBloodWhiffLunais);
			_level.PlayCue(ESFX.LunaisOrbBloodWhiffTarget, _dropletStart);
			if (_returnLoopCueInstance == null)
			{
				_returnLoopCueInstance = _dropletA.CreateCue(ESFX.LunaisOrbBloodReturnLoop, _dropletStart, isLooped: true);
				if (_returnLoopCueInstance != null)
				{
					_returnLoopCueInstance.FadeIn(0.5f);
					_returnLoopCueInstance.Play();
				}
			}
			else if (_returnLoopCueInstance.IsPaused)
			{
				_returnLoopCueInstance.FadeIn(0.5f);
				_returnLoopCueInstance.Resume();
			}
		}
		if (currentThrowTime < 0.2f && _currentThrowTime >= 0.2f)
		{
			foreach (BloodOrbMeleeDamageDroplet bloodDamageArea in _bloodDamageAreas)
			{
				bloodDamageArea.ResetPosition(_dropletStart, base.IsThrowingLeft);
				_level.AddProjectile(bloodDamageArea);
			}
			base.IsAtAttackApex = true;
		}
		if (!_isRecovering && _currentThrowTime > 0.2f)
		{
			bool flag = false;
			foreach (BloodOrbMeleeDamageDroplet bloodDamageArea2 in _bloodDamageAreas)
			{
				if (bloodDamageArea2.IsFinished)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				foreach (BloodOrbMeleeDamageDroplet bloodDamageArea3 in _bloodDamageAreas)
				{
					_level.RequestRemoveObject(bloodDamageArea3);
					bloodDamageArea3.IsFinished = true;
				}
				_level.AddAnimation(new BattleAnimation(_sprite, Position, _level)
				{
					AnimationStart = 0,
					AnimationLength = 5,
					IsAnimationInReverse = true,
					AnchorObject = this,
					AnchorOffset = new Point(base.IsThrowingLeft ? 5 : (-5), -1),
					IsFacingLeft = !base.IsThrowingLeft,
					DrawColor = Color.White * 0.9f,
					TeamSide = ETeamSide.Heroes
				});
				_isRecovering = true;
				PlayCue(ESFX.LunaisOrbBloodReturnFinish, Position);
				if (_returnLoopCueInstance != null && !_returnLoopCueInstance.IsFinished)
				{
					_returnLoopCueInstance.Pause(0.5f);
				}
			}
		}
		Position = _baseOrbitPosition;
	}

	private Point FindDropletStart()
	{
		bool flag = true;
		Point result = Point.Zero;
		Monster nearestVisibleEnemy = _level.GetNearestVisibleEnemy(new Point(base.CurrentTarget.X + 32 * ((!base.IsThrowingLeft) ? 1 : (-1)), base.CurrentTarget.Y));
		if (nearestVisibleEnemy != null && nearestVisibleEnemy.Position.X < base.CurrentTarget.X == base.IsThrowingLeft)
		{
			Point center = nearestVisibleEnemy.OuterBbox.Center;
			int num = base.CurrentTarget.X - center.X;
			int num2 = base.CurrentTarget.Y - center.Y;
			int num3 = num * num + num2 * num2;
			if (num3 < 16384)
			{
				if (num3 <= 4096)
				{
					result = center;
				}
				else
				{
					Vector2 value = new Vector2(num, num2);
					value.Normalize();
					Vector2 b = Vector2.Multiply(value, 64f);
					result = base.CurrentTarget.Subtract(b);
				}
				flag = false;
			}
		}
		if (flag)
		{
			result = base.CurrentTarget.Add(base.IsThrowingLeft ? (-64) : 64, 0);
		}
		return result;
	}

	internal override void UpdateDamage(int damage)
	{
		base.UpdateDamage(damage);
		foreach (BloodOrbMeleeDamageDroplet bloodDamageArea in _bloodDamageAreas)
		{
			bloodDamageArea.Power = damage;
		}
	}

	public override void DisposeOrb()
	{
		if (_returnLoopCueInstance != null)
		{
			_returnLoopCueInstance.Stop();
		}
		foreach (BloodOrbMeleeDamageDroplet bloodDamageArea in _bloodDamageAreas)
		{
			bloodDamageArea.SilentKill();
		}
		base.DisposeOrb();
	}

	public override void ChangeRoom()
	{
		base.State = EOrbState.Idle;
		_doesDrawBaseSprite = true;
		foreach (BloodOrbMeleeDamageDroplet bloodDamageArea in _bloodDamageAreas)
		{
			bloodDamageArea?.ClearTrailHistory();
		}
		base.ChangeRoom();
	}
}
