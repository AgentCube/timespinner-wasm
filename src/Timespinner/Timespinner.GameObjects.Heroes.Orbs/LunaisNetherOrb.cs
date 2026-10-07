using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisNetherOrb : LunaisOrb
{
	private const int OrbTrailLength = 75;

	private const float ThrowTimeToAttack = 0.08f;

	private const float ThrowTimeToReturn = 0.2f;

	private const float ThrowRadius = 56f;

	private static readonly Point OrbBboxOffset = new Point(1, 1);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(5f, 5f);

	private static readonly Color WhiteTrailColor = new Color(0.9f, 1f, 0.95f, 0.8f);

	private static readonly Color ThrowTrailColor = new Color(0.7f, 0.9f, 0.8f, 0.5f);

	private bool _canDoCounter;

	private NetherOrbMeleeCounterDamageArea _counterDamageArea;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Nether;

	public LunaisNetherOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = OrbBboxOffset;
		DrawOrigin = OrbDrawOrigin;
		ChangeAnimation(0, 10, 0.05f, EAnimationType.Cycle);
		Update(0f);
	}

	private bool CanDoCounter()
	{
		if (_canDoCounter)
		{
			if (_counterDamageArea != null)
			{
				return _counterDamageArea.IsFinished;
			}
			return true;
		}
		return false;
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		_canDoCounter = true;
		base.State = EOrbState.Melee;
		_trailColor = ThrowTrailColor;
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
		_level.AddProjectile(new NetherOrbMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.OrbDamage, this));
		PlayCue(ESFX.LunaisOrbNetherWhiff);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		UpdateThrowAttack(delta, 0.08f, 0.2f, UpdateThrow);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		if (_currentThrowTime > 0.28f)
		{
			base.State = EOrbState.Idle;
			_trailColor = base.IdleTrailColor;
			return _baseOrbitPosition.ToVector2();
		}
		if (_currentThrowTime >= 0.08f && currentThrowTime < 0.08f)
		{
			base.IsAtAttackApex = true;
			if (CanDoCounter())
			{
				TryCounter();
			}
		}
		float num = ((_currentThrowTime <= 0.08f) ? (_currentThrowTime / 0.16f) : (0.5f + (_currentThrowTime - 0.08f) / 0.4f));
		_trailColor = ((_currentThrowTime <= 0.18f) ? WhiteTrailColor : ThrowTrailColor);
		float num2 = 56f * (float)Math.Sin((double)num * Math.PI);
		_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
		return new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
	}

	private void TryCounter()
	{
		bool flag = false;
		IEnumerable<Projectile> collidingProjectiles = _level.GetCollidingProjectiles(Bbox, ETeamSide.Enemies);
		if (collidingProjectiles != null)
		{
			foreach (Projectile item in collidingProjectiles)
			{
				if (item != null && item.Power > 0f && item.CanDamageThings && item.CanDamageEnemies)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			AddCounter();
		}
	}

	private void AddCounter()
	{
		if (_counterDamageArea == null)
		{
			int inDamage = (int)Math.Round((float)base.OrbDamage * 2f);
			_counterDamageArea = new NetherOrbMeleeCounterDamageArea(_level, Bbox.Center, ETeamSide.Heroes, inDamage, this);
		}
		else
		{
			_counterDamageArea.Reset(Bbox.Center);
		}
		PlayCue(ESFX.LunaisOrbNetherMeleeVoid);
		_level.AddProjectile(_counterDamageArea);
		_canDoCounter = false;
	}

	internal void OnKillOtherProjectile(Projectile enemyProjectile, Vector2 depth)
	{
		if (CanDoCounter())
		{
			AddCounter();
		}
	}

	public override void ChangeRoom()
	{
		if (_counterDamageArea != null && !_counterDamageArea.IsFinished)
		{
			_counterDamageArea.SilentKill();
		}
		base.ChangeRoom();
	}

	public override void DisposeOrb()
	{
		if (_counterDamageArea != null && !_counterDamageArea.IsFinished)
		{
			_counterDamageArea.SilentKill();
		}
		base.DisposeOrb();
	}
}
